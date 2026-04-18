using Orbital7.Extraction.Csv;

namespace Orbital7.Extraction.Email;

public class EmailMetadataCsvContentWriter :
    ICsvContentWriter<EmailMetadata>
{
    public string?[] GetCsvColumnNames()
    {
        return
        [
            "SentDateTime", 
            "Subject", 
            "SenderName", 
            "SenderEmail" 
        ];
    }

    public string?[] GetCsvContentItemValues(
        EmailMetadata email)
    {
        return
        [
            email.SentDateTimeUtc?.ToLocalTime().ToDefaultDateTimeString(),
            email.Subject.EncloseInQuotes(),
            email.SenderName.EncloseInQuotes(),
            email.SenderEmail
        ];
    }
}
