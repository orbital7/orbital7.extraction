using Orbital7.Extraction.Pdf;
using Syncfusion.Drawing;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace Orbital7.Extraction.Email;

public class MessageContentPdfWriter :
    IPdfContentWriter<MessageContent>
{
    public string GetContentTitle(
        MessageContent contentItem)
    {
        return $"[{contentItem.SentDateTimeUtc?.ToLocalTime().ToDefaultDateTimeString()}] " +
            $"{contentItem.Subject}";
    }

    public Task<Stream> WriteContentAsync( 
        MessageContent contentItem)
    {
        var htmlConverter = new HtmlToPdfConverter
        {
            ConverterSettings = new BlinkConverterSettings()
        };

        // Convert URL to PDF document.
        var pdfDocument = htmlConverter.Convert(
            contentItem.Body ?? String.Empty,
            String.Empty);

        // Draw a heading at the top of the first page.
        var page = pdfDocument.Pages[0];
        page.Graphics.DrawString(
            $"{contentItem.SentDateTimeUtc?.ToLocalTime().ToDefaultDateTimeString()}: " +
                $"{contentItem.Subject}",
            new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold),
            new PdfSolidBrush(Color.Black), 
            new PointF(10, 10));

        // Add the footer.
        RectangleF bounds = new RectangleF(0, 0, pdfDocument.Pages[0].GetClientSize().Width, 10);
        PdfPageTemplateElement footer = new PdfPageTemplateElement(bounds);
        PdfCompositeField compositeField = new PdfCompositeField(
            new PdfStandardFont(PdfFontFamily.Helvetica, 7),
            new PdfSolidBrush(Color.Black),
            $"{contentItem.Subject} | " +
                $"{contentItem.SentDateTimeUtc?.ToLocalTime().ToDefaultDateTimeString()} | " +
                $"{contentItem.SenderName} <{contentItem.SenderEmail}>");
        compositeField.Bounds = footer.Bounds;
        compositeField.Draw(footer.Graphics, new PointF(0, 20));
        pdfDocument.Template.Bottom = footer;

        // Save the document into stream.
        var stream = new MemoryStream();
        pdfDocument.Save(stream);

        // Closes the document.
        pdfDocument.Close(true);

        return Task.FromResult((Stream)stream);
    }
}
