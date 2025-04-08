namespace Orbital7.Extraction;

public static class ExtractionServicesFactory
{
    public static IServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddExtractionServices();

        return services;
    }

    public static IServiceProvider CreateServiceProvider()
    {
        var services = CreateServiceCollection();
        return services.BuildServiceProvider();
    }
}
