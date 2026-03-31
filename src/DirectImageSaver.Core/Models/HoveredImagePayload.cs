namespace DirectImageSaver.Core.Models;

public sealed class HoveredImagePayload
{
    public string ImageUrl { get; set; } = string.Empty;

    public string PageUrl { get; set; } = string.Empty;

    public string PageTitle { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public string? Alt { get; set; }

    public int? NaturalWidth { get; set; }

    public int? NaturalHeight { get; set; }

    public string UserAgent { get; set; } = string.Empty;

    public string? Referrer { get; set; }

    public string Timestamp { get; set; } = string.Empty;
}
