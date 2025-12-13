using System.Text;

namespace ScriptJobsConsole.Scripts;

public class ExtractPerConfig :
    ScriptJobBase
{
    public override async Task ExecuteAsync()
    {
        var config = ExtractionConfig.Load<Program>();
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
            if (exportTarget.ExportFolderPath.HasText())
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
                    var messages = await emailExtractorService.ExtractMessagesContentAsync(
                        config.EmailExtractionAppConfig,
                        config.EmailExtractionAppTokenInfo,
                        folderTarget.EmailAccountFolderPath,
                        new MicrosoftGraphMessagesQueryConfig()
                        {
                            Orderby = ["receivedDateTime ASC"],
                        },
                        onTokenInfoUpdated: (serviceProvider, tokenInfo, cancellationToken) =>
                        {
                            ConfigurationHelper.WriteUserSecrets<ExtractionConfig, Program>(config);
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
                        // TODO: Encapsulate this in a service.
                        ExportToCsv(exportTarget.ExportFolderPath,
                            folderTarget.OutputFilename,
                            messages);
                    }
                }
            }
        }
    }

    private async Task ExportToPdfAsync(
        IPdfExportService pdfExportService,
        string exportFolderPath,
        string? outputFilename,
        List<MessageContent> messages)
    {
        var exportFilePath = Path.Combine(
            exportFolderPath,
            $"{outputFilename} [{DateTime.Now.ToFileSystemSafeDateString()}].pdf".Trim());

        await pdfExportService.ExportToPdfFileAsync(
            new MessageContentPdfWriter(),
            messages,
            exportFilePath);
    }

    private void ExportToCsv(
        string exportFolderPath,
        string? outputFilename,
        List<MessageContent> messages)
    {
        var exportFilePath = Path.Combine(
            exportFolderPath,
            $"{outputFilename} [{DateTime.Now.ToFileSystemSafeDateString()}].csv".Trim());

        var sb = new StringBuilder();
        sb.AppendCommaSeparatedValuesLine("SentDateTime", "Subject", "SenderName", "SenderEmail");
        foreach (var message in messages)
        {
            sb.AppendCommaSeparatedValuesLine(
                message.SentDateTimeUtc?.ToLocalTime().ToDefaultDateTimeString(),
                message.Subject.EncloseInQuotes(),
                message.SenderName.EncloseInQuotes(),
                message.SenderEmail);
        }
        File.WriteAllText(exportFilePath, sb.ToString());
    }
}
