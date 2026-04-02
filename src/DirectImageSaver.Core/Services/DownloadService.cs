using System.Net;
using DirectImageSaver.Core.Models;

namespace DirectImageSaver.Core.Services;

public sealed class DownloadService
{
    private readonly HttpClient _httpClient;

    public DownloadService()
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<DownloadResult> DownloadAsync(
        HoveredMediaPayload payload,
        string tempFilePath,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, payload.MediaUrl);
            request.Headers.TryAddWithoutValidation(
                "Accept",
                GetAcceptHeader(payload.MediaType));

            if (!string.IsNullOrWhiteSpace(payload.UserAgent))
            {
                request.Headers.TryAddWithoutValidation("User-Agent", payload.UserAgent);
            }

            if (Uri.TryCreate(payload.Referrer, UriKind.Absolute, out var referrerUri)
                && (referrerUri.Scheme == Uri.UriSchemeHttp || referrerUri.Scheme == Uri.UriSchemeHttps))
            {
                request.Headers.Referrer = referrerUri;
            }

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorCode = response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized
                    ? SaveErrorCode.AntiHotlinkOrUnauthorized
                    : SaveErrorCode.DownloadFailed;

                throw new SaveRequestException(
                    errorCode,
                    $"Download failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var fileStream = new FileStream(
                tempFilePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            await responseStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

            return new DownloadResult
            {
                ContentType = response.Content.Headers.ContentType?.MediaType
            };
        }
        catch (SaveRequestException)
        {
            throw;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new SaveRequestException(SaveErrorCode.NetworkUnavailable, "The download request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new SaveRequestException(SaveErrorCode.DownloadFailed, "The media download failed.", exception);
        }
    }

    private static string GetAcceptHeader(MediaType mediaType) =>
        mediaType == MediaType.Video
            ? "video/webm,video/mp4,video/ogg,video/*,*/*;q=0.8"
            : "image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8";
}
