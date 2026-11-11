# Story Consequences and Persistent World Response

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-12

## 1. Purpose

Story consequences make discoveries visible in the harbour, the journal, dialogue and later expeditions. The world should not react to a generic morality score. It should react to what the player actually did, what evidence they preserved, which promises they kept and which dangers they exposed others to.

## 2. Consequence categories

### Evidence consequences

- Recovering a log changes the journal from rumour to attributed testimony.
- Verifying a sounding adds a confidence-qualified observation.
- Finding physical damage may support or contradict a witness account.
- Destroying or abandoning an instrument can remove future measurement opportunities, but must not erase already recorded evidence.

### Community consequences

- Returning with missing-crew property gives residents something specific to identify or preserve.
- Sharing verified evidence can change whether a resident believes the protagonist.
- A fulfilled request should be remembered by the requesting resident.
- A resident's work routine may change after a loss, but essential dialogue access must not disappear unpredictably.

### Operational consequences

- Hull and engine damage use the existing vessel-condition system.
- Cargo loss or protected quest cargo rules remain owned by the inventory system.
- The world clock remains action-driven.
- Acoustic incidents should use the existing event architecture rather than a separate narrative-only sound system.

### Ending consequences

- The Answer creates a new unresolved distress call.
- The Silence ends the protagonist's immediate expedition and leaves Greyhaven with an incomplete record.
- The Last Mark leaves a chart warning, memorial record and surviving protagonist.
- The optional Keeper epilogue is not part of the initial implementation scope.

## 3. Consequence matrix

| Player action | Immediate consequence | Later narrative consequence |
|---|---|---|
| Recover sounding buoy intact | Harbourmaster's request is fulfilled | Enables a controlled investigation path |
| Refuse to activate buoy | Avoids immediate risk | A safe recovery/recording route must preserve critical clue access |
| Repeat an anomalous sounding from a new heading | Adds a stronger observation | Distinguishes repeatable evidence from one faulty ping |
| Preserve physical evidence | Journal gains attributable record | Supports later Orison reconstruction |
| Return missing-crew belongings | Resident can identify or archive them | Harbour dialogue and memorial details can change |
| Share evidence carefully | Relevant resident confidence may improve | Unlocks testimony or practical assistance where authored |
| Follow an unexplained voice | May increase immediate risk | Must not be treated as proof that the voice is a living person |
| Retreat from a dangerous site | Preserves boat and player agency | The player may return later; no main clue should be permanently lost |
| Record the final boundary | Preserves coordinates and evidence | Required thematic action for The Last Mark |

## 4. Resident response rules

- Harbourmaster responses focus on safety, route notices and the responsibility to other boats.
- Mara Venn responds to catch condition, family ties, debts and returned belongings.
- Tomas Vale responds to repair evidence, hull damage and the sound of machinery.
- Inez Sarmiento responds to line condition, knots, physical traces and careful handling of recovered gear.

Do not make every resident deliver the same lore. A shared fact can be interpreted differently because each person sees a different part of the working community.

## 5. Knowledge and trust

Existing NPC relationship values are a technical foundation only. They are not currently connected to shop prices, quest availability or trust-based dialogue gating. Future story reactions should use explicit authored conditions.

Recommended dimensions:
- **Familiarity:** whether the resident recognises the protagonist and remembers repeated visits.
- **Reliability:** whether the player fulfils requests and returns borrowed items.
- **Confidence:** whether the resident believes a specific account.
- **Personal trust:** whether the resident is willing to share private testimony.

Avoid collapsing these into one universal reputation or morality score.

## 6. Persistence contract — future implementation

Before adding campaign state, define:
- stable quest IDs;
- stable clue/evidence IDs;
- acquisition provenance and source;
- evidence confidence and verification status;
- resident response flags where needed;
- final decision and ending ID;
- a versioned save migration if the DTO changes.

Use the existing save coordinator and typed participant/DTO architecture. Yarn should retain conversational variables; C# should own authoritative quest progression, inventory, currency and world flags. Do not introduce a separate narrative save file.

### Required save/load regression cases

1. Save after accepting the buoy job; reload and confirm the job remains accepted.
2. Save after collecting but before verifying a clue; reload and confirm its state remains unverified.
3. Save after verifying the clue; reload and confirm its provenance and confidence remain.
4. Save after returning a missing-crew object; reload and confirm the resident consequence does not repeat incorrectly.
5. Save with a partial ending evidence chain; reload and confirm eligibility is unchanged.
6. Save after the final choice; reload into the correct ending/aftermath state without replaying the decision.

## 7. Consequence design guardrails

- Do not punish players for missing a random event or NPC schedule window.
- Do not allow one low-trust conversation to lock the main story.
- Do not overwrite the player's journal interpretation when a new witness disagrees.
- Keep uncertainty visible: a consequence can change a resident's belief without proving the belief true.
- Do not make the protagonist's psychological state the sole explanation for objective anomalies.
- Avoid irreversible boat or cargo losses from untelegraphed story events.
- Keep the inner harbour safe from full hostile chase encounters.

## 8. Implementation boundary

These are proposed narrative consequences. The existing repository already has NPC relationships, Yarn variables, autosave checkpoints, vessel condition, cargo persistence and acoustic events, but the campaign consequence system described here is not implemented.
