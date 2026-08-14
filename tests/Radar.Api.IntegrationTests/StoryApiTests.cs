using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Radar.Api.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace Radar.Api.IntegrationTests;

public sealed class StoryApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .WithDatabase("radar")
        .WithUsername("radar")
        .WithPassword("radar")
        .Build();
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Radar", database.GetConnectionString());
            builder.UseSetting("Environment", "Testing");
            builder.ConfigureServices(services =>
            {
                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
                db.Database.Migrate();
                SeedData.SeedAsync(db).GetAwaiter().GetResult();
            });
        });
        client = factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await factory.DisposeAsync();
        await database.DisposeAsync();
    }

    [Fact]
    public async Task List_returns_seed_story_and_seed_is_idempotent()
    {
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            await SeedData.SeedAsync(db);
        }

        var stories = await client.GetFromJsonAsync<List<StoryListResponse>>("/api/stories");
        var story = Assert.Single(stories!);
        Assert.Equal(SeedData.StoryId, story.Id);
        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<RadarDbContext>();
        Assert.Equal(1, await verifyDb.Sources.CountAsync());
        Assert.Equal(1, await verifyDb.SourceItems.CountAsync());
        Assert.Equal(1, await verifyDb.Stories.CountAsync());
        Assert.Equal(1, await verifyDb.StorySourceItems.CountAsync());
    }

    [Fact]
    public async Task Detail_returns_source_item_source_and_provenance()
    {
        var response = await client.GetFromJsonAsync<StoryDetailResponse>($"/api/stories/{SeedData.StoryId}");
        Assert.NotNull(response);
        Assert.Single(response!.SourceItems);
        Assert.Equal("https://example.com/radar/postgresql-18-query-execution", response.SourceItems[0].CanonicalLocator);
        Assert.Equal("Radar Development Fixture", response.SourceItems[0].Source.Name);
        Assert.Equal("fixture", response.SourceItems[0].MembershipMethod);
        Assert.Equal("fixture-v1", response.SourceItems[0].MembershipMethodVersion);
    }

    [Fact]
    public async Task Missing_story_returns_not_found()
    {
        var response = await client.GetAsync($"/api/stories/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Daily_brief_is_bounded_and_feedback_survives_repeated_reads()
    {
        var brief = await client.GetFromJsonAsync<BriefResponse>("/api/brief?date=2026-08-14&timezone=UTC&limit=1");
        Assert.NotNull(brief);
        Assert.Equal(1, brief!.Count);
        Assert.Equal(1, brief.Limit);
        Assert.False(brief.Completed);

        var feedback = await client.PutAsJsonAsync($"/api/brief/items/{SeedData.SourceItemId}/feedback", new { action = "read", value = true });
        Assert.Equal(HttpStatusCode.OK, feedback.StatusCode);
        var repeat = await client.PutAsJsonAsync($"/api/brief/items/{SeedData.SourceItemId}/feedback", new { action = "read", value = true });
        Assert.Equal(HttpStatusCode.OK, repeat.StatusCode);

        var reopened = await client.GetFromJsonAsync<BriefResponse>("/api/brief?date=2026-08-14&timezone=UTC&limit=1");
        Assert.True(reopened!.Stories[0].Feedback.Read);
        Assert.True(reopened.Completed);
    }

    [Fact]
    public async Task Invalid_timezone_falls_back_to_UTC()
    {
        var response = await client.GetFromJsonAsync<BriefResponse>("/api/brief?date=2026-08-14&timezone=Invalid/Zone&limit=1");
        Assert.NotNull(response);
        Assert.Equal("UTC", response!.Timezone);
    }

    [Fact]
    public async Task Feedback_to_nonexistent_item_returns_not_found()
    {
        var response = await client.PutAsJsonAsync($"/api/brief/items/{Guid.NewGuid()}/feedback", new { action = "read", value = true });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Manual_merge_is_idempotent_and_brief_shows_one_story_with_both_sources()
    {
        var (sourceId, itemId, storyId) = await AddSecondStory();
        var first = await client.PostAsJsonAsync($"/api/stories/{SeedData.StoryId}/merge/{storyId}", new { reason = "Both reports cover the same PostgreSQL release." });
        var second = await client.PostAsJsonAsync($"/api/stories/{SeedData.StoryId}/merge/{storyId}", new { reason = "Both reports cover the same PostgreSQL release." });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var detail = await client.GetFromJsonAsync<StoryDetailResponse>($"/api/stories/{SeedData.StoryId}");
        Assert.Equal(2, detail!.SourceItems.Count);
        Assert.Equal(2, detail.SourceItems.Select(x => x.Source.Id).Distinct().Count());
        Assert.Contains(detail.SourceItems, x => x.Id == itemId && x.MembershipMethod == "manual-merge");
        var brief = await client.GetFromJsonAsync<BriefResponse>("/api/brief?date=2026-08-14&timezone=UTC");
        var story = Assert.Single(brief!.Stories);
        Assert.Equal(SeedData.StoryId, story.Id);
        Assert.Equal(2, story.ItemCount);
        Assert.Equal(2, story.SourceCount);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
        Assert.Equal(2, await db.SourceItems.CountAsync());
        Assert.Equal(2, await db.StorySourceItems.CountAsync());
        Assert.Equal(1, await db.StoryCorrections.CountAsync());
        Assert.True(await db.Sources.AnyAsync(x => x.Id == sourceId));
    }

    [Fact]
    public async Task Manual_split_is_idempotent_and_preserves_items_in_two_brief_stories()
    {
        var (_, itemId, groupedStoryId) = await AddSecondStory(SeedData.StoryId);
        Assert.Equal(SeedData.StoryId, groupedStoryId);
        var first = await client.PostAsJsonAsync($"/api/stories/{SeedData.StoryId}/split/{itemId}", new { reason = "This report covers a different release." });
        var firstResult = await first.Content.ReadFromJsonAsync<StoryCorrectionResult>();
        var second = await client.PostAsJsonAsync($"/api/stories/{SeedData.StoryId}/split/{itemId}", new { reason = "This report covers a different release." });
        var secondResult = await second.Content.ReadFromJsonAsync<StoryCorrectionResult>();
        Assert.Equal(firstResult!.StoryId, secondResult!.StoryId);

        var original = await client.GetFromJsonAsync<StoryDetailResponse>($"/api/stories/{SeedData.StoryId}");
        var split = await client.GetFromJsonAsync<StoryDetailResponse>($"/api/stories/{firstResult.StoryId}");
        Assert.Single(original!.SourceItems);
        var splitItem = Assert.Single(split!.SourceItems);
        Assert.Equal(itemId, splitItem.Id);
        Assert.Equal("manual-split", splitItem.MembershipMethod);
        var brief = await client.GetFromJsonAsync<BriefResponse>("/api/brief?date=2026-08-14&timezone=UTC");
        Assert.Equal(2, brief!.Stories.Count);
        Assert.All(brief.Stories, story => { Assert.Equal(1, story.ItemCount); Assert.Equal(1, story.SourceCount); });

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
        Assert.Equal(2, await db.SourceItems.CountAsync());
        Assert.Equal(2, await db.StorySourceItems.CountAsync());
        Assert.Equal(1, await db.StoryCorrections.CountAsync());
    }

    private async Task<(Guid SourceId, Guid ItemId, Guid StoryId)> AddSecondStory(Guid? existingStoryId = null)
    {
        var sourceId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var storyId = existingStoryId ?? Guid.NewGuid();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
        db.Sources.Add(new Source { Id = sourceId, Name = "Second Source", Locator = $"fixture://second/{sourceId}", CreatedAt = DateTimeOffset.UtcNow });
        db.SourceItems.Add(new SourceItem { Id = itemId, SourceId = sourceId, CanonicalLocator = $"https://second.test/{itemId}", Url = $"https://second.test/{itemId}", Title = "PostgreSQL release from another report", RawContent = "original second observation", ObservedAt = new DateTimeOffset(2026, 8, 14, 10, 0, 0, TimeSpan.Zero) });
        if (existingStoryId is null) db.Stories.Add(new Story { Id = storyId, Title = "Second report", Summary = "Second report", CreatedAt = new DateTimeOffset(2026, 8, 14, 10, 0, 0, TimeSpan.Zero) });
        db.StorySourceItems.Add(new StorySourceItem { StoryId = storyId, SourceItemId = itemId, MembershipMethod = "fixture", MembershipMethodVersion = "fixture-v1", MembershipReason = "Correction test setup.", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        return (sourceId, itemId, storyId);
    }

    private sealed record StoryListResponse(Guid Id, string Title, string Summary, DateTimeOffset CreatedAt);
    private sealed record StoryDetailResponse(Guid Id, string Title, string Summary, DateTimeOffset CreatedAt, List<StorySourceItemResponse> SourceItems, List<StoryCorrectionResponse> Corrections);
    private sealed record StorySourceItemResponse(Guid Id, string Title, string CanonicalLocator, DateTimeOffset ObservedAt, string MembershipMethod, string MembershipMethodVersion, string MembershipReason, SourceResponse Source);
    private sealed record StoryCorrectionResponse(string Action, Guid? PreviousStoryId, Guid? SourceItemId, string Reason, DateTimeOffset CreatedAt);
    private sealed record SourceResponse(Guid Id, string Name, string Locator);
    private sealed record BriefResponse(DateOnly Date, string Timezone, int Limit, int Count, bool Completed, List<BriefStoryResponse> Stories);
    private sealed record BriefStoryResponse(Guid Id, string Title, string Locator, DateTimeOffset? PublishedAt, DateTimeOffset ObservedAt, Guid FeedbackSourceItemId, int ItemCount, int SourceCount, List<BriefSourceResponse> Sources, int SourcePriority, string Reason, FeedbackResponse Feedback);
    private sealed record BriefSourceResponse(Guid Id, string Name);
    private sealed record FeedbackResponse(bool Read, bool Important, bool Saved, bool NotRelevant);
    private sealed record StoryCorrectionResult(Guid StoryId);
}
