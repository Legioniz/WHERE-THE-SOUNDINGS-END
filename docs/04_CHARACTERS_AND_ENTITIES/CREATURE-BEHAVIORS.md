# Creature Behavior Source Map

> Project: WHERE THE SOUNDINGS END  
> Status: Working reference — linked specifications are authoritative  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

This file is an index and design guardrail. It intentionally does not duplicate the complete AI, ecology, or life-cycle specifications.

## 1. Authoritative behavior layers

Creature behavior is divided into three coordinated layers:

1. **Life behavior:** sleeping, resting, active periods, feeding, hunting for food, digestion, healing, breeding, brooding, migration, sheltering, and inactivity.
2. **Tactical behavior:** wandering, patrol, investigation, prey approach, stalking, ambush, attack, fleeing, guarding, searching, and return-to-habitat.
3. **Encounter / anomaly behavior:** curiosity, warning, player-directed stalking, pursuit, attack, disengagement, mimicking, looping, manifesting, and withdrawal.

See [Creature Life Cycles and Ecological Behaviour](../12_AI_AND_SIMULATION/CREATURE-LIFE-CYCLES-AND-ECOLOGY.md) for the life-state model, species schedules, ecological simulation, feeding, breeding, rest, healing, and disturbance.

## 2. Habitat and wildlife assignment

The place where a creature lives must explain its food, cover, movement, resting sites, and sensory strategy. See [Regional Ecology and Creature Distribution](../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md) for the six regional ecosystems and the named sublocation fauna.

## 3. Threat and anomaly behavior

An ordinary predator hunting fish is not automatically hunting the player. Natural wildlife should normally avoid, ignore, or investigate the boat according to species-specific rules. Hostility requires a clear ecological or supernatural trigger.

See [Anomalous Species and Site-Specific Entities](ANOMALOUS-SPECIES.md) for the proposed entity roster, triggers, telegraphs, limits, and counterplay.

## 4. Runtime implementation

The design documents above describe behavior; they are not compiled Unity runtime code. The Unity implementation boundary, update cadence, save state, perception, and testing requirements are documented in:
- [AI Implementation Blueprint](../08_TECHNICAL_DESIGN/AI-IMPLEMENTATION-BLUEPRINT.md)
- [Threat Director](../12_AI_AND_SIMULATION/THREAT-DIRECTOR.md)
- [AI Coordination and Hunting Logic](../12_AI_AND_SIMULATION/AI-COORDINATION-AND-HUNTING-LOGIC.md)
- [World Simulation](../12_AI_AND_SIMULATION/WORLD-SIMULATION.md)

No runtime implementation is implied by the presence of these specifications.
