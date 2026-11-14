# Economy and Vessel Integration Audit

**Audit baseline:** `main` at `b138f899983501c847c89357e62ded647b5e8fda`  
**Current implementation branch:** `feature/seafood-legality-regional-markets-balance-20261114`  
**Scope:** Runtime dependencies and generated-scene wiring for vessel propulsion, fuel, equipment, fishing, cargo, market/vendor transactions, research permits, dockside UI, telemetry, and save/restore.

## Executive result

The economy is implemented as a connected vessel-owned system rather than isolated vendor menus. The generated Greyhaven vessel is the shared owner for cargo, wallet, fuel, equipment, fishing, research, market, vendor services, chart state, and expedition telemetry. Fishing is attached by `HarvestingMinigameController.Configure` (and defensively by its `Awake`) to the same vessel object; the Fish action is forwarded to that director. The dockside hub resolves the same vessel-owned economy components and exposes their actions.

The audit found one cross-system reliability defect: wallet and fuel events used one exception boundary around a multicast delegate. A failing observer could therefore prevent later observers from receiving an already-committed transaction or fuel-consumption event. The hardening change now isolates each subscriber and preserves delivery to later listeners. Regression tests cover both event streams and duplicate wallet transaction rejection.

## Verified integration paths

| Flow | Connection verified in source |
|---|---|
| Propulsion → fuel → equipment wear | `BoatPrototypeController.FixedUpdate` observes propulsion, consumes fuel, then applies the installed engine's power modifier. `VesselFuelController` applies the installed engine's fuel-efficiency modifier and condition wear. |
| Fishing → installed rod and seafood catalogue | `FishingSystemDirector` reads installed-rod cast/control modifiers, handles fish/shellfish/cephalopods, and supports bait-free Kelp seaweed harvesting. Landed lots are evaluated by `CatchLegalityPolicy` and retain source region/technique/weight/legal reason in cargo save data. |
| Cargo → regional market → wallet | Mara builds itemized offers from eligible legal cargo, applies the current destination's market-class factor, uses per-region/per-item stock, and commits cargo removal with the wallet payment callback. Contract-only seafood requires the earned harvest permit. |
| Inez → cargo → wallet | Purchases place cargo before payment and remove the newly placed instance if payment fails. Supply stock is decremented only after payment succeeds. |
| Tomas → vessel services | Hull/engine repair, fuel, hold-cell repair, and installed-equipment repair are routed through the vendor service controller and wallet. |
| Harbourmaster contracts → salvage → wallet | The dockside hub attaches/resolves the contract controller, exposes accept/complete actions, and the controller commits one salvage placement with its wallet reward and persists active/completed state. |
| Sounding discoveries → research → chart/harvest permits | Research requirements read confirmed Sounding Rig discoveries; evidence removal and stipend payment are committed together; successful submissions grant chart and harvest permits without marking a region visited or exposing hidden locations. |
| Wallet/fuel/catches → expedition telemetry | Telemetry subscribes to committed wallet transactions and actual fuel-consumption events, starts on propulsion, records catch legality and departure loadout, and closes after a dockside service session. Up to 50 samples are grouped by destination and loadout band. |
| Runtime state → save | Wallet ledger, market stock, fuel reserve, equipment condition, research submission state, chart permits, and active/completed expedition accounting use existing schema-v8 save fields or namespaced progress flags. |
| Generated vessel → dockside UI | The Greyhaven builder attaches the vessel controllers; `DocksideHubController` resolves the same vessel components and exposes sales, purchases, research, repairs, equipment installation/storage, inspection, and telemetry. |

## Hardening changes in this audit

- Wallet transaction listeners are invoked individually. An exception from one listener is reported as a warning and cannot prevent later listeners from receiving the committed transaction.
- Fuel-consumption listeners are invoked individually for the same reason; the amount already consumed is not rolled back or re-applied.
- Expedition telemetry tracks the wallet/fuel instances it is subscribed to and only changes subscriptions when those references change, avoiding delegate remove/add allocations every rendered frame.
- Edit Mode regression coverage was added for listener isolation and idempotent wallet transactions.
- `tools/validate_project.py` now checks the cross-system links above, including builder attachment, dockside access, propulsion/fishing modifiers, transactional market/vendor/research paths, telemetry subscriptions, and the new regression tests.

## Persistence and transaction observations

- Save schema remains version 8. Catch provenance fields are additive to cargo placements; market stock reuses existing vendor/item entries; harvest permits reuse unlockedRegionIds; completed balance samples use a namespaced progress flag.
- Save participants are discovered at runtime. `SaveRestoreTransaction` validates the candidate and a rollback snapshot before applying state, then restores the captured snapshot if a participant fails.
- Wallet transaction IDs are persisted and reject duplicate application.
- Cargo sale and research submission use cargo removal commit callbacks so a failed wallet operation does not spend the cargo.
- Equipment ownership is represented by the equipment loadout rather than duplicated as a second cargo placement.
- Legacy cargo placements without catch-region metadata are treated as legacy inventory. New illegal catches remain flagged through save/restore and cannot become ordinary-sale eligible by reloading.

## Remaining integration and production work

These are known scope boundaries, not claims that the systems are finished:

1. **Unity runtime validation is still required.** GitHub checks validate repository safeguards, project configuration, and source syntax; they do not compile the project in Unity or execute the Unity Test Framework.
2. **Generated scene regeneration is still required.** Open the project in Unity 6.3 and run the existing Greyhaven environment builder so the current scene contains the latest generated components and catalog.
3. **Edit Mode and Play Mode suites must be run in Unity.** Then play through a complete expedition, including equipment fitting, fishing, sales, resupply, repairs, research submission, save/reload, and return-to-dock telemetry.
4. **Salvage pricing remains provisional.** The seafood catalogue now has 155 authored entries with schedule parity checks. Three Greyhaven salvage values remain explicitly provisional; aberrant specimens remain outside ordinary commodity sales.
5. **Catch legality is implemented as fictional game rules.** Region range, tiered minimum weight, protected species, specific harvest permits, prohibited techniques and the Hollow Trench closure are evaluated at landing. Fine values and weight thresholds require Unity playtest validation; they are not real-world law.
6. **Regional market logic is implemented; physical access is partial.** Destination factors and per-region/per-item stock are wired into the dockside market service. Greyhaven is the only generated market scene today; non-Greyhaven dockside terminals and access triggers must be authored in streamed destination scenes. Region permits do not create travel routes.
7. **Balance instrumentation is implemented; balance results are not yet measured.** Up to 50 samples are retained and reported by destination/loadout. Real Unity starter/average/optimized playtests are still required before tuning.

## Validation status

- GitHub repository integrity workflow: expected to validate the new cross-system safeguards on this branch.
- Unity Editor compilation: not performed in this environment.
- Unity Edit Mode tests: not executed in this environment.
- Unity Play Mode tests: not executed in this environment.
- Greyhaven scene regeneration and Development Build: not performed in this environment.

Do not treat source-level checks as a substitute for the Unity Editor test pass.


## Seafood, legality and destination-market integration pass

The Greyhaven builder now generates all 155 catalog records from the authoritative schedule. A repository safeguard compares IDs, prices and market classes to prevent drift. Regional availability and habitat profiles feed the encounter director; shellfish and cephalopods are catchable through the existing fishing loop, while seaweed uses a separate bait-free Kelp harvesting technique.

Catch legality is evaluated when cargo is landed. The policy records region, technique, weight, legal state and reason. It applies authored fines for undersized, out-of-range, protected, unpermitted, closed-area and prohibited-technique catches. The market excludes flagged lots. The Harbourmaster's three research submissions grant separate harvest permits for Reef Stewardship, licensed sea cucumber and managed abalone harvest; the Outer Shoals permit also satisfies the Hollow Trench access rule. Harvest permit IDs are not rendered as chart regions.

Regional market factors are applied at the destination market, and runtime stock is keyed by regional market ID plus item ID. The dockside panel shows the destination factor, licensed contract intake and flagged-catch exclusion. The physical dockside service is available only while a destination dockside session is open. Greyhaven is the only generated dockside market scene at this stage.

Expedition telemetry now records up to 50 completed samples segmented by destination and departure loadout band. The dockside report shows average net, net per game-hour, fuel and legal-catch rate. The source does not automatically rebalance prices. The controlled test protocol and provisional target envelopes are in the Economy Balancing document; no Unity playtest results have been fabricated.

Repository validation checks catalog completeness/parity, legality policy, cargo provenance persistence, permit progression, market policy, seaweed technique and telemetry sampling. Static checks do not prove Unity compilation, Editor builder execution, Edit Mode/Play Mode success or runtime market access. Those remain mandatory.
