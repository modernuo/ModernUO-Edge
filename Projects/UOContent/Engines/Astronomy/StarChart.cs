// Source: ServUO Scripts/Services/Astronomy/StarChart.cs
using System;
using ModernUO.Serialization;
using Server.Engines.Astronomy;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Guilds;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Items;

[Flippable(0xA1E4, 0xA1E5)]
[SerializationGenerator(0)]
public partial class StarChart : Item, ICraftable
{
    [SerializableProperty(0)]
    [CommandProperty(AccessLevel.GameMaster)]
    public int Constellation
    {
        get => _constellation;
        set
        {
            _constellation = value;
            Hue = _constellation < 0 ? 2500 : 0;
            InvalidateProperties();
            this.MarkDirty();
        }
    }

    [SerializableProperty(1)]
    [CommandProperty(AccessLevel.GameMaster)]
    public string ConstellationName
    {
        get => _constellationName;
        set
        {
            _constellationName = value;
            InvalidateProperties();
            this.MarkDirty();
        }
    }

    [SerializableProperty(2)]
    [CommandProperty(AccessLevel.GameMaster)]
    public Mobile ChartedBy
    {
        get => _chartedBy;
        set
        {
            _chartedBy = value;
            InvalidateProperties();
            this.MarkDirty();
        }
    }

    [SerializableField(3)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private DateTime _chartedOn;

    public override int LabelNumber => _constellation == -1 ? 1158743 : 1158493; // An Indecipherable Star Chart : Star Chart

    [Constructible]
    public StarChart() : base(0xA1E4)
    {
        _constellation = -1;
        Hue = 2500;
    }

    public int OnCraft(
        int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool,
        CraftItem craftItem, int resHue
    )
    {
        Timer.DelayCall(() => SendTarget(from));

        return quality;
    }

    public void SendTarget(Mobile m)
    {
        m.SendLocalizedMessage(1158494); // Which telescope do you wish to create the star chart from?
        m.BeginTarget(
            10,
            false,
            TargetFlags.None,
            (from, targeted) =>
            {
                if (!Deleted && IsChildOf(from.Backpack) && targeted is PersonalTelescope tele)
                {
                    var constellation = AstronomySystem.GetConstellation(tele.TimeCoordinate, tele.RA, tele.DEC);

                    if (constellation != null)
                    {
                        from.SendLocalizedMessage(1158496); // You successfully map the time-coordinate of the constellation.

                        ChartedBy = from;
                        ChartedOn = DateTime.Now;
                        Constellation = constellation.Identifier;
                        from.PlaySound(0x249);
                    }
                    else
                    {
                        from.SendLocalizedMessage(1158495); // There is nothing to chart at these coordinates at this time.
                    }
                }
            }
        );
    }

    public override void OnDoubleClick(Mobile m)
    {
        if (m is PlayerMobile pm && IsChildOf(m.Backpack) && _constellation > -1)
        {
            StarChartGump.DisplayTo(pm, this);
        }
    }

    public override void OnAfterDelete()
    {
        base.OnAfterDelete();
        ChartedBy = null;
    }

    public override void GetProperties(IPropertyList list)
    {
        base.GetProperties(list);

        if (_constellation > -1)
        {
            if (_chartedBy != null)
            {
                list.Add(1158500, _chartedBy.Name); // Charted By: ~1_NAME~
            }

            list.Add(1158501, _constellationName ?? "A Constellation With No Name");
        }
    }
}

public class StarChartGump : DynamicGump
{
    private readonly PlayerMobile _player;

    public StarChart Chart { get; }

    public override bool Singleton => true;

    private StarChartGump(PlayerMobile pm, StarChart chart) : base(50, 50)
    {
        _player = pm;
        Chart = chart;
    }

    public static void DisplayTo(PlayerMobile pm, StarChart chart)
    {
        if (pm?.NetState == null || chart?.Deleted != false || chart.Constellation < 0)
        {
            return;
        }

        if (AstronomySystem.GetConstellation(chart.Constellation) == null)
        {
            return; // constellation data no longer exists (e.g. after a reset)
        }

        pm.SendGump(new StarChartGump(pm, chart));
    }

    protected override void BuildLayout(ref DynamicGumpBuilder builder)
    {
        var info = AstronomySystem.GetConstellation(Chart.Constellation);

        builder.AddPage(0);

        builder.AddBackground(0, 0, 454, 350, 0x24AE);

        builder.AddHtmlLocalized(32, 68, 112, 36, 1158505); // Constellation Name:
        builder.AddHtml(
            154,
            68,
            300,
            36,
            $"{(string.IsNullOrEmpty(Chart.ConstellationName) ? "This constellation has not yet been named" : Chart.ConstellationName)}",
            "#0040FF"
        );

        builder.AddHtmlLocalized(32, 104, 75, 36, 1158502); // Charted By:
        builder.AddHtml(112, 104, 50, 36, $"{Chart.ChartedBy?.Name ?? string.Empty}", "#0040FF");

        builder.AddHtmlLocalized(32, 140, 75, 36, 1158503); // Charted On:
        builder.AddHtml(112, 140, 80, 36, $"{Chart.ChartedOn:d}", "#0040FF");

        builder.AddHtmlLocalized(32, 176, 125, 18, 1158504); // Time-Coordinate:

        if (info != null)
        {
            builder.AddHtmlLocalized(47, 199, 60, 36, AstronomySystem.TimeCoordinateLocalization(info.TimeCoordinate), 0x1F);

            builder.AddHtmlLocalized(157, 199, 20, 36, 1158489); // RA
            builder.AddHtml(182, 199, 20, 36, $"{info.CoordRA}", "#0040FF");

            builder.AddHtmlLocalized(242, 199, 25, 36, 1158490); // DEC
            builder.AddHtml(272, 199, 50, 36, $"{info.CoordDEC}", "#0040FF");
        }

        builder.AddBackground(32, 253, 343, 22, 0x2486);
        builder.AddTextEntry(34, 255, 339, 18, 0, 1);

        builder.AddButton(375, 245, 0x232C, 0x232D, 1, GumpButtonType.Reply, 0);
    }

    public override void OnResponse(NetState sender, in RelayInfo info)
    {
        if (info.ButtonID != 1 || Chart?.Deleted != false || Chart.Constellation < 0)
        {
            return;
        }

        if (AstronomySystem.GetConstellation(Chart.Constellation) == null)
        {
            return; // constellation data no longer exists (e.g. after a reset)
        }

        var text = info.GetTextEntry(1);

        if (text == null)
        {
            return;
        }

        if (BaseGuildGump.CheckProfanity(text) &&
            !AstronomySystem.CheckNameExists(text) &&
            text.Length > 0 &&
            text.Length < 37)
        {
            Chart.ConstellationName = text;
            _player.SendLocalizedMessage(1158512); // You record the name of the constellation.
        }
        else
        {
            _player.SendLocalizedMessage(1158511); // You have entered an invalid name. Please try again.
        }
    }
}
