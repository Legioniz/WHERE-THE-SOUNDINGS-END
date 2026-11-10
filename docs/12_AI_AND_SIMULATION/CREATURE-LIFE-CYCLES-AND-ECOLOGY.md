# Creature Life Cycles and Ecological Behaviour

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed system specification — requires prototype validation  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10  
> Scope: Wildlife and creature life simulation, feeding, rest, recovery, reproduction, migration, and anomalous behaviour

## 1. Design purpose

Creatures should feel as though they existed before the player arrived and will continue existing after the player leaves. They need more than an idle animation and a chase state: they should feed, avoid danger, rest, recover, migrate, defend territory, reproduce where appropriate, and respond to changes in their habitat.

The system must remain economical enough for a solo-first Unity project. It is **not** a full biological simulator. Simulate the behaviours that affect player perception, ecology, quests, or gameplay. Abstract the rest into schedules, population cohorts, and deterministic event outcomes.

Three rules govern the design:

1. **Life behaviour is separate from hostility.** A predator hunting fish is not automatically hunting the player.
2. **Species have different needs and rules.** Do not give every creature the same universal daily routine.
3. **Unnatural behaviour is deliberately rule-bound.** It may violate ordinary ecology, but its pattern must be learnable and its gameplay consequences must be fair.

This document is a proposed project design. Species-specific values and supernatural rules are not canonical until approved in their creature dossiers.

## 2. Behaviour architecture: three independent layers

Do not put sleeping, feeding, reproduction, and player combat into one giant state machine. Use three layers that can communicate through explicit events.

### 2.1 Life state

Describes the creature's biological or ecological condition:

- Resting / sleeping
- Active
- Hungry / foraging
- Feeding
- Recovering / healing
- Courtship / spawning
- Migrating
- Sheltering
- Fleeing / avoiding danger
- Inactive / abstracted

### 2.2 Tactical state

Describes a short-term action:

- Wander
- Patrol
- Investigate
- Follow
- Hunt
- Stalk
- Ambush
- Attack
- Flee
- Guard
- Search
- Return to habitat

### 2.3 Encounter / anomaly state

Describes the creature's relationship to the player or a supernatural event:

- Unaware
- Curious
- Warning
- Stalking the boat
- Pursuing
- Attacking
- Disengaging
- Disturbed
- Mimicking
- Looping / repeating
- Manifesting / withdrawing

The life controller owns long-term scheduling. The tactical controller selects immediate actions. The threat brain owns player-directed hostile encounters. An anomaly controller owns impossible events. They exchange typed intents such as `FoodDetected`, `Threatened`, `InjuryReceived`, `RestSiteReached`, `SpawningWindowOpened`, and `AnomalyCueReceived`.

A creature may be biologically active while tactically resting, or it may be feeding while remaining alert to danger. A hostile encounter can interrupt a life behaviour, but should not erase the underlying hunger, injury, or schedule state.

## 3. Life needs and condition model

Use a small bounded set of normalized values. Each value stays in the range 0–1 unless a species definition explicitly uses another unit.

| Variable | Meaning | Typical influences |
|---|---|---|
| Energy | Available exertion capacity | Swimming, hunting, fleeing, resting |
| Hunger | Need to obtain food | Time since feeding, metabolism, feeding |
| Health | Overall condition | Damage, disease if used, healing |
| Injury | Current impairment | Collision, attack, environmental hazard |
| Stress | Immediate disturbance / fear | Noise, predator proximity, injury, player approach |
| Reproductive readiness | Seasonal readiness to court or spawn | Season, maturity, condition, habitat |
| Territorial pressure | Motivation to patrol or defend an area | Intrusion, species profile, season |
| Curiosity | Willingness to inspect uncertain events | Species and individual personality |

Do not expose all of these meters to the player. They are simulation variables used to create observable behaviour.

Simple update model:

`H(t+Δt) = clamp(H(t) + hungerRate × Δt - feedingReduction, 0, 1)`

`E(t+Δt) = clamp(E(t) - activityCost × Δt + restRecovery × Δt, 0, 1)`

`HealthRecovery = baseRate × foodFactor × restFactor × habitatFactor × safetyFactor`

Each factor is clamped to [0,1] and tuned per species. Healing should be slow or disabled while the creature is actively fighting, fleeing, or exposed to a dangerous habitat. Feeding does not instantly restore all health; it primarily reduces hunger and may support later recovery.

When a need reaches a threshold, it contributes to action utility. It must not force an impossible action: a severely injured animal may shelter instead of hunting even when hungry.

## 4. Daily activity and active periods

Species use authored activity profiles rather than a universal day/night schedule.

| Activity profile | Primary active period | Possible behaviours |
|---|---|---|
| Diurnal | Daylight | Feed, travel, court, patrol |
| Nocturnal | Night | Hunt, forage, migrate, call |
| Crepuscular | Dawn and dusk | Short feeding or migration windows |
| Cathemeral | Irregular periods | Several short active/rest cycles |
| Tide-linked | Selected tide phases | Enter channels, feed on exposed prey, spawn |
| Weather-linked | Calm or storm windows | Shelter, forage, surface, or become unusually active |
| Deepwater cycle | Depth / pressure / authored cycle | Rise, descend, feed, rest |

Use world time, season, tide, weather, depth, and habitat to calculate an activity suitability score. For example, a nocturnal creature may become more likely to hunt after dusk, but a storm or territorial disturbance can override its normal schedule.

Never make all creatures active at night simply because the game is horror. A quiet sea populated by resting, migrating, or feeding wildlife can be more unnerving than constant aggression.

## 5. Wandering, patrol, and habitat use

Wandering should be purposeful rather than random point-to-point movement.

Each species definition should include:

- Habitat regions and permitted depth bands.
- Home range or migration corridor.
- Preferred temperature / current / light conditions if gameplay uses them.
- Rest sites, feeding grounds, breeding or nursery grounds where applicable.
- Avoidance zones and dangerous environmental conditions.
- Movement speed, turn limits, and preferred travel depth.
- A small set of authored route anchors or habitat waypoints.

Movement options:

- **Wander:** choose a nearby valid point inside a home range, weighted toward suitable habitat.
- **Patrol:** follow a route around a territory, nest, feeding ground, or den.
- **Migrate:** follow an authored seasonal or tide-linked corridor between regions.
- **Shelter:** move to a cave, reef, wreck, trench wall, or other species-approved refuge.
- **Drift:** reduce active movement and let currents carry a passive species within safe bounds.
- **Return home:** resume a saved habitat route after disturbance or feeding.

Use bounded waypoint selection, steering, and current influence. Do not use unbounded random walks that strand creatures on shore or send them into invalid depth bands. A creature's home range should be persistent enough that the player can learn where to find it.

## 6. Hunting behaviour

Hunting is a food-acquisition activity, not a synonym for attacking the player.

### 6.1 Hunting stages

`Forage → DetectPrey → SelectTarget → Approach → Stalk / Ambush → Strike → CaptureCheck → Feed → Digest / Rest`

The stages may vary by species. A filter feeder or scavenger does not need the full predator pipeline.

1. **Forage:** move through a suitable feeding area while spending energy.
2. **Detect prey:** use the creature's authored senses and prey-detection range.
3. **Select target:** rank available prey by suitability, distance, risk, energy return, and capture probability.
4. **Approach:** move without immediately exposing the predator.
5. **Stalk / ambush:** use cover, depth, current, darkness, or a route bottleneck when the species is designed for it.
6. **Strike:** spend energy and resolve a bounded attack attempt.
7. **Capture check:** success depends on prey alertness, distance, speed, habitat, and predator condition.
8. **Feed:** reduce hunger and update local food availability.
9. **Digest / rest:** lower activity and allow recovery when safe.

### 6.2 Prey selection

Example utility score:

`U(prey) = energyValue × suitability × captureChance - travelCost - injuryRisk - competitionCost`

The predator selects the highest legal option only when the score is positive and hunger / ecological conditions justify hunting. Add a species-specific preference for prey type, size, depth, and region.

Avoid perfect predator aim. Prey can flee, hide, school, change depth, or exploit cover. A failed hunt consumes time and energy. Repeated failed hunts should cause a predator to abandon the target or change feeding ground rather than chase indefinitely.

### 6.3 Hunting archetypes

| Archetype | Behaviour | Counter-pattern / ecological clue |
|---|---|---|
| Ambush predator | Remains still near cover; strikes when prey enters a lane | Small fish avoid a particular ledge |
| Pursuit predator | Tracks and exhausts prey in short bursts | Schools scatter when it approaches |
| Pack / cooperative hunter | Members pressure prey from different directions | Prey moves toward gaps in the formation |
| Benthic forager | Searches seabed / crevices for food | Disturbed sediment and small feeding pits |
| Scavenger | Visits carcasses, discarded catch, or organic remains | Other scavengers arrive later |
| Filter / plankton feeder | Moves through productive water | Dense schools or feeding surface activity |
| Opportunistic omnivore | Uses several food sources based on availability | Flexible route and diet patterns |

Predators should interact with fish populations and prey behaviours through the ecology model. A hunting event can remove a bounded amount of prey stock or a single local school unit; it must not silently collapse a whole species population.

## 7. Eating, feeding, and digestion

Feeding should be visible through short, readable animations and environmental evidence, not a graphic simulation.

Possible observable actions:

- Snapping at a fish school.
- Swallowing small prey near the surface.
- Picking through the seabed.
- Feeding around a wreck or carcass.
- Filtering through a dense plankton zone.
- Returning to a den or refuge after feeding.
- Defending a feeding site while consuming prey.

On a successful feed:

- Reduce hunger by a species-authored amount.
- Increase energy only if the species profile permits immediate benefit; otherwise energy recovery occurs during digestion / rest.
- Apply a local prey-stock change.
- Emit a feeding event for scavengers or other creatures if appropriate.
- Start a digestion / feeding cooldown to prevent repeated instant consumption.
- Record the time and feeding site for ecology and debugging.

The player should be able to observe a food chain: small fish aggregate, a predator appears, the school disperses, and scavengers may visit later. Do not make every feeding event a guaranteed encounter.

## 8. Resting and sleeping

Rest is a real behavioural state, not merely an animation played when no target exists.

Rest types:

- **Sleep:** a species-specific low-awareness rest period.
- **Light rest:** reduced movement while retaining strong environmental awareness.
- **Shelter:** remain inside a cave, under a ledge, within a wreck, or among reef structures.
- **Drift-rest:** low-energy movement with current.
- **Post-hunt recovery:** recover after an expensive pursuit or feeding action.

Rest site selection should consider habitat suitability, exposure, depth, current strength, and threat level. A sleeping creature may wake when it hears a strong sound, receives physical disturbance, or detects a close threat. Species differ in wake sensitivity.

Do not give the player a free attack window against every resting creature. Some may remain protected by cover, wake quickly, or flee. A rest site can become a memorable discovery without turning every sleeping animal into loot.

Resting reduces energy expenditure and can support healing. It does not instantly refill energy or health.

## 9. Healing, injury, and recovery

Creatures can receive injuries from failed hunts, collisions, territorial disputes, hazards, or player-caused disturbance where that interaction is approved.

Injury effects should be behavioural and readable:

| Condition | Possible effect |
|---|---|
| Minor injury | Shorter pursuit burst; longer rest |
| Moderate injury | Lower turn rate / speed; avoids contested feeding grounds |
| Severe injury | Shelters, abandons hunts, or migrates to a safer area |
| Recovering | Reduced activity; avoids unnecessary encounters |

Healing factors can include rest quality, food condition, species resilience, safe habitat, and time. A safe healing site may be part of a creature's territory. Recovery should be bounded and gradual; do not make a creature reset to full health instantly after leaving the player's camera.

Keep creature health separate from player boat hull integrity and the threat attack damage system. Wildlife must not arbitrarily damage player equipment simply because it is healing.

## 10. Breeding, courtship, and population renewal

Reproduction is selectively simulated. It is not required for every entity, and the game does not need graphic or explicit reproductive animation.

Possible behaviours:

- Seasonal migration to a breeding ground.
- Courtship display, sound, or bioluminescent signalling.
- Pairing or group aggregation.
- Nest / nursery site selection for species that use one.
- Spawning or egg-release event abstracted into a population cohort.
- Guarding a breeding site.
- Juvenile schooling or sheltering in a nursery area.

A breeding event should require all hard gates:

- Species can reproduce in the current design.
- Correct seasonal or tide window.
- Suitable breeding habitat.
- Sufficient population and adult condition.
- Reproductive readiness and adequate energy.
- No overriding disturbance, severe injury, or catastrophic habitat condition.
- Region population cap has not been exceeded.

Use event windows and stable opportunity IDs rather than rolling an independent breeding chance every frame. A missed window should wait for the next valid cycle. A player may observe courtship or nursery activity, but required story progression must never depend on harming a breeding population.

### 10.1 Population abstraction

Do not spawn hundreds of persistent juvenile GameObjects. Represent reproduction as aggregate cohorts:

`N(next) = clamp(N + births - naturalLoss - predationLoss - playerCatch - migrationOut + migrationIn, 0, K)`

Where `K` is the region/species carrying-capacity limit. The population model can track adult, juvenile, and breeding-ready stock as aggregate counts or ratios. Promote cohorts on scheduled boundaries, not every frame.

### 10.2 Player impact

Heavy fishing, repeated disturbance, and destroyed habitat may reduce breeding success or move a population elsewhere. Communicate effects through fewer schools, changed fishmonger supply, altered sightings, or warning dialogue. Do not make a single ordinary fishing trip permanently wipe out a species.

## 11. Unnatural and anomalous behaviour

Unnatural behaviour belongs to a separate authored anomaly layer. It deliberately violates the expected routine of a creature, but it must have repeatable rules, detectable evidence, and limits. This is how the game can feel wrong without making the AI appear buggy.

### 11.1 Anomaly families

| Family | Apparent violation | Example |
|---|---|---|
| Temporal repetition | Behaviour repeats with impossible precision | A creature circles the same rock at the same second each tide |
| Acoustic mimicry | Call resembles a known source but has an impossible pattern | Whale-like call repeats the player's Rig cadence |
| Spatial contradiction | Distance, bearing, or depth readings disagree | Sounding returns suggest the creature is both nearer and deeper |
| Biological refusal | Creature ignores ordinary survival incentives | An injured animal refuses food and remains in a barren zone |
| Predatory distortion | Normal prey-predator relationship reverses or stops | Fish calmly orbit a predator that should hunt them |
| Impossible migration | The creature follows a route that contradicts currents or known habitat | A deepwater silhouette crosses a shallow reef with no valid passage |
| Repeated identity | Similar calls or appearances occur at separate sites | Two distant creatures share the same unique acoustic signature |
| Environmental synchrony | Multiple species move in exact unison without ordinary cues | A whole school turns on the same beat as the lighthouse bell |

These are fictional design patterns, not claims about real animal behaviour. Use a small number of memorable anomalies rather than making the entire ecosystem supernatural.

### 11.2 Anomaly contract

Every anomalous creature profile must define:

1. **Baseline:** what ordinary behaviour would normally be expected.
2. **Violation:** exactly which rule is broken.
3. **Trigger:** tide, location, signal, quest flag, player action, or authored story event.
4. **Domain:** the boundary where the anomaly can occur.
5. **Repetition rule:** how often and under which stable opportunity ID it repeats.
6. **Evidence:** what the player can observe through sound, light, fish movement, instruments, or environmental traces.
7. **Constraint:** what the anomaly cannot do.
8. **Player interpretation:** what pattern can be learned without an exposition dump.
9. **Resolution:** how the event ends, changes, or persists.
10. **Safety rule:** how to prevent an unavoidable attack, quest softlock, or save-state inconsistency.

An anomaly may ignore hunger, sleep, injury, migration, or normal acoustic rules, but only the explicitly listed variables may be overridden. All other behaviours remain governed by the ordinary system. This preserves a contrast between natural and unnatural.

### 11.3 Unnatural does not mean random

Do not use random teleportation, arbitrary speed boosts, invisible damage, or untelegraphed attacks as shorthand for supernatural intelligence. If a creature appears to move impossibly, make that impossibility itself part of a consistent clue pattern. Give the player a way to test the pattern with the Sounding Rig, tide timing, landmarks, or environmental observation.

## 12. Disturbance and player influence

Player actions may alter a creature's routine:

- Repeated Rig pulses may attract curious or sound-sensitive species.
- Engine noise may cause prey to scatter or predators to investigate.
- Repeated fishing may reduce local prey stock and move predators to another feeding ground.
- Bright lights may attract, repel, or disorient only species with that authored response.
- Approaching a nest / nursery may cause warning, guarding, or withdrawal.
- Injuring a creature may change its preferred habitat and future risk tolerance.
- Returning to a disturbed region may reveal altered routes, fewer schools, or changed calls.

Use disturbance by region and species. Do not create a single global aggression meter that makes every animal hostile because the player caught fish.

## 13. Data model and Unity/C# boundaries

Recommended authoring assets:

- `CreatureDefinition`: stable ID, species classification, habitat, movement, senses, and feature flags.
- `CreatureBehaviourProfile`: life schedule, allowed behaviours, personality weights, rest / feeding tendencies.
- `DietDefinition`: prey categories, preferred feeding grounds, food value, capture constraints.
- `HabitatDefinition`: region, depth, current, temperature, cover, safe refuge, and capacity.
- `LifeCycleDefinition`: maturity, activity periods, seasonal windows, breeding gates, lifespan abstraction if required.
- `AnomalyDefinition`: baseline, violated rule, trigger, domain, repetition, evidence, constraints, and resolution.

Recommended runtime services:

- `CreatureLifeController`: long-term schedule and life-state transitions.
- `CreatureNeeds`: hunger, energy, health, stress, and reproductive readiness.
- `CreatureBehaviourSelector`: legal actions and utility selection.
- `CreaturePerception`: prey, threat, sound, light, and habitat sensing.
- `CreatureMemory`: food sites, routes, threats, and prior disturbances.
- `CreatureMotor`: movement and habitat constraints.
- `CreatureFeedingController`: hunt result, consumption, local stock change.
- `CreatureRecoveryController`: rest and healing rules.
- `ReproductionScheduler`: scheduled breeding windows and cohort updates.
- `AnomalyController`: authored violations of ordinary rules.
- `RegionalEcologyService`: population, migration, food availability, and off-screen aggregation.

ScriptableObjects hold authored definitions only. Runtime needs, health, route progress, and anomaly phase belong to per-instance state or regional simulation records. Do not mutate shared ScriptableObject assets during play.

## 14. Simulation and performance policy

Simulate only the detail that can affect the current experience:

- Nearby and important creatures: active behaviours and local movement.
- Distant creatures in an active region: simplified schedules and coarse movement updates.
- Inactive regions: aggregate hunger / energy effects only where they change population or future events; update migration, breeding cohorts, and stock at scheduled boundaries.
- Cosmetic background schools: presentation-only unless explicitly promoted to gameplay entities.
- One-off anomalies: stable event records, not permanent high-frequency controllers.

Never run a full local predator hunt in a region that is inactive. Resolve only explicit off-screen outcomes. Do not let an off-screen simulation silently kill a named creature, remove quest evidence, or permanently destroy a breeding population.

## 15. Acceptance tests

### Ordinary life behaviour
- A nocturnal species is more likely to forage at its authored night window than at midday.
- A tide-linked species changes route only at an eligible tide boundary.
- A creature can wander, patrol, shelter, and return to its home range without entering invalid terrain.
- A predator can hunt fish without automatically targeting the player.
- A failed hunt consumes time / energy and can end in abandonment.
- A successful feed reduces hunger and updates local prey stock exactly once.
- Rest reduces energy expenditure; healing is gradual and condition-dependent.
- Injured creatures select safer actions when their condition is poor.
- Breeding occurs only during a valid scheduled window and respects habitat capacity.
- Population cohorts remain bounded and reproducible after save / load.

### Unnatural behaviour
- An anomaly overrides only its explicitly authored biological rules.
- The anomaly has a stable trigger and repetition contract.
- The player can observe at least one clue that distinguishes it from ordinary behaviour.
- The anomaly cannot bypass safe-zone, save integrity, or quest progression safeguards unless the quest explicitly defines an authored outcome.
- The same seed and opportunity ID reproduce the same anomaly selection.

### Performance and integrity
- Inactive regions use coarse population and schedule updates.
- Cosmetic schools do not consume gameplay RNG.
- No creature remains awake and fully simulated solely because an inactive-region reference is alive.
- Missing or invalid behaviour data falls back to a safe passive profile and logs a diagnostic.
- Reproduction and feeding events apply once, never twice after save / load.

## 16. Implementation order

1. Define creature categories, habitat constraints, and behaviour profiles.
2. Implement life states and daily activity windows for one non-hostile species.
3. Add wander, patrol, shelter, and return-home movement.
4. Add a single predator-prey hunting interaction with one prey type.
5. Add feeding, hunger, energy, and local population updates.
6. Add rest and gradual injury recovery.
7. Add seasonal breeding as a regional cohort event.
8. Add The Sleeper's hostile threat logic as a separate layer.
9. Prototype one unnatural behaviour with a stable clue and trigger.
10. Add inactive-region aggregation, save/load, and regression tests.

Do not implement all species, reproduction stages, or anomaly families before the first ordinary life cycle works. Start with one fish, one small predator, and one large anomalous creature.

## 17. Related project specifications

- [Creature AI and Threat Behaviour](CREATURE-AI.md) — threat-specific perception, pursuit, attacks, and fairness.
- [AI Coordination and Hunting Logic](AI-COORDINATION-AND-HUNTING-LOGIC.md) — intelligent stalking, coordinated hunting, tactical roles, and target belief.
- [World Simulation and Event Scheduler](WORLD-SIMULATION.md) — world time, schedules, deterministic events, and inactive-region simulation.
- [Fish Population Model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md) — regional stock, migration, catch, and recovery.
- [Unity AI Implementation Blueprint](../08_TECHNICAL_DESIGN/AI-IMPLEMENTATION-BLUEPRINT.md) — Unity/C# component boundaries and implementation sequence.

The life-cycle model governs ecological behaviour. The threat model governs player-directed hostile encounters. The anomaly model defines deliberate exceptions. Keep those responsibilities separate.

## 18. Daily body growth and cohort persistence

The world advances animal age and size on completed in-game days, including while the player's scene is unloaded. A fish that survives for several days can become longer and heavier; a juvenile cohort can pass a maturity threshold; a crustacean can approach or complete a molt. This must be represented without creating a GameObject for every animal.

The population service owns aggregate abundance and cohort growth. The creature life controller owns the detailed behaviour of nearby visible individuals. They share species definitions but must not independently create duplicate births, growth, or mortality.

- Fish cohorts use species-specific length / weight growth parameters and maturity gates.
- Crabs and lobsters grow through authored molt stages; post-molt animals may temporarily shelter.
- Squid, cuttlefish, shrimp, bivalves, echinoderms, and sea cucumbers use their own growth profile rather than a universal fish curve.
- Named persistent individuals may have their own age, size, injury, and route state.
- Anomalous beings do not inherit biological growth by default; their dossier must explicitly define it.

Growth increases the cohort's mean weight and therefore biomass. It does not create additional individuals. Reproduction adds delayed juvenile recruitment; mortality and fishing reduce abundance. The two processes must never be conflated.

The detailed formulas and species-profile fields are in [Fish Population and Food-Web Model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md). Unity services, save state, daily scheduling, and tests are defined in [Regional Population Simulation](../08_TECHNICAL_DESIGN/REGIONAL-POPULATION-SIMULATION.md).
