namespace Orbital7.Extraction.Csv;

public interface ICsvContentWriter<T>
{
    string?[] GetCsvColumnNames();

    string?[] GetCsvContentItemValues(
        T contentItem);
}
