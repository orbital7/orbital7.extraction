namespace Orbital7.Extraction.Email;

public interface IMicrosoftAccountEmailExtractorService
{
    string GetAuthorizationUrl(
        MicrosoftEntraIdAppConfig config);

    Task<TokenInfo> GetAccessTokenAsync(
        MicrosoftEntraIdAppConfig config);
}
