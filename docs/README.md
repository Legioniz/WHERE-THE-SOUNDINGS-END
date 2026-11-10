# Documentation Index

| Folder | Purpose |
|---|---|
| 01_GAME_DESIGN | Vision, pillars, experience, core loop, scope |
| 02_LORE_AND_WORLD | Canonical setting facts, history, myths, maritime research |
| 03_STORY_AND_NARRATIVE | Acts, endings, quests, dialogue, side-quest drafts |
| 04_CHARACTERS_AND_ENTITIES | Protagonist, NPCs, fish, creatures |
| 05_WORLD_AND_LEVEL_DESIGN | Regions, locations, exploration, ecology |
| 06_GAMEPLAY_SYSTEMS | Player-facing mechanics |
| 07_GAME_RULES_AND_BALANCING | Probability, ecology, economy, tuning |
| 08_TECHNICAL_DESIGN | Unity/C# architecture, data, scenes, saves |
| 09_ART_AND_VISUAL_DIRECTION | Visual design and asset rules |
| 10_AUDIO_AND_SOUND_DESIGN | Audio language and acoustic gameplay |
| 11_UI_UX_DESIGN | HUD, interfaces, accessibility |
| 12_AI_AND_SIMULATION | Threats, NPCs, fish, world simulation |
| 13_PRODUCTION_MANAGEMENT | Roadmap, tasks, risks, release |
| 14_TESTING_AND_QUALITY | Test plans and quality gates |
| 15_COMMERCIAL_AND_MARKETING | Audience, positioning, release materials |
| 16_REFERENCES_AND_RESEARCH | Research sources |
| 17_ARCHIVE | Deprecated designs and experiments |

## World geography and regional ecology

- [World Bible](02_LORE_AND_WORLD/WORLD-BIBLE.md)
- [Regional ecology and creature distribution](05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md)
- [Location catalog](05_WORLD_AND_LEVEL_DESIGN/LOCATION-CATALOG.md)
- [Exploration design](05_WORLD_AND_LEVEL_DESIGN/EXPLORATION-DESIGN.md)

## Creature roster, fishing, and trade

- [Catchable seafood and market species catalog](04_CHARACTERS_AND_ENTITIES/CATCHABLE-SEAFOOD-CATALOG.md)
- [Fish species index](04_CHARACTERS_AND_ENTITIES/FISH-SPECIES.md)
- [Mutated and unnatural catchable specimens](04_CHARACTERS_AND_ENTITIES/MUTATED-AND-UNNATURAL-CATCHES.md)
- [Creature Bible](04_CHARACTERS_AND_ENTITIES/CREATURE-BIBLE.md)
- [Creature behaviour source map](04_CHARACTERS_AND_ENTITIES/CREATURE-BEHAVIORS.md)
- [Anomalous species and site-specific entities](04_CHARACTERS_AND_ENTITIES/ANOMALOUS-SPECIES.md)
- [Creature life cycles and ecological behaviour](12_AI_AND_SIMULATION/CREATURE-LIFE-CYCLES-AND-ECOLOGY.md)
- [Fish population model](07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md)
- [Seafood selling price schedule](07_GAME_RULES_AND_BALANCING/SEAFOOD-SELLING-PRICE-SCHEDULE.md)
- [Economy mathematics](07_GAME_RULES_AND_BALANCING/ECONOMY-MATHEMATICS.md)
- [AI coordination and hunting logic](12_AI_AND_SIMULATION/AI-COORDINATION-AND-HUNTING-LOGIC.md)
- [Threat Director and encounter scheduling](12_AI_AND_SIMULATION/THREAT-DIRECTOR.md)
- [World event scheduler](12_AI_AND_SIMULATION/WORLD-SIMULATION.md)
- [Entity perception and encounter mathematics](07_GAME_RULES_AND_BALANCING/ENTITY-PERCEPTION-AND-ENCOUNTER-MATH.md)
- [Threat audio logic](10_AUDIO_AND_SOUND_DESIGN/THREAT-AUDIO-LOGIC.md)
- [Unity implementation blueprint](08_TECHNICAL_DESIGN/AI-IMPLEMENTATION-BLUEPRINT.md)
- [Camera FOV and player awareness](08_TECHNICAL_DESIGN/CAMERA-AND-PLAYER-AWARENESS.md)

## Narrative and maritime lore

- [Maritime myths and motifs catalogue](02_LORE_AND_WORLD/MARITIME-MYTHS-CATALOG.md)
- [Maritime research notes and source-quality cautions](02_LORE_AND_WORLD/MARITIME-LORE-RESEARCH-NOTES.md)
- [Maritime side-quest collection](03_STORY_AND_NARRATIVE/SIDE_QUESTS/MARITIME_MYSTERIES/README.md)

The existing docs/technical folder is retained because it contains the established Unity/C# performance and programming-language guidance.

Status labels: Approved baseline, Working baseline, Proposed, Placeholder, Deprecated. A placeholder is not an implementation claim. Research-derived lore drafts are non-canon until reviewed and approved. Every mechanic needs a gameplay specification; every implemented system needs a technical specification and tests. See DOCUMENTATION-STANDARD.md.

## Persistent population and daily creature growth

- [Fish population and food-web model](07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md)
- [Regional population simulation — Unity architecture](08_TECHNICAL_DESIGN/REGIONAL-POPULATION-SIMULATION.md)

Population rules are authored per species and fishing ground. The simulation tracks cohort growth, breeding stock, daily recruitment, fishing mortality, migration, and recovery. These are specifications; Unity runtime implementation remains separate.

- [Ocean legends and sea monsters — expanded research](02_LORE_AND_WORLD/OCEAN-LEGENDS-AND-SEA-MONSTERS.md)

## Rare legendary encounters

- [Ocean legends and sea monsters — research catalogue](02_LORE_AND_WORLD/OCEAN-LEGENDS-AND-SEA-MONSTERS.md)
- [Legendary maritime encounter scheduler](12_AI_AND_SIMULATION/LEGENDARY-ENCOUNTER-SCHEDULER.md)

Legend-inspired encounters are rare, location-bound events evaluated by the Threat Director. They may be harmless sightings, environmental omens, dangerous contacts, or lethal hazards with authored warnings and possible escape routes.
