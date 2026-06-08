// Source: ServUO Scripts/Services/Astronomy/BrassOrrery.cs
using ModernUO.Serialization;

namespace Server.Items
{
    [SerializationGenerator(0)]
    public partial class BrassOrrery : Item
    {
        [SerializableField(0)]
        [SerializedCommandProperty(AccessLevel.GameMaster)]
        private bool _active;

        public override int LabelNumber => 1125363; // orrery

        [Constructible]
        public BrassOrrery() : base(0xA17C)
        {
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m.InRange(GetWorldLocation(), 2))
            {
                ToggleActivation(m);
            }
        }

        public void ToggleActivation(Mobile m)
        {
            if (Active)
            {
                ItemID = 0xA17C;
                m.PlaySound(0x1E2);
                Active = false;
            }
            else
            {
                ItemID = 0xA17B;
                m.PlaySound(480);
                Active = true;
            }
        }
    }
}
