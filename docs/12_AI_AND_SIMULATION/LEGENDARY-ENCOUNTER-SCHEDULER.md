# Legendary Maritime Encounters — Appearance, Trigger, and Survival System

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed encounter design baseline — all probabilities and timings require playtesting  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10  
> Scope: Rare, location-bound appearances inspired by maritime legends, including harmless sightings, environmental omens, dangerous approaches, and potentially lethal encounters

## 1. Design objective

The legendary figures and phenomena documented in [Ocean Legends and Sea Monsters](../02_LORE_AND_WORLD/OCEAN-LEGENDS-AND-SEA-MONSTERS.md) should occasionally enter the playable world. They must not be permanent roaming enemies or predictable boss fights. A player may travel for many days without seeing one, hear about an encounter from a fisher who cannot prove it, then encounter something that appears harmless—or something that can destroy the boat and kill the player.

A legendary encounter can be:
- **Omen:** the world changes before anything is visible.
- **Sighting:** a distant shape, sound, wake, or silhouette; it may be harmless.
- **Contact:** the player enters the entity's active space and receives a direct reaction.
- **Hazard:** current, suction, surf, collision, or an entity's movement threatens the boat.
- **Predation / lethal contact:** the player is in a kill zone and may die if no escape succeeds.
- **Aftermath:** lost time, altered wildlife, wreckage, changed routes, rumors, or evidence.

Not every legend becomes a literal monster. Some appear as weather, a current, a ship silhouette, a lunar event, a guardian sign, or a disputed sighting. A culturally significant being must not be reduced to a boss, loot source, or generic hostile animal.

**This is a design specification only. It does not claim that legendary encounters have been implemented in Unity.**

## 2. Global encounter contract

All legendary events use the existing World Simulation and Threat Director. Do not build a second independent random spawner.

### 2.1 Opportunity boundaries

An event may be evaluated only at a meaningful opportunity:
- entering a named fishing ground or geographic event volume;
- crossing a voyage-distance / elapsed-game-time segment;
- reaching a tide transition or current reversal;
- entering a named weather and visibility window;
- deploying or pulsing the Sounding Rig;
- beginning a salvage attempt or applying high winch / anchor tension;
- entering a lunar / seasonal event window;
- completing a specific clue, rumor, or world-state flag.

Never roll per frame, per second, or every time a scene reloads. Each evaluation receives a stable `OpportunityId`; its result is persisted.

### 2.2 Eligibility gates

Every event must pass all applicable hard gates before a probability roll:
1. Correct region, named fishing ground, and spawn anchor.
2. Correct depth band and navigable water volume.
3. Required time, tide, weather, current, season, and visibility.
4. Required rumor / clue / progression flags, if any.
5. No safe-harbour, cutscene, dialogue lock, or disabled-gameplay state.
6. No other major legendary encounter or active principal hostile encounter.
7. Individual cooldown and global legendary-event cooldown expired.
8. The player is not already in an unavoidable sinking / death sequence.
9. At least one authored warning and a physically valid escape opportunity exists, unless this is an explicitly authored no-control story scene.

If no candidate passes, the world continues with ordinary ecology and ambient events.

### 2.3 Rarity and cooldowns

The probabilities below are starting values **per eligible opportunity**, not per minute and not guarantees. A 1% chance on an eligible voyage segment should remain rare. A candidate may have several hard gates, so its real appearance rate is lower than its listed chance.

- One major legendary encounter at a time.
- Global legendary-encounter cooldown: 2–4 game days after a full encounter; 1 game day after a harmless distant sighting.
- Individual encounter cooldowns are longer and stored per entity / event ID.
- A failed opportunity is recorded; it cannot be rerolled by saving, loading, changing scene, or repeatedly deploying the same tool without reaching a new boundary.
- The same legendary should not appear repeatedly on consecutive trips solely because the player repeats one trigger action.
- The Sleeper remains the principal recurring hostile AI. Legendary encounters share the major-threat lock and may suppress or defer ordinary hostile selection; they do not silently stack with The Sleeper.

### 2.4 Rarity tiers

| Tier | Intended appearance rate | Role |
|---|---:|---|
| I — Rumor / ambient sign | 2–8% per eligible opportunity | Distant calls, unusual wakes, birds, buoy changes |
| II — Rare sighting | 0.5–3% per eligible opportunity | A visible or instrument-confirmed legendary presence |
| III — Exceptional encounter | 0.1–1% per eligible opportunity | Close approach, major hazard, or unique event |
| IV — Mythic event | Calendar / progression gated, often below 0.1% | Eclipse, abyssal silhouette, endgame-scale event |

Rates are tuning targets, not promises. An authored story sequence may guarantee a first encounter after its clue gates are satisfied.

## 3. Appearance and danger model

### 3.1 Encounter phases

Use an authored phase sequence. A typical event can be:

`Dormant Eligibility → Omen → Distant Appearance → Recognition / Warning → Contact → Escape Window → Resolution → Cooldown`

An entity may skip or alter a phase only when its dossier explicitly defines that behaviour. An attack cannot begin from an immediate-range off-screen spawn.

### 3.2 Warning channels

Each encounter should use at least two warning channels before a lethal state:
- surface behaviour: a pressure ring, unusual wake, sudden absence of waves, fish scattering, bird flight;
- acoustic behaviour: buoy interval changes, distorted engine echo, underwater call, radio drop;
- instrument behaviour: depth contour mismatch, current vector conflict, false range return;
- wildlife response: schools flee, predators go silent, scavengers gather, animals face one direction;
- boat feedback: rudder drag, line angle, engine load, lantern reflection, hull vibration;
- human knowledge: chart note, fisher rumor, lighthouse keeper warning, previous journal record.

A distant first sighting can be ambiguous. A lethal contact requires a more specific warning; do not make vague fog alone count as sufficient warning for instant death.

### 3.3 Risk classes

- **Harmless:** no player damage. A poor decision may waste time or fuel, but the sighting cannot directly sink the boat.
- **Conditional hazard:** it can damage the boat or cause a collision / grounding only if the player enters a warned danger volume.
- **Lethal hazard:** it can sink the boat or kill the player after an explicit warning and failed escape opportunity.
- **Predatory contact:** a living or anomalous entity can actively strike, seize, or drag the boat. It still obeys its authored range, cooldown, and telegraph.
- **Mythic / environmental:** the manifestation changes currents, visibility, sound, or navigability. It is not necessarily a targetable creature.

A harmless manifestation can become hazardous if the player follows it. It should not change from harmless to lethal without a new clue, threshold, or boundary the player can learn.

## 4. Escape and death rules

The sea should be capable of killing the player. Survival is not guaranteed once the boat enters the wrong place. However, death must result from a readable chain of events, not an arbitrary dice roll.

### 4.1 Escape factors

The final escape resolution may consider:
- reaction timing after the first clear warning;
- current heading and distance from the nearest escape lane;
- engine, rudder, and hull condition;
- fuel and available propulsion;
- anchor / salvage line state;
- sea state, tide, and current direction;
- installed safety equipment or relevant upgrades;
- a final bounded luck factor.

Player actions and preparation dominate the outcome. Luck can save a poorly positioned player, but it cannot make a badly damaged boat reliably outrun every entity. Conversely, a skilled player in a healthy boat should not be killed by a hidden final random check.

### 4.2 Escape bands

The following are design targets for a last-chance rescue / escape check after a player has already entered a lethal contact state. They are not the normal chance of surviving a properly handled encounter.

| Situation at last chance | Indicative escape chance |
|---|---:|
| Early reaction, correct escape route, boat operational | 65–90% |
| Correct response, but damaged boat or severe weather | 40–65% |
| Late response, poor heading, limited propulsion | 15–40% |
| Trapped in the kill zone, major hull failure, escape lane closing | 5–15% |
| Explicitly authored unavoidable story outcome | 0% only when previously disclosed by the story contract |

The check must be derived from the encounter state, not a free-floating luck roll. A late escape check is a final chance—not a replacement for telegraph, positioning, and player choice.

### 4.3 Death consequences

Death follows the game's established consequence philosophy, not routine permanent deletion of the save:
- recover at the last safe harbour / rescue point after an authored time loss;
- lose some unsecured catch, salvage, or cargo;
- pay for meaningful hull / equipment repairs;
- record the encounter in the journal if evidence was secured;
- preserve major quest flags and unique story progress;
- do not erase dozens of hours of progression by default.

An exceptional story may define a special outcome, but it must be authored and tested separately. Do not make permanent death the default for random legendary encounters.

### 4.4 Fairness and anti-frustration

- No lethal entity may spawn beside the boat without warning.
- No encounter may begin while the player is in safe harbour.
- At least one valid escape route must exist when the event becomes eligible.
- If a route closes during the event, the player must receive a new, readable alternative such as cutting a line, changing depth / heading, using shelter, or abandoning cargo.
- Critical quest evidence is not destroyed by a failed random encounter.
- Below 25% hull, ordinary optional legendary encounters prefer warning-only manifestations and reduce lethal selection weight. A specific authored story event may override this only with a clear setup.
- The encounter cannot be farmed by save-scumming, reloads, or repeated input.
- The entity's behaviour must match its ecological / mythic role: not all legends chase the player.

## 5. Legendary encounter location and trigger matrix

All named regions and sublocations remain provisional until the master bathymetric map is approved. The encounter name in this table identifies its legend inspiration; the shipped entity may use an original in-world name and silhouette.

| Legend / encounter | Main location | Eligible trigger | Initial chance / schedule | Default risk |
|---|---|---|---:|---|
| Mocha Dick / White Whale | Outer Shoals — Outer Wall, Last Anchorage | Dawn / dusk migration window, large whale-call activity, long voyage segment; more likely after a player repeatedly approaches or disturbs its route | 2.5% per eligible pelagic transit | Harmless at distance; conditional collision / wake hazard if chased or crowded |
| Hafgufa / Lyngbakr | Outer Shoals — open pelagic aggregation zone | Dense baitfish convergence, stable current, dawn or overcast surface-feeding window, quiet engine | 1.0% per eligible aggregation boundary | Usually harmless spectacle; lethal if the player enters the warned feeding / mouth zone |
| Kraken-like Deep Arms | Blackrock Reach — The Blue Maw / The Undercliff | Night, high swell or post-storm surge, deep cave edge, repeated Rig pulses or anchor noise | 0.8% per eligible cave approach | Predatory contact; can seize or crush the boat |
| Lusca-like Cave Hunter | Blackrock Reach — The Blue Maw | Slack tide changing to an outgoing cave current; player crosses inner cave marker after warning traces | 1.2% per eligible inner-cave crossing | Lethal conditional ambush; does not pursue beyond cave boundary |
| Scylla-like Cliff Presence | Blackrock Reach — The Needle Gate / Widow's Teeth | Low visibility, surge aligned with cliff face, boat follows the inner rock line | 1.5% per eligible passage | Conditional collision / seizure hazard |
| Charybdis-like Current | Blackrock Reach — The Roaring Narrows / The Needle Gate | Spring-tide turn, opposing currents, player crosses the central convergence lane | 2.0% per eligible adverse tide passage | Environmental lethal hazard; may drag or capsize the boat |
| Flying Dutchman | Drowned Parish outer channel ↔ Outer Shoals | Night fog, weak radio / lighthouse bearing, voyage crossing a known phantom bearing | 1.0% per eligible night transit | Harmless apparition; chasing it can lead into grounding, fuel loss, or real hazards |
| Davy Jones's Locker | Drowned Parish — wreck fields / The Last Anchorage | Salvage line under unusual load, down-current reversal, repeated recovery attempts at the same wreck | 2.0% per eligible salvage start | Environmental lethal hazard; line entanglement and down-current can drag the boat |
| Umibōzu-like Black Horizon | Hollow Trench — Outer Wall / trench edge | Abnormally calm night after a storm, no ordinary wave sound, a valid listening window, and prior discovery of distorted horizon returns | 0.6% per eligible night listen | Appears passive; approach or answering its false signal can initiate a dangerous wave / pressure event |
| Bake-kujira / Ghost Whale | Outer Shoals — Bone Orchard / whale-fall corridor | Night listening window, whale-fall activity, unusual scavenger aggregation, prior Long Call clue | 0.8% per eligible watch | Harmless omen at distance; following it into deep water can create lethal navigation exposure |
| Bakunawa / Moon-Eater Event | Archipelago-wide coastal horizon; most visible from Greyhaven and the Shallows | Authored rare lunar-eclipse calendar window; strongest response during high tide and exposed coastal travel | Calendar-gated; no ordinary random roll once the event is scheduled | Mythic environmental event; may distort tide / current and strand an unprepared boat |
| Kataw-like Sea-People Sighting | The Shallows — reef channels / seagrass edge | Moonless bioluminescent night, calm water, low engine noise, healthy reef activity | 1.5% per eligible night watch | Usually harmless; figures flee when illuminated or approached; following them without chart checks can lead to reef hazards |
| Channel Guardian / Taniwha-inspired Presence | The Shallows ↔ Blackrock Reach — marked channel / dangerous narrows | Player approaches during adverse tide; stronger chance after respecting a nursery closure or recording a local warning | 1.0% per eligible channel passage | Can guide the player to a safe line; defensive hazard only if the marked site is damaged or deliberately violated |
| Serpent Current / Ryūjin-inspired Omen | Greyhaven outer harbor / Shallows reef edge | Successful habitat restoration, safe return of catch / supply, calm weather before seasonal storm, and local ritual / story flag | 1.0% per eligible calm-before-storm window | Harmless or protective manifestation; marks safe water or warns of a weather change |
| Leviathan Shadow | Hollow Trench — The Outer Wall / deep slope | Late-game depth access, at least three independent deep acoustic clues, low ambient noise, no active major threat | 0.1% per eligible deep expedition | Mythic-scale environmental presence; not targetable; pursuing it can be fatal |
| Jörmungandr / Encircling Wake | Outer Shoals ↔ Hollow Trench archipelago margin | Endgame progression, rare extreme spring tide, current anomalies reported in three separated regions | 0.03% per eligible mythic tide event | Non-combat mythic event; dangerous current ring, no conventional boss fight |
| Sea-Serpent / Oarfish False Sighting | Outer Shoals — upwelling lines / surface convergence | Deep upwelling, floating debris line, distant surface movement at dusk | 3.0% per eligible surface survey | Harmless wildlife / uncertain sighting; some events resolve as real rare fish or debris |

These probabilities are not meant to imply equal global frequency. Each row's hard gates, per-entity cooldown, and global encounter lock reduce actual appearances.

## 6. Encounter dossiers and player outcomes

### 6.1 Mocha Dick — the White Whale

- **Behaviour:** a persistent large marine animal with an established migration corridor, feeding intervals, rest periods, injury history, and learned avoidance. It does not seek the player as prey.
- **Omen:** seabirds lift from the surface, whale calls move across bearings, and a pale back briefly surfaces beyond the visible school.
- **Harmless result:** it passes through the Outer Shoals and disappears. The player's log records the bearing.
- **Dangerous result:** if the player cuts across its surfacing path or crowds it, its body displacement creates a heavy wake and possible hull impact.
- **Escape:** stop closing distance, turn parallel to its path, reduce unnecessary engine noise, and avoid crossing ahead of its surfacing direction.
- **Failure:** hull damage, cargo loss, or capsize risk if the player persists while already damaged. It does not deliberately hunt a person.

### 6.2 Hafgufa — the Moving Feeding Ground

- **Behaviour:** a huge, slow-feeding presence that gathers fish around a feeding plume or open-mouth current. Fish schools are attracted or trapped by feeding conditions; the creature does not need to chase the boat.
- **Omen:** multiple schools converge in a shape that does not match current flow; birds circle a patch with no visible bait ball; the Sounding Rig returns a broad false-bottom contour.
- **Harmless result:** the player watches from outside the feeding arc and leaves.
- **Dangerous result:** the player enters the central feeding lane as the creature closes its mouth / feeding aperture.
- **Escape:** turn laterally away from the central convergence, follow the fish's outward escape direction, and apply propulsion before the closing phase.
- **Failure:** the boat is rolled, pulled under, or crushed by water displacement. A last-chance escape is possible only if the boat is not already trapped.

### 6.3 Kraken-like Deep Arms

- **Behaviour:** a territorial deep-water animal or anomaly that remains around the cave / trench structure. It attacks only after repeated disturbance or close encroachment; the arms are not permanently active.
- **Omen:** large sucker-like impressions on wreckage, a line tension change without current, scrape marks on a cave lip, and a low sound that moves against the wave direction.
- **Harmless result:** one or more limbs move below the boat, then withdraw.
- **Dangerous result:** an arm catches the hull, anchor, or salvage line.
- **Escape:** cut the anchor / line if necessary, stop pulling against the limb, angle away from the cave wall, and use the outward current. Do not reverse blindly into another arm.
- **Failure:** hull breach or entanglement can lead to sinking and player death.

### 6.4 Lusca-like Cave Hunter

- **Behaviour:** a cave ambusher with a fixed retreat boundary. It is not a global roaming threat. It may remain inside the Blue Maw for long periods.
- **Omen:** crabs and fish vanish from the cave mouth; shells are piled toward the inner wall; the cave current briefly reverses before the main surge.
- **Harmless result:** the player leaves before crossing the inner marker.
- **Dangerous result:** the hunter strikes from cover and attempts to drag the boat inward.
- **Escape:** cut attached lines, turn toward the open-water current, abandon salvage, and do not attempt to pull the creature into open sea.
- **Failure:** the boat can be dragged against cave structure or submerged in the inner pool.

### 6.5 Scylla-like cliff and Charybdis-like current

- **Behaviour:** a paired passage hazard. The cliff-side presence attacks a narrow route; the central current draws the vessel toward a rotating or converging flow. Their patterns alternate and can overlap only in a rare severe state.
- **Omen:** birds avoid the gap, foam lines form a spiral, and the buoys ring out of phase.
- **Choice:** take the cliff-side route at the cost of possible contact, or cross the current lane at risk of loss of control. The route must be learnable from observation.
- **Escape:** reduce sail / engine load as appropriate, keep the bow aligned with the selected current exit, and avoid correcting against the full lateral flow.
- **Failure:** impact, capsize, or vessel entrapment. The event must never close both routes without a visible story warning and fallback.

### 6.6 Flying Dutchman

- **Behaviour:** a distant phantom vessel or repeated visual bearing. It does not directly attack.
- **Omen:** an unlisted light, rigging silhouette, bell cadence, and a radio call using an obsolete channel.
- **Harmless result:** the vessel disappears when the player maintains a known buoy / lighthouse bearing.
- **Dangerous result:** chasing the ship draws the boat away from marked water and toward a reef, wreck field, or storm boundary.
- **Escape:** compare the compass, chart, buoy bearings, and actual current; disregard the false route and return to the last verified marker.
- **Failure:** grounding, fuel exhaustion, or entering another event's eligibility volume while unprepared.

### 6.7 Davy Jones's Locker

- **Behaviour:** a wreck-site down-current and entanglement event. It is an environmental hazard, not a literal humanoid pirate.
- **Omen:** the salvage line points vertically despite a horizontal current, debris rises from the wreck, and line tension increases when the winch is released.
- **Dangerous result:** a line wraps a propeller or pulls the boat toward a wreck cavity.
- **Escape:** release the winch, cut or abandon the line, kill the engine if the propeller is wrapped, and use the current to clear the structure.
- **Failure:** the boat loses propulsion or is dragged into the wreck. A rescue check may be the final chance to survive.

### 6.8 Umibōzu-like Black Horizon

- **Behaviour:** a large dark manifestation that distorts horizon and wave perception. It may remain passive unless the player approaches, answers the false call, or crosses its boundary.
- **Omen:** the sea becomes too quiet, the horizon rises without a swell, the lighthouse reflection appears at the wrong bearing, and the radio returns a voice with no source.
- **Harmless result:** the player keeps distance, uses a verified bearing, and the shape withdraws.
- **Dangerous result:** approaching the silhouette causes a sudden wave wall or lateral current.
- **Escape:** hold a known compass / buoy bearing, turn away from the dark boundary, and avoid steering toward the apparent light.
- **Failure:** capsize, hull breach, or being carried into the trench edge.

### 6.9 Bake-kujira — the Ghost Whale

- **Behaviour:** an omen, not a hunter. It appears with unusual scavenger and seabird movement around the Bone Orchard.
- **Omen:** a pale skeletal-looking back, calls with a missing harmonic, birds flying low over empty water, and fish leaving the area.
- **Harmless result:** the player records the sighting; local wildlife shifts and a weather or stock anomaly may follow.
- **Dangerous result:** following it sends the boat toward deep water during poor visibility.
- **Escape:** treat it as a warning, not a target; return along known depth contours and avoid the whale-fall current.
- **Failure:** the player becomes exposed to deep-water hazards or another event while navigation conditions are degraded.

### 6.10 Bakunawa — the Moon-Eater Event

- **Behaviour:** a rare lunar and tidal manifestation. A serpent-like shadow may cross the moon's reflection; whether this is a visible creature, an optical event, or an anomaly remains ambiguous.
- **Trigger:** an authored eclipse window, not random daily chance. The event is announced through calendar, NPC, and environmental signs.
- **Harmless result:** the player watches from sheltered water and learns a tide / current pattern.
- **Dangerous result:** a tide surge or current reversal forms along exposed coast and can carry a boat into shallow reef or rock.
- **Escape:** retreat to a mapped sheltered bay, avoid narrow channels during the peak, and wait for the tide boundary to pass.
- **Failure:** grounding or hull damage; death is possible if the player enters the surge zone with a damaged vessel.
- **Cultural handling:** preserve the fact that Bakunawa stories vary among Philippine communities. Do not present the documented Abaknon account as the only Filipino version.

### 6.11 Kataw-like Sea-People Sighting

- **Behaviour:** distant humanoid silhouettes or figures resting near reef edges. They are not automatically hostile and should not chase the boat.
- **Omen:** bioluminescent trail, synchronized surfacing, brief hand-like shape above water, and fish that do not scatter.
- **Harmless result:** the figures disappear below the reef ledge and leave an unusual route marker.
- **Dangerous result:** only if the player follows without charting the reef; the risk is navigation, not an attack by the figures.
- **Escape:** stop pursuit, use the chart / depth contour, and return through the open channel.
- **Cultural handling:** this is an original fictionalized sea-person sighting informed by a specific historical account, not a claim that all Visayan communities share one belief about kataw.

### 6.12 Channel Guardian — place-guardian motif

- **Behaviour:** an original guardian manifestation associated with a named dangerous passage. It may warn the player away from a current or briefly mark the safe route.
- **Omen:** a large wake moves against current, a buoy rings in the wrong rhythm, and small fish form a corridor.
- **Harmless / protective result:** the player follows the safe current seam and avoids a collision.
- **Dangerous result:** if the player damages a marked nursery site or repeatedly anchors inside a forbidden structure, the passage becomes hostile through current, wake, or physical obstruction.
- **Escape:** disengage from the site, clear the marked boundary, and stop repeated disturbance.
- **Cultural handling:** the in-game entity should have an original name and design. Taniwha is a living Māori tradition and must not be used as a generic hostile monster without specific consultation.

### 6.13 Serpent Current — sea-kami / guardian omen

- **Behaviour:** a luminous current-line or serpent-shaped reflection that marks safe water, warns of weather, or guides a boat home. It is not a combat entity.
- **Trigger:** a calm-before-storm weather window, completed local restoration, and a relevant community-story flag.
- **Harmless result:** it draws a visible line toward sheltered water.
- **Dangerous result:** ignoring the warning leaves the player exposed to an approaching storm; the manifestation itself does not attack.
- **Cultural handling:** this is an original motif, not a literal depiction of Ryūjin or Watatsumi. Local Japanese sea-kami traditions are distinct and culturally meaningful.

### 6.14 Leviathan Shadow

- **Behaviour:** an enormous deep-water silhouette that displaces currents and sound. It is not a conventional enemy, cannot be killed, caught, or sold.
- **Omen:** multiple sounding returns fail to agree; nearby creatures cease activity; the hull receives a pressure-like vibration with no nearby contact.
- **Harmless result:** the player remains outside the deep contour and watches the body pass beneath the vessel.
- **Dangerous result:** following the silhouette beyond the safe depth contour brings the boat into a severe current / pressure hazard.
- **Escape:** turn toward the upper slope, keep the depth contour on one side, and cease Rig pulses.
- **Failure:** loss of control, sinking, and potential player death.

### 6.15 Jörmungandr — Encircling Wake

- **Behaviour:** an endgame-scale current / wake event spanning multiple distant regions. The full creature is never presented in one view; a moving boundary or loop of disturbed water may surround the archipelago.
- **Trigger:** endgame progression, a rare extreme spring-tide event, and consistent reports from at least three regions.
- **Harmless result:** the player observes the moving boundary from safe waters and records its direction.
- **Dangerous result:** crossing the current ring at the wrong phase can rotate the vessel or force it off its route.
- **Escape:** cross only through the temporary slack lane identified by buoy timing and current readings; otherwise shelter until it passes.
- **Failure:** being carried into an uncharted hazard region. It is not a killable boss.

## 7. Legends that should remain contextual until location / cultural research supports a literal encounter

Some researched beings belong to cold-water settings, freshwater river systems, or living traditions that should not be transplanted casually into the game's currently provisional warm-water island arc.

| Legend | Current treatment | Reason |
|---|---|---|
| Selkies | Shore stories, human mystery, or distant seal-like sighting only if the climate and species roster support it | North Atlantic seal-person tradition; no reason to force a tropical seal population |
| Qalupalik / Qallupilluit | Folklore reference only; no direct spawn in the current warm-water setting | Inuit shoreline / sea-ice tradition; direct depiction requires community-specific consultation |
| Akkorokamui | Archive / rumor reference until the setting or a respectful fictional adaptation is approved | Ainu-associated regional tradition; do not use as generic octopus boss |
| Mami Wata | Community story and exchange / obligation motif, not a generic mermaid enemy | Diverse religious traditions; requires exact community attribution |
| Yacumama | Only relevant if a freshwater river system and specific source tradition are added | Amazonian river tradition, not an ocean species |
| Leviathan / Lotan | Thematic shadow and environmental scale, not a routine animal | Religious / mythic figure; avoid flattening into a collectible boss |

A contextual legend can still appear through a book, ship name, rumor, chart, recorded testimony, or ambiguous light. A literal in-world manifestation requires its own reviewed dossier.

## 8. Sounding Rig and evidence interaction

The Sounding Rig can help the player identify and escape legendary encounters, but it is not a perfect detector.

- A normal survey can reveal a mismatch in depth, current, or school movement.
- Repeated pings may increase an entity's curiosity or event pressure only where its dossier explicitly says so.
- Some entities create plausible false returns; the game must provide a second way to verify the clue, such as a buoy, current seam, surface wake, or known landmark.
- A Rig reading should not be the sole warning before instant lethal damage.
- Recording an event is optional unless a quest has secured its mandatory fallback.
- The player can stop pinging, reduce throttle, cut anchor / salvage lines, and retreat when appropriate.

Journal evidence should distinguish: Rumor → Unverified Sign → Direct Sighting → Instrument Corroboration → Repeated Pattern. Recording a sighting does not prove the folklore's literal interpretation.

## 9. Integration with world simulation and Threat Director

The existing Threat Director remains the single selection authority. It receives legendary candidates with:
- `LegendEventId`
- `OpportunityId`
- `RegionId` and `BoundsId`
- `DepthBand`
- `Time / Tide / Weather / Current Gates`
- `RequiredStoryFlags`
- `BaseChance` and `ProbabilityCap`
- `IndividualCooldown` and `GlobalLegendaryCooldown`
- `WarningCueSet`
- `RiskClass`
- `EscapeAnchors`
- `ResolutionTable`
- `SaveKey` and stable seed tuple

Major legendary events and The Sleeper share a single major-threat lock. Harmless ambient signs may occur during an ordinary threat cooldown only if they cannot escalate into contact. A legend event cannot start a lethal sequence when another major threat is active.

When the region is inactive, do not simulate the encounter chase. Either defer the opportunity until the player returns or resolve a bounded off-screen omen that cannot damage the player or destroy quest evidence.

## 10. Data architecture

Recommended authoring data:
- `LegendEncounterDefinition`: identity, inspiration, classification, region, bounds, risk class, and canon / cultural-use status.
- `LegendOpportunityDefinition`: boundary type, time / tide / weather / depth gates, base chance, and cooldown.
- `LegendPhaseDefinition`: omen, appearance, warning, contact, escape, and aftermath cues.
- `LegendEscapeProfile`: escape anchors, required controls, hull / engine checks, and last-chance rules.
- `LegendEvidenceDefinition`: audio, visual, instrument, wildlife, and journal evidence.
- `LegendResolutionDefinition`: harmless withdrawal, escaped, vessel damage, sinking, rescue, or death.

Mutable state belongs in save records:
- active legend event ID and phase;
- opportunity ID and evaluation result;
- last appearance / last contact day;
- individual and global cooldown end time;
- warning cues already delivered;
- escape window and route availability;
- boat state at contact;
- evidence collected and journal stage;
- resolution and death / rescue outcome.

ScriptableObjects hold immutable definitions only; do not mutate shared asset fields during play.

## 11. Acceptance tests

- A legend appears only in its authored region / depth / event bounds.
- Every event uses a named opportunity boundary and cannot reroll on scene reload.
- Individual and global cooldowns prevent repetitive appearances.
- At most one major legendary or principal hostile encounter can become active.
- Harmless signs cannot unexpectedly escalate into lethal contact without a new warning.
- Lethal contact always has at least two warning channels and a valid escape opportunity before the kill zone closes.
- At least one possible escape remains when a lethal encounter first becomes active.
- A player can survive through good preparation and correct action; luck is a secondary factor.
- A poorly positioned / damaged player can die even after a final rescue chance is offered.
- A death applies established recovery, loss, and repair consequences without routine permanent save deletion.
- Legends do not become catchable seafood, ordinary sale stock, or repeatable research payouts unless a separate approved document explicitly permits a harmless trace sample.
- Culturally significant traditions are not automatically converted into hostile boss fights.
- A rumor or sighting never becomes proof of a supernatural entity in the journal without corroboration.
- Inactive regions do not run lethal encounter simulation off-screen.
- Legendary events cannot interrupt safe harbour or mandatory quest transitions.

## 12. Related documents

- [Ocean Legends and Sea Monsters](../02_LORE_AND_WORLD/OCEAN-LEGENDS-AND-SEA-MONSTERS.md)
- [World Simulation and Event Scheduler](../12_AI_AND_SIMULATION/WORLD-SIMULATION.md)
- [Threat Director](../12_AI_AND_SIMULATION/THREAT-DIRECTOR.md)
- [Maritime Mysteries — Event Behaviour and Trigger Matrix](../03_STORY_AND_NARRATIVE/SIDE_QUESTS/MARITIME_MYSTERIES/EVENT-BEHAVIOUR-AND-TRIGGER-MATRIX.md)
- [Creature Life Cycles and Ecological Behaviour](../12_AI_AND_SIMULATION/CREATURE-LIFE-CYCLES-AND-ECOLOGY.md)
- [Regional Ecology and Creature Distribution](../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md)
- [Game Design Document](../01_GAME_DESIGN/GAME-DESIGN-DOCUMENT.md)

All rates, locations, and direct manifestations remain proposed until the world geography, cultural review, and playable encounter tests are approved.
