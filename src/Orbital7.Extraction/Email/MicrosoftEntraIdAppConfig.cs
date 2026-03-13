namespace Orbital7.Extraction.Email;

public record MicrosoftEntraIdAppConfig
{
    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }

    public required string RedirectUri { get; init; }
}
