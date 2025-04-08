namespace Orbital7.Extraction.Email;

public class EmailExtractionAction
{
    public EmailExportAction ExportAction { get; set; }

    public string? ExportFolderPath { get; set; }

    public List<EmailExtractionFolderTarget> ExtractionFolderTargets { get; set; } = new();
}
