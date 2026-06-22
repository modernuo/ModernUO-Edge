using System;
using System.Collections.Generic;
using ModernUO.Serialization;
using Server.Collections;
using Server.ContextMenus;
using Server.Engines.CleanUpBritannia;
using Server.Engines.Points;
using Server.Mobiles;
using Server.Multis;

namespace Server.Items;

[SerializationGenerator(0, false)]
public partial class TrashBarrel : Container, IChoppable
{
    private Timer _timer;

    // CUB: in-memory only; in-flight points are not persisted across a restart (documented divergence).
    private readonly Dictionary<Item, (Mobile Dropper, double Points)> _cleanup = [];

    [Constructible]
    public TrashBarrel() : base(0xE77)
    {
        Hue = 0x3B2;
        Movable = false;
    }

    public override int LabelNumber => 1041064; // a trash barrel

    public override int DefaultMaxWeight => 0; // A value of 0 signals unlimited weight

    public override bool IsDecoContainer => false;

    public override void GetContextMenuEntries(Mobile from, ref PooledRefList<ContextMenuEntry> list)
    {
        base.GetContextMenuEntries(from, ref list);

        if (CleanUpBritanniaData.Enabled && from is PlayerMobile)
        {
            list.Add(new AppraiseForCleanupEntry());
        }
    }

    public void OnChop(Mobile from)
    {
        var house = BaseHouse.FindHouseAt(from);

        if (house?.IsCoOwner(from) == true)
        {
            Effects.PlaySound(Location, Map, 0x3B3);
            from.SendLocalizedMessage(500461); // You destroy the item.
            Destroy();
        }
    }

    [AfterDeserialization(false)]
    private void AfterDeserialization()
    {
        if (Items.Count > 0)
        {
            _timer = new EmptyTimer(this);
            _timer.Start();
        }
    }

    private void InvalidateContents(Mobile from)
    {
        if (TotalItems >= 50)
        {
            Empty(501478); // The trash is full!  Emptying!
        }
        else
        {
            SendLocalizedMessageTo(from, 1010442); // The item will be deleted in three minutes

            if (_timer != null)
            {
                _timer.Stop();
            }
            else
            {
                _timer = new EmptyTimer(this);
            }

            _timer.Start();
        }
    }

    public override bool OnDragDrop(Mobile from, Item dropped)
    {
        if (base.OnDragDrop(from, dropped))
        {
            InvalidateContents(from);
            AccumulateCleanup(from, dropped);
            return true;
        }

        return false;
    }

    public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
    {
        if (base.OnDragDropInto(from, item, p))
        {
            InvalidateContents(from);
            AccumulateCleanup(from, item);
            return true;
        }

        return false;
    }

    public void Empty(int message)
    {
        AwardCleanup();

        var items = Items;

        if (items.Count > 0)
        {
            PublicOverheadMessage(MessageType.Regular, 0x3B2, message);

            for (var i = items.Count - 1; i >= 0; --i)
            {
                if (i >= items.Count)
                {
                    continue;
                }

                items[i].Delete();
            }
        }

        _timer?.Stop();
        _timer = null;
    }

    private void AccumulateCleanup(Mobile from, Item item)
    {
        if (!CleanUpBritanniaData.Enabled || from == null)
        {
            return;
        }

        var points = CleanUpBritanniaData.GetPoints(item);

        if (points > 0)
        {
            _cleanup[item] = (from, points);
        }
    }

    private void AwardCleanup()
    {
        if (_cleanup.Count == 0)
        {
            return;
        }

        var instance = CleanUpBritanniaData.Instance;

        if (instance != null)
        {
            // First pass aggregates per dropper (one summed message per dropper even when they
            // turned in several items); the award pass then runs once per unique dropper.
            // AwardPoints has no side effect on _cleanup, so the two passes are independent.
            Dictionary<Mobile, (double Points, int Count)> totals = [];

            foreach (var (item, info) in _cleanup)
            {
                if (item.Deleted || !item.IsChildOf(this) || info.Dropper?.Deleted != false)
                {
                    continue;
                }

                var cur = totals.TryGetValue(info.Dropper, out var t) ? t : default;
                totals[info.Dropper] = (cur.Points + info.Points, cur.Count + 1);
            }

            foreach (var (dropper, t) in totals)
            {
                instance.AwardPoints(dropper, t.Points, false, false);

                // You have received approximately ~1_VALUE~ points for turning in ~2_COUNT~ items for Clean Up Britannia.
                dropper.SendLocalizedMessage(1151280, $"{(int)t.Points}\t{t.Count}");
            }
        }

        _cleanup.Clear();
    }

    private class EmptyTimer : Timer
    {
        private TrashBarrel _barrel;

        public EmptyTimer(TrashBarrel barrel) : base(TimeSpan.FromMinutes(3.0)) => _barrel = barrel;

        protected override void OnTick()
        {
            _barrel.Empty(501479); // Emptying the trashcan!
        }
    }
}
