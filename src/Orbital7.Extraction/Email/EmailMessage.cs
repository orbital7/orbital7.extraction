namespace Orbital7.Extraction.Email;

public record EmailMessage
{
    public string? Id { get; init; }

    public DateTime? SentDateTimeUtc { get; init; }

    public string? SenderEmail { get; init; }

    public string? SenderName { get; init; }

    public string? Subject { get; init; }

    public EmailBodyContentType? BodyContentType { get; init; }

    public string? Body { get; set; }
}
