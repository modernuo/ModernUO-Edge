// Source: ServUO Scripts/Items/Consumables/BaseRewardTitleDeed.cs
using ModernUO.Serialization;
using Server.Engines.RewardTitles;
using Server.Mobiles;

namespace Server.Items;

[SerializationGenerator(0)]
public abstract partial class BaseRewardTitleDeed : Item
{
    public override int LabelNumber => 1155604; // A Deed for a Reward Title

    public abstract TextDefinition Title { get; }

    public BaseRewardTitleDeed() : base(5360)
    {
    }

    public override void OnDoubleClick(Mobile from)
    {
        if (from is not PlayerMobile pm)
        {
            return;
        }

        if (!IsChildOf(pm.Backpack))
        {
            pm.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            return;
        }

        if (Title.IsEmpty)
        {
            return;
        }

        if (RewardTitleSystem.AddTitle(pm, Title))
        {
            pm.SendLocalizedMessage(1155605, Title.ToString()); // Thou hath been bestowed the title ~1_TITLE~!
            Delete();
        }
        else
        {
            pm.SendLocalizedMessage(1073626); // You already have that title!
        }
    }

    public override void GetProperties(IPropertyList list)
    {
        base.GetProperties(list);
        list.Add(1114057, Title.ToString()); // ~1_NOTHING~
    }
}
