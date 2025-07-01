namespace ScriptJobsConsole.Scripts;

public class ExtractPerConfig :
    ScriptJobBase
{
    public override async Task ExecuteAsync()
    {
        var config = ExtractionConfig.Load<Program>();
        var serviceProvider = ExtractionServicesFactory.CreateServiceProvider();
        var emailExtractorService = serviceProvider.GetRequiredService<IMicrosoftAccountEmailExtractorService>();

        // Get authorization URL (use in browser and copy/paste code into EmailExtractionAppTokenInfo.AuthorizationCode).
        //var authorizationUrl = emailExtractorService.GetAuthorizationUrl(
        //  config.EmailExtractionAppConfig,
        //  config.EmailExtractionAppTokenInfo);

        // Update access token and save user secrets.
        await emailExtractorService.UpdateAccessTokenAsync(
            config.EmailExtractionAppConfig,
            config.EmailExtractionAppTokenInfo);
        ConfigurationHelper.WriteUserSecrets<ExtractionConfig, Program>(config);

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
                    Console.WriteLine($"Extracting Messages {i}/{total}: {folderTarget.EmailAccountFolderPath}");

                    // Extract messages.
                    var messages = await emailExtractorService.ExtractMessagesContentAsync(
                        config.EmailExtractionAppTokenInfo,
                        folderTarget.EmailAccountFolderPath,
                        new MicrosoftGraphMessagesQueryConfig()
                        {
                            Orderby = ["receivedDateTime ASC"],
                        });

                    // Export to PDF.
                    if (exportTarget.ExportAction == EmailExportAction.Pdf)
                    {
                        Console.WriteLine($"Exporting to PDF {i}/{total}: {folderTarget.EmailAccountFolderPath}");

                        var pdfExportService = serviceProvider.GetRequiredService<IPdfExportService>();
                        var exportFilePath = Path.Combine(
                            exportTarget.ExportFolderPath,
                            folderTarget.OutputFilename + $" [{DateTime.Now.ToFileSystemSafeDateString()}].pdf");
                        await pdfExportService.ExportToPdfFileAsync(
                            new MessageContentPdfWriter(),
                            messages,
                            exportFilePath);
                    }
                }
            }
        }        
    }
}
