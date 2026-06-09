// Source: ServUO Scripts/Services/Astronomy/Telescope.cs
using System;
using ModernUO.Serialization;
using Server.Collections;
using Server.ContextMenus;
using Server.Engines.Astronomy;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items;

[Flippable(0xA12C, 0xA12D)]
[SerializationGenerator(0)]
public partial class PersonalTelescope : Item, ISecurable
{
    private static readonly string[] _Names =
    {
        "Adranath", "Aeluva the Arcanist", "Aesthyron", "Anon", "Balaki", "Clanin", "Dexter", "Doctor Spector", "Dryus Doost",
        "Gilform", "Grizelda the Hag", "Hawkwind", "Heigel of Moonglow", "Intanya", "Juo'Nar", "King Blackthorn", "Koole the Arcanist",
        "Kronos", "Kyrnia", "Lathiari", "Leoric Gathenwale", "Lysander Gathenwale", "Malabelle", "Mariah", "Melissa", "Minax",
        "Mondain", "Mordra", "Mythran", "Neira the Necromancer", "Nystul", "Queen Zhah", "Relvinian", "Selsius the Astronomer",
        "Sutek", "Uzeraan", "Wexton the Apprentice"
    };

    [SerializableField(0)]
    [InvalidateProperties]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private string _displayName;

    [SerializableField(1)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private SecureLevel _level;

    [SerializableField(2)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private int _rA;

    [SerializableProperty(3)]
    [CommandProperty(AccessLevel.GameMaster)]
    public double DEC
    {
        get => _dEC;
        set
        {
            _dEC = (double)decimal.Round((decimal)value, 2);
            this.MarkDirty();
        }
    }

    // Intentionally not serialized — transient cooldown, matches ServUO.
    public DateTime LastUse { get; set; }

    [CommandProperty(AccessLevel.GameMaster)]
    public TimeCoordinate TimeCoordinate => AstronomySystem.GetTimeCoordinate(this);

    public override int LabelNumber => 1125284;

    [Constructible]
    public PersonalTelescope() : base(0xA12C)
    {
        _level = SecureLevel.Owner;
        _displayName = _Names.RandomElement();
    }

    public override void OnDoubleClick(Mobile m)
    {
        if (m is not PlayerMobile pm)
        {
            return;
        }

        if (!IsLockedDown)
        {
            pm.SendLocalizedMessage(1114298); // This must be locked down in order to use it.
            return;
        }

        if (pm.InRange(Location, 2))
        {
            var house = BaseHouse.FindHouseAt(this);

            if (house != null && house.HasSecureAccess(pm, Level))
            {
                // Faithful to ServUO: first use after a 10-min idle shows "calibrating" and requires a second double-click to open. Intentional parity, not a bug.
                if (DateTime.UtcNow - LastUse > TimeSpan.FromMinutes(10))
                {
                    pm.SendLocalizedMessage(1158643); // The telescope is calibrating, try again in a moment.

                    LastUse = DateTime.UtcNow;
                }
                else
                {
                    TelescopeGump.DisplayTo(pm, this);
                }
            }
        }
        else
        {
            pm.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
        }
    }

    public override void GetProperties(IPropertyList list)
    {
        base.GetProperties(list);

        if (!string.IsNullOrEmpty(_displayName))
        {
            // <BASEFONT COLOR=#FFD24D>From the personal study of ~1_NAME~<BASEFONT COLOR=#FFFFFF>
            list.Add(1158477, _displayName);
        }
    }

    public override void GetContextMenuEntries(Mobile from, ref PooledRefList<ContextMenuEntry> list)
    {
        base.GetContextMenuEntries(from, ref list);

        SetSecureLevelEntry.AddTo(from, this, ref list);
    }
}

public class TelescopeGump : DynamicGump
{
    private readonly PlayerMobile _player;

    public PersonalTelescope Tele { get; }
    public int ImageID { get; set; }
    public ConstellationInfo Constellation { get; set; }
    public (int ImageId, int Cliloc)? InterstellarObject { get; set; }

    public override bool Singleton => true;

    private TelescopeGump(PlayerMobile pm, PersonalTelescope tele) : base(200, 200)
    {
        _player = pm;
        Tele = tele;
    }

    public static void DisplayTo(PlayerMobile pm, PersonalTelescope tele)
    {
        if (pm?.NetState == null || tele?.Deleted != false)
        {
            return;
        }

        pm.SendGump(new TelescopeGump(pm, tele));
    }

    protected override void BuildLayout(ref DynamicGumpBuilder builder)
    {
        builder.AddPage(0);

        if (ImageID == 0)
        {
            ImageID = AstronomySystem.RandomSkyImage(_player);
        }

        builder.AddImage(0, 0, ImageID);

        builder.AddImage(222, 597, 0x694);
        builder.AddImage(229, 600, GetGumpNumber(GetPlace(Tele.RA, 10)));

        builder.AddButton(222, 584, 0x697, 0x698, 60000, GumpButtonType.Reply, 0);
        builder.AddButton(222, 631, 0x699, 0x69A, 60001, GumpButtonType.Reply, 0);

        builder.AddImage(256, 597, 0x694);
        builder.AddImage(263, 600, GetGumpNumber(GetPlace(Tele.RA, 1)));

        builder.AddButton(256, 584, 0x697, 0x698, 60002, GumpButtonType.Reply, 0);
        builder.AddButton(256, 631, 0x699, 0x69A, 60003, GumpButtonType.Reply, 0);

        builder.AddButton(291, 597, 0x69B, 0x69C, 70000, GumpButtonType.Reply, 0);
        builder.AddTooltip(1158499); // View Coordinate

        builder.AddImage(332, 597, 0x694);
        builder.AddImage(339, 600, GetGumpNumber(GetPlace((int)Math.Truncate(Tele.DEC), 10)));

        builder.AddButton(332, 584, 0x697, 0x698, 60004, GumpButtonType.Reply, 0);
        builder.AddButton(332, 631, 0x699, 0x69A, 60005, GumpButtonType.Reply, 0);

        builder.AddImage(366, 597, 0x694);
        builder.AddImage(373, 600, GetGumpNumber(GetPlace((int)Math.Truncate(Tele.DEC), 1)));

        builder.AddButton(366, 584, 0x697, 0x698, 60006, GumpButtonType.Reply, 0);
        builder.AddButton(366, 631, 0x699, 0x69A, 60007, GumpButtonType.Reply, 0);

        builder.AddImage(400, 597, 0x694);
        builder.AddImage(407, 600, GetGumpNumber(GetDecimalPlace(Tele.DEC)));

        builder.AddButton(400, 584, 0x697, 0x698, 60008, GumpButtonType.Reply, 0);
        builder.AddButton(400, 631, 0x699, 0x69A, 60009, GumpButtonType.Reply, 0);

        builder.AddImage(397, 623, 0x696);

        builder.AddHtmlLocalized(251, 651, 100, 50, 1158489, 0x6B55, false, false); // RA
        builder.AddTooltip(1158497); // Right Ascension

        builder.AddHtmlLocalized(371, 651, 100, 50, 1158490, 0x6B55, false, false); // DEC
        builder.AddTooltip(1158498); // Declination

        if (Constellation != null)
        {
            foreach (var pos in Constellation.StarPositions)
            {
                builder.AddImage(pos.X, pos.Y, pos.ImageID);
            }
        }
        else if (InterstellarObject != null)
        {
            builder.AddImage(180, 150, InterstellarObject.Value.ImageId);
        }
    }

    public override void OnResponse(NetState sender, in RelayInfo info)
    {
        if (!_player.InRange(Tele.Location, 2) || _player.Map != Tele.Map)
        {
            return;
        }

        Tele.LastUse = DateTime.UtcNow;

        switch (info.ButtonID)
        {
            case 60000: // RA 10's Up
                {
                    if (Tele.RA >= 20)
                    {
                        Tele.RA -= 20;
                    }
                    else
                    {
                        Tele.RA += 10;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60001: // RA 10's Down
                {
                    if (Tele.RA < 10)
                    {
                        Tele.RA += 20;
                    }
                    else
                    {
                        Tele.RA -= 10;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60002: // RA 1's Up
                {
                    var raOnes = GetPlace(Tele.RA, 1);

                    if (raOnes >= 9)
                    {
                        Tele.RA -= 9;
                    }
                    else
                    {
                        Tele.RA++;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60003: // RA 1's Down
                {
                    var raOnes = GetPlace(Tele.RA, 1);

                    if (raOnes == 0)
                    {
                        Tele.RA += 9;
                    }
                    else
                    {
                        Tele.RA--;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60004: // DEC 10's Up
                {
                    if (Tele.DEC >= 90)
                    {
                        Tele.DEC -= 90;
                    }
                    else
                    {
                        Tele.DEC += 10;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60005: // DEC 10's Down
                {
                    if (Tele.DEC < 10)
                    {
                        Tele.DEC += 90;
                    }
                    else
                    {
                        Tele.DEC -= 10;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60006: // DEC 1's Up
                {
                    var decOnes = GetPlace((int)Math.Truncate(Tele.DEC), 1);

                    if (decOnes >= 9)
                    {
                        Tele.DEC -= 9;
                    }
                    else
                    {
                        Tele.DEC++;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60007: // DEC 1's Down
                {
                    var decOnes = GetPlace((int)Math.Truncate(Tele.DEC), 1);

                    if (decOnes <= 0)
                    {
                        Tele.DEC += 9;
                    }
                    else
                    {
                        Tele.DEC--;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60008: // DEC .2 Up
                {
                    var dec = GetDecimalPlace(Tele.DEC);

                    if (dec >= 8)
                    {
                        Tele.DEC = Math.Truncate(Tele.DEC);
                    }
                    else
                    {
                        Tele.DEC += .2;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 60009: // DEC .2 Down
                {
                    var dec1 = GetDecimalPlace(Tele.DEC);

                    if (dec1 < 2)
                    {
                        Tele.DEC += 0.8;
                    }
                    else if (dec1 == 2)
                    {
                        Tele.DEC = Math.Truncate(Tele.DEC);
                    }
                    else
                    {
                        Tele.DEC -= 0.2;
                    }

                    _player.SendSound(0x4A);
                    break;
                }
            case 70000: // View Coord
                {
                    if (Tele.RA > AstronomySystem.MaxRA || Tele.DEC > AstronomySystem.MaxDEC)
                    {
                        _player.SendLocalizedMessage(1158488); // You have entered invalid coordinates.
                        _player.SendSound(81);
                    }
                    else
                    {
                        InterstellarObject = null;
                        Constellation = null;
                        ImageID = AstronomySystem.RandomSkyImage(_player);

                        var timeCoord = Tele.TimeCoordinate;

                        if (timeCoord == TimeCoordinate.Day)
                        {
                            // You won't have much luck seeing the night sky during the day...
                            _player.SendLocalizedMessage(1158513);
                        }
                        else
                        {
                            var constellation = AstronomySystem.GetConstellation(timeCoord, Tele.RA, Tele.DEC);

                            if (constellation != null)
                            {
                                Constellation = constellation;

                                // You peer into the heavens and see...a constellation!
                                _player.SendLocalizedMessage(1158492, "", 0xBF);
                                _player.SendSound(_player.Female ? 0x32B : 0x43D);
                            }
                            else if (0.2 > Utility.RandomDouble())
                            {
                                InterstellarObject = AstronomySystem.GetRandomInterstellarObject();

                                _player.SendLocalizedMessage(InterstellarObject.Value.Cliloc, "", 0xBF);
                                _player.SendSound(_player.Female ? 0x32B : 0x43D);
                            }
                            else
                            {
                                // You peer into the heavens and see...only empty space...
                                _player.SendLocalizedMessage(1158491, "", 0xBF);
                            }
                        }
                    }

                    DisplayTo(_player, Tele);
                    return;
                }
        }

        if (info.ButtonID != 0)
        {
            DisplayTo(_player, Tele);
        }
    }

    private int GetPlace(int value, int place)
    {
        return ((value % (place * 10)) - (value % place)) / place;
    }

    private int GetDecimalPlace(double value)
    {
        var dec = decimal.Round((decimal)(value - Math.Truncate(value)), 2);

        return (int)(dec * 10);
    }

    private int GetGumpNumber(int v)
    {
        return 0x58F + v;
    }
}
