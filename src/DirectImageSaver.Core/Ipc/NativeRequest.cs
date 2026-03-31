using DirectImageSaver.Core.Models;

namespace DirectImageSaver.Core.Ipc;

public sealed class NativeRequest
{
    public string Type { get; set; } = string.Empty;

    public HoveredImagePayload? Payload { get; set; }
}
