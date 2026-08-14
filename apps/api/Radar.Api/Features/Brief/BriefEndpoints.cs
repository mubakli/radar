using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Radar.Api.Data;

namespace Radar.Api.Features.Brief;

public sealed class BriefOptions { public int MaxItems { get; set; } = 20; }

public static class BriefEndpoints
{
    public static IEndpointRouteBuilder MapBriefEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/brief");
        group.MapGet("", async (DateOnly? date, string? timezone, int? limit, RadarDbContext db, IOptions<BriefOptions> options, CancellationToken ct) =>
        {
            var zone = ResolveTimeZone(timezone);
            var day = date ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime);
            var localStart = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            var start = new DateTimeOffset(localStart, zone.GetUtcOffset(localStart)).ToUniversalTime();
            var endLocal = localStart.AddDays(1);
            var end = new DateTimeOffset(endLocal, zone.GetUtcOffset(endLocal)).ToUniversalTime();
            var take = Math.Clamp(limit ?? options.Value.MaxItems, 1, options.Value.MaxItems);
            var items = await db.SourceItems.AsNoTracking().Include(x => x.Source).Include(x => x.Feedback)
                .Where(x => (x.PublishedAt ?? x.ObservedAt) >= start && (x.PublishedAt ?? x.ObservedAt) < end)
                .OrderByDescending(x => x.Source.Priority).ThenByDescending(x => x.PublishedAt ?? x.ObservedAt).ThenBy(x => x.Id)
                .Take(take).ToListAsync(ct);
            var responseItems = items.Select(x => new BriefItemResponse(x.Id, x.Title, x.Url ?? x.CanonicalLocator, x.PublishedAt, x.ObservedAt, x.Source.Id, x.Source.Name, x.Source.Priority, $"Source priority {x.Source.Priority}; newest published item", new FeedbackResponse(x.Feedback?.Read ?? false, x.Feedback?.Important ?? false, x.Feedback?.Saved ?? false, x.Feedback?.NotRelevant ?? false))).ToList();
            return Results.Ok(new BriefResponse(day, zone.Id, take, responseItems.Count, responseItems.All(x => x.Feedback.Read || x.Feedback.NotRelevant), responseItems));
        });
        group.MapPut("/items/{id:guid}/feedback", async (Guid id, FeedbackRequest request, RadarDbContext db, CancellationToken ct) =>
        {
            if (request.Action is not ("read" or "important" or "saved" or "not relevant")) return Results.BadRequest("Unknown feedback action.");
            if (!await db.SourceItems.AnyAsync(x => x.Id == id, ct)) return Results.NotFound();
            var feedback = await db.ItemFeedback.FindAsync([id], ct);
            if (feedback is null) { feedback = new ItemFeedback { SourceItemId = id }; db.ItemFeedback.Add(feedback); }
            switch (request.Action) { case "read": feedback.Read = request.Value; break; case "important": feedback.Important = request.Value; break; case "saved": feedback.Saved = request.Value; break; case "not relevant": feedback.NotRelevant = request.Value; break; }
            feedback.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new FeedbackResponse(feedback.Read, feedback.Important, feedback.Saved, feedback.NotRelevant));
        });
        return endpoints;
    }

    private static TimeZoneInfo ResolveTimeZone(string? id) { if (string.IsNullOrWhiteSpace(id)) return TimeZoneInfo.Utc; try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; } catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; } }
}

public sealed record FeedbackRequest(string Action, bool Value = true);
public sealed record FeedbackResponse(bool Read, bool Important, bool Saved, bool NotRelevant);
public sealed record BriefItemResponse(Guid Id, string Title, string Locator, DateTimeOffset? PublishedAt, DateTimeOffset ObservedAt, Guid SourceId, string SourceName, int SourcePriority, string Reason, FeedbackResponse Feedback);
public sealed record BriefResponse(DateOnly Date, string Timezone, int Limit, int Count, bool Completed, IReadOnlyList<BriefItemResponse> Items);
