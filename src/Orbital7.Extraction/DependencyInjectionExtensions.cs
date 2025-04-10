using Orbital7.Extraction.Email;
using Orbital7.Extraction.Pdf;

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

            // Pdf.
            services.AddScoped<IPdfExportService, PdfExportService>();

            return services;
        }
    }
}
