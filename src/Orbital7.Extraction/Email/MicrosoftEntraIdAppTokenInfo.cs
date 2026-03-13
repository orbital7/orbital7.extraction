namespace Orbital7.Extraction.Email;

public record MicrosoftEntraIdAppTokenInfo :
    TokenInfo
{
    // This should be "common" for personal Microsoft accounts
    // or the tenant ID for work accounts.
    public required string AccountLoginEndpoint { get; set; } = "common";

    // This should be the code the the user received after 
    // authorizing the app (it will be in the redirect URL as the
    // "code" query parameter).
    public string? AuthorizationCode { get; set; }
}
