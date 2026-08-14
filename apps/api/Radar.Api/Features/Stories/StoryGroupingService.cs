using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Radar.Api.Data;

namespace Radar.Api.Features.Stories;

public sealed partial class StoryGroupingService(RadarDbContext db)
{
    public const string MethodVersion = "deterministic-v1";

    public async Task<Guid> GroupAsync(Guid sourceItemId, CancellationToken cancellationToken = default)
    {
        var existing = await db.StorySourceItems.AsNoTracking().SingleOrDefaultAsync(x => x.SourceItemId == sourceItemId, cancellationToken);
        if (existing is not null) return existing.StoryId;

        var item = await db.SourceItems.AsNoTracking().SingleAsync(x => x.Id == sourceItemId, cancellationToken);
        var candidates = await db.StorySourceItems.AsNoTracking()
            .Include(x => x.SourceItem)
            .OrderBy(x => x.Story.CreatedAt).ThenBy(x => x.StoryId).ThenBy(x => x.SourceItemId)
            .ToListAsync(cancellationToken);

        var locator = item.Url ?? item.CanonicalLocator;
        var canonicalUrl = CanonicalizeUrl(locator);
        var normalizedTitle = NormalizeTitle(item.Title);
        var compared = candidates.Select(x => new { Membership = x, Locator = x.SourceItem.Url ?? x.SourceItem.CanonicalLocator, Url = CanonicalizeUrl(x.SourceItem.Url ?? x.SourceItem.CanonicalLocator), Title = NormalizeTitle(x.SourceItem.Title) });
        var match = compared.FirstOrDefault(x => canonicalUrl is not null && x.Locator == locator);
        var method = "exact-url";
        var reason = match is null ? null : $"Observed URL exactly equals {locator}.";

        if (match is null)
        {
            match = compared.FirstOrDefault(x => canonicalUrl is not null && x.Url == canonicalUrl);
            method = "canonical-url-equivalent";
            reason = match is null ? null : $"Canonical URL equals {canonicalUrl}.";
        }

        if (match is null)
        {
            match = compared.FirstOrDefault(x => normalizedTitle.Length >= 20 && x.Title == normalizedTitle);
            method = "normalized-title";
            reason = match is null ? null : $"Normalized title equals '{normalizedTitle}'.";
        }

        Story story;
        if (match is null)
        {
            story = new Story { Id = StableStoryId(item.Id), Title = item.Title, Summary = item.Summary ?? item.Title, CreatedAt = item.ObservedAt };
            db.Stories.Add(story);
            method = "new-story";
            reason = "No canonical URL or normalized title match was found; created a separate Story.";
        }
        else
        {
            story = await db.Stories.SingleAsync(x => x.Id == match.Membership.StoryId, cancellationToken);
        }

        db.StorySourceItems.Add(new StorySourceItem
        {
            StoryId = story.Id, SourceItemId = item.Id, MembershipMethod = method,
            MembershipMethodVersion = MethodVersion, MembershipReason = reason!, CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        return story.Id;
    }

    public async Task GroupUngroupedAsync(CancellationToken cancellationToken = default)
    {
        var ids = await db.SourceItems.AsNoTracking().Where(x => !x.StoryMemberships.Any()).OrderBy(x => x.ObservedAt).ThenBy(x => x.Id).Select(x => x.Id).ToListAsync(cancellationToken);
        foreach (var id in ids) await GroupAsync(id, cancellationToken);
    }

    public static string NormalizeTitle(string title)
    {
        var decomposed = title.Normalize(NormalizationForm.FormD);
        var withoutMarks = string.Concat(decomposed.Where(x => CharUnicodeInfo.GetUnicodeCategory(x) != UnicodeCategory.NonSpacingMark));
        return WhitespaceRegex().Replace(NonAlphaNumericRegex().Replace(withoutMarks.ToLowerInvariant(), " "), " ").Trim();
    }

    public static string? CanonicalizeUrl(string locator)
    {
        if (!Uri.TryCreate(locator, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) return null;
        var builder = new UriBuilder(uri) { Fragment = "", Scheme = uri.Scheme.ToLowerInvariant(), Host = uri.Host.ToLowerInvariant() };
        if (builder.Uri.IsDefaultPort) builder.Port = -1;
        builder.Path = builder.Path.Length > 1 ? builder.Path.TrimEnd('/') : builder.Path;
        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !x.StartsWith("utm_", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        builder.Query = string.Join('&', query);
        return builder.Uri.AbsoluteUri;
    }

    private static Guid StableStoryId(Guid sourceItemId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"radar-story:{sourceItemId:D}"));
        return new Guid(hash.AsSpan(0, 16));
    }

    [GeneratedRegex(@"[^\p{L}\p{N}]+")]
    private static partial Regex NonAlphaNumericRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
