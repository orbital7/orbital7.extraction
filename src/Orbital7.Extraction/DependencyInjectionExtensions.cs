using Orbital7.Extraction.Email;
using Orbital7.Extraction.Images;
using Orbital7.Extraction.Pdf;

namespace Orbital7.Extraction
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddExtractionServices(
            this IServiceCollection services,
            string? syncfusionLicenseKey)
        {
            // Licensing.
            if (syncfusionLicenseKey.HasText())
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
            }

            // Prerequisites.
            services.AddHttpClient();

            // Email.
            services.AddScoped<IMicrosoftAccountEmailExtractorService, MicrosoftAccountEmailExtractorService>();

            // Images.
            services.AddScoped<IBringATrailerImageExtractorService, BringATrailerImageExtractorService>();

            // Pdf.
            services.AddScoped<IPdfExportService, PdfExportService>();
            services.AddScoped<IPdfExtractorService, PdfExtractorService>();

            return services;
        }
    }
}
