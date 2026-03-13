namespace Orbital7.Extraction.Email;

public record EmailExtractionFolderTarget
{
    public string? EmailAccountFolderPath { get; init; }

    public required string OutputFilename { get; init; }
}
