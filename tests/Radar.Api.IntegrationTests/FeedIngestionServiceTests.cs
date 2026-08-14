using Microsoft.EntityFrameworkCore;
using Radar.Api.Data;
using Radar.Api.Features.Ingestion;
using Testcontainers.PostgreSql;
using Xunit;

namespace Radar.Api.IntegrationTests;

public sealed class FeedIngestionServiceTests : IAsyncLifetime
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
    public async Task Same_feed_twice_persists_no_duplicate_items()
    {
        var fetcher = new FakeFetcher(Fixture("feed-a.xml"));
        await using var db = await NewDatabase(); var source = await AddSource(db);
        var service = new FeedIngestionService(db, fetcher);
        var first = await service.FetchAsync(source.Id, CancellationToken.None);
        var second = await service.FetchAsync(source.Id, CancellationToken.None);
        Assert.Equal(2, first.InsertedCount); Assert.Equal(0, second.InsertedCount); Assert.Equal(2, await db.SourceItems.CountAsync()); Assert.Equal(2, await db.FetchAttempts.CountAsync(x => x.Succeeded));
    }

    [Fact]
    public async Task Mixed_existing_and_new_feed_inserts_only_new_items()
    {
        var fetcher = new FakeFetcher(Fixture("feed-a.xml"), Fixture("feed-abcd.xml"));
        await using var db = await NewDatabase(); var source = await AddSource(db); var service = new FeedIngestionService(db, fetcher);
        await service.FetchAsync(source.Id, CancellationToken.None); var result = await service.FetchAsync(source.Id, CancellationToken.None);
        Assert.Equal(2, result.InsertedCount); Assert.Equal(4, await db.SourceItems.CountAsync());
    }

    [Fact]
    public async Task Disabled_source_does_not_call_fetcher_and_records_failure()
    {
        var fetcher = new FakeFetcher(Fixture("feed-a.xml")); await using var db = await NewDatabase(); var source = await AddSource(db, false);
        var result = await new FeedIngestionService(db, fetcher).FetchAsync(source.Id, CancellationToken.None);
        Assert.False(result.Succeeded); Assert.Equal("disabled", result.FailureCategory); Assert.Equal(0, fetcher.CallCount); Assert.Equal(0, await db.SourceItems.CountAsync()); Assert.Equal("disabled", await db.FetchAttempts.Select(x => x.FailureCategory).SingleAsync());
    }

    [Fact]
    public async Task Http_failure_records_failure_attempt_without_items()
    {
        var fetcher = new FakeFetcher(new FeedFetchException("http", "HTTP 500")); await using var db = await NewDatabase(); var source = await AddSource(db);
        var result = await new FeedIngestionService(db, fetcher).FetchAsync(source.Id, CancellationToken.None);
        Assert.False(result.Succeeded); Assert.Equal("http", result.FailureCategory); Assert.Empty(await db.SourceItems.ToListAsync()); Assert.Equal("http", await db.FetchAttempts.Select(x => x.FailureCategory).SingleAsync());
    }

    [Fact]
    public async Task Malformed_feed_records_parse_failure_without_items()
    {
        var fetcher = new FakeFetcher(Fixture("malformed.xml")); await using var db = await NewDatabase(); var source = await AddSource(db);
        var result = await new FeedIngestionService(db, fetcher).FetchAsync(source.Id, CancellationToken.None);
        Assert.False(result.Succeeded); Assert.Equal("parse", result.FailureCategory); Assert.Empty(await db.SourceItems.ToListAsync()); Assert.Equal("parse", await db.FetchAttempts.Select(x => x.FailureCategory).SingleAsync());
    }

    [Fact]
    public async Task Unexpected_failure_still_records_terminal_attempt_before_propagating()
    {
        var fetcher = new FakeFetcher(new InvalidOperationException("test failure")); await using var db = await NewDatabase(); var source = await AddSource(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new FeedIngestionService(db, fetcher).FetchAsync(source.Id, CancellationToken.None));
        Assert.Equal("unexpected", await db.FetchAttempts.Select(x => x.FailureCategory).SingleAsync());
    }

    [Fact]
    public async Task Unsupported_url_scheme_records_failure_without_fetching()
    {
        var fetcher = new FakeFetcher(Fixture("feed-a.xml")); await using var db = await NewDatabase(); var source = await AddSource(db, locator: "ftp://fixture.test/feed.xml");
        var result = await new FeedIngestionService(db, fetcher).FetchAsync(source.Id, CancellationToken.None);
        Assert.False(result.Succeeded); Assert.Equal("unsupported-url", result.FailureCategory); Assert.Equal(0, fetcher.CallCount); Assert.Empty(await db.SourceItems.ToListAsync()); Assert.Equal("unsupported-url", await db.FetchAttempts.Select(x => x.FailureCategory).SingleAsync());
    }

    [Fact]
    public async Task Cancelled_fetch_records_failure_before_propagating()
    {
        var fetcher = new FakeFetcher(new OperationCanceledException()); await using var db = await NewDatabase(); var source = await AddSource(db);
        await Assert.ThrowsAsync<OperationCanceledException>(() => new FeedIngestionService(db, fetcher).FetchAsync(source.Id, CancellationToken.None));
        Assert.Equal("cancelled", await db.FetchAttempts.Select(x => x.FailureCategory).SingleAsync());
    }

    private async Task<RadarDbContext> NewDatabase()
    {
        var db = new RadarDbContext(options); await db.FetchAttempts.ExecuteDeleteAsync(); await db.SourceItems.ExecuteDeleteAsync(); await db.Sources.ExecuteDeleteAsync(); return db;
    }

    private static async Task<Source> AddSource(RadarDbContext db, bool enabled = true, string? locator = null)
    {
        var source = new Source { Id = Guid.NewGuid(), Name = "Fixture", Locator = locator ?? "https://fixture.test/feed.xml", Enabled = enabled, CreatedAt = DateTimeOffset.UtcNow }; db.Sources.Add(source); await db.SaveChangesAsync(); return source;
    }

    private static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    private sealed class FakeFetcher(params object[] responses) : IFeedFetcher
    {
        private int index;
        public int CallCount { get; private set; }
        public Task<string> FetchAsync(Uri uri, CancellationToken cancellationToken)
        {
            CallCount++; var response = responses[Math.Min(index++, responses.Length - 1)];
            return response switch { string content => Task.FromResult(content), Exception exception => Task.FromException<string>(exception), _ => throw new InvalidOperationException() };
        }
    }
}
