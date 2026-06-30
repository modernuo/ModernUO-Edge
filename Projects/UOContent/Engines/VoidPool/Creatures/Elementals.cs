// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Creatures/Elementals.cs
using ModernUO.Serialization;

namespace Server.Mobiles;

[CorpseName("an earth elemental corpse")]
[SerializationGenerator(0, false)]
public partial class CovetousEarthElemental : CovetousCreature
{
    [Constructible]
    public CovetousEarthElemental()
        : base(AIType.AI_Melee)
    {
        Name = "an earth elemental";
        Body = 14;
        BaseSoundID = 268;
    }

    [Constructible]
    public CovetousEarthElemental(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "an earth elemental";
        Body = 14;
        BaseSoundID = 268;
    }
}

[CorpseName("a water elemental corpse")]
[SerializationGenerator(0, false)]
public partial class CovetousWaterElemental : CovetousCreature
{
    [Constructible]
    public CovetousWaterElemental()
        : base(AIType.AI_Mage)
    {
        Name = "a water elemental";
        Body = 16;
        BaseSoundID = 278;
    }

    [Constructible]
    public CovetousWaterElemental(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn) // VP-NOTE: source parameterized ctor uses AI_Melee; matches ServUO verbatim
    {
        Name = "a water elemental";
        Body = 16;
        BaseSoundID = 278;
    }
}

[CorpseName("a vortex elemental corpse")]
[SerializationGenerator(0, false)]
public partial class VortexElemental : CovetousCreature
{
    [Constructible]
    public VortexElemental()
        : base(AIType.AI_Melee)
    {
        Name = "a vortex elemental";
        Body = 13;
        Hue = 0x4001;
        BaseSoundID = 655;
    }

    [Constructible]
    public VortexElemental(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "a vortex elemental";
        Body = 13;
        Hue = 0x4001;
        BaseSoundID = 655;
    }
}

[CorpseName("a searing elemental corpse")]
[SerializationGenerator(0, false)]
public partial class SearingElemental : CovetousCreature
{
    [Constructible]
    public SearingElemental()
        : base(AIType.AI_Mage)
    {
        Name = "a searing elemental";
        Body = 15;
        BaseSoundID = 838;
    }

    [Constructible]
    public SearingElemental(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a searing elemental";
        Body = 15;
        BaseSoundID = 838;
    }
}

[CorpseName("a venom elemental corpse")]
[SerializationGenerator(0, false)]
public partial class VenomElemental : CovetousCreature
{
    [Constructible]
    public VenomElemental()
        : base(AIType.AI_Mage)
    {
        Name = "a venom elemental";
        Body = 162;
        BaseSoundID = 263;
    }

    [Constructible]
    public VenomElemental(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a venom elemental";
        Body = 162;
        BaseSoundID = 263;
    }

    public override bool BleedImmune => true;
    public override Poison PoisonImmune => Poison.Lethal;
    public override Poison HitPoison => Poison.Lethal;
    public override double HitPoisonChance => 0.75;
    public override int TreasureMapLevel => 5;
}
