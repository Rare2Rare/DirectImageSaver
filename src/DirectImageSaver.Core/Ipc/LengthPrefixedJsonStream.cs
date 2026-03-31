using System.Buffers.Binary;
using System.Text;

namespace DirectImageSaver.Core.Ipc;

public static class LengthPrefixedJsonStream
{
    private const int MaxMessageSize = 4 * 1024 * 1024;

    public static async Task<string?> ReadMessageAsync(Stream stream, CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[sizeof(int)];
        var headerBytes = await FillBufferAsync(stream, lengthBuffer, cancellationToken).ConfigureAwait(false);
        if (headerBytes == 0)
        {
            return null;
        }

        if (headerBytes != sizeof(int))
        {
            throw new EndOfStreamException("Message header was truncated.");
        }

        var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
        if (length < 0 || length > MaxMessageSize)
        {
            throw new InvalidDataException($"Message length {length} is invalid.");
        }

        var payloadBuffer = new byte[length];
        await stream.ReadExactlyAsync(payloadBuffer, cancellationToken).ConfigureAwait(false);
        return Encoding.UTF8.GetString(payloadBuffer);
    }

    public static async Task WriteMessageAsync(Stream stream, string payload, CancellationToken cancellationToken)
    {
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var lengthBuffer = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, payloadBytes.Length);

        await stream.WriteAsync(lengthBuffer, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(payloadBytes, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> FillBufferAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead), cancellationToken)
                .ConfigureAwait(false);

            if (read == 0)
            {
                return totalRead;
            }

            totalRead += read;
        }

        return totalRead;
    }
}
