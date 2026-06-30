using Server.Mobiles;

namespace Server.Engines.Points;

public class VoidPool : PointsSystem
{
    private static readonly TextDefinition _name = TextDefinition.Of(1152733);
    private static readonly TextDefinition _title = TextDefinition.Of(1152531);

    public static VoidPool Instance { get; private set; }
    public static bool Enabled { get; private set; }

    public override PointsType Loyalty => PointsType.VoidPool;
    public override TextDefinition Name => _name;
    public override bool AutoAdd => true;
    public override double MaxPoints => double.MaxValue;

    public VoidPool() : base("VoidPool", 13)
    {
    }

    public static void Configure()
    {
        Enabled = ServerConfiguration.GetSetting("voidpool.enabled", false);

        if (Enabled)
        {
            Instance = new VoidPool();
        }
    }

    public static void Enable()
    {
        if (Enabled)
        {
            return;
        }

        Instance ??= new VoidPool();
        Instance.Register();
        Enabled = true;
        ServerConfiguration.SetSetting("voidpool.enabled", true);
    }

    public static void Disable()
    {
        if (!Enabled)
        {
            return;
        }

        Instance?.Unregister();
        Enabled = false;
        ServerConfiguration.SetSetting("voidpool.enabled", false);
    }

    public override TextDefinition GetTitle(PlayerMobile from) => _title;

    public override void SendMessage(PlayerMobile from, double old, double points, bool quest)
    {
        // For your participation in the Battle for the Void Pool on ~1_FACET~, you have received
        // ~2_POINTS~ reward points. ... redeemed by visiting Vela in Cove.
        from.SendLocalizedMessage(1152649, $"{from.Map}\t{(int)points}");
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
