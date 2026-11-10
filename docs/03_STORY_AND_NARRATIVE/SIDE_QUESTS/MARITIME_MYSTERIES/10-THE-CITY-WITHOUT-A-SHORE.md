# The City Without a Shore

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Underwater exploration / historical evidence  
> Inspiration: Drowned-settlement traditions and the literary account of Atlantis

## Short lore

> “The chart showed streets where the sea was deepest. The old mapmaker called it a city. The lighthouse keeper called it a mistake. The Sounding Rig found straight walls, then a road, then a doorway facing the open trench.”

A fragment of an old chart places a settlement beyond the known coastline. Its labels do not match any surviving village record.

## Player hook

The archivist asks the player to investigate whether the chart is a copied error or evidence of an unknown settlement. The player must collect physical evidence before making a claim.

## Player activity

1. Compare the old chart with current depth readings.
2. Find several aligned stone structures.
3. Recover a marked tile and a household object.
4. Determine whether the apparent street is a collapsed natural formation or built environment.
5. Return the evidence and choose how cautiously to describe the discovery in the village archive.

## Outcome

The evidence supports a former human-built structure, but it does not reveal who built it or why it lies underwater. The player can publish a cautious map note or an exaggerated rumour; NPC reactions differ.

## Reward

A new exploration region marker, a museum/archival entry, and a small payment for verified evidence.

## Dependencies

- Chart comparison.
- Sounding Rig mapping.
- Underwater ruin props.
- Dialogue choice and persistent archive text.

## Research and originality note

Atlantis is known primarily from Plato's literary dialogues and is not an archaeologically established lost civilization. The quest does not call the location Atlantis and does not imply that a real drowned city has been proven. It uses the general story motif of a place remembered after it was lost to the sea.

Reference starting point: https://en.wikipedia.org/wiki/Atlantis  
For historical accuracy, consult translations and scholarship on Plato's *Timaeus* and *Critias* rather than popular lost-civilization claims.

## Scope note

Use one small ruin cluster and three evidence objects. Do not expand this into an entire civilization until the core world map and story justify it.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- Survey begins after the archivist's chart clue and region unlock.
- Three primary evidence objects are authored and always present. Optional loose artifact chance: **15% per completed survey run**, evaluated once on first survey completion.
- Store the survey result and optional artifact roll. Reloading the site cannot reroll the loot or evidence.

### Behaviour states
`Survey Available → Structure Detected → Evidence Points Revealed → Samples Recovered → Report Choice → Archive Updated`.
The "city" is a static environment. It contains aligned walls, a road-like formation, tile, and household object. The player must compare geometry before the journal labels the location human-built. No autonomous city simulation is required.

### Perception / FOV
No creature FOV. Evidence markers appear within 12–20 m after the player scans the correct sector. Camera visibility is controlled by underwater reveal / fog. Important architecture receives silhouette lighting and depth cues; no tiny collectible is hidden outside the main camera view.

### Sound behaviour
Low-frequency returns and structural creaks are ambience only. They do not track the boat and must not use the Sleeper's attack motif. Caption examples: `[a hollow structure answers the Rig]`, `[stone shifts somewhere below]`.

### Attacks and chase
**None.** No monster, chase, collapse attack, or surprise damage. If a falling-silt visual is used, it is cosmetic and cannot damage the boat. Avoid a false "city wakes up" boss twist; this quest is about evidence and interpretation.

### Lore reveal
The recovered object suggests ordinary domestic life in a settlement that is now submerged. The cause and date remain uncertain. Archive wording must say "evidence consistent with a human-built structure" rather than claiming Atlantis or a lost super-civilization.

### Acceptance tests
- Three main evidence points are always available.
- Optional artifact roll occurs once and is saved.
- Player can understand the evidence without sound.
- No hostile AI or damage is triggered by the site.
