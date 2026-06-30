// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Creatures/Repond.cs
using ModernUO.Serialization;

namespace Server.Mobiles;

[CorpseName("a lizardman corpse")]
[SerializationGenerator(0, false)]
public partial class LizardmanWitchdoctor : CovetousCreature
{
    [Constructible]
    public LizardmanWitchdoctor()
        : base(AIType.AI_Mage)
    {
        Name = "a lizardman witchdoctor";
        Body = Utility.RandomList(35, 36);
        BaseSoundID = 417;
    }

    [Constructible]
    public LizardmanWitchdoctor(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a lizardman witchdoctor";
        Body = Utility.RandomList(35, 36);
        BaseSoundID = 417;
    }
}

[CorpseName("an orcish corpse")]
[SerializationGenerator(0, false)]
public partial class OrcFootSoldier : CovetousCreature
{
    [Constructible]
    public OrcFootSoldier()
        : base(AIType.AI_Melee)
    {
        Name = "an orc foot soldier";
        Body = 17;
        BaseSoundID = 0x45A;
    }

    [Constructible]
    public OrcFootSoldier(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "an orc foot soldier";
        Body = 17;
        BaseSoundID = 0x45A;
    }
}

[CorpseName("a ratman corpse")]
[SerializationGenerator(0, false)]
public partial class RatmanAssassin : CovetousCreature
{
    [Constructible]
    public RatmanAssassin()
        : base(AIType.AI_Melee)
    {
        Name = "ratman assassin";
        Body = 42;
        BaseSoundID = 437;
    }

    [Constructible]
    public RatmanAssassin(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "ratman assassin";
        Body = 42;
        BaseSoundID = 437;
    }
}

[CorpseName("an ogre corpse")]
[SerializationGenerator(0, false)]
public partial class OgreBoneCrusher : CovetousCreature
{
    [Constructible]
    public OgreBoneCrusher()
        : base(AIType.AI_Melee)
    {
        Name = "an ogre bone crusher";
        Body = 1;
        BaseSoundID = 427;
    }

    [Constructible]
    public OgreBoneCrusher(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "an ogre bone crusher";
        Body = 1;
        BaseSoundID = 427;
    }
}

[CorpseName("a titan corpse")]
[SerializationGenerator(0, false)]
public partial class TitanRockHunter : CovetousCreature
{
    [Constructible]
    public TitanRockHunter()
        : base(AIType.AI_Mage)
    {
        Name = "a titan rockhurler";
        Body = 76;
        BaseSoundID = 609;
    }

    [Constructible]
    public TitanRockHunter(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a titan rockhurler";
        Body = 76;
        BaseSoundID = 609;
    }
}
