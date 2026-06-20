using ModernUO.Serialization;
using Server.Collections;
using Server.ContextMenus;
using Server.Engines.CleanUpBritannia;
using Server.Engines.Points;
using Server.Mobiles;

namespace Server.Items;

[Flippable(0xE41, 0xE40)]
[SerializationGenerator(0, false)]
public partial class TrashChest : Container
{
    [Constructible]
    public TrashChest() : base(0xE41) => Movable = false;

    public override int DefaultMaxWeight => 0; // A value of 0 signals unlimited weight

    public override bool IsDecoContainer => false;

    public override void GetContextMenuEntries(Mobile from, ref PooledRefList<ContextMenuEntry> list)
    {
        base.GetContextMenuEntries(from, ref list);

        if (CleanUpBritanniaData.Enabled && from is PlayerMobile)
        {
            list.Add(new AppraiseForCleanupEntry(from));
        }
    }

    public override bool OnDragDrop(Mobile from, Item dropped)
    {
        if (base.OnDragDrop(from, dropped))
        {
            PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.Random(1042891, 8));

            if (CleanUpBritanniaData.Enabled && from is PlayerMobile player)
            {
                var points = CleanUpBritanniaData.GetPoints(dropped);
                if (points > 0)
                {
                    CleanUpBritanniaData.Instance?.AwardPoints(player, points, false, false);
                    // You have received approximately ~1_VALUE~ points for turning in ~2_COUNT~ items for Clean Up Britannia.
                    player.SendLocalizedMessage(1151280, $"{(int)points}\t1");
                }
            }

            dropped.Delete();
            return true;
        }

        return false;
    }

    public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
    {
        if (base.OnDragDropInto(from, item, p))
        {
            PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.Random(1042891, 8));

            if (CleanUpBritanniaData.Enabled && from is PlayerMobile player)
            {
                var points = CleanUpBritanniaData.GetPoints(item);
                if (points > 0)
                {
                    CleanUpBritanniaData.Instance?.AwardPoints(player, points, false, false);
                    // You have received approximately ~1_VALUE~ points for turning in ~2_COUNT~ items for Clean Up Britannia.
                    player.SendLocalizedMessage(1151280, $"{(int)points}\t1");
                }
            }

            item.Delete();
            return true;
        }

        return false;
    }
}
