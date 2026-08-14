using Microsoft.EntityFrameworkCore;
using Radar.Api.Data;

namespace Radar.Api.Features.Ingestion;

public sealed record IngestionResult(bool Succeeded, int EntryCount, int InsertedCount, int SkippedCount, string? FailureCategory, string? Message, DateTimeOffset AttemptedAt);

public sealed class FeedIngestionService(RadarDbContext db, IFeedFetcher fetcher)
{
    public async Task<IngestionResult> FetchAsync(Guid sourceId, CancellationToken cancellationToken)
    {
        var source = await db.Sources.SingleOrDefaultAsync(x => x.Id == sourceId, cancellationToken);
        if (source is null) throw new KeyNotFoundException();
        var attemptedAt = DateTimeOffset.UtcNow;
        if (!source.Enabled) return await Record(source, new IngestionResult(false, 0, 0, 0, "disabled", "Source is disabled.", attemptedAt), cancellationToken);
        if (!Uri.TryCreate(source.Locator, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            return await Record(source, new IngestionResult(false, 0, 0, 0, "unsupported-url", "Only HTTP and HTTPS Sources can be fetched.", attemptedAt), cancellationToken);
        try
        {
            var raw = await fetcher.FetchAsync(uri, cancellationToken);
            var parsed = FeedParser.Parse(raw, uri);
            var inserted = 0;
            foreach (var entry in parsed.Entries.GroupBy(x => x.CanonicalLocator, StringComparer.Ordinal).Select(x => x.First()))
            {
                if (await db.SourceItems.AnyAsync(x => x.SourceId == sourceId && x.CanonicalLocator == entry.CanonicalLocator, cancellationToken)) continue;
                db.SourceItems.Add(new SourceItem { SourceId = sourceId, CanonicalLocator = entry.CanonicalLocator, Url = entry.Url, Title = entry.Title!, PublishedAt = entry.PublishedAt, Author = entry.Author, Summary = entry.Summary, RawContent = entry.RawContent, ObservedAt = attemptedAt });
                try { await db.SaveChangesAsync(cancellationToken); inserted++; }
                catch (DbUpdateException) { db.ChangeTracker.Clear(); }
            }
            return await Record(source, new IngestionResult(true, parsed.Entries.Count, inserted, parsed.SkippedCount, null, null, attemptedAt), cancellationToken);
        }
        catch (FeedFetchException ex) { return await Record(source, new IngestionResult(false, 0, 0, 0, ex.Category, ex.Message, attemptedAt), cancellationToken); }
        catch (FeedParseException ex) { return await Record(source, new IngestionResult(false, 0, 0, 0, "parse", ex.Message, attemptedAt), cancellationToken); }
    }

    private async Task<IngestionResult> Record(Source source, IngestionResult result, CancellationToken cancellationToken)
    {
        db.FetchAttempts.Add(new FetchAttempt { SourceId = source.Id, AttemptedAt = result.AttemptedAt, Succeeded = result.Succeeded, EntryCount = result.EntryCount, InsertedCount = result.InsertedCount, SkippedCount = result.SkippedCount, FailureCategory = result.FailureCategory, Message = result.Message });
        await db.SaveChangesAsync(cancellationToken);
        return result;
    }
}
