using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ModernUO.CodeGeneratedEvents;
using Server.Mobiles;

namespace Server.Engines.Points;

public abstract class PointsSystem : GenericPersistence
{
    private static readonly List<PointsSystem> _allSystems = new();

    protected readonly Dictionary<PlayerMobile, PointsEntry> _entries = new();

    protected PointsSystem(string persistenceName, int priority) : base(persistenceName, priority)
    {
        _allSystems.Add(this);
    }

    public abstract TextDefinition Name { get; }
    public abstract PointsType Loyalty { get; }
    public abstract double MaxPoints { get; }

    public virtual bool AutoAdd => false;
    public virtual bool ShowOnLoyaltyGump => true;

    public static IReadOnlyList<PointsSystem> AllSystems => _allSystems;

    public static PointsSystem GetSystemInstance(PointsType type)
    {
        for (var i = 0; i < _allSystems.Count; i++)
        {
            if (_allSystems[i].Loyalty == type)
            {
                return _allSystems[i];
            }
        }

        return null;
    }

    public double GetPoints(Mobile from) => GetEntry(from)?.Points ?? 0.0;

    public virtual void AwardPoints(Mobile from, double points, bool quest = false, bool message = true)
    {
        if (from is not PlayerMobile pm || points <= 0)
        {
            return;
        }

        var entry = GetOrCreateEntry(pm);
        var old = entry.Points;
        entry.Points = Math.Min(MaxPoints, entry.Points + points);

        if (message)
        {
            SendMessage(pm, old, points, quest);
        }
    }

    public virtual bool DeductPoints(Mobile from, double points, bool message = false)
    {
        if (from is not PlayerMobile pm)
        {
            return false;
        }

        if (points <= 0)
        {
            return false;
        }

        var entry = GetEntry(pm);
        if (entry == null || entry.Points < points)
        {
            return false;
        }

        entry.Points -= points;

        if (message)
        {
            // Your loyalty to ~1_GROUP~ has decreased by ~2_AMOUNT~
            pm.SendLocalizedMessage(1115921, $"{Name}\t{(int)points}");
        }

        return true;
    }

    public virtual void SendMessage(PlayerMobile from, double old, double points, bool quest)
    {
        if (quest)
        {
            // You have received ~1_val~ loyalty points as a reward for completing the quest.
            from.SendLocalizedMessage(1113719, $"{(int)points}", 0x26);
        }
        else
        {
            // Your loyalty to ~1_GROUP~ has increased by ~2_AMOUNT~
            from.SendLocalizedMessage(1115920, $"{Name}\t{(int)points}");
        }
    }

    public PointsEntry GetEntry(Mobile from)
    {
        if (from is not PlayerMobile pm)
        {
            return null;
        }

        if (_entries.TryGetValue(pm, out var entry))
        {
            return entry;
        }

        return AutoAdd ? GetOrCreateEntry(pm) : null;
    }

    public TEntry GetPlayerEntry<TEntry>(Mobile from, bool create = false) where TEntry : PointsEntry
    {
        if (from is not PlayerMobile pm)
        {
            return null;
        }

        if (_entries.TryGetValue(pm, out var entry))
        {
            return entry as TEntry;
        }

        return AutoAdd || create ? GetOrCreateEntry(pm) as TEntry : null;
    }

    public PointsEntry GetOrCreateEntry(PlayerMobile pm)
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_entries, pm, out var exists);
        if (!exists)
        {
            entry = CreateEntry(pm);
            OnPlayerAdded(pm);
        }

        return entry;
    }

    // Called during deserialization with a possibly-null player (deleted-player save records).
    // Overrides must tolerate a null pm; the base Deserialize discards the entry afterward if the player is gone.
    protected virtual PointsEntry CreateEntry(PlayerMobile pm) => new(pm);

    public virtual void OnPlayerAdded(PlayerMobile pm)
    {
    }

    public virtual void ProcessKill(Mobile victim, Mobile damager)
    {
    }

    public virtual void ProcessQuest(Mobile from, Type questType)
    {
    }

    public virtual TextDefinition GetTitle(PlayerMobile from) => null;

    [OnEvent(nameof(PlayerMobile.PlayerDeletedEvent))]
    public static void OnPlayerDeleted(Mobile m)
    {
        if (m is not PlayerMobile pm)
        {
            return;
        }

        for (var i = 0; i < _allSystems.Count; i++)
        {
            _allSystems[i]._entries.Remove(pm);
        }
    }

    public override void Serialize(IGenericWriter writer)
    {
        writer.WriteEncodedInt(0); // version
        writer.WriteEncodedInt(_entries.Count);

        foreach (var (pm, entry) in _entries)
        {
            writer.Write(pm);
            entry.Serialize(writer);
        }
    }

    public override void Deserialize(IGenericReader reader)
    {
        reader.ReadEncodedInt(); // version

        var count = reader.ReadEncodedInt();
        for (var i = 0; i < count; i++)
        {
            var player = reader.ReadEntity<PlayerMobile>();
            var entry = CreateEntry(player);
            entry.Deserialize(reader);

            if (player != null)
            {
                _entries[player] = entry;
            }
        }
    }
}
