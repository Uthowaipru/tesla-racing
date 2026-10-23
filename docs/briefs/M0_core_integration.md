# Brief — M0 Core and Integration ("architect")

**Role:** keep the boundaries true. Own Core, the wiring layer, asmdefs, docs, and every change request. Merge the other modules' work.

## Owns (may edit)
`Scripts/Core/*` (`DriveInput.cs`, `SeededRandom.cs`, `GameState.cs`), `Scripts/Game/*` (`LocalPlayerRig.cs`; `VehicleSpawner.cs` is owned by M5 but lives here),
all `*.asmdef`, `AGENTS.md`, `docs/*`, `.gitattributes`.

## May read / call
Everything (read-only), but never edit another module's files except in an agreed stage of `DECOUPLING_PLAN.md`.

## Must not
Put rules or numbers in the wiring layer; add `FindFirstObjectByType` fallbacks; approve a breaking change without listing every affected scene, prefab and saved key.

## Current work
Stages 0 → 4 of `DECOUPLING_PLAN.md` are done. Next: stage 5 cleanups, then the ScriptableObject extractions (stage 6) as separate requests.
Keep `CONTRACTS.md` equal to the code.

## Test checklist
1. Clean recompile, zero errors. 2. Every scene opens, no Missing scripts. 3. Pause on PC and phone; resume. 4. Start a match, drive, bump traffic, finish, see results.
5. Hand-placed car scene still scores. 6. Local player can collect a pickup; a bot cannot (if `localPlayerOnly`). 7. Android build starts.
