namespace DirectImageSaver.Core.Models;

public sealed class AppSettings
{
    public string SaveDirectory { get; set; } = AppPaths.DefaultSaveDirectory;

    public TriggerMode TriggerMode { get; set; } = TriggerMode.ShiftRightClick;

    public bool SuccessSoundEnabled { get; set; } = true;

    public bool ErrorSoundEnabled { get; set; } = true;

    public bool AutoStart { get; set; } = true;

    public bool EnableVideoSave { get; set; } = true;

    public string FilenamePattern { get; set; } = "{site}_{yyyyMMdd_HHmmss}_{seq}";

    public List<string> SupportedBrowsers { get; set; } = ["chrome", "edge"];

    public string LogLevel { get; set; } = "Information";

    public static AppSettings CreateDefault() => new();

    public AppSettings Clone() =>
        new()
        {
            SaveDirectory = SaveDirectory,
            TriggerMode = TriggerMode,
            SuccessSoundEnabled = SuccessSoundEnabled,
            ErrorSoundEnabled = ErrorSoundEnabled,
            AutoStart = AutoStart,
            EnableVideoSave = EnableVideoSave,
            FilenamePattern = FilenamePattern,
            SupportedBrowsers = new List<string>(SupportedBrowsers),
            LogLevel = LogLevel
        };

    public void Normalize()
    {
        SaveDirectory ??= AppPaths.DefaultSaveDirectory;
        FilenamePattern = string.IsNullOrWhiteSpace(FilenamePattern)
            ? "{site}_{yyyyMMdd_HHmmss}_{seq}"
            : FilenamePattern;
        LogLevel = string.IsNullOrWhiteSpace(LogLevel) ? "Information" : LogLevel;

        if (SupportedBrowsers is null || SupportedBrowsers.Count == 0)
        {
            SupportedBrowsers = ["chrome", "edge"];
        }
    }
}
