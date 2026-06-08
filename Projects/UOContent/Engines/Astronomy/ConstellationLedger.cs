// Source: ServUO Scripts/Services/Astronomy/ConstellationLedger.cs
using System;
using ModernUO.Serialization;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Astronomy;

[SerializationGenerator(0)]
public partial class ConstellationLedger : Item
{
    public override int LabelNumber => 1158520; // Constellation Ledger

    [Constructible]
    public ConstellationLedger() : base(0xFF4) => Movable = false;

    public override void OnDoubleClick(Mobile m)
    {
        if (m is PlayerMobile pm && m.InRange(GetWorldLocation(), 3))
        {
            ConstellationLedgerGump.DisplayTo(pm, 0);
        }
    }
}

public class ConstellationLedgerGump : DynamicGump
{
    private readonly PlayerMobile _player;

    public int Page { get; private set; }

    public int Pages => (int)Math.Ceiling(AstronomySystem.DiscoveredConstellations.Count / 20.0);

    public override bool Singleton => true;

    private ConstellationLedgerGump(PlayerMobile pm, int page) : base(100, 100)
    {
        _player = pm;
        Page = page;
    }

    public static void DisplayTo(PlayerMobile pm, int page)
    {
        if (pm?.NetState == null)
        {
            return;
        }

        pm.SendGump(new ConstellationLedgerGump(pm, page));
    }

    protected override void BuildLayout(ref DynamicGumpBuilder builder)
    {
        builder.AddPage(0);

        builder.AddBackground(0, 0, 820, 620, 0x2454);

        // Constellation Ledger (cliloc 1158520 via 1114513 indirection)
        builder.AddHtmlLocalized(10, 28, 800, 18, 1114513, "#1158520", 0x0, false, false);

        // Constellations Discovered: ~1_VAL~ / ~2_VAL~
        builder.AddHtmlLocalized(295, 55, 515, 36, 1158521, $"{AstronomySystem.DiscoveredConstellations.Count}\t{AstronomySystem.MaxConstellations}", 0x0, false, false);

        // Column headers
        builder.AddHtmlLocalized(55, 100, 100, 36, 1114513, "#1158522", 0x0, false, false);  // Constellation Name
        builder.AddHtmlLocalized(245, 100, 80, 36, 1114513, "#1158523", 0x0, false, false);  // Astronomer
        builder.AddHtmlLocalized(375, 100, 80, 36, 1114513, "#1158524", 0x0, false, false);  // Discovery Date
        builder.AddHtmlLocalized(505, 100, 80, 36, 1114513, "#1158525", 0x0, false, false);  // Night Period
        builder.AddHtmlLocalized(635, 100, 80, 36, 1114513, "#1158526", 0x0, false, false);  // Coordinates

        var start = Page * 20;
        var y = 145;

        for (var i = start; i < AstronomySystem.DiscoveredConstellations.Count && i <= start + 20; i++)
        {
            var info = AstronomySystem.GetConstellation(AstronomySystem.DiscoveredConstellations[i]);

            if (info == null)
            {
                continue;
            }

            builder.AddHtml(15, y, 200, 18, $"{info.Name}", "#0040FF");
            builder.AddHtml(240, y, 112, 18, $"{(info.DiscoveredBy != null ? info.DiscoveredBy.Name : "Unknown")}", "#0040FF");
            builder.AddHtml(380, y, 112, 18, $"{info.DiscoveredOn.ToShortDateString()}", "#0040FF");
            builder.AddHtmlLocalized(492, y, 130, 18, AstronomySystem.TimeCoordinateLocalization(info.TimeCoordinate), 0x1F, false, false);

            // RA ~1_VAL~  DEC ~2_VAL~
            builder.AddHtmlLocalized(632, y, 150, 18, 1158527, $"{info.CoordRA}\t{info.CoordDEC}", 0x1F, false, false);

            y += 18;
        }

        // Navigation buttons: first, prev, next, last
        builder.AddButton(340, 540, 0x605, 0x606, 1, GumpButtonType.Reply, 0);
        builder.AddButton(370, 540, 0x609, 0x60A, 2, GumpButtonType.Reply, 0);
        builder.AddButton(460, 540, 0x607, 0x608, 3, GumpButtonType.Reply, 0);
        builder.AddButton(484, 540, 0x603, 0x604, 4, GumpButtonType.Reply, 0);

        builder.AddLabel(415, 570, 0, $"{Page + 1}/{Pages}");
    }

    public override void OnResponse(NetState sender, in RelayInfo info)
    {
        switch (info.ButtonID)
        {
            case 0:
                return;
            case 1:
                Page = 0;
                break;
            case 2:
                Page = Math.Max(0, Page - 1);
                break;
            case 3:
                Page = Math.Min(Page + 1, Math.Max(0, Pages - 1));
                break;
            case 4:
                Page = Math.Max(0, Pages - 1);
                break;
        }

        ConstellationLedgerGump.DisplayTo((PlayerMobile)sender.Mobile, Page);
    }
}
