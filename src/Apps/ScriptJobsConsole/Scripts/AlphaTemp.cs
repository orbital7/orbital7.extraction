namespace ScriptJobsConsole.Scripts;

public class AlphaTemp :
    ScriptJobBase
{
    public override async Task ExecuteAsync()
    {
        var config = ExtractionConfig.Load<Program>();
        var emailExtractionTarget = config.EmailExtractionTargets.First();

        var serviceProvider = ExtractionServicesFactory.CreateServiceProvider();
        var emailExtractorService = serviceProvider.GetRequiredService<IMicrosoftAccountEmailExtractorService>();

        // Get authorization URL (use in browser and copy/paste code into emailExtractionTarget.AuthorizationCode).
        //var authorizationUrl = emailExtractorService.GetAuthorizationUrl(config.EmailExtractionApp);
        
        // Update access token and save user secrets.
        await emailExtractorService.UpdateAccessTokenAsync(
            config.EmailExtractionApp,
            emailExtractionTarget.TokenInfo);
        ConfigurationHelper.WriteUserSecrets<ExtractionConfig, Program>(config);

        // Test gathering message headers.
        var messageHeaders = await emailExtractorService.GatherMessagesSenderSubjectAsync(
            emailExtractionTarget.TokenInfo,
            emailExtractionTarget.EmailExtractionAction.ExtractionFolderTargets.First().EmailAccountFolderPath,
            new MicrosoftGraphMessagesQueryConfig());
    }
}
