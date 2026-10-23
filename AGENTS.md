# AGENTS.md — read this first (any AI, any tool)

Project: **Rickshaw Chaos: Dhaka** — Unity 6.3 LTS (6000.3.11f1), URP, new Input System, Android first.
Game code is in `Assets/Scripts` (namespace `VehicleSystem`). Rendering toolkit is in `Assets/AnimeToon`
(namespace `AnimeToon`) with its glue in `Assets/RickshawChaos_Integration`.

## Before you write anything

1. Read `docs/ARCHITECTURE.md` (rules) — 5 minutes.
2. Open `docs/MODULES.md` and find **your module**. You were given a brief in `docs/briefs/`.
3. Check `docs/CONTRACTS.md` for anything you want to call outside your module.

## Hard rules

1. **Edit only the files your brief lists.** Everything else is read-only for you.
2. **Never rename, move or delete a public class, field, method, event or enum value** listed in `docs/CONTRACTS.md`
   (Unity scenes and prefabs reference them by name). If you need a change, write a change request
   (`docs/CHANGE_REQUEST_TEMPLATE.md`) and stop; do not "just fix it".
3. **Dependencies go one way** (see the layer table in `docs/ARCHITECTURE.md`). Gameplay never references UI.
   Do not add a reference "upward". If an `.asmdef` refuses to compile your reference, that is the rule working.
4. **Do not search the scene** (`FindFirstObjectByType`, `GameObject.Find`) in new code. Receive references through
   `Bind(...)` / `SetX(...)` / inspector fields. (Existing fallbacks stay; do not add more.)
5. **Tuning numbers and rules go in ScriptableObjects**, not hard-coded. State lives on components.
6. **Never edit `Assets/AnimeToon/`.** That belongs to the rendering account. Ask via a change request.
7. **Before creating a file**, search the project for the class name. Duplicates break the build.
8. **Deliver only changed files**, same folder structure, include `.meta` files for new files, plus a short note:
   what changed, why, and how to test it. Add a line to `docs/CHANGELOG.md`.
9. **Be honest.** If you did not compile or play-test something, say so. List your assumptions.

## When you finish

Run the test checklist at the bottom of your brief and report each item as pass / fail / not tested.
