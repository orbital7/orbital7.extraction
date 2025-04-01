using Orbital7.Extraction.Email;

namespace Orbital7.Extraction
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddOrbital7ExtractionServices(
            this IServiceCollection services)
        {
            // Email.
            services.AddScoped<IMicrosoftAccountEmailExtractorService, MicrosoftAccountEmailExtractorService>();


            return services;
        }
    }
}
