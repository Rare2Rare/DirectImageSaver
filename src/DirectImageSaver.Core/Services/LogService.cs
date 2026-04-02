using DirectImageSaver.Core.Models;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DirectImageSaver.Core.Services;

public sealed class LogService : IDisposable
{
    public const string AppLogFilePrefix = "directimagesaver-app-";
    public const string NativeHostLogFilePrefix = "directimagesaver-nativehost-";

    private ILogger _logger = Logger.None;
    private Logger? _serilogLogger;
    private readonly string _logFilePrefix;

    public LogService(string initialLogLevel, string logFilePrefix = AppLogFilePrefix)
    {
        _logFilePrefix = string.IsNullOrWhiteSpace(logFilePrefix) ? AppLogFilePrefix : logFilePrefix;
        Directory.CreateDirectory(AppPaths.LogDirectoryPath);
        Reconfigure(initialLogLevel);
    }

    public void Reconfigure(string logLevel)
    {
        _serilogLogger?.Dispose();

        _serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Is(ParseLevel(logLevel))
            .WriteTo.File(
                Path.Combine(AppPaths.LogDirectoryPath, $"{_logFilePrefix}.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] action={Action} requestType={RequestType} pageUrl={PageUrl} mediaType={MediaType} mediaUrl={MediaUrl} savePath={SavePath} contentType={ContentType} result={Result} errorMessage={ErrorMessage}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
            .CreateLogger();

        _logger = _serilogLogger;
    }

    public void LogInfo(
        string action,
        string result,
        string message,
        HoveredMediaPayload? payload = null,
        string? savePath = null,
        string? requestType = null,
        string? contentType = null)
    {
        CreateContext(action, result, payload, savePath, message, requestType, contentType)
            .Information(message);
    }

    public void LogError(
        string action,
        string result,
        string message,
        Exception? exception,
        HoveredMediaPayload? payload = null,
        string? savePath = null,
        string? requestType = null,
        string? contentType = null)
    {
        CreateContext(action, result, payload, savePath, message, requestType, contentType)
            .Error(exception, message);
    }

    public void LogSaveSuccess(HoveredMediaPayload payload, string savePath, string? contentType)
    {
        LogInfo(
            "SaveMedia",
            "Success",
            "Media saved successfully.",
            payload,
            savePath,
            contentType: contentType);
    }

    public void LogSaveFailure(
        HoveredMediaPayload? payload,
        string? savePath,
        SaveErrorCode errorCode,
        string message,
        Exception? exception)
    {
        LogError("SaveMedia", errorCode.ToString(), message, exception, payload, savePath);
    }

    public void Dispose()
    {
        _serilogLogger?.Dispose();
    }

    private static LogEventLevel ParseLevel(string? logLevel) =>
        Enum.TryParse<LogEventLevel>(logLevel, ignoreCase: true, out var parsedLevel)
            ? parsedLevel
            : LogEventLevel.Information;

    private ILogger CreateContext(
        string action,
        string result,
        HoveredMediaPayload? payload,
        string? savePath,
        string? errorMessage,
        string? requestType,
        string? contentType)
    {
        return _logger
            .ForContext("Action", action)
            .ForContext("RequestType", requestType ?? string.Empty)
            .ForContext("PageUrl", payload?.PageUrl ?? string.Empty)
            .ForContext("MediaType", payload?.MediaType.ToString() ?? string.Empty)
            .ForContext("MediaUrl", payload?.MediaUrl ?? string.Empty)
            .ForContext("SavePath", savePath ?? string.Empty)
            .ForContext("ContentType", contentType ?? string.Empty)
            .ForContext("Result", result)
            .ForContext("ErrorMessage", errorMessage ?? string.Empty);
    }
}
