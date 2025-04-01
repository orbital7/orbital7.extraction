using System.Text.Json.Serialization;

namespace Orbital7.Extensions.Integrations;

// TODO: Delete this after upgrade to Orbital7.Extensions 9.0.6+.
public class OAuthTokenResponse
{
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
