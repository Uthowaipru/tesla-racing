# Brief — M7 Economy and services

**Role:** the player profile, wallet, ads, and later economy, upgrades, vehicle ownership, Unity Gaming Services, purchases.

## Owns (may edit)
`Monetization/{RewardedAds, DevRewardedAdProvider, LocalWallet}.cs`, `Player/PlayerProfile.cs` (target `Services/`).

## May read / call
Core, Vehicle data (`VehicleCatalog`, `VehicleDefinition`).

## Must not
Reference UI, Match, World or Input. Put a vendor SDK type in a public signature (hide it behind an interface). Break offline play: the game must work with no network and no account.
Rename PlayerPrefs keys (it wipes players' saves). Spend coins on vehicles or Cash on upgrades (coins = upgrades only; Cash = vehicles).

## Public surface you promise to keep
`CONTRACTS.md` §F. Economy rules (GDD v0.3): 10 points = 1 coin, 40 points = 1 Cash; rarity is looks only; rewarded ads only (no banners); neutral age screen on first launch, ad treatment by age band (fallback 13+).

## Possible next work (not decided)
`IProfileService` with a local implementation first (offline-first), then Unity Gaming Services free tier and Google Play Games sign-in; `EconomyRules` ScriptableObject; ownership and upgrades; real ad SDK behind `IRewardedAdProvider`; in-app purchases.

## Delivery
Only changed files, same folder structure, `.meta` for new files. Note: what changed, why, assumptions, what was not compiled/tested.
Add a `docs/CHANGELOG.md` line. Need something outside your files? Write a change request and stop (see `AGENTS.md`).

## Test checklist
1. Fresh install: wallet is 0, selected vehicle default works. 2. After a match the wallet grows by the right coins and Cash. 3. "Double your rewards" test ad doubles once and only after a completed ad.
4. Kill and restart the app: wallet and selected vehicle persist. 5. Airplane mode: everything still works. 6. `Changed` events fire so UI refreshes.
