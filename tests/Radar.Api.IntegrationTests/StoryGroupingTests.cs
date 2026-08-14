using System.Text.Json;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Radar.Api.Data;
using Radar.Api.Features.Stories;
using Testcontainers.PostgreSql;
using Xunit;

namespace Radar.Api.IntegrationTests;

public sealed class StoryGroupingTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder().WithImage("postgres:18-alpine").WithDatabase("radar").WithUsername("radar").WithPassword("radar").Build();
    private DbContextOptions<RadarDbContext> options = null!;

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        options = new DbContextOptionsBuilder<RadarDbContext>().UseNpgsql(database.GetConnectionString()).Options;
        await using var db = new RadarDbContext(options);
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await database.DisposeAsync();

    [Fact]
    public async Task Golden_fixture_groups_conservatively_and_is_reproducible()
    {
        var fixture = LoadFixture();
        await using var db = new RadarDbContext(options);
        SeedItems(db, fixture);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        await grouping.GroupUngroupedAsync();
        var first = await Memberships(db);
        await grouping.GroupUngroupedAsync();
        var second = await Memberships(db);

        Assert.Equal(first, second);
        foreach (var expected in fixture.GroupBy(x => x.ExpectedGroup)) Assert.Single(expected.Select(x => first[x.Id]).Distinct());
        Assert.NotEqual(first[fixture.Single(x => x.ExpectedGroup == "kubernetes-134-fix").Id], first[fixture.Single(x => x.ExpectedGroup == "kubernetes-135-fix").Id]);
        Assert.Equal(fixture.Count, await db.StorySourceItems.CountAsync());
        Assert.All(await db.StorySourceItems.ToListAsync(), membership => Assert.Equal(StoryGroupingService.MethodVersion, membership.MembershipMethodVersion));
        Assert.Contains(await db.StorySourceItems.Select(x => x.MembershipMethod).ToListAsync(), method => method == "exact-url");
        Assert.Contains(await db.StorySourceItems.Select(x => x.MembershipMethod).ToListAsync(), method => method == "canonical-url-equivalent");
        Assert.Contains(await db.StorySourceItems.Select(x => x.MembershipMethod).ToListAsync(), method => method == "normalized-title");
    }

    [Fact]
    public async Task Golden_fixture_adversarial_normalization_cases()
    {
        var fixture = LoadFixture();
        await using var db = new RadarDbContext(options);
        SeedItems(db, fixture);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        await grouping.GroupUngroupedAsync();
        var memberships = await Memberships(db);

        var caseInsensitive = fixture.Where(x => x.ExpectedGroup == "case-insensitive").ToList();
        Assert.Equal(2, caseInsensitive.Count);
        Assert.Single(caseInsensitive.Select(x => memberships[x.Id]).Distinct());

        var punctuation = fixture.Where(x => x.ExpectedGroup == "punctuation-norm").ToList();
        Assert.Equal(2, punctuation.Count);
        Assert.Single(punctuation.Select(x => memberships[x.Id]).Distinct());

        var whitespace = fixture.Where(x => x.ExpectedGroup == "whitespace-norm").ToList();
        Assert.Equal(2, whitespace.Count);
        Assert.Single(whitespace.Select(x => memberships[x.Id]).Distinct());

        var docker0 = fixture.Single(x => x.ExpectedGroup == "docker-28-0");
        var docker1 = fixture.Single(x => x.ExpectedGroup == "docker-28-1");
        Assert.NotEqual(memberships[docker0.Id], memberships[docker1.Id]);

        var node24 = fixture.Single(x => x.ExpectedGroup == "node-24");
        var node22 = fixture.Single(x => x.ExpectedGroup == "node-22");
        Assert.NotEqual(memberships[node24.Id], memberships[node22.Id]);
    }

    [Fact]
    public async Task Short_titles_below_threshold_do_not_group_by_normalized_title()
    {
        var fixture = LoadFixture();
        await using var db = new RadarDbContext(options);
        SeedItems(db, fixture);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        await grouping.GroupUngroupedAsync();
        var memberships = await Memberships(db);

        var shortA = fixture.Single(x => x.ExpectedGroup == "short-a");
        var shortB = fixture.Single(x => x.ExpectedGroup == "short-b");
        Assert.NotEqual(memberships[shortA.Id], memberships[shortB.Id]);

        var shortMemberships = await db.StorySourceItems.Where(x => x.SourceItemId == shortA.Id || x.SourceItemId == shortB.Id).ToListAsync();
        Assert.All(shortMemberships, m => Assert.Equal("new-story", m.MembershipMethod));
    }

    [Fact]
    public async Task Normalization_collision_candidate_merges_unsafely()
    {
        var fixture = LoadFixture();
        await using var db = new RadarDbContext(options);
        SeedItems(db, fixture);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        await grouping.GroupUngroupedAsync();
        var memberships = await Memberships(db);

        var collisionA = fixture.Single(x => x.ExpectedGroup == "collision-a");
        var collisionB = fixture.Single(x => x.ExpectedGroup == "collision-b");

        var titleA = StoryGroupingService.NormalizeTitle(collisionA.Title);
        var titleB = StoryGroupingService.NormalizeTitle(collisionB.Title);
        Assert.Equal(titleA, titleB);

        Assert.Equal(memberships[collisionA.Id], memberships[collisionB.Id]);
    }

    [Fact]
    public async Task Reprocessing_idempotency_leave_story_structure_unchanged()
    {
        var fixture = LoadFixture();

        await using (var db = new RadarDbContext(options))
        {
            SeedItems(db, fixture);
            await db.SaveChangesAsync();
            var grouping = new StoryGroupingService(db);
            await grouping.GroupUngroupedAsync();
        }

        await using var dbVerify = new RadarDbContext(options);
        var membershipsAfterFirst = await Memberships(dbVerify);
        var storyCountAfterFirst = await dbVerify.Stories.CountAsync();
        var membershipCountAfterFirst = await dbVerify.StorySourceItems.CountAsync();
        var methodsAfterFirst = await dbVerify.StorySourceItems.OrderBy(x => x.SourceItemId).Select(x => new { x.MembershipMethod, x.MembershipMethodVersion, x.MembershipReason }).ToListAsync();

        await using (var db2 = new RadarDbContext(options))
        {
            var grouping2 = new StoryGroupingService(db2);
            await grouping2.GroupUngroupedAsync();
        }

        await using var dbAfter = new RadarDbContext(options);
        var membershipsAfterSecond = await Memberships(dbAfter);
        var storyCountAfterSecond = await dbAfter.Stories.CountAsync();
        var membershipCountAfterSecond = await dbAfter.StorySourceItems.CountAsync();
        var methodsAfterSecond = await dbAfter.StorySourceItems.OrderBy(x => x.SourceItemId).Select(x => new { x.MembershipMethod, x.MembershipMethodVersion, x.MembershipReason }).ToListAsync();

        Assert.Equal(membershipsAfterFirst, membershipsAfterSecond);
        Assert.Equal(storyCountAfterFirst, storyCountAfterSecond);
        Assert.Equal(membershipCountAfterFirst, membershipCountAfterSecond);
        Assert.Equal(methodsAfterFirst, methodsAfterSecond);
        Assert.Equal(fixture.Count, membershipCountAfterSecond);
    }

    [Fact]
    public async Task Order_independence_produces_same_semantic_partitions()
    {
        var fixture = LoadFixture();
        List<KeyValuePair<Guid, List<Guid>>> forward;

        await using (var db1 = new RadarDbContext(options))
        {
            SeedItems(db1, fixture);
            await db1.SaveChangesAsync();
            var grouping1 = new StoryGroupingService(db1);
            await grouping1.GroupUngroupedAsync();
            forward = await SemanticPartitions(db1);
        }

        List<KeyValuePair<Guid, List<Guid>>> reversed;

        await using (var db2 = new RadarDbContext(options))
        {
            var grouping2 = new StoryGroupingService(db2);
            var idsDesc = fixture.Select(x => x.Id).Reverse().ToList();
            foreach (var id in idsDesc) await grouping2.GroupAsync(id);
            reversed = await SemanticPartitions(db2);
        }

        Assert.Equal(forward.Count, reversed.Count);
        var forwardSets = forward.OrderBy(x => x.Key).Select(x => new HashSet<Guid>(x.Value)).ToList();
        var reversedSets = reversed.OrderBy(x => x.Key).Select(x => new HashSet<Guid>(x.Value)).ToList();
        for (var i = 0; i < forwardSets.Count; i++) Assert.Equal(forwardSets[i], reversedSets[i]);
    }

    [Fact]
    public async Task Merge_survives_regrouping()
    {
        await using var db = new RadarDbContext(options);
        var source1 = new Source { Id = Guid.NewGuid(), Name = "Source A", Locator = "fixture://merge-a", CreatedAt = DateTimeOffset.UnixEpoch };
        var source2 = new Source { Id = Guid.NewGuid(), Name = "Source B", Locator = "fixture://merge-b", CreatedAt = DateTimeOffset.UnixEpoch };
        db.Sources.AddRange(source1, source2);
        var itemA = new SourceItem { Id = Guid.NewGuid(), SourceId = source1.Id, CanonicalLocator = "https://merge.test/item-a", Url = "https://merge.test/item-a", Title = "Kubernetes 1.34 release announcement", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
        var itemB = new SourceItem { Id = Guid.NewGuid(), SourceId = source2.Id, CanonicalLocator = "https://merge.test/item-b", Url = "https://merge.test/item-b", Title = "Docker 28.0 release announcement", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch.AddMinutes(1) };
        db.SourceItems.AddRange(itemA, itemB);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        var storyA = await grouping.GroupAsync(itemA.Id);
        var storyB = await grouping.GroupAsync(itemB.Id);
        Assert.NotEqual(storyA, storyB);

        var membershipsBeforeMerge = await Memberships(db);
        var storyCountBefore = await db.Stories.CountAsync();

        db.StorySourceItems.RemoveRange(await db.StorySourceItems.Where(x => x.StoryId == storyB).ToListAsync());
        await db.SaveChangesAsync();
        db.StorySourceItems.Add(new StorySourceItem { StoryId = storyA, SourceItemId = itemB.Id, MembershipMethod = "manual-merge", MembershipMethodVersion = "manual-v1", MembershipReason = "Test merge.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-merge", ResultStoryId = storyA, PreviousStoryId = storyB, Reason = "Test merge.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        await grouping.GroupUngroupedAsync();
        var membershipsAfterRegroup = await Memberships(db);
        var storyCountAfter = await db.Stories.CountAsync();

        Assert.Equal(storyCountBefore, storyCountAfter);
        Assert.Equal(membershipsBeforeMerge[itemA.Id], membershipsAfterRegroup[itemA.Id]);
        Assert.Equal(storyA, membershipsAfterRegroup[itemB.Id]);
        var itemBMembership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == itemB.Id);
        Assert.Equal("manual-merge", itemBMembership.MembershipMethod);
    }

    [Fact]
    public async Task Split_survives_regrouping()
    {
        await using var db = new RadarDbContext(options);
        var source = new Source { Id = Guid.NewGuid(), Name = "Source", Locator = "fixture://split-regroup", CreatedAt = DateTimeOffset.UnixEpoch };
        db.Sources.Add(source);
        var itemA = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://split-regroup.test/item-a", Url = "https://split-regroup.test/item-a", Title = "PostgreSQL 18.1 new feature overview", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
        var itemB = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://split-regroup.test/item-b", Url = "https://split-regroup.test/item-b", Title = "PostgreSQL 18.1 new feature overview", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch.AddMinutes(1) };
        db.SourceItems.AddRange(itemA, itemB);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        var originalStory = await grouping.GroupAsync(itemA.Id);
        var groupedStory = await grouping.GroupAsync(itemB.Id);
        Assert.Equal(originalStory, groupedStory);

        db.StorySourceItems.RemoveRange(await db.StorySourceItems.Where(x => x.StoryId == originalStory && x.SourceItemId == itemB.Id).ToListAsync());
        await db.SaveChangesAsync();
        var splitStory = new Story { Id = Guid.NewGuid(), Title = itemB.Title, Summary = itemB.Title, CreatedAt = DateTimeOffset.UtcNow };
        db.Stories.Add(splitStory);
        db.StorySourceItems.Add(new StorySourceItem { StoryId = splitStory.Id, SourceItemId = itemB.Id, MembershipMethod = "manual-split", MembershipMethodVersion = "manual-v1", MembershipReason = "Test split.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-split", ResultStoryId = splitStory.Id, PreviousStoryId = originalStory, SourceItemId = itemB.Id, Reason = "Test split.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var membershipsBeforeRegroup = await Memberships(db);
        var storyCountBefore = await db.Stories.CountAsync();

        await grouping.GroupUngroupedAsync();
        var membershipsAfterRegroup = await Memberships(db);
        var storyCountAfter = await db.Stories.CountAsync();

        Assert.Equal(storyCountBefore, storyCountAfter);
        Assert.Equal(membershipsBeforeRegroup[itemA.Id], membershipsAfterRegroup[itemA.Id]);
        Assert.Equal(splitStory.Id, membershipsAfterRegroup[itemB.Id]);
        var itemBMembership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == itemB.Id);
        Assert.Equal("manual-split", itemBMembership.MembershipMethod);
    }

    [Fact]
    public async Task Single_item_split_leaves_valid_state()
    {
        await using var db = new RadarDbContext(options);
        var source = new Source { Id = Guid.NewGuid(), Name = "Source", Locator = "fixture://single-split", CreatedAt = DateTimeOffset.UnixEpoch };
        db.Sources.Add(source);
        var item = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://single-split.test/item", Url = "https://single-split.test/item", Title = "Solitary item for split test", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
        db.SourceItems.Add(item);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        var originalStory = await grouping.GroupAsync(item.Id);

        var splitStoryId = Guid.NewGuid();
        db.StorySourceItems.RemoveRange(await db.StorySourceItems.Where(x => x.StoryId == originalStory && x.SourceItemId == item.Id).ToListAsync());
        await db.SaveChangesAsync();
        var splitStory = new Story { Id = splitStoryId, Title = item.Title, Summary = item.Title, CreatedAt = DateTimeOffset.UtcNow };
        db.Stories.Add(splitStory);
        db.StorySourceItems.Add(new StorySourceItem { StoryId = splitStoryId, SourceItemId = item.Id, MembershipMethod = "manual-split", MembershipMethodVersion = "manual-v1", MembershipReason = "Single item split.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-split", ResultStoryId = splitStoryId, PreviousStoryId = originalStory, SourceItemId = item.Id, Reason = "Single item split.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var originalStoryAfter = await db.Stories.Include(x => x.SourceItems).SingleAsync(x => x.Id == originalStory);
        var splitStoryAfter = await db.Stories.Include(x => x.SourceItems).SingleAsync(x => x.Id == splitStoryId);

        Assert.Empty(originalStoryAfter.SourceItems);
        Assert.Single(splitStoryAfter.SourceItems);
        Assert.Equal(item.Id, splitStoryAfter.SourceItems[0].SourceItemId);

        var membership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == item.Id);
        Assert.Equal(splitStoryId, membership.StoryId);
        Assert.Equal("manual-split", membership.MembershipMethod);

        Assert.Equal(1, await db.SourceItems.CountAsync());
        Assert.Equal(2, await db.Stories.CountAsync());
        Assert.Equal(1, await db.StorySourceItems.CountAsync());
        Assert.Equal(1, await db.StoryCorrections.CountAsync());
    }

    [Fact]
    public async Task Repeated_merge_does_not_duplicate_memberships_or_corrections()
    {
        await using var db = new RadarDbContext(options);
        var source1 = new Source { Id = Guid.NewGuid(), Name = "A", Locator = "fixture://rep-merge-a", CreatedAt = DateTimeOffset.UnixEpoch };
        var source2 = new Source { Id = Guid.NewGuid(), Name = "B", Locator = "fixture://rep-merge-b", CreatedAt = DateTimeOffset.UnixEpoch };
        db.Sources.AddRange(source1, source2);
        var itemA = new SourceItem { Id = Guid.NewGuid(), SourceId = source1.Id, CanonicalLocator = "https://rep-merge.test/a", Url = "https://rep-merge.test/a", Title = "Rust 1.90 async closures stable", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
        var itemB = new SourceItem { Id = Guid.NewGuid(), SourceId = source2.Id, CanonicalLocator = "https://rep-merge.test/b", Url = "https://rep-merge.test/b", Title = "Docker 28 release highlights", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch.AddMinutes(1) };
        db.SourceItems.AddRange(itemA, itemB);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        var storyA = await grouping.GroupAsync(itemA.Id);
        var storyB = await grouping.GroupAsync(itemB.Id);

        await using var db2 = new RadarDbContext(options);
        var existingCorrection = await db2.StoryCorrections.AnyAsync(x => x.Action == "manual-merge" && x.ResultStoryId == storyA && x.PreviousStoryId == storyB && x.SourceItemId == null);
        Assert.False(existingCorrection);

        db.StorySourceItems.RemoveRange(await db.StorySourceItems.Where(x => x.StoryId == storyB).ToListAsync());
        db.StorySourceItems.Add(new StorySourceItem { StoryId = storyA, SourceItemId = itemB.Id, MembershipMethod = "manual-merge", MembershipMethodVersion = "manual-v1", MembershipReason = "Repeated merge.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-merge", ResultStoryId = storyA, PreviousStoryId = storyB, Reason = "Repeated merge.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var correctionsAfterFirst = await db.StoryCorrections.CountAsync();
        var membershipsAfterFirst = await db.StorySourceItems.CountAsync();

        db.ChangeTracker.Clear();
        existingCorrection = await db.StoryCorrections.AnyAsync(x => x.Action == "manual-merge" && x.ResultStoryId == storyA && x.PreviousStoryId == storyB && x.SourceItemId == null);
        Assert.True(existingCorrection);

        Assert.Equal(correctionsAfterFirst, await db.StoryCorrections.CountAsync());
        Assert.Equal(membershipsAfterFirst, await db.StorySourceItems.CountAsync());
    }

    [Fact]
    public async Task Repeated_split_does_not_duplicate_memberships_or_corrections()
    {
        await using var db = new RadarDbContext(options);
        var source = new Source { Id = Guid.NewGuid(), Name = "A", Locator = "fixture://rep-split", CreatedAt = DateTimeOffset.UnixEpoch };
        var source2 = new Source { Id = Guid.NewGuid(), Name = "B", Locator = "fixture://rep-split-b", CreatedAt = DateTimeOffset.UnixEpoch };
        db.Sources.AddRange(source, source2);
        var itemA = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://rep-split.test/a", Url = "https://rep-split.test/a", Title = "Go 1.22 release details", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
        var itemB = new SourceItem { Id = Guid.NewGuid(), SourceId = source2.Id, CanonicalLocator = "https://rep-split.test/b", Url = "https://rep-split.test/b", Title = "  Go 1.22 - release details!  ", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch.AddMinutes(1) };
        db.SourceItems.AddRange(itemA, itemB);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        var story = await grouping.GroupAsync(itemA.Id);
        var storyB = await grouping.GroupAsync(itemB.Id);
        Assert.Equal(story, storyB);

        var splitStoryId = Guid.NewGuid();
        await db.StorySourceItems.Where(x => x.StoryId == story && x.SourceItemId == itemB.Id).ExecuteDeleteAsync();
        var splitStory = new Story { Id = splitStoryId, Title = itemB.Title, Summary = itemB.Title, CreatedAt = DateTimeOffset.UtcNow };
        db.Stories.Add(splitStory);
        db.StorySourceItems.Add(new StorySourceItem { StoryId = splitStoryId, SourceItemId = itemB.Id, MembershipMethod = "manual-split", MembershipMethodVersion = "manual-v1", MembershipReason = "First split.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-split", ResultStoryId = splitStoryId, PreviousStoryId = story, SourceItemId = itemB.Id, Reason = "First split.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var correctionsAfterFirst = await db.StoryCorrections.CountAsync();
        var membershipsAfterFirst = await db.StorySourceItems.CountAsync();

        db.ChangeTracker.Clear();
        var repeat = await db.StoryCorrections.AsNoTracking().SingleOrDefaultAsync(x => x.Action == "manual-split" && x.PreviousStoryId == story && x.SourceItemId == itemB.Id);
        Assert.NotNull(repeat);
        Assert.Equal(splitStoryId, repeat!.ResultStoryId);

        var itemAMembership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == itemA.Id);
        Assert.Equal(story, itemAMembership.StoryId);
        var itemBMembership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == itemB.Id);
        Assert.Equal(splitStoryId, itemBMembership.StoryId);
    }

    [Fact]
    public async Task Merge_after_split_composes_safely()
    {
        await using var db = new RadarDbContext(options);
        var source = new Source { Id = Guid.NewGuid(), Name = "A", Locator = "fixture://merge-after-split", CreatedAt = DateTimeOffset.UnixEpoch };
        db.Sources.Add(source);
        var itemA = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://merge-after-split.test/a", Url = "https://merge-after-split.test/a", Title = "PostgreSQL 19 new features", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
        var itemB = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://merge-after-split.test/b", Url = "https://merge-after-split.test/b", Title = "PostgreSQL 19 performance improvements", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch.AddMinutes(1) };
        db.SourceItems.AddRange(itemA, itemB);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        var storyA = await grouping.GroupAsync(itemA.Id);
        var storyB = await grouping.GroupAsync(itemB.Id);

        var splitStoryId = Guid.NewGuid();
        db.StorySourceItems.RemoveRange(await db.StorySourceItems.Where(x => x.StoryId == storyB && x.SourceItemId == itemB.Id).ToListAsync());
        await db.SaveChangesAsync();
        var splitStory = new Story { Id = splitStoryId, Title = itemB.Title, Summary = itemB.Title, CreatedAt = DateTimeOffset.UtcNow };
        db.Stories.Add(splitStory);
        db.StorySourceItems.Add(new StorySourceItem { StoryId = splitStoryId, SourceItemId = itemB.Id, MembershipMethod = "manual-split", MembershipMethodVersion = "manual-v1", MembershipReason = "Split first.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-split", ResultStoryId = splitStoryId, PreviousStoryId = storyB, SourceItemId = itemB.Id, Reason = "Split first.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        db.StorySourceItems.RemoveRange(await db.StorySourceItems.Where(x => x.StoryId == splitStoryId && x.SourceItemId == itemB.Id).ToListAsync());
        await db.SaveChangesAsync();
        db.StorySourceItems.Add(new StorySourceItem { StoryId = storyA, SourceItemId = itemB.Id, MembershipMethod = "manual-merge", MembershipMethodVersion = "manual-v1", MembershipReason = "Merge after split.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-merge", ResultStoryId = storyA, PreviousStoryId = splitStoryId, Reason = "Merge after split.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var itemAMembership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == itemA.Id);
        var itemBMembership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == itemB.Id);
        Assert.Equal(storyA, itemAMembership.StoryId);
        Assert.Equal(storyA, itemBMembership.StoryId);
        Assert.Equal(2, await db.StoryCorrections.CountAsync());
        Assert.Equal(2, await db.StorySourceItems.CountAsync());
    }

    [Fact]
    public async Task Split_after_merge_composes_safely()
    {
        await using var db = new RadarDbContext(options);
        var source = new Source { Id = Guid.NewGuid(), Name = "A", Locator = "fixture://split-after-merge", CreatedAt = DateTimeOffset.UnixEpoch };
        db.Sources.Add(source);
        var itemA = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://split-after-merge.test/a", Url = "https://split-after-merge.test/a", Title = "Kubernetes 1.36 release notes", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
        var itemB = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://split-after-merge.test/b", Url = "https://split-after-merge.test/b", Title = "Kubernetes 1.36 upgrade guide", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch.AddMinutes(1) };
        db.SourceItems.AddRange(itemA, itemB);
        await db.SaveChangesAsync();

        var grouping = new StoryGroupingService(db);
        var storyA = await grouping.GroupAsync(itemA.Id);
        var storyB = await grouping.GroupAsync(itemB.Id);

        db.StorySourceItems.RemoveRange(await db.StorySourceItems.Where(x => x.StoryId == storyB).ToListAsync());
        await db.SaveChangesAsync();
        db.StorySourceItems.Add(new StorySourceItem { StoryId = storyA, SourceItemId = itemB.Id, MembershipMethod = "manual-merge", MembershipMethodVersion = "manual-v1", MembershipReason = "Merge first.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-merge", ResultStoryId = storyA, PreviousStoryId = storyB, Reason = "Merge first.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var splitStoryId = Guid.NewGuid();
        db.StorySourceItems.RemoveRange(await db.StorySourceItems.Where(x => x.StoryId == storyA && x.SourceItemId == itemB.Id).ToListAsync());
        await db.SaveChangesAsync();
        var splitStory = new Story { Id = splitStoryId, Title = itemB.Title, Summary = itemB.Title, CreatedAt = DateTimeOffset.UtcNow };
        db.Stories.Add(splitStory);
        db.StorySourceItems.Add(new StorySourceItem { StoryId = splitStoryId, SourceItemId = itemB.Id, MembershipMethod = "manual-split", MembershipMethodVersion = "manual-v1", MembershipReason = "Split after merge.", CreatedAt = DateTimeOffset.UtcNow });
        db.StoryCorrections.Add(new StoryCorrection { Id = Guid.NewGuid(), Action = "manual-split", ResultStoryId = splitStoryId, PreviousStoryId = storyA, SourceItemId = itemB.Id, Reason = "Split after merge.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var itemAMembership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == itemA.Id);
        var itemBMembership = await db.StorySourceItems.SingleAsync(x => x.SourceItemId == itemB.Id);
        Assert.Equal(storyA, itemAMembership.StoryId);
        Assert.Equal(splitStoryId, itemBMembership.StoryId);
        Assert.Equal(2, await db.StoryCorrections.CountAsync());
        Assert.Equal(2, await db.StorySourceItems.CountAsync());
    }

    [Fact]
    public async Task Duplicate_membership_rejected_at_persistence_level()
    {
        await using (var setup = new RadarDbContext(options))
        {
            var source = new Source { Id = Guid.NewGuid(), Name = "A", Locator = "fixture://dup-membership", CreatedAt = DateTimeOffset.UnixEpoch };
            setup.Sources.Add(source);
            var item = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://dup-membership.test/item", Url = "https://dup-membership.test/item", Title = "Dup membership test item", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
            setup.SourceItems.Add(item);
            var story = new Story { Id = Guid.NewGuid(), Title = "Dup story", Summary = "Dup story", CreatedAt = DateTimeOffset.UtcNow };
            setup.Stories.Add(story);
            await setup.SaveChangesAsync();
            setup.StorySourceItems.Add(new StorySourceItem { StoryId = story.Id, SourceItemId = item.Id, MembershipMethod = "test", MembershipMethodVersion = "test-v1", MembershipReason = "first", CreatedAt = DateTimeOffset.UtcNow });
            await setup.SaveChangesAsync();
        }

        await using (var db = new RadarDbContext(options))
        {
            var storyId = await db.Stories.Select(x => x.Id).SingleAsync();
            var itemId = await db.SourceItems.Select(x => x.Id).SingleAsync();
            db.StorySourceItems.Add(new StorySourceItem { StoryId = storyId, SourceItemId = itemId, MembershipMethod = "test", MembershipMethodVersion = "test-v1", MembershipReason = "duplicate", CreatedAt = DateTimeOffset.UtcNow });
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Contains("duplicate key value violates unique constraint", ex.InnerException!.Message);
        }
    }

    [Fact]
    public async Task Concurrent_grouping_of_same_item_is_prevented_by_unique_constraint()
    {
        await using var setup = new RadarDbContext(options);
        var source = new Source { Id = Guid.NewGuid(), Name = "A", Locator = "fixture://concurrency", CreatedAt = DateTimeOffset.UnixEpoch };
        setup.Sources.Add(source);
        var item = new SourceItem { Id = Guid.NewGuid(), SourceId = source.Id, CanonicalLocator = "https://concurrency.test/item", Url = "https://concurrency.test/item", Title = "Concurrency test item", RawContent = "raw", ObservedAt = DateTimeOffset.UnixEpoch };
        setup.SourceItems.Add(item);
        await setup.SaveChangesAsync();

        var succeeded = 0;
        var tasks = Enumerable.Range(0, 3).Select(async _ =>
        {
            await using var db = new RadarDbContext(options);
            var grouping = new StoryGroupingService(db);
            try { await grouping.GroupAsync(item.Id); Interlocked.Increment(ref succeeded); }
            catch (DbUpdateException) { }
        }).ToList();
        await Task.WhenAll(tasks);

        Assert.True(succeeded >= 1, "At least one concurrent grouping should succeed.");

        await using var verify = new RadarDbContext(options);
        Assert.Equal(1, await verify.StorySourceItems.CountAsync(x => x.SourceItemId == item.Id));
    }

    private static List<GoldenItem> LoadFixture() =>
        JsonSerializer.Deserialize<List<GoldenItem>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "story-grouping.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    private static void SeedItems(RadarDbContext db, List<GoldenItem> fixture)
    {
        foreach (var (entry, index) in fixture.Select((entry, index) => (entry, index)))
        {
            var source = new Source { Id = Guid.NewGuid(), Name = entry.Source, Locator = $"fixture://golden/{index}", CreatedAt = DateTimeOffset.UnixEpoch };
            db.Sources.Add(source);
            db.SourceItems.Add(new SourceItem { Id = entry.Id, SourceId = source.Id, CanonicalLocator = entry.Url, Url = entry.Url, Title = entry.Title, RawContent = entry.Title, ObservedAt = DateTimeOffset.UnixEpoch.AddMinutes(index) });
        }
    }

    private static Task<Dictionary<Guid, Guid>> Memberships(RadarDbContext db) => db.StorySourceItems.AsNoTracking().OrderBy(x => x.SourceItemId).ToDictionaryAsync(x => x.SourceItemId, x => x.StoryId);

    private static async Task<List<KeyValuePair<Guid, List<Guid>>>> SemanticPartitions(RadarDbContext db)
    {
        var groups = await db.StorySourceItems.AsNoTracking().OrderBy(x => x.StoryId).GroupBy(x => x.StoryId)
            .Select(g => new KeyValuePair<Guid, List<Guid>>(g.Key, g.Select(x => x.SourceItemId).OrderBy(x => x).ToList())).ToListAsync();
        return groups;
    }

    private sealed record GoldenItem(Guid Id, string Source, string Title, string Url, string ExpectedGroup);
}
