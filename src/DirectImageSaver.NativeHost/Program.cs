using System.Diagnostics;
using System.Text.Json;
using DirectImageSaver.Core;
using DirectImageSaver.Core.Ipc;
using DirectImageSaver.Core.Models;
using DirectImageSaver.Core.Services;

var pipeClient = new PipeBridgeClient(NativeHostConstants.PipeName);
var settingsService = new SettingsService();
using var nativeHostLog = new LogService(settingsService.GetCurrentSettings().LogLevel, LogService.NativeHostLogFilePrefix);
await using var stdin = Console.OpenStandardInput();
await using var stdout = Console.OpenStandardOutput();

while (true)
{
    settingsService.Reload();
    nativeHostLog.Reconfigure(settingsService.GetCurrentSettings().LogLevel);

    string? requestJson;
    try
    {
        requestJson = await LengthPrefixedJsonStream.ReadMessageAsync(stdin, CancellationToken.None).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
        nativeHostLog.LogError("NativeHost", "ReadMessageFailed", exception.Message, exception);
        break;
    }

    if (requestJson is null)
    {
        break;
    }

    nativeHostLog.LogInfo("NativeHost", "ReceivedRaw", $"Native host received {requestJson.Length} bytes.");

    NativeResponse response;
    string? requestType = null;
    try
    {
        var request = JsonSerializer.Deserialize<NativeRequest>(requestJson, JsonDefaults.SerializerOptions)
                      ?? throw new InvalidDataException("Native request payload was empty.");
        requestType = request.Type;
        nativeHostLog.LogInfo("NativeHost", "Received", $"Native host request '{request.Type}' received.", requestType: request.Type);
        response = await SendThroughPipeAsync(pipeClient, request, CancellationToken.None, nativeHostLog).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
        nativeHostLog.LogError("NativeHost", "NativeHostError", exception.Message, exception, requestType: requestType);
        response = NativeResponse.Error("NativeHostError", exception.Message);
    }

    nativeHostLog.LogInfo(
        "NativeHost",
        response.Ok ? "Success" : response.ErrorCode ?? "Failure",
        response.Ok ? "Native host request completed." : response.Message ?? "Native host request failed.",
        requestType: requestType);

    var responseJson = JsonSerializer.Serialize(response, JsonDefaults.SerializerOptions);
    await LengthPrefixedJsonStream.WriteMessageAsync(stdout, responseJson, CancellationToken.None).ConfigureAwait(false);
}

static async Task<NativeResponse> SendThroughPipeAsync(
    PipeBridgeClient pipeClient,
    NativeRequest request,
    CancellationToken cancellationToken,
    LogService? logService)
{
    try
    {
        return await pipeClient.SendAsync(request, TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
    }
    catch (Exception firstException)
    {
        logService?.LogError(
            "NativeHost",
            "PipeConnectFailed",
            "The native host could not reach the tray application on the first attempt.",
            firstException,
            requestType: request.Type);

        if (!TryLaunchTrayApplication())
        {
            logService?.LogError(
                "NativeHost",
                SaveErrorCode.NativeHostUnavailable.ToString(),
                "DirectImageSaver tray application is not running and could not be launched.",
                null,
                requestType: request.Type);
            return NativeResponse.Error(
                SaveErrorCode.NativeHostUnavailable,
                "DirectImageSaver tray application is not running and could not be launched.");
        }

        logService?.LogInfo(
            "NativeHost",
            "LaunchAttempted",
            "The native host launched the tray application after the initial pipe failure.",
            requestType: request.Type);

        try
        {
            await Task.Delay(1500, cancellationToken).ConfigureAwait(false);
            return await pipeClient.SendAsync(request, TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logService?.LogError(
                "NativeHost",
                SaveErrorCode.IpcUnavailable.ToString(),
                $"DirectImageSaver tray application did not respond: {exception.Message}",
                exception,
                requestType: request.Type);
            return NativeResponse.Error(
                SaveErrorCode.IpcUnavailable,
                $"DirectImageSaver tray application did not respond: {exception.Message}");
        }
    }
}

static bool TryLaunchTrayApplication()
{
    var applicationPath = ResolveTrayApplicationPath();
    if (applicationPath is null)
    {
        return false;
    }

    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = applicationPath,
            Arguments = "--background",
            UseShellExecute = true
        });
        return true;
    }
    catch
    {
        return false;
    }
}

static string? ResolveTrayApplicationPath()
{
    var baseDirectory = AppContext.BaseDirectory;
    var candidates = new[]
    {
        Path.GetFullPath(Path.Combine(baseDirectory, "..", "app", "DirectImageSaver.App.exe")),
        Path.GetFullPath(Path.Combine(baseDirectory, "..", "DirectImageSaver.App.exe")),
        Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "DirectImageSaver.App", "bin", "Debug", "net8.0-windows", "DirectImageSaver.App.exe")),
        Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "DirectImageSaver.App", "bin", "Release", "net8.0-windows", "DirectImageSaver.App.exe"))
    };

    return candidates.FirstOrDefault(File.Exists);
}
