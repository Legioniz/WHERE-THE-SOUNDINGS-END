# Protagonist — The Sounder

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-12

## 1. Character purpose

The protagonist is a working mariner who understands boats, weather, instruments and the cost of a bad decision. They are capable, observant and useful under pressure, but they are not a soldier, investigator by trade or chosen hero. Their expertise is practical: they know how to keep a small vessel moving, read a working coastline, operate a sounding instrument and recognise when a measurement is unreliable.

The narrative uses the working designation **The Sounder**. It is a production label, not necessarily the character's spoken name or an identity used by residents. Their personal name, face, exact age and gender presentation remain undecided. Do not lock these details into dialogue, save data or marketing until the project lead approves them.

The protagonist's central conflict is not whether they can defeat the sea. It is whether they can stop treating an impossible loss as a problem that can be solved by listening harder.

## 2. Backstory — the Orison incident

Eight years before the opening of the game, the protagonist served as an apprentice hydrographic sounder aboard the survey vessel Orison. The vessel was captained by the protagonist's father. Their older sister, Nell, worked the radio.

The Orison was surveying a deep-water route near the region now associated with the Hollow Trench. During a sounding run, the instrument registered a seabed return, followed by a second return that arrived eleven seconds later. The second return did not agree with the charted depth or the expected bearing. Repeated measurements produced inconsistent results.

A later transmission appeared to contain Nell's voice and used a private call-and-response pattern known only to the siblings. The protagonist believed Nell was speaking directly to them. The crew had already begun to suspect that answering or repeating an unexplained signal could make the vessel easier to locate. The captain ordered the crew to stop responding and divert away from the original survey line.

The protagonist's memory of the final hours is incomplete. They remember a changed engine note, a radio transmission, the sounding instrument continuing after it should have been shut down, and the sea becoming unnaturally quiet. The Orison and its crew were lost. The protagonist was the only person recovered. No intact wreck or definitive remains were found.

The official explanation attributed the loss to navigational error, equipment failure and severe conditions. The report did not explain the delayed return, the private call or the absence of a recoverable wreck. The protagonist accepted the official account publicly and never resolved it privately.

### Important emotional distinction

The protagonist feels responsible because they answered the signal and trusted a familiar voice. The story must not establish that a young apprentice single-handedly caused the Orison's loss. Guilt is a subjective burden, not proof of culpability. Evidence later reveals that the crew had been making its own difficult decisions before the final night.

## 3. Why the protagonist is in Greyhaven now

After the Orison incident, the protagonist avoided deep-water survey work and spent years away from the route where the vessel was lost. They return to Greyhaven eight years later and take work on an old local boat. The Harbourmaster's existing opening line — "You're the new hand on the old boat, then" — remains a suitable first introduction.

The immediate reason to stay is practical: the protagonist needs work and a vessel. The reason to investigate is gradual: three boats have gone beyond the outer marker and only one has returned. The survivor asks whether the lighthouse bell is still ringing, even though the bell has been silent for eleven years. A sounding-buoy recovery job creates the first opportunity to encounter an impossible return without making the protagonist volunteer for a grand conspiracy.

The protagonist does not arrive knowing that the Choir exists. They recognise fragments of the Orison incident only after the player has established ordinary explanations and collected corroborating evidence.

## 4. Motivation and dramatic progression

| Stage | External objective | Internal belief | Change |
|---|---|---|---|
| Opening | Earn a living, learn Greyhaven's waters and recover the sounding buoy | Work and routine can keep the past contained | Familiar warning signs return |
| First investigation | Find out why boats disappear and why the bell is mentioned | Nell may have survived or left a recoverable message | A voice can be reproduced without proving its speaker is alive |
| Wider investigation | Compare soundings, logs, testimony and older records | Better instruments will produce a definitive answer | Instruments provide evidence, not certainty |
| Deep investigation | Reconstruct the Orison's route and the crew's final decisions | If the complete signal is found, the loss can be undone | The past cannot be reversed; it can be recorded honestly |
| Finale | Choose whether to follow, silence or mark the signal | Listening is the same as helping | The protagonist can choose what future people are told |

## 5. Personality and player expression

The default character is restrained, practical and economical with words. They notice operational details before interpreting them emotionally. Their silence should leave room for player projection, but they must still communicate enough to make the story legible.

Player expression should focus on attitude and method rather than changing the protagonist's biography:
- cautious and evidence-led;
- sympathetic toward residents and missing crews;
- impatient with superstition but willing to test it;
- personally drawn toward Nell's voice;
- willing to preserve evidence even when it cannot provide comfort.

These are dialogue approaches, not a morality alignment. No approach should make the protagonist cruel or incompetent by default.

## 6. Competence and limitations

**Competent at:** small-boat operation, basic seamanship, fishing work, equipment maintenance, practical navigation, using the Sounding Rig, documenting observations and judging ordinary maritime risk.

**Not competent at:** military combat, advanced medical treatment, deep-sea diving without suitable equipment, interpreting every biological sound, repairing specialised survey systems without parts, or knowing the truth about the Choir.

The protagonist is physically vulnerable. Most danger should be solved by preparation, route choice, reduced noise, timing, retreat and situational awareness. The story must not turn them into an action hero in the final act.

## 7. Voice, performance and visual direction

- Keep dialogue concise and grounded in immediate observations.
- Avoid constant internal monologue explaining what an image already communicates.
- Use pauses, interrupted phrases and changed behaviour sparingly; do not make every line ominous.
- If the character remains visually understated, show emotion through posture, hand movement, hesitation around the radio and care with recovered objects.
- Do not use a fixed portrait or voiced name until identity presentation is approved.
- The protagonist's private call with Nell should be specific and consistent. Do not reuse it as a generic scare cue.

## 8. Character arc guardrails

1. The protagonist's guilt must be understandable but not presented as objective truth.
2. Nell's voice is not proof that Nell is alive.
3. A successful ending does not resurrect Nell or recover the Orison crew.
4. The player should be able to choose compassion without being tricked into the dangerous ending.
5. The protagonist's practical abilities must remain credible across all acts.
6. The character should not explain the Choir more confidently than the evidence allows.
7. The protagonist's final growth is accepting uncertainty while preserving the truth of what happened.

## 9. Narrative state and implementation notes

This document is a proposed character specification. No protagonist biography, Orison quest chain, Nell dialogue, or ending state is currently implemented by this document.

When implementation begins:
- Use Yarn for authored dialogue and branching.
- Keep authoritative quest and world progression in C# services, not duplicated Yarn-only state.
- Define stable quest, clue and world-state identifiers before content ships.
- Add story progression through the existing save coordinator and typed save contracts.
- Do not add arbitrary strings to the save schema as a shortcut.
- Preserve current resident identifiers and existing Harbourmaster/Mara/Tomas/Inez dialogue variables.
- A complete story save/restore test must prove that evidence and ending eligibility survive reload.

## Related documents

- [Main Story](../03_STORY_AND_NARRATIVE/MAIN-STORY.md)
- [Story Structure](../03_STORY_AND_NARRATIVE/STORY-STRUCTURE.md)
- [Endings](../03_STORY_AND_NARRATIVE/ENDINGS.md)
- [Character Bible](CHARACTER-BIBLE.md)
- [World Bible](../02_LORE_AND_WORLD/WORLD-BIBLE.md)
- [Dialogue System](../03_STORY_AND_NARRATIVE/DIALOGUE-SYSTEM.md)
