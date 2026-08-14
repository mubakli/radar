using Radar.Api.Data;
using Radar.Api.Features.Stories;
using Xunit;

namespace Radar.Api.UnitTests;

public sealed class StoryContractTests
{
    [Fact]
    public void Fixture_ids_are_stable_for_replayable_seed_data()
    {
        Assert.Equal(Guid.Parse("30000000-0000-0000-0000-000000000001"), SeedData.StoryId);
        Assert.Equal(Guid.Parse("20000000-0000-0000-0000-000000000001"), SeedData.SourceItemId);
    }

    [Fact]
    public void Story_detail_contract_keeps_original_locator_and_membership_reason()
    {
        var response = new StoryDetailResponse(
            Guid.NewGuid(), "Title", "Summary", DateTimeOffset.UtcNow,
            [new StorySourceItemResponse(Guid.NewGuid(), "Item", "https://example.test/item", DateTimeOffset.UtcNow,
                "fixture", "explicit fixture assignment", new SourceResponse(Guid.NewGuid(), "Source", "fixture://source"))]);

        Assert.Equal("https://example.test/item", response.SourceItems[0].CanonicalLocator);
        Assert.Equal("explicit fixture assignment", response.SourceItems[0].MembershipReason);
    }
}
