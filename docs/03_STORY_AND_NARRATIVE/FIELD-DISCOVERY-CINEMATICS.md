# Field Discovery Cinematics

> Status: Implemented in source; Unity presentation and campaign save/load verification pending  
> Scope: Greyhaven prototype, catch and dredge recovery

## Purpose

Give first-time recoveries a distinct sense of discovery without turning every catch into a cutscene. The sequence is an archival field record: it identifies the recovered item, records that it was secured, and deliberately avoids explaining its origin or supernatural significance.

## Trigger and persistence

- A catch registers only after the catch has been placed in the spatial cargo hold.
- A dredge object registers only after its cargo placement succeeds and the reserved salvage pull commits.
- Stable item IDs are recorded in `ProgressSaveData.discoveredEntryIds` using `field-discovery:` as a namespace.
- Repeated recovery of an already registered item ID does not replay its reveal.
- Existing save schema version 9 is reused; no migration is required.
- If the cinematic front end is unavailable, the discovery is still recorded. The item itself remains in cargo.

## Cinematic sequence

Each first recovery presents two beats:

1. **Specimen / object record** — item name, recovery type and live camera framing over the vessel and water.
2. **Field journal entry** — a short reminder that condition, habitat and recovery site are evidence, not an explanation.

The director reuses the current cinematic pause, skip, reduced-motion, caption reveal and camera restoration behavior. A short exposure dip separates authored shots. No separate scene, new camera, voiceover, licensed footage or new input action is introduced.

## New generated catalogue entries

### Fish

| ID | Name | Grounds | Method class |
|---|---|---|---|
| FSH-101 | Pacific sailfish | Outer Shoals | Pelagic |
| FSH-102 | Cobia | Greyhaven, Shallows, Outer Shoals | Pelagic |
| FSH-103 | Red cornetfish | Shallows, Blackrock Reach, Outer Shoals | Reef |
| FSH-104 | Black marlin | Outer Shoals | Pelagic |
| FSH-105 | Giant oarfish | Hollow Trench | Deepwater |

The generated catalogue applies the existing market-class defaults for habitat, depth, preferred technique, stock and resistance. The species are not guaranteed encounters: region, habitat, depth, stock, technique, bait and current simulation conditions still determine eligibility and selection weight.

### Archive-grade salvage

- **SALVAGE-ARCHIVE-01 — Brass Sounding Weight**
- **SALVAGE-ARCHIVE-02 — Ceramic Hydrophone Insulator**
- **SALVAGE-ARCHIVE-03 — Salt-Clouded Signal-Lamp Lens**
- **SALVAGE-ARCHIVE-04 — Water-Sealed Chart Tube**

These are cargo rewards in the existing salvage pool. They are not ordinary market stock and have no configured sale price. Their origin is unconfirmed.

## Acceptance and verification

- First item recovery adds exactly one stable discovery ID.
- Duplicate recovery does not replay the reveal.
- Discovery IDs survive capture/restore through the existing progress list.
- Failed cargo placement or uncommitted salvage reservations do not register a new salvage object.
- Camera transform, zoom and follow state restore after completion and skip.
- Reduced-motion mode retains all record text and sequence order while suppressing authored camera movement.

Unity Edit Mode/Play Mode execution, ScriptableObject regeneration, runtime presentation, probabilities, save/load integration and Windows player launch remain required. Static source validation is not a substitute for those checks.
