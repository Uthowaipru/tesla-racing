# Brief — M4 Bots

**Role:** AI drivers. `BotInput` is an `IDriveInputSource`; `BotSettings` is difficulty.

## Owns (may edit)
`Bots/{BotInput, BotSettings}.cs`.

## May read / call
Core, Vehicle (`VehicleController`, `VehicleStat`), World (`RoadGenerator`, `TrafficManager`, `TrafficVehicle`) through their public members only.

## Must not
Reference Match, UI, Input, Camera. Give bots abilities the player does not have unless the difficulty explicitly says so. Cheat with hidden speed unless a setting says so and it is documented.

## Public surface you promise to keep
`BotInput.Configure(BotSettings, RoadGenerator, TrafficManager, int seed)`, `Read()`, `BotSettings`, `BotDifficulty { Easy, Normal, Hard, Custom }`.

## Possible next work (not decided)
`BotSettings` as ScriptableObject assets per difficulty; fairness / rubber-band options (host-chosen); more varied driving personalities; filling empty slots in rooms.

## Delivery
Only changed files, same folder structure, `.meta` for new files. Note: what changed, why, assumptions, what was not compiled/tested.
Add a `docs/CHANGELOG.md` line. Need something outside your files? Write a change request and stop (see `AGENTS.md`).

## Test checklist
1. A bot drives the road without getting stuck for 3 minutes on each difficulty. 2. Bots avoid or hit traffic as designed. 3. Bots recover from flips. 4. Same seed gives the same bot behaviour start.
5. A bot never collects `localPlayerOnly` pickups.
