namespace ScriptJobsConsole.Scripts;

public class AlphaTemp :
    ScriptJobBase
{
    public override async Task ExecuteAsync()
    {
        var config = ExtractionConfig.Load<Program>();
        var emailExtractionTarget = config.EmailExtractionTargets.First();
        var extractionFolderTarget = emailExtractionTarget.EmailExtractionAction.ExtractionFolderTargets.First();

        var serviceProvider = ExtractionServicesFactory.CreateServiceProvider();
        var emailExtractorService = serviceProvider.GetRequiredService<IMicrosoftAccountEmailExtractorService>();

        // Get authorization URL (use in browser and copy/paste code into emailExtractionTarget.AuthorizationCode).
        //var authorizationUrl = emailExtractorService.GetAuthorizationUrl(config.EmailExtractionApp);
        
        // Update access token and save user secrets.
        await emailExtractorService.UpdateAccessTokenAsync(
            config.EmailExtractionApp,
            emailExtractionTarget.TokenInfo);
        ConfigurationHelper.WriteUserSecrets<ExtractionConfig, Program>(config);

        // Extract messages.
        var messages = await emailExtractorService.ExtractMessagesContentAsync(
            emailExtractionTarget.TokenInfo,
            extractionFolderTarget.EmailAccountFolderPath,
            new MicrosoftGraphMessagesQueryConfig()
            {
                Orderby = ["receivedDateTime ASC"],
            });

        // Export to PDF.
        var pdfExportService = serviceProvider.GetRequiredService<IPdfExportService>();
        var exportFilePath = Path.Combine(
            emailExtractionTarget.EmailExtractionAction.ExportFolderPath ?? throw new ArgumentNullException(),
            extractionFolderTarget.OutputFilename + ".pdf");
        await pdfExportService.ExportToPdfFileAsync(
            new MessageContentPdfWriter(),
            messages,
            exportFilePath);
    }
}
