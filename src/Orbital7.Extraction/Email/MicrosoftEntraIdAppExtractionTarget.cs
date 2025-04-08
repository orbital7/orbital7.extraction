namespace Orbital7.Extraction.Email;

public class MicrosoftEntraIdAppExtractionTarget
{
    public string? AccountLoginEndpoint { get; set; }

    public string? AuthorizationCode { get; set; }

    public TokenInfo TokenInfo { get; set; } = new();

    public EmailExtractionAction EmailExtractionAction { get; set; } = new();
}
