using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Orbital7.Extensions;
using Orbital7.Extraction.Email;
using Orbital7.Extraction.Pdf;

namespace Orbital7.Extraction.Tests;

public class EmailExtractionTests(
    ExtractionServicesFixture fixture) :
    IClassFixture<ExtractionServicesFixture>
{
    private readonly ExtractionServicesFixture _fixture = fixture;

    [Fact]
    public void GetAuthorizationUrl()
    {
        var emailExtractorService = _fixture
            .ServiceProvider
            .GetRequiredService<IMicrosoftAccountEmailExtractorService>();

        string authUrl = emailExtractorService.GetAuthorizationUrl(
            _fixture.Config.EmailExtractionAppConfig,
            _fixture.Config.EmailExtractionAppTokenInfo);

        Assert.True(authUrl.HasText());
        Assert.StartsWith("https://", authUrl);
    }

    [Fact]
    public async Task ExtractMessagesAsync()
    {
        var emailExtractorService = _fixture
            .ServiceProvider
            .GetRequiredService<IMicrosoftAccountEmailExtractorService>();
        
        // Ensure output folder exists.
        FileSystemHelper.EnsureFolderExists(
            _fixture.Config.EmailExtractionTarget.ExportFolderPath);
        Assert.True(_fixture.Config.EmailExtractionTarget.ExportActions.Count > 0);

        // Loop through the folder targets.
        foreach (var folderTarget in _fixture.Config.EmailExtractionTarget.ExtractionFolderTargets)
        {
            // Extract messages.
            var messages = await emailExtractorService.ExtractMessagesAsync(
                _fixture.Config.EmailExtractionAppConfig,
                _fixture.Config.EmailExtractionAppTokenInfo,
                folderTarget.EmailAccountFolderPath,
                new MicrosoftGraphMessagesQuery()
                {
                    Orderby = ["receivedDateTime ASC"],
                },
                onTokenInfoUpdated: SaveUserSecretsAsync);
            Assert.True(messages.Count > 0);

            // Validate.
            foreach (var message in messages)
            {
                ValidateEmailMetadata(message);
                Assert.True(message.Body.HasText());
                Assert.Equal(EmailBodyContentType.Html, message.BodyContentType);
            }
            
            // Export to PDF.
            if (_fixture.Config.EmailExtractionTarget.ExportActions.Contains(EmailExportAction.Pdf))
            {
                string filePath = await ExportToPdfAsync(
                    _fixture.ServiceProvider.GetRequiredService<IPdfExportService>(),
                    _fixture.Config.EmailExtractionTarget.ExportFolderPath,
                    folderTarget.OutputFilename,
                    messages);
                
                Assert.EndsWith(".pdf", filePath);
                Assert.True(File.Exists(filePath));
                Assert.True(new FileInfo(filePath).Length > 0);
                
                FileSystemHelper.DeleteFile(filePath);
            }

            // Export to CSV.
            if (_fixture.Config.EmailExtractionTarget.ExportActions.Contains(EmailExportAction.Csv))
            {
                // TODO: Encapsulate this in a service.
                string filePath = ExportToCsv(_fixture.Config.EmailExtractionTarget.ExportFolderPath,
                    folderTarget.OutputFilename,
                    messages);
                
                Assert.EndsWith(".csv", filePath);
                Assert.True(File.Exists(filePath));
                Assert.True(new FileInfo(filePath).Length > 0);
                Assert.True(File.ReadAllLines(filePath).Length > 1);
                
                FileSystemHelper.DeleteFile(filePath);
            }
        }
    }

    [Fact]
    public async Task ExtractMetadataAsync()
    {
        var emailExtractorService = _fixture
            .ServiceProvider
            .GetRequiredService<IMicrosoftAccountEmailExtractorService>();

        // Loop through the folder targets.
        foreach (var folderTarget in _fixture.Config.EmailExtractionTarget.ExtractionFolderTargets)
        {
            // Extract all messages.
            var allEmails = await emailExtractorService.ExtractMetadataAsync(
                _fixture.Config.EmailExtractionAppConfig,
                _fixture.Config.EmailExtractionAppTokenInfo,
                folderTarget.EmailAccountFolderPath,
                new MicrosoftGraphMessagesQuery()
                {
                    Orderby = ["receivedDateTime ASC"],
                },
                onTokenInfoUpdated: SaveUserSecretsAsync);

            // Ensure content.
            Assert.True(allEmails.Count > 0);
            foreach (var email in allEmails)
            {
                ValidateEmailMetadata(email);
            }

            // Extract unread messages.
            var unreadEmails = await emailExtractorService.ExtractMetadataAsync(
                _fixture.Config.EmailExtractionAppConfig,
                _fixture.Config.EmailExtractionAppTokenInfo,
                folderTarget.EmailAccountFolderPath,
                new MicrosoftGraphMessagesQuery()
                {
                    Filter = "isRead eq false",
                    Orderby = ["receivedDateTime ASC"],
                },
                onTokenInfoUpdated: SaveUserSecretsAsync);

            // Ensure unread messages are a subset of all messages.
            Assert.True(unreadEmails.Count > 0);
            Assert.True(unreadEmails.Count < allEmails.Count);
            foreach (var message in unreadEmails)
            {
                Assert.Contains(message, allEmails);
            }
        }
    }

    private void ValidateEmailMetadata(
        EmailMetadata metadata)
    {
        Assert.True(metadata.SenderEmail.HasText());
        Assert.Contains("@", metadata.SenderEmail);
        Assert.EndsWith(".net", metadata.SenderEmail);
        Assert.True(metadata.Subject.HasText());
    }

    private Task SaveUserSecretsAsync(
        IServiceProvider serviceProvider,
        MicrosoftEntraIdAppTokenInfo tokenInfo,
        CancellationToken cancellationToken)
    {
        ConfigurationHelper
            .WriteUserSecrets<ExtractionServicesFixtureConfig, ExtractionServicesFixture>(
                _fixture.Config);
                    
        return Task.CompletedTask;
    }
    
    private async Task<string> ExportToPdfAsync(
        IPdfExportService pdfExportService,
        string exportFolderPath,
        string outputFilename,
        List<EmailMessage> messages)
    {
        var exportFilePath = Path.Combine(
            exportFolderPath,
            $"{outputFilename}.pdf");
        FileSystemHelper.DeleteFile(exportFilePath);

        await pdfExportService.ExportToPdfFileAsync(
            new EmailMessagePdfContentWriter(),
            messages,
            exportFilePath);

        return exportFilePath;
    }

    private string ExportToCsv(
        string exportFolderPath,
        string outputFilename,
        List<EmailMessage> messages)
    {
        var exportFilePath = Path.Combine(
            exportFolderPath,
            $"{outputFilename}.csv");
        FileSystemHelper.DeleteFile(exportFilePath);

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

        return exportFilePath;
    }
}
