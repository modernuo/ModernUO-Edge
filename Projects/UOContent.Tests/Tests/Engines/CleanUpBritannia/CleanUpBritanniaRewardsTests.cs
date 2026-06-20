using System;
using Server;
using Server.Engines.CleanUpBritannia;
using Server.Items;
using Xunit;

namespace UOContent.Tests;

public class CleanUpBritanniaRewardsTests : IClassFixture<CleanUpBritanniaFixture>
{
    [Fact]
    public void Rewards_AreNonEmptyWithPositiveCosts()
    {
        var rewards = CleanUpBritanniaRewards.Rewards;

        Assert.NotEmpty(rewards);

        foreach (var reward in rewards)
        {
            Assert.NotNull(reward.Type);
            Assert.True(reward.Cost > 0, $"{reward.Type.Name} must have a positive cost.");
        }
    }

    [Fact]
    public void Rewards_AllTypesAreDeliverableItems()
    {
        // The redemption gump delivers each reward as an Item. Every reward must derive from Item,
        // and (except ScrollofAlacrity, which the gump constructs directly) must be creatable via
        // Activator.CreateInstance — guarding against shipping a reward that throws on redemption.
        foreach (var reward in CleanUpBritanniaRewards.Rewards)
        {
            Assert.True(typeof(Item).IsAssignableFrom(reward.Type), $"{reward.Type.Name} must be an Item.");

            Item item;

            if (reward.Type == typeof(ScrollofAlacrity))
            {
                item = new ScrollofAlacrity();
            }
            else
            {
                item = Activator.CreateInstance(reward.Type) as Item;
            }

            Assert.NotNull(item);

            item.Delete();
        }
    }
}
