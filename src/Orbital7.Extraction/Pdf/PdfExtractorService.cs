using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace Orbital7.Extraction.Pdf;

public class PdfExtractorService :
    IPdfExtractorService
{
    public List<string> ExtractPageText(
        byte[] pdfBytes)
    {
        using (var ms = new MemoryStream(pdfBytes))
        {
            using (var pdfReader = new PdfReader(ms))
            {
                return ExecutePageTextExtraction(pdfReader);
            }
        }
    }

    public List<string> ExtractPageText(
        string filePath)
    {
        using (var pdfReader = new PdfReader(filePath))
        {
            return ExecutePageTextExtraction(pdfReader);
        }
    }

    private List<string> ExecutePageTextExtraction(
        PdfReader pdfReader)
    {
        var pages = new List<string>();

        using (var pdfDoc = new PdfDocument(pdfReader))
        {
            for (int pageNumber = 1; pageNumber <= pdfDoc.GetNumberOfPages(); pageNumber++)
            {
                pages.Add(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNumber)));
            }
        }

        return pages;
    }
}
