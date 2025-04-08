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
        
        // Get access token and update it in user secrets.
        var tokenInfo = await emailExtractorService.GetAccessTokenAsync(
            config.EmailExtractionApp,
            emailExtractionTarget);
        emailExtractionTarget.TokenInfo = tokenInfo;
        ConfigurationHelper.WriteUserSecrets<ExtractionConfig, Program>(config);


    }
}
