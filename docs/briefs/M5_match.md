# Brief — M5 Match and rules

**Role:** timed matches, scoring, ranks, rival rules, results data, spawning cars from a `RaceConfig`.

## Owns (may edit)
`Race/{MatchManager, MatchPlayer, RaceConfig}.cs`, `VehicleSpawner.cs` (lives in `Game/`, so edits to its public surface also go through M0), `Editor/MatchTools.cs`.

## May read / call
Core, Vehicle, World (`RoadGenerator`, `WrongWayTracker`, `TrafficVehicle`), Bots. Subscribe to events; never reach into private state.

## Must not
Reference UI or `LocalPlayerRig`. Put new scoring numbers inline: new numbers go to the rules asset (see below). Change `MatchResult` fields without a change request (UI reads them).

## Public surface you promise to keep
`CONTRACTS.md` §D.

## Possible next work (not decided)
`ScoringRules` exists now (Stage 6a); next: hide the legacy inline numbers, one rules asset per mode/level, `SetRules` from `LevelDefinition`; `LevelDefinition` objectives and stars (with M3); Equal-tuning vs Classic modes; room rules for networking (host decides); rewards math (10 points = 1 coin, 40 points = 1 Cash).

Reference rules (GDD v0.3): every 100 m +10; side bump on traffic at speed +20 / +30 (once per NPC); chain bonus; ram a rival from behind +20, side +25; overtake +10; cross to oncoming +50; destroyed −50.

## Delivery
Only changed files, same folder structure, `.meta` for new files. Note: what changed, why, assumptions, what was not compiled/tested.
Add a `docs/CHANGELOG.md` line. Need something outside your files? Write a change request and stop (see `AGENTS.md`).

## Test checklist
1. Start a 3-minute match: countdown, running, finished. 2. Each score reason appears once in the right situation (distance, bump, chain, ram, overtake, crossing, destroyed, wrong way).
3. Slow bumps pay nothing; a bump pays once per NPC. 4. Head-on at speed destroys the weaker car per the rules. 5. Ranking matches points with bots. 6. Coins and cash computed at the end as in the GDD.
7. A car placed in the scene by hand still registers (through the wiring layer).
