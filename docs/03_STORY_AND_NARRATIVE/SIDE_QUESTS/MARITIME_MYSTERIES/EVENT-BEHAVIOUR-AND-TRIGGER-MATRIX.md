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

## Legendary roaming encounter matrix

These are optional, rare world encounters, not all required quests. Probabilities are per eligible opportunity and remain subject to hard gates, individual cooldown, and the global legendary-event lock. The complete dossier is [Legendary Maritime Encounters](../../../12_AI_AND_SIMULATION/LEGENDARY-ENCOUNTER-SCHEDULER.md).

| Encounter | Region / bounds | Opportunity and trigger | Starting chance / schedule | Outcome and escape |
|---|---|---|---:|---|
| White Whale | Outer Shoals — Outer Wall / Last Anchorage | Dawn or dusk pelagic transit with whale-call activity | 2.5% per eligible transit | Usually passes harmlessly; crowding its surfacing path can cause a damaging wake. Turn parallel and stop closing distance. |
| Hafgufa | Outer Shoals — pelagic aggregation zone | Dense baitfish convergence, stable current, dawn / overcast surface-feeding window | 1.0% per eligible aggregation boundary | Observe from outside its feeding arc; if too close, turn laterally and escape before the feeding aperture closes. |
| Deep Arms | Blackrock Reach — Blue Maw / Undercliff | Night, post-storm surge, repeated Rig pulses or anchor noise | 0.8% per eligible cave approach | May seize hull or line; abandon anchor / salvage line and use the outward current. |
| Lusca-like Cave Hunter | Blackrock Reach — Blue Maw inner marker | Slack-to-outgoing cave-current transition and player crosses inner boundary | 1.2% per eligible crossing | Conditional ambush; cut attached lines and leave toward open water. |
| Twin Passage | Blackrock Reach — Needle Gate / Roaring Narrows | Spring-tide turn with opposing currents and cliff-side route entry | 1.5–2.0% per eligible passage | Choose a route based on foam and buoy rhythm; both lanes cannot become unavoidable kill zones. |
| Flying Dutchman | Drowned Parish outer channel / Outer Shoals | Night fog, weak radio, false lighthouse bearing | 1.0% per eligible night transit | Does not attack; chasing can cause grounding or fuel loss. Reacquire verified buoy / chart bearings. |
| Davy's Locker | Drowned Parish wreck fields / Last Anchorage | Salvage starts during down-current reversal or abnormal line tension | 2.0% per eligible salvage start | Release winch, cut entangled line, disengage engine if wrapped. Sinking is possible if trapped. |
| Umibōzu-like Black Horizon | Hollow Trench — Outer Wall | Calm night after a storm, listening window, prior distorted horizon clue | 0.6% per eligible night listen | Keep distance and hold a verified bearing; approaching a false horizon may trigger a lethal wave / current. |
| Bake-kujira | Outer Shoals — Bone Orchard | Night watch, whale-fall activity, unusual scavenger congregation | 0.8% per eligible watch | Omen / sighting; following it into deep water creates navigation exposure. |
| Bakunawa lunar event | Archipelago-wide; most visible from Greyhaven / Shallows | Authored rare eclipse calendar window | Calendar-gated, not a daily random roll | Tide / current anomaly; retreat to sheltered water and avoid narrow channels during peak. |
| Kataw-like sighting | Shallows — reef channels / seagrass edge | Moonless bioluminescent night, calm water, low engine noise | 1.5% per eligible night watch | Figures flee; following them without charting can lead into reef hazards. |
| Channel Guardian motif | Shallows / Blackrock marked passage | Adverse tide and prior local warning / nursery-respect flag | 1.0% per eligible passage | May mark a safe lane; leave the marked site if defensive current or wake appears. |
| Serpent Current omen | Greyhaven outer harbor / Shallows reef edge | Calm-before-storm window after restoration / community-story flag | 1.0% per eligible weather window | Protective sign; follow toward shelter rather than treating it as an enemy. |
| Leviathan Shadow | Hollow Trench — deep slope / Outer Wall | Late-game access and three independent deep acoustic clues | 0.1% per eligible expedition | Non-targetable; stay above the contour and cease pings. Pursuit can be fatal. |
| Jörmungandr Encircling Wake | Outer Shoals ↔ Hollow Trench margin | Endgame flag, extreme spring tide, current anomalies reported in three regions | 0.03% per eligible mythic tide event | Avoid crossing the current ring except at a verified slack lane. |
| Sea-Serpent / Oarfish sighting | Outer Shoals — upwelling / surface convergence | Deep upwelling and distant surface movement at dusk | 3.0% per eligible survey | Harmless sighting or real rare animal; no mandatory attack. |

### Encounter fairness

- No lethal encounter starts from an immediate-range off-screen spawn.
- At least two warning channels precede lethal contact.
- One valid escape lane exists when a lethal event first becomes active.
- The player can use heading, engine, anchor, current, shelter, and cargo-abandonment decisions to improve survival.
- A last-chance rescue check may save a trapped player, but is state-dependent and never the sole fairness measure.
- Death applies the project's established recovery / cargo / repair consequences; it does not routinely delete the save.
- These events cannot appear inside safe harbour or during a mandatory quest transition.
- Legends remain optional world encounters unless a quest explicitly makes a clue deterministic.
