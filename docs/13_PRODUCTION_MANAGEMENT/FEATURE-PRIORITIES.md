# Feature Priorities

> Project: WHERE THE SOUNDINGS END  
> Status: Working priority baseline  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-11

Priorities are relative to the commercial roadmap. A priority does not imply that the feature is unimplemented; inspect the repository and owning documents before creating work.

## P0 — Foundation, integrity, and vertical-slice blockers

P0 work blocks reliable progress or the first complete playable slice.

1. Audit the current repository and classify real implementation status.
2. Preserve the Unity 6.3 LTS project baseline, assembly boundaries, generated-scene workflow, and existing code conventions.
3. Close critical boat, camera, action-driven time, scene bootstrap, and interaction integration defects.
4. Complete and verify the first NPC foundation increment using the existing `NpcAgent`, `NpcScheduleResolver`, Yarn dialogue, interaction, and save contracts. Immediate focus: schedule/interaction correctness, stable IDs, dialogue integration, relationship persistence, tests, and clearly scoped next steps for pathfinding and quest state.
5. Verify save/load/autosave and backup recovery; preserve migration compatibility before adding new persisted systems.
6. Establish the first complete expedition loop and the acceptance criteria for the vertical slice.
7. Maintain repository static checks, C# syntax checks, Edit Mode tests, and Unity verification instructions.

**Do not** replace working systems merely to make them look more abstract or generalized. Make the smallest coherent change that closes an evidenced gap.

## P1 — Vertical-slice player experience

1. Preparation, gear, departure, navigation, and safe return.
2. Fishing/dredging/survey actions and spatial cargo handling.
3. Selling/trading, regional pricing where approved, repairs, fuel, recovery, and upgrade progression.
4. Player-facing inventory, market, save/load, interaction, and expedition UI.
5. One playable region with coherent ecology, landmarks, one quest/NPC arc, one meaningful threat, audio, and atmosphere.
6. Clear warnings, player counterplay, readable failure/recovery, and accessibility support for critical cues.

P1 is complete only when the complete slice can be played and reviewed; isolated scripts are not enough.

## P2 — System depth and world expansion

1. Living Current/fish availability and ecological response.
2. Threat perception, encounter scheduling, creature behaviour, rare legendary sightings, and authored horror incidents.
3. NPC routines, narrative memory, relationships, quests, and world-state reactivity.
4. Six-region travel and region-specific environmental systems.
5. Fish, seafood, wildlife, anomalous species, salvage, locations, side quests, and major threats within GDD planning targets.
6. Story acts, endings, and long-term progression.

Scale the shared data-driven systems before multiplying content.

## P3 — Commercial polish and release

1. Performance profiling and stability on supported Windows hardware.
2. Accessibility: captions/text alternatives for critical audio cues, mono compatibility, dynamic-range controls, reduced-motion/flash options where applicable.
3. UI/UX consistency, input remapping/controller support, settings, onboarding, save UX, and error recovery.
4. Asset/audio optimization, final lighting, animation, VFX, mix, and content polish.
5. Full-game regression, external playtesting, clean install/build verification, licensing/credits, store media, support and release preparation.
6. Post-launch issue triage and a controlled patch process.

## Priority rules

- Fix data loss, broken progression, hard crashes, and save corruption before adding content.
- Fix vertical-slice blockers before adding more regions or catalog entries.
- Prefer systems that serve multiple authored creatures/NPCs/locations over one-off scripts.
- Avoid feature creep that does not support the fishing, navigation, ecology, maritime horror, or narrative pillars.
- Keep research-based ecological claims sourced and distinguish real-world evidence from fictional anomalies.
- Defer multiplayer, modding, consoles, and extra release platforms unless formally approved.
