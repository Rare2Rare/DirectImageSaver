using DirectImageSaver.Core.Models;
using DirectImageSaver.Core.Services;
using FluentAssertions;

namespace DirectImageSaver.Core.Tests;

public sealed class SettingsServiceTests
{
    [Fact]
    public void SaveAndReload_ShouldRoundTripSettings()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();
        var configPath = Path.Combine(tempDirectory.FullName, "config.json");
        var service = new SettingsService(configPath);

        try
        {
            var settings = new AppSettings
            {
                SaveDirectory = @"C:\Temp\SavedImages",
                TriggerMode = TriggerMode.AltRightClick,
                SuccessSoundEnabled = false,
                ErrorSoundEnabled = true,
                AutoStart = false,
                FilenamePattern = "{site}_{yyyyMMdd_HHmmss}_{seq}",
                SupportedBrowsers = ["chrome"],
                LogLevel = "Debug"
            };

            service.Save(settings);
            var reloaded = service.Reload();

            reloaded.SaveDirectory.Should().Be(@"C:\Temp\SavedImages");
            reloaded.TriggerMode.Should().Be(TriggerMode.AltRightClick);
            reloaded.SuccessSoundEnabled.Should().BeFalse();
            reloaded.AutoStart.Should().BeFalse();
            reloaded.LogLevel.Should().Be("Debug");
            reloaded.SupportedBrowsers.Should().ContainSingle().Which.Should().Be("chrome");
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    [Fact]
    public void SaveAndReload_ShouldSupportCtrlRightClickTrigger()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();
        var configPath = Path.Combine(tempDirectory.FullName, "config.json");
        var service = new SettingsService(configPath);

        try
        {
            var settings = new AppSettings
            {
                TriggerMode = TriggerMode.CtrlRightClick
            };

            service.Save(settings);
            var reloaded = service.Reload();

            reloaded.TriggerMode.Should().Be(TriggerMode.CtrlRightClick);
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }
}
