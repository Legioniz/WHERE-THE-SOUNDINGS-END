# Save Data Structure

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime schema v1 implemented  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Storage layout

The runtime writes one file per slot to Application.persistentDataPath/Saves/.

| Slot | Primary file | Backup file |
|---|---|---|
| 1 | slot_01.json | slot_01.json.bak |
| 2 | slot_02.json | slot_02.json.bak |
| 3 | slot_03.json | slot_03.json.bak |
| 4 | slot_04.json | slot_04.json.bak |
| 5 | slot_05.json | slot_05.json.bak |

Temporary writes use the .tmp suffix. Corrupt files are not treated as valid saves. A valid backup is loaded when the primary file cannot be read or validated.

## Schema v1

Root DTO: SaveGameData.

| Field | Purpose |
|---|---|
| schemaVersion | Migration version; currently 1 |
| slotNumber | Slot identity, assigned by the file service |
| saveId | Stable unique identifier for this save record |
| savedAtUtc | UTC ISO-8601 timestamp |
| gameVersion | Application version that wrote the file |
| sceneId | Unity scene required before restoration |
| totalPlaytimeSeconds | Campaign playtime |
| worldSeed | Campaign seed represented as a string to preserve exact integer text |
| world | Simulation clock, region, weather, tide and disturbance |
| player | Player transform, health and currency |
| vessel | Starter vessel transform, physics velocity and future condition values |
| inventory | Cargo stacks and equipment records |
| progress | Discoveries, unlocked regions, quests, journal, NPC state, regional stocks, flags, cooldowns and random streams |

All content relationships use stable string IDs. Random stream state and increment are represented as decimal strings so a future unsigned 64-bit random generator state is not rounded through a floating-point conversion.

### Data-only nested records

- Vector3SaveData stores position, Euler rotation, and velocity as numeric x/y/z fields.
- InventoryStackSaveData stores itemId, quantity, quality and freshness.
- EquipmentSaveData stores slotId, itemId and condition.
- QuestProgressSaveData and ObjectiveProgressSaveData preserve quest stage and objective progress.
- JournalEntrySaveData preserves entry identity, interpretation and read state.
- NpcStateSaveData preserves an NPC state and relationship identifier.
- RegionalStockSaveData stores per-region/per-species population and last simulation time.
- WorldFlagSaveData stores boolean or string flags.
- EventCooldownSaveData stores a stable event ID and the game-time deadline.
- RandomStreamSaveData stores the exact stream identity and state text.

## Migration policy

SaveGameData.CurrentSchemaVersion is authoritative.

- Version 0 is treated as the initial/unversioned document and normalized to the v1 structure. Missing nested groups and lists are created with safe defaults.
- Version 1 is current.
- A version greater than the running build's current version is rejected. It must never be loaded and rewritten by an older executable.
- Each future schema increase must add an explicit migration step and an Edit Mode test that loads a fixture from the previous version.

Do not repurpose an existing field with a new meaning. Add a new field, migrate, and retain the old value until the migration is proven.

## Runtime API

- SaveFileService.Save(slot, data)
- SaveFileService.TryLoad(slot, out data)
- SaveFileService.GetSlotInfos()
- SaveFileService.Delete(slot)
- SaveGameCoordinator.SaveSlot(slot)
- SaveGameCoordinator.LoadSlot(slot)
- SaveGameCoordinator.GetSlots()
- SaveGameCoordinator.DeleteSlot(slot)

The coordinator calls ISaveParticipant.CaptureSaveData and ISaveParticipant.RestoreSaveData for scene components that own mutable runtime state. Components should only write their own defined portion of the DTO.

## Ownership boundaries

The DTO is a transport/storage representation only. ScriptableObjects remain authored definitions; they are referenced by stable IDs. Runtime state belongs to systems and participants, not to the save service. Never place MonoBehaviour, GameObject, Transform, Rigidbody, delegates, input actions, or open file handles inside the DTO.

The present prototype wires only the starter boat. Player, clock, inventory, quest, regional simulation, and creature systems must register their own save participants as those systems are implemented.