# Endings — The Answer, The Silence and The Last Mark

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-12

## 1. Design principles

The game has three core endings and one optional secret-epilogue concept. The endings are not good/neutral/evil labels. They express different responses to uncertainty and grief.

Ending selection must emerge from evidence, understanding and an explicit final action. Do not use a hidden morality score, an arbitrary percentage, a random roll or one obscure missable collectible. A player should understand why the chosen action is available and what immediate risk it carries.

The recommended thematic ending is **The Last Mark**. It does not restore the lost crew or solve the phenomenon. It changes what the protagonist chooses to preserve.

## 2. Shared final setup

The protagonist reaches the final acoustic boundary in the Hollow Trench. The Sounding Rig cannot establish a dependable seabed return. A voice matching Nell's private call is heard over the radio or in the repeated signal. The player has the opportunity to review evidence before acting.

The final choice is presented through a physical interaction with the sounding/radio system and the boat's navigation instruments, not a menu that labels the choices as good or bad.

## 3. Ending A — The Answer

**Thematic meaning:** Grief turns uncertainty into an invitation.

### Eligibility and trigger

Available to any player who reaches the final boundary. The player chooses to answer the signal and follows the voice beyond the last reliable sounding, despite prior warnings or incomplete evidence.

This ending must not be selected automatically because the player missed optional records. It is an explicit decision to follow.

### Sequence

The voice uses the private call pattern and appears to become clearer as the boat moves deeper. The sounding display produces a final apparent return, then loses a stable bottom. Radio and ambient sound collapse into a narrow, repeating phrase.

The boat's heading and instruments become increasingly difficult to reconcile. The player continues by choice until the scene cuts at the point where the vessel passes beyond the marked boundary. Do not depict a graphic death or claim to know exactly what happens to the crew.

### Aftermath

Greyhaven later receives a distress call that carries the protagonist's voice. The call repeats a phrase from the opening, including a small personal detail that should not be possible from an ordinary recording. The boat is not recovered.

The sea has not confirmed Nell's survival. It has only repeated a voice. The player has joined the unresolved history.

### Unresolved questions

- Is the distress call a recording, echo or new event?
- Is the protagonist alive?
- Did the signal imitate the protagonist, or did the protagonist hear what they expected?

## 4. Ending B — The Silence

**Thematic meaning:** Survival sometimes requires refusing to answer.

### Eligibility and trigger

Available to any player who reaches the final boundary and chooses to stop the active sounding/radio sequence, disconnect or abandon the apparatus and retreat. It does not require the full evidence set.

### Sequence

The player shuts down or disconnects the equipment and refuses the voice's request to continue. The soundscape returns gradually: engine vibration, wave movement and distant weather. The protagonist navigates back using visual and physical cues rather than waiting for a final reply.

### Aftermath

The protagonist survives. The immediate signal chain at the site is interrupted and the current disappearances may cease or become less frequent, but the game does not claim that the Choir has been destroyed. Greyhaven remains physically intact. The protagonist keeps the incomplete record and chooses not to operate the sounding equipment again.

In the closing scene, three measured knocks are heard beneath the tied-up boat. They may be ordinary hull contact or a residual signal. The protagonist does not answer.

### Unresolved questions

- Did the protagonist interrupt the phenomenon or simply leave its range?
- What happened to Nell?
- Will the signal recur elsewhere?

## 5. Ending C — The Last Mark (recommended thematic ending)

**Thematic meaning:** Acceptance does not erase grief; it preserves the truth and protects others.

### Eligibility

Requires a coherent evidence chain across the main evidence families. The player must have:
- established that the acoustic pattern predates the Orison;
- found evidence that the Orison changed course to keep the signal away from Greyhaven;
- found evidence of Nell's final radio role;
- understood that the private voice does not prove that its speaker is alive.

The exact quest flags and thresholds must be defined in a future data contract. Do not implement these requirements as a raw collectible count.

### Sequence

The player reviews the route, logs the repeated returns and marks the last position where the measurement remained reliable. The final message is heard in context:

**"Don't follow the sound. Mark the water."**

The player records the position and the names that can be verified from the recovered records. They stop answering the signal and disable or abandon the active sounding apparatus without destroying the evidence. The boat turns away from the deepest return.

### Aftermath

The protagonist returns to Greyhaven with the evidence. The harbour does not celebrate a supernatural victory. Instead, a record is made of the missing crews and the Orison. The Harbourmaster can place a warning on the chart; residents can remember the people who were lost through ordinary objects and changed routines.

The protagonist leaves Nell's radio or an associated personal item beside a memorial or archive, not as proof of her death but as an acknowledgement that her final message has been understood. They return to practical work, but no longer use the Sounding Rig to chase the voice.

The journal's final entry should state the principle without explaining the phenomenon:

> No bottom. No safe depth. Only the point where the sound stops returning. I thought listening would bring them home. I was wrong. Someone should know where they were lost.

A final sounding pulse is emitted from a safe distance. No echo returns. The player is not told whether the silence means safety, absence or refusal.

### What this ending resolves

- The Orison's course diversion was an attempt to protect Greyhaven.
- Nell's final message was a warning to record the location, not to pursue her voice.
- The protagonist can accept uncertainty and still act responsibly.
- The lost are remembered and future mariners receive a warning.

### What it does not resolve

- The Choir's origin, intelligence or physical nature.
- Nell's exact physical fate.
- Whether the signal has ended permanently.
- Whether the final silence is natural or anomalous.

## 6. Optional secret epilogue — The Keeper

This is a candidate post-launch or later production feature, not a required fourth ending.

With nearly complete optional evidence and a separate final choice, the protagonist may remain near the boundary long enough to establish a visible warning marker and a repeatable route for other vessels. Years later, another fisherman hears the protagonist's voice warning them away from the water.

The epilogue should be ambiguous: the protagonist may have become part of the signal, may have left a recording, or may simply have become a story told by the next generation. Do not confirm a literal transformation.

Do not build this until all three core endings are implemented and playtested.

## 7. Ending matrix

| Dimension | The Answer | The Silence | The Last Mark |
|---|---|---|---|
| Final action | Follow and answer | Stop and retreat | Record, mark and disengage |
| Protagonist | Lost / unresolved | Survives | Survives |
| Greyhaven | Receives a new distress call | Immediate danger recedes, future unknown | Receives a durable warning and record |
| Nell | Voice remains unresolved | Fate remains unresolved | Her warning is understood; fate remains unresolved |
| Choir | Appears to continue through the call | Interrupted locally or escaped | Documented but unexplained |
| Emotional outcome | Compulsion | Self-preservation | Acceptance and responsibility |

## 8. Fairness and accessibility requirements

- Show the final choice and its immediate risk in plain language and environmental context.
- Provide captions and a visual cue for the private call and final message.
- Never make a critical choice depend on hearing a subtle frequency.
- Keep essential evidence available through multiple acquisition paths.
- Do not punish players who choose to retreat early.
- Allow players to review the journal before committing to an ending.
- Do not use random event timing to decide which ending is selected.

## 9. Implementation boundary

All endings are proposed design. No ending selection, eligibility logic, ending-specific world-state, epilogue or ending cutscene is implemented. Future work requires a stable quest/evidence data contract, save persistence, deterministic tests and Play Mode verification.
