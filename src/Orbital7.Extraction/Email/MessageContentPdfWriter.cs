using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf.Navigation;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.StyledXmlParser.Resolver.Resource;
using Orbital7.Extraction.Pdf;

namespace Orbital7.Extraction.Email;

public class MessageContentPdfWriter :
    IPdfContentWriter<MessageContent>
{
    public Task WriteContentAsync( 
        MessageContent contentItem,
        PdfDocument pdfDocument)
    {
        // NOTE: Because the iText.Html2pdf.HtmlConverter.ConvertToPdf() method closes
        // the provided pdfDocument instance, we need to create a temporary PDF file
        // and then copy over the pages to the main PDF document.

        // Create a temporary PDF file.
        var tempPdfFilePath = CreateTempPdf(
            contentItem);

        // Read the temporary PDF file in and append to the main PDF.
        using var pdfReader = new PdfReader(tempPdfFilePath);
        using var pdfContent = new PdfDocument(pdfReader);

        // Determine the starting page for the copied content.
        int startPage = pdfDocument.GetNumberOfPages() - pdfContent.GetNumberOfPages() + 1;

        // Create a root outline if it doesn't exist.
        var rootOutline = pdfDocument.GetOutlines(false);
        if (rootOutline == null)
        {
            rootOutline = pdfDocument.GetOutlines(true);
        }

        // Add a bookmark (outline) pointing to the start of the copied pages.
        string outlineTitle = $"{contentItem.SentDateTimeUtc?.ToLocalTime().ToDefaultDateTimeString()}: {contentItem.Subject}";
        var outline = rootOutline.AddOutline(outlineTitle);
        outline.AddDestination(PdfExplicitDestination.CreateFit(pdfDocument.GetPage(startPage)));

        // Copy over the pages.
        pdfContent.CopyPagesTo(1, pdfContent.GetNumberOfPages(), pdfDocument);
        pdfContent.Close();

        // Delete the temporary PDF file.
        File.Delete(tempPdfFilePath);

        return Task.CompletedTask;
    }

    private string CreateTempPdf(
        MessageContent contentItem)
    {
        var exportFilePath = Path.GetTempFileName();

        using var pdfWriter = new PdfWriter(exportFilePath);
        using var pdfDocument = new PdfDocument(pdfWriter);
        pdfDocument.AddEventHandler(
            PdfDocumentEvent.END_PAGE,
            new HeaderFooterEventHandler(contentItem));

        var converterProperties = new ConverterProperties();
        converterProperties.SetResourceRetriever(new DefaultResourceRetriever());

        // The problem here is that the ConvertToPdf() method closes pdfDocument,
        // otherwise we could just keep adding on to the same document.
        HtmlConverter.ConvertToPdf(
            contentItem.Body,
            pdfDocument,
            converterProperties);

        pdfDocument.Close();

        return exportFilePath;
    }

    private class HeaderFooterEventHandler : 
        AbstractPdfDocumentEventHandler
    {
        private readonly MessageContent _messageContent;

        public HeaderFooterEventHandler(
            MessageContent messageContent)
        {
            _messageContent = messageContent;
        }

        protected override void OnAcceptedEvent(
            AbstractPdfDocumentEvent eventToHandle)
        {
            var pdfDocEvent = (PdfDocumentEvent)eventToHandle;
            var pdfDocument = pdfDocEvent.GetDocument();
            var page = pdfDocEvent.GetPage();

            var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDocument);
            var document = new iText.Layout.Document(pdfDocument);

            //// Add header.
            //var headerText = _messageContent.Subject;
            //var header = new Paragraph(headerText)
            //    .SetTextAlignment(TextAlignment.CENTER)
            //    .SetFontSize(8);
            //document.ShowTextAligned(
            //    header,
            //    297.5f,
            //    806,
            //    pdfDocument.GetPageNumber(page),
            //    TextAlignment.CENTER,
            //    VerticalAlignment.TOP,
            //    0);

            // Add footer.
            var footerText =
                $"{_messageContent.Subject} | " +
                $"{_messageContent.SentDateTimeUtc?.ToLocalTime().ToDefaultDateTimeString()} | " +
                $"{_messageContent.SenderName} <{_messageContent.SenderEmail}>";
            var footer = new Paragraph(footerText)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(8);
            document.ShowTextAligned(
                footer,
                297.5f,
                20,
                pdfDocument.GetPageNumber(page),
                TextAlignment.CENTER,
                VerticalAlignment.BOTTOM,
                0);

            canvas.Release();
        }
    }
}
