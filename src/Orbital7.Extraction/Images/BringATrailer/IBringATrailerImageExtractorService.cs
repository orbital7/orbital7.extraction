namespace Orbital7.Extraction.Images.BringATrailer;

public interface IBringATrailerImageExtractorService
{
    Task DownloadAuctionPhotosAsync(
        string auctionPageHtml,
        string downloadFolderPath);
}
