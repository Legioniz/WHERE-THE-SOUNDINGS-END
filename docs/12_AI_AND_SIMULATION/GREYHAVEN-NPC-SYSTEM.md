# Greyhaven NPC System

> Project: WHERE THE SOUNDINGS END  
> Status: Relationship-reactive dialogue foundation implemented; market transactions remain future integration  
> Version: 0.2  
> Last reviewed: 2026-11-13

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
- An individual 0–100 trust score, relationship tier, conversation/trade gates and a buy-price multiplier for future market transactions.
- A bounded, persisted per-NPC memory ledger; each named relationship event is applied at most once.
- Save/load participation through the existing `ProgressSaveData.npcStates` collection.

NPC IDs are content identifiers, not GameObject names. Do not rename a stable ID after a campaign has recorded it.

### NpcScheduleEntry and NpcScheduleResolver

A schedule entry defines a start hour, end hour, activity ID and destination Transform. The resolver uses a start-inclusive / end-exclusive interval. It supports overnight windows, for example 18:00–05:00.

Schedule entries are checked in list order. Do not create overlapping windows unless the earlier entry is intentionally higher priority. A missing matching window resolves to idle.

### Time and movement

NPC schedule selection reads `SeaStateDirector.CurrentHour`. NPC movement only runs while the authoritative action-driven sea clock is advancing. Dialogue, inventory and a stopped vessel therefore do not cause village time to pass or residents to move.

Movement currently uses direct waypoint movement between authored Transform anchors. It is a greybox solution, not NavMesh pathfinding. Keep destinations close and routes clear of buildings, cliffs, water and other blockers. A later navigation pass must replace direct movement with validated paths before residents receive longer routes.

### Interaction and dialogue

Generated Greyhaven residents are selected through the persistent `DocksideHubController`; each location definition binds the resident's existing `NpcAgent` and Yarn node. The player approaches by boat and speaks through the menu because the on-foot controller is not implemented. Generated `HarbourmasterInteraction` components are marked externally managed and do not claim the shared Interact action; the component remains available for hand-authored scenes. Yarn owns authored lines and branching; C# owns the authoritative score, event ledger, trust thresholds and relationship variables.

The dialogue UI shows the active resident's name, tier and 0–100 meter. The scene builder registers the typed Yarn command `<<relationship "stable.event.id" delta>>`; it can only modify the active resident. A memory ID is consumed once per resident, and deltas are limited to -20 through +20.

Every resident's node now has a different greeting at each of the seven trust tiers, plus everyday questions, useful local facts, practical work advice and an in-character saying. Familiar residents unlock additional routine or personal topics; trusted residents share more considered observations; allies receive the most personal responses without learning the entire central mystery. Conditional options are filtered by Yarn when the player's relationship does not meet the threshold.

### Relationship tiers and consequences

| Score | Tier | Conversation | Buying goods | Buy-price multiplier |
|---:|---|---|---|---:|
| 0–14 | Hostile | Refuses | Refuses | 0.00× |
| 15–29 | Distrustful | Brief/guarded | Refuses | 0.00× |
| 30–44 | Wary | Available | Accepts eligible goods at a discount | 0.85× |
| 45–59 | Neutral | Normal | Normal market rules | 1.00× |
| 60–74 | Familiar | Warmer, extra responses | Small premium | 1.05× |
| 75–89 | Trusted | More personal information | Better premium | 1.10× |
| 90–100 | Ally | Best access and strongest responses | Best premium | 1.15× |

These are per-resident trust scores. The buy-price multiplier is a runtime policy contract, not an implemented sale transaction. The prototype has no vendor transaction UI or authoritative sale service yet; the future market must check `CanTrade` and `BuyPriceMultiplier` after item eligibility, freshness, stock and currency checks.

All four residents begin at neutral trust (50). Legacy schema-5 NPC records used zero as an unused default; schema 6 migrates that old baseline to 50. Helping Mara does not automatically improve Tomas's opinion.

### Relationship memory and exploit resistance

- Event IDs are stable, unique per NPC and bounded to 128 memories per resident.
- Replaying a dialogue node cannot repeat a relationship reward or penalty.
- Invalid/whitespace-padded IDs, zero deltas and deltas outside -20 to +20 are rejected.
- Critical clues must remain available at neutral-or-better trust; relationship gates should primarily affect optional services, special dialogue and rewards.
- Hostile residents may refuse conversation. Positive actions must remain available through gameplay systems so trust can be rebuilt; do not create a permanent main-quest soft lock.

### Persistence

Each agent implements `ISaveParticipant` and contributes one record to `SaveGameData.progress.npcStates`:
- `npcId`: stable resident identity;
- `stateId`: last known schedule activity;
- `relationshipId`: relationship score serialized as an invariant-culture integer string;
- `lastKnownRegionId`: resident's current region;
- `relationshipMemoryIds`: bounded stable IDs for relationship events already applied.

Save schema 6 adds the per-NPC memory ledger and migrates older NPC records from the previous unused zero baseline to neutral 50. Yarn's separate typed storage continues to own first-meeting and clue-heard flags.

## Initial Greyhaven cast

| Resident | Occupation | Daily activity | Dialogue node | Character direction |
|---|---|---|---|---|
| Harbourmaster | Harbour office / quay watch | 05:00–18:00 quay; 18:00–05:00 office | `Greyhaven_Harbourmaster` | Restrained, procedural, protective of the channel; gives warnings without explaining every fear |
| Mara Venn | Fishmonger | 04:00–16:00 market; 16:00–04:00 off duty | `Greyhaven_Fishmonger` | Practical and observant; judges catch condition, remembers unpaid crates, refuses to sensationalize strange fish |
| Tomas Vale | Boat mechanic | 06:00–18:00 repair shift; 18:00–06:00 off duty | `Greyhaven_Mechanic` | Skeptical and mechanically minded; begins with ordinary explanations but records damage that does not fit them |
| Inez Sarmiento | Net mender | 07:00–17:00 net work; 17:00–07:00 off duty | `Greyhaven_NetMender` | Patient, careful with evidence, knows gear and local working habits; avoids declaring a supernatural cause |

Names, dialogue and occupations are working canon for this prototype and may be revised before voice production. The Harbourmaster remains unnamed until the wider story establishes why that identity matters.

## Generated workplace placement and wayfinding

The Greyhaven Editor builder places the residents at the following service locations. These are generated world X/Z coordinates; Y is sampled from the Greyhaven terrain profile unless the quay-watch position is explicitly placed on the dock deck.

| Resident | Daytime location | Off-duty anchor |
|---|---|---|
| Harbourmaster | Inner main pier quay-watch point (-1.6, 12) | Harbour Office front (21, -28.5) |
| Mara Venn | Fish Market front (-27, -28.5) | Market-side anchor (-23, -29) |
| Tomas Vale | Boat Repair Shed front (1, -40) | Repair-side anchor (5, -38) |
| Inez Sarmiento | Net Mender Shed front (-48, -46) | Net-shed-side anchor (-44, -46) |

The Harbourmaster's named office anchor is on the Harbour Office front, not at the quay-watch point. Mara, Tomas and Inez remain close to their relevant service entrances so the current direct waypoint movement does not need to route through the buildings.

The physical chart table on the east half of the T-pier maps the island outline and labels resident workplaces with pin letters: H for the Harbourmaster's quay-watch point, O for the Harbour Office after-hours anchor, M for Mara's Fish Market, T for Tomas's Boat Repair Shed, and I for Inez's Net Mender Shed. It is a fixed local chart, not a live schedule display: a marker means "workplace / schedule destination" and does not promise that the character is present at every hour.

Do not change the stable IDs, Yarn dialogue nodes or schedule boundaries when moving visual props. If later navigation work changes these anchors, preserve short obstacle-free routes and update the chart and this placement table together.

## Dialogue memory and relationship design

Yarn's `$npc_relationship`, `$npc_can_talk`, `$npc_can_trade` and `$npc_buy_price_multiplier` variables are seeded from the active C# resident before each conversation and refreshed after a relationship command. The UI only starts an NPC conversation when a valid `NpcAgent` is assigned; a missing agent is reported instead of silently falling back to neutral dialogue. Each resident node includes seven-tier greetings, distinct positive and negative choices, conditional personal topics, everyday subjects and a character-specific quote. Existing character-specific flags remain for save compatibility.

The C# buy policy is implemented, but the sale transaction is not. Future shop, repair, quest-reward and service systems must use the active NPC agent rather than inventing separate reputation values.

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
- Individual trust tiers, visible dialogue meter, conversation refusal threshold and trade willingness/price policy.
- Idempotent relationship-event memory with save/load and validation.
- Dialogue-driven positive and negative relationship changes for all four Greyhaven residents.
- Seven-tier dialogue greetings, daily conversation, profession-specific facts, quotations and trust-gated personal topics for every Greyhaven resident.
- Reusable NPC schedule data and resolver.
- Overnight schedule windows.
- Authoritative sea-clock gating.
- Waypoint movement between authored transforms.
- Persistent resident activity and relationship score.
- Four Greyhaven blockout residents generated by the Editor scene builder.
- Separate Yarn dialogue nodes for the fishmonger, mechanic and net mender.
- Four generated resident conversations exposed through the dockside location hub; the legacy proximity-trigger component remains supported for hand-authored scenes.
- Edit Mode tests for schedule boundaries and NPC save/restore.

Not yet implemented:
- On-foot player controller and walking interaction.
- NavMesh or obstacle-aware pathfinding.
- Character animation, rigs, facial expression, voice acting and final models.
- Walkable 3D interior access, doors, sleep locations and full home simulation. Menu-based interior presentations and selected furniture actions are implemented separately by DocksideHubController.
- Shop transactions, repair services, net crafting or sale-price application; the buy-price multiplier is a policy hook only.
- NPC animation, facial expression, relationship-driven schedules and persistent body-language states.
- Structured memories of world actions beyond the bounded relationship-event ledger.
- NPC reactions to weather, missing vessels, world flags, quest stages and anomalies.
- Regional NPC spawning, population simulation or off-screen schedule simulation.

## Acceptance checks

1. Rebuild Greyhaven and confirm four resident agents exist with unique stable IDs.
2. Confirm Harbourmaster quay-watch and office anchors, Mara's market frontage, Tomas's repair frontage and Inez's net-mender frontage match the generated chart legend.
3. Confirm the Harbour Office, Fish Market, Boat Repair Shed and Net Mender Shed resident buttons display the correct resident name and start the correct Yarn node; verify generated trigger components do not produce duplicate prompts or own the shared Interact action.
4. Confirm schedule windows resolve at their exact start and end boundaries, including overnight windows.
5. Confirm residents move only while the sea clock advances and remain stationary during dialogue, inventory and stopped-boat states.
6. Confirm movement remains on the authored clear routes and does not pass through building geometry.
6. Apply a positive and negative relationship event, repeat the same event ID, and confirm it changes the score only once.
7. Confirm score, relationship tier and event-memory IDs survive save/load; load a schema-5 fixture and confirm its unused zero baseline migrates to neutral 50.
8. Confirm scores below 15 refuse conversation, scores below 30 refuse trade, and required story information remains recoverable.
9. For each resident, verify all seven greetings, daily topics, profession-specific facts, personal topics at 60/75/90, and the resident's saying.
10. Confirm the visible meter updates after a Yarn relationship command and matches the agent's authoritative score.
11. Confirm a missing NpcAgent produces a visible configuration error and cannot start neutral fallback dialogue.
12. Confirm no NPC interaction can start a second conversation while another dialogue is active.

Unity scene generation, Yarn compilation, runtime movement and save/load behaviour still require verification in Unity 6000.3.28f1. Static repository checks do not establish those runtime results.
