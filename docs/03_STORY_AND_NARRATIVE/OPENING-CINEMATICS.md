# Opening Cinematics and First Contacts

> Project: WHERE THE SOUNDINGS END  
> Status: Implemented in source; Unity presentation and runtime verification pending  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-15

## Purpose

Give the first minutes a deliberate maritime-horror identity without replacing the working Greyhaven scene, resident dialogue, or player-controlled boat loop. The cinematic layer uses the live Greyhaven camera and world. It does not load a separate movie, create a second scene, or introduce another narrative state owner.

The sequence is restrained: measured camera movement, letterbox framing, a low-contrast archive treatment, brief record captions and fades. It does not use a mandatory jumpscare or explain the Choir.

## Sequence A — application opening record

**Trigger:** first application launch on a local installation, when the PlayerPrefs key wse.openingCinematicSeen is not set.

**Length:** approximately 14 seconds of authored beat time, plus fades. It may be skipped at any time.

| Beat | On-screen heading | Record text |
|---|---|---|
| 1 | The Second Return | The first sounding found the seabed. The second came back eleven seconds late. |
| 2 | A Voice on the Radio | A familiar voice crossed an empty channel. The crew stopped answering. |
| 3 | The Orison Was Lost | One apprentice was recovered. The vessel and the rest of its crew were never found. |
| 4 | The Coast Is Waiting | Eight years later, a new hand takes work on an old boat. There is a harbour to learn and a living to make. |

The opening is intentionally incomplete. It establishes the Orison loss and the protagonist's past without explaining the signal, assigning objective blame, confirming the voice's identity, or revealing later evidence. The title menu appears after the sequence. **Replay Opening Record** is available from the title menu.

## Sequence B — first voyage prologue

**Trigger:** the player chooses Begin / Continue and neither a valid manual save nor a restorable autosave is available.

**Length:** approximately 12 seconds of authored beat time, plus fades. It may be skipped.

| Beat | On-screen heading | Record text |
|---|---|---|
| 1 | Three Departures | Three boats passed beyond the outer marker. Only one came back. |
| 2 | A Silent Bell | The returned skipper kept asking whether the lighthouse bell was ringing. It had been silent for eleven years. |
| 3 | The First Job | Recover the sounding buoy intact. Do not switch it on. |
| 4 | Learn the Water | Keep the old boat running. Learn the channel. Bring back what the sea gives you. |

After the prologue, control returns to the existing Greyhaven gameplay scene. The Harbourmaster's Yarn node remains the source of the resident's actual greeting and optional conversation; the cinematic does not auto-start or pre-answer dialogue.

## Sequence C — resident first contact

**Trigger:** the first accepted dialogue handoff to a resident during the current application session.

Each stable Greyhaven NPC ID may launch one short introduction. The cinematic completes before the existing Yarn node begins. It never bypasses NpcAgent.CanTalk, resident relationship thresholds, dialogue variable seeding, or the existing DialogueRunner.

| Resident | Introduction emphasis |
|---|---|
| Harbourmaster | Channel discipline, arrivals, departures and names that have not returned |
| Mara Venn | Catch condition, weight, colour and the signs of a rough landing |
| Tomas Vale | Engine reliability and faults that are easy to overlook |
| Inez Sarmiento | Knots, worn line and empty nets |

First-contact presentation uses a session-local ID set. It is cosmetic and is not written to campaign saves; re-launching the application may show the resident introduction again. No quest, relationship, evidence or save state depends on seeing it.

## Presentation and controls

- The live CameraFollow2_5D camera is temporarily detached from its follow behaviour and moved between authored focus points. Its original transform, projection zoom and follow-enabled state are restored after the sequence.
- Full-screen shading, restrained scanline accents, upper/lower letterbox bars, a lower-third record card and fade transitions frame the in-world camera.
- Cinematic timing uses unscaled time. The gameplay time scale is paused while the sequence is active, so the action-driven sea clock and NPC schedules do not advance.
- Skip is available through the on-screen control, Escape, Space, Enter, or gamepad Start. Skipping advances to the next gameplay/menu state; it does not skip the resident's actual Yarn dialogue.
- The overlay uses text for every critical narrative cue. No clue or progression requirement depends on an audio-only effect.
- The current implementation uses existing procedural Greyhaven scenery and UI Toolkit elements. It does not include commissioned shots, character animation, voice acting, music or new licensed media.

## Presentation upgrade — cinematic director pass

The presentation layer exposes a beat counter and a per-sequence progress line so players can judge the remaining length. Captions reveal progressively at a controlled reading pace rather than appearing as a static paragraph. The title and eyebrow remain fully visible while the supporting record text types in.

A separate **Reduce cinematic camera movement** accessibility preference is stored locally. When enabled, authored camera moves and zoom are suppressed, captions appear immediately, and transitions shorten. The scene still uses the same narrative cards and sequence order. This is independent from the existing reduce-camera-shake preference.

The progress line advances once per beat, not by elapsed wall-clock time, so skipping cannot produce a misleading percentage. Caption reveal uses a pure, bounded policy with tests for negative timing, zero text, zero speed, oversized progress and invalid floating-point values.

## Ownership and lifecycle

GreyhavenFrontEndUI owns the cinematic overlay, title/replay flow, camera snapshot, animation and skip input. The existing GreyhavenRuntimeUI delegates a first resident greeting to that front end, then starts the same Yarn node after camera restoration. CinematicSequencePolicy owns only the deterministic validation/registration rule for a session-local first contact.

The cinematic code is presentation-only. It introduces no save schema change, new input action, package, quest flag, relationship adjustment, or persistent campaign identifier. The existing save coordinator remains unchanged.

## Acceptance checks

### Automated source coverage

- First-contact registration accepts a valid stable ID once.
- A distinct resident ID is independently eligible.
- Blank, untrimmed and oversized IDs are rejected.
- A missing session set is rejected with an explicit argument exception.
- Repository validation checks that the front end contains the opening and voyage sequences, cinematic camera restoration/skip path, and that resident dialogue routes through the first-contact handoff.

### Unity Editor and player checks still required

1. Launch a clean local installation. Confirm the opening record plays over the Greyhaven scene, then opens the title menu.
2. Relaunch. Confirm the automatic opening does not replay; use Replay Opening Record to play it again.
3. With no save files, choose Begin / Continue. Confirm the first-voyage prologue plays, then returns control to the boat.
4. Create a save, return to title and Continue. Confirm the prologue does not replay over a restored voyage.
5. Open each of the four resident conversations. Confirm one camera/name-card beat precedes the correct Yarn greeting and the relationship UI is still correct.
6. Speak to the same resident again in the same session. Confirm no second introduction is played.
7. Test skip from the button, Escape, Space, Enter and gamepad Start. Confirm the camera transform, zoom and follow behaviour are restored exactly.
8. Confirm the sea clock, NPC schedule and vessel do not advance during the cinematic, and that opening/resident overlays do not overlap pause, dockside or dialogue panels.
9. Repeat at the target desktop aspect ratios and verify text safe areas, legibility, keyboard focus, gamepad access and reduced-motion expectations.

Static repository checks do not substitute for these Unity and Windows player checks.
