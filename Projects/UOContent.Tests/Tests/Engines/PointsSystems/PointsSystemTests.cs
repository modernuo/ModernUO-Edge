using System;
using Server;
using Server.Engines.Points;
using Server.Mobiles;
using Xunit;

namespace UOContent.Tests;

/// <summary>
/// Minimal fixture that registers <see cref="Map.Internal"/> so that
/// <see cref="Mobile(Serial)"/> can initialize its region field without
/// needing the full world fixture (and therefore no tiledata.mul).
/// </summary>
public sealed class PointsSystemFixture
{
    public PointsSystemFixture()
    {
        // Only register the Internal map if it hasn't been registered already
        // (e.g. by the full UOContentFixture in another collection).
        if (Map.Internal == null)
        {
            Map.Maps[0x7F] = new Map(
                0x7F, 0x7F, 0x7F,
                Map.SectorSize, Map.SectorSize,
                1, "Internal", MapRules.Internal
            );
        }
    }
}

public class PointsSystemTests : IClassFixture<PointsSystemFixture>, IDisposable
{
    // A test-only system: AutoAdd, finite cap to exercise clamping.
    private sealed class TestPoints : PointsSystem
    {
        public TestPoints() : base($"TestPoints_Unit_{Guid.NewGuid():N}", 250) { }
        public override TextDefinition Name => TextDefinition.Of("Test");
        public override PointsType Loyalty => PointsType.None;
        public override bool AutoAdd => true;
        public override double MaxPoints => 100.0;
        public override void SendMessage(PlayerMobile from, double old, double points, bool quest) { }
    }

    // These PlayerMobile instances are created via the Serial constructor and are NOT
    // registered in the World. They serve only as unique dictionary keys for the
    // PointsSystem._entries dictionary. Do NOT call Delete() on them — they have no
    // Items list and are not in any map, so Delete() would throw.
    private static PlayerMobile NewPlayer(uint serial) => new((Serial)serial);

    private readonly TestPoints _sys = new();

    public void Dispose()
    {
        PointsSystem.RemoveSystem(_sys);
    }

    [Fact]
    public void Award_AccumulatesAndClampsToMax()
    {
        var pm = NewPlayer(0x10100);

        _sys.AwardPoints(pm, 40, false, false);
        _sys.AwardPoints(pm, 40, false, false);
        Assert.Equal(80, _sys.GetPoints(pm));

        _sys.AwardPoints(pm, 50, false, false); // 130 -> clamp 100
        Assert.Equal(100, _sys.GetPoints(pm));
    }

    [Fact]
    public void Award_OnePlayerOneEntry()
    {
        var pm = NewPlayer(0x10101);

        _sys.AwardPoints(pm, 10, false, false);
        _sys.AwardPoints(pm, 10, false, false);

        Assert.Equal(20, _sys.GetPoints(pm)); // same entry, not duplicated
    }

    [Fact]
    public void Deduct_FailsWhenInsufficient_SucceedsOtherwise()
    {
        var pm = NewPlayer(0x10102);
        _sys.AwardPoints(pm, 30, false, false);

        Assert.False(_sys.DeductPoints(pm, 50)); // insufficient
        Assert.Equal(30, _sys.GetPoints(pm));    // unchanged

        Assert.True(_sys.DeductPoints(pm, 20));
        Assert.Equal(10, _sys.GetPoints(pm));
    }

    [Fact]
    public void GetPoints_UnknownPlayer_IsZero()
    {
        Assert.Equal(0, _sys.GetPoints(NewPlayer(0x10103)));
    }
}
