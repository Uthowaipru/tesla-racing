# M9 Audio — delivery note (2026-11-07, revision 2)

## What changed
New folder `Assets/Scripts/Audio` (nothing outside it edited, except one `CHANGELOG.md` line):
`RC.Audio.asmdef`, `AudioIds`, `AudioBank` (ScriptableObject), `AudioManager`, `AudioLoop`, `AudioBootstrap`, `VehicleAudio`, `MatchAudio`, `WorldAudio`, `AUDIO_IDS.md`;
`Audio/Editor`: `RC.Audio.Editor.asmdef`, `AudioBankTools` (menus: Create Audio Bank, Apply Audio Import Settings To Bank).
All `.meta` files included (new GUIDs). Public surface matches CONTRACTS section K, plus additions: `AudioManager.Bank`, `PlayMusic`, `StopMusic`, `HasSound`, `SetBank`, `AcquireLoop`, `ReleaseLoop`, `GainFor`.

## How to test
1. Copy the folder in, let Unity compile. 2. Tools > Vehicle > Create Audio Bank. 3. Drag clips into entries (`engine_player` needs a loopable clip). 4. Optional: Tools > Vehicle > Apply Audio Import Settings To Bank. 5. Press Play.

## NOT compiled in Unity / NOT play-tested
- Runtime scripts were compiled only with the Mono C# compiler against stub types written from CONTRACTS.md: syntax and signatures pass, nothing else.
- `Audio/Editor/AudioBankTools.cs` was not compiled at all (needs UnityEditor).
- No scene opened, nothing played, nothing profiled on the Narzo 30A.

## Assumptions (any of these can be a compile error or a silent sound; each is a one-line fix)
1. Revision 2: `Pickup.Collected` and `TrafficVehicle.Knocked` are per-instance events (confirmed from the source), so audio does not subscribe to them. Pickups are heard as `VehicleBoost.Batteries` / `VehicleRecovery.ManualResetsLeft` going up during a match (0.5 s grace after the match starts, in case match start resets them). Knocked traffic is heard by the car that hits it: `ImpactInfo.other` has a `TrafficVehicle` and `impactSpeed >= knockSpeed`. Both are approximations; if the sensor ignores soft hits, soft knocks are silent.
2. Instance events with the signatures in CONTRACTS: `VehicleImpactSensor.Impact(ImpactInfo)`, `VehicleHealth.Destroyed(ImpactInfo)` / `Respawned()`, `VehicleBoost.BoostStarted()` / `BoostEnded()`, `VehicleRecovery.Recovered()` (no arguments), `MatchManager.StateChanged / PointsAwarded / Finished`.
3. `VehicleDriver` is on the car (or a parent/child) and `IsLocalPlayer` is set by the time the car moves; it is read every frame, not cached.
4. `ImpactInfo.speedRatio` is 0..1; `ForwardSpeed` is in m/s (`engineReferenceSpeed` in the bank, default 30, sets the speed of highest pitch).
5. The scene has a camera tagged MainCamera with the AudioListener (used for distance culling). No AudioListener is added.
6. **Rule exception (two lookups):** `MatchManager` and `WrongWayTracker` have no static instance, so `MatchAudio` and `WorldAudio` each call `FindFirstObjectByType` once per scene load (never per frame; `WrongWayTracker` does the same for its own road). Needs M0 approval; see change requests 2 and 3.
7. The wrong-way tracker is a scene object; if it is created later, call `AudioManager.Instance.WorldSounds.SetWrongWayTracker(tracker)`.
8. `VehicleHealth.Damaged` is not used (it would double the impact sound). Pickup sound is one id for both pickup types.

## Test checklist (brief)
1. No errors; all scenes open — not tested. 2. Engine pitch rises with speed, idle at stop (`engineIdleVolume`, 0 = silent) — not tested.
3. Light vs hard impact — not tested. 4. Boost, pickup, traffic knocked, destroyed, respawn once each — not tested.
5. Countdown ticks, finish jingle, points chime — not tested. 6. Voice limit (16 one-shots, 8 loops, hard limits in code) and low-end phone profile — limit by construction, phone not tested.
7. Volumes and mute persist — not tested (PlayerPrefs written 0.5 s after the last change and on pause/quit).
8. Removing `RC.Audio` does not break another module — true by construction today (no module references it); UI and Game will once M0 adds the references.

## Change requests (for M0)
**1. From M9, 2026-11-07.** Add `RC.Audio` to the references of `RC.UI`, `RC.Game`, `RC.Editor` once it compiles. Why: UI buttons call `AudioManager.Instance.Play("ui_click")`; a volume panel needs the properties. Breaking: no. Workaround: none needed, audio works without it.
**2. From M9, 2026-11-07.** Add a way to reach the match without searching the scene: `public static MatchManager Instance` or a static `Enabled(MatchManager)` event on `MatchManager` (M5). Why: AGENTS rule 4 forbids the lookup MatchAudio uses today. Breaking: no (additive). Workaround: the one-time lookup per scene load.
**3. From M9, 2026-11-07.** Static hooks so audio needs no lookup or workaround: `WrongWayTracker.Instance`; `Pickup.AnyCollected(Pickup, VehicleController)` and `TrafficVehicle.AnyKnocked(TrafficVehicle)` static events raised next to the existing instance events. Breaking: no (additive). Workaround: the lookups and car-side detection above.
