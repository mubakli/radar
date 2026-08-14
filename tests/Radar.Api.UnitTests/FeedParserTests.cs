using Radar.Api.Features.Ingestion;
using Xunit;

namespace Radar.Api.UnitTests;

public sealed class FeedParserTests
{
    private static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
    private static readonly Uri FeedUri = new("https://feeds.example.test/root/feed.xml");

    [Fact]
    public void Rss_normalizes_metadata_and_preserves_entry_observation()
    {
        var result = FeedParser.Parse(Fixture("rss.xml"), FeedUri);
        Assert.Equal(2, result.Entries.Count);
        var item = result.Entries[0];
        Assert.Equal("First RSS item", item.Title);
        Assert.Equal("https://feeds.example.test/posts/one", item.Url);
        Assert.Equal(new DateTimeOffset(2026, 8, 14, 10, 0, 0, TimeSpan.Zero), item.PublishedAt);
        Assert.Equal("alice@example.test", item.Author);
        Assert.Equal("First summary.", item.Summary);
        Assert.Equal("https://feeds.example.test/posts/one", item.CanonicalLocator);
        Assert.Contains("<item>", item.RawContent);
    }

    [Fact]
    public void Atom_uses_id_when_link_is_missing_and_keeps_missing_fields_null()
    {
        var result = FeedParser.Parse(Fixture("atom.xml"), FeedUri);
        Assert.Equal("https://example.test/atom/one", result.Entries[0].CanonicalLocator);
        Assert.Equal("atom-id:tag:example.test,2026:two", result.Entries[1].CanonicalLocator);
        Assert.Null(result.Entries[1].Url);
        Assert.Null(result.Entries[1].Author);
        Assert.Null(result.Entries[1].Summary);
        Assert.Equal(new DateTimeOffset(2026, 8, 14, 13, 0, 0, TimeSpan.Zero), result.Entries[1].PublishedAt);
    }

    [Fact]
    public void Malformed_feed_is_a_controlled_parse_failure()
    {
        Assert.Throws<FeedParseException>(() => FeedParser.Parse(Fixture("invalid.xml"), FeedUri));
    }

    [Fact]
    public void Missing_metadata_remains_missing()
    {
        var item = Assert.Single(FeedParser.Parse(Fixture("missing-metadata.xml"), FeedUri).Entries);
        Assert.Null(item.PublishedAt);
        Assert.Null(item.Author);
        Assert.Null(item.Summary);
    }

    [Fact]
    public void Entry_without_title_or_identity_is_skipped()
    {
        var result = FeedParser.Parse("<rss><channel><item><title>valid</title><guid>x</guid></item><item><link>https://example.test/no-title</link></item><item><title>no identity</title></item></channel></rss>", FeedUri);
        Assert.Single(result.Entries);
        Assert.Equal(2, result.SkippedCount);
    }
}
