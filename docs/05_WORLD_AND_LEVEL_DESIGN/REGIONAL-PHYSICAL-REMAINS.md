# Regional Physical Remains and Environmental Evidence

> Project: WHERE THE SOUNDINGS END  
> Status: Working environmental-content specification — Greyhaven additions source-implemented; other regional sites proposed  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-14

## 1. Purpose and implementation boundary

This document translates the six-region history into physical places the player can read from a distance, approach by boat, circle, survey and remember. The goal is not to fill every shoreline with ruins. Places that contain human remains should feel specific to their use, age, environment and later disturbance.

Physical remains should answer small practical questions before raising larger ones: where did people land, what did they repair, what was worth carrying uphill, what was left because it was too heavy, and what did the sea move? A player should form a plausible account from arrangement and wear, then discover that one piece does not quite fit.

**Implementation status is split deliberately:**
- Greyhaven's lower-inlet landing, partial lower-settlement foundation and additional wreck framing are source-authored in `GreyhavenEnvironmentBuilder`.
- Greyhaven's existing harbour, workyard, estuary, lighthouse and outer-channel wreck dressing remains source-authored.
- The other five regional treatments below are design specifications only. The Outer Shoals and Hollow Trench survey sandboxes are not finished regional levels; their current survey geometry does not implement these physical-history sites.
- There is no region-travel feature that makes all six locations reachable in one continuous world.

## 2. Evidence language

Use three scales, but do not force all three into every location.

1. **Landform and structural silhouette:** a line of old piles, collapsed roof plane, reef-marker stump, hull spine or broken retaining edge visible through fog or from the boat.
2. **Work/use pattern:** paired mooring points, drainage route, fish-processing surface, winch approach, repeated steps, tool marks or a cleared work patch.
3. **Small residue:** one or two objects implying routine or interruption, such as a float caught on a nail, a cup behind a threshold, an unused pulley, a broken latch or ceramic trapped in silt.

Large sites need quiet areas with no clue, loot or dramatic reveal.

### Ambiguity rules

- A physical fact can be clear while its cause remains uncertain. A broken pile is visible; storm, salvage or rot may explain it.
- Show time through repair layers and material reuse, not one dramatic destruction pose.
- Old fastener holes may show that a beam moved; they do not prove which building it came from.
- Water movement, sediment, corrosion, frost, wave action and marine growth explain much of the damage. The Choir is not responsible for ordinary decay.
- No remains provide required navigation directions, a safe route, an exact bearing or a hidden hazard warning.
- Authoring labels and prop names are never player-facing text.

## 3. Greyhaven — a harbour built over its earlier edge

### Western Mudflat: lower-inlet landing

**Source-implemented; Unity review pending.** The landing is partial: three stone footings, cut-down timber piles, incomplete deck planks, a broken winch frame, mooring ring and loose line coil. Its footprint should read as a landing that once reached farther into the water than the present shore permits.

The contrast between pile heights, sediment level and broken deck line suggests that the shore/water relationship changed and the landing was not rebuilt in its old position. Do not add a sign or date that settles the chronology.

### Western Mudflat: old settlement foundation

**Source-implemented; Unity review pending.** A low foundation outline has partial wall footings, a threshold, reused timber sill and collapsed rafter. It is not a complete house. Its use could have been a store, net room or small dwelling; the arrangement should not decide the answer.

The landing and foundation belong to the western estuary root at negative world X. Eastern mudflat debris uses its own positive-X top-level root so each group streams into the correct area scene.

### Outer-channel wreck field

**Source-implemented; Unity review pending.** The existing broken hull, deck plate and beam remains are reinforced with an exposed keel fragment, partial ribs, crossmembers and sheared mast spar. Existing bell, case, ceramic, glass, canvas and rope details remain. The wreck should read as a hull broken open and partly stripped, not a clean intact ship placed on the seabed.

No body, named passenger, full manifest or definitive cause of loss is added. Keep this bell distinct from the Greyhaven lighthouse bell and Drowned Parish bell structure. The wreck is already a salvage/survey site; the added visuals do not add salvage yield, interactions, discoveries or save state.

### Existing Greyhaven traces

Harbour dressing remains the active working layer: patched crates, spools, floats, rope, an oar and cup. Village workyards contain market tally residue, retired machinery, radio casing, net weights and rain collection. The lighthouse has abandoned keeper equipment, storm shutter, cracked signal mirror and closed log case. These are smaller than the settlement/landing silhouettes.

## 4. The Shallows — infrastructure that respects a living reef

**Proposed; no finished regional scene.**

- **Split Reef marker line:** two weathered marker bases remain on separate reef heads, but only one upright survives. The empty socket and broken fasteners say more than a readable label. A later chart may copy the old pair although the reef cut has changed.
- **Glass Channel stakes:** mismatched stakes transition from upright markers to buried tips and empty holes, suggesting a passage repeatedly moved or remarked.
- **Ribbon Meadow working edge:** low stakes, rolled net, repaired float line and a small cleared patch show controlled access to a nursery ground. Avoid heavy anchors and dragging scars through seagrass.
- **Reef salvage fragments:** one anchor scar and a few timber/corroded-fastener fragments collect in a sand gutter. Damage is local; the reef remains alive.

The player compares the old marker line with the present water passage and learns that charts can preserve obsolete routes without supernatural intervention.

## 5. Blackrock Reach — a coast used only when conditions allow

**Proposed; no finished regional scene.**

- **Lee Shelf seasonal camp:** low stone windbreak, ash-dark hearth, two empty bedding recesses, driftwood rack and short drying line. It must look temporary, not like a permanent town.
- **Backwash Cut work evidence:** rope groove in a rock anchor point, broken float and timber wedge show crews tried to secure gear in a surge channel. These traces do not imply the channel is safe.
- **Glass Cave approach:** damaged mooring eye outside the cave and wave-rounded fragments below it. Do not put a written warning at the cave mouth; exposure and sound should communicate risk.
- **Wreck fragments:** stripped spar, bent fastening and separated hull planks wedged beyond the surge line; no intact wreck where repeated waves would redistribute it.
- **Second Knock connection:** hard narrow surfaces and a sheltered recess provide a plausible reflected-sound path. Two differently worn hammer heads may remain at camp, but no object proves a supernatural impact.

Use fewer loose props, more missing/dragged material, hard basalt silhouettes and isolated sheltered pockets.

## 6. The Outer Shoals — survey evidence without a stable coastline

**Proposed; no finished regional scene.**

- **Three Bearings station:** three low survey marks occupy different headings around a submerged ridge. One has been replaced, one offset and one nearly buried; do not arrange them as a perfect triangle or compass rose.
- **Sounding gear remains:** weighted lead line, broken reel frame and marked cable near a temporary work platform. This is not a second working Sounding Rig or a collectible.
- **Empty Buoy recovery:** lamp housing and date plate lie apart from the severed chain; the anchor is absent. Abrasion, snagging or failure should be plausible.
- **Route drift:** weed lines, foam and sediment fans obscure old marker stubs. Nothing acts as GPS.
- **Wreck scatter:** hull planks and fasteners follow a current seam rather than forming a tidy ship silhouette. Orientation suggests drift, not a proven final heading.

The region is about incomplete measurements. Do not fill the horizon with buoys or make every lost sounding a wreck.

## 7. The Drowned Parish — a settlement that moved in layers

**Proposed; no finished regional scene.** This is the largest abandoned-settlement environment, but it must not look like one town submerged in a single event.

### Lower landing and working edge

Show repairs of different ages: old stone step, later timber facing, cut mooring ring, filled post hole and ramp that no longer reaches navigable water. Some supports are removed cleanly; others broke. Some are buried rather than dramatically tilted.

### House and work-yard footprints

Use partial foundations, thresholds, hearth pads, drainage cuts and distinct yard surfaces. One footprint has a low store base and reused sill; another only two corners and a path; a third is mostly sediment with roof tiles. Do not fill each footprint with a complete ruin.

### Upper Steps and relocation path

The stair route is a sequence of repairs: old stone treads, newer timber bracing, patched retaining wall and later path on higher ground. Reused beams may continue into Greyhaven, but fastener holes are suggestive, not proof.

### Bell House and Record Room

The bell mount remains with bolts and weather shadow, but the bell is absent. The room has had multiple uses; later shelving may cover an earlier wall line. The record room contains fragments of lists, receipts, paper fibres and copied notices, not a complete chronicle.

The route from lower foundations to Upper Steps makes relocation legible. Conflicting accounts of the last gathering remain unresolved. The Parish bell is distinct from the lighthouse bell and wreck bell fragment.

## 8. The Hollow Trench — instrument traces and natural succession

**Proposed; existing survey sandbox only.** The Trench is not an abandoned town. Human remains are sparse and mostly instrumental.

- **Upper Slope survey line:** broken weighted line, corroded attachment and instrument housing show repeated depth work. No one line proves the Orison's final course.
- **Canyon-edge reference:** bent frame or detached transducer mount near hardground; not a permanent navigational beacon.
- **Seep Field:** mineral crust, fluid-stained sediment and specialised animal communities are geological/ecological evidence, not machinery.
- **Brine Boundary:** sediment lip and corrosion gradient show dense saline water; not a portal or artificial wall.
- **Fall Site:** whale-fall remains show ecological succession and must not be confused with human wreck debris.
- **Orison fragments:** only scattered, non-identifying equipment may be introduced after story evidence gates. The Orison was lost in Year -8; fragments must not reveal the crew's final moments or cause.

Keep deep space mostly empty. One instrument fragment carries more weight than an implausibly intact research vessel at trench depth.

## 9. Wreck design rules

1. Give each wreck a material and environmental history: construction, depth, exposure, current, sediment, salvage access and time since loss.
2. Break the silhouette asymmetrically. A shipwreck is not a symmetrical prop arrangement.
3. Separate hull structure, cargo residue and personal effects. Do not scatter all three arbitrarily.
4. Let salvage remove useful objects and leave awkward or low-value pieces. Absence can be stronger than a pile of loot.
5. Do not add human remains or named personal effects solely for shock. Each personal object needs a narrative reason and evidence boundary.
6. A wreck does not automatically become a salvage site, survey discovery, quest target or map marker. Those mechanics retain their existing owners.

## 10. Implementation and quality constraints

- Generate static remains beneath the owning scene/streamed-area root; do not create a duplicate runtime decoration manager.
- Use deterministic Editor generation and shared materials. Avoid per-prop lights, Update methods, rigidbodies and unnecessary packages.
- Keep decorative geometry collider-free. The existing Greyhaven SalvageSite trigger remains the only wreck interaction owner.
- Split top-level streamed roots by actual world-space region; do not group west/east assets beneath an origin root.
- Keep harbour lanes, mudflat approach, wreck interaction radius, camera sightlines and region silhouettes readable.
- A future inspectable remains system requires stable IDs, accessible text/audio, duplicate handling, save behavior and clue redundancy as a separate design.
- Rebuild generated scenes after changing the Editor builder. Generated scenes are output, not the source of truth.

## 11. Acceptance and verification

### Source-level acceptance

- Western lower landing and old foundation are children of the western estuary ownership root.
- Eastern mudflat debris has its own positive-X top-level root.
- Wreck frame details remain children of the existing wreck field and use parent-local coordinates.
- Added remains create no collider, rigidbody, runtime MonoBehaviour, Light, save participant, loot entry, quest state or survey site.
- Rebuilding is deterministic and creates no duplicate groups.

### Unity review still required

- Rebuild Greyhaven and all six streamed area scenes in Unity 6000.3.28f1.
- Verify grounding and overlap at gameplay camera height.
- Confirm west/east scene ownership by unloading and reloading each side independently.
- Confirm the landing does not block shoreline travel or boat approach.
- Review wreck frame visibility beneath the water and from normal boat camera.
- Run Edit Mode artifact tests, Play Mode streaming transitions and a Windows Development Build.
- Measure draw calls, memory and frame time before setting final environment budgets.

Static validators and C# syntax checks are not Unity semantic compilation, Editor generation, camera review, Play Mode or performance evidence.

## Related documents

- [Regional Histories and Maritime Tales](REGIONAL-HISTORIES-AND-MARITIME-TALES.md)
- [Greyhaven settlement and wayfinding](GREYHAVEN-SETTLEMENT-AND-WAYFINDING.md)
- [Environmental Storytelling](../02_LORE_AND_WORLD/ENVIRONMENTAL-STORYTELLING.md)
- [Environment Assets](../09_ART_AND_VISUAL_DIRECTION/ENVIRONMENT-ASSETS.md)
- [Location Catalogue](LOCATION-CATALOG.md)
- [Regional Ecology](REGIONAL-ECOLOGY-AND-CREATURES.md)
