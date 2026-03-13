namespace Orbital7.Extraction.Email;

public record EmailExtractionExportTarget
{
    public List<EmailExportAction> ExportActions { get; init; } = new();

    public required string ExportFolderPath { get; init; }

    public List<EmailExtractionFolderTarget> ExtractionFolderTargets { get; init; } = new();
}
