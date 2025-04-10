using iText.Kernel.Pdf;

namespace Orbital7.Extraction.Pdf;

public interface IPdfContentWriter<T>
{
    Task WriteContentAsync(
        T contentItem,
        PdfDocument pdfDocument);
}
