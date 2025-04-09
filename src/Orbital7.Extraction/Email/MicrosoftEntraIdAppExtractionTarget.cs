namespace Orbital7.Extraction.Email;

public class MicrosoftEntraIdAppExtractionTarget
{
    public MicrosoftEntraIdAppTokenInfo TokenInfo { get; set; } = new();

    public EmailExtractionAction EmailExtractionAction { get; set; } = new();
}
