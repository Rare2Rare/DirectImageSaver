using DirectImageSaver.Core.Ipc;
using DirectImageSaver.Core.Models;

namespace DirectImageSaver.Core.Services;

public sealed class SaveRequestHandler
{
    private readonly SettingsService _settingsService;
    private readonly FilenameService _filenameService;
    private readonly DownloadService _downloadService;
    private readonly AudioService _audioService;
    private readonly LogService _logService;
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    public SaveRequestHandler(
        SettingsService settingsService,
        FilenameService filenameService,
        DownloadService downloadService,
        AudioService audioService,
        LogService logService)
    {
        _settingsService = settingsService;
        _filenameService = filenameService;
        _downloadService = downloadService;
        _audioService = audioService;
        _logService = logService;
    }

    public async Task<NativeResponse> HandleSaveAsync(HoveredMediaPayload? payload, CancellationToken cancellationToken)
    {
        if (payload is null)
        {
            var settings = _settingsService.GetCurrentSettings();
            _audioService.PlayFailureIfEnabled(settings);
            _logService.LogSaveFailure(null, null, SaveErrorCode.InvalidPayload, "The save payload was empty.", null);
            return NativeResponse.Error(SaveErrorCode.InvalidPayload, "The save payload was empty.");
        }

        _logService.LogInfo("SaveMedia", "Received", "Save request reached save handler.", payload);

        try
        {
            ValidatePayload(payload);
        }
        catch (SaveRequestException exception)
        {
            var settings = _settingsService.GetCurrentSettings();
            _audioService.PlayFailureIfEnabled(settings);
            _logService.LogSaveFailure(payload, null, exception.ErrorCode, exception.Message, exception);
            return NativeResponse.Error(exception.ErrorCode, exception.Message);
        }

        await _saveSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? tempFilePath = null;
        string? finalPath = null;

        try
        {
            var settings = _settingsService.GetCurrentSettings();
            _logService.Reconfigure(settings.LogLevel);

            if (string.IsNullOrWhiteSpace(settings.SaveDirectory))
            {
                throw new SaveRequestException(
                    SaveErrorCode.SaveDirectoryNotConfigured,
                    "The save directory is not configured.");
            }

            if (payload.MediaType == MediaType.Video && !settings.EnableVideoSave)
            {
                throw new SaveRequestException(
                    SaveErrorCode.InvalidPayload,
                    "Video saving is disabled in the current settings.");
            }

            Directory.CreateDirectory(settings.SaveDirectory);
            tempFilePath = Path.Combine(settings.SaveDirectory, $".directimagesaver-{Guid.NewGuid():N}.tmp");
            _logService.LogInfo("SaveMedia", "DownloadStarted", "Downloading media to a temporary file.", payload, tempFilePath);

            var downloadResult = await _downloadService.DownloadAsync(payload, tempFilePath, cancellationToken).ConfigureAwait(false);
            var extension = _filenameService.ResolveExtension(downloadResult.ContentType, payload.MediaUrl);
            var timestamp = ParseTimestamp(payload.Timestamp);
            finalPath = _filenameService.GetUniqueFilePath(
                settings.SaveDirectory,
                payload.Host,
                timestamp,
                extension,
                settings.FilenamePattern);

            File.Move(tempFilePath, finalPath);

            _audioService.PlaySuccessIfEnabled(settings);
            _logService.LogSaveSuccess(payload, finalPath, downloadResult.ContentType);
            return NativeResponse.Success(finalPath, downloadResult.ContentType, Path.GetFileName(finalPath));
        }
        catch (SaveRequestException exception)
        {
            var settings = _settingsService.GetCurrentSettings();
            _audioService.PlayFailureIfEnabled(settings);
            _logService.LogSaveFailure(payload, finalPath, exception.ErrorCode, exception.Message, exception);
            CleanupTempFile(tempFilePath);
            return NativeResponse.Error(exception.ErrorCode, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            var settings = _settingsService.GetCurrentSettings();
            _audioService.PlayFailureIfEnabled(settings);
            _logService.LogSaveFailure(payload, finalPath, SaveErrorCode.WriteAccessDenied, exception.Message, exception);
            CleanupTempFile(tempFilePath);
            return NativeResponse.Error(SaveErrorCode.WriteAccessDenied, "Write permission was denied for the save directory.");
        }
        catch (DirectoryNotFoundException exception)
        {
            var settings = _settingsService.GetCurrentSettings();
            _audioService.PlayFailureIfEnabled(settings);
            _logService.LogSaveFailure(payload, finalPath, SaveErrorCode.SaveDirectoryUnavailable, exception.Message, exception);
            CleanupTempFile(tempFilePath);
            return NativeResponse.Error(SaveErrorCode.SaveDirectoryUnavailable, "The save directory does not exist.");
        }
        catch (IOException exception)
        {
            var settings = _settingsService.GetCurrentSettings();
            _audioService.PlayFailureIfEnabled(settings);
            _logService.LogSaveFailure(payload, finalPath, SaveErrorCode.FileSaveException, exception.Message, exception);
            CleanupTempFile(tempFilePath);
            return NativeResponse.Error(SaveErrorCode.FileSaveException, "The file could not be written to disk.");
        }
        catch (Exception exception)
        {
            var settings = _settingsService.GetCurrentSettings();
            _audioService.PlayFailureIfEnabled(settings);
            _logService.LogSaveFailure(payload, finalPath, SaveErrorCode.UnhandledException, exception.Message, exception);
            CleanupTempFile(tempFilePath);
            return NativeResponse.Error(SaveErrorCode.UnhandledException, "An unexpected error occurred while saving the media.");
        }
        finally
        {
            _saveSemaphore.Release();
        }
    }

    private static void ValidatePayload(HoveredMediaPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.MediaUrl))
        {
            throw new SaveRequestException(SaveErrorCode.ImageUrlMissing, "No media URL was detected.");
        }

        if (!Uri.TryCreate(payload.MediaUrl, UriKind.Absolute, out var imageUri)
            || (imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new SaveRequestException(SaveErrorCode.InvalidPayload, "The media URL is invalid or unsupported.");
        }
    }

    private static DateTimeOffset ParseTimestamp(string? timestamp)
    {
        if (DateTimeOffset.TryParse(timestamp, out var parsed))
        {
            return parsed;
        }

        return DateTimeOffset.Now;
    }

    private static void CleanupTempFile(string? tempFilePath)
    {
        if (string.IsNullOrWhiteSpace(tempFilePath) || !File.Exists(tempFilePath))
        {
            return;
        }

        try
        {
            File.Delete(tempFilePath);
        }
        catch
        {
        }
    }
}
