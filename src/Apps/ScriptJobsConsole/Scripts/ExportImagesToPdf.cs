using Orbital7.Extensions.Graphics;
using Orbital7.Extraction.Images;
using SixLabors.ImageSharp;

namespace ScriptJobsConsole.Scripts;

public class ExportImagesToPdf :
    ScriptJobBase
{
    public override async Task ExecuteAsync()
    {
        // Load a configuration for email extraction (as an example here,
        // we're just using user secrets, but this could be from anywhere).
        var config = ConfigurationHelper.GetConfigurationWithUserSecrets<Config, Program>();

        // Create the services.
        var serviceProvider = ExtractionServicesFactory.CreateServiceProvider(
            config.SyncfusionLicenseKey);
        var pdfExportService = serviceProvider.GetRequiredService<IPdfExportService>();

        // Load the images.
        var images = await ImageSharpHelper.LoadImageFilesAsync(
            config.InputImageFilePaths);

        // Export the images to a PDF file.
        await pdfExportService.ExportToPdfFileAsync<Image>(
            new ImagePdfContentWriter(),
            images,
            config.OutputPdfFilePath);
    }

    public class Config
    {
        public string? SyncfusionLicenseKey { get; set; }

        // TODO.
        public string[] InputImageFilePaths =>
        [
            @"C:\Temp\Test01.jpg",
            @"C:\Temp\Test02.jpg",
        ];

        // TODO.
        public string OutputPdfFilePath => 
            @"C:\Temp\ExportedImages.pdf";
    }
}
