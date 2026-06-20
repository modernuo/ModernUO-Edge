using Server.ContextMenus;
using Server.Engines.Points;
using Server.Targeting;

namespace Server.Engines.CleanUpBritannia;

public class AppraiseForCleanupEntry : ContextMenuEntry
{
    private readonly Mobile _from;

    public AppraiseForCleanupEntry(Mobile from) : base(1151298, 2) // Appraise for Cleanup
    {
        _from = from;
    }

    public override void OnClick(Mobile from, IEntity target)
    {
        _from.Target = new AppraiseForCleanupTarget();

        // Target items to see how many Clean Up Britannia points you will receive for throwing them away.
        _from.SendLocalizedMessage(1151299);
    }
}

public class AppraiseForCleanupTarget : Target
{
    public AppraiseForCleanupTarget() : base(-1, true, TargetFlags.None)
    {
    }

    protected override void OnTarget(Mobile from, object targeted)
    {
        if (targeted is not Item item || !item.IsChildOf(from))
        {
            from.SendLocalizedMessage(1151271); // This item has no turn-in value for Clean Up Britannia.
            from.Target = new AppraiseForCleanupTarget();
            return;
        }

        var points = CleanUpBritanniaData.GetPoints(item);

        if (points == 0)
        {
            from.SendLocalizedMessage(1151271); // no turn-in value
        }
        else if (points < 1)
        {
            from.SendLocalizedMessage(1151272); // worth less than one point
        }
        else if (points == 1)
        {
            from.SendLocalizedMessage(1151273); // worth approximately one point
        }
        else
        {
            from.SendLocalizedMessage(1151274, $"{(int)points}"); // worth approximately ~1_VALUE~ points
        }

        from.Target = new AppraiseForCleanupTarget(); // re-arm; loops until ESC
    }
}
