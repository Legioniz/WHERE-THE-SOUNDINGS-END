# Character Bible — Main Narrative Cast

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-12

This document defines the narrative functions and knowledge boundaries of the principal cast. It does not replace the runtime NPC roster or claim that every character is implemented.

## 1. Core cast

### The Sounder — protagonist

A former apprentice hydrographic sounder and the sole person recovered after the loss of the survey vessel Orison eight years before the game. Their father captained the vessel; their older sister Nell operated its radio. They have returned to Greyhaven to work on an old boat. They are practical, restrained and burdened by the belief that answering Nell's signal helped cause the loss.

The player should learn this history through a controlled sequence of evidence, not a full opening monologue. See [Protagonist](PROTAGONIST.md).

### Nell — Orison radio operator

Nell is the protagonist's older sister and former radio operator aboard the Orison. She and the protagonist shared a private call-and-response pattern from childhood. The sound of that pattern later appears in an unexplained transmission.

Nell's fate is unresolved. Her voice can appear in recovered recordings, radio interference or a signal reconstructed by the Choir. The story must never treat a voice alone as proof of survival. Nell should be characterised through fragments of ordinary life as well as the final warning: humour, impatience, familiarity and professional competence make the imitation disturbing without requiring constant shouting or pleading.

Nell is not a supernatural guide, quest-giver who knows everything, or confirmed entity. Her actual final actions are reconstructed from incomplete evidence.

### The Orison Captain — the protagonist's father

The protagonist's father commanded the survey vessel. His name and detailed biography remain open. He was responsible for the vessel and crew, and his decisions should be understandable under pressure even when the protagonist disagreed with them.

The later investigation suggests that he recognised a relationship between repeated transmissions and the vessel's location. He attempted to divert the Orison away from Greyhaven and reduce the chance that the signal would draw attention toward the harbour. Do not write him as a secret villain. His choices are protective, imperfect and ultimately unsuccessful.

His exact fate is unknown. Do not confirm a body, ghost, rescue or survival without a separate approved story decision.

## 2. Existing Greyhaven residents

These four residents already have prototype dialogue and runtime schedule foundations. Preserve their current identities and voice direction unless a deliberate character revision is approved.

| Character | Established function | Knowledge and voice boundary |
|---|---|---|
| Harbourmaster — unnamed | Harbour authority, channel safety, work assignment | Procedural and protective. Knows about the missing boats, the silent lighthouse bell and local channels. Does not know the full Orison truth. |
| Mara Venn | Fishmonger and market observer | Practical and attentive to catch condition, unpaid crates and ordinary livelihood. Notices unusual fish but does not declare a supernatural cause. |
| Tomas Vale | Boat mechanic | Skeptical, mechanically minded and observant about engine pitch and hull scoring. Offers plausible causes before admitting a damage report does not fit. |
| Inez Sarmiento | Net mender | Patient, careful and evidence-led. Understands line damage, knots and working habits. Preserves physical clues rather than interpreting them prematurely. |

The existing Yarn file contains the first-meeting, missing-boat, bell, sounding-buoy, repair, catch and net dialogue foundations. New narrative content should extend these characters through the established dialogue architecture rather than replacing them with exposition terminals.

## 3. Missing crews and the returned skipper

The opening incident involves three boats that travelled beyond the outer marker. Only one returned. The returned skipper refuses to give a coherent account and repeatedly asks whether the lighthouse bell is still ringing.

The missing crew members should remain human beings rather than anonymous horror statistics. Give each recoverable boat or personal effect a distinct owner, work role, family connection or routine. Do not reveal every fate through a collectible note. At least one story thread should be communicated through changed routines at the harbour.

The returned skipper's identity is not fixed in this proposal. Their testimony should be limited by shock, fatigue, memory and what they actually observed. Do not make them a perfect witness.

## 4. Lighthouse Keeper — future supporting role

The NPC roster contains a Lighthouse Keeper role placeholder, but the current runtime cast does not include this character. If introduced, the keeper should know the maintenance history of the bell and the lighthouse's practical duties. They may have records showing when the bell stopped ringing eleven years ago.

The keeper must not provide the entire mystery in one conversation. Their evidence should concern dates, maintenance, weather and the difference between a real bell and a sound heard over water.

## 5. Knowledge separation

No character should possess the whole story. Use this distribution as a writing constraint:

| Character | Can credibly know | Must not know automatically |
|---|---|---|
| The Sounder | Personal memory of Orison, sounding work and Nell's private call | The Choir's origin or Nell's final fate |
| Nell's recordings | Her observations, decisions and warning at the time of recording | Events occurring after her recording |
| Orison Captain's records | Course changes, crew procedure and recorded signal incidents | A complete explanation of the phenomenon |
| Harbourmaster | Current missing boats, channel rules, bell history from local accounts | Deep-ocean survey truth |
| Mara | Catch condition, market routines, missing crews' practical relationships | Instrument interpretation or hidden ship logs |
| Tomas | Engine, hull, rope, impact and repair evidence | The origin of unexplained signals |
| Inez | Nets, knots, cut patterns and fishing routines | The Choir's nature |
| Returned skipper | What was seen or heard during one trip | The objective meaning of every signal |

## 6. Voice and dialogue rules

- Residents speak from work, local habits, responsibilities and personal relationships.
- A character can be wrong without being foolish.
- Supernatural vocabulary should be rare. Most residents describe what happened, not what they think the monster is.
- Avoid giving multiple residents identical cryptic warnings.
- Use silence and refusal as character choices, not as a substitute for writing a believable motive.
- Critical evidence must have a non-random acquisition route. Schedule timing may shape presentation but must not permanently block a required clue.
- Keep dialogue state in Yarn where it is conversational; keep quest, inventory and world state authoritative in C#.

## 7. Production status

The Sounder, Nell, the Orison Captain and the proposed story relationships are not implemented as complete narrative content. The Harbourmaster, Mara Venn, Tomas Vale and Inez Sarmiento have prototype runtime dialogue and schedule foundations; this is not equivalent to final character production, voice acting or completed quest arcs.

## Related documents

- [NPC Roster](NPC-ROSTER.md)
- [Greyhaven NPC System](../12_AI_AND_SIMULATION/GREYHAVEN-NPC-SYSTEM.md)
- [Dialogue System](../03_STORY_AND_NARRATIVE/DIALOGUE-SYSTEM.md)
- [Protagonist](PROTAGONIST.md)
- [Main Story](../03_STORY_AND_NARRATIVE/MAIN-STORY.md)
