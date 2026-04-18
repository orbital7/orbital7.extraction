namespace Orbital7.Extraction.Csv;

public interface ICsvExportService
{
    void ExportToCsvFile<T>(
        ICsvContentWriter<T> csvContentWriter,
        IList<T> contentItems,
        string exportFilePath);
}
