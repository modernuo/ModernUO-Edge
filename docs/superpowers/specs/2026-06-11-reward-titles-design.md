# RewardTitle System + TitlesGump — Design

**Date:** 2026-06-11
**Status:** Approved (design)
**Closes:** the `AstronomerTitleDeed` / RewardTitle gap in `dev-docs/servuo-content-migration-status.md`

## 1. Goal & non-goals

**Goal:** Add a reusable **RewardTitle foundation** — earnable titles granted by deeds, stored per-player, selectable, and displayed on the paperdoll tooltip — and port ServUO's **TitlesGump** as the selection UI. Restore `AstronomerTitleDeed` end-to-end (discover a constellation → earn the "Astronomer" title → display it).

**Non-goals / deferred (see §7):** the FameKarma, Skills, Guild, and Veteran title categories; CityLoyalty city-titles; SkillMasteries title rows; *selectable* champion titles. These need PlayerMobile storage fields or unported systems; the gump carries them as conditional seams that stay hidden until their backing lands. No era gate; the system is inert until a player earns a title.

## 2. Architecture

Mirror Edge's existing **`ChampionTitleSystem`** (`Engines/CannedEvil/`) — a `GenericPersistence` singleton keyed by `PlayerMobile`, with a per-player `[SerializationGenerator]` context. This keeps the large, heavily-coupled `PlayerMobile` almost untouched (only a render hook + a context-menu entry), matching how Edge already moved champion titles out of PlayerMobile into a system.

Reused as-is: `TextDefinition` (cliloc-or-string value type, `Projects/Server/Text/TextDefinition.cs`), `GenericPersistence`, the deed convention (`Items/Deeds/HairRestylingDeed.cs`), the property-list render seam in `PlayerMobile.AddNameProperties` (which already has a `// TODO: Add the Titles Menu for later Eras` marker), and `ChampionTitleSystem`/`ChampionTitleContext` as the structural template.

## 3. Components

### 3.1 `RewardTitleSystem : GenericPersistence`
`Projects/UOContent/Engines/RewardTitles/RewardTitleSystem.cs`

- `private static readonly Dictionary<PlayerMobile, RewardTitleContext> _contexts = new();`
- `public static void Configure()` → constructs the singleton (`base("RewardTitles", <priority>)`).
- `public static RewardTitleContext GetOrCreate(PlayerMobile pm)` → `CollectionsMarshal.GetValueRefOrAddDefault` (mirror `ChampionTitleSystem.GetOrCreateChampionTitleContext`).
- `public static RewardTitleContext GetContext(PlayerMobile pm)` → returns existing or null (render hook uses this, never creates).
- `[OnEvent(typeof(PlayerDeletedEvent))]` cleanup (remove the player's context).
- `Serialize(IGenericWriter)` / `Deserialize(IGenericReader)` — iterate `_contexts`, write player + context; on load resolve the player and re-add (mirror `ChampionTitleSystem` exactly, incl. dropping contexts whose player no longer exists).
- Static convenience: `AddTitle(PlayerMobile, TextDefinition) → bool`, `GetSelectedTitle(PlayerMobile) → TextDefinition?`, `Select(PlayerMobile, int)`.

### 3.2 `RewardTitleContext`
`Projects/UOContent/Engines/RewardTitles/RewardTitleContext.cs` — `[SerializationGenerator(0)] [PropertyObject] public partial class`.

- `[SerializableField] List<TextDefinition> _titles` (earned titles).
- `[SerializableField] int _selected` (initialized to `-1` = hidden).
- `bool Add(TextDefinition title)` — dedupe via `TextDefinition` value-equality, add, return success (the `false` path drives the deed's "you already have that title" message).
- `void Select(int index)` — clamp/validate to `-1 .. _titles.Count - 1`.
- `bool Remove(TextDefinition title)` — adjust `_selected` if needed.
- `TextDefinition? Selected` → `_titles[_selected]` when `0 <= _selected < Count`, else null.

**Serialization note (plan must confirm):** verify the source generator serializes a `[SerializableField] List<TextDefinition>` (TextDefinition is a core type — it may be directly supported). If it is **not**, `RewardTitleContext` instead serializes the list manually (per entry: write `Number` then `String`, mirroring how ServUO tags int vs string). Resolve this in the implementation plan before writing the context.

### 3.3 `BaseRewardTitleDeed : Item`
`Projects/UOContent/Items/Deeds/BaseRewardTitleDeed.cs` — `[SerializationGenerator(0)]`, abstract.

- `public override int LabelNumber => 1155604;` // A Deed for a Reward Title
- `public abstract TextDefinition Title { get; }`
- `[Constructible]`-friendly ctor `base(5360)`.
- `OnDoubleClick(Mobile from)`:
  - `if (from is not PlayerMobile pm) return;`
  - not in pack → `1042001`.
  - `Title.IsEmpty` → no-op.
  - `RewardTitleSystem.AddTitle(pm, Title)` true → `pm.SendLocalizedMessage(1155605, Title.ToString())` + `Delete()`; false → `1073626` (already have it).
- `GetProperties` → `list.Add(1114057, Title.ToString())`.

### 3.4 `AstronomerTitleDeed : BaseRewardTitleDeed`
`Projects/UOContent/Engines/Astronomy/AstronomerTitleDeed.cs` — `public override TextDefinition Title => 1158523;` ("Astronomer").

### 3.5 `TitlesGump` + `TitlesMenuEntry`
`Projects/UOContent/Gumps/TitlesMenu.cs` — port of ServUO `Scripts/Gumps/TitlesMenu.cs`, adapted to Edge.

- `TitleType` (PaperdollPrefix/Suffix/OverheadName/SubTitles) + `TitleCategory` enums.
- Edge `DynamicGump` with a **private ctor + static `DisplayTo(PlayerMobile)`** (gump-system rule 13; never empty).
- Layout: the TYPES / CATEGORIES panels, SELECTIONS/DESCRIPTION pane, footer, pagination (`GumpButtonType.Page`), and the two-step apply/clear flow.
- **`OnResponse(NetState, in RelayInfo)` is a `switch (info.ButtonID)` dispatcher** with explicit ID ranges — NOT ServUO's `ButtonCallbacks` lambda-dictionary + `FirstOrDefault` (that violates Edge audit rules 1 & 8). Re-send to refresh (`Singleton => true`).
- **RewardTitles category (under SubTitles):** lists `RewardTitleContext` titles (int → its own cliloc; string → wrapped in `1070722`), plus a "hide title" (`-1`) row; APPLY → `RewardTitleSystem.Select(pm, index)`. (ServUO's subtitle mutual-exclusion clearing is a no-op here — the competing subtitle sources don't exist yet — so it's omitted with a TODO.)
- **Champion category (under PaperdollSuffix):** wired to the existing `ChampionTitleSystem` — shows the current champion title (`GetChampionTitleLabel`) and toggles `DisplayChampionTitle` (reuse `ToggleChampionTitleDisplay`). Display/toggle only; per-title selection deferred.
- **Deferred categories** (FameKarma/Skills/Guild/Veteran): keep ServUO's "only draw the tab when its data source is non-empty" guards so they don't appear; leave a clearly-marked `// Deferred: needs <system/field>` seam at each.
- **`TitlesMenuEntry`** (cliloc `1115022`, "Open Titles Menu") — a `ContextMenuEntry`/`CallbackEntry` opening `TitlesGump.DisplayTo(pm)`.

### 3.6 Small `PlayerMobile` edits
`Projects/UOContent/Mobiles/PlayerMobile.cs`
- **Render hook:** at the existing `// TODO: Add the Titles Menu` site in the name/property build, add the selected reward title — `var t = RewardTitleSystem.GetSelectedTitle(this); if (t is { } td) { if (td.Number > 0) list.Add(td.Number); else if (td.String != null) list.Add(1070722, td.String); }`.
- **Context-menu entry:** in `GetContextMenuEntries`, next to the champion-title toggle (≈ line 1931), add the `TitlesMenuEntry`.

### 3.7 Astronomy wiring
- `Engines/Astronomy/Willebrord.cs` `OnDragDrop` success branch — restore `m.AddToBackpack(new AstronomerTitleDeed());` alongside the recipe scroll; drop the deferral comment.
- `dev-docs/servuo-content-migration-status.md` — tick the RewardTitle / `AstronomerTitleDeed` gap.

## 4. Data flow

```
Willebrord (discovery) → AstronomerTitleDeed + RecipeScroll into pack
double-click deed → RewardTitleSystem.AddTitle(pm, 1158523) → context._titles += "Astronomer"
paperdoll context menu → "Open Titles Menu" → TitlesGump (RewardTitles tab)
select "Astronomer" → context._selected = i
PlayerMobile name/property build → render hook → tooltip shows the title
[Save] → RewardTitleSystem persists contexts → survives reload
```

## 5. Persistence

`RewardTitleSystem` is a `GenericPersistence` with its own `"RewardTitles"` save bin; registered in `Configure()`. Always active (no opt-in, no era gate) — inert until a player earns a title. Per-player contexts are dropped on `PlayerDeletedEvent` and on load if the player no longer exists.

## 6. Testing

- **xUnit** (`Projects/UOContent.Tests/Tests/Engines/RewardTitles/`): `RewardTitleContext` round-trip (add titles + select → serialize via `BufferWriter`/`BufferReader` → deserialize → fields equal, buffer fully consumed); `Add` returns `false` on duplicate; `Select` clamps to `-1..n-1`; `Selected` returns null when hidden/empty.
- **Manual in-game**: `[add AstronomerTitleDeed` (or via Willebrord) → double-click grants + deletes, re-grant says "already have it"; open Titles Menu → select/hide the title; confirm it shows on the paperdoll tooltip; `[Save]` + restart → title + selection persist; Champion toggle still works.

## 7. Deferred scope (tracked, conditional seams)

| Item | Why deferred | Lights up when |
|---|---|---|
| FameKarma category | needs a `FameKarmaTitle` override field + `GetFameKarmaEntries` | those are added to PlayerMobile |
| Skills category | needs per-skill title overload + 3 storage fields | added |
| Guild category | needs `DisplayGuildAbbr`/`DisplayGuildTitle` flags | added |
| Veteran category | Edge veteran rewards grant items, not titles | a veteran-title source exists |
| CityLoyalty city-titles | CityLoyalty system unported | CityLoyalty ported |
| SkillMasteries rows | SkillMasteries unported | Masteries ported |
| Selectable champion titles | Edge auto-shows the highest; needs a selected-type field | a follow-up adds selection |

## 8. File structure

| File | Responsibility |
|---|---|
| `Engines/RewardTitles/RewardTitleSystem.cs` | `GenericPersistence` singleton, per-player context registry, static helpers |
| `Engines/RewardTitles/RewardTitleContext.cs` | per-player earned titles list + selected index |
| `Items/Deeds/BaseRewardTitleDeed.cs` | abstract title-granting deed |
| `Engines/Astronomy/AstronomerTitleDeed.cs` | the "Astronomer" deed |
| `Gumps/TitlesMenu.cs` | `TitlesGump` (matrix UI) + `TitlesMenuEntry` |
| `Mobiles/PlayerMobile.cs` (modify) | render hook + context-menu entry |
| `Engines/Astronomy/Willebrord.cs` (modify) | grant the deed on discovery |
| `dev-docs/servuo-content-migration-status.md` (modify) | tick the gap |
| `Projects/UOContent.Tests/Tests/Engines/RewardTitles/RewardTitleContextTests.cs` | context round-trip + dedupe + select tests |

## 9. Open implementation questions (resolve in the plan)
- Does the serialization generator serialize a `[SerializableField] List<TextDefinition>`? If not, `RewardTitleContext` serializes the list manually (per-entry `Number`/`String`).
- Confirm the clilocs render correctly in a modern client: `1155604`, `1155605`, `1073626`, `1042001`, `1114057`, `1158523`, `1070722`, `1115022`, and the TitlesGump panel/label clilocs.
