# HANDOFF TEMPLATE — how to start a conversation for one module

## What you paste into a new conversation (any AI)

1. The repo link or a zip of `Assets/Scripts` (+ `RickshawChaos_Integration` if rendering is involved).
2. `AGENTS.md` and `docs/ARCHITECTURE.md` (attach or paste).
3. The module's brief from `docs/briefs/` (paste it whole).
4. Your task, in one or two sentences.

## First message to send

> I'm working on one module of a Unity 6 mobile game (Rickshaw Chaos: Dhaka). Read AGENTS.md, ARCHITECTURE.md and the brief below.
> You own only the files listed in the brief. Task: <one or two sentences>.
> Start by listing which files you will change and which contracts you touch. Do not change anything else.

## What the AI must send back

- Only the changed files, same folder structure, with `.meta` files for any new file.
- A note: what changed · why · assumptions · what was **not** compiled or tested.
- One new line in `docs/CHANGELOG.md` (mark **BREAKING** if any public name changed; that needs an approved change request first).
- The brief's test checklist, each item pass / fail / not tested.

## When the AI needs something outside its module

Stop and write a change request (`CHANGE_REQUEST_TEMPLATE.md`). Give it to the M0 conversation. Do not edit the other module "just this once".

## Brief format (for new modules)

```
# Brief — M<n> <name>
Role: one sentence.
Owns (may edit): file list.
May read / call (public surface only): list with CONTRACTS.md section.
Must not: list.
Public surface you promise to keep: list.
Possible next work (not decided): list.
Delivery: reminders.
Test checklist: Play-mode steps.
```
