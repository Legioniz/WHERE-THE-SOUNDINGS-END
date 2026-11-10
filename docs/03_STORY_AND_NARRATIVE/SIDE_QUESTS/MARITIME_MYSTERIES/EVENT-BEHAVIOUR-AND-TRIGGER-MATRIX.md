# Maritime Mysteries — Runtime Behaviour and Trigger Matrix

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready quest design specification  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Global rules

- An event is evaluated at a named opportunity boundary, never on every frame.
- Quest-critical evidence is deterministic or has a guaranteed fallback.
- Optional ambient variants may use RNG.
- Only The Sleeper is currently Hostile. Other entries are anomalies, wildlife, humans, static ruins, or environmental hazards.
- The elevated camera does not impose a first-person player vision cone. AI visual FOV, camera visibility, fog, and audio perception are separate systems.
- All values below are prototype starting points. Tune them in a playable vertical slice.

## Quest event matrix

| Quest | Trigger boundary | Initial chance / rule | Runtime behaviour | Attack / chase |
|---|---|---|---|---|
| The Voice Under the Keel | Eligible Rig deployment in Outer Shoals | 18% per valid deployment; guaranteed clue after 3 failed opportunities | Voice-like echo changes bearing; compare against known buoys | None; it is an acoustic anomaly |
| Bells Beneath the Field | Tide boundary while quest is active and at the ruin bounds | Deterministic in the window; optional second echo 35% per listen | Bell rings, Rig reveals hollow geometry | None; only mapped reef / wreck collision |
| The Current That Guides | Current update / authored tide state inside shoal corridor | 25% per eligible tide transition; quest path guarantees one observation | Surface wake and current lane appear; follow or mark it | No attack; wildlife does not pursue |
| The Sleeper's Lullaby | High-noise opportunity in Hollow Trench | 12% per eligible high-noise slot; tutorial encounter guaranteed once | Investigate → warning → stalk → pursuit → one strike → retreat | Yes; one telegraphed hull strike, bounded chase |
| The Price in the Pool | Dawn market refresh after prior fishing pressure / coin clue | Deterministic once clue flags are set; coin glint variant 20% per eligible dawn | Pool state and stock records reveal trade-off | None; social / ecological consequence |
| The Channel Guardian | Adverse tide transition while inside channel bounds | 20% per eligible tide transition; hazard state deterministic once active | Lateral current force and unstable buoy rhythm; player learns safe heading | No AI chase or creature attack; collision/current hazard only |
| The Shoreline Warning | Quest start + missing-deckhand last-known-route search | Deterministic rescue event; false-radio variant 30% per search point | Radio / buoy mismatch; player triangulates cove | None; human rescue and fog navigation |
| The Pearl at the Gate | Correct sonar scan of gate structure | Deterministic when scan geometry and quest flag match | Hollow chamber response; recover inscription plate | No creature; optional timed surge is an environmental hazard |
| The Reef That Answers | Rig ping inside reef quest volume | 25% per eligible ping for shifted echo; mandatory pattern guaranteed after 3 pings | Two echoes return from different bearings | None; reef collision risk with visible / instrument warning |
| The City Without a Shore | Survey visit after archive chart clue | 100% for authored evidence points; optional loose artifact 15% | Structural pattern, tile, household object; cautious evidence report | None; no hidden monster or surprise collapse |
| The Bell at the Turning Tide | Tide reaches authored turning phase on event day | Deterministic on window; audio variation seed controls motif | Bell sequence plays; player records and triangulates | None; tide / route limits only |
| The Long Call | Night listening slot at trench edge during wildlife window | 15% per eligible night slot; quest evidence guaranteed after two listens | Whale-like call, bearing comparison, one anomalous repeated phrase | No attack or chase; whale/wildlife remains non-hostile |

## Shared trigger payload

Every quest event should expose:
- `QuestId`
- `EventId`
- `OpportunityId`
- `RequiredFlags`
- `RegionId`
- `BoundsId`
- `TimeWindow`
- `Tide / Weather / Depth Gates`
- `Probability` (if any)
- `Cooldown`
- `WarningCue`
- `ResolutionFlags`
- `SaveKey`

## Player-facing fairness

- Tell the player why an event may be available: NPC rumor, chart mark, weather/tide note, recorded sound, or discovered object.
- A required clue cannot be lost because an ambient RNG roll failed.
- Retreat is always allowed unless an explicit authored story sequence temporarily restricts it.
- Failed optional events can return on a later voyage with a different clue path.
- No attack begins from an off-screen spawn at immediate range.
