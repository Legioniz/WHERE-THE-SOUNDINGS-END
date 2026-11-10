# Greyhaven NPC System

> Project: WHERE THE SOUNDINGS END  
> Status: First runtime foundation and four Greyhaven residents integrated into the generated blockout  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Design goals

Greyhaven should read as a working fishing community before it reads as a horror setting. Residents have stable identities, occupations, daily activity windows, locations, dialogue nodes and persistent relationship values. The same runtime contract should support future regional residents without hard-coding each character into the scene builder.

NPCs are not ambient monsters. Their routines and testimony are grounded in work, family, local knowledge, economic pressure and ordinary explanations for unusual events. The supernatural remains uncertain until the player gathers evidence.

## Runtime architecture

### NpcAgent

Source: `Assets/_Project/Scripts/Runtime/NPC/NpcAgent.cs`.

Each agent owns:
- A stable content ID, display name, occupation ID and region ID.
- An ordered list of daily schedule entries.
- The current activity ID and an authored destination per schedule entry.
- A 0–100 relationship score with explicit modification methods.
- Save/load participation through the existing `ProgressSaveData.npcStates` collection.

NPC IDs are content identifiers, not GameObject names. Do not rename a stable ID after a campaign has recorded it.

### NpcScheduleEntry and NpcScheduleResolver

A schedule entry defines a start hour, end hour, activity ID and destination Transform. The resolver uses a start-inclusive / end-exclusive interval. It supports overnight windows, for example 18:00–05:00.

Schedule entries are checked in list order. Do not create overlapping windows unless the earlier entry is intentionally higher priority. A missing matching window resolves to idle.

### Time and movement

NPC schedule selection reads `SeaStateDirector.CurrentHour`. NPC movement only runs while the authoritative action-driven sea clock is advancing. Dialogue, inventory and a stopped vessel therefore do not cause village time to pass or residents to move.

Movement currently uses direct waypoint movement between authored Transform anchors. It is a greybox solution, not NavMesh pathfinding. Keep destinations close and routes clear of buildings, cliffs, water and other blockers. A later navigation pass must replace direct movement with validated paths before residents receive longer routes.

### Interaction and dialogue

The existing `HarbourmasterInteraction` trigger is now configurable for any Greyhaven resident: dialogue node and display name are set independently. The player still approaches by boat because the on-foot controller is not implemented. Yarn owns authored lines, choices and conversation variables; C# owns NPC identity, schedule and relationship state.

### Persistence

Each agent implements `ISaveParticipant` and contributes one record to `SaveGameData.progress.npcStates`:
- `npcId`: stable resident identity;
- `stateId`: last known schedule activity;
- `relationshipId`: relationship score serialized as an invariant-culture integer string for compatibility with the existing DTO;
- `lastKnownRegionId`: resident's current region.

This first pass reuses the existing NPC save record and does not change the save schema. Dialogue memories such as first meeting and heard rumours remain in Yarn's existing typed variable storage. A future memory system should add explicit structured fields and a deliberate schema migration rather than packing unrelated data into strings.

## Initial Greyhaven cast

| Resident | Occupation | Daily activity | Dialogue node | Character direction |
|---|---|---|---|---|
| Harbourmaster | Harbour office / quay watch | 05:00–18:00 quay; 18:00–05:00 office | `Greyhaven_Harbourmaster` | Restrained, procedural, protective of the channel; gives warnings without explaining every fear |
| Mara Venn | Fishmonger | 04:00–16:00 market; 16:00–04:00 off duty | `Greyhaven_Fishmonger` | Practical and observant; judges catch condition, remembers unpaid crates, refuses to sensationalize strange fish |
| Tomas Vale | Boat mechanic | 06:00–18:00 repair shift; 18:00–06:00 off duty | `Greyhaven_Mechanic` | Skeptical and mechanically minded; begins with ordinary explanations but records damage that does not fit them |
| Inez Sarmiento | Net mender | 07:00–17:00 net work; 17:00–07:00 off duty | `Greyhaven_NetMender` | Patient, careful with evidence, knows gear and local working habits; avoids declaring a supernatural cause |

Names, dialogue and occupations are working canon for this prototype and may be revised before voice production. The Harbourmaster remains unnamed until the wider story establishes why that identity matters.

## Dialogue memory and relationship design

Current Yarn variables:
- Harbourmaster: first meeting, missing-boat warning, bell question, buoy job and trust.
- Mara: first meeting and trust.
- Tomas: first meeting and trust.
- Inez: first meeting and trust.

The C# relationship score is persistent and exposed through `ModifyRelationship`, but dialogue choices do not yet modify it. Do not imply that trust currently changes prices, quest availability or NPC schedules. Those are future integrations.

Recommended future relationship dimensions:
1. Familiarity — whether the resident recognizes the player and remembers repeated visits.
2. Reliability — whether the player fulfils promises and returns borrowed or requested items.
3. Confidence — whether the resident believes the player's account of an incident.
4. Personal trust — willingness to share private testimony or sensitive information.

Avoid one universal morality meter. Different residents should respond to different actions for understandable reasons.

## Horror and reactivity rules

- NPCs do not randomly become hostile or disappear because of a generic panic roll.
- Ordinary work continues through most ambient horror events.
- Residents may disagree about the same event because they observed different details, have different incentives, or are protecting someone.
- A rumor is not automatically a confirmed fact.
- Required story clues must not depend on random NPC schedule timing; the player needs a reliable way to find critical witnesses.
- The protected inner harbor remains safe from full hostile chase encounters.
- The game should show routine disruption gradually: a closed stall, an uncollected crate, a boat that remains tied up, a missing shift, or a changed radio message.

## Current implementation boundary

Implemented:
- Reusable NPC schedule data and resolver.
- Overnight schedule windows.
- Authoritative sea-clock gating.
- Waypoint movement between authored transforms.
- Persistent resident activity and relationship score.
- Four Greyhaven blockout residents generated by the Editor scene builder.
- Separate Yarn dialogue nodes for the fishmonger, mechanic and net mender.
- Boat-proximity interaction prompts configured per resident.
- Edit Mode tests for schedule boundaries and NPC save/restore.

Not yet implemented:
- On-foot player controller and walking interaction.
- NavMesh or obstacle-aware pathfinding.
- Character animation, rigs, facial expression, voice acting and final models.
- Interior access, doors, sleep locations and full home simulation.
- Shop transactions, repair services, net crafting or sale-price integration.
- Dialogue-driven relationship score changes.
- Structured NPC memory beyond Yarn variables.
- NPC reactions to weather, missing vessels, world flags, quest stages and anomalies.
- Regional NPC spawning, population simulation or off-screen schedule simulation.

## Acceptance checks

1. Rebuild Greyhaven and confirm four resident agents exist with unique stable IDs.
2. Confirm all four interaction triggers display the correct resident name and start the correct Yarn node.
3. Confirm schedule windows resolve at their exact start and end boundaries, including overnight windows.
4. Confirm residents move only while the sea clock advances and remain stationary during dialogue, inventory and stopped-boat states.
5. Confirm movement remains on the authored clear routes and does not pass through building geometry.
6. Save after changing an NPC relationship score, reload, and confirm the same stable NPC record restores.
7. Confirm dialogue first-meeting variables persist independently of NPC schedule activity.
8. Confirm no NPC interaction can start a second conversation while another dialogue is active.

Unity scene generation, Yarn compilation, runtime movement and save/load behaviour still require verification in Unity 6000.3.28f1. Static repository checks do not establish those runtime results.
