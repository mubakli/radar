using Microsoft.EntityFrameworkCore;

namespace Radar.Api.Data;

public static class SeedData
{
    public static readonly Guid SourceId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid SourceItemId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid StoryId = Guid.Parse("30000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(RadarDbContext db, CancellationToken cancellationToken = default)
    {
        var source = await db.Sources.SingleOrDefaultAsync(x => x.Id == SourceId, cancellationToken);
        if (source is null)
        {
            source = new Source
            {
                Id = SourceId, Name = "Radar Development Fixture", Locator = "fixture://radar/milestone-1",
                CreatedAt = new DateTimeOffset(2026, 8, 14, 0, 0, 0, TimeSpan.Zero)
            };
            db.Sources.Add(source);
        }

        var item = await db.SourceItems.SingleOrDefaultAsync(x => x.Id == SourceItemId, cancellationToken);
        if (item is null)
        {
            item = new SourceItem
            {
                Id = SourceItemId, SourceId = SourceId, Title = "PostgreSQL 18 improves query execution",
                CanonicalLocator = "https://example.com/radar/postgresql-18-query-execution",
                RawContent = "Development fixture: PostgreSQL 18 introduces improvements to query execution and observability.",
                ObservedAt = new DateTimeOffset(2026, 8, 14, 9, 0, 0, TimeSpan.Zero)
            };
            db.SourceItems.Add(item);
        }

        var story = await db.Stories.SingleOrDefaultAsync(x => x.Id == StoryId, cancellationToken);
        if (story is null)
        {
            story = new Story
            {
                Id = StoryId, Title = "PostgreSQL 18 improves query execution",
                Summary = "A development fixture Story used to verify the first end-to-end Radar slice.",
                CreatedAt = new DateTimeOffset(2026, 8, 14, 9, 0, 0, TimeSpan.Zero)
            };
            db.Stories.Add(story);
        }

        var membershipExists = await db.StorySourceItems.AnyAsync(x => x.StoryId == StoryId && x.SourceItemId == SourceItemId, cancellationToken);
        if (!membershipExists)
        {
            db.StorySourceItems.Add(new StorySourceItem
            {
                StoryId = StoryId, SourceItemId = SourceItemId,
                MembershipMethod = "fixture",
                MembershipReason = "The development fixture explicitly assigns this observed item to the example Story."
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
