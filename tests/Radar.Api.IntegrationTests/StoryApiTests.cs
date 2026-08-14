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
    }

    [Fact]
    public async Task Missing_story_returns_not_found()
    {
        var response = await client.GetAsync($"/api/stories/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record StoryListResponse(Guid Id, string Title, string Summary, DateTimeOffset CreatedAt);
    private sealed record StoryDetailResponse(Guid Id, string Title, string Summary, DateTimeOffset CreatedAt, List<StorySourceItemResponse> SourceItems);
    private sealed record StorySourceItemResponse(Guid Id, string Title, string CanonicalLocator, DateTimeOffset ObservedAt, string MembershipMethod, string MembershipReason, SourceResponse Source);
    private sealed record SourceResponse(Guid Id, string Name, string Locator);
}
