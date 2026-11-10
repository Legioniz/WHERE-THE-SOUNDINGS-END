# Fish Population and Food-Web Model

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline — proposed ecology expansion  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## 1. Purpose

Represent fish abundance by species and region so that fishing, predator feeding, migration, spawning, and habitat disturbance have understandable consequences. The first implementation uses aggregate stock units, not hundreds of simulated fish agents.

A school rendered for atmosphere may be cosmetic. Promote it to a gameplay school only when its stock, location, or behaviour affects fishing, predator feeding, a quest, or player observation.

## 2. Regional stock model

For species s in region r, update stock at scheduled ecological boundaries:

N(next) = clamp(N + G + M_in - M_out - C - P, 0, K)

Where:
- N: current regional stock units.
- G: natural recruitment / recovery during the interval.
- M_in, M_out: migration into and out of the region.
- C: player catch removed from the stock.
- P: predation / wildlife feeding removal.
- K: authored regional carrying-capacity cap.

A simple first-pass recovery term is logistic:

G = r × N × (1 - N/K) × Δt

Where r is the region/species recovery rate. Do not also add an independent birth term unless the model is explicitly split into juvenile and adult cohorts; otherwise recruitment may be counted twice.

Clamp all removal terms so the stock cannot become negative. Avoid updating tiny floating-point stock changes every frame. Use a fixed ecology interval and stable event IDs.

## 3. Habitat suitability

A species may prefer a habitat based on depth, temperature, current, time, weather, cover, and season. A lightweight suitability score can be:

S = w_d D + w_T T + w_c C + w_t Time + w_w Weather + w_h Habitat

Each factor is normalized to [0,1], the weights are authored per species, and the final score is clamped to [0,1]. This is a game-feel suitability function, not a validated ecological distribution model.

Use suitability to influence:
- School density and presence.
- Feeding-ground choice.
- Migration direction and timing.
- Predator hunting opportunities.
- Catch probability / expected catch.
- Breeding and nursery habitat eligibility.

Do not make every species respond to all six factors. Use only factors that are meaningful for that species and that the player can learn through observation.

## 4. Predator and prey interaction

Wildlife feeding removes a bounded quantity of prey stock. A feeding event needs:
- Predator species / creature ID.
- Prey species / stock ID.
- Region and feeding-ground ID.
- Amount consumed.
- Simulation tick and stable event ID.
- Whether the result is visible, off-screen, or abstracted.

Predator hunting should not target the player by default. Normal food acquisition belongs to the creature life-cycle system. Player-directed hostile behaviour belongs to the threat AI system.

Predators should not instantly erase schools. Limit each feeding event by both predator appetite and available prey stock. Failed hunts remove energy/time, not prey stock. Repeated predation can move a prey school or reduce local abundance, which may lead predators to search elsewhere.

## 5. Feeding, hunger, and condition

Creature hunger and energy are per-creature or per-population condition values, not substitutes for regional fish stock.

- Feeding lowers hunger.
- Rest lowers energy expenditure.
- Activity and failed hunts spend energy.
- Good food, rest, and a safe habitat can support gradual healing.
- Injury reduces hunting capability or increases the chance of sheltering.
- A successful feed does not instantly restore all health or make a predator permanently full.

Use species-specific feeding amounts and cooldowns. Do not expose universal hunger bars to the player; show ecology through schools, feeding traces, activity, and sounds.

## 6. Migration and movement between regions

Migration transfers stock, it does not duplicate it. For a source and destination region, subtract the same quantity that is added, and validate both regions' habitat suitability and capacity. Use authored neighbouring zones or routes; do not let random migration move species across disconnected map regions.

A creature may move between feeding, resting, and breeding grounds while regional stock moves as an aggregate. A named visible creature can have its own route, while the wider population is represented by stock values.

## 7. Spawning and population cohorts

Breeding is scheduled only during eligible species / region windows. Conditions include suitable habitat, seasonal or tide phase, adult condition, disturbance, and population capacity.

Start with aggregate recruitment:
- A valid breeding event adds a bounded recruitment amount after an authored delay.
- Juveniles may be represented as a cohort or ratio, not individual GameObjects.
- A scheduled maturation boundary can move part of the cohort into the adult stock.
- Failed breeding conditions skip the current window; they do not roll every frame.
- Player actions may reduce local breeding success, but one normal fishing trip must not permanently wipe out a species.

If the game does not need juvenile / adult distinctions for a species, use a single stock value and a simple recruitment event. Add cohort complexity only when it produces visible gameplay.

## 8. Catch and ecology feedback

Track:
- Stock trajectory by species and region.
- Catch per trip and catch per unit effort.
- Failed / zero-catch trips.
- Predation removal.
- Migration in / out.
- Breeding recruitment and recovery time.
- Rare / anomalous species encounter rate.

Show consequences through changed school density, fishmonger supply, NPC comments, altered feeding grounds, and region survey results. Avoid hidden irreversible punishment.

## 9. Balancing scenarios

Run offline simulations for:
1. No player fishing.
2. Light, regular fishing.
3. Heavy fishing in one region.
4. Fishing while predators are active.
5. Seasonal migration and breeding.
6. Disturbance of a breeding / nursery ground.
7. Recovery after the player leaves a region.

Evaluate whether the stock collapses, recovers too quickly, or becomes effectively infinite. Tune r, K, catch removal, predation, migration, and recruitment together.

## 10. Acceptance tests

- Stock never falls below zero or exceeds carrying capacity.
- Migration conserves total stock between connected regions, except for explicit recruitment / mortality.
- Replaying a feeding event after save / load does not remove prey stock twice.
- A failed hunt does not remove prey stock.
- A predator's ordinary feeding does not trigger player-directed combat.
- Breeding adds stock only on a valid scheduled opportunity.
- Cosmetic schools do not alter stock or gameplay RNG.
- Heavy fishing produces readable but recoverable ecological consequences.
- Inactive-region updates use coarse steps and preserve quest-critical species / events.
