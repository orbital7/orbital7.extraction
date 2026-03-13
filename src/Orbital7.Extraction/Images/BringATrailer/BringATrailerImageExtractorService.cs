using System.Text.RegularExpressions;

namespace Orbital7.Extraction.Images.BringATrailer;

public class BringATrailerImageExtractorService(
    IHttpClientFactory httpClientFactory) :
    IBringATrailerImageExtractorService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    // TODO: Replace "auctionPageHtml" with the auction URL
    // and obtain the page content dynamically; this will require
    // some dynamic web scraping logic as the page content is not
    // loaded statically.
    public async Task DownloadAuctionPhotosAsync(
        string auctionPageHtml,
        string downloadFolderPath)
    {
        using (var client = _httpClientFactory.CreateClient())
        {
            HashSet<string> imageUrls = ExtractAuctionPhotoUrls(auctionPageHtml);

            int i = 0;
            foreach (string imageUrl in imageUrls)
            {
                i++;
                string fileExtension = Path.GetExtension(imageUrl).Split('?')[0]; // Remove query parameters
                string fileName = Path.Combine(downloadFolderPath, $"image_{i:D3}{fileExtension}");

                Console.WriteLine($"Downloading {i}/{imageUrls.Count}: {fileName}");

                byte[] imageData = await client.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(fileName, imageData);
            }
        }
    }

    private HashSet<string> ExtractAuctionPhotoUrls(
        string auctionPageHtml)
    {
        HashSet<string> imageUrls = new HashSet<string>();

        // Regex pattern to find image URLs in the page content.
        string pattern = @"https://bringatrailer\.com/wp-content/uploads/[^""'\s]+?\.(jpg|jpeg|png|webp)";

        MatchCollection matches = Regex.Matches(auctionPageHtml, pattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            string url = match.Value;

            // Remove '-scaled' from filenames to get high-resolution images
            url = Regex.Replace(url, @"-scaled(?=\.(jpg|jpeg|png|webp))", "", RegexOptions.IgnoreCase);

            imageUrls.Add(url);
        }

        return imageUrls;
    }
}
