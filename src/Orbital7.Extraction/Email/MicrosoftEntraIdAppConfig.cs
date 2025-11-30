namespace Orbital7.Extraction.Email;

public record MicrosoftEntraIdAppConfig
{
    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string RedirectUri { get; init; } = string.Empty;
}
