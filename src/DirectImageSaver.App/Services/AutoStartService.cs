using Microsoft.Win32;

namespace DirectImageSaver.App.Services;

public sealed class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "DirectImageSaver";

    public void SetEnabled(bool enabled, string applicationPath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        if (key is null)
        {
            throw new InvalidOperationException("Unable to open the Run registry key.");
        }

        if (enabled)
        {
            key.SetValue(ValueName, $"\"{applicationPath}\" --background");
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}
