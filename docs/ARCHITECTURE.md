# ARCHITECTURE — Rickshaw Chaos: Dhaka

*Version 2 · 2026-10-23. Written from an audit of the project's `Assets` folder. Stages 2 to 4 are done in the project (the four couplings fixed, folders moved, 11 asmdefs added, compiles clean). Treat every new change as a test in the Unity editor.*

## 1. Goal

Different conversations, accounts and AIs must each be able to work on **one module** without breaking the others.
Three things make that possible, in this order of importance:

1. **The compiler enforces the boundaries** (assembly definitions, "asmdefs"). A forbidden reference does not compile.
2. **Small written contracts** (`CONTRACTS.md`) for the few things modules share.
3. **One owner per module** and a written change request for anything shared.

Documents are the backup, not the protection. The asmdefs are the protection.

## 2. Layers

Lower layers know nothing about higher ones. A module may reference only modules in the "may reference" column.

| Layer | Assembly | Folder | May reference |
| --- | --- | --- | --- |
| 0 | `RC.Core` | `Scripts/Core` | nothing |
| 1 | `RC.Vehicle` (includes the vehicle data assets) | `Scripts/Vehicle` (+ `Vehicle/Data`) | Core |
| 2 | `RC.Input` | `Scripts/Input` | Core |
| 2 | `RC.Camera` | `Scripts/Camera` | Core, Vehicle |
| 2 | `RC.World` (road, traffic, pickups) | `Scripts/World` (`Road/`, `Pickups/`) | Core, Vehicle |
| 2 | `RC.Services` (profile, wallet, ads) | `Scripts/Services` | Core, Vehicle |
| 3 | `RC.Bots` | `Scripts/Bots` | Core, Vehicle, World |
| 4 | `RC.Match` (rules, scoring) | `Scripts/Race` | Core, Vehicle, World, Bots |
| 5 | `RC.UI` (HUDs, menus) | `Scripts/UI` (+ `UI/Menu`) | Core, Vehicle, Input, Camera, World, Match, Services |
| 6 | `RC.Game` (wiring: composition root) | `Scripts/Game` | everything above |
| — | `RC.Editor` (editor tools, editor-only) | `Scripts/Editor` | everything above |
| — | `AnimeToon.Runtime` / `AnimeToon.Editor` | `AnimeToon/` | Unity packages only. **Never the game.** |
| — | `AnimeToon.Integration` (+ editor) | `RickshawChaos_Integration/` | `RC.Vehicle`, AnimeToon |

Rules that follow from the table:

- **Gameplay never references UI.** UI reads gameplay; gameplay announces through events.
- **The game never references the integration or the toolkit.** The integration attaches itself at runtime
  (`AnimeDriftBootstrap`). Scenes and prefabs may hold integration components; that is a GUID link, not a code link.
- **`RC.Game` is the only place allowed to know everybody.** It connects things; it contains no rules.
- A reference that points *down* the table is fine. A reference that points *up or sideways between peers* needs
  an event or an interface in the lower module.

## 3. Rules

**R1. One direction.** See the table. If you need something from a higher layer, publish an event or interface and let
the higher layer subscribe.

**R2. Shared types live as low as possible.** `DriveInput`, `IDriveInputSource`, `SeededRandom` and the `GameState`
flags are in Core. Types that mention vehicles (`ImpactInfo`, `VehicleStat`, `HitZone`, `SpeedTier`) stay in Vehicle.

**R3. Intent, not control.** Input is a `DriveInput` struct from an `IDriveInputSource` (local player, bot, network
later). Vehicles never know who drives them. Per-player facts such as "is this the local player" are properties of
`VehicleDriver`, not type checks against `LocalPlayerInput`.

**R4. Composition root.** `LocalPlayerRig` (local player's input, camera, HUDs), `VehicleSpawner` (creates cars) and
`MatchManager` (rules and score) connect things. Everyone else receives references by `Bind` / `SetVehicle` / `Attach`
or inspector fields, not by searching the scene. Existing "if null, find it" fallbacks are tolerated; new ones are not.

**R5. Data vs state.** *ScriptableObjects hold data, catalogs and rules/config. Components hold runtime state.*
See section 4.

**R6. Events over references, interfaces over concrete classes** when talking across a layer. Use C# `event Action<…>`.
Avoid a global "event bus" and avoid ScriptableObject event channels unless a brief asks for them.

**R7. Determinism.** Everything on the road comes from `SeededRandom.For(seed, chunkIndex, channel)`. Same seed = same
road, traffic and pickups on every device. Do not call `UnityEngine.Random` in road, traffic or pickup code.

**R8. UI pattern.** Each UI script uses objects assigned from the Hierarchy if present, otherwise builds a default UI at
runtime; an editor tool ("Generate … UI" / "Add Missing …") builds the same objects in the scene; clicks are connected in
code. Shared UI helpers (`Place`, `Stretch`, `NewUI`, `MakeText`) are still duplicated and will be unified in `RC.UI` (Stage 5); do not add
another copy.

**R9. Public names are frozen** once listed in `CONTRACTS.md`. Adding is fine. Renaming, moving, changing a type or
removing needs a change request and a `CHANGELOG.md` entry marked **BREAKING**.

**R10. Per-scene switches live on components** (`RoadGenerator.twoWayRoad`, `VehicleHealth.headOnKills`,
`MatchManager.rivalKills`), not in global statics.

**R11. Static state is a contract.** The few statics the game has (`PauseMenu.IsPaused` → `GameState.IsPaused`,
`VehicleController.All` and its events, `LocalWallet`, `PlayerProfile`, `RewardedAds.Provider`, `AnimeQuality`) are listed
in `CONTRACTS.md`. New statics need a change request.

**R12. Mobile first.** Target is a low-end Android phone. No per-frame allocations in `Update`/`FixedUpdate`, no
`FindObject…` per frame, no LINQ in hot paths, particle counts go through the quality scale.

**R13. Namespaces.** Game code: `VehicleSystem` (editor tools `VehicleSystem.EditorTools`). Toolkit: `AnimeToon`,
`AnimeToon.Editor`, `AnimeToon.Integration`. Every file has a namespace; a stray global-namespace file is a bug.

**R14. One public MonoBehaviour per file, file name = class name** (Unity requirement for attaching the script).

## 4. ScriptableObjects in plain language

A **ScriptableObject (SO)** is a data file you can edit in the Inspector, saved in the project like a texture.
Many components can point at the same one, and a designer (or an AI) can change numbers without touching code.

Use an SO for:

- **Catalogs and definitions:** `VehicleDefinition`, `VehicleCatalog`, `MapDefinition`, later `LevelDefinition`.
- **Rules and tuning:** scoring numbers, bot difficulty, economy rates, upgrade tables.
- **Default asset bundles** the editor tools create (`VehicleEffectsDefaults`).

Do **not** use an SO for:

- **Runtime state** (current score, current health). That lives on components or plain C# objects. SO changes made in
  Play mode stay in the editor but not in a build, which causes "works in editor, broken on phone" bugs.
- **Replacing events.** Prefer plain events (R6).

Planned SOs (see `DECOUPLING_PLAN.md`, Stage 6): `ScoringRules` (from `MatchManager`'s ~25 numbers), `BotSettings` as an
asset, `LevelDefinition`, `EconomyRules`, `VehicleClass` (ranked Light / Heavy).

## 5. Unity gotchas that have bitten (or will)

- **Move and rename files only inside the Unity Project window** so `.meta` GUIDs survive. Never unzip a reorganised
  copy over the project; that creates duplicate classes (`The namespace already contains a definition for …`).
- After any move, search for each class name; exactly one copy must exist.
- An asmdef covers every script in its folder and below. Scene and prefab references use the script's `.meta` GUID, so they survived adding the 11 asmdefs. When adding a new asmdef, add the ones it references first, or you get a pile of "type not found" errors until all exist.
- A script in `Assembly-CSharp` (no asmdef) cannot be referenced by a script inside an asmdef. Every game script must end
  up in an asmdef; only vendor scripts (`TutorialInfo`) may stay outside.
- `Resources.Load` paths are folder-independent but name-dependent: do not rename the asset or move it out of `Resources/`.
- Line endings: the repo is LF. A file with CRLF is a sign someone edited it by hand on Windows. Keep `.gitattributes`
  `* text=auto eol=lf` so diffs stay readable.
- `internal` members only work inside one assembly. Today they are used by `MatchPlayer` and `MatchManager`
  (both in `RC.Match`), so they are fine.

## 6. Ownership and workflow

- One conversation owns one module (see `MODULES.md`). It edits only that module's files.
- Shared things (Core, `CONTRACTS.md`, `LocalPlayerRig`, assembly definitions) change only through a change request
  handled by the **M0 Integration** conversation.
- Use a git branch per module (`module/vehicle`, `module/ui`, …) and merge through M0. Tag before risky steps
  (`pre-asmdef`, `pre-scoring-so`).
- Deliver only changed files with `.meta` files, same folder structure, plus a note. Add a `CHANGELOG.md` line.
- Every brief ends with a Play-mode test checklist.

## 7. Definition of done (every change)

1. Console: zero errors after a full recompile; no new warnings from your files.
2. Every scene that existed before still opens with no `Missing (MonoBehaviour)` and no "Missing Script" in Hierarchy.
3. No `.meta` file was changed except for files you added.
4. Your brief's test checklist was run; the result is reported honestly.
5. `CONTRACTS.md` still matches the code, or the change is a flagged request.

## 8. Honesty rules

If you did not compile it, say so. If you assumed something, list it. If a test was skipped, say which. A short honest
"not tested" is worth more than a confident guess.
