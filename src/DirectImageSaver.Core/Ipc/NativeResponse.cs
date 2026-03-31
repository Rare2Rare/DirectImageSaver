using DirectImageSaver.Core.Models;

namespace DirectImageSaver.Core.Ipc;

public sealed class NativeResponse
{
    public bool Ok { get; init; }

    public string? SavedPath { get; init; }

    public string? ContentType { get; init; }

    public string? FileName { get; init; }

    public string? ErrorCode { get; init; }

    public string? Message { get; init; }

    public ConfigSnapshot? Config { get; init; }

    public static NativeResponse Success(string savedPath, string? contentType, string fileName) =>
        new()
        {
            Ok = true,
            SavedPath = savedPath,
            ContentType = contentType,
            FileName = fileName
        };

    public static NativeResponse ConfigResult(ConfigSnapshot snapshot) =>
        new()
        {
            Ok = true,
            Config = snapshot
        };

    public static NativeResponse Error(SaveErrorCode errorCode, string message) =>
        new()
        {
            Ok = false,
            ErrorCode = errorCode.ToString(),
            Message = message
        };

    public static NativeResponse Error(string errorCode, string message) =>
        new()
        {
            Ok = false,
            ErrorCode = errorCode,
            Message = message
        };
}
