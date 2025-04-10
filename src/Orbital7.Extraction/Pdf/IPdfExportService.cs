namespace Orbital7.Extraction.Pdf;

public interface IPdfExportService
{
    Task ExportToPdfFileAsync<T>(
        IPdfContentWriter<T> pdfContentWriter,
        List<T> contentItems,
        string exportFilePath);
}
