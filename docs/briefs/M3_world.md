# Brief — M3 World (road, traffic, pickups)

**Role:** the endless deterministic road, NPC traffic, pickups, wrong-way detection, the void kill zone.

## Owns (may edit)
`Road/{RoadGenerator, RoadChunk, RoadPath, TrafficManager, TrafficVehicle, PickupPlacer, VoidKillZone, WrongWayTracker}.cs`, `Pickups/Pickup.cs`,
`Editor/{RoadSetupTools, PickupTools}.cs` (`SeededRandom.cs` is Core: ask M0).

## May read / call
Core, Vehicle (`VehicleController`, `VehicleHealth`, `VehicleBoost`, `VehicleRecovery`, `VehicleDriver.IsLocalPlayer`).

## Must not
Use `UnityEngine.Random` (use `SeededRandom.For(seed, chunkIndex, channel)`). Reference Input, Camera, UI, Match, Bots. Change channel numbers or draw order without a change request
(it changes every seed's road). Add new `FindFirstObjectByType` fallbacks.

## Public surface you promise to keep
`CONTRACTS.md` §C. Two-way road stays an optional per-scene tick box (`twoWayRoad`).

## Possible next work (not decided)
`LevelDefinition` (fixed seed per level, road switches, objectives, stars) together with M5; more road pieces; crossover pieces; pickup variety.

## Delivery
Only changed files, same folder structure, `.meta` for new files. Note: what changed, why, assumptions, what was not compiled/tested.
Add a `docs/CHANGELOG.md` line. Need something outside your files? Write a change request and stop (see `AGENTS.md`).

## Test checklist
1. Same seed on two runs: same road, traffic and pickups. 2. Drive far: chunks spawn and are removed, no hitches. 3. Two-way on: oncoming lanes, median, crossover; wrong-way banner/score events fire.
4. Traffic knocked, then recycles. 5. Local car collects battery and repair kit; a bot does not (if `localPlayerOnly`). 6. Falling off the road triggers the void zone and respawn.
