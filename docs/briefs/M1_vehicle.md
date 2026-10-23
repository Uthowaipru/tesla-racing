# Brief — M1 Vehicle

**Role:** how the car drives, takes damage, boosts, recovers and shows effects; vehicle data assets.

## Owns (may edit)
`Vehicle/{ImpactInfo, VehicleBoost, VehicleController, VehicleDriver, VehicleEffects, VehicleGripAssist, VehicleHealth, VehicleImpactSensor, VehicleRecovery, VehicleStats}.cs`,
`Data/{VehicleDefinition, VehicleCatalog, StatModifierDef, MapDefinition, VehicleEffectsDefaults}.cs`,
`Editor/{VehicleDamageSetup, VehicleStatsEditor, DefaultEffectsBuilder}.cs`.

## May read / call
`RC.Core` only (`DriveInput`, `IDriveInputSource`, `SeededRandom`, `GameState`).

## Must not
Reference Camera, Input, UI, Match, Road, Bots, Services. Rename anything in `CONTRACTS.md` §B or §G (the rendering integration depends on it).
Know who drives the car (use `IDriveInputSource`); check types of drivers.

## Public surface you promise to keep
`CONTRACTS.md` §B in full. The `VehicleEffects` slots and `VehicleEffectsDefaults` are the only place the rendering side attaches.

## Possible next work (not decided)
Vehicle classes for ranked (`VehicleClass`: Light / Heavy, all stats equal inside a class); dents / detachable parts; auto / CNG vehicles; upgrades as
`StatModifierDef` tables; a `_DamageAmount` style visual hook (needs a change request to the rendering account first).

## Delivery
Only changed files, same folder structure, `.meta` for new files. Note: what changed, why, assumptions, what was not compiled/tested.
Add a `docs/CHANGELOG.md` line. Need something outside your files? Write a change request and stop (see `AGENTS.md`).

## Test checklist
1. Drive: accelerate, steer, handbrake feel unchanged. 2. Hit traffic at low and high speed: damage and `Impact` events fire. 3. Health to zero: destroyed, respawn after ~3 s.
4. Boost with a battery: duration ~2.5 s, trail changes. 5. Flip the car, auto recovery and R repair kit. 6. Bots and local car both drive from `IDriveInputSource`.
7. Particles appear on the five effect slots (trail, sparks, dust, low-health smoke, explosion).
