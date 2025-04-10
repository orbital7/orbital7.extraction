
namespace Orbital7.Extraction.Email;

public class MessageContent
{
    public string? Id { get; set; }

    public DateTime? SentDateTimeUtc { get; set; }

    public string? SenderEmail { get; set; }

    public string? SenderName { get; set; }

    public string? Subject { get; set; }

    public EmailBodyContentType? BodyContentType { get; set; }

    public string? Body { get; set; }
}
