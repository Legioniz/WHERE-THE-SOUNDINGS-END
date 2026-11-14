# Economy and Trading System

> Project: WHERE THE SOUNDINGS END  
> Status: Full authored seafood catalog, catch legality, destination pricing/stock and expedition measurement implemented in source; remote dockside scene coverage and Unity playtest validation pending  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-14  
> Currency: USD ($), stored and transacted to cents

## 1. Design intent

The economy is the consequence layer of an expedition, not a detached shop simulator. The player prepares a vessel, spends supplies and fuel, chooses fishing or salvage grounds, accepts exposure and cargo risk, returns to a buyer, converts retained cargo into money, and decides what to repair or purchase before leaving again.

The target feeling is a small working harbour with limited means. Prices, services and trust should create decisions without turning every dock visit into repetitive bookkeeping. A safe short inshore trip must remain viable. A distant premium market must not guarantee profit after travel time, fuel, damage and cargo risk.

The design takes useful structural lessons from *DREDGE* (cargo/freshness/return timing; optional skill input that does not make the core fishing loop inaccessible) and *Sunless Sea* (route planning around fuel, supplies, cargo and port-specific trade). These are references for design analysis, not templates to copy. Developer interviews also show that fuel can become tedious if it is a mandatory repeated dock interaction; fuel in this game must therefore create voyage-planning decisions and clear consequences, not busywork.

## 2. Existing authoritative rules

Do not create a second price schedule. The following documents remain authoritative:

- [Economy Mathematics](../07_GAME_RULES_AND_BALANCING/ECONOMY-MATHEMATICS.md) — sale formula, modifier ranges, legal eligibility, buyer demand, trip costs, upgrade payback and anti-exploit rules.
- [Seafood Selling Price Schedule](../07_GAME_RULES_AND_BALANCING/SEAFOOD-SELLING-PRICE-SCHEDULE.md) — species IDs, base USD rates, sale units and destination market factors.
- [Inventory System](../06_GAMEPLAY_SYSTEMS/INVENTORY-SYSTEM.md) — spatial cargo ownership, item shapes, condition and placement.
- [Greyhaven NPC System](../12_AI_AND_SIMULATION/GREYHAVEN-NPC-SYSTEM.md) — stable resident IDs, relationship tiers, memory and schedule rules.
- [Save Data Structure](SAVE-DATA-STRUCTURE.md) — versioned campaign persistence and migration.

The runtime must use the existing item IDs and species schedule. Never apply a second rarity multiplier or a second regional factor.

## 3. Player-facing expedition loop

1. At Greyhaven, inspect the market board, available buyer demand, service prices, fuel range and vessel condition.
2. Choose supplies, bait, tackle and equipment. Purchases must fit the physical hold or a valid equipment slot.
3. Depart. Fuel is consumed by actual propulsion; bait and other consumables are consumed by the action that uses them.
4. Fish, dredge, survey or recover salvage. Each cargo instance retains identity, quantity/unit, quality, freshness and legal eligibility.
5. Return to a buyer. The player sees the offer line-by-line before confirming a sale.
6. Selling transfers the selected cargo out of the hold, pays the wallet once, and updates only that buyer's stock/demand state.
7. Use the repair shed to pay for actual hull, engine and hold damage. Refuel and restock only what is needed.
8. Save the complete wallet, market state, cargo, vessel condition and NPC trade consequences.

No hidden fee is charged. A service cost is shown before the transaction. If an action fails, neither money nor inventory may be partially changed.

## 4. Vendors and local specialisation

Greyhaven's current residents are retained; do not introduce duplicate shopkeeper NPCs.

| Vendor / resident | Role | Buys | Sells / services |
|---|---|---|---|
| Mara Venn — Fish Market | Primary seafood buyer | Configured ordinary seafood | No supply sales in the current Greyhaven runtime |
| Tomas Vale — Boat Repair Shed | Mechanic / shipwright | No unrestricted salvage buying yet | Paid hull, engine and damaged-hold repairs; fuel |
| Inez Sarmiento — Net Mender Shed | Gear and materials specialist | Configured recovered salvage materials | Starter equipment and four bait types; further tackle catalogue remains open |
| Harbourmaster — Harbour Office | Authority / research broker | No unrestricted commodity buying | Explicit salvage contracts, research submissions, permits and one-time evidence rewards |

Vendor acceptance is a hard gate. A vendor must not buy an item simply because it has a price. Quest-critical cargo, protected wildlife, closed-area catches, undersized catches and quarantined specimens cannot be sold through ordinary commodity transactions.

Vendor stock and buying capacity are finite but readable. A vendor's occupation determines preferences and available services; their schedule determines whether the service is currently staffed. Do not create a universal menu that lets the player buy every item from every resident.

## 5. Current Greyhaven runtime integration

The current generated Greyhaven vessel owns the wallet, Mara market, fuel controller and resident vendor services. These components reuse the existing spatial cargo hold, vessel condition, sea-state clock, NPC schedules and relationship scores rather than duplicating their state.

- **Mara:** accepts only authored ordinary seafood definitions, shows line-by-line weight/quality/freshness/demand offers, and buys through an idempotent atomic cargo/wallet transaction. Aberrant and unpriced catches are rejected.
- **Inez:** appraises and buys the three authored salvage materials through the same atomic transaction boundary. She sells starter rod, lamp, engine and worm/small-fish/shellfish/lure bait in bounded stock. Purchases require hold space and available funds; cargo placement is rolled back if payment fails. Bait is a stack in the hold and one unit is consumed only when a cast is deployed.
- **Tomas:** quotes and charges for actual missing hull/engine integrity, individual damaged hold cells and refuelling. Quotes apply bounded trust discounts and are shown before confirmation. Repairs cannot exceed 100%.
- **Fuel:** main tank consumption is based on actual propulsion load and throttle; idle, zero-throttle, paused and dockside time do not drain fuel. A small separately saved emergency reserve permits low-power return after the main tank is empty.
- **Markets:** Mara and Inez stock is per vendor/item, saved with current/target stock and last restock day. Demand is bounded by the existing policy; stock replenishes gradually once per elapsed in-game day.
- **Dockside:** existing dockside modal ownership is retained. Resident-specific actions expose seafood/salvage appraisals, sale confirmation, purchases, repairs and refuelling.

A new campaign receives a one-time $25.00 starter wallet balance. A zero-balance save with a transaction history is never topped up. The current salvage values are explicitly provisional and are defined in the seafood price schedule's supplemental salvage table.

**Remaining:** physical dockside market terminals and interaction triggers still need to be authored in each streamed destination scene; the current generated Greyhaven scene is the only verified market location in the project. Numerical starter/average/optimized tuning still requires real Unity playtest samples. Telemetry is a measurement system, not evidence that balance is already correct. Unity compilation, Edit Mode execution, Play Mode testing, scene regeneration and economy playtests remain required.

## 5.1 Harbourmaster delivery contracts

The Harbourmaster's dockside panel now exposes a compact three-contract salvage board. The player accepts one job, carries the requested salvage to Greyhaven, and submits it at the office. The initial jobs request Corroded Ship Fittings, Salt-Stiff Canvas, or Waterlogged Timber, with fixed prototype rewards of $8.50, $5.00, and $7.00 respectively. A completed contract is recorded once; its reward uses the existing idempotent wallet ledger, and the cargo placement is removed only when the wallet transaction commits. Active contract identity and the completion bitmask are stored in a namespaced existing world flag, so the save schema remains v8. These are introductory local deliveries. The separate discovery-gated research and chart-permit progression is documented in section 5.3.

## 5.2 Equipment progression and servicing

The vessel has three physical equipment slots: `engine`, `fishing-rod`, and `lantern`. Installing an item removes its placement from the spatial hold; replacing or storing it returns the previous item to the hold and fails cleanly if the hold cannot accept it. The existing schema-v8 `InventorySaveData.equipment` list stores slot ID, stable item ID and normalized condition. No duplicate cargo copy is created.

- **Outboard engine:** condition scales propulsion output and fuel-use efficiency. Engine wear follows actual throttle/load time.
- **Handline rod:** condition scales cast range and fishing timing/control. Rod wear is applied after successful landings.
- **Deck lantern:** creates a point light on the vessel; condition scales intensity and range. It wears during night operation.
- **Service:** Tomas quotes repairs from missing condition and commits the wallet charge before restoring the slot to full condition. Stored equipment retains its condition.

These are prototype modifiers and wear coefficients. They must be playtested against travel time, fuel cost and upgrade payback before final tuning.

## 5.3 Harbourmaster research and chart permits

The Harbourmaster's research ledger is a separate progression path from salvage delivery contracts. Each one-time submission requires a matching salvage placement and confirmed Sounding Rig site discoveries:

| Submission | Evidence threshold | Salvage | Stipend | Permit |
|---|---:|---|---:|---|
| The Inner Water Record | 1 site | Corroded Ship Fittings | $15.00 | The Shallows |
| Where the Brackish Current Turns | 3 sites | Salt-Stiff Canvas | $30.00 | Blackrock Reach |
| A Sound Beneath the Soundings | 5 sites | Waterlogged Timber | $50.00 | The Outer Shoals |

Submission consumes the selected salvage only when the idempotent wallet reward commits. The completion bitmask is saved in a namespaced progress world flag; permits are added to the existing unlocked-region collection and applied to the live chart. A permit does not mark the region visited, reveal its location, create a compass bearing, or imply inter-region travel. Stipends are prototype values pending economy playtests.

## 5.4 Expedition economy telemetry

A trip begins when the vessel applies propulsion. It closes after the player finishes a dockside service session, allowing return-market sales and service costs to be included. The telemetry records gross wallet receipts, operating expenses, actual fuel consumed (including emergency-reserve use), action-driven game-clock duration, net result and net income per hour. Equipment purchases are capital investment and are excluded from recurring operating expense; fuel, bait, repairs and service costs are included. Completed-trip aggregates and latest-trip metrics persist through a namespaced world flag without changing schema v8.

The telemetry measures live player behavior; it does not automatically change prices or certify balance. Starter, average and optimized routes still require controlled Unity playtests and documented tuning.

## 6. Sale value and market demand

For each eligible sale line, use the existing economy formula:

`LineRevenueUSD = roundToCent(quantity × basePrice × destinationFactor × quality × freshness × demand × liveCondition)`

- Base price and sale unit come from the seafood schedule / item definition.
- Destination factor is applied once at the buyer's actual market.
- Quality and freshness come from the retained cargo instance, not a global guess.
- Demand is buyer-and-item specific, bounded to 0.75–1.35, and visible before confirmation.
- Live-condition premium applies only to eligible intact live animals delivered to a buyer that accepts live stock.
- No separate rarity multiplier is added.
- Round each completed line to cents using midpoint-away-from-zero, then sum the transaction.

Demand is driven by visible buyer inventory and posted demand. A stable initial model is:

`DemandFactor = clamp(1 + ScarcityResponse × (TargetStock - CurrentStock) / max(TargetStock, MinimumStock), 0.75, 1.35)`

The response coefficient is a tuning parameter owned by the market profile, not a global hard-coded constant. Smooth updates over in-game time or discrete deliveries; one fish must not cause a dramatic price swing. Deliveries increase buyer stock and normally reduce demand. Restocking/consumption replenishes stock on the authoritative action-driven sea clock. A posted order can temporarily change the buyer's demand for named items, but must disclose quantity, deadline, buyer and payout.

Market factors are destination-based. Catch origin does not grant the destination premium. A player may inspect posted prices before travelling, but selling is only possible at a buyer physically present in the current destination.

## 7. NPC relationship and trade behavior

Use existing stable NPC IDs and `NpcAgent.RelationshipScore`, `CanTrade` and relationship memory events. Do not create a second trust score.

- Below the existing trade threshold, the resident refuses ordinary transactions. Explain why in dialogue/status text.
- At low trust but above the trade threshold, the resident may offer a less favourable quote or restrict specialist stock.
- Neutral trust uses the posted market offer.
- Familiar/trusted relationships can unlock small, bounded discounts, better service availability, preferred buyer demand or occasional purchase orders.
- Ally-tier benefits are convenience and access, not unlimited money generation.
- Apply one relationship modifier per transaction. Do not stack the same benefit through both vendor and global market multipliers.
- A transaction consequence is recorded with a unique event ID and applied through `NpcAgent.TryApplyRelationshipEvent` so reloads or duplicate callbacks cannot award trust repeatedly.
- Refusal, negotiation and trust thresholds must be disclosed through dialogue. Do not silently alter the price after confirmation.

Keep relationship effects asymmetric where character identity warrants it. Mara may value consistent, well-handled landings; Tomas may reward responsible maintenance and reliable payment; Inez may value returning reusable net materials. The Harbourmaster's evidence and contract decisions remain separate from commodity pricing.

## 8. Purchasing, stock and cargo

Purchases must validate, in order: vendor open, vendor accepts/sells item, relationship gate, available vendor stock, valid quantity, wallet balance, destination hold/equipment capacity, and final price. Only then commit money and inventory atomically.

- Bait is consumed when a cast/technique actually uses it, not merely by opening a menu.
- Engine, fishing rod and lantern use the authored prototype durability model: engine wear follows propulsion load, rod wear follows successful landings, and lantern wear follows in-game night operation. Condition and installed ownership persist; Tomas can repair each slot. Wear coefficients remain playtest tuning values.
- A purchase cannot be resold to the same vendor for a profit. Buy and sell quotes must be separate and disclosed.
- Equipment upgrades must identify the capability unlocked, additional operating cost, route/depth/method access and estimated payback trips.
- Cargo hold space remains a physical constraint. No purchased item may appear in the hold if it cannot fit.
- Vendor stock is persistent and refreshes only by the documented stock simulation; scene reloads and save reloads must not restock it.

## 9. Repairs and vessel services

Use existing `VesselConditionController` hull/engine integrity and `SpatialCargoHold` damaged-cell state. The economy is a payer and transaction coordinator; it does not own vessel physics or cargo geometry.

Repair quote components:
- Hull: missing hull integrity × authored cost per integrity point.
- Engine: missing engine condition × authored cost per integrity point.
- Hold: number/type of repairable blocked cells × authored material/labour cost.
- Installed equipment: missing slot condition × item-specific repair rate (engine $18.00, rod $6.50, lantern $5.00 for full restoration before Tomas's relationship discount).

Show a quote before confirmation. Repairs restore only the selected component and never exceed 100%. A failed or unaffordable repair changes nothing. Do not charge a fixed fee when no repair is required.

## 10. Fuel and operating costs

Fuel is a voyage resource and is stored on the vessel. Consumption must be tied to actual propulsion/engine load rather than wall-clock time while docked or paused. The player should be able to see current fuel, estimated remaining range and the estimated cost to refill before departure.

- Burn fuel only while the propulsion system is under load.
- Scale consumption from throttle/engine load and installed engine condition/efficiency using one tested formula; the displayed full-load endurance uses the same installed-engine multiplier.
- No fuel drain while paused, in dialogue, docked with engine off, or idling with zero propulsion.
- When the main tank is empty, a separately saved emergency reserve allows low-power return. After both main and emergency reserves are exhausted, the vessel can drift; a paid tow/rescue service is not yet implemented and remains a vertical-slice requirement.
- Refuelling is a priced service with an explicit quantity and total. Do not require a repetitive manual refuel click after every trip.
- Bait, ice and other consumables are charged only when purchased/consumed. Do not add a flat hidden expedition fee.

The exact consumption and service price coefficients remain tuning values and must be measured during the vertical slice. Do not copy *DREDGE* fuel rates or assume a particular real-world vessel's litres-per-kilometre figure.

## 11. Salvage, materials and discoveries

Salvage has three possible uses, resolved by item definition and contract:
1. Sell as commodity to an accepting buyer.
2. Retain for a repair/upgrade recipe when the crafting/upgrade system is implemented.
3. Submit as evidence or a contract objective, consuming it only when the player confirms the submission.

Do not let a material be sold and simultaneously count toward a repair recipe or quest. One cargo instance has one owner/state at a time. Unique research payouts are claimed once per stable specimen/evidence ID and persisted. A discovery's narrative value may exceed its commodity price; not every artifact should be sellable.

## 12. Transaction integrity and persistence

All transaction methods must be atomic: validate first, then mutate wallet, cargo, vendor stock, vessel condition or relationship state. If any step fails, roll back or do not commit. Use stable item/vendor/event IDs, invariant numeric validation, bounded collections and duplicate-transaction protection.

Persist:
- Wallet in integer cents (USD); retain/migrate the legacy integer currency field without losing old saves.
- Vendor stock and item-specific demand state.
- Market refresh timestamp / day marker.
- Purchase/sale/contract idempotency keys needed to prevent duplicate payouts.
- Fuel, hull and engine condition on the vessel record.
- Cargo instance quality, freshness, quantity and sale unit.
- Any relationship event memory applied by a transaction.

Use the existing save coordinator and ordered schema migrations. Never store mutable market stock in a ScriptableObject asset; assets define starting stock, base prices and tuning only. Runtime market state belongs in the save DTO.

## 13. UI and feedback

The dockside hub keeps exclusive modal ownership. Add service actions to existing locations instead of opening competing overlays:
- Fish Market: inventory selection, offer breakdown, buyer stock/demand, confirm sale.
- Repair Shed: hull/engine/hold status, itemized quote, confirm repair, fuel quote.
- Net Mender: tackle/bait catalogue, vendor stock, price, cargo/equipment destination.
- Harbour Office: contracts, permits, research submission and posted notices.

Every transaction displays item, quantity/unit, base price, destination factor, quality/freshness, demand, relationship modifier, total and post-transaction wallet. Purchases display stock and hold fit. Repairs display current/target condition and line-item cost. Errors must explain the failed condition. Keyboard and gamepad focus must be preserved.

## 14. Implementation sequence and acceptance gates

### Runtime foundation update — 2026-11-14

`EconomyPolicy` implements deterministic sale-line rounding, bounded quality/freshness/demand/live modifiers, stock-driven demand, repair/fuel quote arithmetic and overflow-safe wallet deltas. `SaveGameData` is schema v8 with a cents wallet and transaction-id ledger; v7 whole-dollar balances migrate to cents. Cargo instances now retain total sale quantity in their authored unit (stack count for ordinary stacked goods; landed kilograms for each fish), and fishing records the already-calculated landed weight rather than discarding it. Item definitions expose authored price/unit/ordinary-sale eligibility fields; existing assets still require reviewed schedule values before they can be sold.

`EconomyWalletController` now owns the live integer-cent wallet and bounded idempotency ledger on the generated vessel and participates in the existing save coordinator. That earlier wallet-only milestone has since been superseded by the connected vendor, fuel, repair, dockside, research, equipment, regional market and telemetry implementation described in the current runtime sections below. Do not treat the wallet and pure policies as a playable trading loop.



### Runtime implementation — connected Greyhaven economy

The generated player vessel now owns `MaraFishMarketController`, `VesselFuelController`, `GreyhavenVendorServices` and `EconomyWalletController`. These reuse the existing spatial hold, NPC schedules/relationships, vessel condition, boat propulsion and save coordinator.

- Mara builds itemized offers for the five authored starter fish and atomically pays/removes eligible catch. Aberrant, quest and unpriced cargo is rejected.
- Inez buys three explicitly provisional salvage materials and sells stocked bait/starter equipment. Purchase placement is validated against the spatial hold; failed payment rolls the placement back. The four bait types map to the fishing system's actual bait choices and one unit is consumed only when the cast deploys.
- Tomas sells refuelling, hull/engine repairs and damaged hold-cell repairs. Quotes use actual missing condition and bounded trust discounts; the wallet is charged before repairs are applied.
- Main fuel drains only from propulsion load and throttle. A separately saved emergency reserve enables a reduced-power return after the main tank empties. Paid refuelling restores that reserve.
- Mara and Inez stock is saved per vendor/item and replenishes gradually once per elapsed in-game day. Buyer stock and demand are shown in the itemized dockside offer.
- The existing dockside modal exposes the resident-specific sale, purchase, repair and refuelling actions. New campaigns start with $25.00; transaction history prevents repeat starter grants.

The editor builder must be run in Unity to regenerate Greyhaven with the new runtime components and bait/salvage definitions. The latest source now includes the expanded seafood catalog, per-catch legal provenance, game-authored restrictions and fines, licensed harvest permits, destination-specific market factors and stock ledgers, seaweed harvesting, and measured trip reports. Physical remote market terminals and destination scene interaction remain pending. Unity compilation, Edit Mode execution, Play Mode testing and economy playtests remain required.

### E0 — Data and persistence
- Add authored base sale values/units and vendor profiles without changing stable item IDs.
- Add a versioned economy save DTO and migration from the current schema.
- Add pure deterministic price, demand, repair, refuel and affordability policies.
- Test rounding, clamps, invalid values, demand stability and legacy save migration.

### E1 — Market transactions
- Implement wallet, buyer offers, sale selection, stock/demand updates and atomic cargo removal.
- Reject quest-critical, protected, illegal, unsupported and invalid cargo.
- Test duplicate sale, failed sale, full hold, insufficient money and reload exploit cases.

### E2 — Purchasing and services
- Implement vendor stock, purchases, bait/supply consumption, repair quotes and refuelling.
- Integrate actual fuel consumption into propulsion and the existing action-driven pause/time rules.
- Test no fuel burn while paused/docked/idle and no partial transaction on failure.

### E3 — NPC integration and dockside UI
- Bind Mara, Tomas, Inez and Harbourmaster to their own service profiles.
- Use existing relationship gates and idempotent memory events.
- Add market/repair/supply panels under existing modal ownership.
- Test schedules, trust tiers, refusals, discounts, controller focus and save/load.

### E4 — Balance and vertical slice
- Run starter, average and optimized trip simulations.
- Measure gross revenue, trip net, net income per hour, fuel/range, repair burden and upgrade payback.
- Tune only after the 45–90 minute Greyhaven slice is playable.
- Do not mark the economy complete until Unity tests and actual player tests demonstrate the loop.

## 15. Research references

- Black Salt Games developer interview, *Trawling in the deep: How Black Salt Games made spooky fishing RPG DREDGE*, Game Developer: https://www.gamedeveloper.com/design/trawling-in-the-deep-how-black-salt-games-made-spooky-fishing-rpg-i-dredge-i-
- Joel Mason, *Deep Dive: The surprising depth of spatial inventories in DREDGE*, Game Developer: https://www.gamedeveloper.com/design/deep-dive-the-surprising-depth-of-spatial-inventories-in-dredge
- Black Salt Games / Apple Developer, *Discover how DREDGE was reimagined for Apple platforms*: https://developer.apple.com/videos/play/meet-with-apple/247/
- *Sunless Sea* trade design notes, Failbetter Games via Steam News: https://store.steampowered.com/news/posts/?appids=596970&enddate=1511952977
- Unity, *Separate game data and logic with ScriptableObjects*: https://unity.com/how-to/separate-game-data-logic-scriptable-objects
- Unity Manual, *ScriptableObject*: https://docs.unity3d.com/6000.1/Documentation/Manual/class-ScriptableObject.html
- Jonathon Doran and Ian Parberry, *Emergent Economies for Role Playing Games*: https://ianparberry.com/pubs/econ.pdf

External references inform design principles only. All game-specific prices, coefficients and content must remain consistent with the project's own balance specifications and playtest evidence.


## 5.5 Expanded seafood catalogue and catch legality

The generated catalog now authors all 155 entries in the price schedule: 100 fish, 47 invertebrates and 8 algae/seaweed goods. Stable item IDs and base prices are read from the same authored schedule and checked by repository validation. Regional availability is assigned per species; the encounter director filters candidates against both the current region and habitat. Shellfish and cephalopods participate in the fishing encounter/population system. Seaweed is harvested through the dedicated bait-free Seaweed Harvesting technique, which requires a Kelp habitat and uses its own renewable stock/recovery profile.

Every landed catch stores source region, fishing/harvest technique, landed weight, legal state and reason in its cargo placement. This provenance is additive to the existing schema-v8 placement record and survives save/restore. The legality policy evaluates:

- the six authored region boundaries and species range;
- the game's minimum landing weight by trade tier;
- protected-species permits;
- contract-only harvest permits;
- the closed Hollow Trench rule;
- species-specific permitted technique masks and required hook configurations and required hook configurations.

These are fictional gameplay rules, not real-world legal thresholds. Current authored thresholds are tiered by catch group: fish T1–T5 use 0.08, 0.15, 0.30, 0.60 and 1.00 kg; invertebrates use 0.03, 0.06, 0.10, 0.18 and 0.30 kg. An undersized, out-of-range, protected, unpermitted, closed-area or prohibited-technique landing is retained as a flagged catch, applies its authored fine when the wallet can pay, and is excluded from ordinary market offers. The dockside panel provides an explicit release action for flagged lots; a collected fine is not refunded. The cargo provenance prevents save/reload from turning an illegal catch into a legal sale.

Research submissions now award additional harvest permits: Reef Stewardship, Licensed Sea Cucumber Harvest and Managed Abalone Harvest. FSH-035 requires Reef Stewardship, handlining and the circle hook. INV-031 requires Managed Abalone Harvest. INV-037 through INV-041 require Licensed Sea Cucumber Harvest and are labelled as licensed contract intake. The Hollow Trench remains closed until the Outer Shoals research permit is earned. Harvest permits are persisted in the existing unlocked-permit collection but are kept out of the fog-of-war region-name list.

## 5.6 Destination-specific seafood markets

SeafoodMarketPolicy implements the six-destination by six-market-class matrix from the price schedule. Mara's market controller now reads the region where the player is selling, applies that destination factor once, and keys stock by regional market ID plus item ID. A sale in one destination cannot consume another destination's stock. Legacy Greyhaven market stock is normalized to the Greyhaven regional market during restore.

The dockside panel displays the active market name and destination factor on each itemized line. Contract-only lots are visibly labelled and require the matching harvest permit. Illegal catch lots are excluded from the selectable sale set and shown separately.

The market service can resolve a regional market during a destination dockside session. The generated Greyhaven scene currently provides the authored dockside hub. The remaining world-integration task is to place/open the corresponding dockside market terminal in each streamed destination scene; no market should be reachable merely by remote menu selection or while at sea. Regional price and stock logic is implemented; remote physical access is not yet claimed.

## 5.7 Measured expedition balance

The telemetry records up to 50 completed expedition samples in an additive namespaced progress flag. Each sample stores departure and destination region, departure loadout band, gross receipts, operating expenses, actual fuel, game-clock duration, landed/legal/flagged catch counts and catch weight. Loadout bands are captured at departure from installed engine × rod capability: STARTER below 0.85, AVERAGE from 0.85 to below 1.15, and OPTIMIZED at 1.15 or above.

The dockside report groups samples by destination and loadout band and shows sample count, average net, average net per game-hour, average fuel and legal-catch rate. Active-trip loadout and catch totals survive save/reload. This instrumentation does not automatically rebalance prices or costs. The controlled sample protocol and provisional target envelopes are in [Economy Balancing](../07_GAME_RULES_AND_BALANCING/ECONOMY-BALANCING.md). No actual Unity playtest data has been collected yet.
