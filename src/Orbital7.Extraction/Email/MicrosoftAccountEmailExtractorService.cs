using Newtonsoft.Json.Linq;

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
        var tokenEndpoint = $"https://login.microsoftonline.com/{config.TenantId}/oauth2/v2.0/token";

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
            var tokenResponse = JObject.Parse(responseContent);

            return new TokenInfo()
            {
                AccessToken = tokenResponse["access_token"].ToString(),
                RefreshToken = tokenResponse["refresh_token"].ToString(),
            };
        }
    }

    private async Task<TokenInfo> RefreshAccessTokenAsync(
        MicrosoftEntraIdAppConfig config)
    {
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
            var tokenResponse = JObject.Parse(responseContent);

            return new TokenInfo()
            {
                AccessToken = tokenResponse["access_token"].ToString(),
                RefreshToken = tokenResponse["refresh_token"].ToString(),
            };
        }
    }

    private string GetOAuthTokenUrl(
        MicrosoftEntraIdAppConfig config)
    {
        return $"https://login.microsoftonline.com/{config.TenantId}/oauth2/v2.0/token";
    }
}
