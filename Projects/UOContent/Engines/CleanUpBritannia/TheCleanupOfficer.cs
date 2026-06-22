using System.Collections.Generic;
using ModernUO.Serialization;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.CleanUpBritannia;

[SerializationGenerator(0)]
public partial class TheCleanupOfficer : BaseVendor
{
    private readonly List<SBInfo> _sbInfos = new();
    protected override List<SBInfo> SBInfos => _sbInfos;

    public override bool IsActiveVendor => false;
    public override bool DisallowAllMoves => true;

    [Constructible]
    public TheCleanupOfficer() : base("the Cleanup Officer")
    {
    }

    public override void InitSBInfo()
    {
    }

    public override void InitBody()
    {
        InitStats(100, 100, 25);
        Name = NameList.RandomName("male");
        Body = 0x190;
        Hue = Race.Human.RandomSkinHue();
        HairItemID = 0x2044;
        HairHue = 1644;
        FacialHairItemID = 0x203F;
        FacialHairHue = 1644;
    }

    public override void InitOutfit()
    {
        AddItem(new Cloak(337) { Movable = false });
        AddItem(new ThighBoots { Movable = false });
        AddItem(new LongPants(1409) { Movable = false });
        AddItem(new Doublet(50) { Movable = false });
        AddItem(new FancyShirt(1644) { Movable = false });
        AddItem(new Necklace { Movable = false });
    }

    public override void GetProperties(IPropertyList list)
    {
        base.GetProperties(list);
        list.Add(1151317); // Clean Up Britannia Reward Trader
    }

    public override void OnDoubleClick(Mobile from)
    {
        if (from is PlayerMobile pm && from.InRange(Location, 5))
        {
            CleanUpBritanniaRewardGump.DisplayTo(pm, this);
        }
    }
}
