using ModernUO.Serialization;

namespace Server.Items;

[SerializationGenerator(0)]
public partial class Bamboo : Item
{
    public override int LabelNumber => 1029324; // bamboo

    [Constructible]
    public Bamboo() : base(0x246D) => Weight = 10;
}

[SerializationGenerator(0)]
public partial class NestWithEggs : Item
{
    public override int LabelNumber => 1026868; // nest with eggs

    [Constructible]
    public NestWithEggs() : base(0x1AD4)
    {
        Hue = 2415;
        Weight = 2;
    }
}

[SerializationGenerator(0)]
public partial class TableLamp : Item
{
    public override int LabelNumber => 1151220; // table lamp

    [Constructible]
    public TableLamp() : base(0x49C2) => Weight = 1;

    public override void OnDoubleClick(Mobile from)
    {
        ItemID = ItemID == 0x49C2 ? 0x49C1 : 0x49C2;
    }
}

[SerializationGenerator(0)]
public partial class LillyPad : Item
{
    [Constructible]
    public LillyPad() : base(0xDBC) => Weight = 1.0;
}

[SerializationGenerator(0)]
public partial class LillyPads : Item
{
    [Constructible]
    public LillyPads() : base(0xDBE) => Weight = 1.0;
}
