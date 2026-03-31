using DirectImageSaver.Core.Services;
using FluentAssertions;

namespace DirectImageSaver.Core.Tests;

public sealed class FilenameServiceTests
{
    private readonly FilenameService _service = new();

    [Fact]
    public void NormalizeSite_ShouldCanonicalizeTwitterHosts()
    {
        _service.NormalizeSite("x.com").Should().Be("twitter");
        _service.NormalizeSite("twitter.com").Should().Be("twitter");
    }

    [Fact]
    public void NormalizeSite_ShouldRemoveWwwAndInvalidCharacters()
    {
        _service.NormalizeSite("www.exa:mple.com").Should().Be("example_com");
    }

    [Fact]
    public void ResolveExtension_ShouldPreferContentType()
    {
        _service.ResolveExtension("image/webp", "https://example.com/image.jpg").Should().Be(".webp");
    }

    [Fact]
    public void ResolveExtension_ShouldFallbackToUrlExtension()
    {
        _service.ResolveExtension(null, "https://example.com/photo.jpeg?size=large").Should().Be(".jpg");
    }

    [Fact]
    public void ResolveExtension_ShouldFallbackToBinForUnknownValues()
    {
        _service.ResolveExtension("application/octet-stream", "https://example.com/image").Should().Be(".bin");
    }

    [Fact]
    public void GetUniqueFilePath_ShouldIncrementSequenceForCollisions()
    {
        var directory = Directory.CreateTempSubdirectory().FullName;

        try
        {
            var timestamp = new DateTimeOffset(2026, 3, 31, 21, 30, 55, TimeSpan.FromHours(9));
            var first = _service.GetUniqueFilePath(directory, "x.com", timestamp, ".jpg", "{site}_{yyyyMMdd_HHmmss}_{seq}");
            File.WriteAllText(first, "occupied");

            var second = _service.GetUniqueFilePath(directory, "x.com", timestamp, ".jpg", "{site}_{yyyyMMdd_HHmmss}_{seq}");
            Path.GetFileName(first).Should().Be("twitter_20260331_213055_01.jpg");
            Path.GetFileName(second).Should().Be("twitter_20260331_213055_02.jpg");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
