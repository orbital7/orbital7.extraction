namespace Orbital7.Extraction.Pdf;

public interface IPdfExtractorService
{
    List<string> ExtractPageText(
        byte[] pdfBytes);

    List<string> ExtractPageText(
        string filePath);
}
