# Threat Audio Logic and Cue Matrix

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Design rule

Audio is part of the information system. Threat and anomaly states emit semantic cues through the shared SoundEvent bus; presentation maps those cues to clips, spatialization, captions, and mixer snapshots. Audio playback does not own AI or quest logic.

## Cue matrix

| Event / state | Audio behavior | Spatial / mix behavior | Required player information |
|---|---|---|---|
| Voice Under the Keel | Familiar voice-like contour with repeated interval | Starts far; bearing shifts between repetitions | Source is uncertain; compare bearings |
| Bells Beneath the Field | Low, water-muted bell with long decay | Very distant and partially masked by surf | Bell repeats on tide window |
| Current That Guides | Soft wake / pressure rush; no monster vocalization | Moves beside boat then away | Current direction is changing |
| Sleeper investigating | Infrasonic-like tonal bed (audible upper harmonics), faint chain resonance | Outside camera at first; low volume | Something heard the engine / ping |
| Sleeper warning | Two short low pulses, water displaced near one side | Directional cue plus caption | Threat is approaching from indicated side |
| Sleeper attack telegraph | Distinct rising scrape / pressure swell over 2.2–3.0 s | Brief ambient ducking | Immediate evasive action is possible |
| Sleeper impact | Hull transient, water burst, equipment rattle | Impact pan then central damage UI | Hull or equipment has been hit |
| Reef That Answers | Ping followed by a delayed / shifted return | Return appears in waveform UI and sound position | Echo bearing is unreliable |
| Shoreline Warning | Damaged buoy rhythm with radio hiss | Alternates between radio and physical source | Do not trust the voice until corroborated |
| Pearl at the Gate | Hollow resonance when the correct stone is scanned | Centered on the structure | A cavity / chamber is present |
| City Without a Shore | Deep muted return and distant structural creak | Very low density, no jump-scare hit | Built geometry may be submerged |
| Bell at the Turning Tide | Repeating bell sequence at tide boundary | Intensity varies with tide / position | The event window is active |
| Long Call | Long-form animal vocalization, repeated phrase | Broad, distant stereo field; mono caption equivalent | Wildlife call is present; not automatically hostile |

## Sound-event API fields

Minimum event data:
- `EventId`
- `SourceId`
- `Category` (Engine, RigPing, Collision, Horn, Wildlife, Anomaly, Threat, Navigation)
- `Position`
- `Loudness` (0–1 gameplay scalar)
- `FrequencyBand`
- `Duration`
- `SimulationTick`
- `OpportunityId`

The same event may play one audio clip and be consumed by several AI listeners. Do not emit multiple gameplay events for each audio loop iteration.

## Sleeper attack cue timing

- Warning A: distant two-pulse motif; begins when stalking starts.
- Warning B: directional water displacement; minimum 2.2 seconds before impact.
- Commitment: creature turns into its attack lane; its movement is visible through surface wake.
- Impact: short hull transient; do not sustain loud noise after impact.
- Recovery: scrape and wake travel away; cue stops before the next attack can be considered.

## Caption and mono requirements

Use descriptive non-spoiler captions: `[deep water pulses behind the boat]`, `[a bell rings beneath the water]`, `[long animal call, distant]`. Do not caption unknown sounds as a named monster. Critical direction cues need a non-audio indicator. Test with mono playback, low volume, and dynamic-range compression.

## Implementation checks

- The same SoundEvent ID is handled once per listener.
- Audio clips never mutate gameplay state.
- State transitions send one semantic event.
- Attack telegraph is distinct from ordinary engine / ocean ambience.
- Audio occlusion and volume changes do not modify encounter RNG.
