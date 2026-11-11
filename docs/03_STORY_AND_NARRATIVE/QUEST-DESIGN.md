# Quest Design — Main Narrative Framework

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-12

## 1. Quest philosophy

Quests should emerge from working life: a boat did not return, a line came back cut, a buoy drifted ashore, a repair does not match the reported impact, or an old record needs to be checked. The player should be doing something practical while the larger mystery accumulates.

A quest is not a checklist of exposition. It should give the player:
- a reason to travel or speak to someone;
- a practical task with understandable risk;
- evidence that changes an interpretation;
- a decision or consequence;
- a safe way to stop and return when appropriate.

## 2. Quest categories

| Category | Purpose | Example |
|---|---|---|
| Main investigation | Advances the Choir/Orison mystery | Recover and verify a sounding record |
| Resident request | Builds a relationship and community context | Return damaged netting or missing-crew property |
| Survey assignment | Teaches instrument use and regional observation | Compare two pings from different headings |
| Maritime incident | Tests preparation and counterplay | Investigate a drifting boat with an uncertain source |
| Historical discovery | Establishes a timeline | Compare the silent-bell record with the Drowned Parish |
| Optional legend encounter | Adds an independent maritime mystery | Observe a rare creature without forcing hostility |

## 3. Main investigation spine — proposed quest sequence

These are design beats, not committed quest IDs. Stable identifiers must be assigned only when a quest data contract is introduced.

### Q1 — The Outer Marker

**Giver:** Harbourmaster  
**Purpose:** Introduce the missing boats and establish the working route.

Tasks:
1. Speak to the Harbourmaster.
2. Learn that three boats went beyond the outer marker and only one returned.
3. Ask about the bell or leave the subject alone.
4. Receive the sounding-buoy recovery job.
5. Recover the buoy without being forced to activate it.

Evidence: the buoy's physical condition, attached record or later controlled test.  
Outcome: the player has a practical reason to use the Sounding Rig.  
Failure handling: if the player refuses the job, it remains available; if the buoy is not activated, a second safe evidence route exists.

### Q2 — A Return That Arrived Late

**Purpose:** Teach verification rather than jump scare.

Tasks:
1. Perform a baseline sounding in a known area.
2. Compare a second return at a changed heading.
3. Record the delay, bearing and confidence.
4. Return to harbour and compare the reading with a witness or chart.

Evidence: a repeatable acoustic discrepancy.  
Outcome: the event is no longer explained solely by a single damaged instrument.  
Failure handling: allow the player to retry later; do not fail the story because of one poor measurement.

### Q3 — The Boats That Came Back Empty

**Givers:** Mara Venn and Inez Sarmiento  
**Purpose:** Make the missing crews matter as people and working neighbours.

Tasks:
1. Inspect recovered gear or a returned boat.
2. Compare net condition, missing catch and personal property.
3. Ask each resident what they personally observed.
4. Record contradictions without selecting a forced explanation.

Evidence: physical object plus separate testimony.  
Outcome: the player understands the cost of the missing boats beyond a monster rumour.

### Q4 — The Engine Heard It First

**Giver:** Tomas Vale  
**Purpose:** Ground the horror in vessel mechanics.

Tasks:
1. Bring the boat or recovered component for inspection.
2. Compare engine pitch, hull marks and possible contact damage.
3. Test ordinary explanations before recording the unresolved detail.

Evidence: a damage pattern or vibration that does not fit the initial report.  
Outcome: the player learns to distinguish ordinary boat noise from a repeated pattern.

### Q5 — The Bell's Last Service

**Potential giver:** Harbourmaster or future Lighthouse Keeper  
**Purpose:** Establish the eleven-year gap between the bell's silence and the present events.

Tasks:
1. Find maintenance or harbour records.
2. Compare the date with older survey reports.
3. Locate a second source in local history or the Drowned Parish.

Evidence: dated record with provenance.  
Outcome: the bell motif has a history independent of the Orison.

### Q6 — Before the Orison

**Purpose:** Reveal that the acoustic pattern predates the protagonist's loss.

Tasks:
1. Survey an older site or recover an archival record.
2. Verify the record's date and location.
3. Compare its acoustic description with the modern observations.

Evidence: pre-Orison record.  
Outcome: the Orison was not the origin of the phenomenon.

### Q7 — The Course Away

**Purpose:** Reconstruct the captain's decision.

Tasks:
1. Recover a chart fragment or course log.
2. Compare the original route with the recorded diversion.
3. Cross-reference the event with the crew's radio procedure.

Evidence: route change plus an independent account.  
Outcome: the captain attempted to keep the signal away from Greyhaven.

### Q8 — Nell's Last Transmission

**Purpose:** Reinterpret the personal call.

Tasks:
1. Recover two distinct radio/recording fragments.
2. Identify the shared private call pattern.
3. Compare the message with the course log.
4. Separate the original warning from the later repeated signal.

Evidence: Nell's final warning and provenance.  
Outcome: "Don't follow the sound. Mark the water." is understood as an instruction, not a plea.

### Q9 — The Last Sounding

**Purpose:** Set up the finale.

Tasks:
1. Review the evidence families.
2. Prepare the vessel and choose a route.
3. Reach the final sounding boundary.
4. Make an explicit final action.

Outcome: The Answer, The Silence or The Last Mark.

## 4. Side-quest framework

Side quests should deepen local history and livelihood without becoming compulsory keys to the main story. Examples:
- identify the owner of recovered fishing gear;
- repair or return a damaged net;
- compare a strange catch with known local species;
- investigate a bell-like tone heard near a physical structure;
- help a resident preserve a missing crew member's effects;
- observe a legendary animal without attacking it;
- document a survey site that has no immediate threat.

Each side quest must have a practical reward or changed relationship, and must not falsely promise to solve the Choir.

## 5. Clue design and redundancy

For every essential revelation, plan at least two independent evidence paths. Example:

| Revelation | Evidence path A | Evidence path B |
|---|---|---|
| Bell silent for eleven years | Harbour maintenance record | Keeper testimony or archived service note |
| Acoustic pattern predates Orison | Drowned Parish record | Older survey log from another site |
| Orison diverted away from Greyhaven | Course chart | Captain's log or crew procedure note |
| Nell's final message is a warning | Original recording | Radio transcript cross-referenced with route marks |
| Voice does not prove survival | Delayed/repeated recording | Instrument comparison and contradictory timing |

A critical path must not depend on:
- a rare random world event;
- a single NPC being at a specific schedule point;
- a one-time collectible that can be lost or sold;
- hearing a specific frequency without captions;
- a particular fishing catch being rolled by chance.

## 6. Quest state model — design requirements

A future quest data contract should represent:
- stable quest ID and stage ID;
- prerequisites;
- accepted/active/completed/failed state where applicable;
- objective progress;
- evidence IDs and their provenance;
- checkpoint/autosave request;
- reward and resident consequence;
- retry or recovery path;
- completion telemetry for playtesting.

Quest stages should be idempotent. Reopening dialogue or reloading a save must not award the same item twice, duplicate a record or re-run a one-time world event.

Yarn can manage dialogue branches and conversation-local variables. C# remains authoritative for quest progression, inventory, currency and world state. Do not create campaign state as unstructured Yarn strings.

## 7. Vertical-slice quest scope

The first 45–90 minute Greyhaven slice should implement only a small, coherent chain:
1. The Outer Marker.
2. A Return That Arrived Late.
3. One resident's physical-evidence follow-up.
4. A clear return-to-harbour and save/resume path.

This slice should foreshadow the Orison without implementing the entire campaign, all missing crews, the Drowned Parish or any ending.

## 8. Acceptance checks

- A new player can identify the active task and its next practical action.
- Every main clue can be acquired through a deterministic, documented path.
- The player can retreat and resume without losing required evidence.
- Repeated interactions do not duplicate rewards or advance stages incorrectly.
- The journal labels observation, testimony and interpretation separately.
- A saved campaign restores quest stage, evidence provenance and resident consequences.
- No critical clue is delivered solely by audio.
- Quest completion and ending eligibility are deterministic.

## 9. Implementation boundary

This document is a proposed quest framework. The current Harbourmaster dialogue includes a sounding-buoy job checkpoint, but the full quest system, objectives, clue records, evidence journal, resident consequences and ending logic are not implemented by this specification.
