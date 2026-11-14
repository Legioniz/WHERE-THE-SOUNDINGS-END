# Economy Balancing and Expedition Measurement

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime measurement implemented; numerical tuning awaits Unity playtest samples  
> Owner: Project lead  
> Version: 1.0  
> Last reviewed: 2026-11-14  
> Currency: USD ($), integer cents at runtime

## 1. Purpose

This document defines how expedition profitability is measured and how economy values may be tuned. The game must reward preparation and route knowledge without allowing one optimal commodity or route to erase the survival-horror pressure.

**No playtest results are claimed here.** The runtime now records real player trips. Prices and target envelopes remain authored starting points until enough Unity playtests exist to support changes.

## 2. Authoritative inputs

- [Seafood Selling Price Schedule](SEAFOOD-SELLING-PRICE-SCHEDULE.md): 100 fish, 47 invertebrates, 8 algae/seaweed, units, base prices and destination market matrix.
- [Economy Mathematics](ECONOMY-MATHEMATICS.md): line sale formula, rounding, bounded quality/freshness/demand modifiers, operating costs and transaction guardrails.
- [Economy and Trading System](../08_TECHNICAL_DESIGN/ECONOMY-AND-TRADING-SYSTEM.md): vendor roles, regional markets, equipment, permits and dockside flow.
- [Economy Integration Audit](../13_PRODUCTION_MANAGEMENT/ECONOMY-INTEGRATION-AUDIT.md): current source integration and verification boundaries.

Do not create a second species price table. Do not apply a rarity payout multiplier or destination multiplier twice.

## 3. What a completed expedition records

ExpeditionEconomyTelemetry starts when the vessel applies meaningful propulsion and completes after the player opens and closes a dockside service session. This includes the return sale and service transactions in the same trip.

Each of the latest 50 completed samples records:

- departure and destination region IDs;
- departure loadout band: STARTER, AVERAGE or OPTIMIZED;
- gross wallet receipts;
- operating expenses;
- actual fuel consumed, including emergency reserve fuel;
- action-driven game-clock duration;
- net result and net income per game-hour (derived);
- landed catch count, legal catch count, flagged catch count and landed weight.

Equipment purchases are capital investment and are excluded from recurring operating expenses. Fuel, bait, repairs and other service costs are operating expenses. Research stipends and other positive wallet transactions are receipts; review them separately if they dominate a sample.

The dockside panel groups results by destination and loadout band and displays average net per trip, average net per game-hour, average fuel and legal-catch rate. The report is observational: it does not silently change prices, stock, fuel use or encounter odds.

## 4. Loadout bands

The departure band uses the installed engine and rod capability at trip start:

| Band | Combined capability | Interpretation |
|---|---:|---|
| STARTER | Below 0.85 | Baseline, absent or worn equipment |
| AVERAGE | 0.85 to below 1.15 | Serviceable mixed or upgraded loadout |
| OPTIMIZED | 1.15 and above | Upgraded engine and rod in strong condition |

Combined capability is the engine propulsion multiplier multiplied by rod cast-range multiplier. The band is captured once at departure; equipment condition changes later do not rewrite the trip's classification.

## 5. Controlled playtest protocol

Run a clean save and use the same game-clock duration for comparable trips. Record the build commit, destination, loadout, selected technique, gear condition, cargo capacity, route, weather/sea state, and whether the player used the emergency reserve.

Minimum useful sample:

1. At least five completed trips for each tested destination/loadout combination.
2. For the primary Greyhaven loop, collect at least five STARTER, five AVERAGE and five OPTIMIZED samples.
3. Repeat high-value routes with ordinary and poor catch quality to measure variance.
4. Include at least one trip with repairs, one with bait resupply, one with emergency reserve use, and one with flagged catches.
5. Do not mix tutorial/research reward trips with ordinary commodity trips when judging repeatable earning power. Mark the reward transaction in the test notes.

A trip is not a valid completed sample until the telemetry closes it at dockside. A save/reload during an active trip must preserve the in-progress totals and loadout band.

## 6. Provisional target envelopes

These are **design targets for evaluation, not measured outcomes or guaranteed payouts**. They are intentionally broad because travel distance, catch mix, demand, freshness and service decisions change results.

| Scenario | Desired economic role | Initial net/game-hour target |
|---|---|---:|
| STARTER / Greyhaven inshore | Safe learning loop; can cover bait and a modest fuel top-up | $2–$8 |
| AVERAGE / Shallows or Blackrock | Upgrade payback and repair reserve become achievable | $6–$16 |
| OPTIMIZED / Outer Shoals or Hollow Trench | Premium risk can outperform coastal work, but not without fuel and damage exposure | $10–$24 |

Do not tune a destination to its top end based on one lucky catch. Evaluate median and spread alongside the mean. If the target envelope conflicts with the actual risk, route length or player effort, revise the target first and document why.

## 7. Tuning order

Change one class of variable at a time and re-run the same sample:

1. **Acquisition and ecology:** regional availability, stock, recovery and encounter weighting.
2. **Legal eligibility:** authored minimum weight, regional closure, permit and technique restrictions.
3. **Sale price:** base price from the schedule, only after verifying unit and destination factor.
4. **Market behavior:** buyer stock, restock cadence and bounded demand.
5. **Operating costs:** fuel burn, bait cost, repairs and emergency reserve.
6. **Equipment:** condition wear, service price and capability modifiers.

Never compensate for an illegal catch by paying more for it. Never make a distant market profitable solely by multiplying the catch's origin region. The market factor belongs to the destination where the player physically sells.

## 8. Acceptance gates

- No route produces repeatable profit without meaningful time, fuel, cargo or condition cost.
- A short starter trip remains viable without requiring an optimized loadout.
- Optimized equipment improves efficiency but does not remove catch risk or make every destination equally profitable.
- Destination markets have distinct stock and demand state; a sale at one port does not mutate another port's stock.
- Legal catches remain the only ordinary commodity sale; contract-only products require the correct harvest permit.
- Market prices use one destination factor and one relationship modifier at most.
- A trip's recorded receipts, expenses, fuel and catch counts survive save/reload.
- At least five comparable samples exist before a numerical balance change is accepted.
- Any price or rule change updates the price schedule, this document, the technical design, tests and backlog together.

## 9. Current verification boundary

Source now records and reports measured expedition samples, but **no Unity playtest sample set has been collected in this development environment**. GitHub syntax/static validation is not runtime economy evidence. Unity 6.3 scene regeneration, Edit Mode tests, Play Mode trips and a Windows Development Build remain required before the provisional target envelopes can be confirmed or adjusted.
