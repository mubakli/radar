using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Radar.Api.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace Radar.Api.IntegrationTests;

public sealed class PersistenceConstraintTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .WithDatabase("radar")
        .WithUsername("radar")
        .WithPassword("radar")
        .Build();
    private WebApplicationFactory<Program> factory = null!;

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
            });
        });
    }

    public async Task DisposeAsync()
    {
        await factory.DisposeAsync();
        await database.DisposeAsync();
    }

    [Fact]
    public async Task Duplicate_source_with_same_id_is_rejected()
    {
        var id = Guid.Parse("10000000-0000-0000-0000-000000000001");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            db.Sources.Add(NewSource(id, "loc://a"));
            await db.SaveChangesAsync();
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            db.Sources.Add(NewSource(id, "loc://other"));
            await AssertRejectedByUniqueConstraint(db);
        }
    }

    [Fact]
    public async Task Duplicate_source_with_same_locator_is_rejected()
    {
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            db.Sources.Add(NewSource(Guid.Parse("10000000-0000-0000-0000-000000000002"), "loc://shared"));
            await db.SaveChangesAsync();
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            db.Sources.Add(NewSource(Guid.Parse("10000000-0000-0000-0000-000000000003"), "loc://shared"));
            await AssertRejectedByUniqueConstraint(db);
        }
    }

    [Fact]
    public async Task Duplicate_source_item_locator_within_source_is_rejected()
    {
        var sourceId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            db.Sources.Add(NewSource(sourceId, "loc://items"));
            await db.SaveChangesAsync();
            db.SourceItems.Add(NewSourceItem(Guid.Parse("20000000-0000-0000-0000-000000000002"), sourceId, "https://example.com/a"));
            await db.SaveChangesAsync();
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            db.SourceItems.Add(NewSourceItem(Guid.Parse("20000000-0000-0000-0000-000000000003"), sourceId, "https://example.com/a"));
            await AssertRejectedByUniqueConstraint(db);
        }
    }

    [Fact]
    public async Task Duplicate_story_source_item_membership_is_rejected()
    {
        var sourceId = Guid.Parse("10000000-0000-0000-0000-000000000005");
        var itemId = Guid.Parse("20000000-0000-0000-0000-000000000004");
        var storyId = Guid.Parse("30000000-0000-0000-0000-000000000002");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            db.Sources.Add(NewSource(sourceId, "loc://membership"));
            await db.SaveChangesAsync();
            db.SourceItems.Add(NewSourceItem(itemId, sourceId, "https://example.com/membership"));
            await db.SaveChangesAsync();
            db.Stories.Add(new Story { Id = storyId, Title = "T", Summary = "S", CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
            db.StorySourceItems.Add(NewMembership(storyId, itemId));
            await db.SaveChangesAsync();
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
            db.StorySourceItems.Add(NewMembership(storyId, itemId));
            await AssertRejectedByUniqueConstraint(db);
        }
    }

    private static async Task AssertRejectedByUniqueConstraint(RadarDbContext db)
    {
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        var inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.Contains("duplicate key value violates unique constraint", inner!.Message);
    }

    private static Source NewSource(Guid id, string locator) => new()
    {
        Id = id, Name = "Test Source", Locator = locator, CreatedAt = DateTimeOffset.UtcNow
    };

    private static SourceItem NewSourceItem(Guid id, Guid sourceId, string locator) => new()
    {
        Id = id, SourceId = sourceId, CanonicalLocator = locator, Title = "Test Item",
        RawContent = "raw", ObservedAt = DateTimeOffset.UtcNow
    };

    private static StorySourceItem NewMembership(Guid storyId, Guid sourceItemId) => new()
    {
        StoryId = storyId, SourceItemId = sourceItemId,
        MembershipMethod = "test", MembershipMethodVersion = "test-v1", MembershipReason = "test", CreatedAt = DateTimeOffset.UtcNow
    };
}
