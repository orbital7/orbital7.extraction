namespace Orbital7.Extraction.Email;

public class MicrosoftAccountEmailExtractorService(
    IHttpClientFactory httpClientFactory) :
    IMicrosoftAccountEmailExtractorService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public string GetAuthorizationUrl(
        MicrosoftEntraIdAppConfig config)
    {
        // Specify offline_access to get a refresh token.
        const string SCOPES = "user.read mail.read offline_access";

        return $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?" +
            $"client_id={config.ClientId}&" +
            $"response_type=code&" +
            $"redirect_uri={config.RedirectUri}&" +
            $"response_mode=query&scope={SCOPES}";
    }

    public async Task<TokenInfo> GetAccessTokenAsync(
        MicrosoftEntraIdAppConfig config)
    {
        var tokenInfo = config.RefreshToken.HasText() ?
            await RefreshAccessTokenAsync(config) :
            await GetInitialAccessTokenAsync(config);

        return tokenInfo;
    }

    private async Task<TokenInfo> GetInitialAccessTokenAsync(
        MicrosoftEntraIdAppConfig config)
    {
        // Validate.
        ArgumentNullException.ThrowIfNull(config.ClientId, nameof(config.ClientId));
        ArgumentNullException.ThrowIfNull(config.ClientSecret, nameof(config.ClientSecret));
        ArgumentNullException.ThrowIfNull(config.AuthorizationCode, nameof(config.AuthorizationCode));
        ArgumentNullException.ThrowIfNull(config.RedirectUri, nameof(config.RedirectUri));

        using (var client = _httpClientFactory.CreateClient())
        {
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", config.ClientId),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("code", config.AuthorizationCode),
                new KeyValuePair<string, string>("redirect_uri", config.RedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_secret", config.ClientSecret)
            });

            var response = await client.PostAsync(
                GetOAuthTokenUrl(config), 
                requestBody);

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializationHelper.DeserializeFromJson<OAuthTokenResponse>(responseContent);

            return ToTokenInfo(tokenResponse);
        }
    }

    private async Task<TokenInfo> RefreshAccessTokenAsync(
        MicrosoftEntraIdAppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config.ClientId, nameof(config.ClientId));
        ArgumentNullException.ThrowIfNull(config.ClientSecret, nameof(config.ClientSecret));
        ArgumentNullException.ThrowIfNull(config.RefreshToken, nameof(config.RefreshToken));

        using (var client = _httpClientFactory.CreateClient())
        {
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", config.ClientId),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("refresh_token", config.RefreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_secret", config.ClientSecret)
            });

            var response = await client.PostAsync(
                GetOAuthTokenUrl(config), 
                requestBody);

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializationHelper.DeserializeFromJson<OAuthTokenResponse>(responseContent);

            return ToTokenInfo(tokenResponse);
        }
    }

    private string GetOAuthTokenUrl(
        MicrosoftEntraIdAppConfig config)
    {
        return $"https://login.microsoftonline.com/{config.TenantId}/oauth2/v2.0/token";
    }

    private TokenInfo ToTokenInfo(
        OAuthTokenResponse? response)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));
        ArgumentNullException.ThrowIfNull(response.AccessToken, nameof(response.AccessToken));
        ArgumentNullException.ThrowIfNull(response.ExpiresIn, nameof(response.ExpiresIn));

        return new TokenInfo()
        {
            AccessToken = response.AccessToken,
            AccessTokenExpirationDateTimeUtc = DateTime.UtcNow.AddSeconds(response.ExpiresIn.Value),
            RefreshToken = response.RefreshToken,
        };
    }
}
