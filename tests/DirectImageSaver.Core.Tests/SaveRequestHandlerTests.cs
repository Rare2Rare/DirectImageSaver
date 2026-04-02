using DirectImageSaver.Core.Models;
using DirectImageSaver.Core.Services;
using FluentAssertions;

namespace DirectImageSaver.Core.Tests;

public sealed class SaveRequestHandlerTests
{
    [Fact]
    public async Task HandleSaveAsync_ShouldRejectVideoWhenVideoSavingIsDisabled()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();
        var configPath = Path.Combine(tempDirectory.FullName, "config.json");
        var settingsService = new SettingsService(configPath);
        settingsService.Save(new AppSettings
        {
            SaveDirectory = tempDirectory.FullName,
            EnableVideoSave = false
        });

        using var logService = new LogService("Information", $"directimagesaver-test-{Guid.NewGuid():N}-");
        var handler = new SaveRequestHandler(
            settingsService,
            new FilenameService(),
            new DownloadService(),
            new AudioService(),
            logService);

        try
        {
            var response = await handler.HandleSaveAsync(
                new HoveredMediaPayload
                {
                    MediaType = MediaType.Video,
                    MediaUrl = "https://example.com/video.mp4",
                    Host = "example.com",
                    Timestamp = "2026-04-02T00:00:00+09:00"
                },
                CancellationToken.None);

            response.Ok.Should().BeFalse();
            response.ErrorCode.Should().Be(SaveErrorCode.InvalidPayload.ToString());
            response.Message.Should().Contain("Video saving is disabled");
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }
}
