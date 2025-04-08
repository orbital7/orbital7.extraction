namespace Orbital7.Extraction.Email;

public interface IMicrosoftAccountEmailExtractorService
{
    string GetAuthorizationUrl(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppExtractionTarget extractionTarget);

    Task<TokenInfo> GetAccessTokenAsync(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppExtractionTarget extractionTarget);
}
