namespace Orbital7.Extraction.Images;

public interface IBringATrailerImageExtractorService
{
    Task DownloadAuctionPhotosAsync(
        string auctionPageHtml,
        string downloadFolderPath);
}
