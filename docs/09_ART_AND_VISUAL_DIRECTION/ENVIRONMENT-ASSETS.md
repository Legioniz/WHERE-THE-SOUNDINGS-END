# Environment Assets and Set Dressing

> Project: WHERE THE SOUNDINGS END  
> Status: Working visual specification — Greyhaven generated artifact pass source-implemented; final art and Unity review pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## 1. Visual target

The environment should read as a cold, isolated working coastline: weathered timber, dark basalt, stained concrete, oxidized metal, salt-faded canvas, cloudy glass, wet sediment, and restrained warm practical lights. The 2.5D elevated camera must preserve strong silhouettes and useful negative space.

The current Greyhaven builder remains a deterministic greybox/set-dressing generator. Primitive geometry and generated materials are layout and composition placeholders, not final production art.

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
- Estuary: damaged fish trap, driftwood, glass float, old tide stake, boot, ceramic fragments and rope.
- Lighthouse: abandoned keeper equipment, storm shutter, cracked signal mirror and a closed log case.
- Wreck: bell fragments, a personal case, ceramic/glass fragments, canvas, rope and a broken rail section.

The pass uses generated primitive meshes and four muted material variants (oxidized iron, chipped ceramic, salt-faded canvas, and tarred rope) alongside existing wood, metal, glass and paper materials. It is not a final bespoke model/texture/UV pass.

Artifact cylinders and spheres use parent-local transforms, including the offset wreck transform, so the cluster stays correctly attached when its streamed root moves. All dressing geometry is created without colliders, rigidbodies, lights, or runtime components. It does not become loot, an inspection target, a survey discovery, a map mark, or save data. The streamed-area revision is incremented so generated scene validation recognizes the new content revision.

## 5. Performance and asset ownership

- Keep repeated primitive materials shared through the existing material factory and enable GPU instancing where supported.
- Do not add per-prop lights, Update loops, rigidbodies, or scripts.
- Keep all static geometry beneath the correct builder-owned area root.
- Avoid Resources.UnloadUnusedAssets on every transition; use the existing area-streaming ownership and telemetry.
- The current builder does not establish a final draw-call, memory, GPU, or frame-time budget. Measure in a Windows Development Build on representative hardware before setting one.

## 6. Provenance and final asset pipeline

No third-party models, textures, audio, fonts, or code are introduced by this pass. Generated Unity primitives and project-authored materials are used.

Before replacing greybox pieces with external or generated production assets:
- record the source, author/provider, license, commercial-use rights, modification rights, attribution, and file hash where applicable;
- validate mesh scale, pivots, normals, materials, texture dimensions, shader compatibility and LOD behavior;
- test collision separately from render meshes;
- preserve stable object/area ownership and generated-scene rebuild behavior.

## 7. Acceptance criteria

- Readable at the target gameplay camera height and resolution.
- No visual clutter obscures vessel navigation or service locations.
- No visible floating/interpenetrating artifacts after Unity rebuild.
- Static props do not create extra colliders, scripts, lights, rigidbodies, or save state.
- Streamed ownership remains valid and the new revision is accepted by StreamedAreaSceneRoot.
- Final material/mesh quality and performance are reviewed in Unity; repository syntax/static checks alone are insufficient.
