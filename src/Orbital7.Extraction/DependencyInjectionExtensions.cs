using Orbital7.Extraction.Email;

namespace Orbital7.Extraction
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddExtractionServices(
            this IServiceCollection services)
        {
            // Prerequisites.
            services.AddHttpClient();

            // Email.
            services.AddScoped<IMicrosoftAccountEmailExtractorService, MicrosoftAccountEmailExtractorService>();

            return services;
        }
    }
}
