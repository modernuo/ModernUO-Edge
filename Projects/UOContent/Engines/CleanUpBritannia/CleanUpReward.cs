using System;
using Server.Items;

namespace Server.Engines.CleanUpBritannia;

public sealed record CleanUpReward(Type Type, int ItemId, int Cliloc, int Hue, double Cost);

public static class CleanUpBritanniaRewards
{
    // Starter set (PR 1): only items that exist in Edge today. Full catalog ported in later PRs.
    // Every Type here is constructible by the reward gump (parameterless [Constructible] ctor,
    // except ScrollofAlacrity which the gump special-cases).
    public static readonly CleanUpReward[] Rewards =
    [
        new(typeof(LillyPad), 0xDBC, 1023516, 0, 5000),
        new(typeof(LillyPads), 0xDBE, 1023518, 0, 5000),
        new(typeof(TableLamp), 0x49C1, 1151220, 0, 15000),
        new(typeof(Bamboo), 0x246D, 1029324, 0, 15000),
        new(typeof(ScrollofAlacrity), 0x14EF, 1078604, 1195, 20000),
        new(typeof(NestWithEggs), 0x1AD4, 1026868, 2415, 50000),
        new(typeof(ArcheryButteDeed), 0x100B, 1024106, 0, 80000),
    ];
}
