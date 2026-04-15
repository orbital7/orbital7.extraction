namespace Orbital7.Extraction.Email;

public record EmailMessage :
    EmailMetadata
{
    public EmailBodyContentType? BodyContentType { get; set; }

    public string? Body { get; set; }
}
