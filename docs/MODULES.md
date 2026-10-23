# MODULES — who owns what

*Version 2 · 2026-10-23. Folders were moved (Stage 3) and the 11 assembly definitions are in the project (Stage 4); the project compiled clean. Paths below are the real ones.*

## Dependency matrix (row may reference column)

| ↓ may reference → | Core | Vehicle | Input | Camera | World | Services | Bots | Match | UI | Game |
| --- | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: |
| **Core** | – | | | | | | | | | |
| **Vehicle** | ✔ | – | | | | | | | | |
| **Input** | ✔ | | – | | | | | | | |
| **Camera** | ✔ | ✔ | | – | | | | | | |
| **World** | ✔ | ✔ | | | – | | | | | |
| **Services** | ✔ | ✔ | | | | – | | | | |
| **Bots** | ✔ | ✔ | | | ✔ | | – | | | |
| **Match** | ✔ | ✔ | | | ✔ | | ✔ | – | | |
| **UI** | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ | | ✔ | – | |
| **Game** | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ | – |

Empty = forbidden. The rendering toolkit (`AnimeToon`) and `AnimeToon.Integration` sit outside this matrix (see M8).

---

## M0 — Core and Integration ("architect")

**Purpose:** contracts, assembly definitions, the wiring layer, docs. The only module allowed to touch shared files.
**Assemblies:** `RC.Core`, `RC.Game`.
**Files:** `Core/DriveInput.cs` (`DriveInput`, `IDriveInputSource`), `Core/SeededRandom.cs`, `Core/GameState.cs` (new),
`Game/LocalPlayerRig.cs`, all `*.asmdef`, `docs/*`, `AGENTS.md`.
**Handles:** change requests from every other module; cross-module bugs; moving files; merging branches.
**Brief:** `briefs/M0_core_integration.md`

## M1 — Vehicle

**Purpose:** how a car drives, takes damage, boosts, recovers, shows effects; vehicle data assets.
**Assembly:** `RC.Vehicle` · **Depends on:** Core.
**Files:** `Vehicle/{ImpactInfo, VehicleBoost, VehicleController, VehicleDriver, VehicleEffects, VehicleGripAssist, VehicleHealth, VehicleImpactSensor, VehicleRecovery, VehicleStats}.cs`,
`Vehicle/Data/{VehicleDefinition, VehicleCatalog, StatModifierDef, MapDefinition, VehicleEffectsDefaults}.cs`.
**Editor tools:** `Editor/{VehicleDamageSetup, VehicleStatsEditor, DefaultEffectsBuilder}.cs`.
**Public surface:** `VehicleController`, `VehicleHealth`, `VehicleBoost`, `VehicleRecovery`, `VehicleImpactSensor`,
`VehicleDriver`, `VehicleStats`, `ImpactInfo` + enums, `VehicleEffects` slots, `VehicleDefinition`. Details in `CONTRACTS.md` §B.
**Never:** reference Camera, Input, UI, Match, Road. Talk to them by events.
**Brief:** `briefs/M1_vehicle.md`

## M2 — Input and Camera

**Purpose:** turning touch / tilt / keyboard / gamepad into `DriveInput`; the chase / free / look-back camera.
**Assemblies:** `RC.Input` (Core), `RC.Camera` (Core, Vehicle).
**Files:** `Input/{InputModeManager, LocalPlayerInput, MobileControls, TouchHoldButton}.cs`, `Camera/VehicleCameraRig.cs`,
`Editor/MobileControlsBuilder.cs`.
**Public surface:** `LocalPlayerInput` (`IDriveInputSource`; `Current`, `LookBack`, `AutoAccelerate`), `InputModeManager.Instance.Mode`,
`MobileControls`, `VehicleCameraRig` (`SetTarget`, `Snap`, `SetLookBack`, `FreeCamera`, `SetFreeCamera`, static `SetUiNeedsCursor`).
**Never:** read `PauseMenu`. Read `GameState.IsPaused` (Core).
**Brief:** `briefs/M2_input_camera.md`

## M3 — World (road, traffic, pickups)

**Purpose:** the endless deterministic road, traffic, pickups, wrong-way detection, void kill zone.
**Assembly:** `RC.World` · **Depends on:** Core, Vehicle.
**Files (`World/`):** `Road/{RoadGenerator, RoadChunk, RoadPath, TrafficManager, TrafficVehicle, PickupPlacer, VoidKillZone, WrongWayTracker}.cs`,
`Pickups/Pickup.cs`. **Editor tools:** `Editor/{RoadSetupTools, PickupTools}.cs`.
**Public surface:** `RoadGenerator` (`Begin(seed)`, `SetTarget/AddTarget/RemoveTarget`, lane / width queries, `GetStartPose`, `IsOnRoad`, events),
`WrongWayTracker` events, `TrafficManager.ActiveVehicles`, `TrafficVehicle` (`Knocked`), `Pickup` (`Collected`).
**Never:** call `UnityEngine.Random`; reference Input, Camera, UI, Match, Bots.
**Brief:** `briefs/M3_world.md`

## M4 — Bots

**Purpose:** AI drivers: `BotInput` (an `IDriveInputSource`) and `BotSettings` (difficulty).
**Assembly:** `RC.Bots` · **Depends on:** Core, Vehicle, World.
**Files:** `Bots/{BotInput, BotSettings}.cs`.
**Public surface:** `BotInput.Configure(settings, road, traffic, seed)`, `Read()`; `BotSettings`, `BotDifficulty`.
**Brief:** `briefs/M4_bots.md`

## M5 — Match and rules

**Purpose:** timed matches, scoring, ranks, rival rules, results data, spawning cars from a `RaceConfig`.
**Assembly:** `RC.Match` (rules) · `VehicleSpawner` lives in `RC.Game` but is owned here.
**Depends on:** Core, Vehicle, World, Bots.
**Files:** `Race/{MatchManager, MatchPlayer, RaceConfig}.cs`, `Game/VehicleSpawner.cs`, `Editor/MatchTools.cs`.
**Public surface:** `MatchManager` (`State`, `TimeLeft`, `Players`, `LocalPlayer`, `Result`, `RegisterPlayer`, `UnregisterPlayer`,
`GetRanking`, `GetRank`, `Begin`, `Finish`, `AddPoints`, events `StateChanged`, `PointsAwarded`, `Finished`),
`MatchPlayer`, `MatchState`, `ScoreReason`, `MatchResult`, `RaceConfig` / `PlayerSlot` / `SlotController`.
**Never:** reference UI or `LocalPlayerRig`.
**Brief:** `briefs/M5_match.md`

## M6 — UI and menus

**Purpose:** everything the player sees and taps: HUDs, pause, results flow, vehicle select.
**Assembly:** `RC.UI` · **Depends on:** Core, Vehicle, Input, Camera, World, Match, Services.
**Files:** `UI/{ControlsOverlay, HealthBar, MatchHud, MatchResultsFlow, PauseMenu, PickupHud, ResetCounterUI, RespawnCountdown, setactiveobject, Speedometer, VehicleNameTag, WrongWayBanner}.cs`,
`UI/Menu/{VehicleSelectMenu, VehiclePreviewStage, VehiclePreviewDrag}.cs`,
`Editor/{ControlsOverlayBuilder, SpeedometerBuilder, VehicleSelectMenuBuilder}.cs`.
**Public surface:** each HUD's `Bind` / `SetVehicle` / `SetTracker`; `PauseMenu` (sets `GameState.IsPaused`); `VehicleSelectMenu` event "vehicle confirmed".
**Never:** be referenced by any lower layer.
**Brief:** `briefs/M6_ui.md`

## M7 — Economy and services

**Purpose:** profile, wallet, ads, later economy / upgrades / ownership / Unity Gaming Services / purchases.
**Assembly:** `RC.Services` · **Depends on:** Core, Vehicle.
**Files (`Services/`):** `Monetization/{RewardedAds, DevRewardedAdProvider, LocalWallet}.cs`, `Player/PlayerProfile.cs`.
**Public surface:** `LocalWallet` (`Coins`, `Cash`, `Add`, `Changed`), `PlayerProfile` (`SelectedVehicleId`, `GetSelectedVehicle`, `Changed`),
`RewardedAds` / `IRewardedAdProvider`.
**Rule:** offline-first; hide the backend behind an `IProfileService`-style interface.
**Brief:** `briefs/M7_economy_services.md`

## M8 — Rendering (separate account)

**Purpose:** all anime-style rendering: shaders, materials, sky, quality tiers, graphics menu, effects.
**Folders:** `AnimeToon/` (universal, never references the game) and `RickshawChaos_Integration/` (the only glue).
**Rule for everyone else:** do not edit these folders. The integration needs a list of game members that must not be
renamed (`CONTRACTS.md` §G).
**Brief:** `briefs/M8_rendering.md`

## Future modules (not started)

Netcode (rooms, Relay), audio, offline levels (`LevelDefinition`, level select), economy / garage / upgrades, age screen.
Each gets a row in the matrix and its own brief when started. Netcode will sit beside Match and implement
`IDriveInputSource` for remote players; audio will subscribe to vehicle and match events.
