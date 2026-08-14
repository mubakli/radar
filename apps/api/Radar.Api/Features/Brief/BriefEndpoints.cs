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
            var stories = await db.Stories.AsNoTracking()
                .Include(x => x.SourceItems).ThenInclude(x => x.SourceItem).ThenInclude(x => x.Source)
                .Include(x => x.SourceItems).ThenInclude(x => x.SourceItem).ThenInclude(x => x.Feedback)
                .Where(x => x.SourceItems.Any(m => (m.SourceItem.PublishedAt ?? m.SourceItem.ObservedAt) >= start && (m.SourceItem.PublishedAt ?? m.SourceItem.ObservedAt) < end))
                .ToListAsync(ct);
            var responseStories = stories.Select(story =>
                {
                    var datedItems = story.SourceItems.Where(m => (m.SourceItem.PublishedAt ?? m.SourceItem.ObservedAt) >= start && (m.SourceItem.PublishedAt ?? m.SourceItem.ObservedAt) < end).ToList();
                    var lead = datedItems.OrderByDescending(m => m.SourceItem.Source.Priority).ThenByDescending(m => m.SourceItem.PublishedAt ?? m.SourceItem.ObservedAt).ThenBy(m => m.SourceItemId).First();
                    var sources = story.SourceItems.Select(m => new BriefSourceResponse(m.SourceItem.Source.Id, m.SourceItem.Source.Name)).Distinct().OrderBy(x => x.Name).ToList();
                    return new BriefStoryResponse(story.Id, story.Title, $"/stories/{story.Id}", lead.SourceItem.PublishedAt, lead.SourceItem.ObservedAt, lead.SourceItemId,
                        story.SourceItems.Count, sources.Count, sources, lead.SourceItem.Source.Priority,
                        $"Source priority {lead.SourceItem.Source.Priority}; newest contributing item; {story.SourceItems.Count} item(s) from {sources.Count} source(s)",
                        new FeedbackResponse(lead.SourceItem.Feedback?.Read ?? false, lead.SourceItem.Feedback?.Important ?? false, lead.SourceItem.Feedback?.Saved ?? false, lead.SourceItem.Feedback?.NotRelevant ?? false));
                })
                .OrderByDescending(x => x.SourcePriority).ThenByDescending(x => x.PublishedAt ?? x.ObservedAt).ThenBy(x => x.Id)
                .Take(take).ToList();
            return Results.Ok(new BriefResponse(day, zone.Id, take, responseStories.Count, responseStories.All(x => x.Feedback.Read || x.Feedback.NotRelevant), responseStories));
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
public sealed record BriefSourceResponse(Guid Id, string Name);
public sealed record BriefStoryResponse(Guid Id, string Title, string Locator, DateTimeOffset? PublishedAt, DateTimeOffset ObservedAt, Guid FeedbackSourceItemId, int ItemCount, int SourceCount, IReadOnlyList<BriefSourceResponse> Sources, int SourcePriority, string Reason, FeedbackResponse Feedback);
public sealed record BriefResponse(DateOnly Date, string Timezone, int Limit, int Count, bool Completed, IReadOnlyList<BriefStoryResponse> Stories);
