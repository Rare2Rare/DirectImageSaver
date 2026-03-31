namespace DirectImageSaver.Core.Models;

public sealed class ConfigSnapshot
{
    public TriggerMode TriggerMode { get; init; } = TriggerMode.ShiftRightClick;

    public bool SuccessSoundEnabled { get; init; }

    public bool ErrorSoundEnabled { get; init; }

    public IReadOnlyList<string> SupportedBrowsers { get; init; } = Array.Empty<string>();

    public static ConfigSnapshot FromSettings(AppSettings settings) =>
        new()
        {
            TriggerMode = settings.TriggerMode,
            SuccessSoundEnabled = settings.SuccessSoundEnabled,
            ErrorSoundEnabled = settings.ErrorSoundEnabled,
            SupportedBrowsers = settings.SupportedBrowsers.ToArray()
        };
}
