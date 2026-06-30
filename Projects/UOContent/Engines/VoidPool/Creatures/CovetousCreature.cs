// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Creatures/CovetousCreature.cs
using System;
using ModernUO.Serialization;
using Server.Items;

namespace Server.Mobiles;

[SerializationGenerator(0, false)]
public abstract partial class CovetousCreature : BaseCreature
{
    [SerializableField(0)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private int _level;

    [SerializableField(1)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private bool _voidSpawn;

    public virtual int Stage => Math.Max(1, Level / 5);
    public virtual int MaxStage => 15;

    public virtual int StatRatio => Utility.RandomMinMax(35, 60);

    public virtual double SkillStart => Utility.RandomMinMax(35, 50);
    public virtual double SkillMax => 160.0;

    public virtual int StrStart => Utility.RandomMinMax(91, 100);
    public virtual int DexStart => Utility.RandomMinMax(91, 100);
    public virtual int IntStart => IsMagical ? Utility.RandomMinMax(91, 100) : 1;

    public virtual int StrMax => 410;
    public virtual int DexMax => 422;
    public virtual int IntMax => 250;

    public virtual int MaxHits => 2400;
    public virtual int MaxStam => 3000;
    public virtual int MaxMana => IsMagical ? 8500 : 1500;

    public virtual int MinDamMax => 5;
    public virtual int MaxDamMax => 12;

    public virtual int MinDamStart => 5;
    public virtual int MaxDamStart => 15;

    public virtual int HitsStart => StrStart + (int)(StrStart * (StatRatio / 100.0));
    public virtual int StamStart => DexStart + (int)(DexStart * (StatRatio / 100.0));
    public virtual int ManaStart => IntStart + (int)(IntStart * (StatRatio / 100.0));

    public virtual bool RaiseDamage => true;
    public virtual double RaiseDamageFactor => 0.33;

    public virtual int ResistStart => 25;
    public virtual int ResistMax => 95;

    public virtual bool IsMagical => AIObject is MageAI;

    public override bool PlayerRangeSensitive => false;
    public override bool CanDestroyObstacles => true;

    private WayPoint _timeOnWayPointWayPoint;
    private DateTime _timeOnWayPointExpiry;

    protected CovetousCreature(AIType ai, int level = 60, bool voidSpawn = false)
        : base(ai, FightMode.Closest, 10, 1)
    {
        _level = level;
        _voidSpawn = voidSpawn;
        NoKillAwards = true;

        SetSkill(SkillName.MagicResist, SkillStart);
        SetSkill(SkillName.Tactics, SkillStart);
        SetSkill(SkillName.Wrestling, SkillStart);
        SetSkill(SkillName.Anatomy, SkillStart);

        switch (ai)
        {
            case AIType.AI_Mage:
            {
                SetSkill(SkillName.Magery, SkillStart);
                SetSkill(SkillName.EvalInt, SkillStart);
                SetSkill(SkillName.Meditation, SkillStart);
                break;
            }
        }

        SetStr(StrStart);
        SetDex(DexStart);
        SetInt(IntStart);

        SetHits(HitsStart);
        SetStam(StamStart);
        SetMana(ManaStart);

        SetDamage(MinDamStart, MaxDamStart);

        SetDamageType(ResistanceType.Physical, 100);

        SetResistance(ResistanceType.Physical, ResistStart - 5, ResistStart + 5);
        SetResistance(ResistanceType.Fire, ResistStart - 5, ResistStart + 5);
        SetResistance(ResistanceType.Cold, ResistStart - 5, ResistStart + 5);
        SetResistance(ResistanceType.Poison, ResistStart - 5, ResistStart + 5);
        SetResistance(ResistanceType.Energy, ResistStart - 5, ResistStart + 5);

        if (Stage > 1)
        {
            Timer.DelayCall(TimeSpan.FromSeconds(0.5), SetPower);
        }

        Fame = Math.Min(8500, Level * 142);
        Karma = Math.Min(8500, Level * 142) * -1;
    }

    public override void OnThink()
    {
        base.OnThink();

        if (!Alive)
        {
            return;
        }

        if (_timeOnWayPointWayPoint == null && CurrentWayPoint != null)
        {
            _timeOnWayPointWayPoint = CurrentWayPoint;
            _timeOnWayPointExpiry = DateTime.UtcNow + TimeSpan.FromMinutes(2);
        }
        else if (_timeOnWayPointWayPoint != null && _timeOnWayPointWayPoint == CurrentWayPoint && _timeOnWayPointExpiry < DateTime.UtcNow)
        {
            if (CheckCanTeleport())
            {
                MoveToWorld(CurrentWayPoint.Location, Map);
            }
        }
        else if (_timeOnWayPointWayPoint != null && _timeOnWayPointWayPoint != CurrentWayPoint)
        {
            _timeOnWayPointWayPoint = CurrentWayPoint;
            _timeOnWayPointExpiry = DateTime.UtcNow + TimeSpan.FromMinutes(2);
        }
    }

    protected virtual bool CheckCanTeleport()
    {
        if (CurrentWayPoint == null || Frozen || Paralyzed || Combatant?.InLOS(this) == true)
        {
            return false;
        }

        foreach (var m in Map.GetMobilesInRange(Location, 10))
        {
            if (m is PlayerMobile && m.AccessLevel < AccessLevel.Counselor)
            {
                return false;
            }
        }

        foreach (var item in Map.GetItemsInRange(Location, 8))
        {
            var id = item.ItemID;
            if (id == 0x82 || id == 0x3946 || id == 0x3956 || id == 0x3967 || id == 0x3979)
            {
                return false;
            }
        }

        return true;
    }

    public override void GenerateLoot()
    {
        if (!VoidSpawn)
        {
            AddLoot(LootPack.Rich, Math.Max(1, Stage / 2));
        }
    }

    public virtual void SetPower()
    {
        foreach (var skill in Skills)
        {
            if (skill != null && skill.Base > 0 && skill.Base < SkillMax)
            {
                var toRaise = (SkillMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5);

                if (toRaise > skill.Base)
                {
                    skill.Base = Math.Min(SkillMax, toRaise);
                }
            }
        }

        SetResistance(ResistanceType.Physical, (ResistMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5));
        SetResistance(ResistanceType.Fire, (ResistMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5));
        SetResistance(ResistanceType.Cold, (ResistMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5));
        SetResistance(ResistanceType.Poison, (ResistMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5));
        SetResistance(ResistanceType.Energy, (ResistMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5));

        var strRaise = (StrMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5);
        var dexRaise = (DexMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5);
        var intRaise = (IntMax / MaxStage * Stage) + Utility.RandomMinMax(-5, 5);

        if (strRaise > RawStr)
        {
            SetStr(Math.Min(StrMax, strRaise));
        }

        if (dexRaise > RawDex)
        {
            SetDex(Math.Min(DexMax, dexRaise));
        }

        if (intRaise > RawInt)
        {
            SetInt(Math.Min(IntMax, intRaise));
        }

        var hitsRaise = (MaxHits / 60 * Level) + Utility.RandomMinMax(-5, 5);
        var stamRaise = (MaxStam / 60 * Level) + Utility.RandomMinMax(-5, 5);
        var manaRaise = (MaxMana / 60 * Level) + Utility.RandomMinMax(-5, 5);

        if (hitsRaise > HitsMax)
        {
            SetHits(Math.Min(MaxHits, hitsRaise));
        }

        if (stamRaise > StamMax)
        {
            SetStam(Math.Min(MaxStam, stamRaise));
        }

        if (manaRaise > ManaMax)
        {
            SetMana(Math.Min(MaxMana, manaRaise));
        }

        if (RaiseDamage && Utility.RandomDouble() < RaiseDamageFactor)
        {
            DamageMin = Math.Min(MinDamMax, DamageMin + 1);
            DamageMax = Math.Min(MaxDamMax, DamageMax + 1);
        }
    }
}
