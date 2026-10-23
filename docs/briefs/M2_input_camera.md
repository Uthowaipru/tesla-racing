# Brief — M2 Input and Camera

**Role:** turn touch / tilt / keyboard / gamepad into `DriveInput`; run the chase / free / look-back camera.

## Owns (may edit)
`Input/{InputModeManager, LocalPlayerInput, MobileControls, TouchHoldButton}.cs`, `Camera/VehicleCameraRig.cs`, `Editor/MobileControlsBuilder.cs`.
Note: `VehicleCameraRig.cs` and `MobileControlsBuilder.cs` look hand-edited (Windows line endings); get the latest from the project, not an old zip.

## May read / call
Core (`DriveInput`, `IDriveInputSource`, `GameState.IsPaused`). Camera also reads Vehicle (`VehicleController`, `VehicleStat`).

## Must not
Reference `PauseMenu` or any UI class (use `GameState.IsPaused`). Reference Match, World or Bots. Change the `SetUiNeedsCursor` contract.

## Public surface you promise to keep
`CONTRACTS.md` §E. `LocalPlayerInput` stays an `IDriveInputSource`; `AutoAccelerate` behaviour stays (auto-acceleration is a game pillar).

## Possible next work (not decided)
Gamepad polish, tilt calibration, input remapping, camera feel for two-way road, replay input source.

## Delivery
Only changed files, same folder structure, `.meta` for new files. Note: what changed, why, assumptions, what was not compiled/tested.
Add a `docs/CHANGELOG.md` line. Need something outside your files? Write a change request and stop (see `AGENTS.md`).

## Test checklist
1. Touch (device or Device Simulator): steer, handbrake, look back (hold), free camera (tap). 2. Tilt steering. 3. Keyboard and gamepad. 4. Switching input mode mid-play updates the overlay.
5. Pause: controls ignored, cursor freed on PC; resume restores. 6. Results screen frees the mouse (`SetUiNeedsCursor`) and gives the lock back. 7. Camera snaps after recovery.
