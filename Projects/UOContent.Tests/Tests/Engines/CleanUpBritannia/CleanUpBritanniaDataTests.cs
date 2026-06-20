using System;
using System.Reflection;
using Server;
using Server.Engines.Points;
using Server.Mobiles;
using Xunit;

namespace UOContent.Tests;

// Reuses the same minimal Internal-map fixture approach as PointsSystemTests.
public sealed class CleanUpBritanniaFixture
{
    public CleanUpBritanniaFixture()
    {
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

public class CleanUpBritanniaDataTests : IClassFixture<CleanUpBritanniaFixture>, IDisposable
{
    private readonly CleanUpBritanniaData _sys = new();

    public void Dispose() => PointsSystem.RemoveSystem(_sys);

    private static PlayerMobile NewPlayer(uint serial) => new((Serial)serial);

    // Sets World.WorldState via reflection so FindEntity<PlayerMobile> can look up test mobiles.
    // Restores the original state in the returned IDisposable.
    private static IDisposable WithWorldRunning()
    {
        var prop = typeof(World).GetProperty(
            nameof(World.WorldState),
            BindingFlags.Public | BindingFlags.Static
        )!;
        var prev = (WorldState)prop.GetValue(null)!;
        prop.SetValue(null, WorldState.Running);
        return new RestoreWorldState(prop, prev);
    }

    private sealed class RestoreWorldState : IDisposable
    {
        private readonly PropertyInfo _prop;
        private readonly WorldState _prev;
        public RestoreWorldState(PropertyInfo prop, WorldState prev) { _prop = prop; _prev = prev; }
        public void Dispose() => _prop.SetValue(null, _prev);
    }

    [Fact]
    public void Award_Persists_RoundTrip()
    {
        var pm = NewPlayer(0x20100);
        _sys.AwardPoints(pm, 1234, false, false);

        var writer = new BufferWriter(true);
        _sys.Serialize(writer);

        var buffer = new byte[writer.Position];
        writer.Buffer.AsSpan(0, (int)writer.Position).CopyTo(buffer);

        // Register pm in World.Mobiles so ReadEntity<PlayerMobile> finds it,
        // and set WorldState to Running so FindEntity's switch reaches the lookup branch.
        World.Mobiles[(Serial)0x20100] = pm;
        using (WithWorldRunning())
        {
            var reader = new BufferReader(buffer);
            var sys2 = new CleanUpBritanniaData();
            try
            {
                sys2.Deserialize(reader);
                Assert.Equal(1234, sys2.GetPoints(pm));
            }
            finally
            {
                PointsSystem.RemoveSystem(sys2);
            }
        }

        World.Mobiles.Remove((Serial)0x20100);
    }
}
