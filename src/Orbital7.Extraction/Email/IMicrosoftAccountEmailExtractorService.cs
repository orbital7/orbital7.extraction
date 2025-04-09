using Microsoft.Graph.Models;

namespace Orbital7.Extraction.Email;

public interface IMicrosoftAccountEmailExtractorService
{
    string GetAuthorizationUrl(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppTokenInfo tokenInfo);

    Task UpdateAccessTokenAsync(
        MicrosoftEntraIdAppConfig config,
        MicrosoftEntraIdAppTokenInfo tokenInfo);

    Task ExecuteMessagesFolderQueryAsync(
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string folderPath,
        MicrosoftGraphMessagesQueryConfig queryConfig,
        Func<Message, Task<bool>> messageIteratorHandler,
        Func<Task<bool>>? pageIteratorHandler = null);

    Task<List<(string?, string?)>> GatherMessagesSenderSubjectAsync(
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string folderPath,
        MicrosoftGraphMessagesQueryConfig queryConfig);
}
