namespace Orbital7.Extraction.Email;

public class EmailExtractionExportTarget
{
    public List<EmailExportAction> ExportActions { get; set; } = new();

    public string? ExportFolderPath { get; set; }

    public List<EmailExtractionFolderTarget> ExtractionFolderTargets { get; set; } = new();
}
