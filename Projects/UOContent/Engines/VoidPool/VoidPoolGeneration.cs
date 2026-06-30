// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Generate.cs (Setup + AddWaypoints — Cora/Vela spawners and ConvertSpawners omitted)
using Server.Commands;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.VoidPool;

public static class VoidPoolGeneration
{
    public static void Configure()
    {
        CommandSystem.Register("GenVoidPool", AccessLevel.Administrator, GenVoidPool_OnCommand);
        CommandSystem.Register("DelVoidPool", AccessLevel.Administrator, DelVoidPool_OnCommand);
        CommandSystem.Register("VoidPool", AccessLevel.Player, VoidPool_OnCommand);
    }

    [Usage("GenVoidPool")]
    [Description("Enables the Void Pool system (opt-in, persisted) and places the controllers, static tiles, and waypoints if not already present.")]
    private static void GenVoidPool_OnCommand(CommandEventArgs e)
    {
        Points.VoidPool.Enable();

        if (VoidPoolController.InstanceTram != null || VoidPoolController.InstanceFel != null)
        {
            e.Mobile.SendMessage("Void Pool has already been set up!");
            return;
        }

        var placed = 0;

        // Controllers
        var one = new VoidPoolController(Map.Trammel);
        one.MoveToWorld(new Point3D(5605, 1998, 10), Map.Trammel);
        placed++;

        var two = new VoidPoolController(Map.Felucca);
        two.MoveToWorld(new Point3D(5605, 1998, 10), Map.Felucca);
        placed++;

        // Void Pool static tiles — 7x7 grid, both facets
        for (var x = 5497; x <= 5503; x++)
        {
            for (var y = 1995; y <= 2001; y++)
            {
                int id;
                if (x == 5497 && y == 1995)
                {
                    id = 1886;
                }
                else if (x == 5497 && y == 2001)
                {
                    id = 1887;
                }
                else if (x == 5503 && y == 1995)
                {
                    id = 1888;
                }
                else if (x == 5503 && y == 2001)
                {
                    id = 1885;
                }
                else if (x == 5497)
                {
                    id = 1874;
                }
                else if (x == 5503)
                {
                    id = 1876;
                }
                else if (y == 1995)
                {
                    id = 1873;
                }
                else if (y == 2001)
                {
                    id = 1875;
                }
                else
                {
                    id = Utility.Random(8511, 6);
                }

                var hue = id >= 8511 ? 0 : 1954;
                var point = new Point3D(x, y, 5);

                var tileT = new Static(id) { Name = "Void Pool", Hue = hue };
                tileT.MoveToWorld(point, Map.Trammel);
                placed++;

                var tileF = new Static(id) { Name = "Void Pool", Hue = hue };
                tileF.MoveToWorld(point, Map.Felucca);
                placed++;
            }
        }

        AddWaypoints(one, two);
        placed += one.WaypointsA.Count + one.WaypointsB.Count + two.WaypointsA.Count + two.WaypointsB.Count;

        e.Mobile.SendMessage($"Void Pool: enabled; placed {placed} object(s).");
    }

    /// <summary>
    /// Places and chains all waypoints for both path A and path B on both facets,
    /// adding them to the controller lists and linking each via NextPoint.
    /// </summary>
    private static void AddWaypoints(VoidPoolController one, VoidPoolController two)
    {
        // Clear any existing waypoints first
        foreach (var w in one.WaypointsA)
        {
            if (w is { Deleted: false })
            {
                w.Delete();
            }
        }

        foreach (var w in one.WaypointsB)
        {
            if (w is { Deleted: false })
            {
                w.Delete();
            }
        }

        foreach (var w in two.WaypointsA)
        {
            if (w is { Deleted: false })
            {
                w.Delete();
            }
        }

        foreach (var w in two.WaypointsB)
        {
            if (w is { Deleted: false })
            {
                w.Delete();
            }
        }

        one.WaypointsA.Clear();
        one.WaypointsB.Clear();
        two.WaypointsA.Clear();
        two.WaypointsB.Clear();

        // Path A — Trammel + Felucca (interleaved per ServUO source)
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5590, 2024, 0));
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5578, 2029, 0));
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5566, 2027, 0));
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5555, 2021, 0));
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5545, 2015, 0));
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5537, 2020, 0));
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5527, 2015, 0));
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5509, 2005, 0));
        PlaceWaypointPair(one, two, one.WaypointsA, two.WaypointsA, new Point3D(5500, 1998, 0));

        // Path B — Trammel + Felucca
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5469, 2016, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5478, 2025, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5484, 2029, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5490, 2027, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5504, 2027, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5516, 2020, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5524, 2012, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5513, 2005, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5502, 2004, 0));
        PlaceWaypointPair(one, two, one.WaypointsB, two.WaypointsB, new Point3D(5500, 1998, 0));

        // Chain NextPoint links for all four lists
        ChainWaypoints(one.WaypointsA);
        ChainWaypoints(one.WaypointsB);
        ChainWaypoints(two.WaypointsA);
        ChainWaypoints(two.WaypointsB);
    }

    private static void PlaceWaypointPair(
        VoidPoolController one, VoidPoolController two,
        System.Collections.Generic.List<WayPoint> listOne,
        System.Collections.Generic.List<WayPoint> listTwo,
        Point3D point)
    {
        var wpT = new WayPoint();
        wpT.MoveToWorld(point, Map.Trammel);
        listOne.Add(wpT);

        var wpF = new WayPoint();
        wpF.MoveToWorld(point, Map.Felucca);
        listTwo.Add(wpF);
    }

    private static void ChainWaypoints(System.Collections.Generic.List<WayPoint> list)
    {
        for (var i = 0; i < list.Count - 1; i++)
        {
            list[i].NextPoint = list[i + 1];
        }
    }

    [Usage("DelVoidPool")]
    [Description("Removes the Void Pool controllers (and their waypoints/region), plus the Void Pool static tiles on both facets.")]
    private static void DelVoidPool_OnCommand(CommandEventArgs e)
    {
        var deleted = 0;

        // Delete controllers — their OnDelete() cleans up waypoints + region
        if (VoidPoolController.InstanceTram != null)
        {
            VoidPoolController.InstanceTram.Delete();
            VoidPoolController.InstanceTram = null;
            deleted++;
        }

        if (VoidPoolController.InstanceFel != null)
        {
            VoidPoolController.InstanceFel.Delete();
            VoidPoolController.InstanceFel = null;
            deleted++;
        }

        // Delete Void Pool static tiles by name (both facets, z=5 at the 7x7 grid)
        for (var x = 5497; x <= 5503; x++)
        {
            for (var y = 1995; y <= 2001; y++)
            {
                var point = new Point3D(x, y, 5);

                foreach (var item in Map.Trammel.GetItemsAt<Static>(point))
                {
                    if (item.Name == "Void Pool" && item.Z == 5)
                    {
                        item.Delete();
                        deleted++;
                        break;
                    }
                }

                foreach (var item in Map.Felucca.GetItemsAt<Static>(point))
                {
                    if (item.Name == "Void Pool" && item.Z == 5)
                    {
                        item.Delete();
                        deleted++;
                        break;
                    }
                }
            }
        }

        e.Mobile.SendMessage($"Void Pool: deleted {deleted} object(s).");
    }

    [Usage("VoidPool")]
    [Description("Opens the Void Pool status gump for the caller's current facet.")]
    private static void VoidPool_OnCommand(CommandEventArgs e)
    {
        var controller = e.Mobile.Map == Map.Felucca
            ? VoidPoolController.InstanceFel
            : VoidPoolController.InstanceTram;

        if (controller == null)
        {
            e.Mobile.SendMessage("The Void Pool is not available on this facet.");
            return;
        }

        if (e.Mobile is PlayerMobile pm)
        {
            VoidPoolGump.DisplayTo(pm, controller);
        }
        else
        {
            e.Mobile.SendMessage("Only players may access the Void Pool.");
        }
    }
}
