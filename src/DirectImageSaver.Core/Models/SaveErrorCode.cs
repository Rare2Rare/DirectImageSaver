namespace DirectImageSaver.Core.Models;

public enum SaveErrorCode
{
    UnsupportedRequest,
    InvalidPayload,
    ImageUrlMissing,
    SaveDirectoryNotConfigured,
    SaveDirectoryUnavailable,
    WriteAccessDenied,
    DownloadFailed,
    NetworkUnavailable,
    AntiHotlinkOrUnauthorized,
    FileSaveException,
    NativeHostUnavailable,
    IpcUnavailable,
    UnhandledException
}
