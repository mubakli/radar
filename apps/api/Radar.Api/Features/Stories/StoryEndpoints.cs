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
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (story is null) return Results.NotFound();

            return Results.Ok(new StoryDetailResponse(
                story.Id, story.Title, story.Summary, story.CreatedAt,
                story.SourceItems.Select(m => new StorySourceItemResponse(
                    m.SourceItem.Id, m.SourceItem.Title, m.SourceItem.CanonicalLocator,
                    m.SourceItem.ObservedAt, m.MembershipMethod, m.MembershipReason,
                    new SourceResponse(m.SourceItem.Source.Id, m.SourceItem.Source.Name, m.SourceItem.Source.Locator))).ToList()));
        });
        return endpoints;
    }
}

public sealed record StoryListResponse(Guid Id, string Title, string Summary, DateTimeOffset CreatedAt);
public sealed record StoryDetailResponse(Guid Id, string Title, string Summary, DateTimeOffset CreatedAt, IReadOnlyList<StorySourceItemResponse> SourceItems);
public sealed record StorySourceItemResponse(Guid Id, string Title, string CanonicalLocator, DateTimeOffset ObservedAt, string MembershipMethod, string MembershipReason, SourceResponse Source);
public sealed record SourceResponse(Guid Id, string Name, string Locator);
