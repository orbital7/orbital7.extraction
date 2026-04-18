using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Text.RegularExpressions;

namespace Orbital7.Extraction.Email;

public class MicrosoftAccountEmailExtractorService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory) :
    IMicrosoftAccountEmailExtractorService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public string GetAuthorizationUrl(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo)
    {
        var client = new MicrosoftEntraIdAppOAuthClient(
            _serviceProvider,
            _httpClientFactory,
            appConfig,
            tokenInfo);

        return client.GetAuthorizationUrl();
    }

    public async Task<List<EmailMetadata>> ExtractMetadataAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        EmailExtractionQuery query,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null)
    {
        var messages = new List<EmailMetadata>();

        // Limit selection to only what we need.
        var updatedQueryConfig = query.CloneIgnoringReferenceProperties(
            overrides: new Dictionary<string, object?>
            {
                { 
                    nameof(EmailExtractionQuery.Select), 
                    new[] { "sender", "subject" } },
            });

        // Execute.
        await ExecuteMessagesFolderQueryAsync(
            appConfig,
            tokenInfo,
            folderPath,
            updatedQueryConfig,
            (msg) => // Message iterator handler.
            {
                // Add the summary and keep going.
                messages.Add(msg.ToEmailMetadata<EmailMetadata>());
                return Task.FromResult(true);
            },
            onTokenInfoUpdated: onTokenInfoUpdated);

        return messages;
    }

    public async Task<List<EmailMessage>> ExtractMessagesAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        EmailExtractionQuery query,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null)
    {
        var messages = new List<EmailMessage>();

        // Limit selection to only what we need.
        var updatedQueryConfig = query.CloneIgnoringReferenceProperties(
            overrides: new Dictionary<string, object?>
            {
                { 
                    nameof(EmailExtractionQuery.Select),
                    new[] { "sentdatetime", "sender", "subject", "body" }
                },
            });
        
        await ExecuteMessagesFolderQueryAsync(
            appConfig,
            tokenInfo,
            folderPath,
            query,
            async (msg) => // Message iterator handler.
            {
                var message = msg.ToEmailMetadata<EmailMessage>();
                message.Body = msg.Body?.Content;
                message.BodyContentType =
                    msg.Body != null ?
                        msg.Body?.ContentType == BodyType.Html ?
                            EmailBodyContentType.Html :
                            EmailBodyContentType.Text :
                        null;

                // If configured, for HTML messages, extract the inline image attachments and
                // embed them into the body.
                if (query.DownloadAttachments && 
                    message.BodyContentType == EmailBodyContentType.Html)
                {
                    var attachments = await ExtractMessageFileAttachmentsAsync(appConfig, tokenInfo, msg.Id);
                    EmbedInlineImageAttachmentsIntoBody(message, attachments);
                }

                messages.Add(message);

                return true;
            },
            onTokenInfoUpdated: onTokenInfoUpdated);

        return messages;
    }

    private void EmbedInlineImageAttachmentsIntoBody(
        EmailMessage message,
        List<FileAttachment> attachments)
    {
        if (message.Body.HasText() && 
            message.BodyContentType == EmailBodyContentType.Html)
        {
            var inlineAttachments = attachments
                .Where(att => 
                    att.IsInline == true && 
                    att.ContentId != null && 
                    att.ContentBytes != null)
                .ToDictionary(att => att.ContentId!, att => att);

            if (inlineAttachments.Count > 0)
            {
                var updatedHtml = Regex.Replace(message.Body, @"<img[^>]*src=[""']cid:(.+?)[""'][^>]*>", match =>
                {
                    var cid = match.Groups[1].Value;
                    if (inlineAttachments.TryGetValue(cid, out var fileAttachment))
                    {
                        string mimeType = fileAttachment.ContentType ?? "image/png"; // fallback
                        string base64 = Convert.ToBase64String(fileAttachment.ContentBytes!);
                        string dataUri = $"data:{mimeType};base64,{base64}";
                        return match.Value.Replace($"cid:{cid}", dataUri);
                    }
                    return match.Value; // fallback to original if not found
                }, RegexOptions.IgnoreCase);

                message.Body = updatedHtml;
            }
        }
    }

    public async Task ExecuteMessagesFolderQueryAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        EmailExtractionQuery query,
        Func<Message, Task<bool>> messageIteratorHandler,
        Func<Task<bool>>? pageIteratorPausedHandler = null,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null)
    {
        var graphClient = await CreateGraphServiceClientAsync(appConfig, tokenInfo, onTokenInfoUpdated);
        var messagesCount = 0;

        // Get the folder ID for the specified path.
        var folderId = await GetFolderIdByPathAsync(graphClient, folderPath);

        // Search.
        var messagesResponse = await graphClient.Me.MailFolders[folderId].Messages
            .GetAsync(requestConfig =>
            {
                // Set the request query parameters.
                requestConfig.QueryParameters.Top = query.Top;
                requestConfig.QueryParameters.Filter = query.Filter;
                requestConfig.QueryParameters.Orderby = query.Orderby;
                requestConfig.QueryParameters.Select = query.Select;
                requestConfig.QueryParameters.Expand = query.Expand;

                // Set the request headers.
                if (query.Headers != null)
                {
                    foreach (var header in query.Headers)
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
                            query.Maximum.HasValue && 
                            messagesCount >= query.Maximum)
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
                        if (query.Headers != null)
                        {
                            foreach (var header in query.Headers)
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
                if (query.Maximum.HasValue && messagesCount >= query.Maximum)
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

    public async Task<List<FileAttachment>> ExtractMessageFileAttachmentsAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? messageId,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null)
    {
        var attachments = new List<FileAttachment>();
        var graphClient = await CreateGraphServiceClientAsync(appConfig, tokenInfo, onTokenInfoUpdated);

        // Execute.
        if (messageId.HasText())
        {
            var attachmentsResponse = await graphClient.Me.Messages[messageId].Attachments
                .GetAsync();

            // Continue if we have attachments.
            if (attachmentsResponse != null)
            {
                var pageIterator = PageIterator<Attachment, AttachmentCollectionResponse>
                    .CreatePageIterator(
                        graphClient,
                        attachmentsResponse,
                        (attachment) =>
                        {
                            if (attachment is FileAttachment fileAttachment)
                            {
                                attachments.Add(fileAttachment);
                            }

                            return true;
                        });

                await pageIterator.IterateAsync();
            }
        }

        return attachments;
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

    private async Task<GraphServiceClient> CreateGraphServiceClientAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated)
    {
        // Create the OAuth client to use to manage the access token.
        var oAuthClient = new MicrosoftEntraIdAppOAuthClient(
            _serviceProvider,
            _httpClientFactory,
            appConfig,
            tokenInfo,
            onTokenInfoUpdated: onTokenInfoUpdated);

        // Ensure we have a valid access token.
        var accessToken = await oAuthClient.EnsureValidAccessTokenAsync();

        // Create the graph service client using the access token.
        return new GraphServiceClient(
            new BaseBearerTokenAuthenticationProvider(
                new TokenProvider()
                {
                    Token = accessToken,
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
