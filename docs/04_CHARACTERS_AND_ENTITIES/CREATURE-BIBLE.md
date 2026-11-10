# Creature Bible

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline — regional roster proposed  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## 1. Creature design principles

Creatures must feel like inhabitants of a living marine ecosystem. Each species needs a habitat, food source, ecological role, active period, shelter or resting behavior, movement envelope, and response to disturbance.

Life behavior and player hostility are separate. Predators primarily hunt prey; herbivores graze; scavengers follow carrion; migratory animals follow food and environmental conditions; brooding animals protect a specific nursery or egg site. Most wildlife should avoid the player or ignore the boat.

Anomalous entities are a separate category. Their unnatural behavior must have a defined rule, trigger, warning, limit, and counterplay. Do not turn every large animal or remote location into a boss encounter.

## 2. Regional signature fauna

The following game-native wildlife names are proposed identities, not real-world species. Their ecological inspirations and habitat assignments are maintained in [Regional Ecology and Creature Distribution](../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md).

| Region | Signature wildlife | Ecological role |
|---|---|---|
| Greyhaven | Mooring Ray | Bottom-feeding visitor following incoming tides |
| The Shallows | Ribbon Grazer | Herbivorous meadow and reef-edge grazer |
| Blackrock Reach | Basalt Crawler | Crevice-dwelling mollusk and carrion feeder |
| The Drowned Parish | Parish Eel | Ambush predator using flooded structures |
| The Outer Shoals | Longwake Manta | Plankton-feeding migratory visitor |
| The Hollow Trench | Trench Drifter | Low-energy deep-water scavenger |

## 3. Anomalous presence roster

Proposed major presences:
- The Mooring Guest — non-combat observer near the outer Greyhaven harbor.
- The Glassback — conditional shallow-water ambush predator.
- The Cliffblind — territorial basalt-cavity ambusher.
- The Bell Eel — acoustic mimic in the Drowned Parish.
- The Countercurrent — migratory stalker across offshore current corridors.
- The Sleeper — principal hostile deep-water predator.

Environmental or colony-scale anomalies:
- The Marrow Choir — acoustic anomaly associated with the Bone Orchard food fall.
- The Borrowed Light — midwater light-pattern anomaly associated with Lantern Drift.

The roster and encounter contracts are detailed in [Anomalous Species and Site-Specific Entities](ANOMALOUS-SPECIES.md). The Sleeper's existing encounter numbers and quest gates remain owned by its established AI and threat specifications.

## 4. Planning target and scope

Planning target remains 6–8 major threat entities, with ordinary wildlife distributed across the six regions and their sublocations. Signature wildlife are not all separate full AI agents: schooling fish, small crustaceans, and distant fauna may use group or population-level simulation.

## 5. Required creature dossier fields

Each new creature record must include:
- Stable entity ID and working name.
- Real-world inspiration clearly separated from fictional invention.
- Primary and secondary habitat, depth band, and range.
- Diet, prey, predators, and ecological function.
- Active period, feeding schedule, rest or dormancy, breeding, and migration behavior.
- Perception and movement model.
- Disturbance response and player relationship.
- For hostiles: escalation, warning cues, attack contract, disengagement, and counterplay.
- Audio/visual tells, accessibility equivalents, save requirements, and test cases.

## 6. Related specifications

- [Regional ecology and creature distribution](../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md)
- [Creature behavior source map](CREATURE-BEHAVIORS.md)
- [Anomalous species](ANOMALOUS-SPECIES.md)
- [Creature life cycles and ecological behaviour](../12_AI_AND_SIMULATION/CREATURE-LIFE-CYCLES-AND-ECOLOGY.md)
- [AI coordination and hunting logic](../12_AI_AND_SIMULATION/AI-COORDINATION-AND-HUNTING-LOGIC.md)
- [Fish population model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md)
