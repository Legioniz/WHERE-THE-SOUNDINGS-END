# Environmental Storytelling

> Project: WHERE THE SOUNDINGS END  
> Status: Working design specification — Greyhaven physical-remains extension source-implemented; other five regional treatments proposed; Unity verification pending  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-13

## 1. Player promise

The world should feel inhabited, used, repaired, weathered, abandoned, and occasionally disturbed without constantly explaining itself. Players should infer fragments of ordinary lives and past events from placement, wear, material, repetition, absence, and contradiction.

Environmental storytelling is not a substitute for authored dialogue or a required quest clue. A visual artifact may suggest a story without confirming one. The player is allowed to misunderstand it.

## 2. Design rules

1. **Show before telling.** Prefer physical evidence over explanatory labels, floating text, tutorial prompts, journal entries, or forced dialogue.
2. **No automatic interpretation.** Merely seeing an artifact does not create a journal entry, objective, map mark, quest flag, relationship event, or lore unlock.
3. **No false guidance.** Do not use glow, pulsing outlines, arrows, compass bearings, or loot-style presentation to imply that an environmental prop is a required objective.
4. **Ordinary life comes first.** Greyhaven is an operational settlement. Its spaces need work residue, maintenance, storage, personal habits, and signs of routine before overt supernatural evidence is layered in.
5. **Specificity over clutter.** A small cluster with a clear use and history is stronger than random prop scatter. Every cluster should have a reason to occupy its exact place.
6. **Preserve ambiguity.** Contradictory or uncanny objects may be present, but no single decorative object should confirm a major mystery or ending.
7. **Respect navigation and camera.** Props must not obstruct the vessel's turning lanes, service approaches, shoreline readability, or the elevated camera's view of important silhouettes.
8. **Visual props are not mechanics.** A prop does not become loot, collision, cover, an interaction target, or a quest item unless a separate gameplay implementation explicitly grants that behavior.

## 3. Environmental layers

- **Working layer:** ropes, crates, tools, nets, repair scraps, lamps, containers, fish-handling equipment, and weather protection.
- **Personal layer:** cups, clothing, keepsakes, worn cases, hobby objects, private collections, and evidence of repeated routines.
- **Weather layer:** salt bloom, faded canvas, rust, cracked paint, water staining, warped wood, sediment, and storm damage.
- **Historical layer:** obsolete fittings, repaired structures, old markers, reused materials, and debris whose original purpose is no longer obvious.
- **Anomalous layer:** rare spatial or material contradictions. Keep these restrained and separate from explicit threat telegraphs or required progression clues.

## 4. Greyhaven artifact pass

The first generated pass adds collider-free, non-interactive visual clusters to the existing six-area Greyhaven layout:

| District | Cluster | Story carried by the arrangement |
|---|---|---|
| Harbour | Split crate, net-repair spool, sea-glass floats, old lantern, rope coil, discarded oar, cup | The pier is actively used, patched, and shared; some belongings are left behind |
| Fish market | Reused crates, brine jar, tally slate and chalk residue | Daily sorting and accounting continue beyond the visible workstations |
| Repair shed | Retired propeller, radio casing without a dial, loose bearing, stained canvas | Repairs and experiments have accumulated over time |
| Net-mender yard | Net weights, old float line and mismatched floats | Work materials are reused rather than replaced |
| Inn threshold | Rain barrel, basin, folded oilskin | Residents and visitors leave traces of ordinary weather routines |
| Estuary | Broken fish trap, driftwood, glass float, tide stake, boot and ceramic fragments; collapsed lower landing with stone footings, partial decking and winch; low settlement foundation with threshold and reused sill | The inlet's working edge and an earlier settlement layer have been partly buried and dismantled; exact building use remains uncertain |
| Lighthouse | Keeper lantern, storm shutter, cracked signal mirror, sealed log case, rope ring | Maintenance has stopped or moved elsewhere; the reason remains unstated |
| Outer-channel wreck | Broken hull, exposed keel, partial ribs and crossmembers, sheared mast spar, bell fragments, personal case, ceramic/glass, canvas and rope | The vessel broke apart and was disturbed by time, current or salvage; no named victim, complete manifest or definitive loss account is exposed |

These are visual arrangements, not newly authored canonical history. Object names are implementation identifiers and must not be surfaced as player-facing text.

## 5. Placement and density

- Cluster props around plausible work surfaces, thresholds, sheltered corners, sediment pockets, mooring edges, and wreck structure.
- Keep clear movement and camera corridors around the central pier and all service approaches.
- Avoid uniform scatter or equal spacing. Use deliberate small groups with varied orientation, scale, and wear.
- Repeat a few practical material types across districts so the settlement feels like one economy; use distinctive personal objects sparingly.
- Use the deterministic Editor builder for repeatable placement. Random variation must be seeded and bounded if introduced later.
- Keep generated static props beneath their owning streamed-area roots. Do not attach runtime scripts, rigidbodies, or interaction components to static area roots.
- Use collider-free render geometry for dressing unless a separately specified gameplay obstacle requires a tested collider.

## 6. Interaction and discovery boundaries

The current artifact pass is intentionally non-interactive. The existing Sounding Rig site discovery and close-range phenomenon inspection systems remain separate and authoritative. Looking at or passing an artifact does not reveal a site, update the regional chart, change NPC dialogue, or write save data.

A future inspectable artifact system must define stable IDs, interaction range, text/audio accessibility, save behavior, duplicate handling, clue redundancy, and whether the object is optional or progression-critical before implementation. It must not retroactively turn every decorative prop into a collectible.

## 7. Horror escalation

- Start with believable work and domestic residue.
- Introduce isolated inconsistencies only after the ordinary environment is readable.
- Prefer absence, displacement, impossible wear, repetition, and mismatched scale over gore or obvious supernatural ornament.
- Never hide an essential hazard warning inside an ambiguous prop.
- Preserve the separate rule for unexplained indicators: no hint, note, map mark, bearing, or automatic explanation.

## 8. Acceptance criteria

- Generated clusters appear in their intended districts after the Greyhaven builder runs.
- Rebuilding produces the same hierarchy and placement.
- Dressing adds no colliders, rigidbodies, runtime scripts, save participants, or interaction targets.
- The main pier, service approaches, wreck interaction radius, and lighthouse silhouette remain readable.
- Western and eastern estuary dressing are separate top-level roots whose world positions place them in their corresponding streamed scenes.
- The collapsed landing and foundation remain render-only and do not become traversal obstacles, salvage nodes, survey discoveries, or quest evidence by themselves.
- Props remain with their correct streamed area and are unloaded with it where applicable.
- No artifact automatically writes a note, map mark, objective, discovery, or save value.
- Visual scale, material quality, occlusion, clipping, and density are reviewed in Unity at gameplay camera height.


## 9. Regional physical remains

The six-region treatment is specified in [Regional Physical Remains and Environmental Evidence](../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-PHYSICAL-REMAINS.md). It covers reef-marker infrastructure, seasonal Blackrock camps, Outer Shoals survey debris, the Drowned Parish's staged relocation layers, and the Hollow Trench's expedition traces. These five regional treatments are proposed content designs, not claims that additional playable scenes or travel links exist.

The Greyhaven source extension is narrower: the western mudflat contains a partial lower-inlet landing and a low old-settlement foundation outline; the wreck dressing has exposed frame members. All pieces remain deterministic, static and non-interactive. Unity generation, camera-height review, grounding and streamed-scene inspection are still required.
