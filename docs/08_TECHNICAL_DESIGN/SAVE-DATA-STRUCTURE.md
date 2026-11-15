# Save Data Structure

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime schema v9 implemented; Unity verification pending  
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
- EconomySaveData stores walletCents, the bounded processedTransactionIds ledger, and per-vendor/item current stock, target stock and lastRestockDay. Schema-v8 saves created before market stock was introduced receive an empty stock list during normalization; each vendor seeds missing stock from its authored target and begins restocking from the current game day. VesselSaveData stores main fuel and a separately saved emergencyFuel reserve. A legacy zero-balance wallet with no transaction history receives the one-time $25.00 starting balance; transaction history prevents repeat grants.
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

Save schema v9 is the current save contract and retains schema v8 economy records while extending each NPC state record with `relationshipMemoryIds`, a per-resident list capped at 128 stable event IDs. Validation rejects null/oversized ledgers, duplicate or malformed IDs, and more than 4096 remembered events across the save.

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

Schema v8 adds `EconomySaveData` with a USD wallet stored as integer cents and a processed-transaction ID ledger. The v7-to-v8 migration converts the legacy `PlayerSaveData.currency` whole-dollar integer to cents at 100 cents per unit while retaining the legacy field for compatibility. New cargo placement records also preserve sale quantity; legacy placements receive a safe default quantity equal to their saved stack count until their item-specific unit is authored. `EconomyWalletController` owns the live wallet and idempotency ledger on the generated player vessel and participates in the existing save coordinator.

The migration and wallet test source is authored but still requires Unity Edit Mode execution. No sale/purchase transaction flow is claimed complete by this schema change.


## Schema v8 equipment, research and expedition records

No save schema bump is required for the current equipment/research/telemetry pass. `VesselEquipmentController` owns the existing `InventorySaveData.equipment` list, storing one stable item ID and normalized condition per supported slot (`engine`, `fishing-rod`, `lantern`). Equipped items are absent from the spatial cargo placements until stored again. The controller validates slot-to-item compatibility, rejects duplicate slot/item records, and restores condition with the rest of inventory state.

`HarbourmasterResearchController` stores its one-time submission bitmask in a namespaced `ProgressSaveData.worldFlags` entry and writes earned permit IDs into the existing `ProgressSaveData.unlockedRegionIds` collection. `RegionalChartController` restores those unlocked permits separately from `WorldSaveData.visitedRegionIds`; a permit is not a visit and does not reveal an unknown region.

`ExpeditionEconomyTelemetry` stores completed-trip count, lifetime gross receipts, operating expenses, game-clock duration, fuel consumed and the latest trip metrics in a namespaced progress world flag. An in-progress expedition is not committed as a completed trip until the player finishes a dockside service session. The wallet and fuel controllers expose successful transaction/consumption events for accounting; listener exceptions are isolated from the underlying wallet/cargo commit. Unity save/load round-trip verification remains pending.


## Schema v8 additive seafood legality and balance records

The schema version remains 8. New cargo placement fields are additive and have legacy-safe defaults:

- catchRegionId and catchTechniqueId record where/how a seafood lot was landed;
- catchLegal and catchLegalityReason preserve the result of the authored game legality policy;
- catchSizeKg preserves the weight used for the legal landing check.

Legacy cargo placements without a catch region are treated as legacy inventory rather than retroactively illegal catches. New catch metadata is validated for bounded strings, finite non-negative weight and valid placement identity before restore. This prevents an illegal landing from becoming saleable after save/reload.

Expedition telemetry continues to use a namespaced progress world flag for trip totals. A second namespaced flag stores up to 50 completed balance samples in a compact delimited format (single-character region codes and invariant numeric fields) so the record stays below the save flag's maximum string length. Each sample includes departure/destination region, departure loadout band (starter/average/optimized), gross receipts, operating expenses, actual fuel, game-clock duration, landed/legal/flagged catch counts and landed weight. The active-trip record was extended additively to preserve current catch totals, source region and loadout band through save/reload. Existing schema-v8 telemetry records with 9, 14 or 19 fields remain readable; the current active-trip record uses the 20-field format.

Regional market stock reuses the existing per-vendor/item market stock collection. Destination market IDs use the stable prefix regional_market. plus the region ID. Legacy Mara Venn stock records normalize to regional_market.greyhaven during restore, and duplicate normalized market/item entries are deduplicated. No mutable market stock is stored in ScriptableObject assets.

The additional harvest permits (reef_stewardship, licensed_sea_cucumber, managed_harvest) reuse the existing unlockedRegionIds collection. RegionalChartController filters those permit IDs out of the region-name list, so a harvest permit cannot reveal an unvisited chart region or create a false map marker.

Unity Editor compilation and actual save/load round-trip tests are still required. Static schema validation does not prove JsonUtility serialization or Unity runtime participant ordering.


## Schema v9 historical artifact inspection

Schema v9 adds `WorldSaveData.inspectedHistoricalArtifactIds`, a bounded, unique-ID list of historical records examined through the existing `DeepOceanSiteInteractionController`. It is independent from `inspectedPhenomenonSiteIds`, which continues to mean that a player has inspected a Sounding Rig site.

The v8-to-v9 migration creates an empty artifact ledger for older saves. It does not copy existing site IDs, create artifact discoveries or modify the regional chart. The controller captures/restores the separate list transactionally with its existing save-participant contract. Artifact UI text and visual GameObjects remain transient/scene-authored; only stable artifact IDs are persisted.


## Field journal discovery records

The Greyhaven field journal reuses `ProgressSaveData.journalEntries` and the existing `JournalEntrySaveData` DTO. No save schema migration is required.

- Specimen and salvage IDs use the stable `field-discovery:<CargoItemDefinition.ItemId>` namespace. Sounding Rig findings use `field-finding:<DeepOceanPhenomenonSite.SiteId>`.
- `entryId` stores the stable item or survey-site identity.
- `interpretationId` stores the normalized recovery/survey region ID (or `unknown` when no chart region is active).
- `recordedAtUtc` stores the UTC timestamp at the moment cargo recovery is committed.
- `read` tracks whether the player opened the entry.
- `ProgressSaveData.discoveredEntryIds` remains the compatibility/discovery index. On restore, field journal records and discovery IDs are reconciled so saves created before the journal UI can still show previously discovered item IDs, with missing metadata displayed as unknown / date not recorded.

The fishing and Sounding Rig save participants replace only entries in their own `field-discovery:` and `field-finding:` namespaces during capture. Quest, narrative and other journal records remain untouched. The field journal reads from the generated cargo catalogue and never fabricates a specimen or salvage entry that has not been secured.
