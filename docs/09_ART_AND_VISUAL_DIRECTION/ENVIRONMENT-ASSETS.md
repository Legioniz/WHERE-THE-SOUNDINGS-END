# Environment Assets and Set Dressing

> Project: WHERE THE SOUNDINGS END  
> Status: Working visual specification — bespoke Greyhaven maritime dressing source-implemented; final production meshes/textures and Unity review pending  
> Owner: Project lead  
> Version: 0.4  
> Last reviewed: 2026-11-14  

## 1. Visual target

The environment should read as a cold, isolated working coastline: weathered timber, dark basalt, stained concrete, oxidized metal, salt-faded canvas, cloudy glass, wet sediment, and restrained warm practical lights. The 2.5D elevated camera must preserve strong silhouettes and useful negative space.

The Greyhaven builder remains a deterministic generated environment system. The latest pass replaces the most visible repeated blockout silhouettes with assembled maritime forms, varied vegetation and region-specific wear. These are project-authored procedural assets, not final sculpted production meshes or authored texture sets.

## 2. Material language

| Material | Visual behavior | Typical use |
|---|---|---|
| Weathered timber | Desaturated brown, uneven edges, darkened seams | Piers, crates, repair structures, driftwood |
| Basalt and wet stone | Near-charcoal, low saturation, broad rough shapes | Headlands, breakwater, shoreline fragments |
| Oxidized metal | Brown-red rust with subdued highlights | Hinges, retired fittings, propeller parts, lantern frames |
| Salt-faded canvas | Grey-beige, matte, softened edges | Tarps, coats, sail scraps, net repair residue |
| Clouded glass | Cool green/grey, low clarity, restrained specular | Floats, lantern panes, radio faces, bottle fragments |
| Chipped ceramic | Warm grey-brown, matte, irregular silhouette | Cups, bowls, jars, shards |
| Tarred rope | Very dark brown, fibrous silhouette | Coils, lashings, net lines |
| Paper and slate | Warm paper, charcoal marks, low contrast | Logs, tally surfaces, charts and cases |

Avoid pristine bright metal, saturated toy-like color, decorative gold trim, and excessive emissive surfaces. Keep red for established navigation/safety language rather than general decoration.

## 3. Composition rules

- Establish each district with one or two readable landmark silhouettes before adding micro-detail.
- Use three density scales: large structural forms, medium prop clusters, and sparse small artifacts.
- Cluster props according to work and routine. Do not distribute objects as a uniform noise field.
- Vary orientation and wear while preserving readable silhouettes from the gameplay camera.
- Keep the centre of the main pier and service approach lanes open.
- Avoid piling small details into a single flat plane; use height variation, overlap, partial occlusion, and believable support surfaces.
- Keep objects visually grounded. Avoid floating, intersecting, or implausibly balanced props.
- Use restrained color contrast. Important navigation markers remain more legible than environmental clutter.

## 4. Greyhaven source implementation

GreyhavenEnvironmentBuilder.BuildEnvironmentalArtifacts adds deterministic, collider-free visual clusters beneath the harbour, village, estuary, lighthouse, and outer-channel wreck ownership roots. The current pass includes:

- Harbour: net repair residue, sea-glass floats, a worn lantern, rope coil, damaged oar and an unclaimed cup.
- Village: market crates and tally residue; a retired propeller and dismantled radio; net weights and mismatched floats; rain collection and oilskin at the inn.
- Estuary: damaged fish trap, driftwood, glass float, old tide stake, boot, ceramic fragments and rope; partial lower landing with footings, piles, broken deck planks and winch frame; low storehouse foundation outline with threshold and reused timber sill.
- Lighthouse: abandoned keeper equipment, storm shutter, cracked signal mirror and a closed log case.
- Wreck: bell fragments, personal case, ceramic/glass fragments, canvas, rope and broken rail section; exposed keel, partial ribs/crossmembers and a sheared mast spar make the site read as a broken vessel rather than only a debris pile.

The pass uses generated primitive meshes and four muted material variants (oxidized iron, chipped ceramic, salt-faded canvas, and tarred rope) alongside existing wood, metal, glass and paper materials. It is not a final bespoke model/texture/UV pass. The western estuary adds a collapsed lower-inlet landing and partial storehouse foundation; the outer-channel wreck adds exposed structural framing to its existing broken-hull mass.

Artifact cylinders and spheres use parent-local transforms, including the offset wreck transform, so each cluster stays attached to its owning streamed root. The visual props remain static, collider-free dressing and do not become loot, survey discoveries or chart marks. Six HistoricalArtifact metadata anchors are separate components beneath the persistent water root, outside static streamed candidates; the existing boat Interact/controller path owns inspection and save identity. The streamed-area revision is now 4. Western and eastern mudflat visual roots occupy their respective scene bounds. All six generated area scenes must be rebuilt.

## 5. Bespoke Greyhaven maritime pass

`GreyhavenMaritimeAssetBuilder.Build` is called by the Greyhaven scene generator and owns four distinct dressing families:

- **Harbour work zone:** paired mooring bollards with horns and rope lashings, deck cleats, framed glass lanterns, retired anchor, lobster pots, float line, rope coil, fender timber, and sparse salt/tar wear.
- **Village work yards:** stave-and-hoop barrels, slatted fish crates, a hand-powered repair winch, three-blade propeller, net-drying rack, wash tub, oil/mud scuffs and doorstep wear.
- **Estuary and tide line:** four irregular mangroves with bent trunks, exposed prop roots and layered crowns; several reed/salt-grass clumps; driftwood, collapsed crab pot, abandoned float line, half-buried anchor and silt/wrack patches.
- **Lighthouse headland:** keeper weather station with tripod, wind vane and rain gauge, storm lantern, rope/float remnants, wind-felled branch and stone salt staining.

The assets use assembled low-cost meshes from shared Unity primitive mesh resources, not single cubes scaled into every role. Their silhouettes and component detail are specific to the maritime object being represented. Wear is intentionally broad and sparse so it reads from the elevated camera without becoming noisy. District identity comes from different work patterns and ecology, not only color swaps.

All generated children in this bespoke maritime art helper are static render-only dressing: colliders are removed immediately; no Rigidbody, MonoBehaviour, Light, interaction, loot, discovery, map, journal, save or economy behavior is added. The separate HistoricalArtifact anchors are created by GreyhavenEnvironmentBuilder rather than this helper. The helper remains deterministic and uses the owning harbour, village, estuary and landmark roots. Streamed area content revision is now 4 so the six generated area scenes must be rebuilt.

## 6. Performance and asset ownership

- Keep repeated primitive materials shared through the existing material factory and enable GPU instancing where supported.
- Do not add per-prop lights, Update loops, rigidbodies, or scripts.
- Keep all static geometry beneath the correct builder-owned area root.
- Avoid Resources.UnloadUnusedAssets on every transition; use the existing area-streaming ownership and telemetry.
- The current builder does not establish a final draw-call, memory, GPU, or frame-time budget. Measure in a Windows Development Build on representative hardware before setting one.

## 7. Provenance and final asset pipeline

No third-party models, textures, audio, fonts, or code are introduced by this pass. Generated Unity primitives and project-authored materials are used.

Before replacing greybox pieces with external or generated production assets:
- record the source, author/provider, license, commercial-use rights, modification rights, attribution, and file hash where applicable;
- validate mesh scale, pivots, normals, materials, texture dimensions, shader compatibility and LOD behavior;
- test collision separately from render meshes;
- preserve stable object/area ownership and generated-scene rebuild behavior.

## 8. Acceptance criteria

- Readable at the target gameplay camera height and resolution.
- No visual clutter obscures vessel navigation or service locations.
- No visible floating/interpenetrating artifacts after Unity rebuild.
- Static props do not create extra colliders, scripts, lights, rigidbodies, or save state.
- Distinct maritime silhouettes and regional clusters remain readable from the target camera; vegetation does not obstruct navigation or service approaches.
- Streamed ownership remains valid and the new revision is accepted by StreamedAreaSceneRoot.
- Final material/mesh quality and performance are reviewed in Unity; repository syntax/static checks alone are insufficient.


## 9. Historical artifact visual layer

The Greyhaven builder adds physical representations of a copied chart and folded letter on the T-pier chart table, a repaired tool roll at the repair yard, a damaged letter and sounding lead near the western lower landing, a keeper's ledger and glass-plate photograph on the lighthouse headland, and a recovered recorder at the outer-channel wreck.

The physical meshes are static and collider-free. Their six metadata anchors sit beneath the persistent `Historical_Artifact_Inspection_Anchors` root, not beneath the visual props or streamed-area scene candidates. This preserves correct area streaming while the existing boat-only inspection controller owns archival reads. These are greybox forms, not final document textures, legible handwriting, photograph grain or bespoke tool models.
