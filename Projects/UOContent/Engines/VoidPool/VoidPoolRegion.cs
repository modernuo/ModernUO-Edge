// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Region.cs
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using Server.Spells;

namespace Server.Engines.VoidPool;

public class VoidPoolRegion : BaseRegion
{
    private static readonly Rectangle2D[] _bounds =
    {
        new(5383, 1960, 236, 80),
        new(5429, 1948, 12, 10)
    };

    public VoidPoolController Controller { get; }

    public VoidPoolRegion(VoidPoolController controller, Map map)
        : base("Void Pool", map, Find(new Point3D(5505, 1998, 5), map), ConvertBounds())
    {
        Controller = controller;
    }

    private static Rectangle3D[] ConvertBounds()
    {
        var area = new Rectangle3D[_bounds.Length];

        for (var i = 0; i < _bounds.Length; i++)
        {
            area[i] = ConvertTo3D(_bounds[i]);
        }

        return area;
    }

    public void SendRegionMessage(int localization)
    {
        var players = GetPlayers();

        for (var i = 0; i < players.Count; i++)
        {
            players[i].SendLocalizedMessage(localization);
        }
    }

    public void SendRegionMessage(int localization, int hue)
    {
        var players = GetPlayers();

        for (var i = 0; i < players.Count; i++)
        {
            players[i].SendLocalizedMessage(localization, "", hue);
        }
    }

    public void SendRegionMessage(int localization, string args)
    {
        var players = GetPlayers();

        for (var i = 0; i < players.Count; i++)
        {
            players[i].SendLocalizedMessage(localization, args);
        }
    }

    public void SendRegionMessage(string message)
    {
        var players = GetPlayers();

        for (var i = 0; i < players.Count; i++)
        {
            players[i].SendMessage(0x25, message);
        }
    }

    public override void OnDeath(Mobile m)
    {
        if (m is BaseCreature { Controlled: false, Summoned: false } bc && Controller?.OnGoing == true)
        {
            Controller.OnCreatureKilled(bc);
        }

        base.OnDeath(m);
    }

    public override bool OnDoubleClick(Mobile m, object o)
    {
        if (o is Corpse c && m.AccessLevel < AccessLevel.Counselor)
        {
            if (c.Owner == null || c.Owner is CovetousCreature { VoidSpawn: true })
            {
                c.LabelTo(m, 1152684); // There is no loot on the corpse.
                return false;
            }
        }

        return base.OnDoubleClick(m, o);
    }

    public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
    {
        global = LightCycle.DungeonLevel;
    }

    public override bool CanUseStuckMenu(Mobile m) => false;

    public override bool CheckTravel(Mobile m, Point3D newLocation, TravelCheckType travelType, out TextDefinition message)
    {
        message = null;

        if (m.AccessLevel > AccessLevel.Player)
        {
            return true;
        }

        switch (travelType)
        {
            case TravelCheckType.RecallTo:
            case TravelCheckType.GateTo:
            case TravelCheckType.Mark:
                {
                    return false;
                }
            default:
                {
                    return true;
                }
        }
    }
}
