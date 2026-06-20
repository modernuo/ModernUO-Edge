using Server.Mobiles;

namespace Server.Engines.Points;

public class CleanUpBritanniaData : PointsSystem
{
    public static CleanUpBritanniaData Instance { get; private set; }
    public static bool Enabled { get; private set; }

    public override PointsType Loyalty => PointsType.CleanUpBritannia;
    public override TextDefinition Name => null;
    public override bool AutoAdd => true;
    public override double MaxPoints => double.MaxValue;
    public override bool ShowOnLoyaltyGump => false;

    public CleanUpBritanniaData() : base("CleanUpBritannia", 12)
    {
    }

    public static void Configure()
    {
        Enabled = ServerConfiguration.GetSetting("cleanupbritannia.enabled", false);

        if (Enabled)
        {
            Instance = new CleanUpBritanniaData();
        }
    }

    public static void Enable()
    {
        if (Enabled)
        {
            return;
        }

        Instance ??= new CleanUpBritanniaData();
        Instance.Register();
        Enabled = true;
        ServerConfiguration.SetSetting("cleanupbritannia.enabled", true);
    }

    public static void Disable()
    {
        if (!Enabled)
        {
            return;
        }

        Instance?.Unregister();
        Enabled = false;
        ServerConfiguration.SetSetting("cleanupbritannia.enabled", false);
    }

    public override void SendMessage(PlayerMobile from, double old, double points, bool quest)
    {
        // Your Clean Up Britannia point total is now ~1_VALUE~!
        from.SendLocalizedMessage(1151281, $"{(int)GetPoints(from)}");
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);
        writer.Write(0); // version
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);
        reader.ReadInt(); // version
    }
}
