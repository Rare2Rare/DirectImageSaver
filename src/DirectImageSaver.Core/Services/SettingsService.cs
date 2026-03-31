using System.Text.Json;
using DirectImageSaver.Core.Models;

namespace DirectImageSaver.Core.Services;

public sealed class SettingsService
{
    private readonly object _sync = new();
    private readonly string _configPath;
    private AppSettings? _cachedSettings;

    public SettingsService(string? configPath = null)
    {
        _configPath = configPath ?? AppPaths.ConfigFilePath;
    }

    public AppSettings GetCurrentSettings()
    {
        lock (_sync)
        {
            _cachedSettings ??= LoadOrCreateInternal();
            return _cachedSettings.Clone();
        }
    }

    public AppSettings Reload()
    {
        lock (_sync)
        {
            _cachedSettings = LoadOrCreateInternal();
            return _cachedSettings.Clone();
        }
    }

    public AppSettings Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        lock (_sync)
        {
            settings.Normalize();
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
            File.WriteAllText(_configPath, JsonSerializer.Serialize(settings, JsonDefaults.SerializerOptions));
            _cachedSettings = settings.Clone();
            return _cachedSettings.Clone();
        }
    }

    private AppSettings LoadOrCreateInternal()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);

        if (!File.Exists(_configPath))
        {
            var defaults = AppSettings.CreateDefault();
            File.WriteAllText(_configPath, JsonSerializer.Serialize(defaults, JsonDefaults.SerializerOptions));
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonDefaults.SerializerOptions)
                           ?? AppSettings.CreateDefault();
            settings.Normalize();
            return settings;
        }
        catch
        {
            var backupPath = $"{_configPath}.broken-{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(_configPath, backupPath, overwrite: true);
            var defaults = AppSettings.CreateDefault();
            File.WriteAllText(_configPath, JsonSerializer.Serialize(defaults, JsonDefaults.SerializerOptions));
            return defaults;
        }
    }
}
