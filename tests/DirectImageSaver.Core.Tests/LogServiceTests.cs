using DirectImageSaver.Core;
using DirectImageSaver.Core.Services;
using FluentAssertions;

namespace DirectImageSaver.Core.Tests;

public sealed class LogServiceTests
{
    [Fact]
    public void LogInfo_ShouldWriteToCustomPrefixedLogFile()
    {
        var prefix = $"directimagesaver-test-{Guid.NewGuid():N}-";
        var service = new LogService("Information", prefix);

        try
        {
            service.LogInfo("NativeHost", "Received", "Test log entry.", requestType: "getConfig");
            service.Dispose();

            var logFiles = Directory.GetFiles(AppPaths.LogDirectoryPath, $"{prefix}*.log");

            logFiles.Should().ContainSingle();
            File.ReadAllText(logFiles[0]).Should().Contain("action=NativeHost");
            File.ReadAllText(logFiles[0]).Should().Contain("requestType=getConfig");
            File.ReadAllText(logFiles[0]).Should().Contain("result=Received");
        }
        finally
        {
            service.Dispose();
            foreach (var logFile in Directory.GetFiles(AppPaths.LogDirectoryPath, $"{prefix}*.log"))
            {
                File.Delete(logFile);
            }
        }
    }
}
