using Server.Mobiles;

namespace Server.Engines.Points;

public class DespiseCrystals : PointsSystem
{
    private static DespiseCrystals _instance;

    public static void Configure()
    {
        _instance = new DespiseCrystals();
    }

    public override PointsType Loyalty => PointsType.DespiseCrystals;
    public override TextDefinition Name => TextDefinition.Of(1151673);
    public override bool AutoAdd => true;
    public override double MaxPoints => double.MaxValue;

    public DespiseCrystals() : base("DespiseCrystals", 10)
    {
    }

    public override void SendMessage(PlayerMobile from, double old, double points, bool quest)
    {
        // You have gained ~1_AMT~ Dungeon Crystal Points of Despise.
        from.SendLocalizedMessage(1153423, $"{(int)points}");
    }

    public override TextDefinition GetTitle(PlayerMobile from) => TextDefinition.Of(1123418);

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
