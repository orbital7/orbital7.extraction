namespace Orbital7.Extraction;

public static class ExtractionServicesFactory
{
    public static IServiceCollection CreateServiceCollection(
        string? syncFusionLicenseKey)
    {
        var services = new ServiceCollection();
        services.AddExtractionServices(syncFusionLicenseKey);

        return services;
    }

    public static IServiceProvider CreateServiceProvider(
        string? syncFusionLicenseKey)
    {
        var services = CreateServiceCollection(syncFusionLicenseKey);
        return services.BuildServiceProvider();
    }
}
