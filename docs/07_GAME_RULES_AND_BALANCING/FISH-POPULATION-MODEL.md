# Regional Fish Population and Food-Web Model

> Project: WHERE THE SOUNDINGS END  
> Status: Working design baseline — numeric species parameters require simulation and playtesting  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-10  
> Scope: Persistent species abundance, daily growth, recruitment, overfishing, recovery, migration, predation, and catch-rate feedback

## 1. Purpose

Fishing grounds are living populations, not infinite loot tables. The player can reduce a species in one cove, reef, channel, offshore bank, or deep-water site without instantly deleting that species from an entire island. Repeated harvesting can reduce local abundance, the number of mature breeders, future recruitment, average catch size, and the chance of encountering a school.

The world continues to change while the player is away. Surviving animals grow, juveniles mature, breeding opportunities occur, populations receive recruits, animals die naturally, predators feed, and mobile species move through connected habitats.

The simulation must be persistent but affordable for a solo-first Unity project. Use aggregate population cohorts and habitat patches. Do not create one permanent GameObject for every fish in the simulated sea.

This is a design specification. It does not claim that a Unity runtime system has been implemented.

## 2. Spatial population hierarchy

Track population at three nested levels:

1. **Fishing ground / habitat patch:** the authoritative stock location. Examples: Greyhaven's East Channel, a Shallows seagrass bed, a Blackrock surge channel, a Drowned Parish flooded street, an Outer Shoals tuna corridor, or a Hollow Trench slope shelf.
2. **Region / island:** an aggregate view of its constituent fishing grounds. It is useful for maps, fishmonger reports, and regional recovery.
3. **Archipelago:** a high-level total used for conservation and migration checks.

A population key is:

`PopulationKey = SpeciesID + FishingGroundID`

A species can be healthy in one ground and depleted in another. Do not apply one global island multiplier to every fishing ground. Region summaries are weighted aggregates; they must not overwrite the state of local grounds.

Each fishing ground needs:
- Stable ID and parent region.
- Habitat type, depth band, substrate, cover, and nursery suitability.
- Seasonal reference abundance and biomass.
- Carrying capacity or maximum viable patch biomass.
- Neighbouring grounds and valid migration routes.
- Disturbance and habitat-condition state.
- Species permitted to occupy the patch.

The six major regions remain Greyhaven, the Shallows, Blackrock Reach, the Drowned Parish, the Outer Shoals, and the Hollow Trench. Their population behavior is not interchangeable; see the regional ecology document and the technical implementation specification.

## 3. Population state: abundance, biomass, and cohorts

For each species in each ground, retain at least:

| State | Meaning |
|---|---|
| Total abundance | Estimated number of living individuals / stock units |
| Total biomass | Sum of living individual weights |
| Juvenile cohort | Newly settled or young animals not yet fully vulnerable to ordinary gear |
| Subadult cohort | Growing animals approaching maturity |
| Adult cohort | Mature or nearly mature population |
| Spawning-stock biomass (SSB) | Mature biomass weighted by maturity and reproductive condition |
| Mean size / weight | Current average by cohort and catchable size class |
| Habitat condition | Quality of food, cover, nursery and spawning habitat |
| Recruitment buffer | Spawned / settling offspring awaiting their authored arrival delay |
| Seasonal reference | Expected healthy abundance for this ground and season |

`Biomass = Σ(CohortAbundance × CohortMeanWeight)`

`SSB = Σ(AdultAbundance × MeanWeight × MaturityFraction × SpawningCondition)`

Keep number and biomass separate. Individual growth increases biomass, not the number of animals. Recruitment increases the number of animals. Catch, predation, and natural mortality reduce abundance and biomass.

Cohorts can be represented as aggregated size/age bands. If a named creature must persist as an individual, it may have a dedicated record; that is an exception, not the default for schools.

## 4. The daily ecology clock

Use the game's calendar, not real-world elapsed time.

- A **game day** is one complete 24-hour in-world cycle.
- Catch removals apply immediately when the catch is resolved.
- Body growth, mortality, cohort maturation, reproduction checks, settlement, migration, and patch recovery run on scheduled daily or species-specific boundaries.
- A save/load, scene reload, boat travel, or sleeping action must not duplicate a daily update.
- When time advances across multiple days, process every missed ecology day deterministically or use a validated multi-day aggregation that produces the same result within defined tolerances.
- The daily update uses the day's temperature, season, tide summary, current, food availability, and habitat condition.

Do not update every population every rendered frame. Use stable event IDs and the saved last-processed game-day index.

## 5. Daily population accounting

For each cohort:

`N(next) = N + Recruits + MaturingIn + Immigration - MaturingOut - NaturalDeaths - PredationDeaths - FishingDeaths - Emigration`

Every term is bounded by the cohort's available population. The result cannot be negative. Migration transfers population; it does not create it.

Biomass is recalculated after individual growth, recruitment, mortality, catches, and movement. A catch removes the actual estimated number and weight from the size cohorts selected by that gear. Bycatch and catch-and-release mortality count as fishing deaths; a live release is not automatically a zero-impact action.

Do not add a second free logistic birth term on top of cohort recruitment. If the cohort model is active, reproduction and maturation are the population renewal mechanism. A simplified logistic model may be used only for species that deliberately have no cohort detail.

## 6. Individual growth: animals change size every game day

Living cohorts age every completed game day. Their average length and weight advance according to the species' life-history definition. This is how the world produces larger, heavier catches over time without simulating every fish as a persistent actor.

### 6.1 Fish growth

For fish using a von Bertalanffy-style growth curve:

`L(d+1) = L∞ - (L∞ - L(d)) × e^(-k_day)`

`W(d) = a × L(d)^b`

Where:
- `L` is mean length.
- `L∞` is the authored asymptotic length.
- `k_day` is a species-specific game-day growth coefficient.
- `W` is estimated weight.
- `a` and `b` are species-specific length-weight parameters.

This is a useful game model, not a claim that one growth curve perfectly describes every fish. Use authored parameters and age/size maturity thresholds per species.

### 6.2 Other animal groups

| Group | Daily growth representation | Special rule |
|---|---|---|
| Crabs and lobsters | Age and molt-progress counters advance daily | Size increases at successful molts; post-molt animals are temporarily soft and vulnerable |
| Squid and cuttlefish | Rapid cohort growth and short life-cycle progression | Growth, maturity, breeding pulses, and post-spawn mortality are species-authored |
| Shrimp and prawns | Cohort growth with molt stages | Recruitment and harvest can change size distribution quickly |
| Clams, oysters, scallops and other bivalves | Shell and tissue mass increase incrementally | Water quality, burial, food flow, and patch condition affect growth |
| Sea cucumbers and echinoderms | Slow size/condition progression | Recovery and maturity are deliberately slower than small pelagic fish |
| Game-native natural fauna | Use the closest biological life-history profile unless its dossier overrides it | Named individuals may have unique age, injury, and route records |
| Anomalous entities | No automatic biological growth rule | Only grow, reproduce, or recover when their anomaly dossier explicitly says so |

A rendered animal may look unchanged after one day if the size delta is too small to notice. Its cohort length and weight still advance; visible size classes change only when a meaningful threshold is crossed.

## 7. Reproduction, spawning, and recruitment

Spawning is not a random event rolled every frame. Each species has authored breeding windows and gates:
- Correct season, tide, temperature, light, or current window.
- Suitable adult condition and reproductive readiness.
- Enough mature spawning biomass.
- Suitable spawning and nursery habitat.
- Acceptable disturbance and food conditions.
- Remaining population / habitat capacity.

Spawning opportunity is reduced when mature fish are repeatedly removed. Use this normalized spawning-stock feedback:

`SpawnerFactor = clamp((SSB / SSB_reference)^0.80, 0, 1.15)`

`Recruitment = BaseRecruitment × SpawnerFactor × SpawnWindow × NurserySuitability × SettlementSurvival`

The factors are authored or derived per species and are bounded. If local SSB reaches zero, local reproduction produces no recruits; recovery then depends on immigration, habitat restoration, or a specific authored reintroduction event.

Spawned offspring enter a recruitment buffer. After the species' egg / larval / settlement delay, surviving recruits enter the juvenile cohort. They do not appear as catchable adults on the same day they spawn.

Nursery capacity and density-dependent survival prevent unlimited recruitment. Do not make every extra egg become a surviving fish; survival depends on habitat, food, density, and environmental conditions.

## 8. Overfishing and population health

Overfishing is evaluated per species and fishing ground, not solely per island.

### 8.1 Abundance and catchable stock

`StockRatio = VulnerableBiomass / SeasonalReferenceVulnerableBiomass`

Vulnerable biomass is the population actually available to the player's current method and gear. A ground can contain juveniles or deep sheltering adults but still have very little catchable stock.

### 8.2 Catch-rate feedback

`AbundanceCatchMultiplier = clamp(StockRatio^0.70, 0, 1.15)`

`ExpectedCPUE = BaseCPUE × AbundanceCatchMultiplier × HabitatSuitability × TimeWindow × GearSelectivity`

CPUE means catch per unit of fishing effort. This makes abundance decline produce fewer encounters, fewer bites, smaller or less frequent schools, and lower expected catch per trip. Gear selectivity, current, tide, weather, bait, and player technique still matter; a low catch is not always proof of overfishing.

Illustrative abundance effects from this formula:

| Vulnerable stock remaining | Approximate abundance catch multiplier |
|---:|---:|
| 100% | 100% |
| 75% | 82% |
| 50% | 62% |
| 25% | 38% |
| 10% | 20% |
| 0% | 0% |

The formula is continuous. Do not abruptly turn fishing off at an arbitrary status threshold.

### 8.3 Fishing pressure versus natural replacement

Track fishing removals over a rolling 14-game-day window:

`FishingPressure14 = FishingRemovalBiomass14 / max(NetNaturalProductionBiomass14, ε)`

Net natural production includes recruitment and body growth, less natural mortality and wildlife predation. Keep migration as a separately reported transfer so it cannot disguise archipelago-wide overharvest. If estimated natural production is effectively zero while fishing removals continue, mark pressure as critical rather than dividing by zero.

- Below 0.75: current removals are below estimated natural replacement.
- 0.75–1.00: removals are approaching natural replacement.
- Above 1.00: fishing is removing more biomass than estimated natural replacement.
- Above 1.50: severe pressure; recovery is unlikely while the same effort continues.

Use both stock level and rolling pressure. A large stock can be under heavy pressure before it looks depleted. A depleted stock may remain depleted even after fishing stops.

### 8.4 Player-facing stock states

Use a continuous formula internally and readable, delayed state labels externally:

| State | Approximate stock ratio | Player-facing meaning |
|---|---:|---|
| Healthy | 0.80–1.15 | Normal schools and ordinary catch opportunities |
| Pressured | 0.60–0.79 | Fewer encounters; fishers report lower supply |
| Depleted | 0.35–0.59 | Noticeably sparse schools, smaller average catches, weaker recruitment |
| Severely depleted | 0.10–0.34 | Rare encounters, few mature breeders, poor recovery while fishing continues |
| Near collapse | Below 0.10 | Almost no catchable stock; local spawning may fail |

The most severe applicable condition wins. A healthy stock with a very high rolling fishing-pressure ratio should be labelled **heavily fished / at risk**, not falsely reported as fully healthy. Use a short persistence window before changing public labels to avoid daily flicker.

## 9. What the player experiences

Population decline must be visible, understandable, and persistent:

- Schools become smaller, farther apart, or less frequent in repeatedly harvested grounds.
- Sonar / Sounding Rig surveys report lower density or weaker school returns.
- Bite, hook, and encounter opportunities fall in proportion to vulnerable stock.
- Large adults become less common when gear selectively removes large animals.
- Average catch weight and the proportion of mature adults can decline.
- Spawning displays, juvenile sightings, and nursery activity become less frequent when breeding stock or habitat is damaged.
- Fishmongers and local fishers mention that a species is getting scarce in a named ground.
- A regional report can reveal that a fishing patch is depleted even when the island-wide stock remains moderate.

Do not hide every consequence behind a numerical meter. Let observation, records, NPC dialogue, and the Sounding Rig teach the player what happened.

## 10. Recovery, rotation, and habitat damage

Stopping fishing does not instantly refill a ground. Recovery comes from:
- Surviving adults continuing to grow.
- Successful spawning and delayed juvenile recruitment.
- Juveniles maturing into adult and spawning cohorts.
- Immigration from connected healthy grounds.
- Reduced fishing pressure.
- Habitat recovery where nursery, feeding, and spawning grounds were disturbed.

Recovery speed is species-specific. Initial **game-balance targets**, not scientific promises:

| Life-history profile | Examples | Noticeable local recovery target when habitat and breeders remain |
|---|---|---|
| Fast-cycle | Anchovies, sardines, small scads, many prawns and squid | 5–20 game days |
| Moderate | Mullet, milkfish, rabbitfish, common coastal fish, swimming crabs | 15–60 game days |
| Slow | Large snappers, groupers, lobsters, sea cucumbers, deep-slope fish | 60–180+ game days |
| Very slow / sensitive | Long-lived deep-water species and fragile habitat-associated populations | 180–360+ game days; some sites may take longer |
| Harvested algae patch | Sea grapes and other managed seaweed beds | 7–30 game days if holdfasts remain; longer after patch damage |

These ranges describe early visible improvement, not full restoration. Species data overrides the class. An isolated ground with no mature breeders may recover much more slowly. Deep-water species must not recover at the same speed as anchovies simply because the player slept for three days.

Habitat damage reduces capacity and/or recruitment:
- Repeated anchoring or dragging gear through seagrass reduces nursery cover.
- Removing reef structure or damaging benthic habitat lowers shelter and settlement success.
- Stripping seaweed holdfasts lowers patch regrowth capacity.
- Disturbing a brooding site can cause abandonment if the species dossier supports that outcome.
- A healthy patch can recover fish before a damaged habitat has fully recovered; keep those processes separate.

The player can rotate fishing grounds, reduce effort, respect a temporary closure, remove ghost gear, or complete habitat-restoration work. These actions improve recovery but never spawn instant free fish.

## 11. Migration and food-web effects

Migration transfers stock along authored connected routes. Subtract the same cohort amount from the source that is added to the destination. Do not allow teleportation between disconnected islands.

Predators remove bounded prey biomass through the food-web system. A successful hunt consumes prey once; a failed hunt consumes no prey. If a prey ground becomes depleted, a predator may search elsewhere, switch prey, reduce activity, or leave. Do not make all predators attack the player because fish are scarce.

Fishing pressure can therefore change the local ecosystem:
- Fewer forage fish can reduce predator feeding opportunities.
- Predators may follow prey schools to a different ground.
- Scavengers may shift toward discards or carcasses.
- Herbivore depletion can alter algae pressure on reef or seagrass habitat where that relationship is explicitly authored.
- Effects must be species- and habitat-specific; do not simulate a universal catastrophic food-chain collapse after one fishing trip.

## 12. Species life-history profiles

Every sale-eligible species receives a life-history profile in the species data sheet. The existing catalogue IDs remain authoritative.

Required profile fields:
- Population model family.
- Growth coefficient / molt schedule.
- Maximum or asymptotic size and length-weight parameters where applicable.
- Age or size at maturity.
- Seasonal / tide-linked breeding window.
- Recruitment delay and juvenile survival.
- Natural mortality and predator vulnerability.
- Gear vulnerability by size / stage.
- Migration routes and home range.
- Habitat / nursery / spawning suitability.
- Recovery class and authored override values.

Use a small set of profiles to author the 155 market species efficiently. The profile is a starting point; it is not a substitute for real species-specific overrides where life history strongly differs.

## 13. Simulation scenarios and balancing

Run reproducible simulations for:
1. No fishing for 30, 90, 180, and 360 game days.
2. Light, regular, and heavy fishing in one ground.
3. Heavy fishing spread across every ground in one island.
4. Harvesting adults versus harvesting juveniles.
5. A fast-cycle species versus a slow deep-water species under identical effort.
6. Fishing while predators are active.
7. Migration from a healthy ground into a depleted neighbouring ground.
8. Recovery after effort stops, with healthy, damaged, and destroyed nursery habitat.
9. Sleeping / fast-forwarding, save/load, and scene reload across many days.
10. Fishing in a depleted ground while a healthy ground remains nearby.

Measure abundance, biomass, SSB, average size, juvenile/adult ratio, catch per unit effort, zero-catch days, recruitment, time to warning, collapse risk, recovery time, and migration conservation.

## 14. Acceptance tests

- Catch removes the correct number and biomass from the correct species, ground, and size cohorts immediately.
- No stock becomes negative and no patch exceeds its authored capacity.
- A local ground can be depleted while other grounds on the same island remain healthy.
- Heavy fishing reduces catch opportunities continuously; zero vulnerable stock produces zero ordinary catch.
- Repeated adult removal reduces spawning-stock biomass and future recruitment after the authored delay.
- Catching juveniles affects future adult abundance when those cohorts would otherwise mature.
- Each living cohort grows in age / size / weight once per completed game day.
- Growth changes biomass without creating new individuals.
- Juveniles mature only after their species-specific size / age gate.
- A spawning event cannot create same-day adult fish.
- Fast-cycle populations recover faster than slow / deep-water populations under comparable valid conditions.
- A severely damaged nursery or missing local breeders slows recovery.
- Migration conserves total abundance across connected grounds.
- Predation and bycatch are counted once; failed hunts and invalid fishing attempts remove no stock.
- Catch, recruitment, migration, and daily updates cannot duplicate after save/load.
- Inactive-region updates remain deterministic and do not silently remove quest-critical named creatures.
- Cosmetic schools do not change gameplay stock or consume gameplay RNG.

## 15. Related specifications

- [Regional population simulation — Unity architecture](../08_TECHNICAL_DESIGN/REGIONAL-POPULATION-SIMULATION.md)
- [Regional ecology and creature distribution](../../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md)
- [Creature life cycles and ecological behaviour](../../12_AI_AND_SIMULATION/CREATURE-LIFE-CYCLES-AND-ECOLOGY.md)
- [Seafood selling price schedule](SEAFOOD-SELLING-PRICE-SCHEDULE.md)
- [Economy mathematics](ECONOMY-MATHEMATICS.md)
- [Balancing and simulation](BALANCING-AND-SIMULATION.md)

## 16. Research references

These sources support the general structure of the model. The numeric game-day rates and fictional species parameters remain design assumptions until playtested.

- FAO, [Chapter 1: Fisheries Management — biological considerations](https://www.fao.org/4/y3427e/y3427e03.htm): recruitment, body growth, natural mortality, fishing mortality, and maintaining reproductive age structure.
- FAO, [Management techniques for elasmobranch fisheries — surplus production models](https://www.fao.org/4/a0212e/A0212E14.htm): biomass accounting as production and recruitment minus natural mortality and catch.
- NOAA Fisheries, [Stock Assessment Model Descriptions](https://www.fisheries.noaa.gov/insight/stock-assessment-model-descriptions): stock size, reproductive rate, mortality, catchability, and reference points.
- Fisheries and Oceans Canada, [Introduction to stock assessment](https://www.dfo-mpo.gc.ca/science/species-especes/fisheries-halieutiques/assessment-evaluation/module-1/09-eng.html): abundance, biomass, recruitment, cohorts, fishing mortality, natural mortality, and catchability.
- University of Florida IFAS, [Fish Population Recruitment](https://edis.ifas.ufl.edu/fa222): juvenile recruitment and density-dependent survival.

