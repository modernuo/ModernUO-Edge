// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Creatures/Abyss.cs
using ModernUO.Serialization;

namespace Server.Mobiles;

[CorpseName("a mongbat corpse")]
[SerializationGenerator(0, false)]
public partial class DaemonMongbat : CovetousCreature
{
    [Constructible]
    public DaemonMongbat()
        : base(AIType.AI_Mage) // VP-MAP: AI_Necro → AI_Mage (AI_Necro not in Edge)
    {
        Name = "a dameon mongbat";
        Body = 39;
        BaseSoundID = 422;
    }

    [Constructible]
    public DaemonMongbat(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "a dameon mongbat";
        Body = 39;
        BaseSoundID = 422;
    }
}

[CorpseName("a gargoyle corpse")]
[SerializationGenerator(0, false)]
public partial class GargoyleAssassin : CovetousCreature
{
    [Constructible]
    public GargoyleAssassin()
        : base(AIType.AI_Mage)
    {
        Name = "a gargoyle assassin";
        Body = 0x4;
        BaseSoundID = 0x174;
    }

    [Constructible]
    public GargoyleAssassin(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a gargoyle assassin";
        Body = 0x4;
        BaseSoundID = 0x174;
    }
}

[CorpseName("a doppleganger corpse")]
[SerializationGenerator(0, false)]
public partial class CovetousDoppleganger : CovetousCreature
{
    [Constructible]
    public CovetousDoppleganger()
        : base(AIType.AI_Melee)
    {
        Name = "a doppleganger";
        Body = 0x309;
        BaseSoundID = 0x451;
    }

    [Constructible]
    public CovetousDoppleganger(int level, bool voidSpawn)
        : base(AIType.AI_Melee, level, voidSpawn)
    {
        Name = "a doppleganger";
        Body = 0x309;
        BaseSoundID = 0x451;
    }
}

[CorpseName("an oni corpse")]
[SerializationGenerator(0, false)]
public partial class LesserOni : CovetousCreature
{
    [Constructible]
    public LesserOni()
        : base(AIType.AI_Mage)
    {
        Name = "a lesser oni";
        Body = 241;
        // VP-DEFER: SpecialAbility.AngryFire not in Edge (backlog)
    }

    [Constructible]
    public LesserOni(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a lesser oni";
        Body = 241;
    }

    public override int GetAngerSound() { return 0x4E3; }
    public override int GetIdleSound() { return 0x4E2; }
    public override int GetAttackSound() { return 0x4E1; }
    public override int GetHurtSound() { return 0x4E4; }
    public override int GetDeathSound() { return 0x4E0; }
}

[CorpseName("a fire daemon corpse")]
[SerializationGenerator(0, false)]
public partial class CovetousFireDaemon : CovetousCreature
{
    [Constructible]
    public CovetousFireDaemon()
        : base(AIType.AI_Mage)
    {
        Name = "a fire daemon";
        Body = 9;
        BaseSoundID = 357;
    }

    [Constructible]
    public CovetousFireDaemon(int level, bool voidSpawn)
        : base(AIType.AI_Mage, level, voidSpawn)
    {
        Name = "a fire daemon";
        Body = 9;
        BaseSoundID = 357;
    }
}
