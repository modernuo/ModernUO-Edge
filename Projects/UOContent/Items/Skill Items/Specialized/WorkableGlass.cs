// Source: ServUO Scripts/Items/Resource/WorkableGlass.cs
using ModernUO.Serialization;

namespace Server.Items;

[SerializationGenerator(0)]
public partial class WorkableGlass : Item, ICommodity
{
    [Constructible]
    public WorkableGlass(int amount = 1) : base(0x4B80)
    {
        Stackable = true;
        Amount = amount;
    }

    public override double DefaultWeight => 1.0;

    public override int LabelNumber => 1154170; // workable glass

    int ICommodity.DescriptionNumber => LabelNumber;
    bool ICommodity.IsDeedable => true;
}
