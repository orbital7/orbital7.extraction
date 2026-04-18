namespace Orbital7.Extraction.Pdf;

public interface IPdfExportService
{
    Task ExportToPdfFileAsync<T>(
        IPdfContentWriter<T> pdfContentWriter,
        IList<T> contentItems,
        string exportFilePath);
}
