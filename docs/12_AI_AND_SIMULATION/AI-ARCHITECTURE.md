# AI Architecture

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

Use finite-state machines for discrete modes, behavior trees for hierarchical tasks, and utility scoring where several eligible actions need ranking. The threat director chooses encounter opportunities; each creature owns local perception and behavior. Throttle sensing/decision updates, use spatial queries, and profile before parallelizing. Avoid expensive whole-world AI while regions are inactive.


## Authoritative implementation specifications

- [Creature AI and threat behaviour](CREATURE-AI.md) defines the state machine, visual / acoustic sensing, memory, chase, telegraph, attacks, and counterplay.
- [Threat Director](THREAT-DIRECTOR.md) owns event opportunities, RNG, eligibility, cooldowns, tension, and safe spawn validation.
- [World Simulation](WORLD-SIMULATION.md) owns deterministic event scheduling and save/load behavior.
- [Environmental Reactions](ENVIRONMENTAL-REACTIONS.md) owns engine, wake, Rig, collision, and ecological responses.
- [Entity Perception and Encounter Math](../07_GAME_RULES_AND_BALANCING/ENTITY-PERCEPTION-AND-ENCOUNTER-MATH.md) owns prototype parameters and formulas.
- [Unity Implementation Blueprint](../08_TECHNICAL_DESIGN/AI-IMPLEMENTATION-BLUEPRINT.md) maps the design to components and data assets.

The only hostile entity currently approved for a full chase/attack prototype is The Sleeper. Other maritime mystery quests intentionally use non-hostile anomalies, wildlife, NPC rescue, or environmental hazards.
