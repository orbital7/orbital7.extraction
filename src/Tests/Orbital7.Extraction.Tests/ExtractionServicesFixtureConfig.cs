using Orbital7.Extraction.Email;

namespace Orbital7.Extraction.Tests;

public class ExtractionServicesFixtureConfig
{
    public required string SyncfusionLicenseKey { get; init; }

    public required MicrosoftEntraIdAppConfig EmailExtractionAppConfig { get; init; }

    public required MicrosoftEntraIdAppTokenInfo EmailExtractionAppTokenInfo { get; init; }

    public required EmailExtractionExportTarget EmailExtractionTarget { get; init; }
}