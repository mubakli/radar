namespace Radar.Api.Features.Ingestion;

public sealed class FetchOptions
{
    public int TimeoutSeconds { get; set; } = 15;
    public int MaxResponseBytes { get; set; } = 2_000_000;
    public int MaxRedirects { get; set; } = 3;
}

public interface IFeedFetcher { Task<string> FetchAsync(Uri uri, CancellationToken cancellationToken); }

public sealed class HttpFeedFetcher(HttpClient client, Microsoft.Extensions.Options.IOptions<FetchOptions> options) : IFeedFetcher
{
    public async Task<string> FetchAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (uri.Scheme is not ("http" or "https")) throw new FeedFetchException("unsupported-url", "Only HTTP and HTTPS Sources are supported.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.TimeoutSeconds));
        try
        {
            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode) throw new FeedFetchException("http", $"Feed returned HTTP {(int)response.StatusCode}.");
            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var reader = new StreamReader(new LimitedReadStream(stream, options.Value.MaxResponseBytes));
            return await reader.ReadToEndAsync(timeout.Token);
        }
        catch (FeedFetchException) { throw; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { throw new FeedFetchException("timeout", "Feed request timed out."); }
        catch (Exception ex) { throw new FeedFetchException("connection", "Feed request failed.", ex); }
    }
}

public sealed class FeedFetchException(string category, string message, Exception? inner = null) : Exception(message, inner) { public string Category { get; } = category; }

internal sealed class LimitedReadStream(Stream inner, int maxBytes) : Stream
{
    private int total;
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) { var read = await inner.ReadAsync(buffer, cancellationToken); total += read; if (total > maxBytes) throw new FeedFetchException("response-limit", "Feed response exceeded the configured size limit."); return read; }
    public override int Read(byte[] buffer, int offset, int count) { var read = inner.Read(buffer, offset, count); total += read; if (total > maxBytes) throw new FeedFetchException("response-limit", "Feed response exceeded the configured size limit."); return read; }
    public override bool CanRead => inner.CanRead; public override bool CanSeek => false; public override bool CanWrite => false; public override long Length => inner.Length; public override long Position { get => inner.Position; set => throw new NotSupportedException(); }
    public override void Flush() => inner.Flush(); public override long Seek(long o, SeekOrigin w) => throw new NotSupportedException(); public override void SetLength(long v) => throw new NotSupportedException(); public override void Write(byte[] b, int o, int c) => throw new NotSupportedException();
}
