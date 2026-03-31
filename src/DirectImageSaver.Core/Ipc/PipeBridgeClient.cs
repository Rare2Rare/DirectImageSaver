using System.IO.Pipes;
using System.Text.Json;

namespace DirectImageSaver.Core.Ipc;

public sealed class PipeBridgeClient
{
    private readonly string _pipeName;

    public PipeBridgeClient(string pipeName)
    {
        _pipeName = pipeName;
    }

    public async Task<NativeResponse> SendAsync(
        NativeRequest request,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCancellation.CancelAfter(timeout);

        await using var pipe = new NamedPipeClientStream(
            ".",
            _pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        await pipe.ConnectAsync((int)timeout.TotalMilliseconds, linkedCancellation.Token).ConfigureAwait(false);

        var requestJson = JsonSerializer.Serialize(request, JsonDefaults.SerializerOptions);
        await LengthPrefixedJsonStream.WriteMessageAsync(pipe, requestJson, linkedCancellation.Token).ConfigureAwait(false);

        var responseJson = await LengthPrefixedJsonStream.ReadMessageAsync(pipe, linkedCancellation.Token).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            throw new EndOfStreamException("Pipe response was empty.");
        }

        return JsonSerializer.Deserialize<NativeResponse>(responseJson, JsonDefaults.SerializerOptions)
               ?? throw new InvalidDataException("Pipe response could not be deserialized.");
    }
}
