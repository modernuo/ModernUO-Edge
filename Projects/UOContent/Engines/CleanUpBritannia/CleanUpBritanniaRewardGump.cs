using System;
using Server.Engines.Points;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.CleanUpBritannia;

public sealed class CleanUpBritanniaRewardGump : DynamicGump
{
    private const int RowHeight = 75;
    private const int RowTop = 90;

    private readonly PlayerMobile _user;
    private readonly Mobile _owner;

    public override bool Singleton => true;

    private CleanUpBritanniaRewardGump(PlayerMobile user, Mobile owner) : base(50, 50)
    {
        _user = user;
        _owner = owner;
    }

    public static void DisplayTo(PlayerMobile pm, Mobile owner)
    {
        if (pm?.NetState == null || !CleanUpBritanniaData.Enabled || CleanUpBritanniaData.Instance == null)
        {
            return;
        }

        pm.SendGump(new CleanUpBritanniaRewardGump(pm, owner));
    }

    protected override void BuildLayout(ref DynamicGumpBuilder builder)
    {
        var rewards = CleanUpBritanniaRewards.Rewards;
        var balance = (int)CleanUpBritanniaData.Instance.GetPoints(_user);

        var height = RowTop + rewards.Length * RowHeight + 20;

        builder.AddPage(0);
        builder.AddBackground(0, 0, 450, height, 0x2454);

        // Clean Up Britannia (title)
        builder.AddHtmlLocalized(10, 15, 430, 18, 1151316, 0x0, false, false);

        // Your Points: ~1_VAL~
        builder.AddHtmlLocalized(10, 45, 430, 18, 1151318, $"{balance}", 0x0, false, false);

        var y = RowTop;

        for (var i = 0; i < rewards.Length; i++)
        {
            var reward = rewards[i];

            builder.AddItem(30, y, reward.ItemId, reward.Hue);
            builder.AddHtmlLocalized(110, y + 10, 250, 36, reward.Cliloc, 0x0, false, false);

            // Cost label
            builder.AddLabel(110, y + 45, 0x480, $"{(int)reward.Cost}");

            if (balance >= reward.Cost)
            {
                // Select button; ButtonID = reward index + 1 (0 reserved for close)
                builder.AddButton(370, y + 20, 0xFA5, 0xFA7, i + 1, GumpButtonType.Reply, 0);
            }

            y += RowHeight;
        }
    }

    public override void OnResponse(NetState sender, in RelayInfo info)
    {
        if (info.ButtonID == 0 || sender.Mobile is not PlayerMobile pm)
        {
            return;
        }

        var index = info.ButtonID - 1;
        var rewards = CleanUpBritanniaRewards.Rewards;

        if (index < 0 || index >= rewards.Length)
        {
            return;
        }

        pm.SendGump(new CleanUpBritanniaConfirmGump(pm, _owner, index));
    }
}

public sealed class CleanUpBritanniaConfirmGump : BaseConfirmGump
{
    private readonly PlayerMobile _user;
    private readonly Mobile _owner;
    private readonly int _index;

    public CleanUpBritanniaConfirmGump(PlayerMobile user, Mobile owner, int index)
    {
        _user = user;
        _owner = owner;
        _index = index;
    }

    public override void Confirm(Mobile from)
    {
        if (from is not PlayerMobile pm || !CleanUpBritanniaData.Enabled || CleanUpBritanniaData.Instance == null)
        {
            return;
        }

        var rewards = CleanUpBritanniaRewards.Rewards;

        if (_index < 0 || _index >= rewards.Length)
        {
            return;
        }

        var reward = rewards[_index];

        if (!from.InRange(_owner, 5))
        {
            return;
        }

        var data = CleanUpBritanniaData.Instance;

        if (data.GetPoints(pm) < reward.Cost)
        {
            return;
        }

        var backpack = pm.Backpack;

        if (backpack == null)
        {
            return;
        }

        Item item;

        // ScrollofAlacrity has no true parameterless ctor (only an optional-arg one), which
        // Activator.CreateInstance cannot satisfy, so construct it directly and randomize its skill.
        if (reward.Type == typeof(ScrollofAlacrity))
        {
            item = new ScrollofAlacrity { Skill = (SkillName)Utility.Random(SkillInfo.Table.Length) };
        }
        else if (Activator.CreateInstance(reward.Type) is Item created)
        {
            item = created;
        }
        else
        {
            return;
        }

        if (backpack.TryDropItem(pm, item, false))
        {
            if (!data.DeductPoints(pm, reward.Cost))
            {
                item.Delete(); // undo the just-delivered item
                pm.SendLocalizedMessage(1074361); // The reward could not be given. Make sure you have room in your pack.
                return;
            }

            pm.SendLocalizedMessage(1073621); // Your reward has been placed in your backpack.
            pm.PlaySound(0x5A7);

            CleanUpBritanniaRewardGump.DisplayTo(pm, _owner);
        }
        else
        {
            pm.SendLocalizedMessage(1074361); // The reward could not be given. Make sure you have room in your pack.
            item.Delete();
        }
    }

    public override void Refuse(Mobile from)
    {
        if (from is PlayerMobile pm)
        {
            CleanUpBritanniaRewardGump.DisplayTo(pm, _owner);
        }
    }
}
