using Orbital7.Extensions.Graphics;
using Orbital7.Extraction.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace Orbital7.Extraction.Images;

public class ImagePdfContentWriter :
    IPdfContentWriter<Image>
{
    // NOTE: Not quite sure why these are the max dimensions, but anything
    // more than this (only MAX_IMAGE_WIDTH has been observed so far) causes
    // the image to be drawn off the PDF page.
    private const int MAX_IMAGE_WIDTH = 610;
    private const int MAX_IMAGE_HEIGHT = 920;

    public string GetContentTitle(
        Image contentItem)
    {
        // TODO: Is there something in the image metadata
        // perhaps that we can use for the title?
        return "Image";
    }

    public Task<Stream> WriteContentAsync(
        Image image)
    {
        using (var pdfDocument = new PdfDocument())
        {
            // Resize the image to fit on the page
            // if necessary.
            image.Mutate(
                x => x.EnsureMaximumSize(
                    MAX_IMAGE_WIDTH, 
                    MAX_IMAGE_HEIGHT));

            // Add the image to the PDF document.
            using (var pdfImageStream = new MemoryStream())
            {
                // Save the image as a PNG stream.
                image.Save(pdfImageStream, new PngEncoder());

                // Add a page and draw the image on it.
                var pdfPage = pdfDocument.Pages.Add();
                var pdfImage = PdfImage.FromStream(pdfImageStream);
                pdfPage.Graphics.DrawImage(pdfImage, 0, 0);
            }
          
            // Save the document into a stream and close.
            var pdfStream = new MemoryStream();
            pdfDocument.Save(pdfStream);
            pdfDocument.Close(true);

            return Task.FromResult((Stream)pdfStream);
        }
    }
}
