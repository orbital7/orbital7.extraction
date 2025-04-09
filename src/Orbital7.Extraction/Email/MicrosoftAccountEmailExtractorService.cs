using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Orbital7.Extraction.Email;

public class MicrosoftAccountEmailExtractorService(
    IHttpClientFactory httpClientFactory) :
    IMicrosoftAccountEmailExtractorService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public string GetAuthorizationUrl(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppTokenInfo tokenInfo)
    {
        // Specify offline_access to get a refresh token.
        const string SCOPES = "user.read mail.read offline_access";

        return $"{GetOAuthAuthorizeUrl(tokenInfo.AccountLoginEndpoint)}?" +
            $"client_id={config.ClientId}&" +
            $"response_type=code&" +
            $"redirect_uri={config.RedirectUri}&" +
            $"response_mode=query&scope={SCOPES}";
    }

    public async Task UpdateAccessTokenAsync(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppTokenInfo tokenInfo)
    {
        if (tokenInfo.RefreshToken.HasText())
        {
            await RefreshAccessTokenAsync(config, tokenInfo);
        }
        else
        {
            await GetInitialAccessTokenAsync(config, tokenInfo);
        }
    }

    public async Task<List<(string?, string?)>> GatherMessagesSenderSubjectAsync(
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        MicrosoftGraphMessagesQueryConfig queryConfig)
    {
        var summary = new List<(string?, string?)>();

        // Ensure we're only selecting sender and subject.
        queryConfig.Select = ["sender", "subject"];

        await ExecuteMessagesFolderQueryAsync(
            tokenInfo,
            folderPath,
            queryConfig,
            (message) => // Message iterator handler.
            {
                // Add the summary and keep going.
                summary.Add((message.Sender?.EmailAddress?.Address, message.Subject));
                return Task.FromResult(true);
            });

        return summary;
    }

    public async Task ExecuteMessagesFolderQueryAsync(
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        MicrosoftGraphMessagesQueryConfig queryConfig,
        Func<Message, Task<bool>> messageIteratorHandler,
        Func<Task<bool>>? pageIteratorPausedHandler = null)
    {
        var graphClient = CreateGraphServiceClient(tokenInfo);
        var messagesCount = 0;

        // Get the folder ID for the specified path.
        var folderId = await GetFolderIdByPathAsync(graphClient, folderPath);

        // Search.
        var messagesResponse = await graphClient.Me.MailFolders[folderId].Messages
            .GetAsync(requestConfig =>
            {
                // Set the request query parameters.
                requestConfig.QueryParameters.Top = queryConfig.Top;
                requestConfig.QueryParameters.Filter = queryConfig.Filter;
                requestConfig.QueryParameters.Orderby = queryConfig.Orderby;
                requestConfig.QueryParameters.Select = queryConfig.Select;
                requestConfig.QueryParameters.Expand = queryConfig.Expand;

                // Set the request headers.
                if (queryConfig.Headers != null)
                {
                    foreach (var header in queryConfig.Headers)
                    {
                        requestConfig.Headers.Add(header.Item1, header.Item2);
                    }
                }
            });

        // Continue if we have messages.
        if (messagesResponse != null)
        {
            var pageIterator = PageIterator<Message, MessageCollectionResponse>
                .CreatePageIterator(
                    graphClient,
                    messagesResponse,
                    async (message) =>
                    {
                        // Increment the count and handle the message.
                        messagesCount++;
                        bool shouldContinue = await messageIteratorHandler(message);

                        // Consider the maximum to determine whether we should continue.
                        if (shouldContinue && 
                            queryConfig.Maximum.HasValue && 
                            messagesCount >= queryConfig.Maximum)
                        {
                            return false;
                        }

                        // NOTE: A return value of False here will cause the page iterator to 
                        // pause and the pageIteratorPausedHandler to be called.
                        return shouldContinue;
                    },
                    (request) =>
                    {
                        // We have to re-add the headers.
                        if (queryConfig.Headers != null)
                        {
                            foreach (var header in queryConfig.Headers)
                            {
                                request.Headers.Add(header.Item1, header.Item2);
                            }
                        }

                        return request;
                    });

            // This gets broken out of either when the page iterator completes or is paused.
            await pageIterator.IterateAsync();

            // This will then get entered if the page iterator is paused.
            while (pageIterator.State != PagingState.Complete)
            {
                // We want to resume by default.
                bool shouldResume = true;

                // Consider the maximum to determine whether we should resume.
                if (queryConfig.Maximum.HasValue && messagesCount >= queryConfig.Maximum)
                {
                    shouldResume = false;
                }

                // If we're still set to resume and we have page iterator paused handler,
                // call it to determine whether we should resume.
                if (shouldResume && pageIteratorPausedHandler != null)
                {
                    shouldResume = await pageIteratorPausedHandler();
                }
                
                // Resume the page iterator if requested.
                if (shouldResume)
                {
                    await pageIterator.ResumeAsync();
                }
                // Else break out of the loop.
                else
                {
                    break;
                }
            }
        }
    }

    private async Task<string> GetFolderIdByPathAsync(
        GraphServiceClient graphClient,
        string? folderPath)
    {
        string? currentFolderId = null;

        // Continue if a folder path was specified.
        if (folderPath.HasText())
        {
            // Split the path into segments
            var folderNames = folderPath.Split(
                ['\\', '/'],
                StringSplitOptions.RemoveEmptyEntries);

            // Continue if we have segments.
            if (folderNames.Length > 0)
            {
                // Navigate through the folder hierarchy
                for (int i = 0; i < folderNames.Length; i++)
                {
                    // Get the current segment.
                    var folderName = folderNames[i];

                    // Query subfolders of the current folder / root.
                    var foldersResponse = currentFolderId.HasText() ?
                        await graphClient.Me.MailFolders[currentFolderId].ChildFolders
                            .GetAsync(
                                (config) =>
                                {
                                    config.QueryParameters.Filter = $"displayName eq '{folderName}'";
                                }) :
                        await graphClient.Me.MailFolders
                            .GetAsync(
                                (config) =>
                                {
                                    config.QueryParameters.Filter = $"displayName eq '{folderName}'";
                                });

                    // Find the folder with the matching display name.
                    var targetFolder = foldersResponse?.Value?.FirstOrDefault(
                        x => string.Equals(
                            x.DisplayName,
                            folderName,
                            StringComparison.OrdinalIgnoreCase));

                    if (targetFolder == null)
                        throw new Exception($"Could not find folder '{folderName}'");

                    // Use this folder's ID for the next iteration.
                    currentFolderId = targetFolder.Id;
                }
            }
        }

        // Default to inbox if no folder path specified.
        return currentFolderId ?? "inbox";
    }


    private async Task GetInitialAccessTokenAsync(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppTokenInfo tokenInfo)
    {
        // Validate.
        ArgumentNullException.ThrowIfNull(config.ClientId, nameof(config.ClientId));
        ArgumentNullException.ThrowIfNull(config.ClientSecret, nameof(config.ClientSecret));
        ArgumentNullException.ThrowIfNull(config.RedirectUri, nameof(config.RedirectUri));
        ArgumentNullException.ThrowIfNull(tokenInfo.AuthorizationCode, nameof(tokenInfo.AuthorizationCode));

        using (var client = _httpClientFactory.CreateClient())
        {
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", config.ClientId),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("code", tokenInfo.AuthorizationCode),
                new KeyValuePair<string, string>("redirect_uri", config.RedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_secret", config.ClientSecret)
            });

            var response = await client.PostAsync(
                GetOAuthTokenUrl(tokenInfo.AccountLoginEndpoint), 
                requestBody);

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializationHelper.DeserializeFromJson<OAuthTokenResponse>(responseContent);

            UpdateTokenInfo(tokenInfo, tokenResponse);
        }
    }

    private async Task RefreshAccessTokenAsync(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppTokenInfo tokenInfo)
    {
        ArgumentNullException.ThrowIfNull(config.ClientId, nameof(config.ClientId));
        ArgumentNullException.ThrowIfNull(config.ClientSecret, nameof(config.ClientSecret));
        ArgumentNullException.ThrowIfNull(tokenInfo.RefreshToken, nameof(tokenInfo.RefreshToken));

        using (var client = _httpClientFactory.CreateClient())
        {
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", config.ClientId),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("refresh_token", tokenInfo.RefreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_secret", config.ClientSecret)
            });

            var response = await client.PostAsync(
                GetOAuthTokenUrl(tokenInfo.AccountLoginEndpoint), 
                requestBody);

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializationHelper.DeserializeFromJson<OAuthTokenResponse>(responseContent);

            UpdateTokenInfo(tokenInfo, tokenResponse);
        }
    }

    private string GetOAuthAuthorizeUrl(
        string? accountLoginEndpoint)
    {
        return $"{GetOAuthUrl(accountLoginEndpoint)}/authorize";
    }

    private string GetOAuthTokenUrl(
        string? accountLoginEndpoint)
    {
        return $"{GetOAuthUrl(accountLoginEndpoint)}/token";
    }

    private string GetOAuthUrl(
        string? accountLoginEndpoint)
    {
        return $"https://login.microsoftonline.com/{accountLoginEndpoint}/oauth2/v2.0";
    }

    private void UpdateTokenInfo(
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        OAuthTokenResponse? response)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));
        ArgumentNullException.ThrowIfNull(response.AccessToken, nameof(response.AccessToken));
        ArgumentNullException.ThrowIfNull(response.ExpiresIn, nameof(response.ExpiresIn));

        // Update access token.
        tokenInfo.AccessToken = response.AccessToken;
        tokenInfo.AccessTokenExpirationDateTimeUtc = DateTime.UtcNow.AddSeconds(response.ExpiresIn.Value);

        // Ensure we have a refresh token by coping over the existing token if the one returned
        // from the response is null.
        tokenInfo.RefreshToken = response.RefreshToken ?? tokenInfo.RefreshToken;
    }

    private GraphServiceClient CreateGraphServiceClient(
        MicrosoftEntraIdAppTokenInfo tokenInfo)
    {
        ArgumentNullException.ThrowIfNull(tokenInfo.AccessToken, nameof(tokenInfo.AccessToken));

        return new GraphServiceClient(
            new BaseBearerTokenAuthenticationProvider(
                new TokenProvider()
                {
                    Token = tokenInfo.AccessToken,
                }));
    }

    private class TokenProvider : 
        IAccessTokenProvider
    {
        public required string Token { get; init; }

        public AllowedHostsValidator AllowedHostsValidator =>
            throw new NotImplementedException();

        public Task<string> GetAuthorizationTokenAsync(
            Uri uri,
            Dictionary<string, object>? additionalAuthenticationContext = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.Token);
        }
    }
}
