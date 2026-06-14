using Server.Mobiles;

namespace Server.Engines.Points;

public class PointsEntry
{
    [CommandProperty(AccessLevel.GameMaster)]
    public PlayerMobile Player { get; set; }

    [CommandProperty(AccessLevel.GameMaster)]
    public double Points { get; set; }

    public PointsEntry(PlayerMobile pm)
    {
        Player = pm;
    }

    public PointsEntry(PlayerMobile pm, double points)
    {
        Player = pm;
        Points = points;
    }

    public override bool Equals(object obj) => obj is PointsEntry other && other.Player == Player;

    public override int GetHashCode() => Player?.GetHashCode() ?? 0;

    public virtual void Serialize(IGenericWriter writer)
    {
        writer.WriteEncodedInt(0); // version
        writer.Write(Points);
    }

    public virtual void Deserialize(IGenericReader reader)
    {
        reader.ReadEncodedInt(); // version
        Points = reader.ReadDouble();
    }
}
