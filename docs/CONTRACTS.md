# CONTRACTS — signatures that must not change without agreement

*Version 1 · 2026-10-22. Signatures were read from the code on that date; none were compiled by the author of this document.
Adding members is free. Renaming, removing, moving or retyping anything below is a **BREAKING** change: write a change request
(`CHANGE_REQUEST_TEMPLATE.md`) and add a `CHANGELOG.md` entry. Names matter because scenes and prefabs reference them.*

Legend: **[exists]** in the code today · **[planned]** approved by this plan, built in the stage shown.

---

## A. Core (`RC.Core`)

```csharp
[Serializable] public struct DriveInput { float throttle; float steer; bool handbrake;
    DriveInput(float throttle, float steer, bool handbrake); static DriveInput Neutral; }      // [exists]
public interface IDriveInputSource { DriveInput Read(); }                                      // [exists]
public struct SeededRandom { SeededRandom(ulong seed); static SeededRandom For(int seed, int index, int channel);
    ulong NextULong(); float NextFloat(); float Range(float,float); int Range(int,int); bool Chance(float); }  // [exists]
public static class GameState { static bool IsPaused { get; } ; static void SetPaused(bool); }   // [done, Stage 2A]
```

- `PauseMenu.IsPaused` stays as a read-only forwarder to `GameState.IsPaused` so nothing breaks.
- Rule: vehicles never know who drives them; the only link is `IDriveInputSource.Read()` once per frame.

## B. Vehicle (`RC.Vehicle`)

**Types and enums [exists]**

- `ImpactInfo` (struct): `impactSpeed`, `speedRatio`, `tier` (`SpeedTier`), `zone` (`HitZone`), `point`, `normal`, `other` (`Collider`),
  `otherBody`, `otherVehicle` (`VehicleController`).
- `HitZone { Front, Rear, Side }` · `ImpactVerdict { Normal, Immune, Kill }` · `SpeedTier { Cruising = 1, Fast = 2, FullThrottle = 3 }`.
- `VehicleStat`, `ModifierType`, `StatModifier`, `VehicleStats` (`Get(VehicleStat)`), `StatModifierDef`, `VehicleDefinition`, `VehicleCatalog` (`Get(string id)`).

**`VehicleController` [exists]**

- Fields: `List<WheelSetup> wheels` (`WheelSetup`: `collider`, `handbrake`, `drive`).
- Read: `ForwardSpeed` (m/s, + forward), `SpeedKmh`, `Stats`, `IsGrounded`, `HandbrakeAmount`, `ThrottleInput`.
- Write: `SetInput(float, float, bool)`, `SetInput(DriveInput)`, `RebuildWheels()`.
- Static: `All` (`IReadOnlyList<VehicleController>`), events `VehicleEnabled`, `VehicleDisabled` (`Action<VehicleController>`).

**`VehicleDriver` [exists; one member planned]**

- `inputEnabled`, `Source` (`IDriveInputSource`), `LastInput`, `SetSource(IDriveInputSource)`.
- **[done, Stage 2C]:** `bool IsLocalPlayer { get; set; }`, set true by `LocalPlayerRig.Attach`, false on `Detach`. Replaces
  `driver.Source is LocalPlayerInput` checks.

**`VehicleHealth` [exists]**

- Read: `Health`, `Health01`, `IsDestroyed`, `IsInvulnerable`, `RespawnTimeLeft`.
- Events: `HealthChanged(float, float)`, `Damaged(float, ImpactInfo)`, `Destroyed(ImpactInfo)`, `Respawned()`.
- Methods: `Kill()`, `ApplyDamage(float, ImpactInfo = default)`, `Heal(float)`, `SetHealthFromNetwork(float)`, `ResetHealth()`.
- Hook: `Func<ImpactInfo, ImpactVerdict> impactResolver` (Match sets it to decide kill / immune).

**`VehicleImpactSensor` [exists]:** event `Impact(ImpactInfo)`. **`VehicleBoost` [exists]:** `Batteries`, `IsBoosting`, `BoostTimeLeft`,
`BoostTimeTotal`, events `BatteriesChanged(int)`, `BoostStarted`, `BoostEnded`, `AddBattery(int)`, `ResetBatteries()`, `TryBoost()`.

**`VehicleRecovery` [exists]:** `ManualResetsLeft`, `CanManualReset`, `IsReversingOut`, `AutoRecoverProgress`, `ResetManualUses()`,
`AddManualUse(int)`, `RecoverManual()`, `IsInSafePlace()`, `TeleportToLastSafePoint()`, `RecoverAuto()`, events `Recovered`,
`ManualRecovered(bool)`, `ManualResetDenied`.
- **[done, Stage 2B]:** the public field `VehicleCameraRig cameraRig` was removed. `LocalPlayerRig` subscribes to `Recovered` and snaps the camera (respecting `snapCameraOnRecover`). `Recovered` fires after every teleport (manual and auto).

**`VehicleEffects` slots [exists]:** `ParticleSystem trail, hitSparks, hitDust, lowHealthSmoke, explosion`; anchors `trailAnchor, smokeAnchor,
explosionAnchor`; `useDefaultsForEmptySlots`. `VehicleEffectsDefaults` (ScriptableObject, loaded via `Resources`) and `DefaultEffectsBuilder`
(`AssetPath`, `EnsureDefaults()`) feed those slots. The integration relies on them (see §G).

## C. World (`RC.World`)

**`RoadGenerator` [exists]:** `Path`, `Seed`, `IsStarted`, `PlayerS`, `LeadS`, `RearS`, `ActiveChunks`, `LaneCount`, `ForwardLaneCount`,
`OncomingLaneCount`, `LaneWidth`, `MedianWidth`, `TotalWidth`; `Begin(int seed)`, `SetTarget(Transform)`, `AddTarget`, `RemoveTarget`,
`LaneOffset(int)`, `OncomingLaneOffset(int)`, `LateralOffset(Vector3, float, out float)`, `GetRoadPoseNear(Vector3)`, `GetStartPose(int slot)`,
`IsOnRoad(Vector3)`; events `ChunkSpawned`, `ChunkRemoved`, `Restarted`; switch `twoWayRoad`; `enum RoadSide { Forward, Oncoming }`.
**`WrongWayTracker` [exists]:** events `WrongWayChanged(bool)`, `CrossedToOncoming(int)`, `CrossedToForward(int)`.
**`TrafficManager` [exists]:** `ActiveVehicles`. **`TrafficVehicle` [exists]:** `enum State { Driving, Knocked }`, event `Knocked(TrafficVehicle)`.
**`Pickup` [exists]:** `enum PickupType` (includes `Battery`, `RepairKit`), event `Collected(Pickup, VehicleController)`, flag `localPlayerOnly`.
- Determinism contract: all content uses `SeededRandom.For(seed, chunkIndex, channel)`. Changing channel numbers or the order of draws changes every
  seed's road: **BREAKING** (it breaks fixed-seed offline levels).

## D. Match (`RC.Match`)

**`MatchManager` [exists]:** `State`, `TimeLeft`, `CountdownLeft`, `LocalPlayer`, `Result`, `Players`; `RegisterPlayer(VehicleController, string, bool, VehicleDefinition …)`,
`UnregisterPlayer(VehicleController)`, `GetRanking()`, `GetRank(MatchPlayer)`, `Begin()`, `Finish()`, `AddPoints(MatchPlayer, int, ScoreReason)`;
events `StateChanged(MatchState)`, `PointsAwarded(MatchPlayer, int, ScoreReason)`, `Finished(MatchResult)`.
**Enums [exists]:** `MatchState { Waiting, Countdown, Running, Finished }`, `ScoreReason { Distance, NpcBump, Chain, RivalRam, Overtake, Crossing, Destroyed, WrongWay }`,
`MatchManager.MatchLength`. **`MatchResult` [exists]:** `points, rank, playerCount, distance, wrongWayDistance, npcBumps, rivalRams, overtakes, timesDestroyed, maxChain, coins, cash`.
**`RaceConfig` / `PlayerSlot` / `SlotController` [exists].** **`VehicleSpawner` [exists]:** `LocalVehicle`, `SpawnBots()`, `SpawnLocalFromProfile()`, `SpawnAll(RaceConfig)`, `Spawn(PlayerSlot)`.
- **[done, Stage 2D]:** `MatchManager` no longer looks up `LocalPlayerRig`. For a hand-placed car it registers the car in `VehicleController.All` whose `VehicleDriver.IsLocalPlayer` is true (same timing as before). The spawner path is unchanged.
- **[planned] Stage 6:** the ~25 scoring and timing numbers move into a `ScoringRules` ScriptableObject. The points table in the GDD is the default.

## E. Input and Camera

**`LocalPlayerInput` [exists]:** implements `IDriveInputSource`; `Current`, `LookBack`, `AutoAccelerate`. **`InputModeManager` [exists]:** `Instance`, `Mode` (`InputMode`).
**`VehicleCameraRig` [exists]:** `SetTarget(VehicleController)`, `Snap()`, `SetLookBack(bool)`, `FreeCamera`, `SetFreeCamera(bool)`, `ToggleFreeCamera()`,
`SetLookInput(Vector2, bool)`, static `SetUiNeedsCursor(bool)` (UI screens that need the mouse call it and must switch it off again).
Both read **`GameState.IsPaused`** [done, Stage 2A] instead of `PauseMenu.IsPaused`.

## F. Services

**`LocalWallet` [exists]:** `Coins`, `Cash`, `Add(int coins, int cash)`, event `Changed`; stored in `PlayerPrefs` under `VehicleSystem.Wallet.Coins` and `VehicleSystem.Wallet.Cash`.
**`PlayerProfile` [exists]:** `SelectedVehicleId`, `GetSelectedVehicle(VehicleCatalog)`, event `Changed`.
**`RewardedAds` [exists]:** `Provider` (`IRewardedAdProvider`), `IsReady`, `Show(string placement, Action<bool> onFinished)`.
- Economy numbers (GDD): 10 points = 1 coin, 40 points = 1 Cash; coins = upgrades only; Cash = vehicles.
- Stored data must keep working offline. A future backend sits behind an interface; UI never talks to a vendor SDK directly.

## G. Rendering boundary (`AnimeToon`, `AnimeToon.Integration`)

The integration compiles only if these game members keep their names and types. **Rename none of them without updating the integration in the same change.**

- `VehicleController`: `wheels`, `ForwardSpeed`, `IsGrounded`, `HandbrakeAmount`, `Stats.Get(VehicleStat.MaxSpeed)`, static `All`, `VehicleEnabled`.
- `WheelSetup`: `collider`, `handbrake`, `drive`.
- `VehicleHealth`: `IsDestroyed`, `Destroyed(ImpactInfo)`, `Respawned`. `VehicleBoost.IsBoosting`. `VehicleImpactSensor.Impact(ImpactInfo)`; `ImpactInfo`: `point`, `speedRatio`, `tier`.
- `VehicleEffects` slots: `trail`, `hitSparks`, `hitDust`, `lowHealthSmoke`, `explosion`. `VehicleEffectsDefaults`; `DefaultEffectsBuilder.AssetPath` / `EnsureDefaults()`.
- The integration references no other game type today (audit of 2026-10-22: only `VehicleController`, `WheelSetup`, `VehicleStat`, `VehicleHealth`, `VehicleBoost`, `VehicleImpactSensor`, `ImpactInfo`, `VehicleEffects`, `VehicleEffectsDefaults`, `DefaultEffectsBuilder`). It does **not** use `LocalPlayerRig`, so it only needs `RC.Vehicle` (and `RC.Editor` for its installers). Keep it that way.

Game scripts call AnimeToon only through its public API: `AnimeQuality` (static: `Mode`, `Tier`, `SetMode`, `CycleMode`, `Changed`, `ParticleScale`, `SkidMarks`, `DriftSparks`),
`AnimeGraphicsMenu` (`Open`, `Close`, `Toggle`, `Show`, `Hide`, `ToggleVisible`), `AnimeStyleController` (`SetTimeOfDay`), `AnimeSpeedFx` (`SetSpeed01`, `SetBoosting`, `Impact`).
Game code never sets `_Anime_*` shader globals. The game scripts today set **no** shader properties or `MaterialPropertyBlock`s directly (verified by search on 2026-10-22), so the "visuals contract" is only these slots.
If a future feature needs a new shader property (for example `_DamageAmount`, `_BoostAmount`), it goes in this section, must be neutral at 0, and is added to the toolkit by the rendering account.

Details of the toolkit: `Assets/AnimeToon/README.md` and the rendering account's own handout.

## H. Paths and keys that must not move

- `Resources/VehicleSystem/…` (including `AnimeDriftEffectsDefaults.asset`) and `VehicleEffectsDefaults` load by `Resources` path.
- `Resources/AnimeToon/AnimeQualitySettings.asset`.
- PlayerPrefs keys: `VehicleSystem.Wallet.Coins`, `VehicleSystem.Wallet.Cash`; `AnimeToon.Quality.Mode`, `AnimeToon.Quality.AutoLevel`, `AnimeToon.Quality.Custom`; the selected-vehicle key in `PlayerProfile`.
  Renaming a key silently wipes players' saved data.

## I. Static state (the complete list)

`GameState.IsPaused` (planned) / `PauseMenu.IsPaused`, `VehicleController.All` + `VehicleEnabled` + `VehicleDisabled`, `LocalWallet.Changed`, `PlayerProfile.Changed`,
`VehicleDefinition.ValuesChanged`, `RewardedAds.Provider`, `AnimeQuality`. Each has `[RuntimeInitializeOnLoadMethod]`-style reset where the code already has one (e.g. `PauseMenu.ResetStatics`);
keep resets when moving them.
