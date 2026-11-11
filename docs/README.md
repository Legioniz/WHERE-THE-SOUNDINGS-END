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
| 13_PRODUCTION_MANAGEMENT | Roadmap, tasks, risks, release, AI workflow |
| 14_TESTING_AND_QUALITY | Test plans and quality gates |
| 15_COMMERCIAL_AND_MARKETING | Audience, positioning, release materials |
| 16_REFERENCES_AND_RESEARCH | Research sources |
| 17_ARCHIVE | Deprecated designs and experiments |

## Production management and AI-agent workflow

- [Development Roadmap](13_PRODUCTION_MANAGEMENT/DEVELOPMENT-ROADMAP.md) — 24-month commercial production baseline, phase windows, weekly capacity, scope controls.
- [Production Plan](13_PRODUCTION_MANAGEMENT/PRODUCTION-PLAN.md) — two-week work cycles, capacity model, workflow, quality gates.
- [Project Milestones](13_PRODUCTION_MANAGEMENT/PROJECT-MILESTONES.md) — evidence-based exit criteria.
- [Implementation Audit](13_PRODUCTION_MANAGEMENT/IMPLEMENTATION-AUDIT.md) — evidence-based M0 source, project and verification audit.
- [Task Backlog](13_PRODUCTION_MANAGEMENT/TASK-BACKLOG.md) — prioritized foundation and first vertical-slice work.
- [Feature Priorities](13_PRODUCTION_MANAGEMENT/FEATURE-PRIORITIES.md) — P0–P3 sequencing and immediate blockers.
- [Development Sprints](13_PRODUCTION_MANAGEMENT/DEVELOPMENT-SPRINTS.md) — sprint planning and reporting process.
- [AI-Assisted Development Workflow](13_PRODUCTION_MANAGEMENT/AI-ASSISTED-DEVELOPMENT.md) — agent responsibilities, work loop, verification, and handoff.
- [Release Plan](13_PRODUCTION_MANAGEMENT/RELEASE-PLAN.md) — commercial release gates and decisions still open.
- [Repository coding-agent instructions](../AGENTS.md).
- [GitHub Copilot instructions](../.github/copilot-instructions.md).

AI agents must read the repository's AGENTS.md before changing code. A roadmap, design specification, generated code snippet or passing static check does not prove a feature is implemented or verified.

## Repository safety and verification

- [Repository Safeguards](14_TESTING_AND_QUALITY/REPOSITORY-SAFEGUARDS.md) — CI policy, secret-pattern checks, code-owner review, pull-request process and required GitHub branch rules.
- [Testing Strategy](14_TESTING_AND_QUALITY/TESTING-STRATEGY.md) — Edit Mode, Play Mode, integration, performance and accessibility coverage.
- [Documentation Standard](DOCUMENTATION-STANDARD.md) — authoritative sources, evidence/status labels and documentation change requirements.

## World geography and regional ecology

- [World Bible](02_LORE_AND_WORLD/WORLD-BIBLE.md)
- [World Map](05_WORLD_AND_LEVEL_DESIGN/WORLD-MAP.md)
- [Greyhaven NPC system and initial resident cast](12_AI_AND_SIMULATION/GREYHAVEN-NPC-SYSTEM.md)
- [Greyhaven environment and first-hub layout](05_WORLD_AND_LEVEL_DESIGN/GREYHAVEN.md)
- [Regional ecology and creature distribution](05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md)
- [Location catalog](05_WORLD_AND_LEVEL_DESIGN/LOCATION-CATALOG.md)
- [Exploration design](05_WORLD_AND_LEVEL_DESIGN/EXPLORATION-DESIGN.md)

## Vehicle handling and world physics

- [Boat Movement and Hydrodynamics](06_GAMEPLAY_SYSTEMS/BOAT-MOVEMENT.md)

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

- [Main Story](03_STORY_AND_NARRATIVE/MAIN-STORY.md) — proposed four-act campaign, the Choir and Orison mystery.
- [Story Structure](03_STORY_AND_NARRATIVE/STORY-STRUCTURE.md) — act escalation, evidence gates and vertical-slice boundary.
- [Act I — Familiar Waters](03_STORY_AND_NARRATIVE/ACT-01.md)
- [Act II — Beyond the Coast](03_STORY_AND_NARRATIVE/ACT-02.md)
- [Act III — The Deep](03_STORY_AND_NARRATIVE/ACT-03.md)
- [Act IV — The Last Sounding](03_STORY_AND_NARRATIVE/ACT-04.md)
- [Endings](03_STORY_AND_NARRATIVE/ENDINGS.md) — The Answer, The Silence and The Last Mark.
- [Quest Design](03_STORY_AND_NARRATIVE/QUEST-DESIGN.md) — proposed main investigation chain and clue redundancy.
- [Story Consequences](03_STORY_AND_NARRATIVE/STORY-CONSEQUENCES.md) — persistent responses and save requirements.
- [Protagonist](04_CHARACTERS_AND_ENTITIES/PROTAGONIST.md) — The Sounder, the Orison incident and character arc.
- [Character Bible](04_CHARACTERS_AND_ENTITIES/CHARACTER-BIBLE.md) — Nell, the Orison Captain and Greyhaven cast.
- [Maritime myths and motifs catalogue](02_LORE_AND_WORLD/MARITIME-MYTHS-CATALOG.md)
- [Maritime research notes and source-quality cautions](02_LORE_AND_WORLD/MARITIME-LORE-RESEARCH-NOTES.md)
- [Ocean legends and sea monsters — expanded research](02_LORE_AND_WORLD/OCEAN-LEGENDS-AND-SEA-MONSTERS.md)
- [Maritime side-quest collection](03_STORY_AND_NARRATIVE/SIDE_QUESTS/MARITIME_MYSTERIES/README.md)

## Deep-ocean phenomena and mariner horror

- [Deep-ocean phenomena and mariner experiences](06_GAMEPLAY_SYSTEMS/DEEP-OCEAN-PHENOMENA-AND-MARINER-EXPERIENCES.md)
- [Sounding Rig algorithms](07_GAME_RULES_AND_BALANCING/SOUNDING-RIG-ALGORITHMS.md)
- [Deep-ocean survey implementation and file analysis](08_TECHNICAL_DESIGN/DEEP-OCEAN-SURVEY-IMPLEMENTATION.md)
- [Maritime anomaly rules and encounter balancing](07_GAME_RULES_AND_BALANCING/MARITIME-ANOMALY-RULES.md)
- [Maritime Horror Director](06_GAMEPLAY_SYSTEMS/MARITIME-HORROR-DIRECTOR.md)
- [Audio Design](10_AUDIO_AND_SOUND_DESIGN/AUDIO-DESIGN-DOCUMENT.md)

The deep-ocean document separates established science, documented maritime practice, reported accounts, folklore, unsupported claims and fictional anomalies. The balancing document governs event eligibility, RNG, cooldowns, warnings, player counterplay, Operational Strain, persistence and accessibility. The Sounding Rig, Greyhaven shallow survey profile, regional profile data contract, acoustic event publisher and survey save fields now exist in Unity runtime code. Only Greyhaven is wired into a generated scene, with one shallow outer-channel wreck marker. The Outer Shoals and Hollow Trench profiles are available in code but still require authored region scenes, real site placement and site-specific physical controllers.

The existing docs/technical folder is retained because it contains the established Unity/C# performance and programming-language guidance.

Status labels: Approved baseline, Working baseline, Proposed, Placeholder, Deprecated. A placeholder is not an implementation claim. Research-derived lore drafts are non-canon until reviewed and approved. Every mechanic needs a gameplay specification; every implemented system needs a technical specification and tests. See DOCUMENTATION-STANDARD.md.

## Persistent population and daily creature growth

- [Fish population and food-web model](07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md)
- [Regional population simulation — Unity architecture](08_TECHNICAL_DESIGN/REGIONAL-POPULATION-SIMULATION.md)

Population rules are authored per species and fishing ground. The simulation tracks cohort growth, breeding stock, daily recruitment, fishing mortality, migration and recovery. These are specifications; Unity runtime implementation remains separate.

## Rare legendary encounters

- [Legendary maritime encounter scheduler](12_AI_AND_SIMULATION/LEGENDARY-ENCOUNTER-SCHEDULER.md)
- [Legendary encounter visual, behaviour and discoverable lore dossiers](12_AI_AND_SIMULATION/LEGENDARY-ENCOUNTER-VISUAL-AND-LORE-DOSSIERS.md)

Legend-inspired encounters are rare, location-bound events. They may be harmless sightings, environmental omens, dangerous contacts or lethal hazards with authored warnings and possible escape routes. They share the existing major-threat lock with The Sleeper.
