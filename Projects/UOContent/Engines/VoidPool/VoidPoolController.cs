// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/VoidPoolController.cs
using System;
using System.Collections.Generic;
using ModernUO.Serialization;
using Server.Collections;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.VoidPool;

public enum VoidType
{
    Abyss,
    Repond,
    Undead,
    Reptile,
    Elemental
}

[SerializationGenerator(0, false)]
public partial class VoidPoolController : Item
{
    public static VoidPoolController InstanceTram { get; set; }
    public static VoidPoolController InstanceFel { get; set; }

    private const int RestartSpan = 15;
    private const int PoolStartHits = 15;
    private const int StartPointVariance = 8;

    private static readonly Point3D StartPoint1 = new(5592, 2012, 0);
    private static readonly Point3D StartPoint2 = new(5466, 2007, 0);

    private static readonly Point3D EndPoint = new(5500, 1998, 5);
    private static readonly Rectangle2D PoolWalls = new(5495, 1993, 10, 10);
    private static readonly Point3D PoolCenter = new(5500, 1998, 5);

    [SerializableField(0)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private int _respawnMin;

    [SerializableField(1)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private int _respawnMax;

    [SerializableField(2, getter: "private", setter: "private")]
    private bool _activeState;

    [Tidy]
    [SerializableField(3)]
    private List<WayPoint> _waypointsA;

    [Tidy]
    [SerializableField(4)]
    private List<WayPoint> _waypointsB;

    private TimerExecutionToken _timerToken;

    [CommandProperty(AccessLevel.GameMaster)]
    public bool Active
    {
        get => _activeState;
        set
        {
            if (!value)
            {
                _timerToken.Cancel();

                if (Region != null)
                {
                    Region.Unregister();
                    Region = null;
                }
            }
            else
            {
                Region ??= new VoidPoolRegion(this, Map);
                Region.Register();

                if (!_timerToken.Running)
                {
                    Timer.StartTimer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), OnTick, out _timerToken);

                    NextStart = DateTime.UtcNow + TimeSpan.FromMinutes(RestartSpan);

                    // The battle for the Void Pool will begin in ~1_VALUE~ minutes.
                    Region.SendRegionMessage(1152526, RestartSpan.ToString());
                }
            }

            ActiveState = value;
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int Wave { get; set; }

    [CommandProperty(AccessLevel.GameMaster)]
    public int Stage
    {
        get
        {
            if (Wave < 2)
            {
                return 0;
            }

            return Math.Max(1, Wave / 5);
        }
    }

    public List<WaveInfo> Waves { get; set; }

    public VoidPoolRegion Region { get; set; }
    public Dictionary<Mobile, long> CurrentScore { get; set; }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool OnGoing { get; private set; }

    [CommandProperty(AccessLevel.GameMaster)]
    public VoidType VoidType { get; set; }

    [CommandProperty(AccessLevel.GameMaster)]
    public DateTime NextStart { get; private set; }

    [CommandProperty(AccessLevel.GameMaster)]
    public DateTime NextWave { get; private set; }

    [CommandProperty(AccessLevel.GameMaster)]
    public int PoolHits { get; set; }

    public VoidPoolController(Map map) : base(3803)
    {
        Name = "Void Pool Controller";
        Visible = false;
        Movable = false;

        PoolHits = PoolStartHits;

        if (map == Map.Trammel)
        {
            if (InstanceTram != null)
            {
                Delete();
            }
            else
            {
                InstanceTram = this;
            }
        }
        else if (map == Map.Felucca)
        {
            if (InstanceFel != null)
            {
                Delete();
            }
            else
            {
                InstanceFel = this;
            }
        }
        else
        {
            Delete();
        }

        _waypointsA = new List<WayPoint>();
        _waypointsB = new List<WayPoint>();

        Region = new VoidPoolRegion(this, map);
        Region.Register();

        _respawnMin = 60;
        _respawnMax = 90;

        Active = true;
    }

    public override void OnDoubleClick(Mobile from)
    {
        if (from.AccessLevel >= AccessLevel.GameMaster)
        {
            from.SendGump(new Gumps.PropertiesGump(from, this));
        }
    }

    private void OnTick()
    {
        if (!OnGoing && DateTime.UtcNow > NextStart && Region != null && Region.GetPlayerCount() > 0)
        {
            NextStart = DateTime.MaxValue;
            OnGoing = true;
            VoidType = (VoidType)Utility.Random(5);
            PoolHits = PoolStartHits;
            Wave = 0;

            CurrentScore?.Clear();

            if (Waves != null)
            {
                Waves.Clear();
                Waves.TrimExcess();
            }

            CurrentScore = new Dictionary<Mobile, long>();
            Waves = new List<WaveInfo>();

            // The battle for the Void Pool is beginning now!
            Region.SendRegionMessage(1152527, 0x2B);

            SpawnWave();
        }
        else if (OnGoing)
        {
            if (DateTime.UtcNow > NextWave)
            {
                SpawnWave();
            }

            if (Map == null)
            {
                return;
            }

            foreach (var bc in Map.GetMobilesInRange<BaseCreature>(PoolCenter, 7))
            {
                if (!OnGoing)
                {
                    break;
                }

                if (bc.Controlled || bc.Summoned)
                {
                    continue;
                }

                if (!PoolWalls.Contains(bc.Location))
                {
                    continue;
                }

                if (Utility.RandomDouble() > 0.25)
                {
                    OnVoidWallDamaged(bc);
                }
            }
        }
    }

    public void SpawnWave()
    {
        Wave++;

        // Wave ~1_WAVE~ approaches!
        Region.SendRegionMessage(1152528, Wave.ToString());

        var toSpawn = (int)Math.Ceiling(Math.Max(5, Math.Sqrt(Wave) * 2) * 1.5);
        var creatures = new List<BaseCreature>();

        for (var i = 0; i < toSpawn; i++)
        {
            var start = i % 2 == 0 ? StartPoint1 : StartPoint2;

            for (var j = 0; j < 25; j++)
            {
                var x = start.X + Utility.RandomMinMax(start.X - StartPointVariance / 2, start.X + StartPointVariance / 2);
                var y = start.Y + Utility.RandomMinMax(start.Y - StartPointVariance / 2, start.Y + StartPointVariance / 2);
                var z = Map.GetAverageZ(x, y);

                if (Map.CanSpawnMobile(x, y, z))
                {
                    start = new Point3D(x, y, z);
                    break;
                }
            }

            var ran = Utility.RandomMinMax(0, Stage < 10 ? 12 : Stage < 15 ? 14 : 15);
            Type t;

            switch (ran)
            {
                default:
                case 0:
                case 1:
                case 3:
                case 4:
                    {
                        t = SpawnTable[(int)VoidType][0];
                        break;
                    }
                case 5:
                case 6:
                case 7:
                case 8:
                    {
                        t = SpawnTable[(int)VoidType][1];
                        break;
                    }
                case 9:
                case 10:
                case 11:
                    {
                        t = SpawnTable[(int)VoidType][2];
                        break;
                    }
                case 12:
                case 13:
                    {
                        t = SpawnTable[(int)VoidType][3];
                        break;
                    }
                case 14:
                case 15:
                    {
                        t = SpawnTable[(int)VoidType][4];
                        break;
                    }
            }

            if (Activator.CreateInstance(t, Wave, true) is BaseCreature bc)
            {
                var spawnLocation = start;

                Timer.DelayCall(
                    TimeSpan.FromSeconds(i * 0.75),
                    () =>
                    {
                        if (OnGoing)
                        {
                            bc.MoveToWorld(spawnLocation, Map);
                            bc.Home = EndPoint;
                            bc.RangeHome = 1;

                            creatures.Add(bc);

                            bc.CurrentWayPoint = GetNearestWaypoint(bc);
                        }
                        else
                        {
                            bc.Delete();
                        }
                    }
                );
            }
        }

        var gate1 = new VoidPoolGate();
        gate1.MoveToWorld(StartPoint1, Map);
        Effects.PlaySound(StartPoint1, Map, 0x20E);

        var gate2 = new VoidPoolGate();
        gate2.MoveToWorld(StartPoint2, Map);
        Effects.PlaySound(StartPoint2, Map, 0x20E);

        Timer.DelayCall(
            TimeSpan.FromSeconds(toSpawn * 0.80),
            () =>
            {
                Effects.SendLocationParticles(
                    EffectItem.Create(gate1.Location, gate1.Map, EffectItem.DefaultDuration),
                    0x376A,
                    9,
                    20,
                    5042
                );
                Effects.PlaySound(gate1.GetWorldLocation(), gate1.Map, 0x201);

                Effects.SendLocationParticles(
                    EffectItem.Create(gate2.Location, gate2.Map, EffectItem.DefaultDuration),
                    0x376A,
                    9,
                    20,
                    5042
                );
                Effects.PlaySound(gate2.GetWorldLocation(), gate2.Map, 0x201);

                gate1.Delete();
                gate2.Delete();
            }
        );

        Waves.Add(new WaveInfo(Wave, creatures));
        NextWave = GetNextWaveTime();
    }

    public WayPoint GetNearestWaypoint(Mobile m, int range = 15)
    {
        var closestRange = 15;
        WayPoint closest = null;

        foreach (var wp in Map.GetItemsInRange<WayPoint>(m.Location, range))
        {
            var dist = (int)m.GetDistanceToSqrt(wp);

            if (dist < closestRange || closest == null)
            {
                closest = wp;
                closestRange = dist;
            }
        }

        return closest;
    }

    public Item GetNearestVoidPoolWall(Mobile m)
    {
        var closestRange = 5;
        Item closest = null;

        foreach (var item in Map.GetItemsInRange(m.Location, 5))
        {
            if (item.Name != "Void Pool")
            {
                continue;
            }

            var dist = (int)m.GetDistanceToSqrt(item);

            if (dist < closestRange || closest == null)
            {
                closest = item;
                closestRange = dist;
            }
        }

        return closest;
    }

    public DateTime GetNextWaveTime()
    {
        if (Wave == 1)
        {
            return DateTime.UtcNow + TimeSpan.FromSeconds(10);
        }

        var min = Math.Max(30, RespawnMin - Wave) + Utility.RandomMinMax(0, 10);
        var max = Math.Max(45, RespawnMax - Wave) - Utility.RandomMinMax(0, 5);

        return DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(min, max));
    }

    public void OnVoidWallDamaged(Mobile damager)
    {
        if (0.5 > Utility.RandomDouble())
        {
            PoolHits--;
        }

        // The Void Pool walls have been damaged! Defend the Void Pool!
        Region.SendRegionMessage(1152529);

        var item = GetNearestVoidPoolWall(damager);

        if (item != null)
        {
            var p = new Point3D(item.X, item.Y, item.Z + 5);
            Effects.SendLocationParticles(
                EffectItem.Create(p, item.Map, EffectItem.DefaultDuration),
                Utility.RandomList(0x36BD, 0x36B0, 0x3728),
                20,
                10,
                5044
            );
            Effects.PlaySound(p, item.Map, 0x307);
        }

        if (PoolHits <= 0 && OnGoing)
        {
            OnGoing = false;
            EndInvasion();
        }
    }

    public void EndInvasion()
    {
        // Cora's forces have destroyed the Void Pool walls. The battle is lost!
        Region.SendRegionMessage(1152530);

        NextStart = DateTime.UtcNow + TimeSpan.FromMinutes(RestartSpan);

        // The battle for the Void Pool will begin in ~1_VALUE~ minutes.
        Region.SendRegionMessage(1152526, RestartSpan.ToString());

        var players = Region.GetPlayers();

        for (var i = 0; i < players.Count; i++)
        {
            var m = players[i];

            Points.VoidPool.Instance?.AwardPoints(m, GetCurrentPoints(m));

            if (CurrentScore != null && CurrentScore.TryGetValue(m, out var score))
            {
                // During the battle, you helped fight back ~1_COUNT~ out of ~2_TOTAL~ waves of enemy forces.
                // Your final wave was ~3_MAX~. Your total score for the battle was ~4_SCORE~ points.
                m.SendLocalizedMessage(1152650, $"{GetTotalWaves(m)}\t{Wave}\t{Wave}\t{score}");
            }
        }

        ClearSpawn(true);
    }

    public void OnCreatureKilled(BaseCreature killed)
    {
        if (Waves == null)
        {
            return;
        }

        for (var w = 0; w < Waves.Count; w++)
        {
            var info = Waves[w];

            if (!info.Creatures.Contains(killed))
            {
                continue;
            }

            var list = BaseCreature.GetLootingRights(killed.DamageEntries, killed.HitsMax);
            list.Sort();

            for (var i = 0; i < list.Count; i++)
            {
                var ds = list[i];
                var m = ds.m_Mobile;

                if (ds.m_Mobile is BaseCreature damagerCreature && damagerCreature.GetMaster() is PlayerMobile master)
                {
                    m = master;
                }

                if (!info.Credit.Contains(m))
                {
                    info.Credit.Add(m);
                }

                if (CurrentScore != null)
                {
                    if (!CurrentScore.ContainsKey(m))
                    {
                        CurrentScore[m] = killed.Fame / 998;
                    }
                    else
                    {
                        CurrentScore[m] += killed.Fame / 998;
                    }
                }
            }

            list.Clear();
            list.TrimExcess();

            info.Creatures.Remove(killed);

            if (info.Creatures.Count == 0)
            {
                for (var c = 0; c < info.Credit.Count; c++)
                {
                    var m = info.Credit[c];

                    if (m.Region != Region || m is not PlayerMobile)
                    {
                        continue;
                    }

                    var award = Math.Max(0, Map == Map.Felucca ? Stage * 2 : Stage);

                    if (award > 0 && CurrentScore != null)
                    {
                        // Score bonus
                        if (!CurrentScore.ContainsKey(m))
                        {
                            CurrentScore[m] = Stage * 125;
                        }
                        else
                        {
                            CurrentScore[m] += Stage * 125;
                        }
                    }
                }
            }

            if (killed.Corpse is Corpse { Deleted: false } corpse)
            {
                corpse.BeginDecay(TimeSpan.FromMinutes(1));
            }
        }
    }

    public void ClearSpawn() => ClearSpawn(false);

    public void ClearSpawn(bool effects)
    {
        if (Region == null || Map == null)
        {
            return;
        }

        using var queue = PooledRefQueue<Mobile>.Create();

        foreach (var bc in Map.GetMobilesInRange<CovetousCreature>(PoolCenter, 60))
        {
            if (bc.Region == Region)
            {
                queue.Enqueue(bc);
            }
        }

        while (queue.Count > 0)
        {
            var m = queue.Dequeue();

            if (effects)
            {
                Effects.SendLocationEffect(m.Location, m.Map, 0xDDA, 30, 10, 0, 0);
            }

            m.Delete();
        }
    }

    public int GetCurrentPoints(Mobile from)
    {
        if (Waves == null)
        {
            return 0;
        }

        var points = 0;

        for (var i = 0; i < Waves.Count; i++)
        {
            if (Waves[i].Credit.Contains(from))
            {
                points += Map == Map.Felucca ? Stage * 2 : Stage;
            }
        }

        return points;
    }

    public int GetTotalWaves(Mobile from)
    {
        if (Waves == null)
        {
            return 0;
        }

        var count = 0;

        for (var i = 0; i < Waves.Count; i++)
        {
            if (Waves[i].Wave > 2 && Waves[i].Credit.Contains(from))
            {
                count++;
            }
        }

        return count;
    }

    public static int GetPlayerScore(Dictionary<Mobile, long> score, Mobile m)
    {
        if (score == null || m == null || !score.TryGetValue(m, out var value))
        {
            return 0;
        }

        return (int)value;
    }

    public static readonly Type[][] SpawnTable =
    {
        new[] { typeof(DaemonMongbat), typeof(GargoyleAssassin), typeof(CovetousDoppleganger), typeof(LesserOni), typeof(CovetousFireDaemon) },
        new[] { typeof(LizardmanWitchdoctor), typeof(OrcFootSoldier), typeof(RatmanAssassin), typeof(OgreBoneCrusher), typeof(TitanRockHunter) },
        new[] { typeof(AngeredSpirit), typeof(BoneSwordSlinger), typeof(VileCadaver), typeof(DiseasedLich), typeof(CovetousRevenant) },
        new[] { typeof(WarAlligator), typeof(MagmaLizard), typeof(ViciousDrake), typeof(CorruptedWyvern), typeof(CovetousWyrm) },
        new[] { typeof(CovetousEarthElemental), typeof(CovetousWaterElemental), typeof(VortexElemental), typeof(SearingElemental), typeof(VenomElemental) }
    };

    public override void OnDelete()
    {
        if (OnGoing)
        {
            EndInvasion();
        }

        if (Region != null)
        {
            Region.Unregister();
            Region = null;
        }

        _timerToken.Cancel();

        if (_waypointsA != null)
        {
            foreach (var wp in _waypointsA)
            {
                if (wp is { Deleted: false })
                {
                    wp.Delete();
                }
            }
        }

        if (_waypointsB != null)
        {
            foreach (var wp in _waypointsB)
            {
                if (wp is { Deleted: false })
                {
                    wp.Delete();
                }
            }
        }

        base.OnDelete();
    }

    [AfterDeserialization(false)]
    private void AfterDeserialization()
    {
        _waypointsA ??= new List<WayPoint>();
        _waypointsB ??= new List<WayPoint>();

        if (Map == Map.Felucca)
        {
            InstanceFel = this;
        }
        else
        {
            InstanceTram = this;
        }

        NextStart = DateTime.UtcNow;

        // Active was persisted; rebuild the region + timer if the invasion controller was running.
        if (_activeState)
        {
            Active = true;
        }

        Timer.DelayCall(TimeSpan.FromSeconds(10), ClearSpawn);
    }
}
