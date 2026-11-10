# Economy Mathematics

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline — numerical tuning remains provisional  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10  
> Currency: US dollar (USD / $) — in-game dollars

## 1. Purpose

Define how fishing income, research rewards, operating costs, buyer demand, and equipment progression interact. The economy should reward knowledge, preparation, and safe landings without allowing one species, buyer, anomaly, or repeatable contract to dominate every other activity.

The [Seafood Selling Price Schedule](SEAFOOD-SELLING-PRICE-SCHEDULE.md) is the single source of truth for USD species prices, sale units, six destination-island market factors, aberrant specimen outcomes, and finite anomaly rewards. This document defines the calculations that use those values.

## 2. Revenue model

For an ordinary eligible product line:

`LineRevenueUSD = roundToCent(SaleQuantity × BaseDocksidePriceUSD × IslandMarketFactor × QualityFactor × FreshnessFactor × DemandFactor × LiveConditionFactor)`

Where:
- `SaleQuantity` is saleable whole weight in kg, unless the schedule defines an individual, processed, or special weight unit.
- `BaseDocksidePriceUSD` comes from the price schedule. It is not a retail price.
- `IslandMarketFactor` comes from the market class and the island where the product is sold. It is applied once and does not depend on where the catch was caught.
- `QualityFactor` represents visible size grade, damage, and landing condition.
- `FreshnessFactor` represents time, temperature, and storage since landing.
- `DemandFactor` represents the selected buyer's current stock and posted demand.
- `LiveConditionFactor` is 1.00 by default, or 1.10–1.15 for an eligible crab / lobster delivered alive and intact to a buyer that accepts live stock.

`TripGross = Σ(LineRevenue) + AcceptedContracts + FirstTimeResearchRewards`

`TripNet = TripGross - Fuel - Bait - Ice - Repairs - Fees - Consumables - ExpectedLoss`

`ExpectedProfitPerHour = AverageTripNet ÷ AverageTripDurationHours`

Calculate each species / buyer line separately, round to the nearest cent, and then sum the transaction. Do not round intermediate factors or apply the same modifier twice.

Island-market multipliers are distinct from temporary demand. The regional factor is stable for each market class; demand changes with visible buyer stock, orders, events, and shortages.

## 3. Modifier boundaries

| Factor | Working range | Application |
|---|---:|---|
| Island market | 0.50–1.40 | Destination market and seafood class; see the seafood price schedule |
| Quality | 0.60–1.20 | Poor handling / damage to premium condition |
| Freshness | 0.35–1.00 | Warm delay to promptly chilled or properly held catch |
| Demand | 0.75–1.35 | Buyer inventory, local event, season, storm shortage, or posted order |
| Legal eligibility | 0 or 1 | Hard gate; illegal / protected / closed catch is rejected |
| Live shellfish condition | 1.00 or 1.10–1.15 | Optional premium only for eligible intact live animals |

Quality and freshness must be explained by observable catch state. Demand changes must be discoverable through buyer stock, dialogue, a board, or a contract. Clamp demand to the stated range; never use an unbounded multiplier.

There is no separate rarity multiplier in the sale equation. Rarity controls encounter availability and progression. The base price expresses expected market value. Avoid multiplying by both a T1–T5 tier and the schedule price.

## 4. Quantity and product handling

- Ordinary whole fish and most invertebrates use landed whole weight in kg.
- Special units override weight where listed: mature sea urchins are priced per approved individual; jellyfish use washed / drained weight; seaweed uses wet harvest weight.
- A catch that is undersized, from a closed area, above a quota, collected with prohibited gear, or otherwise ineligible is not sold as a discounted product. Reject it or follow the release / report rule.
- Do not pay for retained water, ice, bait, containers, or packaging weight.
- Processing may change both sale quantity and unit price. It must use a recorded yield and must account for time, fuel, storage, packaging, and waste. Do not apply a premium rate to the original whole weight and then also sell the processed by-product.
- Live crab and lobster premiums require a living, intact animal at buyer handoff. Dead or damaged specimens use the ordinary freshness / quality assessment.

## 5. Aberrant and unnatural specimen rewards

- Aberrant specimens inherit a base species rate and use the inspection result and condition rule in the price schedule.
- A first verified research reward is paid once per ABR ID per save, after the record is Reviewed and submitted.
- Unnatural specimens use a fixed first-record contract, not a per-kg product sale. Each UNC ID pays once per save after the evidence is accepted.
- Protected wildlife remains unsellable. A conservation reward compensates documentation, rescue, or safe release; it does not transfer ownership of the animal.
- Duplicate catches, replayed events, save reloads, repeated samples, and repeated dialogue cannot reset a claimed reward.

## 6. Buyer demand and market saturation

Each buyer has:
- A preferred product list.
- A visible intake capacity or stock target.
- A demand factor inside 0.75–1.35.
- A refresh trigger based on elapsed game time, supply delivery, or a scheduled market event.

Demand falls only when buyer stock or order capacity visibly fills. The game must not silently lower prices because the player is doing well. A posted order may raise demand for its named goods, but the order must have a quantity, deadline, buyer, and explicit payout.

Do not let the player repeatedly sleep, reload, leave and re-enter a scene, or switch buyers to reset the same order. Persist contract acceptance, fulfillment, and reward state.

## 7. Trip costs and net income

Track operating costs independently:
- Fuel: distance, engine load, and time under power.
- Bait and consumables: amount used, not a flat fee per cast.
- Ice / storage: capacity and time; upgrades reduce spoilage rather than creating free value.
- Repairs: actual hull, engine, electrical, and equipment damage.
- Fees: only where the world has an established dock, license, or service charge.
- Expected loss: probability-weighted cost of damaged gear, lost catch, or recovery events.

`ExpectedLoss = Σ(ProbabilityOfOutcome × CostOfOutcome)`

Do not charge the player for a cost that did not occur unless it is a clearly disclosed fixed service fee. A safe, short inshore trip should not be made unprofitable by mandatory hidden costs.

## 8. Upgrade affordability and progression

For every purchasable upgrade, record:
- Price.
- Capability gained.
- New route, depth band, fishing method, or safety option unlocked.
- Additional operating cost.
- Expected incremental net income per trip.
- Expected payback trips.

`PaybackTrips = UpgradePrice ÷ AverageIncrementalTripNet`

Use the incremental profit created by the upgrade, not the player's full fishing income. If the upgrade has no measurable payback, document its non-financial purpose (safety, access, story, or convenience).

The first boat and starter gear must support reliable income in Greyhaven and the Shallows. The player should be able to view destination-market prices before committing to a longer trade route. Progression into offshore and deep-slope fishing should require both capability and operating-cost planning, not merely a price gate.

## 9. Initial playtest targets

The following are initial gross-revenue targets before fuel, bait, repairs, and fees. They must be validated with measured trip time and catch distributions.

| Activity | Target gross return | Intended role |
|---|---:|---|
| Short inshore trip | $3.96–$11.10 | Starter income; low gear and fuel risk |
| Productive coastal trip | $11.10–$28.54 | Regular progression income |
| Targeted reef / crab / squid trip | $14.27–$39.64 | Skill and location reward |
| Offshore pelagic trip | $28.54–$79.29 | Mid-to-late progression, meaningful fuel and risk |
| Deep-slope contract trip | $31.72–$87.22 | Specialized gear and controlled catch volume |
| First verified unnatural record | $19.03–$57.09 | One-time research reward, not repeatable income |

A trip is balanced by net income per hour, risk, fuel use, and required skill—not by gross sale value alone. No activity should dominate every other route in income, safety, and convenience simultaneously.

## 10. Simulation and anti-exploit tests

Simulate at least three player profiles:
1. New player: starter rod, low storage, imperfect landings, short familiar routes.
2. Average player: mid-tier equipment, mixed fishing methods, ordinary knowledge of tides and habitats.
3. Optimized player: route planning, species knowledge, high-quality handling, upgraded storage and engine.

Run each against:
- Low, normal, and high abundance.
- Low and high buyer demand.
- Short and long trips.
- Fresh, chilled, and delayed sales.
- Ordinary catch, contract catch, and unique discovery rewards.

Acceptance checks:
- Verify the schedule's 100 fish, 47 invertebrate, and 8 algae price IDs are complete and unique.
- A 2 kg standard milkfish sale returns $4.60 at neutral market factor 1.00 before other modifiers.
- Five approved mature sea urchins return $3.55 before modifiers.
- Ineligible catch returns zero ordinary-sale revenue.
- A first-time research contract cannot be claimed twice.
- No buyer or contract can be refreshed by a scene reload.
- No rarity tier is multiplied on top of an explicit species base price.
- Island-market factors apply once at the destination buyer; they are not stacked with a second hidden regional premium.
- Compare gross return, net return, and net income per hour before approving upgrade costs.

## 11. Related specifications

- [Seafood Selling Price Schedule](SEAFOOD-SELLING-PRICE-SCHEDULE.md)
- [Catchable Seafood and Market Species Catalog](../../04_CHARACTERS_AND_ENTITIES/CATCHABLE-SEAFOOD-CATALOG.md)
- [Mutated and Unnatural Catchable Specimens](../../04_CHARACTERS_AND_ENTITIES/MUTATED-AND-UNNATURAL-CATCHES.md)
- [Fish Population and Food-Web Model](FISH-POPULATION-MODEL.md)
- [Development Roadmap](../13_PRODUCTION_MANAGEMENT/DEVELOPMENT-ROADMAP.md)
