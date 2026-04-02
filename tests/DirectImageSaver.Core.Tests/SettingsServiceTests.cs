using DirectImageSaver.Core.Models;
using DirectImageSaver.Core.Ipc;
using DirectImageSaver.Core.Services;
using FluentAssertions;
using System.Text.Json;

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
                EnableVideoSave = true,
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
            reloaded.EnableVideoSave.Should().BeTrue();
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

    [Fact]
    public void SaveAndReload_ShouldPersistVideoSetting()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();
        var configPath = Path.Combine(tempDirectory.FullName, "config.json");
        var service = new SettingsService(configPath);

        try
        {
            var settings = new AppSettings
            {
                EnableVideoSave = false
            };

            service.Save(settings);
            var reloaded = service.Reload();

            reloaded.EnableVideoSave.Should().BeFalse();
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    [Fact]
    public void NativeRequest_ShouldAcceptLegacyImageUrlPayload()
    {
        const string json = """
            {
              "type": "saveImage",
              "payload": {
                "imageUrl": "https://example.com/photo.jpg",
                "pageUrl": "https://example.com/post",
                "host": "example.com",
                "timestamp": "2026-03-31T23:15:00+09:00"
              }
            }
            """;

        var request = JsonSerializer.Deserialize<NativeRequest>(json, JsonDefaults.SerializerOptions);

        request.Should().NotBeNull();
        request!.Payload.Should().NotBeNull();
        request.Payload!.MediaUrl.Should().Be("https://example.com/photo.jpg");
        request.Payload.MediaType.Should().Be(MediaType.Image);
    }

    [Fact]
    public void NativeRequest_ShouldAcceptMediaUrlPayload()
    {
        const string json = """
            {
              "type": "saveMedia",
              "payload": {
                "mediaType": "Image",
                "mediaUrl": "https://example.com/photo.jpg",
                "pageUrl": "https://example.com/post",
                "host": "example.com",
                "timestamp": "2026-03-31T23:15:00+09:00"
              }
            }
            """;

        var request = JsonSerializer.Deserialize<NativeRequest>(json, JsonDefaults.SerializerOptions);

        request.Should().NotBeNull();
        request!.Payload.Should().NotBeNull();
        request.Payload!.MediaUrl.Should().Be("https://example.com/photo.jpg");
        request.Payload.MediaType.Should().Be(MediaType.Image);
    }
}
