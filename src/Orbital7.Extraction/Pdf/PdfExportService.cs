using iText.Kernel.Pdf;

namespace Orbital7.Extraction.Pdf;

public class PdfExportService :
    IPdfExportService
{
    public async Task ExportToPdfFileAsync<T>(
        IPdfContentWriter<T> pdfContentWriter,
        List<T> contentItems,
        string exportFilePath)
    {
        Environment.SetEnvironmentVariable(
            "ITEXT_BOUNCY_CASTLE_FACTORY_NAME", 
            "bouncy-castle");

        // Create the main PDF at the specified export path.
        using var pdfWriter = new PdfWriter(exportFilePath);
        using var pdfDocument = new PdfDocument(pdfWriter);

        // Loop through the content items and use the export processor
        // to generate PDF content to write.
        var i = 0;
        foreach (var contentItem in contentItems)
        {
            i++;
            Console.WriteLine($"Processing {i} of {contentItems.Count}");

            await pdfContentWriter.WriteContentAsync(
                contentItem,
                pdfDocument);
        }

        pdfDocument.Close();
    }
}
