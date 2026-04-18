using Orbital7.Extraction.Csv;
using Orbital7.Extraction.Email;
using Orbital7.Extraction.Images.BringATrailer;
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

            // Csv.
            services.AddSingleton<ICsvExportService, CsvExportService>();

            // Email.
            services.AddSingleton<IMicrosoftAccountEmailExtractorService, MicrosoftAccountEmailExtractorService>();

            // Images.
            services.AddSingleton<IBringATrailerImageExtractorService, BringATrailerImageExtractorService>();

            // Pdf.
            services.AddSingleton<IPdfExportService, PdfExportService>();
            services.AddSingleton<IPdfExtractorService, PdfExtractorService>();

            return services;
        }
    }
}
