namespace Orbital7.Extraction.Email;

public class MicrosoftEntraIdAppOAuthClient :
    OAuthApiClientBase<MicrosoftEntraIdAppTokenInfo>, IMicrosoftEntraIdAppOAuthClient
{
    private readonly MicrosoftEntraIdAppConfig _config;

    protected override string OAuthTokenEndpointUrl => GetOAuthTokenUrl(this.TokenInfo.AccountLoginEndpoint);

    public MicrosoftEntraIdAppOAuthClient(
        IServiceProvider serviceProvider, 
        IHttpClientFactory httpClientFactory,
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppTokenInfo tokenInfo, 
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null, 
        string? httpClientName = null) : 
        base(
            serviceProvider, 
            httpClientFactory, 
            tokenInfo, 
            onTokenInfoUpdated, 
            httpClientName)
    {
        _config = config;
    }

    public override string GetAuthorizationUrl(
        string? state = null)
    {
        // Specify offline_access to get a refresh token.
        const string SCOPES = "user.read mail.read offline_access";

        return $"{GetOAuthAuthorizeUrl(this.TokenInfo.AccountLoginEndpoint)}?" +
            $"client_id={_config.ClientId}&" +
            $"response_type=code&" +
            $"redirect_uri={_config.RedirectUri}&" +
            $"response_mode=query&" +
            $"scope={SCOPES}";
    }

    protected override Task<List<KeyValuePair<string, string>>> CreateGetTokenRequestAsync()
    {
        ArgumentNullException.ThrowIfNull(this.TokenInfo.AuthorizationCode, nameof(this.TokenInfo.AuthorizationCode));

        return Task.FromResult(new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("client_id", _config.ClientId),
            new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
            new KeyValuePair<string, string>("code", this.TokenInfo.AuthorizationCode),
            new KeyValuePair<string, string>("redirect_uri", _config.RedirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_secret", _config.ClientSecret),
        });
    }

    protected override List<KeyValuePair<string, string>> CreateRefreshTokenRequest(
        string refreshToken)
    {
        return [
            new KeyValuePair<string, string>("client_id", _config.ClientId),
            new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_secret", _config.ClientSecret)
        ];
    }

    protected override Exception CreateUnsuccessfulResponseException(
        HttpResponseMessage httpResponse,
        string responseBody)
    {
        if (responseBody.StartsWith("{\"error\":"))
        {
            var errorResponse = JsonSerializationHelper.DeserializeFromJson<ErrorResponse>(responseBody);
            throw new Exception($"Error ({errorResponse.Error}): {errorResponse.ErrorDescription}");
        }
        else
        {
            return base.CreateUnsuccessfulResponseException(httpResponse, responseBody);
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
}
