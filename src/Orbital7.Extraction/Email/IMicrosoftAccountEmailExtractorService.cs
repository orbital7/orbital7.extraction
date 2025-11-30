using Microsoft.Graph.Models;

namespace Orbital7.Extraction.Email;

public interface IMicrosoftAccountEmailExtractorService
{
    string GetAuthorizationUrl(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo);

    Task ExecuteMessagesFolderQueryAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        MicrosoftGraphMessagesQueryConfig queryConfig,
        Func<Message, Task<bool>> messageIteratorHandler,
        Func<Task<bool>>? pageIteratorHandler = null,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, Task>? onTokenInfoUpdated = null);

    Task<List<(string?, string?)>> ExtractMessagesSenderSubjectAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        MicrosoftGraphMessagesQueryConfig queryConfig,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, Task>? onTokenInfoUpdated = null);

    Task<List<MessageContent>> ExtractMessagesContentAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        MicrosoftGraphMessagesQueryConfig queryConfig,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, Task>? onTokenInfoUpdated = null);

    Task<List<FileAttachment>> ExtractMessageFileAttachmentsAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? messageId,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, Task>? onTokenInfoUpdated = null);
}
