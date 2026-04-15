using Microsoft.Graph.Models;

namespace Orbital7.Extraction.Email;

public static class Extensions
{
    public static TEmailMetadata ToEmailMetadata<TEmailMetadata>(
        this Message msg) 
        where TEmailMetadata : EmailMetadata, new()
    {
        return new TEmailMetadata()
        {
            Id = msg.Id,
            SentDateTimeUtc = msg.SentDateTime?.UtcDateTime,
            SenderEmail = msg.Sender?.EmailAddress?.Address ?? msg.From?.EmailAddress?.Address,
            SenderName = msg.Sender?.EmailAddress?.Name ?? msg.From?.EmailAddress?.Name,
            Subject = msg.Subject,
        };
    }
}
