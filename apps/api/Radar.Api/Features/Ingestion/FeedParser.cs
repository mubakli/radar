using System.Net;
using System.Xml.Linq;
using System.Xml;

namespace Radar.Api.Features.Ingestion;

public sealed record ParsedEntry(string? Title, string? Url, DateTimeOffset? PublishedAt, string? Author, string? Summary, string CanonicalLocator, string RawContent);
public sealed record FeedParseResult(IReadOnlyList<ParsedEntry> Entries, int SkippedCount);

public static class FeedParser
{
    public static FeedParseResult Parse(string xml, Uri feedUri)
    {
        XDocument document;
        try { document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace); }
        catch (Exception ex) when (ex is XmlException or ArgumentException) { throw new FeedParseException("Malformed XML.", ex); }

        var root = document.Root ?? throw new FeedParseException("Feed has no root element.");
        var entries = root.Name.LocalName.Equals("rss", StringComparison.OrdinalIgnoreCase)
            ? ParseRss(root, feedUri) : root.Name.LocalName.Equals("feed", StringComparison.OrdinalIgnoreCase)
                ? ParseAtom(root, feedUri) : throw new FeedParseException("Unsupported feed format.");
        return new FeedParseResult(entries.Valid, entries.Skipped);
    }

    private static (List<ParsedEntry> Valid, int Skipped) ParseRss(XElement root, Uri feedUri)
    {
        var valid = new List<ParsedEntry>(); var skipped = 0;
        foreach (var item in root.Descendants().Where(x => x.Name.LocalName == "item"))
        {
            var title = Text(item, "title"); var link = AbsoluteUrl(Text(item, "link"), feedUri);
            var guid = Text(item, "guid"); var locator = Identity(link, guid, "rss-guid:");
            if (string.IsNullOrWhiteSpace(title) || locator is null) { skipped++; continue; }
            valid.Add(new ParsedEntry(title, link, Date(item, "pubDate"), Text(item, "author") ?? Text(item, "creator"), Text(item, "description"), locator, item.ToString(SaveOptions.DisableFormatting)));
        }
        return (valid, skipped);
    }

    private static (List<ParsedEntry> Valid, int Skipped) ParseAtom(XElement root, Uri feedUri)
    {
        var valid = new List<ParsedEntry>(); var skipped = 0;
        foreach (var entry in root.Descendants().Where(x => x.Name.LocalName == "entry"))
        {
            var title = Text(entry, "title"); var link = AbsoluteUrl(entry.Elements().FirstOrDefault(x => x.Name.LocalName == "link" && ((string?)x.Attribute("rel") is null or "alternate"))?.Attribute("href")?.Value, feedUri);
            var id = Text(entry, "id"); var locator = Identity(link, id, "atom-id:");
            if (string.IsNullOrWhiteSpace(title) || locator is null) { skipped++; continue; }
            var author = entry.Elements().FirstOrDefault(x => x.Name.LocalName == "author")?.Elements().FirstOrDefault(x => x.Name.LocalName == "name")?.Value;
            valid.Add(new ParsedEntry(title, link, Date(entry, "published") ?? Date(entry, "updated"), Clean(author), Text(entry, "summary") ?? Text(entry, "content"), locator, entry.ToString(SaveOptions.DisableFormatting)));
        }
        return (valid, skipped);
    }

    private static string? Text(XElement parent, string name) => Clean(parent.Elements().FirstOrDefault(x => x.Name.LocalName == name)?.Value);
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : System.Text.RegularExpressions.Regex.Replace(WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(value, "<[^>]+>", " ")), "\\s+", " ").Trim().Replace(" .", ".");
    private static DateTimeOffset? Date(XElement parent, string name) => DateTimeOffset.TryParse(Text(parent, name), out var date) ? date : null;
    private static string? AbsoluteUrl(string? value, Uri feedUri)
    {
        if (!Uri.TryCreate(feedUri, value, out var uri) || uri.Scheme is not ("http" or "https") || string.IsNullOrEmpty(uri.Host)) return null;
        var builder = new UriBuilder(uri) { Fragment = string.Empty }; return builder.Uri.AbsoluteUri;
    }
    private static string? Identity(string? url, string? native, string prefix) => url ?? (string.IsNullOrWhiteSpace(native) ? null : prefix + native.Trim());
}

public sealed class FeedParseException(string message, Exception? inner = null) : Exception(message, inner);
