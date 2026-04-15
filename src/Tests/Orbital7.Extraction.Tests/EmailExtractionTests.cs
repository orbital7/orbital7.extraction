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
    public async Task ExtractEmailMessagesAsync()
    {
        var emailExtractorService = _fixture
            .ServiceProvider
            .GetRequiredService<IMicrosoftAccountEmailExtractorService>();
        
        // Ensure output folder exists.
        FileSystemHelper.EnsureFolderExists(
            _fixture.Config.EmailExtractionTarget.ExportFolderPath);

        // Loop through the folder targets.
        foreach (var folderTarget in _fixture.Config.EmailExtractionTarget.ExtractionFolderTargets)
        {
            // Extract messages.
            var messages = await emailExtractorService.ExtractMessagesContentAsync(
                _fixture.Config.EmailExtractionAppConfig,
                _fixture.Config.EmailExtractionAppTokenInfo,
                folderTarget.EmailAccountFolderPath,
                new MicrosoftGraphMessagesQuery()
                {
                    Orderby = ["receivedDateTime ASC"],
                },
                onTokenInfoUpdated: SaveUserSecretsAsync);
            
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
    public async Task ExtractEmailHeaders()
    {
        var emailExtractorService = _fixture
            .ServiceProvider
            .GetRequiredService<IMicrosoftAccountEmailExtractorService>();
        
        // Loop through the folder targets.
        foreach (var folderTarget in _fixture.Config.EmailExtractionTarget.ExtractionFolderTargets)
        {
            // Extract messages.
            var messages = await emailExtractorService.ExtractMessagesSenderSubjectAsync(
                _fixture.Config.EmailExtractionAppConfig,
                _fixture.Config.EmailExtractionAppTokenInfo,
                folderTarget.EmailAccountFolderPath,
                new MicrosoftGraphMessagesQuery()
                {
                    Orderby = ["receivedDateTime ASC"],
                },
                onTokenInfoUpdated: SaveUserSecretsAsync);
        }
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
