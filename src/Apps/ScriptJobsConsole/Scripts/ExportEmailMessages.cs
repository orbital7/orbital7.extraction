using Orbital7.Extraction.Csv;
using System.Text;

namespace ScriptJobsConsole.Scripts;

public class ExportEmailMessages :
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
        var emailExtractorService = serviceProvider.GetRequiredService<IMicrosoftAccountEmailExtractorService>();

        // Get authorization URL (use in browser and copy/paste code into EmailExtractionAppTokenInfo.AuthorizationCode).
        //var authorizationUrl = emailExtractorService.GetAuthorizationUrl(
        //  config.EmailExtractionAppConfig,
        //  config.EmailExtractionAppTokenInfo);
        
        // Prepare to export targets.
        int i = 0;
        int total = config.EmailExtractionTargets.Sum(x => x.ExtractionFolderTargets.Count);

        // Extract and export.
        foreach (var exportTarget in config.EmailExtractionTargets)
        {
            // Ensure output folder exists.
            FileSystemHelper.EnsureFolderExists(exportTarget.ExportFolderPath);

            // Loop through the folder targets.
            foreach (var folderTarget in exportTarget.ExtractionFolderTargets)
            {
                // Notify.
                i++;
                Console.Write($"Extracting Messages {i}/{total}: {folderTarget.EmailAccountFolderPath}...");

                // Extract messages.
                var messages = await emailExtractorService.ExtractMessagesAsync(
                    config.EmailExtractionAppConfig,
                    config.EmailExtractionAppTokenInfo,
                    folderTarget.EmailAccountFolderPath,
                    new EmailExtractionQuery()
                    {
                        Orderby = [EmailExtractionQuery.ORDERING_RECEIVED_DATE_TIME_ASC],
                    },
                    onTokenInfoUpdated: (serviceProvider, tokenInfo, cancellationToken) =>
                    {
                        ConfigurationHelper.WriteUserSecrets<Config, Program>(config);
                        return Task.CompletedTask;
                    });

                Console.WriteLine($"done ({messages.Count} extracted)");

                // Export to PDF.
                if (exportTarget.ExportActions.Contains(EmailExportAction.Pdf))
                {
                    Console.Write($"Exporting to PDF {i}/{total}: {folderTarget.EmailAccountFolderPath}");
                    await ExportToPdfAsync(
                        serviceProvider.GetRequiredService<IPdfExportService>(),
                        exportTarget.ExportFolderPath,
                        folderTarget.OutputFilename,
                        messages);
                    Console.WriteLine("done");
                }

                // Export to CSV.
                if (exportTarget.ExportActions.Contains(EmailExportAction.Csv))
                {
                    Console.Write($"Exporting to CSV {i}/{total}: {folderTarget.EmailAccountFolderPath}");
                    var exportFilePath = Path.Combine(
                        exportTarget.ExportFolderPath,
                        $"{folderTarget.OutputFilename} [{DateTime.Now.ToFileSystemSafeDateString()}].csv".Trim());
                    ExportToCsv(
                        serviceProvider.GetRequiredService<ICsvExportService>(),
                        exportTarget.ExportFolderPath,
                        folderTarget.OutputFilename,
                        messages.ToList<EmailMetadata>());
                    Console.WriteLine("done");
                }
            }
        }
    }

    private async Task ExportToPdfAsync(
        IPdfExportService pdfExportService,
        string exportFolderPath,
        string outputFilename,
        List<EmailMessage> emails)
    {
        var exportFilePath = Path.Combine(
            exportFolderPath,
            $"{outputFilename} [{DateTime.Now.ToFileSystemSafeDateString()}].pdf".Trim());

        await pdfExportService.ExportToPdfFileAsync(
            new EmailMessagePdfContentWriter(),
            emails,
            exportFilePath);
    }

    private void ExportToCsv(
        ICsvExportService csvExportService,
        string exportFolderPath,
        string outputFilename,
        List<EmailMetadata> emails)
    {
        var exportFilePath = Path.Combine(
            exportFolderPath,
            $"{outputFilename} [{DateTime.Now.ToFileSystemSafeDateString()}].csv".Trim());

        csvExportService.ExportToCsvFile(
            new EmailMetadataCsvContentWriter(),
            emails,
            exportFilePath);
    }

    public class Config
    {
        public required string SyncfusionLicenseKey { get; init; }

        public required MicrosoftEntraIdAppConfig EmailExtractionAppConfig { get; init; }

        public required MicrosoftEntraIdAppTokenInfo EmailExtractionAppTokenInfo { get; init; }

        public required List<EmailExtractionExportTarget> EmailExtractionTargets { get; init; }
    }
}
