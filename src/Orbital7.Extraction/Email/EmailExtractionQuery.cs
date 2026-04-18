namespace Orbital7.Extraction.Email;

public record EmailExtractionQuery
{
    public const string ORDERING_RECEIVED_DATE_TIME_ASC = "receivedDateTime ASC";
    public const string ORDERING_RECEIVED_DATE_TIME_DESC = "receivedDateTime DESC";
    public const string FILTER_UNREAD = "isRead eq false";
    public const string FILTER_READ = "isRead eq true";

    public int? Top { get; set; } = 100;

    public int? Maximum { get; set; }

    public string? Filter { get; set; }

    public string[]? Orderby { get; set; } = [ORDERING_RECEIVED_DATE_TIME_DESC];

    public string[]? Select { get; set; }

    public string[]? Expand { get; set; }

    public List<(string, string)>? Headers { get; set; }

    public bool DownloadAttachments { get; set; } = false;

    //public MicrosoftGraphMessagesQueryConfig SetPreferHtmlContent()
    //{
    //    this.Headers ??= new List<(string, string)>();
    //    this.Headers.Add(("Prefer", "outlook.body-content-type=\"html\""));
    //    return this;
    //}

    //public MicrosoftGraphMessagesQueryConfig SetPreferTextContent()
    //{
    //    this.Headers ??= new List<(string, string)>();
    //    this.Headers.Add(("Prefer", "outlook.body-content-type=\"text\""));
    //    return this;
    //}
}
