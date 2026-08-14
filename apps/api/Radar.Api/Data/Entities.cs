namespace Radar.Api.Data;

public sealed class Source
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Locator { get; set; }
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<SourceItem> Items { get; set; } = [];
    public List<FetchAttempt> FetchAttempts { get; set; } = [];
}

public sealed class SourceItem
{
    public Guid Id { get; set; }
    public Guid SourceId { get; set; }
    public required string CanonicalLocator { get; set; }
    public string? Url { get; set; }
    public required string Title { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? Author { get; set; }
    public string? Summary { get; set; }
    public required string RawContent { get; set; }
    public DateTimeOffset ObservedAt { get; set; }
    public Source Source { get; set; } = null!;
    public List<StorySourceItem> StoryMemberships { get; set; } = [];
    public ItemFeedback? Feedback { get; set; }
}

public sealed class FetchAttempt
{
    public Guid Id { get; set; }
    public Guid SourceId { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public bool Succeeded { get; set; }
    public int EntryCount { get; set; }
    public int InsertedCount { get; set; }
    public int SkippedCount { get; set; }
    public string? FailureCategory { get; set; }
    public string? Message { get; set; }
    public Source Source { get; set; } = null!;
}

public sealed class Story
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<StorySourceItem> SourceItems { get; set; } = [];
    public List<StoryCorrection> Corrections { get; set; } = [];
}

public sealed class StorySourceItem
{
    public Guid StoryId { get; set; }
    public Guid SourceItemId { get; set; }
    public required string MembershipMethod { get; set; }
    public string MembershipMethodVersion { get; set; } = "legacy-v1";
    public required string MembershipReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Story Story { get; set; } = null!;
    public SourceItem SourceItem { get; set; } = null!;
}

public sealed class StoryCorrection
{
    public Guid Id { get; set; }
    public required string Action { get; set; }
    public Guid ResultStoryId { get; set; }
    public Guid? PreviousStoryId { get; set; }
    public Guid? SourceItemId { get; set; }
    public required string Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Story ResultStory { get; set; } = null!;
}

public sealed class ItemFeedback
{
    public Guid SourceItemId { get; set; }
    public bool Read { get; set; }
    public bool Important { get; set; }
    public bool Saved { get; set; }
    public bool NotRelevant { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public SourceItem SourceItem { get; set; } = null!;
}
