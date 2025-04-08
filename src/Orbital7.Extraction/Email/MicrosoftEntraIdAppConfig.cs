namespace Orbital7.Extraction.Email;

public class MicrosoftEntraIdAppConfig
{
    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    // TODO: I believe this is not needed, but keep for now.
    [Obsolete]
    public string? TenantId { get; set; }

    public string? RedirectUri { get; set; }

}
