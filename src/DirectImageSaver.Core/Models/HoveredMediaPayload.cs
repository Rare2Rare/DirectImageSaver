using System.Text.Json.Serialization;

namespace DirectImageSaver.Core.Models;

public sealed class HoveredMediaPayload
{
    public MediaType MediaType { get; set; } = MediaType.Image;

    public string MediaUrl { get; set; } = string.Empty;

    // Compatibility bridge for older extension payloads that still send imageUrl.
    [JsonPropertyName("imageUrl")]
    public string? LegacyImageUrl
    {
        set
        {
            if (string.IsNullOrWhiteSpace(MediaUrl) && !string.IsNullOrWhiteSpace(value))
            {
                MediaUrl = value;
            }
        }
    }

    public string PageUrl { get; set; } = string.Empty;

    public string PageTitle { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public string? Alt { get; set; }

    public int? NaturalWidth { get; set; }

    public int? NaturalHeight { get; set; }

    public double? DurationSeconds { get; set; }

    public int? VideoWidth { get; set; }

    public int? VideoHeight { get; set; }

    public string UserAgent { get; set; } = string.Empty;

    public string? Referrer { get; set; }

    public string Timestamp { get; set; } = string.Empty;
}
