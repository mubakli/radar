using Microsoft.EntityFrameworkCore;
using Radar.Api.Data;
using Radar.Api.Features.Ingestion;

namespace Radar.Api.Features.Sources;

public static class SourceEndpoints
{
    public static IEndpointRouteBuilder MapSourceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sources");
        group.MapGet("", async (RadarDbContext db, CancellationToken ct) => Results.Ok(await db.Sources.AsNoTracking().OrderBy(x => x.Name).Select(x => new SourceListResponse(x.Id, x.Name, x.Locator, x.Enabled, x.CreatedAt, x.FetchAttempts.OrderByDescending(a => a.AttemptedAt).Select(a => new FetchResponse(a.AttemptedAt, a.Succeeded, a.EntryCount, a.InsertedCount, a.SkippedCount, a.FailureCategory, a.Message)).FirstOrDefault())).ToListAsync(ct)));
        group.MapPost("", async (CreateSourceRequest request, RadarDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || !Uri.TryCreate(request.Locator, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) return Results.BadRequest("Name and an HTTP(S) feed URL are required.");
            var locator = new UriBuilder(uri) { Fragment = string.Empty }.Uri.AbsoluteUri;
            if (await db.Sources.AnyAsync(x => x.Locator == locator, ct)) return Results.Conflict("A Source with this locator already exists.");
            var source = new Source { Id = Guid.NewGuid(), Name = request.Name.Trim(), Locator = locator, CreatedAt = DateTimeOffset.UtcNow, Enabled = true };
            db.Sources.Add(source); await db.SaveChangesAsync(ct); return Results.Created($"/api/sources/{source.Id}", new SourceListResponse(source.Id, source.Name, source.Locator, source.Enabled, source.CreatedAt, null));
        });
        group.MapPatch("/{id:guid}/enabled", async (Guid id, SetEnabledRequest request, RadarDbContext db, CancellationToken ct) => { var source = await db.Sources.FindAsync([id], ct); if (source is null) return Results.NotFound(); source.Enabled = request.Enabled; await db.SaveChangesAsync(ct); return Results.Ok(new { source.Id, source.Enabled }); });
        group.MapPost("/{id:guid}/fetch", async (Guid id, FeedIngestionService ingestion, CancellationToken ct) => { try { return Results.Ok(await ingestion.FetchAsync(id, ct)); } catch (KeyNotFoundException) { return Results.NotFound(); } });
        group.MapGet("/{id:guid}/fetches", async (Guid id, RadarDbContext db, CancellationToken ct) => Results.Ok(await db.FetchAttempts.AsNoTracking().Where(x => x.SourceId == id).OrderByDescending(x => x.AttemptedAt).Select(x => new FetchResponse(x.AttemptedAt, x.Succeeded, x.EntryCount, x.InsertedCount, x.SkippedCount, x.FailureCategory, x.Message)).ToListAsync(ct)));
        group.MapGet("/{id:guid}/items", async (Guid id, RadarDbContext db, CancellationToken ct) => Results.Ok(await db.SourceItems.AsNoTracking().Where(x => x.SourceId == id).OrderByDescending(x => x.PublishedAt ?? x.ObservedAt).Select(x => new SourceItemResponse(x.Id, x.Title, x.Url, x.PublishedAt, x.Author, x.Summary, x.ObservedAt, x.CanonicalLocator)).ToListAsync(ct)));
        return endpoints;
    }
}

public sealed record CreateSourceRequest(string Name, string Locator);
public sealed record SetEnabledRequest(bool Enabled);
public sealed record FetchResponse(DateTimeOffset AttemptedAt, bool Succeeded, int EntryCount, int InsertedCount, int SkippedCount, string? FailureCategory, string? Message);
public sealed record SourceListResponse(Guid Id, string Name, string Locator, bool Enabled, DateTimeOffset CreatedAt, FetchResponse? LastFetch);
public sealed record SourceItemResponse(Guid Id, string Title, string? Url, DateTimeOffset? PublishedAt, string? Author, string? Summary, DateTimeOffset ObservedAt, string CanonicalLocator);
