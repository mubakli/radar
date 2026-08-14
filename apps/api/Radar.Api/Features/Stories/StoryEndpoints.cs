using Microsoft.EntityFrameworkCore;
using Radar.Api.Data;

namespace Radar.Api.Features.Stories;

public static class StoryEndpoints
{
    public static IEndpointRouteBuilder MapStoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/stories");
        group.MapGet("", async (RadarDbContext db, CancellationToken cancellationToken) =>
        {
            var stories = await db.Stories.AsNoTracking().OrderByDescending(x => x.CreatedAt)
                .Select(x => new StoryListResponse(x.Id, x.Title, x.Summary, x.CreatedAt))
                .ToListAsync(cancellationToken);
            return Results.Ok(stories);
        });

        group.MapGet("/{id:guid}", async (Guid id, RadarDbContext db, CancellationToken cancellationToken) =>
        {
            var story = await db.Stories.AsNoTracking()
                .Include(x => x.SourceItems).ThenInclude(x => x.SourceItem).ThenInclude(x => x.Source)
                .Include(x => x.Corrections)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (story is null) return Results.NotFound();

            return Results.Ok(new StoryDetailResponse(
                story.Id, story.Title, story.Summary, story.CreatedAt,
                story.SourceItems.Select(m => new StorySourceItemResponse(
                    m.SourceItem.Id, m.SourceItem.Title, m.SourceItem.CanonicalLocator,
                    m.SourceItem.ObservedAt, m.MembershipMethod, m.MembershipMethodVersion, m.MembershipReason,
                    new SourceResponse(m.SourceItem.Source.Id, m.SourceItem.Source.Name, m.SourceItem.Source.Locator))).ToList(),
                story.Corrections.OrderBy(x => x.CreatedAt).Select(x => new StoryCorrectionResponse(x.Action, x.PreviousStoryId, x.SourceItemId, x.Reason, x.CreatedAt)).ToList()));
        });

        group.MapPost("/{destinationId:guid}/merge/{sourceId:guid}", async (Guid destinationId, Guid sourceId, CorrectionRequest request, RadarDbContext db, CancellationToken ct) =>
        {
            if (destinationId == sourceId) return Results.Ok(new StoryCorrectionResult(destinationId));
            var destination = await db.Stories.SingleOrDefaultAsync(x => x.Id == destinationId, ct);
            var source = await db.Stories.SingleOrDefaultAsync(x => x.Id == sourceId, ct);
            if (destination is null || source is null) return Results.NotFound();

            var existingCorrection = await db.StoryCorrections.AnyAsync(x => x.Action == "manual-merge" && x.ResultStoryId == destinationId && x.PreviousStoryId == sourceId && x.SourceItemId == null, ct);
            if (!existingCorrection)
            {
                var memberships = await db.StorySourceItems.Where(x => x.StoryId == sourceId).ToListAsync(ct);
                foreach (var membership in memberships)
                {
                    db.StorySourceItems.Remove(membership);
                    db.StorySourceItems.Add(new StorySourceItem
                    {
                        StoryId = destinationId, SourceItemId = membership.SourceItemId,
                        MembershipMethod = "manual-merge", MembershipMethodVersion = "manual-v1",
                        MembershipReason = request.Reason, CreatedAt = DateTimeOffset.UtcNow
                    });
                }
                db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-merge", ResultStoryId = destinationId, PreviousStoryId = sourceId, Reason = request.Reason, CreatedAt = DateTimeOffset.UtcNow });
                await db.SaveChangesAsync(ct);
            }
            return Results.Ok(new StoryCorrectionResult(destinationId));
        });

        group.MapPost("/{storyId:guid}/split/{sourceItemId:guid}", async (Guid storyId, Guid sourceItemId, CorrectionRequest request, RadarDbContext db, CancellationToken ct) =>
        {
            var prior = await db.StoryCorrections.AsNoTracking().SingleOrDefaultAsync(x => x.Action == "manual-split" && x.PreviousStoryId == storyId && x.SourceItemId == sourceItemId, ct);
            if (prior is not null) return Results.Ok(new StoryCorrectionResult(prior.ResultStoryId));

            var membership = await db.StorySourceItems.Include(x => x.SourceItem).SingleOrDefaultAsync(x => x.StoryId == storyId && x.SourceItemId == sourceItemId, ct);
            if (membership is null) return Results.NotFound();
            var splitStory = new Story { Id = Guid.NewGuid(), Title = membership.SourceItem.Title, Summary = membership.SourceItem.Summary ?? membership.SourceItem.Title, CreatedAt = DateTimeOffset.UtcNow };
            db.Stories.Add(splitStory);
            db.StorySourceItems.Remove(membership);
            db.StorySourceItems.Add(new StorySourceItem
            {
                StoryId = splitStory.Id, SourceItemId = sourceItemId, MembershipMethod = "manual-split",
                MembershipMethodVersion = "manual-v1", MembershipReason = request.Reason, CreatedAt = DateTimeOffset.UtcNow
            });
            db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-split", ResultStoryId = splitStory.Id, PreviousStoryId = storyId, SourceItemId = sourceItemId, Reason = request.Reason, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync(ct);
            return Results.Ok(new StoryCorrectionResult(splitStory.Id));
        });
        return endpoints;
    }
}

public sealed record StoryListResponse(Guid Id, string Title, string Summary, DateTimeOffset CreatedAt);
public sealed record StoryDetailResponse(Guid Id, string Title, string Summary, DateTimeOffset CreatedAt, IReadOnlyList<StorySourceItemResponse> SourceItems, IReadOnlyList<StoryCorrectionResponse> Corrections);
public sealed record StorySourceItemResponse(Guid Id, string Title, string CanonicalLocator, DateTimeOffset ObservedAt, string MembershipMethod, string MembershipMethodVersion, string MembershipReason, SourceResponse Source);
public sealed record SourceResponse(Guid Id, string Name, string Locator);
public sealed record CorrectionRequest(string Reason);
public sealed record StoryCorrectionResult(Guid StoryId);
public sealed record StoryCorrectionResponse(string Action, Guid? PreviousStoryId, Guid? SourceItemId, string Reason, DateTimeOffset CreatedAt);
