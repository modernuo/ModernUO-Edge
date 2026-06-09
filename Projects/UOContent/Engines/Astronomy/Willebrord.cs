// Source: ServUO Scripts/Services/Astronomy/Willebrord.cs
using System.Collections.Generic;
using ModernUO.Serialization;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Astronomy;

[SerializationGenerator(0)]
public partial class Willebrord : BaseVendor
{
    private readonly List<SBInfo> m_SBInfos = new();

    protected override List<SBInfo> SBInfos => m_SBInfos;

    public override bool IsActiveVendor => false;

    [Constructible]
    public Willebrord() : base("the Astronomer")
    {
    }

    public override void InitSBInfo()
    {
    }

    public override void InitBody()
    {
        InitStats(100, 100, 25);
        CantWalk = true;
        Name = "Willebrord";

        Race = Race.Human;
        Body = Race.MaleBody;

        HairItemID = Race.RandomHair(false);
        HairHue = Race.RandomHairHue();
    }

    public override void InitOutfit()
    {
        // Edge BaseVendor has no SetWearable; dress via AddItem with the ServUO hues preserved (1908/1255).
        AddItem(new Kamishimo { Movable = false });
        AddItem(new ThighBoots { Hue = 1908, Movable = false });
        AddItem(new FancyShirt { Hue = 1255, Movable = false });
    }

    public override void OnDoubleClick(Mobile m)
    {
        // InLOS(m) direction matches ServUO; LOS is symmetric so this is fine for a stationary NPC.
        if (m.InRange(Location, 3) && InLOS(m))
        {
            WillebrordInfoGump.DisplayTo(m);
        }
    }

    public override bool OnDragDrop(Mobile m, Item dropped)
    {
        if (dropped is StarChart chart)
        {
            if (chart.Constellation >= 0 && chart.Constellation < AstronomySystem.MaxConstellations)
            {
                if (string.IsNullOrEmpty(chart.ConstellationName))
                {
                    m.SendLocalizedMessage(1158751); // You must name your constellation before submitting it.
                }
                else
                {
                    var info = AstronomySystem.GetConstellation(chart.Constellation);

                    if (info != null)
                    {
                        if (info.HasBeenDiscovered)
                        {
                            // That constellation name has already been chosen, please choose another and resubmit your star chart.
                            m.SendLocalizedMessage(1158764);

                            // Sorry to say that constellation has already been discovered! ...
                            WillebrordResultGump.DisplayTo(m, 1158530);
                        }
                        else
                        {
                            // Wow! Would you look at that! ... I've recorded your discovery in the ledger. ...
                            WillebrordResultGump.DisplayTo(m, 1158519);

                            info.DiscoveredBy = chart.ChartedBy;
                            info.Name = chart.ConstellationName;
                            info.DiscoveredOn = chart.ChartedOn;
                            AstronomySystem.AddDiscovery(info);

                            m.AddToBackpack(new RecipeScroll(465));
                            // AstronomerTitleDeed reward deferred — Edge has no RewardTitle system yet (tracked in migration status doc).
                        }
                    }
                }
            }
        }
        else
        {
            SayTo(m, 1158529); // What's this? I haven't time for this! Star Charts only please!
        }

        // Returns false (chart NOT consumed) to match ServUO. Re-submission is harmless: the constellation is flagged discovered on first success, so no second reward is granted.
        return false;
    }
}

public class WillebrordInfoGump : StaticGump<WillebrordInfoGump>
{
    private WillebrordInfoGump() : base(100, 100)
    {
    }

    public static void DisplayTo(Mobile m)
    {
        if (m?.NetState == null)
        {
            return;
        }
        m.SendGump(new WillebrordInfoGump());
    }

    protected override void BuildLayout(ref StaticGumpBuilder builder)
    {
        builder.AddPage();

        builder.AddBackground(0, 0, 720, 270, 0x2454);
        builder.AddImage(0, 0, 0x69D);

        builder.AddHtmlLocalized(290, 14, 418, 18, 1114513, "#1158517", 0xC63); // Willebrord the Astronomer
        builder.AddHtmlLocalized(290, 51, 418, 209, 1158518, 0xC63, false, true);
    }
}

public class WillebrordResultGump : DynamicGump
{
    private readonly int _bodyCliloc;

    private WillebrordResultGump(int bodyCliloc) : base(100, 100)
    {
        _bodyCliloc = bodyCliloc;
    }

    public static void DisplayTo(Mobile m, int bodyCliloc)
    {
        if (m?.NetState == null)
        {
            return;
        }
        m.SendGump(new WillebrordResultGump(bodyCliloc));
    }

    protected override void BuildLayout(ref DynamicGumpBuilder builder)
    {
        builder.AddPage();

        builder.AddBackground(0, 0, 720, 270, 0x2454);
        builder.AddImage(0, 0, 0x69D);

        builder.AddHtmlLocalized(290, 14, 418, 18, 1114513, "#1158517", 0xC63); // Willebrord the Astronomer
        builder.AddHtmlLocalized(290, 51, 418, 209, _bodyCliloc, 0xC63, false, true);
    }
}
