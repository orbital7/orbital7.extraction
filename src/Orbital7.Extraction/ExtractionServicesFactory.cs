namespace Orbital7.Extraction;

public static class ExtractionServicesFactory
{
    public static IServiceCollection CreateServiceCollection(
        string? syncfusionLicenseKey)
    {
        var services = new ServiceCollection();
        services.AddExtractionServices(syncfusionLicenseKey);

        return services;
    }

    public static IServiceProvider CreateServiceProvider(
        string? syncfusionLicenseKey)
    {
        var services = CreateServiceCollection(syncfusionLicenseKey);
        return services.BuildServiceProvider();
    }
}
