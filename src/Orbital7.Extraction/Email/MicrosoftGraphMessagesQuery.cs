namespace Orbital7.Extraction.Email;

public record MicrosoftGraphMessagesQuery
{
    public int? Top { get; set; } = 100;

    public int? Maximum { get; set; }

    public string? Filter { get; set; }

    public string[]? Orderby { get; set; } = ["receivedDateTime DESC"];

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
