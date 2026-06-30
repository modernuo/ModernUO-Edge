// Source: ServUO Scripts/Services/Dungeons/CovetousVoidSpawn/Gumps.cs (status block only)
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.VoidPool;

public sealed class VoidPoolGump : DynamicGump
{
    private const int Red = 0x4800;
    private const int Orange = 0xB104;

    private readonly VoidPoolController _controller;
    private readonly PlayerMobile _user;

    public override bool Singleton => true;

    private VoidPoolGump(VoidPoolController controller, PlayerMobile user) : base(50, 50)
    {
        _controller = controller;
        _user = user;
    }

    public static void DisplayTo(PlayerMobile pm, VoidPoolController controller)
    {
        if (pm?.NetState == null || controller == null)
        {
            return;
        }

        pm.SendGump(new VoidPoolGump(controller, pm));
    }

    protected override void BuildLayout(ref DynamicGumpBuilder builder)
    {
        builder.AddPage(0);
        builder.AddBackground(0, 0, 400, 230, 9350);

        // The Void Pool
        builder.AddHtmlLocalized(10, 10, 200, 16, 1152531, Red, false, false);

        // Felucca / Trammel facet name
        builder.AddHtmlLocalized(10, 30, 200, 16, _controller.Map == Map.Felucca ? 1012001 : 1012000, Red, false, false);

        if (_controller.OnGoing)
        {
            // Current Battle:
            builder.AddHtmlLocalized(10, 50, 200, 16, 1152914, Orange, false, false);

            // Wave ~1_WAVE~
            builder.AddHtmlLocalized(180, 50, 200, 16, 1152915, $"{_controller.Wave}", Orange, false, false);
        }
        else
        {
            // Next Battle:
            builder.AddHtmlLocalized(10, 50, 200, 16, 1152916, Orange, false, false);

            if (_controller.NextStart > Core.Now)
            {
                var minutesRemaining = (int)(_controller.NextStart - Core.Now).TotalMinutes;

                // Starts in ~1_MIN~ min.
                builder.AddHtmlLocalized(180, 50, 200, 16, 1152917, $"{minutesRemaining}", Orange, false, false);
            }
        }

        // See Loyalty Menu for Reward Points
        builder.AddHtmlLocalized(10, 70, 380, 16, 1152552, Orange, false, false);

        // See Vela in Cove for rewards
        builder.AddHtmlLocalized(10, 90, 380, 16, 1152553, Orange, false, false);

        // Player's own current score
        var score = _controller.CurrentScore != null && _controller.CurrentScore.TryGetValue(_user, out var s) ? s : 0L;
        builder.AddLabel(10, 110, Orange, "Your Score:");
        builder.AddLabel(100, 110, Orange, $"{score}");
    }

    public override void OnResponse(NetState sender, in RelayInfo info)
    {
        // No buttons in the status-only gump; close on button 0 (window X) is handled by the base.
    }
}
