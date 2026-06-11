# RewardTitle System + TitlesGump — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a reusable per-player RewardTitle foundation (earned, selectable, displayed titles) + port ServUO's TitlesGump (RewardTitles functional, Champion as a display toggle, other categories deferred), and restore `AstronomerTitleDeed` end-to-end.

**Architecture:** Mirror Edge's `ChampionTitleSystem` — a `GenericPersistence` singleton keyed by `PlayerMobile`, with a per-player `[SerializationGenerator] [PropertyObject]` context holding a `List<TextDefinition>` + selected index. A `BaseRewardTitleDeed` grants titles; the selected title renders in `PlayerMobile.GetProperties`; a context-menu entry opens the ported `TitlesGump`.

**Tech Stack:** .NET 10, ModernUO source-generated serialization, `GenericPersistence`, `DynamicGump`, xUnit.

**Source of truth:** ServUO `Scripts/Items/Consumables/BaseRewardTitleDeed.cs`, `Scripts/Services/Astronomy/AstronomerTitleDeed.cs`, `Scripts/Gumps/TitlesMenu.cs`. Edge templates: `Engines/CannedEvil/ChampionTitleSystem.cs` + `ChampionTitleContext.cs`, `Items/Deeds/HairRestylingDeed.cs`, `Gumps/Guilds/New Guild System/BaseGuildListGump.cs`.

**Design doc:** `docs/superpowers/specs/2026-06-11-reward-titles-design.md`.

**Conventions (apply throughout):** `[SerializationGenerator(version)]` with NO `encoded` arg (CLAUDE.md rule 9); braces on all control flow (rule 15); no `Console`/LINQ-on-hot-paths/`Tuple`/`StringBuilder`; gumps use private ctor + static `DisplayTo` + `Singleton => true` (rule 13); after adding `[SerializationGenerator]` types, regenerate migration schemas (Task 8). `TextDefinition` (`Projects/Server/Text/TextDefinition.cs`) is a class with value-equality; `Number`/`String`/`IsEmpty`; binary-serializable via `IGenericWriter.Write(TextDefinition)` / `IGenericReader.ReadTextDefinition()`.

---

## File structure

| File | Responsibility |
|---|---|
| `Projects/UOContent/Engines/RewardTitles/RewardTitleContext.cs` | per-player earned titles list + selected index (codegen serialized) |
| `Projects/UOContent/Engines/RewardTitles/RewardTitleSystem.cs` | `GenericPersistence` singleton + per-player registry + static helpers |
| `Projects/UOContent/Items/Deeds/BaseRewardTitleDeed.cs` | abstract title-granting deed |
| `Projects/UOContent/Engines/Astronomy/AstronomerTitleDeed.cs` | the "Astronomer" deed |
| `Projects/UOContent/Gumps/TitlesMenu.cs` | `TitlesGump` (matrix UI) + `TitlesMenuEntry` open hook |
| `Projects/UOContent/Mobiles/PlayerMobile.cs` (modify) | render hook in `GetProperties` + context-menu entry |
| `Projects/UOContent/Engines/Astronomy/Willebrord.cs` (modify) | grant the deed on discovery |
| `dev-docs/servuo-content-migration-status.md` (modify) | tick the gap |
| `Projects/UOContent.Tests/Tests/Engines/RewardTitles/RewardTitleContextTests.cs` | context round-trip + dedupe + select |

---

## Task 1: Branch + green baseline

**Files:** none (setup)

- [ ] **Step 1: Confirm branch + clean baseline**

Run: `cd /c/dev/ModernUO-Edge && git branch --show-current`
Expected: `feat/reward-titles` (the spec was already committed here). If not, `git checkout feat/reward-titles`.

Run: `dotnet build`
Expected: Build succeeded, 0 errors. Stop and fix environment if red.

---

## Task 2: `RewardTitleContext` (test-first)

A per-player `[SerializationGenerator(0)] [PropertyObject]` holding the earned titles + selected index. The `PlayerMobile` key is NOT serialized by the context (the system serializes it), mirroring `ChampionTitleContext`.

**Files:**
- Create: `Projects/UOContent/Engines/RewardTitles/RewardTitleContext.cs`
- Test: `Projects/UOContent.Tests/Tests/Engines/RewardTitles/RewardTitleContextTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Projects/UOContent.Tests/Tests/Engines/RewardTitles/RewardTitleContextTests.cs`:

```csharp
using System;
using Server;
using Server.Engines.RewardTitles;
using Xunit;

namespace UOContent.Tests;

public class RewardTitleContextTests
{
    [Fact]
    public void Add_DedupesAndReports()
    {
        var ctx = new RewardTitleContext(null);

        Assert.True(ctx.Add(1158523));            // cliloc title "Astronomer"
        Assert.False(ctx.Add(1158523));           // duplicate cliloc -> false
        Assert.True(ctx.Add("Custom Title"));     // string title
        Assert.False(ctx.Add("Custom Title"));    // duplicate string -> false
        Assert.Equal(2, ctx.Titles.Count);
    }

    [Fact]
    public void Select_ClampsAndHides()
    {
        var ctx = new RewardTitleContext(null);
        ctx.Add(1158523);

        Assert.Null(ctx.Selected);                // default -1 = hidden
        ctx.Select(0);
        Assert.Equal(1158523, ctx.Selected.Number);
        ctx.Select(-1);
        Assert.Null(ctx.Selected);                // hidden
        ctx.Select(99);                           // out of range -> ignored
        Assert.Null(ctx.Selected);
    }

    [Fact]
    public void Serialize_RoundTrips()
    {
        var ctx = new RewardTitleContext(null);
        ctx.Add(1158523);
        ctx.Add("Custom Title");
        ctx.Select(1);

        var writer = new BufferWriter(true);
        ctx.Serialize(writer);

        var buffer = new byte[writer.Position];
        writer.Buffer.AsSpan(0, (int)writer.Position).CopyTo(buffer);

        var reader = new BufferReader(buffer);
        var loaded = new RewardTitleContext(null);
        loaded.Deserialize(reader);

        Assert.Equal(2, loaded.Titles.Count);
        Assert.Equal(1158523, loaded.Titles[0].Number);
        Assert.Equal("Custom Title", loaded.Titles[1].String);
        Assert.Equal("Custom Title", loaded.Selected.String);
        Assert.Equal(buffer.Length, reader.Position);
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test Projects/UOContent.Tests --filter RewardTitleContextTests`
Expected: FAIL — `RewardTitleContext` does not exist.

- [ ] **Step 3: Create `RewardTitleContext.cs`**

```csharp
using System.Collections.Generic;
using ModernUO.Serialization;
using Server.Mobiles;

namespace Server.Engines.RewardTitles
{
    [PropertyObject]
    [SerializationGenerator(0)]
    public partial class RewardTitleContext
    {
        [SerializableField(0, setter: "private")]
        private List<TextDefinition> _titles;

        [SerializableField(1, setter: "private")]
        private int _selected;

        private readonly PlayerMobile _player;

        public PlayerMobile Player => _player;

        public RewardTitleContext(PlayerMobile player)
        {
            _player = player;
            _titles = new List<TextDefinition>();
            _selected = -1;
        }

        public IReadOnlyList<TextDefinition> Titles => _titles;

        public TextDefinition Selected =>
            _selected >= 0 && _selected < _titles.Count ? _titles[_selected] : null;

        public bool Add(TextDefinition title)
        {
            if (title == null || title.IsEmpty || _titles.Contains(title))
            {
                return false;
            }

            _titles.Add(title);
            return true;
        }

        public void Select(int index)
        {
            if (index >= -1 && index < _titles.Count)
            {
                _selected = index;
            }
        }

        public bool Remove(TextDefinition title)
        {
            var idx = _titles.IndexOf(title);
            if (idx < 0)
            {
                return false;
            }

            _titles.RemoveAt(idx);

            if (_selected == idx)
            {
                _selected = -1;
            }
            else if (_selected > idx)
            {
                _selected--;
            }

            return true;
        }
    }
}
```

Notes:
- `_player` is `readonly` and NOT a `[SerializableField]` — the system writes the player key. The ctor `RewardTitleContext(PlayerMobile player)` is also how the system reconstructs it on load (`new RewardTitleContext(reader.ReadEntity<PlayerMobile>())` then `Deserialize`). Tests pass `null` (no player needed for context logic).
- `[SerializableField(setter: "private")]` generates public `Titles`/`Selected`... wait — the generator emits a property named from the field. `_titles` → `Titles` (a `List<TextDefinition>`), `_selected` → `Selected` (int). That COLLIDES with the hand-written `Titles` (IReadOnlyList) and `Selected` (TextDefinition) properties above. RESOLVE: rename the public accessors to avoid collision — keep the generated `Titles`/`Selected` OFF by not exposing them, OR name the manual ones differently. SIMPLEST: drop `setter: "private"` is irrelevant; the generated property name is the problem. Use a leading-underscore field with a name that generates a non-colliding property, OR mark the fields so no property is generated. The clean fix: name the manual read-only views differently (`TitleList`, `SelectedTitle`) so they don't collide with generated `Titles`/`Selected`. Update the test + all consumers accordingly. **Decision:** make the generated property the source of truth — remove the manual `Titles`/`Selected` properties, and instead expose `IReadOnlyList<TextDefinition> TitleList => _titles;` and `TextDefinition SelectedTitle => _selected >= 0 && _selected < _titles.Count ? _titles[_selected] : null;`. Update the test (`loaded.TitleList`, `loaded.SelectedTitle`) and every later reference in this plan from `Titles`→`TitleList` and `Selected`→`SelectedTitle`. (The generated `Titles`/`Selected` properties still exist for serialization/CommandProperty; consumers use the typed views.)

Build first to read the generated property names from `obj/.../RewardTitleContext.Serialization.g.cs` and confirm there is no naming collision; adjust the manual view names if needed.

- [ ] **Step 4: Update the test for the resolved names**

In `RewardTitleContextTests.cs`, change `ctx.Titles` → `ctx.TitleList`, `ctx.Selected` → `ctx.SelectedTitle`, `loaded.Titles` → `loaded.TitleList`, `loaded.Selected` → `loaded.SelectedTitle`.

- [ ] **Step 5: Run tests**

Run: `dotnet test Projects/UOContent.Tests --filter RewardTitleContextTests`
Expected: PASS (3 tests). If the generator does NOT serialize `List<TextDefinition>` (round-trip fails), replace the `[SerializableField(0)] _titles` with a manual serialize: keep `_titles` a plain field, add `[SerializableField]` only for `_selected`, and hand-write within the class a `void SerializeTitles(IGenericWriter w)` / `DeserializeTitles(IGenericReader r)` that writes `w.WriteEncodedInt(_titles.Count)` + `w.Write(td)` each and reads back via `r.ReadTextDefinition()`, invoked from `[AfterSerialization]`/`[AfterDeserialization]` hooks — but try the generated path first; `Write(TextDefinition)` support means it should work.

- [ ] **Step 6: Commit**

```bash
git add Projects/UOContent/Engines/RewardTitles/RewardTitleContext.cs \
        Projects/UOContent.Tests/Tests/Engines/RewardTitles/RewardTitleContextTests.cs
git commit -m "feat(rewardtitles): RewardTitleContext (earned titles + selection)"
```

---

## Task 3: `RewardTitleSystem` (GenericPersistence)

Mirror `ChampionTitleSystem` exactly (per-player dictionary, Configure, OnPlayerDeleted, Serialize/Deserialize) + add static helpers the deed/gump/render-hook use.

**Files:**
- Create: `Projects/UOContent/Engines/RewardTitles/RewardTitleSystem.cs`

- [ ] **Step 1: Create the system**

```csharp
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ModernUO.CodeGeneratedEvents;
using Server.Mobiles;

namespace Server.Engines.RewardTitles
{
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

        // --- helpers used by the deed / gump / render hook ---

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
}
```

Notes: `context.Serialize`/`Deserialize` are the generated methods from Task 2 (public). The `if (player != null)` guard drops contexts whose player was deleted (ChampionTitleSystem omits this guard; we add it to avoid a null key). No `Initialize()` needed (no timer).

- [ ] **Step 2: Build**

Run: `dotnet build Projects/UOContent`
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add Projects/UOContent/Engines/RewardTitles/RewardTitleSystem.cs
git commit -m "feat(rewardtitles): RewardTitleSystem GenericPersistence + helpers"
```

---

## Task 4: `BaseRewardTitleDeed` + `AstronomerTitleDeed`

**Files:**
- Create: `Projects/UOContent/Items/Deeds/BaseRewardTitleDeed.cs`
- Create: `Projects/UOContent/Engines/Astronomy/AstronomerTitleDeed.cs`

- [ ] **Step 1: Create `BaseRewardTitleDeed.cs`**

Port from ServUO `Scripts/Items/Consumables/BaseRewardTitleDeed.cs`, using the codegen serialization + `RewardTitleSystem`.

```csharp
// Source: ServUO Scripts/Items/Consumables/BaseRewardTitleDeed.cs
using ModernUO.Serialization;
using Server.Engines.RewardTitles;
using Server.Mobiles;

namespace Server.Items
{
    [SerializationGenerator(0)]
    public abstract partial class BaseRewardTitleDeed : Item
    {
        public override int LabelNumber => 1155604; // A Deed for a Reward Title

        public abstract TextDefinition Title { get; }

        public BaseRewardTitleDeed() : base(5360)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from is not PlayerMobile pm)
            {
                return;
            }

            if (!IsChildOf(pm.Backpack))
            {
                pm.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            if (Title.IsEmpty)
            {
                return;
            }

            if (RewardTitleSystem.AddTitle(pm, Title))
            {
                pm.SendLocalizedMessage(1155605, Title.ToString()); // Thou hath been bestowed the title ~1_TITLE~!
                Delete();
            }
            else
            {
                pm.SendLocalizedMessage(1073626); // You already have that title!
            }
        }

        public override void GetProperties(IPropertyList list)
        {
            base.GetProperties(list);
            list.Add(1114057, Title.ToString()); // ~1_NOTHING~
        }
    }
}
```

Note: `abstract partial` + `[SerializationGenerator(0)]` is fine (the generator emits serialization for the concrete subclasses). `[Constructible]` is NOT on the abstract base (it can't be constructed); subclasses get it. `5360` is the ServUO deed item id.

- [ ] **Step 2: Create `AstronomerTitleDeed.cs`**

```csharp
// Source: ServUO Scripts/Services/Astronomy/AstronomerTitleDeed.cs
using ModernUO.Serialization;

namespace Server.Items
{
    [SerializationGenerator(0)]
    public partial class AstronomerTitleDeed : BaseRewardTitleDeed
    {
        public override TextDefinition Title => 1158523; // Astronomer

        [Constructible]
        public AstronomerTitleDeed()
        {
        }
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build Projects/UOContent`
Expected: Build succeeded. (If the generator objects to `[SerializationGenerator]` on an abstract base, move the attribute to only the concrete `AstronomerTitleDeed` and leave `BaseRewardTitleDeed` a plain `abstract partial class : Item` without the attribute — the concrete class's generator covers the inherited Item serialization. Verify which the generator wants and adjust.)

- [ ] **Step 4: Commit**

```bash
git add Projects/UOContent/Items/Deeds/BaseRewardTitleDeed.cs \
        Projects/UOContent/Engines/Astronomy/AstronomerTitleDeed.cs
git commit -m "feat(rewardtitles): BaseRewardTitleDeed + AstronomerTitleDeed"
```

---

## Task 5: `TitlesGump` + `TitlesMenuEntry`

Port ServUO's `Scripts/Gumps/TitlesMenu.cs` matrix UI as an Edge `DynamicGump`, wiring only the **RewardTitles** (functional) and **Champion** (display toggle) categories; other categories render only when backed (hidden now). Rewrite the response path as a `switch` dispatcher.

**Files:**
- Create: `Projects/UOContent/Gumps/TitlesMenu.cs`

- [ ] **Step 1: Create the enums + gump skeleton**

```csharp
// Source: ServUO Scripts/Gumps/TitlesMenu.cs
using Server.ContextMenus;
using Server.Engines.CannedEvil;
using Server.Engines.RewardTitles;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps
{
    public enum TitleType
    {
        None,
        PaperdollPrefix,
        PaperdollSuffix,
        OverheadName,
        SubTitles
    }

    public enum TitleCategory
    {
        None,
        FameKarma,
        Skills,
        Guild,
        Champion,
        RewardTitles,
        Veteran
    }

    public class TitlesGump : DynamicGump
    {
        private readonly PlayerMobile _player;
        private TitleType _type;
        private TitleCategory _category;
        private int _page;

        public override bool Singleton => true;

        private TitlesGump(PlayerMobile pm) : base(50, 50)
        {
            _player = pm;
            _type = TitleType.SubTitles;       // default to the only fully-wired type
            _category = TitleCategory.RewardTitles;
            _page = 0;
        }

        public static void DisplayTo(PlayerMobile pm)
        {
            if (pm?.NetState == null)
            {
                return;
            }

            pm.SendGump(new TitlesGump(pm));
        }

        protected override void BuildLayout(ref DynamicGumpBuilder builder)
        {
            // Port the static panel layout from ServUO TitlesMenu.cs AddGumpLayout (lines 85-196):
            //   background 9200 (540x350); header bar; TYPES panel; CATEGORIES panel;
            //   SELECTIONS/DESCRIPTION pane; footer with CLOSE (1060675).
            //   Transcribe the AddImageTiled/AddAlphaRegion/AddHtmlLocalized coordinates verbatim,
            //   prefixing each with `builder.`.
            builder.AddPage();
            // ... transcribed panels ...

            BuildTypeButtons(ref builder);
            BuildCategoryButtons(ref builder);
            BuildSelections(ref builder);
        }
    }
}
```

- [ ] **Step 2: Type + category buttons (conditional, with deferred seams)**

Add these methods to `TitlesGump`. Only show categories backed in Edge.

```csharp
        // TYPE buttons: ids 10001..10004. Only the wired types are interactive; others are seams.
        private void BuildTypeButtons(ref DynamicGumpBuilder builder)
        {
            // PaperdollSuffix (Champion) and SubTitles (RewardTitles) are the wired types.
            // Transcribe the TYPES-panel button coordinates from ServUO (lines ~100-130);
            // use button ids 10001 (PaperdollPrefix), 10002 (PaperdollSuffix),
            // 10003 (OverheadName), 10004 (SubTitles). Clicking sets _type, resets _category/_page.
        }

        // CATEGORY buttons for the current _type. Render a category button ONLY when it has data.
        private void BuildCategoryButtons(ref DynamicGumpBuilder builder)
        {
            switch (_type)
            {
                case TitleType.PaperdollSuffix:
                    {
                        // Champion category — only if the player has any champion title.
                        if (ChampionTitleSystem.GetChampionTitleLabel(_player) > 0)
                        {
                            // category button id 10101 -> _category = Champion
                        }
                        // Deferred: Skills category (needs storage fields) -> not rendered.
                        break;
                    }
                case TitleType.SubTitles:
                    {
                        // RewardTitles category — only if the player has any earned title.
                        if (RewardTitleSystem.GetContext(_player, out var ctx) && ctx.TitleList.Count > 0)
                        {
                            // category button id 10105 -> _category = RewardTitles
                        }
                        // Deferred: Skills/Guild/Veteran categories -> not rendered.
                        break;
                    }
                // Deferred: PaperdollPrefix (FameKarma), OverheadName -> no backed categories yet.
            }
        }
```

- [ ] **Step 3: Selections pane — RewardTitles + Champion**

```csharp
        private void BuildSelections(ref DynamicGumpBuilder builder)
        {
            switch (_category)
            {
                case TitleCategory.RewardTitles:
                    {
                        BuildRewardTitles(ref builder);
                        break;
                    }
                case TitleCategory.Champion:
                    {
                        BuildChampion(ref builder);
                        break;
                    }
            }
        }

        private void BuildRewardTitles(ref DynamicGumpBuilder builder)
        {
            if (!RewardTitleSystem.GetContext(_player, out var ctx))
            {
                return;
            }

            var titles = ctx.TitleList;
            const int perPage = 9;
            var start = _page * perPage;

            // "Hide title" row at the top -> APPLY button id 5000 selects -1.
            // Transcribe the row/description layout from ServUO (subtitle reward tab, lines ~840-900).
            var y = 70;
            for (var i = start; i < titles.Count && i < start + perPage; i++)
            {
                var td = titles[i];
                // Row: int -> builder.AddHtmlLocalized(td.Number); string -> builder.AddHtml(td.String).
                // Per-row APPLY button id = 6000 + i  (recover index as id - 6000 in OnResponse).
                y += 22;
            }

            // Pagination: Back (id 5100) when _page>0, Forward (id 5101) when more rows.
        }

        private void BuildChampion(ref DynamicGumpBuilder builder)
        {
            // Display the current champion title label + a toggle.
            var label = ChampionTitleSystem.GetChampionTitleLabel(_player);
            if (label > 0)
            {
                // builder.AddHtmlLocalized(... label ...)
            }
            // Toggle button id 5200 -> flip _player.DisplayChampionTitle (it shows/hides the champ title).
        }
```

- [ ] **Step 4: `OnResponse` switch dispatcher**

```csharp
        public override void OnResponse(NetState sender, in RelayInfo info)
        {
            if (sender.Mobile is not PlayerMobile pm || pm != _player)
            {
                return;
            }

            var id = info.ButtonID;

            switch (id)
            {
                case 0: // close
                    return;

                case 10001: _type = TitleType.PaperdollPrefix; _category = TitleCategory.None; _page = 0; break;
                case 10002: _type = TitleType.PaperdollSuffix; _category = TitleCategory.None; _page = 0; break;
                case 10003: _type = TitleType.OverheadName;    _category = TitleCategory.None; _page = 0; break;
                case 10004: _type = TitleType.SubTitles;       _category = TitleCategory.None; _page = 0; break;

                case 10101: _category = TitleCategory.Champion; _page = 0; break;
                case 10105: _category = TitleCategory.RewardTitles; _page = 0; break;

                case 5000: // hide reward title
                    RewardTitleSystem.Select(_player, -1);
                    break;

                case 5100: _page = _page > 0 ? _page - 1 : 0; break; // back
                case 5101: _page++; break;                            // forward

                case 5200: // champion display toggle
                    _player.DisplayChampionTitle = !_player.DisplayChampionTitle;
                    _player.InvalidateProperties();
                    break;

                default:
                    {
                        if (id >= 6000) // reward-title row APPLY
                        {
                            RewardTitleSystem.Select(_player, id - 6000);
                            _player.InvalidateProperties();
                        }
                        break;
                    }
            }

            DisplayTo(_player); // re-send (Singleton replaces the open one)
        }
```

- [ ] **Step 5: `TitlesMenuEntry` (the open hook)** — add to the same file:

```csharp
    public class TitlesMenuEntry : ContextMenuEntry
    {
        private readonly PlayerMobile _from;

        public TitlesMenuEntry(PlayerMobile from) : base(1115022, -1) // Open Titles Menu
        {
            _from = from;
        }

        public override void OnClick(Mobile from, IEntity target)
        {
            TitlesGump.DisplayTo(_from);
        }
    }
```

- [ ] **Step 6: Build & transcribe**

Open ServUO `Scripts/Gumps/TitlesMenu.cs` and transcribe the panel coordinates (lines 85-196) into `BuildLayout`, the TYPES button coords into `BuildTypeButtons`, the category button coords into `BuildCategoryButtons`, and the row/description/pagination coords into `BuildRewardTitles`/`BuildChampion` — using the Edge `DynamicGumpBuilder` API (see `Projects/UOContent/Gumps/Guilds/New Guild System/BaseGuildListGump.cs` for paging buttons and `RunebookGump.cs` for `AddPage`). The gump must always render the panels (never empty — `DisplayTo` + `Singleton` satisfy rule 13).

Run: `dotnet build Projects/UOContent`
Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```bash
git add Projects/UOContent/Gumps/TitlesMenu.cs
git commit -m "feat(rewardtitles): port TitlesGump (RewardTitles + Champion wired, rest deferred)"
```

---

## Task 6: PlayerMobile wiring (render hook + context entry)

**Files:**
- Modify: `Projects/UOContent/Mobiles/PlayerMobile.cs` (`GetProperties` ~line 3450, `GetContextMenuEntries` ~line 1929)

- [ ] **Step 1: Render the selected reward title**

In `GetProperties(IPropertyList list)`, immediately AFTER the existing champion-title block (the `if (DisplayChampionTitle) { ... }` near the `// TODO: Add the Titles Menu` comment), add:

```csharp
            var rewardTitle = Server.Engines.RewardTitles.RewardTitleSystem.GetSelectedTitle(this);
            if (rewardTitle != null)
            {
                if (rewardTitle.Number > 0)
                {
                    list.Add(rewardTitle.Number);
                }
                else if (rewardTitle.String != null)
                {
                    list.Add(1070722, rewardTitle.String); // ~1_val~
                }
            }
```

(Or add `using Server.Engines.RewardTitles;` at the top of PlayerMobile.cs and drop the namespace prefix.)

- [ ] **Step 2: Add the context-menu entry**

In `GetContextMenuEntries(Mobile from, ref PooledRefList<ContextMenuEntry> list)`, in the same `if (from == this)` / `Alive` region where the champion toggle is added (`list.Add(new CallbackEntry(6210, ToggleChampionTitleDisplay));`), add:

```csharp
                    list.Add(new Server.Gumps.TitlesMenuEntry(this));
```

(`TitlesMenuEntry` is the public class from Task 5; no need to reuse the private `CallbackEntry`.)

- [ ] **Step 3: Build**

Run: `dotnet build Projects/UOContent`
Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Projects/UOContent/Mobiles/PlayerMobile.cs
git commit -m "feat(rewardtitles): render selected title + Titles Menu context entry"
```

---

## Task 7: Restore Willebrord's deed grant + status doc

**Files:**
- Modify: `Projects/UOContent/Engines/Astronomy/Willebrord.cs`
- Modify: `dev-docs/servuo-content-migration-status.md`

- [ ] **Step 1: Grant the deed on discovery**

In `Willebrord.OnDragDrop`, in the success branch where `m.AddToBackpack(new RecipeScroll(465));` is, add alongside it:

```csharp
                                m.AddToBackpack(new AstronomerTitleDeed());
```

Remove the `// AstronomerTitleDeed reward deferred ...` comment that precedes it.

- [ ] **Step 2: Tick the gap in the status doc**

In `dev-docs/servuo-content-migration-status.md`, change the RewardTitle backlog item to closed:

```markdown
- [x] **RewardTitle system** — *(Astronomy)* **Closed:** added a `RewardTitleSystem` (GenericPersistence) + `BaseRewardTitleDeed`/`AstronomerTitleDeed` + ported TitlesGump (RewardTitles + Champion wired, rest deferred); restored Willebrord's `AstronomerTitleDeed` grant.
```

- [ ] **Step 3: Build & commit**

Run: `dotnet build Projects/UOContent` → succeeded.

```bash
git add Projects/UOContent/Engines/Astronomy/Willebrord.cs dev-docs/servuo-content-migration-status.md
git commit -m "feat(astronomy): restore AstronomerTitleDeed reward; close RewardTitle gap"
```

---

## Task 8: Migration schemas + full build + audit + tests

**Files:** generated migration JSONs under `Projects/UOContent/Migrations/`

- [ ] **Step 1: Regenerate serialization schemas**

New `[SerializationGenerator]` types were added (`RewardTitleContext`, `BaseRewardTitleDeed`, `AstronomerTitleDeed`). Generate their committed schemas:

Run: `dotnet tool restore && dotnet tool run ModernUOSchemaGenerator -- ModernUO.slnx`
Then: `git status --short Projects/UOContent/Migrations/`
Expected: new files `Server.Engines.RewardTitles.RewardTitleContext.v0.json`, `Server.Items.BaseRewardTitleDeed.v0.json`, `Server.Items.AstronomerTitleDeed.v0.json` (and no unrelated churn).

- [ ] **Step 2: Full solution build**

Run: `dotnet build`
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Full test run + baseline comparison**

Run: `dotnet test Projects/UOContent.Tests --no-build --filter RewardTitleContextTests`
Expected: 3 passed.

Run: `dotnet test Projects/UOContent.Tests --no-build 2>&1 | tail -3`
Expected: same failure count as `main` (the pre-existing `tiledata.mul` environment failures) plus our 3 new passing tests; zero NEW failures attributable to this change.

- [ ] **Step 4: Convention audit**

Review every new file against CLAUDE.md: `[SerializationGenerator(0)]` (no `encoded` arg), braces everywhere, no `Console`, no hot-path LINQ, gump non-empty + `DisplayTo`+`Singleton`, `PropertyList` cliloc args not bare literals. Run the `modernuo-code-audit` skill over `Engines/RewardTitles/`, `Items/Deeds/BaseRewardTitleDeed.cs`, `Gumps/TitlesMenu.cs`.

- [ ] **Step 5: Commit schemas + any audit fixes**

```bash
git add Projects/UOContent/Migrations/Server.Engines.RewardTitles.RewardTitleContext.v0.json \
        Projects/UOContent/Migrations/Server.Items.BaseRewardTitleDeed.v0.json \
        Projects/UOContent/Migrations/Server.Items.AstronomerTitleDeed.v0.json
git commit -m "chore(rewardtitles): generate serialization migration schemas"
```

---

## Task 9: Manual in-game smoke test + PR

- [ ] **Step 1: In-game verification (owner)**

Boot server + client. `[add AstronomerTitleDeed` → double-click: receive the title ("Thou hath been bestowed…"), deed deletes; re-`[add` + double-click a second granted deed of the same title → "You already have that title!". Open the paperdoll context menu → "Open Titles Menu" → TitlesGump → select the Astronomer title → confirm it shows on your paperdoll tooltip; select "hide" → it disappears. Toggle the Champion tab (if you have a champion title). Then via Astronomy: discover a constellation, drop the chart on Willebrord → receive both the RecipeScroll AND the AstronomerTitleDeed. `[Save]` + restart → the earned title + selection persist.

- [ ] **Step 2: Push + open PR (pin the repo — Edge is a fork)**

```bash
git push -u origin feat/reward-titles
gh pr create --repo modernuo/ModernUO-Edge --base main \
  --title "feat: RewardTitle system + TitlesGump (closes AstronomerTitleDeed gap)" \
  --body "Implements docs/superpowers/specs/2026-06-11-reward-titles-design.md. RewardTitleSystem (GenericPersistence) + BaseRewardTitleDeed/AstronomerTitleDeed + ported TitlesGump (RewardTitles functional, Champion as display toggle, FameKarma/Skills/Guild/Veteran deferred until backed). Restores Willebrord's title grant. Verified: build green, RewardTitleContext round-trip/dedupe/select tests pass, manual in-game grant→select→display→persist confirmed."
```

---

## Self-review notes (addressed)

- **Spec coverage:** §3.1 RewardTitleSystem → T3; §3.2 RewardTitleContext → T2; §3.3 BaseRewardTitleDeed → T4; §3.4 AstronomerTitleDeed → T4; §3.5 TitlesGump+TitlesMenuEntry → T5; §3.6 PlayerMobile edits → T6; §3.7 Willebrord+status → T7; §6 testing → T2 (xUnit) + T9 (manual); §5 persistence → T3. Migration schemas (learned convention) → T8.
- **Naming collision resolved:** the `[SerializableField]`-generated `Titles`/`Selected` collide with manual views → manual views renamed `TitleList`/`SelectedTitle` (T2 Step 3-4); all later references (T3 `GetSelectedTitle`, T5 gump) use `TitleList`/`SelectedTitle`/`SelectedTitle`. Verify generated names against the `.g.cs` before relying on them.
- **Serialization fallback** for `List<TextDefinition>` documented (T2 Step 5) in case the generator can't handle the list — but `Write(TextDefinition)`/`ReadTextDefinition()` exist, so the generated path is expected to work.
- **Type-on-abstract-base caveat** flagged (T4 Step 3) in case the generator dislikes `[SerializationGenerator]` on `abstract`.
- **Gump transcription**: the TitlesGump layout coordinates are ported from a cited ServUO file/line range (T5 Step 6) — the data-driven category builders + the full `OnResponse` switch are given complete; the static panel coordinates are transcribed from source (a faithful port, not invented).
