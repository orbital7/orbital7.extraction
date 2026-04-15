using Orbital7.Extensions;

namespace Orbital7.Extraction.Tests;

public class ExtractionServicesFixture
{
    public ExtractionServicesFixtureConfig Config { get; init; }
    
    public IServiceProvider ServiceProvider { get; init; }
    
    public ExtractionServicesFixture()
    { 
        this.Config = ConfigurationHelper
            .GetConfigurationWithUserSecrets<ExtractionServicesFixtureConfig, ExtractionServicesFixture>();

        // Create the services.
        this.ServiceProvider = ExtractionServicesFactory.CreateServiceProvider(
            this.Config.SyncfusionLicenseKey);
    }
}