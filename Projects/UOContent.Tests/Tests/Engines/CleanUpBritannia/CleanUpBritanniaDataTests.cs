using System;
using System.Reflection;
using Server;
using Server.Engines.Points;
using Server.Items;
using Server.Mobiles;
using Xunit;

namespace UOContent.Tests;

/// <summary>
/// Minimal fixture for CleanUpBritannia tests.
/// Bootstraps the subset of server infrastructure required to construct Item objects
/// (Map.Internal + World running state + DecayScheduler) without loading tiledata.mul.
/// All calls are idempotent / guarded against double-init so this co-exists safely with
/// the full TestServerInitializer used by the Sequential UOContent Tests collection.
/// </summary>
public sealed class CleanUpBritanniaFixture
{
    private static bool _initialized;

    public CleanUpBritanniaFixture()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        if (Map.Internal == null)
        {
            Map.Maps[0x7F] = new Map(
                0x7F, 0x7F, 0x7F,
                Map.SectorSize, Map.SectorSize,
                1, "Internal", MapRules.Internal
            );
        }

        // Bootstrap the minimum server infrastructure so Item constructors can run.
        // World.Load() is guarded (WorldState.Initial check), so safe to call even if
        // the full TestServerInitializer has already run.
        if (World.WorldState == WorldState.Initial)
        {
            // Core.ApplicationAssembly drives Core.BaseDirectory; set it before Configure().
            Core.ApplicationAssembly = Assembly.GetExecutingAssembly();
            Core.LoopContext = new EventLoopContext();
            Core.Expansion = Expansion.EJ;

            ServerConfiguration.Load(true);
            World.Configure();
            Timer.Init(0);
            World.Load();
            World.ExitSerializationThreads();
        }

        if (DecayScheduler.Shared == null)
        {
            DecayScheduler.Configure();
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

    [Fact]
    public void GetPoints_Stackable_ScalesByAmount()
    {
        var ingots = new IronIngot(100); // 0.10 each
        try
        {
            Assert.Equal(10.0, CleanUpBritanniaData.GetPoints(ingots), 3);
        }
        finally
        {
            ingots.Delete();
        }
    }

    [Fact]
    public void GetPoints_TreasureMap_ByLevel()
    {
        var map = new TreasureMap(3, Map.Trammel); // level 3 -> 750
        try
        {
            Assert.Equal(750.0, CleanUpBritanniaData.GetPoints(map), 3);
        }
        finally
        {
            map.Delete();
        }
    }

    [Fact]
    public void GetPoints_UnknownItem_IsZero()
    {
        var rock = new Item(0x1363); // arbitrary non-table item
        try
        {
            Assert.Equal(0.0, CleanUpBritanniaData.GetPoints(rock), 3);
        }
        finally
        {
            rock.Delete();
        }
    }
}
