namespace Orbital7.Extraction.Email;

public class MicrosoftGraphMessagesQueryConfig
{
    public int? Top { get; set; } = 100;

    public int? Maximum { get; set; }

    public string? Filter { get; set; }

    public string[]? Orderby { get; set; } = ["receivedDateTime DESC"];

    public string[]? Select { get; set; }

    public string[]? Expand { get; set; }

    public List<(string, string)>? Headers { get; set; }
}
