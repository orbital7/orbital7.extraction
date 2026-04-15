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
        MicrosoftGraphMessagesQuery query,
        Func<Message, Task<bool>> messageIteratorHandler,
        Func<Task<bool>>? pageIteratorHandler = null,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null);

    Task<List<EmailMetadata>> ExtractMetadataAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        MicrosoftGraphMessagesQuery query,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null);

    Task<List<EmailMessage>> ExtractMessagesAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? folderPath,
        MicrosoftGraphMessagesQuery query,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null);

    Task<List<FileAttachment>> ExtractMessageFileAttachmentsAsync(
        MicrosoftEntraIdAppConfig appConfig,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        string? messageId,
        Func<IServiceProvider, MicrosoftEntraIdAppTokenInfo, CancellationToken, Task>? onTokenInfoUpdated = null);
}
