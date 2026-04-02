using System.Globalization;

namespace DirectImageSaver.Core.Services;

public sealed class FilenameService
{
    private static readonly Dictionary<string, string> ContentTypeToExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/gif"] = ".gif",
        ["image/webp"] = ".webp",
        ["image/avif"] = ".avif",
        ["image/bmp"] = ".bmp",
        ["image/svg+xml"] = ".svg",
        ["video/mp4"] = ".mp4",
        ["video/webm"] = ".webm",
        ["video/ogg"] = ".ogv",
        ["video/quicktime"] = ".mov",
        ["video/x-m4v"] = ".m4v"
    };

    private static readonly HashSet<string> KnownExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp",
        ".avif",
        ".bmp",
        ".svg",
        ".mp4",
        ".webm",
        ".ogv",
        ".mov",
        ".m4v"
    };

    public string NormalizeSite(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return "media";
        }

        var normalized = host.Trim().ToLowerInvariant();
        if (normalized.StartsWith("www.", StringComparison.Ordinal))
        {
            normalized = normalized[4..];
        }

        if (normalized is "x.com" or "twitter.com")
        {
            normalized = "twitter";
        }

        var invalidChars = Path.GetInvalidFileNameChars().Concat(['\\', '/', ':', '*', '?', '"', '<', '>', '|']);
        foreach (var invalidChar in invalidChars.Distinct())
        {
            normalized = normalized.Replace(invalidChar.ToString(), string.Empty, StringComparison.Ordinal);
        }

        normalized = normalized.Replace(".", "_", StringComparison.Ordinal);
        return string.IsNullOrWhiteSpace(normalized) ? "media" : normalized;
    }

    public string ResolveExtension(string? contentType, string? mediaUrl)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            var mediaType = contentType.Split(';', 2)[0].Trim();
            if (ContentTypeToExtension.TryGetValue(mediaType, out var extension))
            {
                return extension;
            }
        }

        if (Uri.TryCreate(mediaUrl, UriKind.Absolute, out var imageUri))
        {
            var extension = Path.GetExtension(imageUri.AbsolutePath);
            if (!string.IsNullOrWhiteSpace(extension))
            {
                if (extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    return ".jpg";
                }

                if (KnownExtensions.Contains(extension))
                {
                    return extension.ToLowerInvariant();
                }
            }
        }

        return ".bin";
    }

    public string GetUniqueFilePath(string saveDirectory, string? host, DateTimeOffset timestamp, string extension, string pattern)
    {
        var site = NormalizeSite(host);
        var safeExtension = extension.StartsWith('.') ? extension : $".{extension}";
        var safePattern = string.IsNullOrWhiteSpace(pattern) ? "{site}_{yyyyMMdd_HHmmss}_{seq}" : pattern;
        var stamp = timestamp.LocalDateTime.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

        for (var sequence = 1; sequence < 1000; sequence++)
        {
            var fileNameWithoutExtension = safePattern
                .Replace("{site}", site, StringComparison.Ordinal)
                .Replace("{yyyyMMdd_HHmmss}", stamp, StringComparison.Ordinal)
                .Replace("{seq}", sequence.ToString("00", CultureInfo.InvariantCulture), StringComparison.Ordinal);

            var candidatePath = Path.Combine(saveDirectory, $"{fileNameWithoutExtension}{safeExtension}");
            if (!File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        throw new IOException("Unable to allocate a unique file name after 999 attempts.");
    }
}
