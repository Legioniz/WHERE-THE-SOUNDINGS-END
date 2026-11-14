# Historical Artifacts and Archival Evidence

> Project: WHERE THE SOUNDINGS END  
> Status: Greyhaven artifact anchors and inspection/save path source-implemented; Unity verification pending; other-region catalog proposed  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-14

## 1. Player promise

Historical artifacts make the world legible through records that survived imperfectly. A chart has been copied and corrected. A letter was never sent. A photograph has an annotation added years later. A survey instrument has been recovered without a reliable chain of custody. The object is evidence; the interpretation is the player's work.

Artifacts are not a conventional collectible checklist. They should feel like things people made, carried, repaired, copied, used and sometimes abandoned. Their importance comes from where they are found, how they were preserved, who can plausibly have handled them and what they cannot prove.

The first playable artifact set is located in Greyhaven. Other regions have proposed artifact families in this document, but are not represented as implemented scenes. The copied chart, repair tool roll, keeper's ledger and photograph are read through existing dockside furniture menus; the lower-landing letter and wreck recorder use direct close-range vessel inspection.

## 2. Artifact types and record contract

The runtime content type is stable and intentionally small:

| Type | What it can preserve | What can make it unreliable |
|---|---|---|
| Letter | Personal concern, request, disagreement, intended recipient | Missing date, water damage, no envelope, copied or incomplete text |
| Logbook | Repeated observations, maintenance, weather, soundings, work records | Replacement pages, inconsistent clocks, later handwriting, gaps |
| Map | A route or place as understood at one time | Copying, changed shoreline, obsolete bearings, unknown datum, later corrections |
| Photograph | Arrangement, clothing, structure, light and visible people | Unknown camera position/date, crop, damaged emulsion, later annotation |
| Tool | Work practice, repair habits, repeated use and modification | Mixed ownership, replacement parts, reused tools, no maker's mark |
| Survey equipment | Method, measurement capability, recovery history | Calibration uncertainty, damaged serial plate, missing cable, incomplete custody |

Every inspectable record has:
- A stable content ID that is never derived from a Transform, hierarchy path, array index or generated instance ID.
- Region ID and artifact type.
- Display title and date/period statement.
- Physical condition and provenance statement.
- A short reading or inspection note.
- An explicit uncertainty boundary where the object cannot establish its own attribution.

Keep source text separate from an author's interpretation. Do not present a later marginal note as if it were part of an original letter, logbook or photograph caption. Use phrases such as “the surviving line reads”, “a later hand added”, “the date is missing”, and “the serial plate is incomplete” when those limitations matter.

## 3. Greyhaven — first implemented archival set

**Source-implemented; Unity scene rebuild and interaction review pending.** The generated Greyhaven environment builder now authors six metadata anchors under a persistent archive root. Their physical render-only counterparts remain under their owning environment roots. They use the existing vessel Interact action and close-range inspection controller.

### 3.1 Working Chart of the Lower Inlet

- **ID:** `greyhaven.quay.working-chart`
- **Type:** Map
- **Location:** Fixed chart table on the east side of Greyhaven's outer T-pier
- **Condition:** Fold lines, salt bleaching and route marks in at least two hands
- **Provenance:** A copy of an older Harbour Office sheet; original date uncertain

The main line follows the old lower landing before bending toward a later marker position. Depth figures stop before the outer ridge. A pencil correction is visibly in a different hand. It records how the chart changed, not a safe route or a current navigational instruction. The chart does not reveal unknown regions or place marks on the regional chart.

### 3.2 Repair Yard Tool Roll

- **ID:** `greyhaven.repair.tool-roll`
- **Type:** Tool
- **Location:** Tomas's repair yard
- **Condition:** Oil darkening, mismatched fasteners and replacement handles from different periods
- **Provenance:** Kept in the repair yard; ownership of individual tools is uncertain

A worn spanner and brass dividers share space with improvised repairs. Their wear is consistent with repeated engine and line work. The tools are a mixed working set, not proof that one surveyor owned or used every piece.

### 3.3 Unposted Letter from the Lower Landing

- **ID:** `greyhaven.western-mudflat.unposted-letter`
- **Type:** Letter
- **Location:** Western mudflat, beside the abandoned lower-inlet landing
- **Condition:** Paper swollen by water; only short lines of ink survive
- **Provenance:** Recovered at the landing; addressee and writer are absent

The surviving lines mention a marker moved between soundings and ask that an old copy not be used as a route. The final sentence is missing. The note does not identify its writer or prove why the landing was abandoned.

### 3.4 Keeper's Weather Ledger

- **ID:** `greyhaven.lighthouse.keeper-logbook`
- **Type:** Logbook
- **Location:** Western lighthouse keeper's station
- **Condition:** Salt-stiff pages; replacement leaves sewn into an older spine
- **Provenance:** Original and copied entries are mixed

The last confirmed lighthouse bell sounding is entered in Year -11. Later entries describe lamp, lens and shutter maintenance but do not confirm another sounding. The ledger does not explain why the bell fell silent. This is the lighthouse bell, not the wreck's bell fragment or the separate Drowned Parish bell structure.

### 3.5 Glass Plate: The Lower Landing

- **ID:** `greyhaven.lighthouse.keeper-photograph`
- **Type:** Photograph
- **Location:** Keeper's effects near the lighthouse
- **Condition:** Scratched emulsion; one corner cut away
- **Provenance:** Kept among the keeper's effects; place and subjects are not conclusively identified

Four figures stand near a low landing with three visible piles. A later pencil annotation reads “before the second move”. That note may refer to the settlement's relocation, but it is not contemporary with the image and cannot date the photograph. No person is identified.

### 3.6 Recovered Bottom Recorder

- **ID:** `greyhaven.outer-channel.recovered-recorder`
- **Type:** Survey equipment
- **Location:** Outer Channel wreck field
- **Condition:** Pressure-crushed housing, seized cable drum, abraded serial plate
- **Provenance:** Outer Channel salvage field; the recovery tag is a later copy

The casing contains a mechanical depth trace and a damaged timing wheel. The surviving serial digits do not match a complete Harbour Office register entry. The recorder establishes that survey equipment was recovered. It does not identify the vessel, operator or reason for loss.

## 4. Proposed artifact families in the other regions

The lists below are content proposals, not existing interactables or completed regional scenes. Any future artifact must have a plausible physical location, discovery boundary, reading text, stable ID, accessibility treatment and save integration before it is implemented.

### The Shallows

- A copied reef-marker sheet with the missing marker crossed out in a later hand.
- A tide and catch notebook that records the old Glass Channel as usable at a water level that no longer occurs.
- A lead line with worn depth marks and a replacement weight, left near a working edge rather than on living coral.
- A damaged field photograph of the Ribbon Meadow stakes, with no geotag or exact route.

The artifacts should teach that charts age and local knowledge changes. They must not become an in-world GPS or encourage damaging the seagrass nursery.

### Blackrock Reach

- A short camp letter folded inside a weathered oilskin pouch.
- A hammer with a replacement head and an old wedge, showing practical repair work at the Lee Shelf.
- A photograph of Glass Cave's approach taken from a sheltered rock shelf; the photographer's heading is unknown.
- A rope gauge or hand-measured line record with a note about surge timing.

The objects show temporary use of exposed coast. None certify that Backwash Cut or Glass Cave is safe to approach.

### The Outer Shoals

- A three-heading sounding notebook with separate observations and a deliberately blank final line.
- A damaged brass buoy plate recovered away from its chain and anchor.
- A broken reel frame, marked lead line and cable counter from a survey platform.
- A partial recorder trace whose second copy contains a detail absent from the original.

Keep the original record and later copy visibly distinguishable. A copied sound report must not silently become a confirmed event or a direction to follow.

### The Drowned Parish

- A folded household letter that refers to moving a load uphill, with no single final evacuation date.
- Two photographs from different periods showing a repaired landing and a later empty yard.
- Fragments of a land/property register, including duplicate entries and crossed-out holdings.
- A Bell House maintenance ledger with a removed-rope entry that conflicts with a later oral account.
- A route sketch of Upper Steps that shows multiple repairs and path changes.

Do not create one complete parish book explaining the whole settlement. The relocation should be inferred from multiple incomplete sources. The Parish bell remains separate from the lighthouse bell and the wreck fragment.

### The Hollow Trench

- A damaged depth recorder and cable counter, with a calibration note and no complete operator identity.
- Pressure logger strips and a recovered instrument housing from separate dives.
- A maintenance checklist for a lost transducer mount.
- Orison-related fragments only after the story's evidence gates permit them; keep the Year -8 loss and the protagonist's uncertainty intact.

The Trench is not an archival warehouse or abandoned research base. Instruments should be sparse, damaged and partially separated by depth and current. Do not attribute a fragment to a named crew member unless the campaign explicitly earns that conclusion.

## 5. Inspection, discovery and persistence rules

- No new keybind or duplicate interaction manager is introduced. The existing Interact action opens nearby dockside menus; four records are read through their archive furniture entries. The lower-landing letter and wreck recorder use the same close-range inspection controller directly.
- Artifact anchors use stable IDs and the existing close-range inspection controller. The six Greyhaven metadata anchors are placed under the persistent scene's historical-artifact anchor root, outside static area-stream candidate hierarchies; their visual props still belong to the correct streamed-area roots.
- The artifact inspection ledger is separate from discovered phenomenon-site IDs, Sounding Rig discoveries, visited regions, NPC relationships and quest state.
- Reading an artifact does not reveal a region, place a chart mark, grant a bearing, update a quest, change an NPC relationship or create a generic journal collectible.
- Re-reading an artifact reports that it has been examined before and does not duplicate persistent state.
- Artifact inspection does not apply the one-time sea-disturbance consequence used by first-time phenomenon-site inspection.
- Save schema v9 adds `WorldSaveData.inspectedHistoricalArtifactIds`. The v8-to-v9 migration creates an empty list for older saves; it never copies sonar-site IDs into the artifact ledger.
- Keep existing save backups, schema validation limits and transactional restore semantics. Do not serialize GameObject references, object names or hierarchy paths as artifact identity.

Direct lower-landing and wreck inspections use the existing field-note panel. Dockside archive entries show the same record in the furniture detail panel. DocksideHubController executes before the direct inspector so an accessible dockside menu takes priority over a competing Interact action. A later UI pass may improve pagination, document zoom and accessibility, but it must preserve the boat-only interaction model until an on-foot controller is deliberately approved.

## 6. Narrative and evidence rules

1. Every artifact must be plausibly located where it was used, dropped, stored, recovered or moved.
2. Separate original writing, copies, annotations, transcriptions and translations.
3. A document can be authentic while its interpretation is wrong. Show the physical record and let sources disagree.
4. Do not fill every site with an artifact. Empty rooms, missing pages, removed tools and absent cargo are evidence too.
5. Do not use artifacts to bypass the fog-of-war chart, provide exact bearings or deliver mandatory hazard warnings.
6. Do not use an artifact as a disguised quest flag unless its owning quest design explicitly requires that behavior and has redundant clue access.
7. Keep ordinary material decay, human relocation, salvage and ecological processes separate from the Choir. The presence of an old instrument is not proof of an anomaly.
8. No named victim, crew attribution or final-moment transcript is added merely for shock.

## 7. Art and implementation constraints

- Use the deterministic Greyhaven Editor builder as the source of generated physical props.
- Render props are shared-material, static, collider-free geometry; no Rigidbody, per-prop Light or Update component is added.
- Inspection metadata anchors remain under the persistent scene's dedicated root and are not children of the six static streamed-area scene candidates.
- Do not parent a metadata anchor beneath a decorative visual prop. Keep render geometry and interaction/save ownership separate.
- Rebuild generated scenes after builder changes; inspect position, height, camera readability, prop overlap, and stream transitions in Unity.
- No third-party assets are introduced by this pass. Existing Unity primitive meshes and project-authored materials serve as greybox representations; final document textures, handwriting, photograph grain and custom tool models remain art tasks.

## 8. Acceptance and verification

### Source-level acceptance

- Six Greyhaven anchors exist with unique stable IDs and all six artifact types.
- The builder is wired into Greyhaven generation and requires the established roots/materials.
- Visual artifacts remain static and collider-free beneath their correct area roots.
- Metadata anchors remain outside static streamed candidates and are discoverable by the existing controller.
- First inspection and repeat inspection produce different feedback but the same stable ID.
- Capturing/restoring the save ledger preserves inspected artifacts without altering inspected site IDs.
- Schema 8 migrates to 9 with an empty artifact ledger and preserves existing survey inspection data.
- Save validation bounds the new collection and rejects duplicate/invalid IDs.

### Unity review still required

- Open Unity 6000.3.28f1 and rebuild Greyhaven.
- Read the chart, repair tool roll, keeper ledger and photograph through dockside archive entries; approach the lower-landing letter and wreck recorder for direct Interact inspection. Re-read each and confirm previously-examined feedback.
- Save and reload after reading a subset; confirm the known/first-read status persists.
- Confirm reading artifacts does not add site discoveries, chart marks, region names, quest state, NPC relationships or sea disturbance.
- Unload/reload estuary and lighthouse streaming areas while keeping the persistent metadata anchors valid.
- Verify visible paper/tool/photograph/recorder props are grounded, readable at camera height and do not block the chart table, repair frontage, shoreline or wreck approach.
- Run the new Edit Mode tests, all save migration tests, Play Mode interaction/streaming tests and a Windows Development Build.

Static repository checks and syntax parsing do not constitute Unity compilation, generated-scene review, Play Mode or performance evidence.

## Related documents

- [Environmental Storytelling](../02_LORE_AND_WORLD/ENVIRONMENTAL-STORYTELLING.md)
- [Regional Physical Remains](REGIONAL-PHYSICAL-REMAINS.md)
- [Regional Histories and Maritime Tales](REGIONAL-HISTORIES-AND-MARITIME-TALES.md)
- [Greyhaven](GREYHAVEN.md)
- [Save and Load](../06_GAMEPLAY_SYSTEMS/SAVE-AND-LOAD.md)
- [Save Data Structure](../08_TECHNICAL_DESIGN/SAVE-DATA-STRUCTURE.md)
- [Testing Strategy](../14_TESTING_AND_QUALITY/TESTING-STRATEGY.md)
