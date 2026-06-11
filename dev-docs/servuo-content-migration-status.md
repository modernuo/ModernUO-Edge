# ServUO → ModernUO-Edge Content Migration — Status

Living tracker for the feature-at-a-time ServUO content import. One row per feature.
Design/roadmap: `docs/superpowers/specs/2026-06-08-servuo-content-migration-design.md`.

Status legend: ✅ Ported · 🔄 In progress · ⏳ Planned · ⛔ Deferred/blocked

| Feature | Era | Status | PR | Dependencies | Notes |
|---|---|---|---|---|---|
| **Astronomy** (telescope constellation minigame) | ~Pub 86; **opt-in** (`astronomy.enabled`, default off) | ✅ Ported (pilot) | PR #1 | Cartography (present), Tinkering (present), `SextantParts` (present) | First migration PR. See divergences/deferrals below. |

## Known gaps & future efforts (backlog)

Accepted divergences and deferrals across all ported systems, tracked here (not as GH issues) so the
follow-up work stays in one place. Each item names the system that first hit it; several are shared
engine/foundation gaps that later systems will also need. Check off when closed and link the PR.

- [ ] **`WorkableGlass` + SA glassblowing** — *(Astronomy)* `PersonalTelescope` Tinkering recipe omits the 1× `WorkableGlass` resource (currently `IronIngot ×25 + SextantParts ×1`) because the SA glassblowing resource/system isn't ported. **Close:** port glassblowing, then re-add the `AddRes(index, typeof(WorkableGlass), 1154170, 1, 1154171)` line in `DefTinkering.cs`.
- [ ] **RewardTitle system** — *(Astronomy)* Edge has no reward-title infrastructure (`BaseRewardTitleDeed` absent), so `Willebrord`'s discovery reward grants only `RecipeScroll(465)`, dropping ServUO's `AstronomerTitleDeed`. **Close:** port a RewardTitle foundation, then restore the title-deed grant in `Willebrord.OnDragDrop`.
- [ ] **Craft force-success mechanism** — *(Astronomy)* Edge's craft system has no `SetForceSuccess`; `StarChart` uses a skill-based `0.0–60.0` Cartography range instead of ServUO's flat 75%. **Close:** add a force-success option to the craft engine (or decide skill-based is the desired behavior).
- [x] **`SayTo` hue overload** — *(Astronomy)* ~~Edge `Mobile.SayTo` had no hued localized overload, so `Willebrord`'s "Star Charts only" line dropped ServUO's `1163` hue.~~ **Closed:** added the overload upstream (modernuo/ModernUO PR #2481), cherry-picked it here, and restored `SayTo(m, 1158529, 1163)` in `Willebrord`.

**Intentional, not gaps (no follow-up needed):** `StarChart` is not consumed on submission (returns `false`, matches ServUO; re-submission grants no second reward). Ledger pagination was *corrected* to 20 rows/page (ServUO's loop rendered 21) — a fix, not a divergence to revert.

## Astronomy — details

Ported files (`Projects/UOContent/Engines/Astronomy/`): `AstronomySystem` (GenericPersistence singleton, 1000 constellations), `ConstellationInfo`, `PersonalTelescope` (+ telescope gump), `StarChart` (+ naming gump, Cartography craft), `ConstellationLedger` (+ paged gump), `BrassOrrery`, `PrimerOnBritannianAstronomy`, `AstronomyTent`, `Willebrord` (NPC), `AstronomyGeneration` (`[GenAstronomy]`/`[DelAstronomy]`). Craft edits: `DefCartography` (StarChart), `DefTinkering` (PersonalTelescope = recipe 465). Tests: serialization round-trips for `ConstellationInfo` and the `AstronomySystem` persistence blob.

**Opt-in (like Factions):** disabled by default. `AstronomySystem.Configure()` reads `ServerConfiguration.GetSetting("astronomy.enabled", false)`; when off, no persistence/singleton is registered and the static helpers are NPE-safe (operate on empty lists). `AstronomySystem.Enable()`/`Disable()` toggle at runtime and persist the setting (Register/Unregister the `GenericPersistence`). `[GenAstronomy]` calls `Enable()` then places the world content (mirrors the Factions generator). No era gate — content uses ~Pub 86 clilocs, so a reasonably modern client is needed, but enablement is the admin's choice.

Its accepted divergences and deferred items are tracked in the **Known gaps & future efforts** backlog above (`WorkableGlass`/glassblowing, RewardTitle/`AstronomerTitleDeed`, craft force-success, `SayTo` hue).

### Verification status
- ✅ `dotnet build` (full solution): 0 warnings, 0 errors.
- ✅ xUnit: +2 new Astronomy round-trip tests pass; zero regressions vs `main` (the ~258 environment failures are the pre-existing missing-`tiledata.mul` data-file issue, identical on `main`).
- ⏳ **Manual in-game smoke test: PENDING (owner action).** Content is placed on **Trammel** (Moonglow ~4705,1127). Boot server + client, `[GenAstronomy]` (twice → second reports 0 placed), then exercise: Primer reading gump, Willebrord info gump, BrassOrrery toggle, telescope gump (lock down in a house; RA/DEC dials; "View Coordinate" at night), craft a StarChart from a BlankMap, chart + name a constellation, drop on Willebrord (receive RecipeScroll), open the Ledger (paging).
  - **Debug helper:** constellations only appear at UO night (the world clock is real-time accelerated, ~2h/cycle). Use the admin command `[AstronomyTime Midnight` to force night for testing (`[AstronomyTime clear` reverts; not saved, resets on restart). Then `[Save]` + restart + reload → confirm telescope RA/DEC/DisplayName, orrery Active, StarChart constellation/name, and ledger discoveries all persisted. (Cannot be run in the dev/CI environment — no UO `.mul` data files.)
