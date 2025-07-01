using System.Text.Json.Serialization;

namespace Orbital7.Extraction.Email;

public record ErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    [JsonPropertyName("error_codes")]
    public List<int> ErrorCodes { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public string? TimeStamp { get; set; }

    [JsonPropertyName("trace_id")]
    public string? TraceId { get; set; }

    [JsonPropertyName("correlation_id")]
    public string? CorrelationId { get; set; }

    [JsonPropertyName("error_uri")]
    public string? ErrorUri { get; set; }
}
