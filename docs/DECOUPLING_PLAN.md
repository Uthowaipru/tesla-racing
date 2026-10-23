# DECOUPLING PLAN — from "one big loop" to enforced modules

*Version 2 · 2026-10-23. Stages 0 to 4 are done. Next: Stage 5 (cleanups) and Stage 6 (ScriptableObjects). Owner: M0 Integration conversation. Nothing here has been compiled by its author: do one stage at a time,
compile and test after each, and tag git before starting.*

## 0. What the audit found

- **Clean already:** `AnimeToon/` never references the game; the game never references `AnimeToon/` or the integration.
  The integration touches only vehicle types.
- **Tangled:** 11 game folders form one dependency loop. Looking at exactly which links close it, the loop comes from
  **four small couplings plus folder placement**, not from deep design problems:

| ID | Coupling | Where | Fix |
| --- | --- | --- | --- |
| 2A | Camera and Input read `PauseMenu.IsPaused` (a UI class) | `VehicleCameraRig`, `LocalPlayerInput`, `LocalPlayerRig` | `GameState.IsPaused` in Core |
| 2B | Vehicle holds a `VehicleCameraRig` field only to call `Snap()` | `VehicleRecovery` line ~115 / ~643 | Drop the field; wire `Recovered += cameraRig.Snap` in `LocalPlayerRig` |
| 2C | Pickup checks `driver.Source is LocalPlayerInput` | `Pickup` | `VehicleDriver.IsLocalPlayer` |
| 2D | Match looks up `LocalPlayerRig` (a wiring class above it) | `MatchManager.Start` | Wiring layer registers the placed vehicle |

Also by placement (no code change, only moving files in Unity): `DriveInput.cs` and `SeededRandom.cs` go to Core;
`Data/` goes into Vehicle (the two reference each other, so they are one assembly); `Pickups/` goes under World;
`Menu/` goes under UI; `LocalPlayerRig` and `VehicleSpawner` go to Game; `PlayerProfile` goes with Services.

Not needed (and left alone on purpose): `MatchManager` referencing `RoadGenerator` / `WrongWayTracker` / `TrafficVehicle` /
`VehicleBoost` / `VehicleRecovery`. Match sits *above* them in the layer table, so those references point down and are legal.
Turning them into interfaces is optional later work, only if Match must be tested without a road.

## 1. Stage 0 — Safety net (30 minutes, no code)

1. Commit everything. `git tag pre-decoupling`.
2. Get the list of hand-edited scripts: `git diff <commit that imported the AI zip> -- Assets/Scripts`.
   Heuristic candidates (mixed or Windows line endings): `Camera/VehicleCameraRig.cs`, `Editor/MobileControlsBuilder.cs`,
   `UI/setactiveobject.cs`, `AnimeToon/Runtime/AnimeGraphicsMenu.cs` (this one is inside the toolkit: tell the rendering account).
   Confirm with git; the heuristic can miss files.
3. Add `.gitattributes` with `* text=auto eol=lf` and normalise line endings in one commit by itself.
4. Record the baseline: open every scene; Console must be clean; note any existing warnings.

**Pass when:** baseline recorded, tag exists.

## 2. Stage 1 — Documents

Add `AGENTS.md` (repo root) and `docs/` (`ARCHITECTURE`, `MODULES`, `CONTRACTS`, `DECOUPLING_PLAN`, `HANDOFF_TEMPLATE`,
`CHANGE_REQUEST_TEMPLATE`, `CHANGELOG`, `briefs/`). Delete or archive the old draft versions so there is one truth.
The old `DECOUPLING_PLAN.md`, `MODULES.md`, `CONTRACTS.md`, `ARCHITECTURE.md` and `HANDOFF_TEMPLATE.md` are replaced by these.

## 3. Stage 2 — The four couplings (code, all small) — **DONE 2026-10-23, awaiting your compile and test** (see `stage2/STAGE2_NOTES.md`)

Do each as its own commit. In each, keep the old public member as a forwarder where a scene or other script might use it.

**2A — Pause flag.** New `Core/GameState.cs`:

```csharp
namespace VehicleSystem
{
    public static class GameState
    {
        public static bool IsPaused { get; private set; }
        public static void SetPaused(bool paused) => IsPaused = paused;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => IsPaused = false;
    }
}
```

`PauseMenu` calls `GameState.SetPaused(true/false)` wherever it sets its own flag, and `PauseMenu.IsPaused` becomes
`public static bool IsPaused => GameState.IsPaused;`. `VehicleCameraRig` and `LocalPlayerInput` (and `LocalPlayerRig`) read `GameState.IsPaused`.
*Test:* pause on PC and phone; steering and camera ignore input while paused; cursor frees; resume restores.

**2B — Camera snap.** Removed `VehicleRecovery.cameraRig`. `LocalPlayerRig.Attach` subscribes `recovery.Recovered += OnRecovered`; `OnRecovered` snaps the camera when `snapCameraOnRecover` is on; `Detach` unsubscribes. `Recovered` fires after every teleport (both manual and auto go through one `Teleport` method), so behaviour matches. Serialized values of the removed field are simply ignored by Unity. *Test:* flip the car, press R; camera snaps as before.

**2C — Local player flag.** Added `VehicleDriver.IsLocalPlayer`. `LocalPlayerRig.Attach` sets it true and `Detach` clears it; the spawner leaves bots false. `Pickup` checks it. *Test:* only the local car collects `localPlayerOnly` pickups.

**2D — Placed vehicle registration.** Changed from the original idea. `MatchManager.Start` no longer searches for `LocalPlayerRig`; for a hand-placed car it finds the car in `VehicleController.All` with `driver.IsLocalPlayer`. (Having the rig register the car would have clobbered the spawner's name and vehicle definition, because the spawner also calls `Attach`.) *Test:* a scene with a hand-placed car and no spawner still scores.

**Pass when:** Stage 0 baseline still holds; scoring and a full match from start to results work.

## 4. Stage 3 — Move files (inside the Unity Project window only) — **DONE 2026-10-23**

Resulting tree:

```
Assets/Scripts/
  Core/       DriveInput.cs  SeededRandom.cs  GameState.cs
  Vehicle/    (existing)  + Data/ (VehicleDefinition, VehicleCatalog, StatModifierDef, MapDefinition, VehicleEffectsDefaults)
  Input/      Camera/     Bots/     Race/     (unchanged)
  World/      Road/  Pickups/
  Services/   Monetization/*  PlayerProfile.cs
  UI/         (existing)  + Menu/ (VehicleSelectMenu, VehiclePreviewStage, VehiclePreviewDrag)
  Game/       LocalPlayerRig.cs  VehicleSpawner.cs
  Editor/     (unchanged)
```

Rules: drag in the Project window; never move on disk. After each batch: recompile, confirm exactly one copy of each class name, open a scene.
`UI/setactiveobject.cs` gets a namespace and a proper name in the same pass (`SetActiveObject.cs`), checking that no scene uses it first.
**Pass when:** Stage 0 baseline holds; no `.meta` changed except moved-file path churn (GUIDs identical).

## 5. Stage 4 — Assembly definitions — **DONE 2026-10-23 (compiled clean after all 11 were imported)**

Order (each followed by compile + scene check): `RC.Core` → `RC.Vehicle` → `RC.World` → `RC.Bots` → `RC.Input` → `RC.Camera` →
`RC.Match` → `RC.Services` → `RC.UI` → `RC.Game` → `RC.Editor` (Editor platform only, references all).
Reference columns come from the matrix in `MODULES.md`. Add Unity package references (Input System, UI, TextMeshPro, URP) only when a compile error asks for them.
Leave **Auto Referenced = ON** for now: `AnimeToon/` and the integration still compile in `Assembly-CSharp`, which can only see auto-referenced assemblies (the integration needs `RC.Vehicle` and `RC.Editor`). Turn it off later, once the integration has its own asmdef referencing `RC.Vehicle` and the toolkit (its editor part also references `RC.Editor`). Ready-made files and the step-by-step are in `stage4/STAGE4_NOTES.md` (verified with a layout simulation: no forbidden references). The toolkit's own asmdefs (`AnimeToon.Runtime`,
`AnimeToon.Editor`) belong to the rendering account.

If a reference is refused, that is the point: do not add it. Fix the code with an event or interface and note it in `CHANGELOG.md`.
Expected surprises (list, not promises): a UI class in a gameplay file, an `#if UNITY_EDITOR` block using `UnityEditor` in a runtime file (move to `Editor/`),
an `internal` member used across folders.

**Pass when:** zero compile errors; all scenes open; a full match plays; an Android build starts on a device.

## 6. Stage 5 — Small cleanups

- One shared UI helper class in `RC.UI` for `Place`, `Stretch`, `NewUI`, `MakeText` (duplicated in every UI script today).
- `LocalPlayerRig`: replace its ~18 `FindFirstObjectByType` fallbacks with inspector fields filled by the editor tool; keep the fallback only where the tool cannot reach.
- Remove the per-script `EventSystem` creation copies into one helper.

## 7. Stage 6 — ScriptableObjects for rules (feature work, one conversation each)

1. **DONE 2026-10-23 (awaiting your test; see `scoring-rules/SCORING_RULES_NOTES.md`)** `ScoringRules` (M5): the ~25 numbers now on `MatchManager` (`metersPerReward`, `pointsPerReward`, bump points, chain bonuses, ram points, overtake,
   head-on, destroyed penalty, `pointsPerCoin`, `pointsPerCash`, cooldowns). `MatchManager` gets a `rules` field; a default asset is created by an editor tool with the current values.
   Migration: keep the old public fields for one release, copy them into the default asset once, then deprecate.
2. `BotSettings` as an asset per difficulty (M4).
3. `LevelDefinition` (M3 + M5): seed, road switches, objectives, star thresholds.
4. `EconomyRules` (M7), `VehicleClass` for ranked (M1).

## 8. Stage 7 — Start the parallel work

Paste a brief from `docs/briefs/` into each new conversation with `AGENTS.md`. M0 stays open to handle change requests and merges.

## 9. Assumptions still to verify (honest list)

1. Adding an asmdef keeps scene and prefab script references (they use the script's `.meta` GUID). Test with one scene before the rest.
2. ~~`Recovered` fires in every path where the old code snapped the camera.~~ Checked in code: yes, all teleports go through one method. Still needs the play test.
3. No runtime file uses `UnityEditor` outside `#if UNITY_EDITOR`.
4. `MapDefinition` is used only by code that may reference Vehicle (it references nothing else today).
5. The `AnimeToon` runtime needs UI and possibly Input System package references; the rendering account decides its asmdef set.
6. `setactiveobject.cs` is referenced by no scene under that exact name (rename may break a reference).
7. The hand-edit list above is a guess until `git diff` confirms it.

## 10. Rollback

Each stage is its own commit or branch. If a stage fails its "pass when", `git reset` to the stage's tag. Never fix a half-moved project by unzipping over it.
