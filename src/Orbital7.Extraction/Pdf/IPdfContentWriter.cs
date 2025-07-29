namespace Orbital7.Extraction.Pdf;

public interface IPdfContentWriter<T>
{
    string GetContentTitle(
        T contentItem);

    Task<Stream> WriteContentAsync(
        T contentItem);
}
