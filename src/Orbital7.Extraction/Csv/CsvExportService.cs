using System.Text;

namespace Orbital7.Extraction.Csv;

public class CsvExportService :
    ICsvExportService
{
    public void ExportToCsvFile<T>(
        ICsvContentWriter<T> csvContentWriter, 
        IList<T> contentItems, 
        string exportFilePath)
    {
        var sb = new StringBuilder();

        sb.AppendCommaSeparatedValuesLine(
            csvContentWriter.GetCsvColumnNames());

        foreach (var contentItem in contentItems)
        {
            sb.AppendCommaSeparatedValuesLine(
                csvContentWriter.GetCsvContentItemValues(contentItem));
        }

        File.WriteAllText(
            exportFilePath, 
            sb.ToString().Trim());
    }
}
