// Source: ServUO Scripts/Services/Astronomy/PrimerOnBritannianAstronomy.cs
using ModernUO.Serialization;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
    [SerializationGenerator(0)]
    public partial class PrimerOnBritannianAstronomy : Item
    {
        public override int LabelNumber => 1158515; // Looking to the Heavens: A Primer on Britannian Astronomy

        [Constructible]
        public PrimerOnBritannianAstronomy() : base(0xFF0) => Hue = 298;

        public override void OnDoubleClick(Mobile m)
        {
            if (m is PlayerMobile && m.InRange(GetWorldLocation(), 3))
            {
                m.SendGump(new PrimerGump());
            }
        }

        private class PrimerGump : StaticGump<PrimerGump>
        {
            public PrimerGump() : base(100, 100)
            {
            }

            protected override void BuildLayout(ref StaticGumpBuilder builder)
            {
                builder.AddPage();
                builder.AddImage(0, 0, 0x761C);
                builder.AddImage(95, 40, 0x69E);
                builder.AddHtmlLocalized(115, 200, 350, 400, 1158516, "#1158516", 0x1, false, true);
            }
        }
    }
}
