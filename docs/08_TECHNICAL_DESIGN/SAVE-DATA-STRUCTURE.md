# Save Data Structure

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime schema v8 implemented; Unity verification pending  
> Owner: Project lead  
> Version: 0.4  
> Last reviewed: 2026-11-14

## Storage layout

The runtime writes five manual save files and one dedicated autosave to Application.persistentDataPath/Saves/.

| Slot | Primary file | Backup file |
|---|---|---|
| 1 | slot_01.json | slot_01.json.bak |
| 2 | slot_02.json | slot_02.json.bak |
| 3 | slot_03.json | slot_03.json.bak |
| 4 | slot_04.json | slot_04.json.bak |
| 5 | slot_05.json | slot_05.json.bak |

Temporary writes use the .tmp suffix. The automatic save uses autosave.json and autosave.json.bak; it is separate from all manual slots. Corrupt files are not treated as valid saves. A valid backup is loaded when the primary file cannot be read or validated.

## Schema v1

Root DTO: SaveGameData.

| Field | Purpose |
|---|---|
| schemaVersion | Migration version; currently 8 |
| slotNumber | Slot identity, assigned by the file service |
| saveId | Stable unique identifier for this save record |
| savedAtUtc | UTC ISO-8601 timestamp |
| gameVersion | Application version that wrote the file |
| sceneId | Unity scene required before restoration |
| saveKind | manual or autosave |
| saveReason | autosave trigger or checkpoint identifier |
| totalPlaytimeSeconds | Campaign playtime |
| worldSeed | Campaign seed represented as a string to preserve exact integer text |
| world | Simulation clock, region, weather, tide and disturbance |
| player | Player transform, health and legacy whole-dollar currency field |
| economy | Integer-cent USD wallet and processed transaction IDs for idempotency |
| vessel | Starter vessel transform, physics velocity and future condition values |
| inventory | Cargo stacks and equipment records |
| progress | Discoveries, unlocked regions, quests, journal, NPC state, regional stocks, flags, cooldowns and random streams |

All content relationships use stable string IDs. Random stream state and increment are represented as decimal strings so a future unsigned 64-bit random generator state is not rounded through a floating-point conversion.

### Data-only nested records

- Vector3SaveData stores position, Euler rotation, and velocity as numeric x/y/z fields.
- InventoryStackSaveData stores itemId, quantity, quality and freshness.
- CargoPlacementSaveData stores each physical cargo instance, placement, quality, freshness and saleQuantity in its authored sale unit.
- EconomySaveData stores walletCents and the bounded processedTransactionIds ledger.
- EquipmentSaveData stores slotId, itemId and condition.
- QuestProgressSaveData and ObjectiveProgressSaveData preserve quest stage and objective progress.
- JournalEntrySaveData preserves entry identity, interpretation and read state.
- NpcStateSaveData preserves stable NPC identity, activity/region, relationship score text and a bounded list of once-only relationship event IDs.
- RegionalStockSaveData stores per-region/per-species population and last simulation time.
- WorldFlagSaveData stores boolean or string flags.
- EventCooldownSaveData stores a stable event ID and the game-time deadline.
- RandomStreamSaveData stores the exact stream identity and state text.

## Migration policy

SaveGameData.CurrentSchemaVersion is authoritative.

- Versions 0 through 5 are migrated in ordered steps. The v5-to-v6 migration initializes NPC memory ledgers and maps the old unused zero relationship baseline to neutral 50.
- The v6-to-v7 migration initializes WorldSaveData.visitedRegionIds as an empty collection. It preserves existing ProgressSaveData.unlockedRegionIds; no legacy save is granted a region visit it did not record.
- Version 7 is current.
- A version greater than the running build's current version is rejected. It must never be loaded and rewritten by an older executable.
- Each future schema increase must add an explicit migration step and an Edit Mode test that loads a fixture from the previous version.

Do not repurpose an existing field with a new meaning. Add a new field, migrate, and retain the old value until the migration is proven.

## NPC relationship persistence

Save schema v7 is the current save contract and extends each NPC state record with `relationshipMemoryIds`, a per-resident list capped at 128 stable event IDs. Validation rejects null/oversized ledgers, duplicate or malformed IDs, and more than 4096 remembered events across the save.

Schema 5 NPC relationship scores were not modified by gameplay and used zero as their original default. The v5-to-v6 migration maps an empty or zero legacy score to neutral 50, initializes each memory list, and preserves any non-zero score. Later saves store the score and event ledger together.

## Regional chart persistence

Schema v7 adds WorldSaveData.visitedRegionIds. RegionalChartController records a region only after the vessel physically enters that region's DeepOceanSurveyRegion bounds. The same stable ID is added to the existing ProgressSaveData.unlockedRegionIds collection. A visit unlocks the chart record, not an inter-region travel route. The chart lists only visited regions and shows a phenomenon-site mark only when SoundingRigController has already discovered that stable site ID. Unknown regions and sites remain absent.

## Runtime API

- SaveFileService.Save(slot, data)
- SaveFileService.TryLoad(slot, out data)
- SaveFileService.SaveAutosave(data)
- SaveFileService.TryLoadAutosave(out data)
- SaveFileService.DeleteAutosave()
- SaveFileService.GetSlotInfos()
- SaveFileService.Delete(slot)
- SaveGameCoordinator.SaveSlot(slot)
- SaveGameCoordinator.LoadSlot(slot)
- SaveGameCoordinator.SaveAutosave(reason)
- SaveGameCoordinator.LoadAutosave()
- SaveGameCoordinator.GetSlots()
- SaveGameCoordinator.DeleteSlot(slot)
- SaveGameCoordinator.DeleteAutosave()

The coordinator calls ISaveParticipant.CaptureSaveData and ISaveParticipant.RestoreSaveData for scene components that own mutable runtime state. Components should only write their own defined portion of the DTO.

## Ownership boundaries

The DTO is a transport/storage representation only. ScriptableObjects remain authored definitions; they are referenced by stable IDs. Runtime state belongs to systems and participants, not to the save service. Never place MonoBehaviour, GameObject, Transform, Rigidbody, delegates, input actions, or open file handles inside the DTO.

The present prototype wires only the starter boat. Player, clock, inventory, quest, regional simulation, and creature systems must register their own save participants as those systems are implemented.

The Greyhaven dockside hub does not add a separate save DTO or schema field. Its Harbour Inn Guest Ledger calls the existing `AutosaveController.RequestCheckpointAutosave` path; cargo, vessel condition, sea state, NPC relationships and Yarn state remain owned by their existing save participants.


## Schema v8 economy migration

Schema v8 adds `EconomySaveData` with a USD wallet stored as integer cents and a processed-transaction ID ledger. The v7-to-v8 migration converts the legacy `PlayerSaveData.currency` whole-dollar integer to cents at 100 cents per unit while retaining the legacy field for compatibility. New cargo placement records also preserve sale quantity; legacy placements receive a safe default quantity of one until their item-specific unit is authored. `EconomyWalletController` owns the live wallet and idempotency ledger on the generated player vessel and participates in the existing save coordinator.

The migration and wallet test source is authored but still requires Unity Edit Mode execution. No sale/purchase transaction flow is claimed complete by this schema change.
