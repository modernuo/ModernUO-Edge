using Server.Engines.Points;
using Server.Mobiles;

namespace Server.Engines.CleanUpBritannia;

public static class CleanUpBritanniaGeneration
{
    // New Magincia bank, Trammel — canonical OSI/ServUO officer location.
    private static readonly Point3D OfficerLocation = new(3712, 2218, 20);

    public static void Configure()
    {
        CommandSystem.Register("GenCleanUpBritannia", AccessLevel.Administrator, Generate);
        CommandSystem.Register("DelCleanUpBritannia", AccessLevel.Administrator, Delete);
    }

    [Usage("GenCleanUpBritannia")]
    [Description("Enables Clean Up Britannia (opt-in, persisted) and places the Cleanup Officer if absent.")]
    private static void Generate(CommandEventArgs e)
    {
        CleanUpBritanniaData.Enable();

        var map = Map.Trammel;
        var found = false;
        foreach (var m in map.GetMobilesAt<TheCleanupOfficer>(OfficerLocation))
        {
            if (m.Z == OfficerLocation.Z)
            {
                found = true;
                break;
            }
        }

        var placed = 0;
        if (!found)
        {
            var officer = new TheCleanupOfficer();
            officer.MoveToWorld(OfficerLocation, map);
            placed = 1;
        }

        e.Mobile.SendMessage($"Clean Up Britannia: enabled; placed {placed} officer(s).");
    }

    [Usage("DelCleanUpBritannia")]
    [Description("Removes the Cleanup Officer.")]
    private static void Delete(CommandEventArgs e)
    {
        var map = Map.Trammel;
        var deleted = 0;
        foreach (var m in map.GetMobilesAt<TheCleanupOfficer>(OfficerLocation))
        {
            if (m.Z == OfficerLocation.Z)
            {
                m.Delete();
                deleted++;
                break;
            }
        }

        e.Mobile.SendMessage($"Clean Up Britannia: deleted {deleted} officer(s).");
    }
}
