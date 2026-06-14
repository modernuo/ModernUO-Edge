// Source: ServUO Scripts/Gumps/TitlesMenu.cs
//
// Edge port of the TitleType x TitleCategory titles matrix gump. Only the two
// categories Edge can back today are wired:
//   - RewardTitles (fully functional: list/select/hide reward titles)
//   - Champion     (display toggle for the champion kill title)
// The remaining categories (FameKarma/Skills/Guild/Veteran) are deferred: their
// category buttons only render when a data source exists, so they stay hidden.
//
// ServUO's ButtonCallbacks lambda-dictionary + FirstOrDefault response path is
// rewritten here as a switch (info.ButtonID) dispatcher (the LINQ form violates
// Edge audit rules).

using Server.ContextMenus;
using Server.Engines.CannedEvil;
using Server.Engines.RewardTitles;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps;

public enum TitleType
{
    None,
    PaperdollPrefix,
    PaperdollSuffix,
    OverheadName,
    SubTitles
}

public enum TitleCategory
{
    None,
    FameKarma,
    Skills,
    Guild,
    Champion,
    RewardTitles,
    Veteran
}

public class TitlesGump : DynamicGump
{
    private readonly PlayerMobile _player;
    private TitleType _type;
    private TitleCategory _category;
    private int _page;

    public override bool Singleton => true;

    private TitlesGump(PlayerMobile pm) : base(50, 50)
    {
        _player = pm;
        _type = TitleType.SubTitles;
        _category = TitleCategory.RewardTitles;
        _page = 0;
    }

    public static void DisplayTo(PlayerMobile pm)
    {
        if (pm?.NetState == null)
        {
            return;
        }

        pm.SendGump(new TitlesGump(pm));
    }

    protected override void BuildLayout(ref DynamicGumpBuilder builder)
    {
        builder.AddPage();

        // Panels (transcribed from ServUO AddGumpLayout).
        builder.AddBackground(0, 0, 540, 350, 9200);

        builder.AddImageTiled(10, 10, 520, 30, 2624);  // header bar
        builder.AddImageTiled(10, 45, 200, 115, 2624);  // TYPES panel
        builder.AddImageTiled(10, 165, 200, 175, 2624); // CATEGORIES panel
        builder.AddImageTiled(215, 45, 315, 260, 2624); // SELECTIONS/DESCRIPTION pane
        builder.AddImageTiled(215, 310, 315, 30, 2624); // footer

        builder.AddAlphaRegion(10, 10, 520, 30);
        builder.AddAlphaRegion(10, 45, 200, 115);
        builder.AddAlphaRegion(10, 165, 200, 175);
        builder.AddAlphaRegion(215, 45, 315, 260);
        builder.AddAlphaRegion(215, 310, 290, 30);

        builder.AddHtmlLocalized(0, 15, 540, 16, 1115023, 0xFFFF);  // <CENTER>TITLES MENU</CENTER>
        builder.AddHtmlLocalized(10, 50, 220, 16, 1115024, 0xFFFF); // <CENTER>TYPES</CENTER>
        builder.AddHtmlLocalized(10, 170, 220, 16, 1044010, 0xFFFF); // <CENTER>CATEGORIES</CENTER>

        builder.AddHtmlLocalized(480, 315, 80, 16, 1060675, 0xFFFF); // CLOSE
        builder.AddButton(445, 315, 4005, 4007, 0);

        if (_category != TitleCategory.None)
        {
            builder.AddHtmlLocalized(215, 50, 315, 16, 1044011, 0xFFFF); // <CENTER>SELECTIONS</CENTER>
        }

        BuildTypeButtons(ref builder);
        BuildCategoryButtons(ref builder);
        BuildSelections(ref builder);
    }

    private void BuildTypeButtons(ref DynamicGumpBuilder builder)
    {
        // All four TYPE buttons shown for fidelity; only PaperdollSuffix + SubTitles
        // have backed categories today.
        builder.AddHtmlLocalized(55, 70, 160, 16, 1115026, 0xFFFF); // Paperdoll Name (Prefix)
        builder.AddButton(20, 70, 4005, 4007, 10001);

        builder.AddHtmlLocalized(55, 92, 160, 16, 1115027, 0xFFFF); // Paperdoll Name (Suffix)
        builder.AddButton(20, 92, 4005, 4007, 10002);

        builder.AddHtmlLocalized(55, 114, 160, 16, 1115028, 0xFFFF); // Overhead Name
        builder.AddButton(20, 114, 4005, 4007, 10003);

        builder.AddHtmlLocalized(55, 136, 160, 16, 1115029, 0xFFFF); // Subtitle
        builder.AddButton(20, 136, 4005, 4007, 10004);
    }

    private void BuildCategoryButtons(ref DynamicGumpBuilder builder)
    {
        switch (_type)
        {
            case TitleType.PaperdollSuffix:
                {
                    // Champion category button (id 10101) ONLY if the player has a champion title.
                    if (ChampionTitleSystem.GetChampionTitleLabel(_player) > 0)
                    {
                        builder.AddHtmlLocalized(55, 190, 160, 16, 1115032, 0xFFFF); // Monster
                        builder.AddButton(20, 190, 4005, 4007, 10101);
                    }

                    // Deferred: Skills -> not rendered.
                    break;
                }
            case TitleType.SubTitles:
                {
                    // RewardTitles category button (id 10105) ONLY if the player has earned titles.
                    if (RewardTitleSystem.GetContext(_player, out var ctx) && ctx.TitleList.Count > 0)
                    {
                        builder.AddHtmlLocalized(55, 190, 160, 16, 1115034, 0xFFFF); // Rewards
                        builder.AddButton(20, 190, 4005, 4007, 10105);
                    }

                    // Deferred: Skills/Guild/Veteran -> not rendered.
                    break;
                }

            // Deferred: PaperdollPrefix(FameKarma), OverheadName -> no backed categories.
        }
    }

    private void BuildSelections(ref DynamicGumpBuilder builder)
    {
        switch (_category)
        {
            case TitleCategory.RewardTitles:
                {
                    BuildRewardTitles(ref builder);
                    break;
                }
            case TitleCategory.Champion:
                {
                    BuildChampion(ref builder);
                    break;
                }
        }
    }

    private void BuildRewardTitles(ref DynamicGumpBuilder builder)
    {
        if (!RewardTitleSystem.GetContext(_player, out var ctx))
        {
            return;
        }

        var titles = ctx.TitleList;
        const int perPage = 9;
        var maxPage = titles.Count > 0 ? (titles.Count - 1) / perPage : 0;
        if (_page > maxPage)
        {
            _page = maxPage;
        }
        if (_page < 0)
        {
            _page = 0;
        }
        var start = _page * perPage;

        // First row: "Hide title" -> APPLY button id 5000 (selects -1).
        builder.AddHtmlLocalized(260, 70, 245, 16, 1154764, 0xFFFF); // (DEFAULT)
        builder.AddButton(225, 70, 4005, 4007, 5000);

        for (var i = start; i < start + perPage && i < titles.Count; i++)
        {
            var row = i - start + 1; // row 0 is the hide-title row
            var y = 70 + row * 22;

            var td = titles[i];

            if (td.Number > 0)
            {
                builder.AddHtmlLocalized(260, y, 245, 16, td.Number, 0xFFFF);
            }
            else if (td.String != null)
            {
                builder.AddHtml(260, y, 245, 16, td.String, "#FFFFFF");
            }

            // Per-row APPLY button id = 6000 + i (absolute index); reward-title counts are small,
            // well below the 10001 type-button range. Recover index = id - 6000 in OnResponse.
            builder.AddButton(225, y, 4005, 4007, 6000 + i);
        }

        // Pagination (transcribed PREV/NEXT PAGE coords from ServUO CheckPage).
        if (_page > 0)
        {
            builder.AddHtmlLocalized(265, 275, 100, 16, 1044044, 0xFFFF); // PREV PAGE
            builder.AddButton(225, 275, 4014, 4016, 5100);
        }

        if (start + perPage < titles.Count)
        {
            builder.AddHtmlLocalized(415, 275, 100, 16, 1044045, 0xFFFF); // NEXT PAGE
            builder.AddButton(380, 275, 4005, 4007, 5101);
        }
    }

    private void BuildChampion(ref DynamicGumpBuilder builder)
    {
        var label = ChampionTitleSystem.GetChampionTitleLabel(_player);
        if (label <= 0)
        {
            _category = TitleCategory.None;
            return;
        }

        builder.AddHtmlLocalized(225, 220, 160, 16, 1115027, 0xFFFF); // Paperdoll Name (Suffix)
        builder.AddHtmlLocalized(275, 240, 245, 32, label, 0xFFFF); // current champion title

        // Toggle button id 5200 -> shows/hides via _player.DisplayChampionTitle.
        builder.AddHtmlLocalized(225, 275, 200, 16, _player.DisplayChampionTitle ? 1062419 : 1062418, 0xFFFF); // hide / display
        builder.AddButton(445, 275, 4005, 4007, 5200);
    }

    public override void OnResponse(NetState sender, in RelayInfo info)
    {
        if (sender.Mobile is not PlayerMobile pm || pm != _player)
        {
            return;
        }

        var id = info.ButtonID;

        switch (id)
        {
            case 0:
                {
                    return; // close
                }

            case 10001:
                {
                    _type = TitleType.PaperdollPrefix;
                    _category = TitleCategory.None;
                    _page = 0;
                    break;
                }
            case 10002:
                {
                    _type = TitleType.PaperdollSuffix;
                    _category = TitleCategory.None;
                    _page = 0;
                    break;
                }
            case 10003:
                {
                    _type = TitleType.OverheadName;
                    _category = TitleCategory.None;
                    _page = 0;
                    break;
                }
            case 10004:
                {
                    _type = TitleType.SubTitles;
                    _category = TitleCategory.None;
                    _page = 0;
                    break;
                }

            case 10101:
                {
                    _category = TitleCategory.Champion;
                    _page = 0;
                    break;
                }
            case 10105:
                {
                    _category = TitleCategory.RewardTitles;
                    _page = 0;
                    break;
                }

            case 5000: // hide reward title
                {
                    RewardTitleSystem.Select(_player, -1);
                    _player.InvalidateProperties();
                    break;
                }

            case 5100: // back
                {
                    _page = _page > 0 ? _page - 1 : 0;
                    break;
                }
            case 5101: // forward
                {
                    _page++;
                    break;
                }

            case 5200: // champion display toggle
                {
                    _player.DisplayChampionTitle = !_player.DisplayChampionTitle;
                    _player.InvalidateProperties();
                    break;
                }

            default:
                {
                    if (id >= 6000) // reward-title row APPLY
                    {
                        RewardTitleSystem.Select(_player, id - 6000);
                        _player.InvalidateProperties();
                    }

                    break;
                }
        }

        _player.SendGump(this);
    }
}

public class TitlesMenuEntry : ContextMenuEntry
{
    public TitlesMenuEntry() : base(1115022, -1) // Open Titles Menu
    {
    }

    public override void OnClick(Mobile from, IEntity target)
    {
        if (from is PlayerMobile pm)
        {
            TitlesGump.DisplayTo(pm);
        }
    }
}
