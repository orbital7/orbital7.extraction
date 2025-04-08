namespace Orbital7.Extraction.Email;

public class MicrosoftAccountEmailExtractorService(
    IHttpClientFactory httpClientFactory) :
    IMicrosoftAccountEmailExtractorService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public string GetAuthorizationUrl(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppExtractionTarget extractionTarget)
    {
        // Specify offline_access to get a refresh token.
        const string SCOPES = "user.read mail.read offline_access";

        return $"{GetOAuthAuthorizeUrl(extractionTarget.AccountLoginEndpoint)}?" +
            $"client_id={config.ClientId}&" +
            $"response_type=code&" +
            $"redirect_uri={config.RedirectUri}&" +
            $"response_mode=query&scope={SCOPES}";
    }

    public async Task<TokenInfo> GetAccessTokenAsync(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppExtractionTarget extractionTarget)
    {
        var tokenInfo = extractionTarget.TokenInfo.RefreshToken.HasText() ?
            await RefreshAccessTokenAsync(config, extractionTarget) :
            await GetInitialAccessTokenAsync(config, extractionTarget);

        return tokenInfo;
    }

    private async Task<TokenInfo> GetInitialAccessTokenAsync(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppExtractionTarget extractionTarget)
    {
        // Validate.
        ArgumentNullException.ThrowIfNull(config.ClientId, nameof(config.ClientId));
        ArgumentNullException.ThrowIfNull(config.ClientSecret, nameof(config.ClientSecret));
        ArgumentNullException.ThrowIfNull(config.RedirectUri, nameof(config.RedirectUri));
        ArgumentNullException.ThrowIfNull(extractionTarget.AuthorizationCode, nameof(extractionTarget.AuthorizationCode));

        using (var client = _httpClientFactory.CreateClient())
        {
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", config.ClientId),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("code", extractionTarget.AuthorizationCode),
                new KeyValuePair<string, string>("redirect_uri", config.RedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_secret", config.ClientSecret)
            });

            var response = await client.PostAsync(
                GetOAuthTokenUrl(extractionTarget.AccountLoginEndpoint), 
                requestBody);

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializationHelper.DeserializeFromJson<OAuthTokenResponse>(responseContent);

            return ToTokenInfo(extractionTarget, tokenResponse);
        }
    }

    private async Task<TokenInfo> RefreshAccessTokenAsync(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppExtractionTarget extractionTarget)
    {
        ArgumentNullException.ThrowIfNull(config.ClientId, nameof(config.ClientId));
        ArgumentNullException.ThrowIfNull(config.ClientSecret, nameof(config.ClientSecret));
        ArgumentNullException.ThrowIfNull(extractionTarget.TokenInfo.RefreshToken, nameof(extractionTarget.TokenInfo.RefreshToken));

        using (var client = _httpClientFactory.CreateClient())
        {
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", config.ClientId),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("refresh_token", extractionTarget.TokenInfo.RefreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_secret", config.ClientSecret)
            });

            var response = await client.PostAsync(
                GetOAuthTokenUrl(extractionTarget.AccountLoginEndpoint), 
                requestBody);

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializationHelper.DeserializeFromJson<OAuthTokenResponse>(responseContent);

            return ToTokenInfo(extractionTarget, tokenResponse);
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

    private TokenInfo ToTokenInfo(
        MicrosoftEntraIdAppExtractionTarget extractionTarget,
        OAuthTokenResponse? response)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));
        ArgumentNullException.ThrowIfNull(response.AccessToken, nameof(response.AccessToken));
        ArgumentNullException.ThrowIfNull(response.ExpiresIn, nameof(response.ExpiresIn));

        // Ensure we have a refresh token by coping over the existing token if the one returned
        // from the response is null.
        return new TokenInfo()
        {
            AccessToken = response.AccessToken,
            AccessTokenExpirationDateTimeUtc = DateTime.UtcNow.AddSeconds(response.ExpiresIn.Value),
            RefreshToken = response.RefreshToken ?? extractionTarget.TokenInfo.RefreshToken,
        };
    }
}
