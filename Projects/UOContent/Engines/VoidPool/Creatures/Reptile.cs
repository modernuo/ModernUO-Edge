// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Creatures/Reptile.cs
using ModernUO.Serialization;

namespace Server.Mobiles;

[CorpseName("an alligator corpse")]
[SerializationGenerator(0, false)]
public partial class WarAlligator : CovetousCreature
{
    [Constructible]
    public WarAlligator()
        : base(AIType.AI_Melee)
    {
        Name = "a war alligator";
        Body = 0xCA;
        BaseSoundID = 660;
    }

    [Constructible]
    public WarAlligator(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "a war alligator";
        Body = 0xCA;
        BaseSoundID = 660;
    }
}

[CorpseName("a magma lizard corpse")]
[SerializationGenerator(0, false)]
public partial class MagmaLizard : CovetousCreature
{
    [Constructible]
    public MagmaLizard()
        : base(AIType.AI_Melee)
    {
        Name = "a magma lizard";
        Body = 0xCE;
        Hue = Utility.RandomList(0x647, 0x650, 0x659, 0x662, 0x66B, 0x674);
        BaseSoundID = 0x5A;
    }

    [Constructible]
    public MagmaLizard(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "a magma lizard";
        Body = 0xCE;
        Hue = Utility.RandomList(0x647, 0x650, 0x659, 0x662, 0x66B, 0x674);
        BaseSoundID = 0x5A;
    }
}

[CorpseName("a drake corpse")]
[SerializationGenerator(0, false)]
public partial class ViciousDrake : CovetousCreature
{
    [Constructible]
    public ViciousDrake()
        : base(AIType.AI_Melee)
    {
        Name = "a vicious drake";
        Body = Utility.RandomList(60, 61);
        BaseSoundID = 362;
    }

    [Constructible]
    public ViciousDrake(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "a vicious drake";
        Body = Utility.RandomList(60, 61);
        BaseSoundID = 362;
    }
}

[CorpseName("a wyvern corpse")]
[SerializationGenerator(0, false)]
public partial class CorruptedWyvern : CovetousCreature
{
    [Constructible]
    public CorruptedWyvern()
        : base(AIType.AI_Mage)
    {
        Name = "a corrupted wyvern";
        Body = 62;
        BaseSoundID = 362;
    }

    [Constructible]
    public CorruptedWyvern(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a corrupted wyvern";
        Body = 62;
        BaseSoundID = 362;
    }
}

[CorpseName("a covetous wyrm corpse")]
[SerializationGenerator(0, false)]
public partial class CovetousWyrm : CovetousCreature
{
    [Constructible]
    public CovetousWyrm()
        : base(AIType.AI_Mage) // VP-MAP: AI_Necro → AI_Mage (AI_Necro not in Edge)
    {
        Name = "a covetous wyrm";
        Body = 106;
        BaseSoundID = 362;
    }

    [Constructible]
    public CovetousWyrm(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a covetous wyrm";
        Body = 106;
        BaseSoundID = 362;
    }
}
