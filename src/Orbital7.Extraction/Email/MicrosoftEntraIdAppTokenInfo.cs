namespace Orbital7.Extraction.Email;

public class MicrosoftEntraIdAppTokenInfo :
    TokenInfo
{
    public string? AccountLoginEndpoint { get; set; }

    public string? AuthorizationCode { get; set; }
}
