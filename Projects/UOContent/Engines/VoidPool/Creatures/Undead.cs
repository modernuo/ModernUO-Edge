// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Creatures/Undead.cs
using ModernUO.Serialization;
using Server.Items;

namespace Server.Mobiles;

[CorpseName("an angered spirit corpse")]
[SerializationGenerator(0, false)]
public partial class AngeredSpirit : CovetousCreature
{
    [Constructible]
    public AngeredSpirit()
        : base(AIType.AI_Mage)
    {
        Name = "an angered spirit";
        Body = 3;
        BaseSoundID = 471;
    }

    [Constructible]
    public AngeredSpirit(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "an angered spirit";
        Body = 3;
        BaseSoundID = 471;
    }
}

[CorpseName("a bone swordslinger corpse")]
[SerializationGenerator(0, false)]
public partial class BoneSwordSlinger : CovetousCreature
{
    [Constructible]
    public BoneSwordSlinger()
        : base(AIType.AI_Melee)
    {
        Name = "a bone swordslinger";
        Body = 147;
        BaseSoundID = 451;
    }

    [Constructible]
    public BoneSwordSlinger(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "a bone sword slinger";
        Body = 147;
        BaseSoundID = 451;
    }
}

[CorpseName("a vile cadaver")]
[SerializationGenerator(0, false)]
public partial class VileCadaver : CovetousCreature
{
    [Constructible]
    public VileCadaver()
        : base(AIType.AI_Melee)
    {
        Name = "a vile cadaver";
        Body = 154;
        BaseSoundID = 471;
    }

    [Constructible]
    public VileCadaver(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "a vile cadaver";
        Body = 154;
        BaseSoundID = 471;
    }
}

[CorpseName("a liche's corpse")]
[SerializationGenerator(0, false)]
public partial class DiseasedLich : CovetousCreature
{
    [Constructible]
    public DiseasedLich()
        : base(AIType.AI_Mage)
    {
        Name = "a diseased lich";
        Body = 24;
        BaseSoundID = 0x3E9;
    }

    [Constructible]
    public DiseasedLich(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a diseased lich";
        Body = 24;
        BaseSoundID = 0x3E9;
    }
}

[CorpseName("a revenant corpse")]
[SerializationGenerator(0, false)]
public partial class CovetousRevenant : CovetousCreature
{
    public override bool AlwaysMurderer => true;

    [Constructible]
    public CovetousRevenant()
        : base(AIType.AI_Mage)
    {
        Name = "a covetous revenant";
        Body = 400;
        Hue = 0x847E;

        // VP-ADAPT: ServUO used SetWearable (not in Edge); using AddItem instead.
        var shroud = new Robe
        {
            ItemID = 0x2683,
            Hue = 0x4001,
            Movable = false
        };
        AddItem(shroud);

        var boots = new Boots
        {
            Hue = 0x4001,
            Movable = false
        };
        AddItem(boots);
    }

    [Constructible]
    public CovetousRevenant(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a covetous revenant";
        Body = 400;
        // BaseSoundID intentionally omitted (ServUO TODO comment)
    }
}
