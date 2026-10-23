# Brief — M8 Rendering (separate account)

**Role:** all anime-style rendering — shaders, materials, sky, quality tiers, graphics menu, effects.

## Owns (may edit)
`Assets/AnimeToon/**` and `Assets/RickshawChaos_Integration/**`. Nobody else edits these folders.

## May read / call
The game's public contracts in `CONTRACTS.md` §G only. Everything else in the game is read-only.

## Must not
Let `AnimeToon/` mention any game type (`VehicleSystem`, `PauseMenu`, `LocalPlayerRig`, `VehicleController` …). Let the game reference the integration. Add a shader global that is not neutral at 0.

## Rules that already exist (from the AnimeToon handout; the older handout text on the bridge is out of date)
- Game code calls AnimeToon only through its public API (`AnimeQuality`, `AnimeGraphicsMenu`, `AnimeStyleController`, `AnimeSpeedFx`, `AnimeLightShafts`).
- `AnimeQuality` owns `Application.targetFrameRate` and replaces the render pipeline asset at runtime; the game must not set a frame-rate cap elsewhere.
- `AnimeVehicleEffectsQuality` scales `VehicleEffects` particles; nobody scales them elsewhere.
- `AnimeGraphicsMenuHook.cs` must not exist.
- `AnimeGraphicsMenu.cs` shows signs of a hand edit (mixed line endings): confirm with git and fold it back in properly.
- Asmdefs for the toolkit (`AnimeToon.Runtime`, `AnimeToon.Editor`) and for the integration are this account's decision; the integration needs `RC.Vehicle` (and `RC.Editor` for its installers) and must not be referenced by the game.

## Open work (from the handout)
Measure Low-tier fps on the Narzo 30A; full Android build test; building outline gaps; light shafts through walls; wet road tuning; restyle the game's UI in the anime look (needs a change request with M6); hair/eye/face shaders (future).

## Delivery
Same rules as everyone: changed files only, honest "not compiled" notes, a `CHANGELOG.md` line. If the game must change to support rendering (a new slot, a new event), send a change request to M0.

## Test checklist
1. Clean compile. 2. Low and High quality tiers both play without errors. 3. No GRAPHICS button appears by itself; the menu opens only from the settings button. 4. Drift effects and particle scaling follow the tier.
5. Full Android build succeeds (close other programs first: the shader compiler ran out of memory once). 6. Record average fps on the Narzo 30A, Low tier.
