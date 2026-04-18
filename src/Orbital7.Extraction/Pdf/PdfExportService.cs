using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Interactive;

namespace Orbital7.Extraction.Pdf;

public class PdfExportService :
    IPdfExportService
{
    public async Task ExportToPdfFileAsync<T>(
        IPdfContentWriter<T> pdfContentWriter,
        IList<T> contentItems,
        string exportFilePath)
    {
        using (PdfDocument pdfDocument = new PdfDocument())
        {
            // Loop through the content items and use the export processor
            // to generate PDF content to write.
            foreach (var contentItem in contentItems)
            {
                int nextPage = pdfDocument.Pages.Count;

                // Extract the content to a PDF stream.
                var pdfContentStream = await pdfContentWriter.WriteContentAsync(
                    contentItem);

                // Merge it into the current PDF document.
                PdfDocumentBase.Merge(pdfDocument, pdfContentStream);

                // Create the bookmark for the content item.
                PdfBookmark bookmark = pdfDocument.Bookmarks.Add(pdfContentWriter.GetContentTitle(contentItem));
                //Sets the destination page.
                bookmark.Destination = new PdfDestination(pdfDocument.Pages[nextPage]);
                //Sets the destination location.
                bookmark.Destination.Location = new PointF(0, 0);
                //Sets the text style and color.
                bookmark.TextStyle = PdfTextStyle.Bold;
                bookmark.Color = Color.Red;

                Console.Write(".");
            }

            // Save and close.
            pdfDocument.Save(exportFilePath);
            pdfDocument.Close(true);
        }
    }
}
