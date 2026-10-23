# Brief — M6 UI and menus

**Role:** everything the player sees and taps: HUDs, pause, results flow, vehicle select, future screens.

## Owns (may edit)
`UI/{ControlsOverlay, HealthBar, MatchHud, MatchResultsFlow, PauseMenu, PickupHud, ResetCounterUI, RespawnCountdown, setactiveobject, Speedometer, VehicleNameTag, WrongWayBanner}.cs`,
`Menu/{VehicleSelectMenu, VehiclePreviewStage, VehiclePreviewDrag}.cs`, `Editor/{ControlsOverlayBuilder, SpeedometerBuilder, VehicleSelectMenuBuilder}.cs`.

## May read / call
Core, Vehicle, Input, Camera, World, Match, Services, through public members and events. UI may *read* gameplay and *call* its public methods.

## Must not
Be referenced by lower layers. Hold game rules (score math lives in Match). Add a new `Place/Stretch/NewUI/MakeText` copy. Talk to a vendor SDK directly (go through Services). Edit `AnimeToon/`.
Restyling in the anime look is a separate request with the rendering account.

## Patterns to keep
Assigned Hierarchy objects are used; otherwise a default UI is built at runtime; an editor tool generates the same objects ("Generate … UI" / "Add Missing …"); clicks connected in code (ARCHITECTURE R8).
`PauseMenu` sets `GameState.SetPaused(...)`. Screens that need the mouse call `VehicleCameraRig.SetUiNeedsCursor(true)` and must switch it off.

## Public surface you promise to keep
Each HUD's `Bind` / `SetVehicle` / `SetTracker`, `VehicleSelectMenu` confirm event, `PauseMenu` behaviour (`CONTRACTS.md` §E, §I).

## Possible next work (not decided)
One shared UI helper class; level select; garage and upgrade screens (after M7 exists); age screen at first launch; settings screen hosting the `AnimeGraphicsMenu` prefab.

## Delivery
Only changed files, same folder structure, `.meta` for new files. Note: what changed, why, assumptions, what was not compiled/tested.
Add a `docs/CHANGELOG.md` line. Need something outside your files? Write a change request and stop (see `AGENTS.md`).

## Test checklist
1. Each HUD shows and updates: speedometer, health, pickups, reset counter, respawn countdown, wrong-way banner, match HUD. 2. Pause opens and closes on PC and phone.
3. Vehicle select: browse, select, the choice is saved and the next race uses it. 4. Results flow: winner, leaderboard, rewards, "double rewards" test ad. 5. Layout on a tall phone and a tablet.
6. Mouse cursor is free on results (PC) and locked again in play.
