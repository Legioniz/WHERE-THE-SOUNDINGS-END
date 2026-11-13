# Economy and Vessel Integration Audit

**Audit baseline:** `main` at `b138f899983501c847c89357e62ded647b5e8fda`  
**Hardening branch:** `feature/economy-integration-hardening-20261114`  
**Scope:** Runtime dependencies and generated-scene wiring for vessel propulsion, fuel, equipment, fishing, cargo, market/vendor transactions, research permits, dockside UI, telemetry, and save/restore.

## Executive result

The economy is implemented as a connected vessel-owned system rather than isolated vendor menus. The generated Greyhaven vessel is the shared owner for cargo, wallet, fuel, equipment, fishing, research, market, vendor services, chart state, and expedition telemetry. Fishing is attached by `HarvestingMinigameController.Configure` (and defensively by its `Awake`) to the same vessel object; the Fish action is forwarded to that director. The dockside hub resolves the same vessel-owned economy components and exposes their actions.

The audit found one cross-system reliability defect: wallet and fuel events used one exception boundary around a multicast delegate. A failing observer could therefore prevent later observers from receiving an already-committed transaction or fuel-consumption event. The hardening change now isolates each subscriber and preserves delivery to later listeners. Regression tests cover both event streams and duplicate wallet transaction rejection.

## Verified integration paths

| Flow | Connection verified in source |
|---|---|
| Propulsion → fuel → equipment wear | `BoatPrototypeController.FixedUpdate` observes propulsion, consumes fuel, then applies the installed engine's power modifier. `VesselFuelController` applies the installed engine's fuel-efficiency modifier and condition wear. |
| Fishing → installed rod | `FishingSystemDirector` reads installed-rod cast range and control modifiers and wears the installed rod after a landed catch. |
| Cargo → Mara → wallet | Mara builds itemized offers from eligible cargo, calculates demand/quality/freshness pricing, and commits cargo removal with the wallet payment callback. |
| Inez → cargo → wallet | Purchases place cargo before payment and remove the newly placed instance if payment fails. Supply stock is decremented only after payment succeeds. |
| Tomas → vessel services | Hull/engine repair, fuel, hold-cell repair, and installed-equipment repair are routed through the vendor service controller and wallet. |
| Harbourmaster contracts → salvage → wallet | The dockside hub attaches/resolves the contract controller, exposes accept/complete actions, and the controller commits one salvage placement with its wallet reward and persists active/completed state. |
| Sounding discoveries → research → chart | Research requirements read confirmed Sounding Rig discoveries; evidence removal and stipend payment are committed together; successful submissions grant chart permits without marking a region visited. |
| Wallet/fuel → expedition telemetry | Telemetry subscribes to committed wallet transactions and actual fuel-consumption events, starts on propulsion, and closes after a dockside service session. |
| Runtime state → save | Wallet ledger, market stock, fuel reserve, equipment condition, research submission state, chart permits, and active/completed expedition accounting use existing schema-v8 save fields or namespaced progress flags. |
| Generated vessel → dockside UI | The Greyhaven builder attaches the vessel controllers; `DocksideHubController` resolves the same vessel components and exposes sales, purchases, research, repairs, equipment installation/storage, inspection, and telemetry. |

## Hardening changes in this audit

- Wallet transaction listeners are invoked individually. An exception from one listener is reported as a warning and cannot prevent later listeners from receiving the committed transaction.
- Fuel-consumption listeners are invoked individually for the same reason; the amount already consumed is not rolled back or re-applied.
- Edit Mode regression coverage was added for listener isolation and idempotent wallet transactions.
- `tools/validate_project.py` now checks the cross-system links above, including builder attachment, dockside access, propulsion/fishing modifiers, transactional market/vendor/research paths, telemetry subscriptions, and the new regression tests.

## Persistence and transaction observations

- Save schema remains version 8; this pass adds no new persisted field.
- Save participants are discovered at runtime. `SaveRestoreTransaction` validates the candidate and a rollback snapshot before applying state, then restores the captured snapshot if a participant fails.
- Wallet transaction IDs are persisted and reject duplicate application.
- Cargo sale and research submission use cargo removal commit callbacks so a failed wallet operation does not spend the cargo.
- Equipment ownership is represented by the equipment loadout rather than duplicated as a second cargo placement.

## Remaining integration and production work

These are known scope boundaries, not claims that the systems are finished:

1. **Unity runtime validation is still required.** GitHub checks validate repository safeguards, project configuration, and source syntax; they do not compile the project in Unity or execute the Unity Test Framework.
2. **Generated scene regeneration is still required.** Open the project in Unity 6.3 and run the existing Greyhaven environment builder so the current scene contains the latest generated components and catalog.
3. **Edit Mode and Play Mode suites must be run in Unity.** Then play through a complete expedition, including equipment fitting, fishing, sales, resupply, repairs, research submission, save/reload, and return-to-dock telemetry.
4. **Economy catalogue breadth remains limited.** Five starter fish currently have authored ordinary-sale prices. Three salvage prices remain explicitly provisional. Unlisted seafood and aberrant specimens are not ordinary-sale eligible.
5. **Catch legality is not yet a complete rules system.** Per-catch size limits, closed-area rules, protected species, and prohibited-gear consequences still need authored game rules and market/research integration.
6. **Destination-specific markets and route economics remain future work.** Current buyer/supplier behavior is Greyhaven-local; region permits are not travel-route unlocks.
7. **Balance is not yet measured.** Expedition telemetry is instrumentation. Starter, average, and optimized trip targets still require real Unity playtest data before tuning.

## Validation status

- GitHub repository integrity workflow: expected to validate the new cross-system safeguards on this branch.
- Unity Editor compilation: not performed in this environment.
- Unity Edit Mode tests: not executed in this environment.
- Unity Play Mode tests: not executed in this environment.
- Greyhaven scene regeneration and Development Build: not performed in this environment.

Do not treat source-level checks as a substitute for the Unity Editor test pass.
