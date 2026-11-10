# Legendary Encounters — Visual, Behaviour, and Discoverable Lore Dossiers

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed art / narrative / encounter specification — non-canon until reviewed  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10  
> Scope: Detailed silhouettes, anatomy, materials, scale, movement, sound, environmental presentation, encounter behaviour, discoverable documents, field notes, and journal interpretation for legendary maritime events

## 1. Purpose

The [Legendary Encounter Scheduler](LEGENDARY-ENCOUNTER-SCHEDULER.md) defines when and where rare legendary encounters can happen, how dangerous they are, and how they can be escaped. This document defines what the player actually sees, hears, finds, reads, and remembers.

The goal is not to fill the sea with boss monsters. A legendary event should feel like a rare discovery inside a living maritime world. It may be an animal, an apparition, a current, a phantom vessel, a place-guardian motif, a lunar event, or a sighting that could have an ordinary explanation.

Every encounter dossier therefore has five layers:
1. **Visual identity:** silhouette, scale, body plan, surface materials, distinctive details, lighting, and animation.
2. **Behaviour identity:** how it moves, rests, feeds, reacts, retreats, or changes the environment.
3. **Encounter identity:** the clues before appearance, the player's possible decisions, the danger boundary, and the resolution.
4. **Archive identity:** documents, charts, recordings, inscriptions, personal effects, and witness accounts the player can discover.
5. **Interpretation identity:** what the player can reasonably conclude, what remains disputed, and how the journal records uncertainty.

**Implementation status:** this is a detailed production specification. It is not an implemented Unity art asset, encounter controller, journal, or document pickup system.

## 2. Visual direction for legendary encounters

### 2.1 The game must preserve uncertainty

The 2.5D camera means the player usually sees a limited slice of the sea. Use that limitation deliberately:
- Reveal a back, fin, limb, light, wake, or silhouette before revealing a whole body.
- Keep the full scale difficult to estimate. Compare the entity with the player's boat, a buoy, a lighthouse, a wreck mast, or a known island cliff.
- Avoid constant close-up views. Distance, fog, rain, spray, darkness, and interrupted line of sight are part of the art direction.
- Do not use a glowing outline or a permanent health bar. Legendary encounters are not ordinary combat targets.
- Keep the silhouette readable against the water even when surface detail is subtle.
- Give each entity one or two memorable identifying features, not a pile of random spikes, teeth, tentacles, and glowing eyes.

### 2.2 Surface and material language

Use the following material logic:
- **Living animal:** skin, scars, folds, parasites, barnacles, old wounds, natural wet highlights, and plausible buoyancy.
- **Anomalous body:** real organic materials with one precise impossible detail, such as a reflection showing a different movement or a call that arrives before its source.
- **Apparition:** lower contrast than a solid object, partial transparency only at selected edges, no uniform blue ghost glow, and an interaction with fog / water that makes it difficult to tell whether it is in front of or behind the surface.
- **Environmental event:** the water, foam, current, birds, debris, and sound create the entity's silhouette. The player may never see a creature at all.
- **Phantom vessel:** readable rigging and ship shape at distance, but missing or contradictory detail when the player approaches.

### 2.3 Scale without a measurement popup

Do not announce exact sizes when an entity appears. Let the player estimate scale through:
- a back longer than several boat lengths;
- a fin taller than the wheelhouse;
- tentacles crossing more than one camera frame;
- a wake that reaches a buoy before the body does;
- a shadow covering an entire wreck field;
- a ship silhouette whose mast appears above a fog bank but whose hull never resolves.

Internal dimensions below are production estimates for camera, collision, and animation planning. They are not measurements from folklore.

### 2.4 Presentation budget

A rare event must still be practical for a small team:
- Build one hero silhouette and a small set of reusable movement / surface-break animations.
- Use wakes, foam, particles, sound, and camera framing to imply scale.
- Use impostors, layered 2.5D silhouettes, depth-offset sprites, or low-poly mesh cards as appropriate.
- Keep far-field sightings low detail. Reserve the most expensive animation for the brief moment the player is close enough to identify it.
- Use fixed camera-safe composition anchors so a large body does not disappear behind UI or screen edges.
- Give colour and brightness variants a strict authored range; avoid random material changes that make the creature unrecognizable.

## 3. Discoverable lore and field-record system

The player should be able to learn about an encounter before, during, or after seeing it. No single collectible should explain everything.

### 3.1 Evidence stages

Every encounter uses the following optional evidence ladder:

| Stage | Journal label | Example | What it means |
|---|---|---|---|
| 0 | Rumour | Tavern account, dockside warning, radio gossip | The player has heard a claim |
| 1 | Trace | Scarred buoy, broken mast, unusual scale marks, dead sonar recorder | A physical or recorded sign exists |
| 2 | Testimony | Ship log, personal letter, witness statement, rescue interview | Someone claims to have experienced it |
| 3 | Direct sighting | Player personally observes an appearance | The player saw something, but may not know what it was |
| 4 | Corroboration | Separate records agree on time, location, movement, or sound | Multiple sources support part of the event |
| 5 | Pattern | Player connects recurring trigger, location, tide, or behaviour | A repeatable rule is recorded |

A player can reach Pattern without deciding that a supernatural interpretation is true. The journal may state: "The sightings share a route and timing. The cause remains unknown."

### 3.2 Types of discoverable records

- **Deck log:** a captain's dated entry; often short, practical, and incomplete.
- **Fishing ledger:** catches, size, weather, location, and unusual absences. Useful for linking legends to ecology and overfishing.
- **Coastguard / harbour report:** coordinates, vessel condition, missing crew, rescue details; generally avoids supernatural language.
- **Personal letter:** fear, grief, guilt, superstition, or family knowledge.
- **Chart annotation:** a scratched route, crossed-out depth, warning symbol, or handwritten name.
- **Sound cylinder / recorder / radio tape:** acoustic evidence that can be replayed and compared.
- **Newspaper clipping:** public rumor that may exaggerate or contradict official records.
- **Research folio:** later analysis, measurements, sketches, and competing explanations.
- **Memorial / grave / nameplate:** human aftermath rather than creature exposition.
- **Physical trace:** scale impression, sucker scar, strange line abrasion, bone, damaged buoy, or displaced wreckage.
- **Oral account:** NPC testimony that may change with trust, location, or the player's earlier questions.

### 3.3 Placement rules

Each legendary should have:
- At least one **pre-encounter clue** in a safe or lower-risk location.
- At least one **field clue** located near the encounter's bounds.
- At least one **post-encounter clue** that becomes meaningful only after a sighting or event flag.
- One **contradictory or uncertain account** so the player is not given a perfectly reliable exposition chain.
- A journal synthesis that separates evidence from interpretation.

Do not place every clue beside the creature. Use Greyhaven's archive, lighthouse rooms, boat cabins, wrecks, shrine-like community spaces only when culturally and contextually justified, abandoned survey stations, fish markets, and recovered personal effects.

### 3.4 Sample document voice

Documents should sound like different people, not like a single narrator explaining game mechanics. The fragments below are original fictional text drafts for the game.

**A practical deck log:**
> "03:40. Outer bearing changed twice. No engine fault. We were not drifting, though the buoy came up on the wrong side."

**A frightened personal note:**
> "Do not answer it because it uses your own call sign. I heard my brother on the radio. He was buried before the new lighthouse was built."

**A measured survey note:**
> "Three returns, same depth. The fourth had no bottom. Instrument checked dry and repeated. No fault found."

**A dismissive official report:**
> "Reported animal not confirmed. Crew exhausted. Surface conditions poor. Vessel damage consistent with collision against submerged structure."

The player should be able to disagree with each author.

### 3.5 Journal entry structure

Every entry stores:
- `EvidenceID`
- `LegendEventID`
- `RecordType`
- `FoundAtLocationID`
- `DiscoveryCondition`
- `ObservedDate` or source date
- `OriginalText`
- `ReliabilityBand`
- `EvidenceStage`
- `LinkedEvidenceIDs`
- `PlayerObservation`
- `OpenQuestions`
- `CulturalSourceNote` where relevant

Reliability is about the record's usefulness, not whether the author is a good person. A frightened sailor may accurately describe a sound but misidentify its source. An official report may be carefully written but omit an uncomfortable detail.

## 4. Legendary field dossiers

All dimensions, colour treatments, and movement speeds in this section are fictional production estimates. The folklore name identifies the inspiration; the final shipped creature or event may use an original in-world title.

---

## LEG-001 — The White Whale
**Inspiration:** Mocha Dick / Moby-Dick  
**Classification:** Living animal, exceptionally large and unusually pale  
**Primary area:** Outer Shoals — Outer Wall, Last Anchorage migration line  
**Risk:** Usually harmless; dangerous only through close approach, surfacing displacement, or deliberate harassment  
**Appearance tier:** Rare sighting / exceptional close encounter

### Visual identity
- **Silhouette:** a long, heavy sperm-whale shape; deep blunt head, narrow lower jaw, thick mid-body, tapering rear, and a small triangular-to-rounded dorsal hump rather than a tall shark fin.
- **Internal scale target:** approximately 20–23 metres for a mature named individual. The player is never shown this number during the encounter.
- **Colour:** weathered ivory to pale smoke-grey, never clean white. The dorsal surface has grey mottling; the underside is darker and stained.
- **Head:** broad block-like forehead, with subtle asymmetry. A left-offset blowhole produces a low, angled spout.
- **Scars:** three long healed rake marks behind the head, old rope grooves near the tail stock, a pale notch on the right fluke, and small circular scars from old impacts.
- **Eyes:** small, dark, wet, and difficult to see. No glowing eyes.
- **Surface detail:** skin folds, healed tissue, small barnacles near older scars, and natural specular highlights that break under the waves.

### Movement and sound
- It surfaces slowly with a long roll. The head appears first, then the back, then the tail.
- It can remain below water for a long quiet interval. Do not make it circle the player's boat for attention.
- A deep breath produces a short angled plume and a low exhalation; the tail lift is broad and heavy.
- Its calls are low, spaced knocks and clicks with long gaps. Use direction changes rather than a constant soundtrack.
- When it changes direction underwater, a pale moving shadow may cross below the boat without revealing the full animal.

### Behaviour and danger
- It follows a persistent Outer Shoals route and revisits feeding / resting areas over long in-game intervals.
- It avoids repeated engine noise and may alter its route if repeatedly approached.
- The main danger is the player cutting across its surfacing path or operating close to its tail.
- It may strike the water defensively with its fluke if crowded, producing a broad damaging wake. This is not a deliberate human hunt.
- The safe response is to stop closing distance, turn parallel to its course, and allow it to pass.

### Discoverable lore
1. **Greyhaven — Harbour Archive, Whaling Ledger 04.** An old ledger records one pale whale repeatedly sighted along the Outer Shoals. The entries contain no claim that it attacked anyone; they note that experienced crews refused to harpoon it after a disastrous attempt.
   > "White back at the outer line again. No catch entered. Mate says she carries old iron. I say the sea has already had enough from us."
2. **Last Anchorage — Cabin of the wrecked tender *Morrow*.** A damp deck log contains a sketch of a pale fluke with one missing corner. It records a close pass and a broken steering cable.
   > "It did not turn toward us. We turned toward it. The water lifted beneath the stern as if the whole sea had stood up."
3. **Outer Wall — Floating recorder case.** A salvageable audio cylinder repeats three low clicks, then a long silence, then a blow. The bearing matches the player's sighting.
4. **Greyhaven — Retired net-mender NPC.** A second-hand account claims it leads lost boats home. The NPC admits they never saw it themselves.

### Journal interpretation
The animal is real within the game world. Its legendary status comes from rarity, size, old scars, and human accounts. The journal should not claim that it is supernatural or that it deliberately protects sailors.

---

## LEG-002 — The Moving Feeding Ground
**Inspiration:** Hafgufa / Lyngbakr  
**Classification:** Vast pelagic filter-feeding animal or anomalous animal-like presence; exact classification unresolved  
**Primary area:** Outer Shoals — open pelagic aggregation zone  
**Risk:** Harmless at a distance; potentially lethal inside the feeding lane  
**Appearance tier:** Exceptional encounter

### Visual identity
- **Distant silhouette:** a low black-grey ridge among floating weed and surface foam. At first glance it resembles a half-submerged reef, a drifting island of debris, or a whale-fall buoy field.
- **Internal scale target:** 40–55 metres of visible body line; only a fraction is exposed at once.
- **Back:** broad, shallow, and irregular, with ridges resembling rounded basalt shelves. Kelp, goose barnacles, pale encrustations, and scraps of floating weed cling to parts of it.
- **Head:** wide and flattened, difficult to distinguish from the back until it turns.
- **Mouth:** a horizontal aperture that can open across much of the front body. Inside are dark folds and pale filtering ridges, not rows of fantasy teeth.
- **Skin:** charcoal grey with muted brown-green growth, pale scars, and wet slicks. The upper surface reflects cloud shapes more strongly than ordinary water.
- **Eyes:** not visible during the first encounter. At very close range, one recessed dark eye may briefly catch a weak reflection.

### Movement and sound
- It barely moves while feeding. Water and fish move around it instead.
- A feeding phase begins with small fish converging, then birds arriving, then a long low opening of the mouth.
- The body rises a few metres as the feeding aperture opens. A broad inward current pulls floating debris and baitfish toward it.
- The mouth closes slowly, followed by a massive surface displacement that travels outward as a wide pressure ring.
- Sound: a long wet creak, a low resonant intake, then an abrupt silence after closure.

### Behaviour and danger
- It is not a boat hunter. The player is in danger only by entering the feeding lane or trying to retrieve fish inside it.
- Before the mouth opens, schools gather in a pattern that contradicts the visible current. Birds circle too low and do not land.
- The player can watch from outside the pressure ring. If too close, turn laterally away from the centre and follow the outward movement of free-floating debris.
- Do not let it chase the boat after the player escapes. It resumes feeding or sinks from view.
- A player caught inside the closing zone may be rolled, pulled under, or lose the vessel.

### Discoverable lore
1. **Greyhaven — Naturalist's notebook, shelf behind the fish scale.** A local surveyor sketched what looks like a floating reef, then crossed it out.
   > "The birds were feeding on fish that had not yet reached the surface. Something below was making the shoal."
2. **Outer Shoals — Survey buoy 7.** The buoy has a circular scrape field and an old depth recorder. Its last entry reports an apparent bottom where no charted shelf exists.
   > "Return is broad and flat. It is moving against the current. I will not lower the camera."
3. **Last Anchorage — Captain's chart.** A route line circles a blank patch labelled "false land." A later hand adds: "Do not anchor here."
4. **After a sighting — Research station field sheet.** The player can find a later page matching the feeding sequence but disagreeing about whether the animal has a mouth or an opening in the water itself.

### Journal interpretation
The first corroborated pattern is fish aggregation followed by a large feeding event. The nature and full anatomy of the creature remain unresolved. Do not provide a neat taxonomic label.

---

## LEG-003 — Deep Arms
**Inspiration:** Kraken stories and giant cephalopod accounts  
**Classification:** Original cave-dwelling animal / anomaly; not the folkloric Kraken itself  
**Primary area:** Blackrock Reach — The Blue Maw and The Undercliff  
**Risk:** Conditional predatory contact; potentially lethal if the vessel is entangled  
**Appearance tier:** Rare sighting / exceptional contact

### Visual identity
- **Silhouette:** first a single arm crossing behind a basalt pillar; then several limbs occupying separate cave openings. Do not reveal the mantle during the initial encounter.
- **Internal scale target:** mantle 8–12 metres; longest visible arm 18–25 metres. Use camera cropping to keep the total animal unknown.
- **Arms:** thick near the base, tapering to flexible ends. Their underside has two irregular rows of suction discs, each with a pale outer ring and a dark central cup.
- **Skin:** basalt-black with bruised plum undertones. It can lighten to ash-grey when stretched across a wet rock face.
- **Texture:** coarse pebbled skin, old pale scars, small attached shell fragments, and a few smooth patches that reflect lantern light.
- **Eyes / mantle:** one brief deep-red or amber reflection may be visible from a crevice, but never a constant glowing eye effect.

### Movement and sound
- Arms move by anchoring suction cups and pulling the body along cave structure.
- One limb may remain perfectly still for minutes while another tests the current.
- A threatened limb releases with a series of wet suction pops, followed by a heavy splash.
- The cave transmits a low dragging rumble as the body changes position.
- When it retreats, the player sees the arms withdraw in the wrong order: the last visible limb disappears first, then a shadow crosses a distant aperture.

### Behaviour and danger
- It is territorial around the cave system and attracted by anchor vibration, salvage winches, and repeated Rig pulses.
- It may merely inspect the hull with one arm. Actual contact begins only when the player crosses the cave boundary or remains after a clear warning.
- It can catch an anchor chain, salvage line, or rudder. It should not instantly wrap the player's entire boat with invisible limbs.
- Escape requires cutting an attached line if necessary, stopping the winch, turning away from the cave wall, and using the outward current.
- It does not leave the Blue Maw to pursue the player into open sea.

### Discoverable lore
1. **Blackrock — Old basalt quarry cabin.** A quarryman's note describes ropes moving when no crew was hauling.
   > "We pulled the chain up clean. Something below pulled the other end back through the water."
2. **The Undercliff — Wreck *Saint Orra*.** A broken anchor has circular pressure marks too regular to be rock damage. The ship's log records one low impact and then a slow pull.
3. **Greyhaven — Salvage Registry.** Three boats abandoned gear at the same cave entrance over different years. Each report blames a different cause: snagged rock, bad knot, or "something that held."
4. **After contact — Rig recorder.** A short series of suction-like acoustic pulses matches the interval of the anchor-chain movement.

### Journal interpretation
The cave has a large mobile inhabitant or something imitating one. The evidence supports physical contact with a flexible object, but not the full body plan.

---

## LEG-004 — The Blue Maw Hunter
**Inspiration:** Lusca / blue-hole monster stories  
**Classification:** Original cave ambush predator  
**Primary area:** Blackrock Reach — inner pool of The Blue Maw  
**Risk:** High; lethal only after crossing a warned inner boundary  
**Appearance tier:** Exceptional encounter

### Visual identity
- **Silhouette:** a broad, low head emerges below the waterline; a dark back follows, then several short, muscular appendages that brace against the cave walls.
- **Internal scale target:** 10–14 metres long. Large enough to threaten the fishing boat, but substantially smaller than the mythic deep presences.
- **Head:** flattened wedge with a wide sensory front. Avoid a literal octopus-shark hybrid face; it is an original animal.
- **Eyes:** small recessed eyes adapted to darkness. They reflect the player's lantern only when the animal turns its head.
- **Body:** dark slate skin with blue-grey mottling and pale abrasion marks from scraping cave stone.
- **Fins / limbs:** two rear stabilizing fins and four to six short muscular gripping limbs. Their purpose is bracing and pulling, not theatrical tentacle waving.
- **Mouth:** opens sideways with a pale inner membrane; teeth are short and recurved, not oversized.

### Movement and sound
- It remains close to the cave floor and uses short ambush bursts.
- It can hold position against an incoming current using its limbs, then launch sideways across the entrance.
- Its body makes little noise while still. A strike is preceded by a deep throat click and a sudden cloud of displaced silt.
- It retreats backwards into the inner pool rather than swimming after the boat.

### Behaviour and danger
- The inner pool is its refuge and feeding site.
- Fish and crabs disappear from the entrance before the predator becomes visible.
- A warning should include at least two signs: a sudden slackening current plus a cluster of fresh shell piles, or a line of disturbed silt plus a close sonar return.
- It attacks a vessel only when the player crosses the inner marker or disturbs its den.
- Escape: turn outward with the main current, abandon any salvage line, and do not reverse blindly into the cave wall.
- If the player clears the outer marker, the hunter breaks pursuit.

### Discoverable lore
1. **Blackrock — Fisher's tide board.** A hand-drawn cross-section divides the cave into "outer water" and "mouth water."
   > "Fish can be taken from the lip. Past the black stone, leave the water to its owner."
2. **The Blue Maw — Shell pile near the inner wall.** The shells are stacked in a repeated crescent shape. A small brass tag beneath them is stamped with a wreck's registry number.
3. **Greyhaven — Former cave fisher's letter.**
   > "It never followed us home. The mistake was believing that meant it could not reach us inside."
4. **After escape — Boat damage record.** A broad scrape on the keel aligns with the inner cave rock, while a smaller circular mark suggests direct contact.

### Journal interpretation
A territorial animal may be defending a den. The local fisher's boundary knowledge is more reliable than the legend's monster description.

---

## LEG-005 — The Widow's Teeth
**Inspiration:** Scylla and cliff-side passage monsters  
**Classification:** Original cliff-associated ambush hazard; biological identity unresolved  
**Primary area:** Blackrock Reach — The Needle Gate and Widow's Teeth  
**Risk:** Conditional collision / seizure hazard  
**Appearance tier:** Rare passage event

### Visual identity
- **Distant silhouette:** several narrow dark shapes protrude from a cliff line like broken mast tips or sea stacks.
- **Close silhouette:** three to five long, jointed neck-like structures emerge from separate cracks in the rock. The player may mistake them for kelp, driftwood, or basalt pinnacles.
- **Surface:** wet black-brown material with rough mineral crust. At the joints, the surface is smoother and lighter, like old scar tissue.
- **Tips:** each ends in a small hooked structure or gripping pad. No identical monster heads; each tip has a different profile.
- **Scale:** the visible structures reach 5–9 metres above the surface, but their bases remain hidden under the cliff.
- **Eyes:** none clearly visible. A small reflective patch may align with the boat for a moment and then vanish.

### Movement and sound
- The structures stay motionless until the boat enters the inner rock line.
- They move in delayed sequence, as if separate pieces are testing the water.
- A single tip may tap the hull or pull a buoy line. Multiple tips do not attack simultaneously unless the severe event state is active.
- The sound resembles wet rope dragged across stone, followed by a hollow knock.

### Behaviour and danger
- It is a local passage hazard, not a roaming pursuer.
- It is more likely during low visibility and a surge running parallel to the cliff.
- The player can choose a wider route or take the narrow route after reading the foam and buoy pattern.
- If a tip catches the boat, turn toward open water and avoid steering into the cliff.
- A successful escape causes the appendages to fold back into the cracks.

### Discoverable lore
1. **Blackrock — Beacon keeper's wall chart.** The chart has three separate hand-drawn safe tracks, each dated after a different accident.
2. **Widow's Teeth — Old signal bell.** The bell's rope has a smooth section where something repeatedly rubbed against it.
3. **Last Anchorage — Letter from a missing pilot's sister.**
   > "He said the rocks had fingers. I told him every sailor sees hands in rope and shadows. The boat came back with the bell still ringing."
4. **After contact — Cliff survey plate.** A survey sketch shows new indentations several metres above the waterline.

### Journal interpretation
The visible forms are real, but their connection to the cliff is unclear. The journal should preserve both a biological and geological explanation.

---

## LEG-006 — The Roaring Narrows
**Inspiration:** Charybdis / dangerous whirlpool accounts  
**Classification:** Environmental current event; no confirmed creature  
**Primary area:** Blackrock Reach — The Roaring Narrows and central Needle Gate  
**Risk:** Potentially lethal vessel-control hazard  
**Appearance tier:** Rare environmental event

### Visual identity
- **First sign:** a broad circular foam line that seems to rotate against the visible surface current.
- **Developing phase:** the centre becomes dark and smooth, with broken white streaks spiralling inward.
- **Peak:** a depression or rolling convergence forms across a wide patch of water. It should not look like a perfect bathtub drain.
- **Edges:** crossing currents form diagonal foam bands; loose kelp and driftwood trace the flow.
- **Scale:** 40–80 metres across during the severe version; smaller events may occur as warning-only flows.
- **No eye or mouth:** the threat is the water itself.

### Movement and sound
- The event develops over several minutes of game time, not as an instant pop-up.
- Foam bands tighten, the centre becomes smooth, and the boat's wake is pulled sideways.
- The sound begins as a low roar under the wind and builds into a hollow thunder.
- The current changes in pulses. There is a short slack interval during which the player can cross a marked lane.

### Behaviour and danger
- Triggered by an authored tide transition, opposing current flows, and the boat entering the convergence lane.
- The player can observe the foam direction and buoy timing before committing.
- Escape: keep the bow aligned with the verified exit, reduce abrupt steering corrections, and cross during the slack interval.
- Do not allow both the cliff-side route and current lane to become lethal simultaneously without an explicit warning that the passage is closed.
- Severe failure can capsize or drag the boat into rock. Player death is possible if the vessel is already damaged or the player ignores the warning.

### Discoverable lore
1. **Greyhaven — Tide almanac.** A repeated note marks a short interval called "the quiet between pulls."
2. **The Needle Gate — Buoy recorder.** Its bell pattern repeats in three groups, with a pause before the strongest current.
3. **Blackrock — Rescue report.** The official account blames engine failure, but the attached drift map shows the boat was carried sideways.
4. **After crossing — Survey chart.** The player can overlay their own route and identify the slack lane.

### Journal interpretation
A repeatable current hazard exists. The supernatural explanation remains an optional local belief, not the confirmed cause.

---

## LEG-007 — The Ship That Never Makes Port
**Inspiration:** Flying Dutchman  
**Classification:** Phantom vessel / uncertain visual anomaly  
**Primary area:** Drowned Parish outer channel to Outer Shoals  
**Risk:** Harmless at distance; dangerous through misnavigation if pursued  
**Appearance tier:** Rare night sighting

### Visual identity
- **Silhouette:** a two-masted working brig or small merchant vessel, not a grand pirate galleon. It has a narrow bow, high stern, and a dark hull that sits low in the water.
- **Sails:** three or four torn sail panels hang in a wind that does not match the player's wind.
- **Rigging:** thin dark lines cross the sails. One mast is slightly out of true.
- **Lights:** two warm lanterns at the stern and one dim cold light near the foremast. They do not illuminate nearby water.
- **Crew:** no clear faces. At most, one silhouette crosses behind the rail and is gone before the player can focus.
- **Surface interaction:** the hull creates no ordinary bow wave. It may be reflected in water while the visible ship is hidden by fog—or the reverse.

### Movement and sound
- It appears on a fixed bearing, then seems to move laterally without changing its distance.
- Its sails may fill while the player's wind flag hangs limp.
- A bell rings at an interval inconsistent with the vessel's visible rocking.
- If approached, the hull may blur behind rain, fog, or spray. On a clear bearing check, it can disappear entirely.
- It never rams, fires at, or deliberately chases the player.

### Behaviour and danger
- Triggered on a night transit through the known phantom bearing in fog or weak visibility.
- It is harmless unless the player abandons verified navigation to follow it.
- The dangerous part is false bearing, reef, wreck field, fuel loss, or entry into a storm boundary.
- Escape: stop chasing, compare compass with chart and two independent landmarks, then steer toward the last verified buoy.
- The ship may reappear far off the player's stern after the player resumes the correct route. This is an optional unsettling variation.

### Discoverable lore
1. **Drowned Parish — Harbour master's registry.** A ship name appears in arrival and departure columns on the same night, decades apart.
2. **Outer Channel — Half-submerged cargo manifest.** The manifest's cargo was recorded as delivered even though the vessel was never officially recovered.
3. **Greyhaven — Lighthouse keeper's cassette.**
   > "Two lamps beyond the shoal. No hull on the radar. I turned the lamp toward them and the lamps turned back."
4. **After sighting — Player's compass log.** It records the phantom bearing and actual buoy bearing separately.

### Journal interpretation
The player saw a vessel-shaped image or phenomenon. The record does not establish whether it was a ship, mirage, repeated light pattern, or something else.

---

## LEG-008 — Davy's Locker
**Inspiration:** Davy Jones's Locker and wreck-site sailor folklore  
**Classification:** Wreck / entanglement / down-current event; no literal pirate entity  
**Primary area:** Drowned Parish — flooded streets, wreck field, The Last Anchorage  
**Risk:** Potentially lethal salvage hazard  
**Appearance tier:** Triggered environmental encounter

### Visual identity
- **Water:** a patch of nearly black water inside a wreck opening. It absorbs the player's lantern reflection rather than reflecting it.
- **Debris:** rope ends, net fragments, splintered planks, and small bubbles rise from below.
- **Line behaviour:** the player's salvage cable hangs too straight despite the current, then begins to vibrate.
- **Wreck details:** a door, hatch, or broken stair seems to open deeper than the structure's visible dimensions.
- **No humanoid apparition:** keep the event environmental. Avoid a pirate captain with a skeletal face.

### Movement and sound
- Current direction shifts around the wreck, then reverses below the surface.
- The winch produces a low strain sound even after the player releases the control.
- Metallic knocks travel up the line with no corresponding movement at the wreck.
- A propeller may catch a loose line or net. The player receives visible vibration and steering drag before propulsion is lost.

### Behaviour and danger
- Triggered when salvage begins during an adverse down-current reversal or after repeated high-tension recovery attempts.
- The player can abort before the line wraps the propeller.
- Escape: release the winch, cut or abandon the cable, kill the engine if the propeller is fouled, then use the current to move away from the wreck.
- Failure can cause loss of propulsion, hull damage, or sinking. The player may be rescued if they reach a safe float zone; death is possible if trapped in the wreck's current.

### Discoverable lore
1. **Drowned Parish — Wreck salvage permit ledger.** Several crews list "line returned without load" and "cable missing below the hatch."
2. **Flooded Parish House 12 — Sailor's tin.** A water-damaged note reads:
   > "The locker is not a place. It is the moment the sea takes the other end of the rope."
3. **Last Anchorage — Winch maintenance card.** Three separate winches have the same unusual twist damage.
4. **After escape — Cable fragment.** The outer fibres are abraded in a spiral pattern, useful for identifying the same hazard at another wreck.

### Journal interpretation
A dangerous current / entanglement zone is supported by physical evidence. The sailor's phrase is a metaphor or superstition; the journal does not claim a literal Davy Jones.

---

## LEG-009 — The Black Horizon
**Inspiration:** Umibōzu  
**Classification:** Apparition / horizon-distortion event  
**Primary area:** Hollow Trench — Outer Wall and trench edge  
**Risk:** Passive at distance; lethal environmental escalation if approached or falsely followed  
**Appearance tier:** Rare night listening event

### Visual identity
- **Silhouette:** a vertical mass rising from the water, broad at the base and slightly narrowing toward the top. It resembles a human torso only at a distance.
- **Scale:** the visible rise may appear 20–35 metres above the sea. Its true base is never confirmed.
- **Surface:** nearly matte black, with no obvious skin or clothing texture. The edges disappear into fog and night rather than glowing.
- **Head:** a smooth rounded upper mass with no visible face. Never draw a clear pair of eyes.
- **Waterline:** the water at its base appears too flat, as if the waves stop before reaching it.
- **Reflection:** sometimes the reflection is shorter, tilted, or absent even when the entity is visible.

### Movement and sound
- It does not walk or swim. It becomes taller, shorter, or shifts position between lightning flashes or camera occlusions.
- The horizon may appear to rise behind it.
- A sound resembling a wet rope pulled through a ring arrives from the opposite bearing to the silhouette.
- The radio may return the player's last transmission with a delay that is too short or too long.
- When it withdraws, the waterline resumes ordinary wave motion rather than the figure sinking.

### Behaviour and danger
- It is passive while the player remains outside the boundary.
- Approaching it or following a false light may trigger a large lateral current or a wave wall.
- Escape: use a known compass heading and verified buoy bearing; do not steer toward the reflection or answer the radio.
- It should not chase the player once they turn away. The sea event itself is the danger.
- Death is possible if the player enters the boundary in a damaged vessel or follows the false bearing into the trench edge.

### Discoverable lore
1. **Hollow Trench — Night-watch log.** An old watchkeeper records a "standing dark" beyond the last buoy.
2. **Last Anchorage — Broken radio.** A recording repeats a call sign in the player's own voice, although the original tape predates the player character.
3. **Outer Wall — Signal buoy.** The buoy's lens has a second burn mark on its inside surface, as though a light was shining inward.
4. **After sighting — Navigation overlay.** The player can compare the apparent bearing with the real lighthouse line and discover a persistent offset.

### Journal interpretation
A dark shape and navigational distortion were observed. Its apparent humanoid form may be a perceptual interpretation; the journal records the radio and bearing anomalies separately.

---

## LEG-010 — The Pale Choir
**Inspiration:** Bake-kujira / ghost-whale omen stories  
**Classification:** Anomalous whale-like apparition and ecological omen  
**Primary area:** Outer Shoals — Bone Orchard and whale-fall corridor  
**Risk:** Not predatory; can lead the player into dangerous deep water if followed  
**Appearance tier:** Rare omen

### Visual identity
- **Silhouette:** a large whale-shaped back with pale rib-like lines visible through a translucent surface. Avoid a literal walking skeleton or exposed bone monster.
- **Colour:** cold bone-grey, faded cream, and the faintest green-blue undertone. The body is never uniformly luminous.
- **Body:** appears complete from one side and skeletal from the other. The player cannot hold both views at once.
- **Head:** long and blunt, with no visible eye on the side facing the player.
- **Tail:** the fluke does not always match the direction of travel. On one pass, it may bend against the body motion.
- **Companions:** seabirds and scavengers may follow it, but their shadows do not always align with their bodies.

### Movement and sound
- It moves slowly parallel to the whale-fall corridor.
- Its surface break has no splash. Instead, a thin sheet of water slides off the back.
- Its call consists of a normal whale-like tonal phrase with one missing harmonic or a silent interval.
- Scavengers gather after it passes, even when no carcass is visible.
- It may appear for one breath, then be absent when the player looks back.

### Behaviour and danger
- It is an omen, not an attacker.
- It may coincide with a shift in scavenger activity, weather, or local fishing availability, but these correlations are not guaranteed.
- Following it can lead to poor visibility and dangerous deep water.
- Escape: record the bearing and return along known depth contours; do not follow beyond the player's equipped depth / navigation range.
- No ordinary catch or loot reward is granted for approaching it.

### Discoverable lore
1. **Greyhaven — Fish market noticeboard.** A handwritten note says the catch is poor whenever "the white ribs pass." Another fisher writes that the claim is nonsense.
2. **Bone Orchard — Research buoy.** Its recorder captures a call with the same missing harmonic as the player's encounter.
3. **Last Anchorage — Memorial plaque.** A list of missing crew members includes one boat that returned without its crew's catch or equipment.
4. **After sighting — Scavenger survey.** The player can observe an unusual congregation of isopods and deep scavengers, supporting an ecological change but not proving the apparition caused it.

### Journal interpretation
The event may be connected to a whale-fall ecosystem and an unusual acoustic phenomenon. The skeletal appearance remains unverified.

---

## LEG-011 — The Moon-Eater Shadow
**Inspiration:** Bakunawa, treated as a culturally specific Filipino folklore reference  
**Classification:** Rare lunar / tide manifestation; literal creature status unresolved  
**Primary area:** Visible across the archipelago, strongest from Greyhaven and The Shallows  
**Risk:** Environmental tide hazard; no direct attack  
**Appearance tier:** Calendar-gated mythic event

### Visual identity
- **Sky:** the eclipse or darkened lunar disc remains the primary visual anchor.
- **Water reflection:** a long serpent-like darkness crosses the moon's reflection, appearing to swallow or bend the reflected light.
- **Body:** never show a complete dragon model. A short curved segment may be visible in cloud, reflection, or surface swell, but not all at once.
- **Scale:** the apparent length spans multiple water patches, yet no physical body is measured.
- **Colour:** moonlight silver is interrupted by deep ink-black segments; avoid bright fantasy purple or red.
- **Secondary detail:** birds stop crossing the reflection; surface fish turn as a single school; distant bells may ring from more than one island.

### Movement and sound
- The dark shape appears to travel across the reflection while the actual moon remains fixed.
- Tide gauges and buoys disagree briefly, then settle into a new rhythm.
- The sound is not a roar; it is a series of distant wooden knocks and a low vibration.
- The event builds slowly and ends at the authored eclipse / tide boundary.
- A harmless version can be observed from sheltered water.

### Behaviour and danger
- Triggered by an authored lunar calendar event, never by random daily chance.
- Player danger comes from current reversal, exposed channels, and shallow reef edges.
- The player should receive advance notice through the calendar, NPCs, tide notes, and a visible sky change.
- Escape: retreat to a sheltered bay before peak tide; avoid narrow channels until the current settles.
- A damaged boat caught in the surge can ground, capsize, or sink.

### Discoverable lore
1. **Greyhaven — Community moon calendar.** A particular lunar date is circled, but notes differ about whether boats should leave harbour.
2. **Greyhaven — Archive folio on Abaknon navigation.** A research document records one specific Abaknon account of Bakunawa and details of traditional moon, tide, and current knowledge. It explicitly notes that versions vary between communities.
3. **The Shallows — Old tide board.** A faded table marks a short quiet phase before the strongest current change.
4. **After the event — Buoy recorder.** The player's data reveals that the current reversed in one channel but not another.

### Journal interpretation
The lunar and tidal event is measurable. The serpent-like form belongs to the legend and visual interpretation; the journal does not declare that all Philippine communities share one version of the story.

### Cultural handling
This is not a generic dragon boss. Keep the fictional event focused on moon, tide, memory, and local knowledge. Attribute any real-world source to the specific community and do not present invented rituals as authentic Filipino custom.

---

## LEG-012 — The Reef Walkers
**Inspiration:** historical Visayan accounts of kataw; the in-game presence is an original fictionalized sighting, not a direct representation of a universal belief  
**Classification:** Ambiguous sea-person sighting / possible human or animal misidentification  
**Primary area:** The Shallows — reef channels, seagrass edge, sand gutters  
**Risk:** Usually harmless; danger comes from the player following them into reef hazards  
**Appearance tier:** Rare night watch

### Visual identity
- **Distant silhouette:** two or three upright shapes at the edge of a reef ledge, visible only when bioluminescence or moonlight outlines the water.
- **Proportions:** roughly human-sized from the player's viewpoint, but the legs / lower body are never clearly visible.
- **Skin / surface:** no visible scales or mermaid tail. The forms have smooth dark upper silhouettes and pale reflective patches near the shoulders.
- **Head:** small, with a narrow neck and no readable facial detail.
- **Movement:** they may appear to sit, stand, or rise from the water in one slow motion. Their lower halves vanish into the reef shadow.
- **Lighting:** brief bioluminescent traces on the water, not glowing skin.

### Movement and sound
- They remain near the reef, often aligned with current direction.
- When the boat's lantern points directly at them, one silhouette turns its head and all disappear below the ledge.
- A faint clicking sound may be mistaken for shellfish or reef creak.
- They do not pursue the boat and do not attack.
- A rare variation places one silhouette on the far side of a buoy, then removes it when the player checks the bearing.

### Behaviour and danger
- The event requires a moonless or low-moon night, calm water, low engine noise, and healthy reef activity.
- It is safe to observe from a charted channel.
- Following them off-route can push the boat into coral heads, shallow rock, or surge channels.
- Escape: stop pursuit, use the chart and depth contour, and return through the open channel.
- They are not catchable, lootable, or automatically hostile.

### Discoverable lore
1. **Greyhaven — Fisher's family notebook.** It contains a sketch of upright forms at the waterline, but the writer labels it "probably divers."
2. **The Shallows — Abandoned reef marker.** Its underside has three small hand-like scratches and a shell tied to the line.
3. **Drowned Parish — Old translated account.** A historical note describes sea-people in one Visayan account, while warning that the record is a specific author's description, not a universal belief.
4. **After sighting — Player's field sketch.** The journal lets the player mark uncertain lower-body shape rather than auto-identifying a mermaid.

### Journal interpretation
The player saw upright figures near a reef. They may be people, animals, reflections, or an anomalous presence. Keep the uncertainty alive.

### Cultural handling
Use an original in-game name and visual design. Do not call the figures "the kataw" as though one historical text defined all Visayan beliefs.

---

## LEG-013 — The Channel Warden
**Inspiration:** broad place-guardian motif; taniwha are not directly depicted  
**Classification:** Original protective / territorial current presence  
**Primary area:** The Shallows ↔ Blackrock Reach — marked channel and dangerous narrows  
**Risk:** Usually protective; hazardous only after deliberate disturbance of its marked site  
**Appearance tier:** Rare passage manifestation

### Visual identity
- **Main form:** a long, broad-backed ray-like silhouette moving just below the surface, with a tapering tail and a faint ridge along the back.
- **Scale:** larger than the player's boat by two to three boat lengths, but rarely visible all at once.
- **Surface:** dark green-grey, with pale mineral-looking lines that resemble old current maps.
- **Head:** broad and flat, with no visible mouth during the ordinary manifestation.
- **Tail:** a narrow wake follows it in a perfectly straight line, even through a twisting current.
- **Variant:** at night, only the wake and a series of pale surface marks are visible; no full body is shown.

### Movement and sound
- It moves across the current rather than with it.
- Small fish may align in a loose corridor behind its wake.
- Buoys ring in an altered but consistent sequence as it passes.
- It may circle the safe side of a channel once, then continue out to deeper water.
- It never chases the player into open sea.

### Behaviour and danger
- The presence may guide the player around the dangerous current seam.
- It is more likely to appear after the player respects a nursery closure, marks a safe route, or records a local warning.
- Deliberate repeated anchoring or damage to a marked site can produce a defensive surge or blocked passage. The manifestation is not a morality detector; the trigger is a specific authored disturbance.
- Escape: leave the marked site, stop the disturbing activity, and follow the open-water route.
- A helpful appearance must be allowed to remain helpful; do not force every guardian motif to turn hostile.

### Discoverable lore
1. **Greyhaven — Channel pilot's licence book.** A retired pilot drew a line that is safer than the official chart.
2. **The Shallows — Marker stone / buoy chain.** The stone carries a unique current symbol. A nearby note says the symbol means "pass beside, not over."
3. **Blackrock — Pilot's testimony.**
   > "It went ahead of us. The current did not stop. It opened."
4. **After a safe passage — Player's route record.** The journal compares the observed wake with the safest current seam.

### Journal interpretation
A recurring presence or physical animal may be associated with the channel. Its protective meaning is a local interpretation, not a confirmed biological motive.

### Cultural handling
Do not use the name taniwha or reproduce Māori sacred imagery in the shipped design without appropriate community-specific research and consultation. The game entity is an original place-guardian concept.

---

## LEG-014 — The Serpent Current
**Inspiration:** broad sea-kami / Ryūjin motif, not a literal depiction of a named deity  
**Classification:** Luminous current phenomenon / protective omen  
**Primary area:** Greyhaven outer harbor and Shallows reef edge  
**Risk:** Harmless; dangerous weather may follow if the warning is ignored  
**Appearance tier:** Very rare weather-window event

### Visual identity
- **Primary form:** a long, thin, luminous current line that bends in the water like the outline of a serpent.
- **Light:** muted silver-green or pale blue, made from many small points of suspended bioluminescence rather than one solid beam.
- **Edges:** broken and granular; light appears in separate drifting clusters.
- **Scale:** may stretch hundreds of metres along the surface, disappearing behind wave crests.
- **Head / face:** none. A bright leading knot of light may look like a head from one angle but has no eyes or mouth.
- **Reflection:** the line may show on the water before it is visible directly.

### Movement and sound
- The light travels along a safe current seam toward sheltered water.
- It pulses once at each buoy interval, creating a navigational rhythm.
- The sound is a faint low tone under the normal wave ambience.
- It fades when the player reaches the protected channel.
- It does not respond to attacks or direct contact.

### Behaviour and danger
- Triggered after a calm-before-storm weather window, a completed local restoration activity, and an authored community-story flag.
- It is a warning or guide, not a reward for ritual behaviour.
- Following it can help the player find shelter; ignoring it leaves the vessel exposed to the approaching storm.
- Escape: follow charted shelter route and verify the line against known buoys.
- The phenomenon never directly damages the boat.

### Discoverable lore
1. **Greyhaven — Lighthouse keeper's storm card.** A pencilled note describes "a thin light running along the inside current."
2. **Harbor — Retired fisher's letter.**
   > "It did not lead us to fish. It led us away from the rocks."
3. **The Shallows — Water sample record.** A sample from a previous event shows elevated bioluminescent plankton, offering a natural hypothesis.
4. **After sighting — Weather chart.** The player can align the light's route with the actual sheltered current.

### Journal interpretation
A luminous surface-current event may precede severe weather. The serpent description is a human interpretation of its shape.

### Cultural handling
Do not depict Ryūjin or Watatsumi as a generic glowing monster. This is an original phenomenon inspired by themes of sea guidance and reciprocity.

---

## LEG-015 — The Leviathan Shadow
**Inspiration:** Leviathan / Lotan as an image of unknowable sea scale  
**Classification:** Mythic-scale anomaly; not a killable creature  
**Primary area:** Hollow Trench — outer slope and Outer Wall  
**Risk:** Passive at safe contour; potentially lethal if followed into the deep boundary  
**Appearance tier:** Mythic / late-game

### Visual identity
- **Never show the full body.** The player sees a moving shadow across the seabed, a large curve that crosses below the boat, or a dark vertical shape that blocks all ambient light.
- **Scale:** the shadow may cover the visible trench wall for several seconds. Its size cannot be measured.
- **Surface:** no consistent texture. At different angles it resembles a smooth hide, a layered ridge, or a moving absence of light.
- **Anatomy:** one brief suggestion of a fin or ridge may appear, but the next view contradicts it.
- **Lighting:** ambient plankton light dims along a moving line. Do not use a bright glowing eye or conventional monster face.
- **Environmental detail:** silt rises in a long wave, deep fish leave the contour, and small lights appear to move in the wrong direction.

### Movement and sound
- Movement is slower than the boat at first. It crosses a wide region without visible propulsion.
- It may pass beneath the player without producing a surface wake.
- The Sounding Rig returns several layers at different depths, then one return with no bottom.
- The sound resembles distant pressure, not a roar.
- When the player turns away, the shadow may remain in the same place, leaving uncertainty about whether it moved at all.

### Behaviour and danger
- It is not targetable, catchable, or directly aggressive.
- The risk comes from the player following the contour below the safe operating depth or continuing to ping after multiple contradictory returns.
- Escape: turn toward the upper slope, hold a verified depth contour, stop repeated Rig pulses, and preserve engine reserve.
- If the player continues beyond the authored threshold, the boat can lose control, flood, sink, and cause death.
- A distant sighting may remain entirely harmless.

### Discoverable lore
1. **Hollow Trench — Deep survey station.** A depth graph shows a moving return over three different expeditions.
2. **Greyhaven — Old theological / sailor text.** A local translation uses "the thing beneath the measure" without naming Leviathan.
3. **Outer Wall — Recorder with pressure damage.** The audio contains a low pulse repeated at intervals too long for a normal boat engine.
4. **After sighting — Survey analysis.** The player overlays the three records and sees a moving boundary, but no stable shape.

### Journal interpretation
A region-scale moving acoustic / visual anomaly exists. The journal is not allowed to certify Leviathan as a literal animal.

---

## LEG-016 — The Encircling Wake
**Inspiration:** Jörmungandr / world-serpent motif  
**Classification:** Archipelago-scale current event / mythic anomaly  
**Primary area:** Outer Shoals, Hollow Trench, and the archipelago margin  
**Risk:** Non-combat environmental event; crossing at the wrong phase can be fatal  
**Appearance tier:** Endgame / mythic event

### Visual identity
- **Wide view:** a curved line of white disturbance wraps around part of the distant horizon.
- **Local view:** only one segment is visible at a time: foam, a travelling swell, a line of birds, or a dark ridge.
- **Shape:** the ring is not a perfect circle. It bends around islands and deep-water features as though constrained by bathymetry.
- **Light:** no glowing serpent body. At night the event is visible through moonlit foam and intermittent surface breaks.
- **Scale:** the disturbance appears to connect distant regions; it cannot be seen in full from the player camera.
- **Possible body clue:** a long dark curve may pass under the surface, but its presence is never consistent across viewpoints.

### Movement and sound
- The wake boundary travels around the archipelago margin during a rare current / tide event.
- Buoy intervals drift in sequence as the event approaches.
- Birds and surface fish move ahead of the boundary.
- A distant low-frequency sound travels from one region to another, arriving later than expected.
- The event has a temporary slack lane where crossing is possible.

### Behaviour and danger
- It is not a boss and cannot be attacked.
- It requires endgame access, a severe tide condition, and prior reports from at least three regions.
- It may be observed from safe water without consequence.
- Crossing the ring at the wrong phase can rotate or displace the boat, carry it off course, or drive it into uncharted water.
- Escape: wait for or identify the slack lane through buoy timing, current readings, and surface foam. Shelter if the lane is not available.
- A player who ignores multiple regional warnings can be swept into an endgame hazard and die.

### Discoverable lore
1. **Greyhaven — Old chart with a broken circular mark.** It shows a ring-shaped route but no legend.
2. **Outer Shoals — Migratory observer's notebook.** Three distant schools changed direction on the same night.
3. **Hollow Trench — Buoy network report.** Three recorders show the same current change arriving at different times.
4. **After event — Player's multi-region map.** The route data draws a partial ring around the archipelago margin.

### Journal interpretation
A large-scale current / wake pattern connects distant regions. The serpent image is a mythic interpretation; the physical cause remains unresolved.

---

## LEG-017 — The Long Red Ribbon
**Inspiration:** sea-serpent sightings and the real oarfish  
**Classification:** Rare real wildlife sighting / possible misidentification  
**Primary area:** Outer Shoals — upwelling lines and surface convergence  
**Risk:** Harmless  
**Appearance tier:** Uncommon rare-wildlife sighting

### Visual identity
- **Silhouette:** a narrow silver ribbon breaking the surface in separate curves, sometimes mistaken for multiple humps of one long serpent.
- **Colour:** reflective silver body with a red or crimson dorsal edge. The colour is muted at distance and bright only at a close surface break.
- **Body:** extremely elongated and laterally compressed, with a small head and a trailing dorsal fin.
- **Movement:** undulates in a way that may look like a sea snake from a boat, especially when only short sections are visible.
- **Scale:** individual length can be very large for a fish, but the player should not be shown a giant monster-sized body.
- **Damage:** none. Do not add fangs, tentacles, or hostile eyes.

### Movement and sound
- It drifts or swims near a productive upwelling line.
- It may surface briefly, roll, and disappear; it does not target the boat.
- Small fish may gather nearby, but the event does not guarantee a fishing bonus.
- It makes no supernatural call. The audio is ordinary surface movement and distant fish activity.

### Behaviour and danger
- Triggered by deep upwelling, floating debris lines, and dusk surface surveys.
- It can resolve as a genuine rare fish, debris, or an uncertain sighting.
- The player can record it from a safe distance.
- Following it is not inherently lethal, but leaving charted water in poor visibility can create ordinary navigation risk.

### Discoverable lore
1. **Greyhaven — Newspaper clipping.** A decades-old report describes a "red-backed sea serpent" near an upwelling line.
2. **Outer Shoals — Naturalist's sketchbook.** A drawing includes the long ribbon body and red fin, but the caption reads "possibly fish."
3. **Research station — Oarfish plate.** A comparative illustration helps the player identify the creature after a clear sighting.
4. **After encounter — Field note.** The journal records the shape and behaviour but labels identification as probable, not certain.

### Journal interpretation
This is the most grounded sea-serpent report in the set. The player may have seen a rare real animal or misread part of a longer body through waves.

---

## 5. Contextual legends: discoverable in archives, not direct spawns

The following traditions remain lore-only or require a different climate / cultural review. They can still be discovered through books, translated accounts, ship names, letters, or research notes. They should not be forced into the current warm-water archipelago as literal enemies.

### 5.1 Selkies — North Atlantic seal-person stories
- **Visual treatment:** old charcoal drawing of a seal on a rock beside a discarded human coat; no literal spawn in the current region.
- **Discoverable source:** a sailor's book in the Greyhaven archive, found among donated foreign charts.
- **Note fragment:**
  > "The skin was not a coat. It was the way back. He kept it locked until the woman forgot the sound of waves."
- **Purpose:** identity, captivity, and returning home; a literary motif rather than a sea-monster encounter.

### 5.2 Qalupalik / Qallupilluit — Inuit shoreline traditions
- **Visual treatment:** archival illustration of an ice edge and a listening figure beneath it. No direct depiction or spawn in the warm-water game setting.
- **Discoverable source:** a university / polar expedition folio, clearly attributed to a specific Inuit storyteller and region if included.
- **Note fragment:**
  > "The warning was not about a monster being everywhere. It was about a child going where the ice could not hold."
- **Purpose:** place-specific survival knowledge and boundaries. Direct use requires Inuit-specific consultation and attribution.

### 5.3 Akkorokamui — Ainu-associated octopus tradition
- **Visual treatment:** old bay map with a red wash around a named coastal inlet; avoid inventing a sacred creature portrait.
- **Discoverable source:** a translated maritime folklore volume in the archive, with the Ainu source and publication context identified.
- **Note fragment:**
  > "The mapmaker drew the bay red, then left the creature's shape blank."
- **Purpose:** place, power, and regional memory. Do not use it as a generic octopus boss.

### 5.4 Mami Wata — diverse African and diasporic water-spirit traditions
- **Visual treatment:** a print or art-history reproduction with source attribution, not a generic game mermaid.
- **Discoverable source:** a maritime trade archive or museum catalogue; explain that the term spans diverse communities and forms.
- **Note fragment:**
  > "The captain wrote that the bargain was not for gold. It was for the promise he would return what he had taken."
- **Purpose:** exchange, wealth, obligation, healing, and danger. Not a single standardized character or enemy.

### 5.5 Yacumama — Amazonian river-water tradition
- **Visual treatment:** a river-map serpent motif in an expedition journal; no ocean spawn.
- **Discoverable source:** a clearly attributed river expedition folio with the particular community / account identified.
- **Note fragment:**
  > "The river was named before the creature was. The map does not say where one ends and the other begins."
- **Purpose:** freshwater boundary, river-mouth ecology, and water as a living place. Do not label it an ocean legend.

## 6. Evidence placement map

This map gives the narrative team a first pass at where records can be found. It is not a final level layout; all placement IDs must be reconciled with the approved location map later.

| Location | Record / evidence | Associated encounter | Discovery condition |
|---|---|---|---|
| Greyhaven Harbour Archive | Whaling Ledger 04 | White Whale | Open archive after first harbour introduction |
| Greyhaven fish market | Naturalist's notebook | Moving Feeding Ground | Inspect shelf after hearing baitfish rumour |
| Greyhaven tide board | Eclipse / current almanac | Moon-Eater Shadow, Roaring Narrows | Read after acquiring basic tide chart |
| Greyhaven old chart cabinet | Broken circular route chart | Encircling Wake | Late-game archive access |
| Greyhaven donated foreign-chart shelf | Selkie book, sea-spirit art catalogue | Contextual legends | Optional archive exploration |
| Last Anchorage tender cabin | *Morrow* deck log | White Whale | Reach tender wreck |
| Last Anchorage chart table | False-land chart | Moving Feeding Ground | Salvage chart case |
| Last Anchorage lighthouse / signal room | Phantom lamps cassette | Ship That Never Makes Port | Restore recorder power |
| Last Anchorage winch station | Cable maintenance card | Davy's Locker | Inspect after line vibration event |
| Outer Shoals Survey Buoy 7 | Flat-return depth recorder | Moving Feeding Ground | Recover recorder after sighting or survey clue |
| Outer Shoals Bone Orchard | Whale-fall research buoy | Pale Choir | Visit whale-fall corridor at night |
| Outer Shoals upwelling survey | Red-ribbon fish sketch | Long Red Ribbon | Complete a surface survey |
| Blackrock quarry cabin | Quarryman's chain note | Deep Arms | Enter quarry cabin |
| Blackrock tide board | Outer / inner cave boundary | Blue Maw Hunter | Speak to cave fisher or inspect board |
| Blackrock Widow's Teeth signal bell | Bell rope abrasion | Widow's Teeth | Reach cliff-side beacon |
| The Roaring Narrows buoy | Three-pulse recorder | Roaring Narrows | Record one adverse tide |
| The Blue Maw inner wall | Crescent shell pile and brass tag | Blue Maw Hunter | Observe shell pattern and retrieve tag |
| The Undercliff wreck | Anchor with circular marks | Deep Arms | Salvage from the outer wreck |
| Drowned Parish harbour office | Repeated arrival registry | Ship That Never Makes Port | Search flooded registry |
| Drowned Parish House 12 | Sailor's tin | Davy's Locker | Search submerged home |
| Drowned Parish old chapel / memorial wall | Missing-crew names | Pale Choir / Davy's Locker | Optional exploration; no sacred real-world copy |
| Drowned Parish translated account | Historical sea-person note | Reef Walkers | Archive recovery; clearly identified author |
| Hollow Trench night-watch post | Standing-dark log | Black Horizon | Unlock trench night access |
| Hollow Trench deep survey station | Moving depth graph | Leviathan Shadow | Complete prior deep acoustic survey |
| Hollow Trench buoy network | Regional timing report | Encircling Wake | Late-game multi-region survey |
| The Shallows reef marker | Three scratches and shell | Reef Walkers | Inspect marker after night sighting |
| The Shallows channel pilot marker | Safe current line | Channel Warden | Map the safe channel |
| The Shallows water-sample post | Bioluminescent sample | Serpent Current | Collect sample after omen |

## 7. How the player actually finds the information

Use a mix of optional discovery paths. Do not turn every document into a glowing collectible.

### 7.1 Physical inspection
- A readable object has a contextual interaction prompt only when the player is near enough.
- Documents may be folded, water-damaged, burned, salt-stained, or incomplete.
- A paper can be read in the boat cabin after collection; the player does not need to remain in a dangerous location to finish reading.
- Some records require a light, a dry storage bag, a tool, or a successful salvage recovery.
- A record can be found without the associated entity ever spawning.

### 7.2 NPC testimony
- NPCs initially share short public rumours.
- More precise details emerge after trust, a related delivery, a rescue, a survey, or showing them physical evidence.
- NPC testimony can be mistaken or self-serving. The journal stores the speaker and context.
- No mandatory quest should require guessing which NPC tells the truth.

### 7.3 Sound recordings
- A recorder contains a fixed clip with a visible time / location stamp if that device supports it.
- The player may compare the clip to the Sounding Rig's current recording.
- A matching acoustic signature advances corroboration, not certainty.
- Some recordings are damaged; the missing interval is part of the clue, not an audio bug.

### 7.4 Environmental traces
- Wakes, scars, broken buoys, line abrasion, shell piles, bird movement, unusual fish absence, and displaced wreckage can be recorded.
- Some traces appear only after the event or at the next tide.
- A trace can persist after the entity leaves, allowing the player to investigate later.
- Do not make every trace a guaranteed spawn marker; the player must distinguish habitat evidence from anomaly evidence.

## 8. The journal: wording, contradiction, and player knowledge

The journal must never automatically replace a witness description with an authoritative monster name before evidence supports it.

Examples:
- Rumour: "Old fishers call it the White Whale."
- After a distant sighting: "Large pale marine animal observed beyond Outer Wall. Species and identity unconfirmed."
- After physical corroboration: "A pale whale with a recurring fluke notch appears to match three historical sightings."
- After an instrument pattern: "The Moving Feeding Ground appears to create a local convergence in baitfish and surface current. Full body not observed."

Use a confidence label only where it reflects evidence:
- Unverified.
- Single observation.
- Independently corroborated.
- Repeatable pattern.
- Best-supported interpretation.

Do not use "confirmed supernatural" as a default stage. The journal records what the player knows, not what the game's lore author knows.

## 9. Art, animation, sound, and VFX production notes

### 9.1 Shared reusable assets
- Large surface wake spline.
- Underwater shadow card / projected silhouette.
- Water displacement ring.
- Foam convergence and current ribbon.
- Sucker / line-contact decal set.
- Buoy chain tension animation.
- Distant ship silhouette and rigging planes.
- Bio-luminescent suspended particle clusters.
- Fog and spray occlusion.
- Low-frequency audio bed with separate distance / occlusion parameters.
- Field-record paper materials and handwriting variants.
- Journal sketch overlays built from in-world documents rather than generic UI icons.

### 9.2 Audio is part of identity
Each encounter requires:
- a far-distance cue;
- a close-range cue;
- a warning cue;
- a contact / hazard cue;
- a retreat / disappearance cue;
- an aftermath ambience change.

Never use the same generic monster roar for all legends. The White Whale breathes and clicks; the Moving Feeding Ground draws and closes; Deep Arms pulls against rope and rock; the Ship That Never Makes Port rings a bell without a matching hull sound; the Black Horizon suppresses ordinary wave ambience.

### 9.3 Accessibility and readability
- Provide a subtitle / caption option for meaningful environmental audio clues.
- Do not rely on colour alone; shape, motion, sound captions, and instrument readings must also communicate the clue.
- Give sufficient contrast between entity silhouette and background without making it glow unnaturally.
- Critical warnings use redundant visual / audio channels.
- Do not hide the escape direction solely in a fast blink or a tiny UI indicator.

## 10. Production data checklist

Every legendary art / lore package must deliver:
- [ ] Front, side, and 3/4 silhouette sheet.
- [ ] Relative scale sheet beside the player boat / buoy / known structure.
- [ ] Surface material and scar / damage detail sheet.
- [ ] Idle, travel, surface / reveal, threat, retreat animations where applicable.
- [ ] Distance, medium, and close LOD / impostor variants.
- [ ] Audio cue sheet with distance and occlusion notes.
- [ ] Omen and aftermath VFX.
- [ ] At least three discoverable records (one pre-encounter, one field clue, one post-encounter).
- [ ] One contradictory account and one evidence-based journal synthesis.
- [ ] Cultural review notes for any living tradition used as inspiration.
- [ ] Trigger and escape data linked to the scheduler.
- [ ] Save / load persistence test for evidence and encounter state.

## 11. Related project specifications

- [Legendary Maritime Encounter Scheduler](LEGENDARY-ENCOUNTER-SCHEDULER.md) — location, triggers, danger, escape, cooldowns.
- [Ocean Legends and Sea Monsters](../02_LORE_AND_WORLD/OCEAN-LEGENDS-AND-SEA-MONSTERS.md) — source traditions and evidence classification.
- [World Simulation and Event Scheduler](WORLD-SIMULATION.md) — event opportunity boundaries.
- [Threat Director](THREAT-DIRECTOR.md) — candidate selection and major-threat lock.
- [Maritime Mysteries — Event Behaviour and Trigger Matrix](../03_STORY_AND_NARRATIVE/SIDE_QUESTS/MARITIME_MYSTERIES/EVENT-BEHAVIOUR-AND-TRIGGER-MATRIX.md) — quest event contract.
- [Regional Ecology and Creature Distribution](../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md) — wildlife and habitat baselines.
- [Creature Bible](../04_CHARACTERS_AND_ENTITIES/CREATURE-BIBLE.md) — entity roster and approved creature identities.

All designs remain proposed. The exact geography, species plausibility, cultural treatment, in-world document placements, and final art assets require review before production approval.
