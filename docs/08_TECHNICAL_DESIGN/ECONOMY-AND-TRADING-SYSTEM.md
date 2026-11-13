# Economy and Trading System

> Project: WHERE THE SOUNDINGS END  
> Status: Greyhaven seafood/salvage sales, bait/supply purchasing, fuel, repair and dockside service runtime partially implemented; full catalogue and playtest verification pending  
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

**Not yet implemented:** Harbourmaster salvage/research contracts, complete catch legality and protected-species rules, the full catalogue price authoring, active rod/reel/line equipment-slot application, destination-specific travel-market routes, detailed fuel range/net-profit analytics, and full Unity Play Mode balancing. The editor builder must be run in Unity to regenerate the Greyhaven scene with the new runtime components and authored cargo data.

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

## 6. NPC relationship and trade behavior

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

## 7. Purchasing, stock and cargo

Purchases must validate, in order: vendor open, vendor accepts/sells item, relationship gate, available vendor stock, valid quantity, wallet balance, destination hold/equipment capacity, and final price. Only then commit money and inventory atomically.

- Bait is consumed when a cast/technique actually uses it, not merely by opening a menu.
- Tackle and equipment are durable inventory/equipment items; do not invent wear rates until the repair and equipment model is authored.
- A purchase cannot be resold to the same vendor for a profit. Buy and sell quotes must be separate and disclosed.
- Equipment upgrades must identify the capability unlocked, additional operating cost, route/depth/method access and estimated payback trips.
- Cargo hold space remains a physical constraint. No purchased item may appear in the hold if it cannot fit.
- Vendor stock is persistent and refreshes only by the documented stock simulation; scene reloads and save reloads must not restock it.

## 8. Repairs and vessel services

Use existing `VesselConditionController` hull/engine integrity and `SpatialCargoHold` damaged-cell state. The economy is a payer and transaction coordinator; it does not own vessel physics or cargo geometry.

Repair quote components:
- Hull: missing hull integrity × authored cost per integrity point.
- Engine: missing engine condition × authored cost per integrity point.
- Hold: number/type of repairable blocked cells × authored material/labour cost.
- Equipment: only if a real equipment-condition field and repair contract exist.

Show a quote before confirmation. Repairs restore only the selected component and never exceed 100%. A failed or unaffordable repair changes nothing. Do not charge a fixed fee when no repair is required.

## 9. Fuel and operating costs

Fuel is a voyage resource and is stored on the vessel. Consumption must be tied to actual propulsion/engine load rather than wall-clock time while docked or paused. The player should be able to see current fuel, estimated remaining range and the estimated cost to refill before departure.

- Burn fuel only while the propulsion system is under load.
- Scale consumption from throttle/engine load and engine condition using one tested formula.
- No fuel drain while paused, in dialogue, docked with engine off, or idling with zero propulsion.
- At zero fuel, propulsion is unavailable; the vessel can drift and the player retains a fair recovery option (e.g. a disclosed paid tow/rescue), rather than an unrecoverable softlock.
- Refuelling is a priced service with an explicit quantity and total. Do not require a repetitive manual refuel click after every trip.
- Bait, ice and other consumables are charged only when purchased/consumed. Do not add a flat hidden expedition fee.

The exact consumption and service price coefficients remain tuning values and must be measured during the vertical slice. Do not copy *DREDGE* fuel rates or assume a particular real-world vessel's litres-per-kilometre figure.

## 10. Salvage, materials and discoveries

Salvage has three possible uses, resolved by item definition and contract:
1. Sell as commodity to an accepting buyer.
2. Retain for a repair/upgrade recipe when the crafting/upgrade system is implemented.
3. Submit as evidence or a contract objective, consuming it only when the player confirms the submission.

Do not let a material be sold and simultaneously count toward a repair recipe or quest. One cargo instance has one owner/state at a time. Unique research payouts are claimed once per stable specimen/evidence ID and persisted. A discovery's narrative value may exceed its commodity price; not every artifact should be sellable.

## 11. Transaction integrity and persistence

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

## 12. UI and feedback

The dockside hub keeps exclusive modal ownership. Add service actions to existing locations instead of opening competing overlays:
- Fish Market: inventory selection, offer breakdown, buyer stock/demand, confirm sale.
- Repair Shed: hull/engine/hold status, itemized quote, confirm repair, fuel quote.
- Net Mender: tackle/bait catalogue, vendor stock, price, cargo/equipment destination.
- Harbour Office: contracts, permits, research submission and posted notices.

Every transaction displays item, quantity/unit, base price, destination factor, quality/freshness, demand, relationship modifier, total and post-transaction wallet. Purchases display stock and hold fit. Repairs display current/target condition and line-item cost. Errors must explain the failed condition. Keyboard and gamepad focus must be preserved.

## 13. Implementation sequence and acceptance gates

### Runtime foundation update — 2026-11-14

`EconomyPolicy` implements deterministic sale-line rounding, bounded quality/freshness/demand/live modifiers, stock-driven demand, repair/fuel quote arithmetic and overflow-safe wallet deltas. `SaveGameData` is schema v8 with a cents wallet and transaction-id ledger; v7 whole-dollar balances migrate to cents. Cargo instances now retain total sale quantity in their authored unit (stack count for ordinary stacked goods; landed kilograms for each fish), and fishing records the already-calculated landed weight rather than discarding it. Item definitions expose authored price/unit/ordinary-sale eligibility fields; existing assets still require reviewed schedule values before they can be sold.

`EconomyWalletController` now owns the live integer-cent wallet and bounded idempotency ledger on the generated vessel and participates in the existing save coordinator. This does not yet implement vendor stock/demand persistence, sales/purchases, fuel burn, paid repairs, NPC service bindings or dockside economy UI. Do not treat the wallet and pure policies as a playable trading loop.



### Runtime implementation — Mara's Greyhaven fish market

MaraFishMarketController is attached to the persistent player vessel and uses the existing SpatialCargoHold, EconomyWalletController, and NpcAgent for ownership. It only opens while Mara's market_shift activity is active and her existing CanTrade relationship gate passes. The initial authored commodity catalogue is limited to FSH-001, FSH-002, FSH-003, FSH-005 and FSH-007 at their existing USD/kg schedule values; aberrant specimens, salvage, quest cargo, equipment and unpriced definitions remain ineligible.

The market builds itemized line offers from each cargo instance's retained sale quantity, quality and freshness. Demand is buyer/item-specific and recalculated from persisted current/target stock using the existing bounded policy. A sale requires a unique caller-supplied transaction ID. The hold validates every selected instance first, invokes the idempotent wallet commit, then removes the complete selection; failed validation or payment leaves cargo untouched. Successful sales increase Mara's stock and reduce future demand. Market stock is saved in the existing economy DTO without changing schema v8; old v8 saves normalize to an empty market list and seed stock when first quoted.

Current boundary: this is a callable runtime transaction service, not yet a player-facing market panel. It does not implement purchasing, passive market restock, salvage buying, relationship discounts, Tomas/Inez services or full dockside receipts. The editor builder must be run in Unity to regenerate the vessel with the new controller and author the starter price fields. Unity compilation and Edit Mode execution remain unverified.

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

## 14. Research references

- Black Salt Games developer interview, *Trawling in the deep: How Black Salt Games made spooky fishing RPG DREDGE*, Game Developer: https://www.gamedeveloper.com/design/trawling-in-the-deep-how-black-salt-games-made-spooky-fishing-rpg-i-dredge-i-
- Joel Mason, *Deep Dive: The surprising depth of spatial inventories in DREDGE*, Game Developer: https://www.gamedeveloper.com/design/deep-dive-the-surprising-depth-of-spatial-inventories-in-dredge
- Black Salt Games / Apple Developer, *Discover how DREDGE was reimagined for Apple platforms*: https://developer.apple.com/videos/play/meet-with-apple/247/
- *Sunless Sea* trade design notes, Failbetter Games via Steam News: https://store.steampowered.com/news/posts/?appids=596970&enddate=1511952977
- Unity, *Separate game data and logic with ScriptableObjects*: https://unity.com/how-to/separate-game-data-logic-scriptable-objects
- Unity Manual, *ScriptableObject*: https://docs.unity3d.com/6000.1/Documentation/Manual/class-ScriptableObject.html
- Jonathon Doran and Ian Parberry, *Emergent Economies for Role Playing Games*: https://ianparberry.com/pubs/econ.pdf

External references inform design principles only. All game-specific prices, coefficients and content must remain consistent with the project's own balance specifications and playtest evidence.
