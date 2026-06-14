using Server.Mobiles;

namespace Server.Engines.Points;

public class ShameCrystals : PointsSystem
{
    private static ShameCrystals _instance;

    public static void Configure()
    {
        _instance = new ShameCrystals();
    }

    public override PointsType Loyalty => PointsType.ShameCrystals;
    public override TextDefinition Name => TextDefinition.Of(1151673);
    public override bool AutoAdd => true;
    public override double MaxPoints => double.MaxValue;

    public ShameCrystals() : base("ShameCrystals", 11)
    {
    }

    public override void SendMessage(PlayerMobile from, double old, double points, bool quest)
    {
        // You gain ~1_AMT~ dungeon points for ~2_NAME~. Your total is now ~3_TOTAL~.
        from.SendLocalizedMessage(1151634, $"{(int)points}\tShame\t{(int)(old + points)}");
    }

    public override TextDefinition GetTitle(PlayerMobile from) => TextDefinition.Of(1123444);

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
