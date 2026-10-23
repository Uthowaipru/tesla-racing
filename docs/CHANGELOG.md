# CHANGELOG

Format: `date · module · what · (BREAKING if a public name/type/key changed)`. Newest first.

- 2026-10-23 · M5 · Added `ScoringRules` ScriptableObject + `MatchManager.rules` / `SetRules()` + editor tool `Tools > Vehicle > Create Scoring Rules From Match Manager` (not yet compiled; additive, no breaking change).
- 2026-10-23 · M0 · Stage 3 (folders moved) and Stage 4 (11 asmdefs: RC.Core, Vehicle, World, Bots, Input, Camera, Match, Services, UI, Game, Editor) done; project compiles clean.
- 2026-10-23 · M0/M1/M2/M3/M5 · Stage 2 decoupling (not yet compiled): added `Core/GameState` (`IsPaused`); `PauseMenu.IsPaused` now forwards to it; added `VehicleDriver.IsLocalPlayer`; **BREAKING:** removed `VehicleRecovery.cameraRig` (camera snap now wired by `LocalPlayerRig` through `Recovered`); `Pickup` and `MatchManager` no longer reference `LocalPlayerInput` / `LocalPlayerRig`.
- 2026-10-22 · M0 · Architecture docs v1 written (AGENTS, ARCHITECTURE, MODULES, CONTRACTS, DECOUPLING_PLAN, briefs). No code changed.
