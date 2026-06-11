using System.Collections.Generic;
using System.Runtime.InteropServices;
using ModernUO.CodeGeneratedEvents;
using Server.Mobiles;

namespace Server.Engines.RewardTitles;

public class RewardTitleSystem : GenericPersistence
{
    private static RewardTitleSystem _persistence;

    private static readonly Dictionary<PlayerMobile, RewardTitleContext> _contexts = new();

    public static void Configure()
    {
        _persistence = new RewardTitleSystem();
    }

    public RewardTitleSystem() : base("RewardTitles", 10)
    {
    }

    [OnEvent(nameof(PlayerMobile.PlayerDeletedEvent))]
    public static void OnPlayerDeleted(Mobile m)
    {
        if (m is PlayerMobile pm)
        {
            _contexts.Remove(pm);
        }
    }

    public static RewardTitleContext GetOrCreate(PlayerMobile player)
    {
        ref var context = ref CollectionsMarshal.GetValueRefOrAddDefault(_contexts, player, out var exists);
        if (!exists)
        {
            context = new RewardTitleContext(player);
        }

        return context;
    }

    public static bool GetContext(PlayerMobile player, out RewardTitleContext context)
    {
        if (player != null && _contexts.TryGetValue(player, out context))
        {
            return true;
        }

        context = null;
        return false;
    }

    public static bool AddTitle(PlayerMobile pm, TextDefinition title) => GetOrCreate(pm).Add(title);

    public static void Select(PlayerMobile pm, int index) => GetOrCreate(pm).Select(index);

    public static TextDefinition GetSelectedTitle(PlayerMobile pm) =>
        GetContext(pm, out var context) ? context.SelectedTitle : null;

    public override void Serialize(IGenericWriter writer)
    {
        writer.WriteEncodedInt(0); // version
        writer.WriteEncodedInt(_contexts.Count);

        foreach (var (m, context) in _contexts)
        {
            writer.Write(m);
            context.Serialize(writer);
        }
    }

    public override void Deserialize(IGenericReader reader)
    {
        reader.ReadEncodedInt(); // version

        var count = reader.ReadEncodedInt();
        for (var i = 0; i < count; ++i)
        {
            var player = reader.ReadEntity<PlayerMobile>();
            var context = new RewardTitleContext(player);
            context.Deserialize(reader);

            if (player != null)
            {
                _contexts[player] = context;
            }
        }
    }
}
