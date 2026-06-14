using System;
using Server.Commands;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Engines.Points;

public static class PointsCommands
{
    public static void Configure()
    {
        CommandSystem.Register("Points", AccessLevel.GameMaster, Points_OnCommand);
        CommandSystem.Register("AwardPoints", AccessLevel.GameMaster, AwardPoints_OnCommand);
        CommandSystem.Register("DeductPoints", AccessLevel.GameMaster, DeductPoints_OnCommand);
    }

    [Usage("Points")]
    [Description("Target a player to list their points across all registered point systems.")]
    private static void Points_OnCommand(CommandEventArgs e)
    {
        e.Mobile.SendMessage("Target a player to view their points.");
        e.Mobile.BeginTarget(-1, false, TargetFlags.None, (from, targeted) =>
        {
            if (targeted is not PlayerMobile pm)
            {
                from.SendMessage("That is not a player.");
                return;
            }

            from.SendMessage($"Points for {pm.Name}:");
            var systems = PointsSystem.AllSystems;
            for (var i = 0; i < systems.Count; i++)
            {
                var s = systems[i];
                from.SendMessage($"  {s.Loyalty}: {s.GetPoints(pm)}");
            }
        });
    }

    [Usage("AwardPoints <PointsType> <amount>")]
    [Description("Target a player to award points in the given system (debug).")]
    private static void AwardPoints_OnCommand(CommandEventArgs e) => AdjustPoints(e, true);

    [Usage("DeductPoints <PointsType> <amount>")]
    [Description("Target a player to deduct points from the given system (debug).")]
    private static void DeductPoints_OnCommand(CommandEventArgs e) => AdjustPoints(e, false);

    private static void AdjustPoints(CommandEventArgs e, bool award)
    {
        if (e.Length < 2 || !Enum.TryParse<PointsType>(e.GetString(0), true, out var type))
        {
            e.Mobile.SendMessage($"Usage: [{(award ? "Award" : "Deduct")}Points <PointsType> <amount>");
            return;
        }

        var amount = e.GetDouble(1);
        var system = PointsSystem.GetSystemInstance(type);
        if (system == null)
        {
            e.Mobile.SendMessage($"No registered points system for {type}.");
            return;
        }

        e.Mobile.SendMessage("Target a player.");
        e.Mobile.BeginTarget(-1, false, TargetFlags.None, (from, targeted) =>
        {
            if (targeted is not PlayerMobile pm)
            {
                from.SendMessage("That is not a player.");
                return;
            }

            if (award)
            {
                system.AwardPoints(pm, amount, false, false);
                from.SendMessage($"Awarded {amount} {type} points to {pm.Name}; total {system.GetPoints(pm)}.");
            }
            else
            {
                var ok = system.DeductPoints(pm, amount);
                if (ok)
                {
                    from.SendMessage($"Deducted {amount} {type} points from {pm.Name}; total {system.GetPoints(pm)}.");
                }
                else
                {
                    from.SendMessage($"{pm.Name} does not have {amount} {type} points to deduct.");
                }
            }
        });
    }
}
