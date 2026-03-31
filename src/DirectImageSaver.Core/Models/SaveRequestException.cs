namespace DirectImageSaver.Core.Models;

public sealed class SaveRequestException : Exception
{
    public SaveRequestException(SaveErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public SaveRequestException(SaveErrorCode errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public SaveErrorCode ErrorCode { get; }
}
