# NPC Relationships

> Project: WHERE THE SOUNDINGS END  
> Status: Relationship-driven dialogue and runtime trust rules implemented for Greyhaven; trade transactions and world-action reactions remain future integration  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-13

## Purpose

NPC relationships make Greyhaven feel like a community that notices how the player behaves. Residents should remember honesty, care, reliability, disrespect and the handling of shared evidence. They should not all react identically, and a single global morality score must not replace personal relationships.

Each resident owns an independent trust score. Improving Mara's opinion does not automatically improve Tomas's or the Harbourmaster's opinion. The system rewards consistent conduct over repeated conversations, not repeated button presses.

## Authoritative state

`NpcAgent` owns the relationship score and event-memory ledger. Yarn reads a temporary view of the active resident's state; it never owns the authoritative score.

Before starting a conversation, `GreyhavenRuntimeUI` writes:

- `$npc_relationship`: the active resident's current score (float).
- `$npc_can_talk`: whether normal conversation is permitted.
- `$npc_can_trade`: whether the resident will buy eligible goods.
- `$npc_buy_price_multiplier`: the relationship policy multiplier for future sale transactions.

The Yarn command `<<relationship "stable.event.id" delta>>` requests a relationship event. The command applies only to the active resident and only when the ID has not been used for that resident. C# updates the score and synchronizes the Yarn variables and visible dialogue meter immediately.

## Trust scale

All four Greyhaven residents start at neutral trust (50).

| Score | Tier | Typical response | Trading policy |
|---:|---|---|---|
| 0–14 | Hostile | Refuses normal conversation | Refuses |
| 15–29 | Distrustful | Terse, guarded answers | Refuses |
| 30–44 | Wary | Practical answers, little personal detail | 0.85× policy |
| 45–59 | Neutral | Normal everyday conversation | 1.00× policy |
| 60–74 | Familiar | Warmer greetings and routine/personal topics | 1.05× policy |
| 75–89 | Trusted | Personal work habits and more sensitive observations | 1.10× policy |
| 90–100 | Ally | Most candid optional subjects and strongest goodwill | 1.15× policy |

At scores below 15, the interaction prompt communicates refusal and the normal dialogue node is not started. At scores below 30, the resident may speak but will not buy. At 30–44, the API permits eligible goods at the cautious multiplier. The current prototype does not yet have a sale transaction that applies these multipliers; they are a policy contract for the future market.

## What the relationship changes

1. **Recognition and greeting.** Each resident has a distinct greeting for every tier, not just a first-meeting line and one high-trust line.
2. **Tone and detail.** A wary resident gives short practical answers. A familiar resident talks about routines. A trusted resident shares the reasoning behind their habits. An ally may share an optional personal observation.
3. **Topic availability.** Each resident has optional subjects that appear at trust thresholds 60, 75 and 90.
4. **Service willingness.** `CanTrade` blocks the future vendor from accepting goods below 30. The multiplier adjusts a future accepted purchase price; it does not create currency or force a transaction.
5. **Relationship UI.** The active conversation shows the resident's name, tier and 0–100 meter. The meter refreshes immediately after a relationship event.

## Resident-specific relationship and dialogue identity

| Resident | Trust-building behaviour | Trust-damaging behaviour | Everyday subjects | Voice and sayings |
|---|---|---|---|---|
| Harbourmaster | Listening to channel warnings, respecting markers, accepting assigned work | Dismissing safety warnings | Watch routine, markers, weather, missing boats, the silent bell | Restrained, procedural, protective. "A harbour is a promise, not a fence." |
| Mara Venn | Honest descriptions of unusual catches and careful handling | Hiding catch condition or misrepresenting what was found | Species in the market, catch quality, freshness, market chores, missing crews | Practical and observant. "Freshness is a promise the buyer cannot check for themselves." |
| Tomas Vale | Admitting engine problems and taking repair advice seriously | Dismissing practical warnings | Engine idle, outboard checks, fasteners, hull marks, repair routine | Skeptical and mechanically minded. "An engine tells you its troubles twice: once in a whisper, once in a tow bill." |
| Inez Sarmiento | Preserving damaged line and respecting physical evidence | Dismissing findings without inspection | Net wear, knots, float lines, repairs, local working rules | Patient and evidence-led. "A net is only as strong as the knot nobody checks." |

These sayings are original in-world dialogue. Practical facts stay within each character's occupation and knowledge boundary; no resident is allowed to explain the full Orison mystery.

## Relationship event memory and anti-farming rules

- A relationship event ID is stable, unique per resident and limited to 256 characters.
- One resident can remember at most 128 relationship events.
- Deltas must be non-zero and between -20 and +20.
- Empty, whitespace-padded, duplicate or malformed IDs are rejected.
- Reopening a topic does not reapply the same event.
- Event IDs are persisted with the resident in save schema v6 and validated during save/load.
- Yarn variables are refreshed after a successful event so any later conditional dialogue in the same conversation reads the new score.
- Conversation-only repeated questions should not award trust. Rewards should represent a specific action, honest admission or meaningful decision.

## Recovery, clues and quest safety

Relationship penalties must create social consequences without permanently blocking the campaign.

- Critical story information must have another discoverable source or an alternate witness.
- The current Greyhaven nodes keep core opening clues available to any resident who is willing to speak; extra personal details are optional.
- Future world actions—returning borrowed gear, helping with repairs, delivering requested goods, preserving evidence or completing jobs—should be able to improve the relevant resident's trust through the same `NpcAgent` API.
- The current prototype does not yet implement a general gift, repair, sale or quest-completion relationship event service. Do not present those recovery paths as implemented.
- A hostile resident currently refuses the normal conversation path. Before a full market/quest loop ships, provide at least one non-conversation recovery action for every essential service NPC.

## Acceptance tests

1. Confirm the active resident's score, tier and meter match the C# `NpcAgent`.
2. Confirm each node produces its own seven-tier greeting and changes everyday answers according to trust.
3. Confirm Familiar, Trusted and Ally optional dialogue topics are hidden below 60, 75 and 90 respectively.
4. Confirm each character's practical facts and sayings stay within their documented knowledge and voice.
5. Confirm the same relationship event can only be applied once per resident.
6. Confirm scores below 15 refuse normal conversation and scores below 30 refuse trade.
7. Confirm schema-5 saves migrate the old zero default to neutral 50 and schema-6 event memories survive reload.
8. Confirm required clues remain obtainable without maximizing any resident's relationship.
9. Confirm a missing `NpcAgent` cannot start a neutral fallback dialogue.
10. Compile Yarn in Unity, exercise each resident node at scores 10, 20, 40, 55, 65, 80 and 95, and record a Play Mode test run.

## Related implementation and design

- `Assets/_Project/Scripts/Runtime/NPC/NpcAgent.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs`
- `Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn`
- `docs/12_AI_AND_SIMULATION/GREYHAVEN-NPC-SYSTEM.md`
- `docs/03_STORY_AND_NARRATIVE/DIALOGUE-SYSTEM.md`
- `docs/06_GAMEPLAY_SYSTEMS/SAVE-AND-LOAD.md`

## Narrative evidence and relationship access

The expanded Greyhaven investigation uses four shared Yarn clue flags: $gh_clue_bell_log, $gh_clue_cold_catch, $gh_clue_hull_marks, and $gh_clue_net_return. These are story-discovery flags, not relationship scores and not a substitute for NpcAgent memory. Discovering an observation does not award trust merely for reopening a conversation.

Core evidence topics are available to any resident who permits normal conversation; they are not gated at Familiar, Trusted or Ally. The cross-record comparison appears after at least one observation has been found and offers a cautious partial summary until all four are known. Personal routine/hobby details remain gated at the existing Familiar threshold (60). No resident's trust is changed by reading the evidence summary, and the existing per-resident event IDs, trust thresholds and save schema remain unchanged.
