using System.Text.Json.Serialization;

namespace Winzer.Library.Loop;

// Root myDeserializedClass = JsonSerializer.Deserialize<ReturnsMetadata>(myJsonResponse);
public class ReturnsMetadata
{
    [JsonPropertyName("returns")]
    public List<string> Returns { get; set; } = new List<string>();

    [JsonPropertyName("refunds")]
    public List<string> Refunds { get; set; } = new List<string>();
}

