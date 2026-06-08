# ServUO → ModernUO-Edge Content Migration — Status

Living tracker for the feature-at-a-time ServUO content import. One row per feature.
Design/roadmap: `docs/superpowers/specs/2026-06-08-servuo-content-migration-design.md`.

Status legend: ✅ Ported · 🔄 In progress · ⏳ Planned · ⛔ Deferred/blocked

| Feature | Era | Status | PR | Dependencies | Notes |
|---|---|---|---|---|---|
| **Astronomy** (telescope constellation minigame) | TOL (~Pub 86; gated `Core.TOL`) | ✅ Ported (pilot) | _pending_ | Cartography (present), Tinkering (present), `SextantParts` (present) | First migration PR. See divergences/deferrals below. |

## Astronomy — details

Ported files (`Projects/UOContent/Engines/Astronomy/`): `AstronomySystem` (GenericPersistence singleton, 1000 constellations), `ConstellationInfo`, `PersonalTelescope` (+ telescope gump), `StarChart` (+ naming gump, Cartography craft), `ConstellationLedger` (+ paged gump), `BrassOrrery`, `PrimerOnBritannianAstronomy`, `AstronomyTent`, `Willebrord` (NPC), `AstronomyGeneration` (`[GenAstronomy]`/`[DelAstronomy]`). Craft edits: `DefCartography` (StarChart), `DefTinkering` (PersonalTelescope = recipe 465). Tests: serialization round-trips for `ConstellationInfo` and the `AstronomySystem` persistence blob.

**Era gate:** single source — `AstronomySystem.Enabled = Core.TOL`. `Configure()` early-returns when disabled (no persistence registered); `[GenAstronomy]` checks it. Disabled state is NPE-safe (static helpers operate on empty lists).

### Accepted divergences from ServUO (intentional)
- **StarChart craft uses skill-based success** (0.0–60.0 Cartography) instead of ServUO's `SetForceSuccess(index, 75)` — that API does not exist in ModernUO Edge.
- **PersonalTelescope Tinkering recipe omits the `WorkableGlass` resource** (ServUO requires 1×). `WorkableGlass` is an unported SA glassblowing resource. Recipe currently needs `IronIngot ×25 + SextantParts ×1`. Re-add `WorkableGlass` when SA glassblowing is ported. Marked with a comment in `DefTinkering.cs`.
- **`Willebrord.SayTo` hue argument dropped** — ServUO passed a text hue (`1163`); Edge `SayTo` has no hue overload. Message cliloc unchanged.
- **StarChart not consumed on submission** — `OnDragDrop` returns `false` (chart retained), matching ServUO. Re-submission is harmless (constellation flagged discovered on first success → no second reward).
- **Ledger pagination off-by-one fixed** — ServUO's row loop showed 21 rows/page; corrected to 20 to match the page-count math (a bug fix, slight display divergence from source).

### Deferred (tracked follow-ups)
- ⛔ **`AstronomerTitleDeed` reward** — ServUO grants it alongside `RecipeScroll(465)` on discovery. Edge has **no reward-title system** (`BaseRewardTitleDeed` absent). Pilot grants only the recipe scroll. Add the deed once a RewardTitle foundation lands.
- ⛔ **`WorkableGlass` + SA glassblowing** — see divergence above.

### Verification status
- ✅ `dotnet build` (full solution): 0 warnings, 0 errors.
- ✅ xUnit: +2 new Astronomy round-trip tests pass; zero regressions vs `main` (the ~258 environment failures are the pre-existing missing-`tiledata.mul` data-file issue, identical on `main`).
- ⏳ **Manual in-game smoke test: PENDING (owner action).** Boot server + client, `[GenAstronomy]` (twice → second reports 0 placed), then exercise: Primer reading gump, Willebrord info gump, BrassOrrery toggle, telescope gump (lock down in a house; RA/DEC dials; "View Coordinate" at night), craft a StarChart from a BlankMap, chart + name a constellation, drop on Willebrord (receive RecipeScroll), open the Ledger (paging). Then `[Save]` + restart + reload → confirm telescope RA/DEC/DisplayName, orrery Active, StarChart constellation/name, and ledger discoveries all persisted. (Cannot be run in the dev/CI environment — no UO `.mul` data files.)
