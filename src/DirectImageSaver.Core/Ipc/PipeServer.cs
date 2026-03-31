using System.IO.Pipes;
using System.Text.Json;

namespace DirectImageSaver.Core.Ipc;

public sealed class PipeServer : IAsyncDisposable
{
    private readonly string _pipeName;
    private readonly Func<NativeRequest, CancellationToken, Task<NativeResponse>> _handler;
    private readonly Action<string, Exception?>? _errorLogger;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _listenTask;

    public PipeServer(
        string pipeName,
        Func<NativeRequest, CancellationToken, Task<NativeResponse>> handler,
        Action<string, Exception?>? errorLogger = null)
    {
        _pipeName = pipeName;
        _handler = handler;
        _errorLogger = errorLogger;
    }

    public void Start()
    {
        if (_listenTask is not null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _listenTask = Task.Run(() => ListenLoopAsync(_cancellationTokenSource.Token));
    }

    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource is null)
        {
            return;
        }

        await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);

        if (_listenTask is not null)
        {
            try
            {
                await _listenTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        _cancellationTokenSource.Dispose();
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var server = new NamedPipeServerStream(
                _pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                await HandleClientAsync(server, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task HandleClientAsync(Stream stream, CancellationToken cancellationToken)
    {
        var requestJson = await LengthPrefixedJsonStream.ReadMessageAsync(stream, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(requestJson))
        {
            return;
        }

        NativeResponse response;
        try
        {
            var request = JsonSerializer.Deserialize<NativeRequest>(requestJson, JsonDefaults.SerializerOptions)
                          ?? throw new InvalidDataException("Request payload was empty.");

            response = await _handler(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _errorLogger?.Invoke("Pipe server failed to handle a native request.", exception);
            response = NativeResponse.Error("UnhandledPipeError", exception.Message);
        }

        var responseJson = JsonSerializer.Serialize(response, JsonDefaults.SerializerOptions);
        await LengthPrefixedJsonStream.WriteMessageAsync(stream, responseJson, cancellationToken).ConfigureAwait(false);
    }
}
