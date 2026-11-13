using System;
using System.Collections.Generic;

namespace WhereTheSoundingsEnd.Persistence
{
    /// <summary>
    /// Versioned, Unity-serializable snapshot of persistent campaign state.
    /// Keep this DTO free of scene objects and UnityEngine component references.
    /// </summary>
    [Serializable]
    public sealed class SaveGameData
    {
        public const int CurrentSchemaVersion = 8;

        public int schemaVersion;
        public int slotNumber;
        public string saveId;
        public string savedAtUtc;
        public string gameVersion;
        public string sceneId;
        public string saveKind = "manual";
        public string saveReason = string.Empty;
        public double totalPlaytimeSeconds;
        public string worldSeed;

        public WorldSaveData world = new WorldSaveData();
        public PlayerSaveData player = new PlayerSaveData();
        public VesselSaveData vessel = new VesselSaveData();
        public InventorySaveData inventory = new InventorySaveData();
        public ProgressSaveData progress = new ProgressSaveData();
        public DialogueSaveData dialogue = new DialogueSaveData();
        public EconomySaveData economy = new EconomySaveData();

        public static SaveGameData CreateNew(int slot, string activeSceneId)
        {
            return new SaveGameData
            {
                schemaVersion = CurrentSchemaVersion,
                slotNumber = slot,
                saveId = Guid.NewGuid().ToString("N"),
                savedAtUtc = DateTime.UtcNow.ToString("O"),
                gameVersion = string.Empty,
                sceneId = activeSceneId ?? string.Empty,
                saveKind = slot == 0 ? "autosave" : "manual",
                saveReason = string.Empty
            };
        }
    }

    [Serializable]
    public sealed class EconomySaveData
    {
        // USD is stored as integer cents to avoid floating-point wallet drift.
        public long walletCents;
        public List<string> processedTransactionIds = new List<string>();
    }

    [Serializable]
    public sealed class WorldSaveData
    {
        public double elapsedGameSeconds;
        public int dayNumber = 1;
        public double timeOfDaySeconds;
        public string activeRegionId = "greyhaven";
        public List<string> visitedRegionIds = new List<string>();
        public string weatherId = "clear";
        public string tideStateId = "unknown";
        public float seaDisturbance;
        public float panicLevel;
        public List<SalvageSiteSaveData> salvageSites = new List<SalvageSiteSaveData>();
        public int soundingPulseCount;
        public List<string> discoveredPhenomenonSiteIds = new List<string>();
        public List<string> evaluatedSurveyOpportunityIds = new List<string>();
        public List<string> inspectedPhenomenonSiteIds = new List<string>();

        // Action-driven weather and horror state; timer fields are game seconds unless named RealSeconds.
        public float weatherFogPressure;
        public double weatherRemainingGameSeconds;
        public double weatherShiftCooldownGameSeconds = 900.0;
        public double hallucinationCooldownGameSeconds = 60.0;
        public bool birdEventPending;
        public double birdEventCountdownGameSeconds;
        public string latestOmen = string.Empty;

        // Event-driven acoustic perception history.
        public float acousticPressure;
        public string lastAcousticEventId = string.Empty;
        public string lastAcousticRegionId = string.Empty;
        public Vector3SaveData lastAcousticPosition = new Vector3SaveData();
        public double lastAcousticGameTimestamp;
        public List<string> heardAcousticEventIds = new List<string>();

        // Temporary maritime-atmosphere state.
        public double atmosphereIncidentCountdownGameSeconds;
        public string atmosphereCaption = string.Empty;
        public double atmosphereCaptionRemainingRealSeconds;
        public double atmosphereHushRemainingRealSeconds;
        public float atmosphereHushBlend = 1f;
        public bool atmosphereFalseLightActive;
        public Vector3SaveData atmosphereFalseLightPosition = new Vector3SaveData();
        public double atmosphereFalseLightRemainingRealSeconds;
    }

    [Serializable]
    public sealed class SalvageSiteSaveData
    {
        public string siteId;
        public int remainingPulls;
        public int lastRefillDay;
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public bool isPresent;
        public Vector3SaveData position = new Vector3SaveData();
        public Vector3SaveData rotationEuler = new Vector3SaveData();
        public float health = 100.0f;
        public int currency;
    }

    [Serializable]
    public sealed class VesselSaveData
    {
        public bool isPresent;
        public string vesselId = "greyhaven.starter_vessel";
        public Vector3SaveData position = new Vector3SaveData();
        public Vector3SaveData rotationEuler = new Vector3SaveData();
        public Vector3SaveData linearVelocity = new Vector3SaveData();
        public Vector3SaveData angularVelocity = new Vector3SaveData();
        public float hullIntegrity = 100.0f;
        public float fuel = 100.0f;
        public float engineCondition = 100.0f;
    }

    [Serializable]
    public sealed class InventorySaveData
    {
        public List<InventoryStackSaveData> cargo = new List<InventoryStackSaveData>();
        public List<EquipmentSaveData> equipment = new List<EquipmentSaveData>();
        public int holdWidth = 7;
        public int holdHeight = 9;
        public List<CargoPlacementSaveData> placements = new List<CargoPlacementSaveData>();
        public List<GridCellSaveData> damagedCells = new List<GridCellSaveData>();
    }

    [Serializable]
    public sealed class GridCellSaveData
    {
        public int x;
        public int y;
    }

    [Serializable]
    public sealed class InventoryStackSaveData
    {
        public string itemId;
        public int quantity;
        public float quality = 1.0f;
        public float freshness = 1.0f;
    }

    [Serializable]
    public sealed class CargoPlacementSaveData
    {
        public string instanceId;
        public string itemId;
        public int anchorX;
        public int anchorY;
        public int rotationQuarterTurns;
        public int quantity = 1;
        public float quality = 1.0f;
        public float freshness = 1.0f;
        public float saleQuantity = 1.0f;
    }

    [Serializable]
    public sealed class EquipmentSaveData
    {
        public string slotId;
        public string itemId;
        public float condition = 1.0f;
    }

    [Serializable]
    public sealed class YarnFloatVariableSaveData
    {
        public string variableName;
        public float value;
    }

    [Serializable]
    public sealed class YarnStringVariableSaveData
    {
        public string variableName;
        public string value;
    }

    [Serializable]
    public sealed class YarnBoolVariableSaveData
    {
        public string variableName;
        public bool value;
    }

    [Serializable]
    public sealed class DialogueSaveData
    {
        public List<YarnFloatVariableSaveData> floatVariables = new List<YarnFloatVariableSaveData>();
        public List<YarnStringVariableSaveData> stringVariables = new List<YarnStringVariableSaveData>();
        public List<YarnBoolVariableSaveData> boolVariables = new List<YarnBoolVariableSaveData>();
        public List<string> completedNodeIds = new List<string>();
    }

    [Serializable]
    public sealed class ProgressSaveData
    {
        public List<string> discoveredEntryIds = new List<string>();
        public List<string> unlockedRegionIds = new List<string>();
        public List<QuestProgressSaveData> quests = new List<QuestProgressSaveData>();
        public List<JournalEntrySaveData> journalEntries = new List<JournalEntrySaveData>();
        public List<NpcStateSaveData> npcStates = new List<NpcStateSaveData>();
        public List<RegionalStockSaveData> regionalStocks = new List<RegionalStockSaveData>();
        public List<WorldFlagSaveData> worldFlags = new List<WorldFlagSaveData>();
        public List<EventCooldownSaveData> eventCooldowns = new List<EventCooldownSaveData>();
        public List<RandomStreamSaveData> randomStreams = new List<RandomStreamSaveData>();
    }

    [Serializable]
    public sealed class QuestProgressSaveData
    {
        public string questId;
        public string stateId;
        public string stageId;
        public List<ObjectiveProgressSaveData> objectives = new List<ObjectiveProgressSaveData>();
    }

    [Serializable]
    public sealed class ObjectiveProgressSaveData
    {
        public string objectiveId;
        public int currentAmount;
        public bool completed;
    }

    [Serializable]
    public sealed class JournalEntrySaveData
    {
        public string entryId;
        public string interpretationId;
        public string recordedAtUtc;
        public bool read;
    }

    [Serializable]
    public sealed class NpcStateSaveData
    {
        public string npcId;
        public string stateId;
        public string relationshipId;
        public string lastKnownRegionId;
        public List<string> relationshipMemoryIds = new List<string>();
    }

    [Serializable]
    public sealed class RegionalStockSaveData
    {
        public string regionId;
        public string speciesId;
        public int population;
        public double lastSimulatedGameSeconds;
    }

    [Serializable]
    public sealed class WorldFlagSaveData
    {
        public string flagId;
        public bool value;
        public string stringValue;
    }

    [Serializable]
    public sealed class EventCooldownSaveData
    {
        public string eventId;
        public double readyAtGameSeconds;
    }

    [Serializable]
    public sealed class RandomStreamSaveData
    {
        public string streamId;
        // Decimal strings preserve the full unsigned 64-bit PCG state without float conversion.
        public string state;
        public string increment;
    }

    [Serializable]
    public sealed class Vector3SaveData
    {
        public float x;
        public float y;
        public float z;

        public Vector3SaveData()
        {
        }

        public Vector3SaveData(float xValue, float yValue, float zValue)
        {
            x = xValue;
            y = yValue;
            z = zValue;
        }
    }

    /// <summary>
    /// Central place for schema migrations and post-deserialization defaults.
    /// Add a new migration step instead of changing the meaning of old fields.
    /// </summary>
    public static class SaveGameDataMigration
    {
        public const int MaximumCollectionEntries = 2048;
        public const int MaximumStringLength = 4096;
        public const int MaximumNpcRelationshipMemories = 128;

        public static bool TryPrepare(SaveGameData data, out string error)
        {
            error = null;
            if (data == null)
            {
                error = "The save data is empty or could not be deserialized.";
                return false;
            }

            if (data.schemaVersion < 0)
            {
                error = "The save file has an invalid schema version.";
                return false;
            }

            if (data.schemaVersion > SaveGameData.CurrentSchemaVersion)
            {
                error = "This save was created by a newer game version and cannot be loaded by this build.";
                return false;
            }

            while (data.schemaVersion < SaveGameData.CurrentSchemaVersion)
            {
                switch (data.schemaVersion)
                {
                    case 0:
                        NormalizeDefaults(data);
                        data.schemaVersion = 1;
                        break;

                    case 1:
                        NormalizeDefaults(data);
                        data.schemaVersion = 2;
                        break;

                    case 2:
                        NormalizeDefaults(data);
                        data.schemaVersion = 3;
                        break;

                    case 3:
                        NormalizeDefaults(data);
                        data.schemaVersion = 4;
                        break;

                    case 4:
                        NormalizeDefaults(data);
                        data.world.weatherFogPressure = data.world.weatherId == "dense_fog" ? 0.85f :
                            data.world.weatherId == "mist" ? 0.4f : 0f;
                        data.world.weatherRemainingGameSeconds = data.world.weatherFogPressure > 0f ? 600.0 : 0.0;
                        data.world.weatherShiftCooldownGameSeconds = 900.0;
                        data.world.hallucinationCooldownGameSeconds = 60.0;
                        data.world.birdEventPending = false;
                        data.world.birdEventCountdownGameSeconds = 0.0;
                        data.world.latestOmen = string.Empty;
                        data.world.atmosphereIncidentCountdownGameSeconds = 420.0;
                        data.world.atmosphereHushBlend = 1f;
                        data.schemaVersion = 5;
                        break;

                    case 5:
                        NormalizeDefaults(data);
                        // Before schema 6 relationship values were not gameplay-driven.
                        // An empty or zero legacy value means the previous unused neutral baseline.
                        for (int npcIndex = 0; npcIndex < data.progress.npcStates.Count; npcIndex++)
                        {
                            NpcStateSaveData npc = data.progress.npcStates[npcIndex];
                            if (npc == null) continue;
                            if (string.IsNullOrWhiteSpace(npc.relationshipId) ||
                                string.Equals(npc.relationshipId, "0", StringComparison.Ordinal))
                                npc.relationshipId = "50";
                            if (npc.relationshipMemoryIds == null)
                                npc.relationshipMemoryIds = new List<string>();
                        }
                        data.schemaVersion = 6;
                        break;

                    case 6:
                        NormalizeDefaults(data);
                        // Regional chart discovery starts empty for existing saves.
                        // Current/legacy region state is not treated as a physical visit.
                        data.world.visitedRegionIds = new List<string>();
                        data.schemaVersion = 7;
                        break;

                    case 7:
                        NormalizeDefaults(data);
                        // Legacy currency was an integer number of whole USD.
                        data.economy.walletCents = Math.Max(0L, (long)data.player.currency) * 100L;
                        data.economy.processedTransactionIds = new List<string>();
                        for (int placementIndex = 0; placementIndex < data.inventory.placements.Count; placementIndex++)
                        {
                            CargoPlacementSaveData placement = data.inventory.placements[placementIndex];
                            if (placement != null)
                                placement.saleQuantity = Math.Max(1, placement.quantity);
                        }
                        data.schemaVersion = 8;
                        break;

                    default:
                        error = "No migration path exists for save schema version " + data.schemaVersion + ".";
                        return false;
                }
            }

            NormalizeDefaults(data);

            if (!IsFinite(data.world.timeOfDaySeconds) ||
                data.world.timeOfDaySeconds < 0.0 || data.world.timeOfDaySeconds >= 86400.0 ||
                data.world.dayNumber < 1 ||
                !IsFinite(data.world.seaDisturbance) ||
                data.world.seaDisturbance < 0f || data.world.seaDisturbance > 1f)
            {
                error = "The save contains invalid world clock or disturbance values.";
                return false;
            }

            if (!IsValidVector(data.vessel.position, 1000000f) ||
                !IsValidVector(data.vessel.rotationEuler, 360000f) ||
                !IsValidVector(data.vessel.linearVelocity, 100f) ||
                !IsValidVector(data.vessel.angularVelocity, 50f) ||
                !IsValidVector(data.player.position, 1000000f) ||
                !IsValidVector(data.player.rotationEuler, 360000f) ||
                !IsValidVector(data.world.lastAcousticPosition, 1000000f) ||
                !IsValidVector(data.world.atmosphereFalseLightPosition, 1000000f))
            {
                error = "The save contains invalid or out-of-range player, vessel, or world physics coordinates.";
                return false;
            }

            if (!IsFinite(data.vessel.hullIntegrity) || data.vessel.hullIntegrity < 0f || data.vessel.hullIntegrity > 100f ||
                !IsFinite(data.vessel.fuel) || data.vessel.fuel < 0f || data.vessel.fuel > 100f ||
                !IsFinite(data.vessel.engineCondition) || data.vessel.engineCondition < 0f || data.vessel.engineCondition > 100f)
            {
                error = "The save contains invalid vessel condition values.";
                return false;
            }

            if (data.economy.walletCents < 0L || data.economy.walletCents > 1000000000000L)
            {
                error = "The save contains an invalid economy wallet balance.";
                return false;
            }

            if (data.inventory.holdWidth > 128 || data.inventory.holdHeight > 128)
            {
                error = "The save contains hold dimensions outside the supported bounds.";
                return false;
            }

            int[] collectionCounts =
            {
                data.world.salvageSites.Count,
                data.world.discoveredPhenomenonSiteIds.Count,
                data.world.evaluatedSurveyOpportunityIds.Count,
                data.world.inspectedPhenomenonSiteIds.Count,
                data.world.visitedRegionIds.Count,
                data.world.heardAcousticEventIds.Count,
                data.inventory.cargo.Count,
                data.inventory.equipment.Count,
                data.inventory.placements.Count,
                data.inventory.damagedCells.Count,
                data.progress.discoveredEntryIds.Count,
                data.progress.unlockedRegionIds.Count,
                data.progress.quests.Count,
                data.progress.journalEntries.Count,
                data.progress.npcStates.Count,
                data.progress.regionalStocks.Count,
                data.progress.worldFlags.Count,
                data.progress.eventCooldowns.Count,
                data.progress.randomStreams.Count,
                data.dialogue.floatVariables.Count,
                data.dialogue.stringVariables.Count,
                data.dialogue.boolVariables.Count,
                data.dialogue.completedNodeIds.Count,
                data.economy.processedTransactionIds.Count
            };
            for (int index = 0; index < collectionCounts.Length; index++)
            {
                if (collectionCounts[index] > MaximumCollectionEntries)
                {
                    error = "The save contains an oversized collection at index " + index + ".";
                    return false;
                }
            }

            string[] metadataStrings =
            {
                data.saveId, data.sceneId, data.savedAtUtc, data.gameVersion,
                data.saveKind, data.saveReason, data.worldSeed,
                data.world.activeRegionId, data.world.weatherId, data.world.tideStateId,
                data.world.latestOmen, data.world.lastAcousticEventId,
                data.world.lastAcousticRegionId, data.world.atmosphereCaption
            };
            for (int index = 0; index < metadataStrings.Length; index++)
            {
                if (metadataStrings[index] != null && metadataStrings[index].Length > MaximumStringLength)
                {
                    error = "The save contains an oversized metadata string at index " + index + ".";
                    return false;
                }
            }

            if (!IsFinite(data.player.health) || data.player.health < 0f || data.player.health > 100f ||
                data.player.currency < 0)
            {
                error = "The save contains invalid player condition or currency values.";
                return false;
            }

            if (double.IsNaN(data.totalPlaytimeSeconds) ||
                double.IsInfinity(data.totalPlaytimeSeconds) ||
                data.totalPlaytimeSeconds < 0.0)
            {
                error = "The save contains an invalid playtime value.";
                return false;
            }

            if (double.IsNaN(data.world.elapsedGameSeconds) ||
                double.IsInfinity(data.world.elapsedGameSeconds) ||
                data.world.elapsedGameSeconds < 0.0)
            {
                error = "The save contains an invalid world-time value.";
                return false;
            }

            for (int index = 0; index < data.inventory.cargo.Count; index++)
            {
                InventoryStackSaveData stack = data.inventory.cargo[index];
                if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity < 1 ||
                    !IsFinite(stack.quality) || stack.quality < 0f || stack.quality > 1f ||
                    !IsFinite(stack.freshness) || stack.freshness < 0f || stack.freshness > 1f)
                {
                    error = "The save contains an invalid cargo entry at index " + index + ".";
                    return false;
                }
            }

            for (int index = 0; index < data.inventory.equipment.Count; index++)
            {
                EquipmentSaveData item = data.inventory.equipment[index];
                if (item == null || string.IsNullOrWhiteSpace(item.slotId) ||
                    string.IsNullOrWhiteSpace(item.itemId) || !IsFinite(item.condition) ||
                    item.condition < 0f || item.condition > 1f)
                {
                    error = "The save contains an invalid equipment entry at index " + index + ".";
                    return false;
                }
            }

            if (float.IsNaN(data.world.panicLevel) || float.IsInfinity(data.world.panicLevel) ||
                data.world.panicLevel < 0f || data.world.panicLevel > 1f)
            {
                error = "The save contains an invalid panic value.";
                return false;
            }

            if (float.IsNaN(data.world.weatherFogPressure) || float.IsInfinity(data.world.weatherFogPressure) ||
                data.world.weatherFogPressure < 0f || data.world.weatherFogPressure > 1f ||
                float.IsNaN(data.world.acousticPressure) || float.IsInfinity(data.world.acousticPressure) ||
                data.world.acousticPressure < 0f || data.world.acousticPressure > 1f ||
                float.IsNaN(data.world.atmosphereHushBlend) || float.IsInfinity(data.world.atmosphereHushBlend) ||
                data.world.atmosphereHushBlend < 0f || data.world.atmosphereHushBlend > 1f)
            {
                error = "The save contains an invalid atmospheric or acoustic pressure value.";
                return false;
            }

            double[] eventTimers =
            {
                data.world.weatherRemainingGameSeconds,
                data.world.weatherShiftCooldownGameSeconds,
                data.world.hallucinationCooldownGameSeconds,
                data.world.birdEventCountdownGameSeconds,
                data.world.atmosphereIncidentCountdownGameSeconds,
                data.world.atmosphereCaptionRemainingRealSeconds,
                data.world.atmosphereHushRemainingRealSeconds,
                data.world.atmosphereFalseLightRemainingRealSeconds,
                data.world.lastAcousticGameTimestamp
            };
            for (int index = 0; index < eventTimers.Length; index++)
            {
                if (double.IsNaN(eventTimers[index]) || double.IsInfinity(eventTimers[index]) ||
                    eventTimers[index] < 0.0)
                {
                    error = "The save contains an invalid event timer at index " + index + ".";
                    return false;
                }
            }

            for (int index = 0; index < data.inventory.damagedCells.Count; index++)
            {
                GridCellSaveData cell = data.inventory.damagedCells[index];
                if (cell == null || cell.x < 0 || cell.y < 0 ||
                    cell.x >= data.inventory.holdWidth || cell.y >= data.inventory.holdHeight)
                {
                    error = "The save contains an invalid damaged hold cell at index " + index + ".";
                    return false;
                }
            }

            HashSet<string> cargoInstanceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < data.inventory.placements.Count; index++)
            {
                CargoPlacementSaveData placement = data.inventory.placements[index];
                if (placement == null ||
                    string.IsNullOrWhiteSpace(placement.itemId) ||
                    placement.quantity < 1 ||
                    placement.anchorX < 0 ||
                    placement.anchorY < 0 ||
                    placement.rotationQuarterTurns < 0 ||
                    placement.rotationQuarterTurns > 3 ||
                    !IsFinite(placement.quality) || placement.quality < 0f || placement.quality > 1f ||
                    !IsFinite(placement.freshness) || placement.freshness < 0f || placement.freshness > 1f ||
                    !IsValidCargoInstanceId(placement.instanceId) ||
                    !cargoInstanceIds.Add(placement.instanceId))
                {
                    error = "The save contains a malformed or duplicate cargo instance identifier or invalid placement at index " + index + ".";
                    return false;
                }
            }

            for (int index = 0; index < data.dialogue.floatVariables.Count; index++)
            {
                YarnFloatVariableSaveData variable = data.dialogue.floatVariables[index];
                if (variable == null || string.IsNullOrWhiteSpace(variable.variableName) ||
                    !IsFinite(variable.value) || variable.variableName.Length > MaximumStringLength)
                {
                    error = "The save contains an invalid Yarn float variable at index " + index + ".";
                    return false;
                }
            }

            for (int index = 0; index < data.dialogue.stringVariables.Count; index++)
            {
                YarnStringVariableSaveData variable = data.dialogue.stringVariables[index];
                if (variable == null || string.IsNullOrWhiteSpace(variable.variableName) ||
                    variable.variableName.Length > MaximumStringLength ||
                    (variable.value != null && variable.value.Length > MaximumStringLength))
                {
                    error = "The save contains an invalid or oversized Yarn string variable at index " + index + ".";
                    return false;
                }
            }

            HashSet<string> salvageSiteIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < data.world.salvageSites.Count; index++)
            {
                SalvageSiteSaveData site = data.world.salvageSites[index];
                if (site == null || string.IsNullOrWhiteSpace(site.siteId) ||
                    site.siteId.Length > 256 || site.remainingPulls < 0 ||
                    site.lastRefillDay < 1 || !salvageSiteIds.Add(site.siteId))
                {
                    error = "The save contains a malformed or duplicate salvage-site record at index " + index + ".";
                    return false;
                }
            }

            if (!ValidateUniqueIds(data.world.discoveredPhenomenonSiteIds, "discovered site", out error) ||
                !ValidateUniqueIds(data.world.evaluatedSurveyOpportunityIds, "survey opportunity", out error) ||
                !ValidateUniqueIds(data.world.inspectedPhenomenonSiteIds, "inspected site", out error) ||
                !ValidateUniqueIds(data.world.heardAcousticEventIds, "acoustic event", out error) ||
                !ValidateUniqueIds(data.dialogue.completedNodeIds, "completed dialogue node", out error))
                return false;

            HashSet<string> questIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < data.progress.quests.Count; index++)
            {
                QuestProgressSaveData quest = data.progress.quests[index];
                if (quest == null || string.IsNullOrWhiteSpace(quest.questId) ||
                    !questIds.Add(quest.questId) ||
                    quest.objectives == null || quest.objectives.Count > MaximumCollectionEntries)
                {
                    error = "The save contains a malformed quest collection at index " + index + ".";
                    return false;
                }

                HashSet<string> objectiveIds = new HashSet<string>(StringComparer.Ordinal);
                for (int objectiveIndex = 0; objectiveIndex < quest.objectives.Count; objectiveIndex++)
                {
                    ObjectiveProgressSaveData objective = quest.objectives[objectiveIndex];
                    if (objective == null || string.IsNullOrWhiteSpace(objective.objectiveId) ||
                        objective.currentAmount < 0 || !objectiveIds.Add(objective.objectiveId))
                    {
                        error = "The save contains a malformed or duplicate quest objective at index " + objectiveIndex + ".";
                        return false;
                    }
                }
            }

            if (!ValidateUniqueIds(data.progress.discoveredEntryIds, "journal discovery", out error) ||
                !ValidateUniqueIds(data.progress.unlockedRegionIds, "unlocked region", out error))
                return false;

            HashSet<string> journalIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < data.progress.journalEntries.Count; index++)
            {
                JournalEntrySaveData entry = data.progress.journalEntries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.entryId) ||
                    string.IsNullOrWhiteSpace(entry.interpretationId) ||
                    !journalIds.Add(entry.entryId))
                {
                    error = "The save contains a malformed or duplicate journal entry at index " + index + ".";
                    return false;
                }
            }

            HashSet<string> randomStreamIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < data.progress.randomStreams.Count; index++)
            {
                RandomStreamSaveData stream = data.progress.randomStreams[index];
                if (stream == null || string.IsNullOrWhiteSpace(stream.streamId) ||
                    string.IsNullOrWhiteSpace(stream.state) || string.IsNullOrWhiteSpace(stream.increment) ||
                    stream.streamId.Length > 256 || stream.state.Length > 32 || stream.increment.Length > 32 ||
                    !randomStreamIds.Add(stream.streamId))
                {
                    error = "The save contains a malformed or duplicate random stream at index " + index + ".";
                    return false;
                }
            }

            HashSet<string> npcIds = new HashSet<string>(StringComparer.Ordinal);
            int totalNpcRelationshipMemories = 0;
            for (int index = 0; index < data.progress.npcStates.Count; index++)
            {
                NpcStateSaveData npc = data.progress.npcStates[index];
                if (npc == null || string.IsNullOrWhiteSpace(npc.npcId) || !npcIds.Add(npc.npcId))
                {
                    error = "The save contains a malformed or duplicate NPC state at index " + index + ".";
                    return false;
                }

                if (npc.relationshipMemoryIds == null ||
                    npc.relationshipMemoryIds.Count > MaximumNpcRelationshipMemories)
                {
                    error = "The save contains an oversized or malformed NPC relationship memory collection at index " + index + ".";
                    return false;
                }

                HashSet<string> npcMemoryIds = new HashSet<string>(StringComparer.Ordinal);
                for (int memoryIndex = 0; memoryIndex < npc.relationshipMemoryIds.Count; memoryIndex++)
                {
                    string memoryId = npc.relationshipMemoryIds[memoryIndex];
                    totalNpcRelationshipMemories++;
                    if (string.IsNullOrWhiteSpace(memoryId) || memoryId.Length > 256 ||
                        !string.Equals(memoryId, memoryId.Trim(), StringComparison.Ordinal) ||
                        !npcMemoryIds.Add(memoryId) || totalNpcRelationshipMemories > 4096)
                    {
                        error = "The save contains a malformed, duplicate, or oversized NPC relationship memory at index " + memoryIndex + ".";
                        return false;
                    }
                }
            }

            HashSet<string> regionalStockIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < data.progress.regionalStocks.Count; index++)
            {
                RegionalStockSaveData stock = data.progress.regionalStocks[index];
                string key = stock == null ? string.Empty : stock.regionId + ":" + stock.speciesId;
                if (stock == null || string.IsNullOrWhiteSpace(stock.regionId) ||
                    string.IsNullOrWhiteSpace(stock.speciesId) || stock.population < 0 ||
                    !IsFinite(stock.lastSimulatedGameSeconds) || stock.lastSimulatedGameSeconds < 0.0 ||
                    !regionalStockIds.Add(key))
                {
                    error = "The save contains a malformed or duplicate regional stock record at index " + index + ".";
                    return false;
                }
            }

            if (!ValidateUniqueIds(data.progress.worldFlags.ConvertAll(item => item == null ? null : item.flagId),
                    "world flag", out error) ||
                !ValidateUniqueIds(data.progress.eventCooldowns.ConvertAll(item => item == null ? null : item.eventId),
                    "event cooldown", out error))
                return false;

            for (int index = 0; index < data.progress.worldFlags.Count; index++)
            {
                WorldFlagSaveData flag = data.progress.worldFlags[index];
                if (flag == null || (flag.stringValue != null && flag.stringValue.Length > MaximumStringLength))
                {
                    error = "The save contains an invalid world flag at index " + index + ".";
                    return false;
                }
            }

            for (int index = 0; index < data.progress.eventCooldowns.Count; index++)
            {
                EventCooldownSaveData cooldown = data.progress.eventCooldowns[index];
                if (cooldown == null || !IsFinite(cooldown.readyAtGameSeconds) || cooldown.readyAtGameSeconds < 0.0)
                {
                    error = "The save contains an invalid event cooldown at index " + index + ".";
                    return false;
                }
            }

            if (!ValidateUniqueIds(data.dialogue.floatVariables.ConvertAll(item => item == null ? null : item.variableName),
                    "Yarn float variable", out error) ||
                !ValidateUniqueIds(data.dialogue.stringVariables.ConvertAll(item => item == null ? null : item.variableName),
                    "Yarn string variable", out error) ||
                !ValidateUniqueIds(data.dialogue.boolVariables.ConvertAll(item => item == null ? null : item.variableName),
                    "Yarn bool variable", out error))
                return false;

            for (int index = 0; index < data.dialogue.boolVariables.Count; index++)
            {
                YarnBoolVariableSaveData variable = data.dialogue.boolVariables[index];
                if (variable == null || string.IsNullOrWhiteSpace(variable.variableName) ||
                    variable.variableName.Length > MaximumStringLength)
                {
                    error = "The save contains an invalid Yarn bool variable at index " + index + ".";
                    return false;
                }
            }

            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool IsValidVector(Vector3SaveData value, float maximumAbsoluteComponent)
        {
            return value != null &&
                   IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z) &&
                   Math.Abs(value.x) <= maximumAbsoluteComponent &&
                   Math.Abs(value.y) <= maximumAbsoluteComponent &&
                   Math.Abs(value.z) <= maximumAbsoluteComponent;
        }

        private static bool IsValidCargoInstanceId(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId) || !string.Equals(instanceId, instanceId.Trim(), StringComparison.Ordinal))
                return false;

            Guid parsed;
            bool valid = Guid.TryParseExact(instanceId, "N", out parsed) ||
                         Guid.TryParseExact(instanceId, "D", out parsed);
            return valid && parsed != Guid.Empty;
        }

        private static bool ValidateUniqueIds(List<string> values, string label, out string error)
        {
            error = null;
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                string value = values[index];
                if (string.IsNullOrWhiteSpace(value) || value.Length > 256 || !ids.Add(value))
                {
                    error = "The save contains an invalid or duplicate " + label + " identifier at index " + index + ".";
                    return false;
                }
            }
            return true;
        }

        private static void NormalizeDefaults(SaveGameData data)
        {
            if (string.IsNullOrWhiteSpace(data.saveId))
            {
                data.saveId = Guid.NewGuid().ToString("N");
            }

            if (data.sceneId == null) data.sceneId = string.Empty;
            if (data.savedAtUtc == null) data.savedAtUtc = string.Empty;
            if (data.gameVersion == null) data.gameVersion = string.Empty;
            if (data.worldSeed == null) data.worldSeed = string.Empty;
            if (data.saveKind == null) data.saveKind = data.slotNumber == 0 ? "autosave" : "manual";
            if (data.saveReason == null) data.saveReason = string.Empty;

            if (data.world == null) data.world = new WorldSaveData();
            if (data.world.salvageSites == null) data.world.salvageSites = new List<SalvageSiteSaveData>();
            if (data.world.discoveredPhenomenonSiteIds == null) data.world.discoveredPhenomenonSiteIds = new List<string>();
            if (data.world.evaluatedSurveyOpportunityIds == null) data.world.evaluatedSurveyOpportunityIds = new List<string>();
            if (data.world.inspectedPhenomenonSiteIds == null) data.world.inspectedPhenomenonSiteIds = new List<string>();
            if (data.world.visitedRegionIds == null) data.world.visitedRegionIds = new List<string>();
            if (data.world.heardAcousticEventIds == null) data.world.heardAcousticEventIds = new List<string>();
            if (data.world.latestOmen == null) data.world.latestOmen = string.Empty;
            if (data.world.lastAcousticEventId == null) data.world.lastAcousticEventId = string.Empty;
            if (data.world.lastAcousticRegionId == null) data.world.lastAcousticRegionId = string.Empty;
            if (data.world.atmosphereCaption == null) data.world.atmosphereCaption = string.Empty;
            if (data.world.lastAcousticPosition == null) data.world.lastAcousticPosition = new Vector3SaveData();
            if (data.world.atmosphereFalseLightPosition == null) data.world.atmosphereFalseLightPosition = new Vector3SaveData();
            if (data.world.weatherFogPressure == 0f && data.world.weatherId != "clear")
                data.world.weatherFogPressure = data.world.weatherId == "dense_fog" ? 0.85f : 0.4f;
            data.world.weatherFogPressure = Math.Max(0f, Math.Min(1f, data.world.weatherFogPressure));
            data.world.acousticPressure = Math.Max(0f, Math.Min(1f, data.world.acousticPressure));
            data.world.atmosphereHushBlend = Math.Max(0f, Math.Min(1f, data.world.atmosphereHushBlend));
            if (data.world.soundingPulseCount < 0) data.world.soundingPulseCount = 0;
            data.world.panicLevel = Math.Max(0f, Math.Min(1f, data.world.panicLevel));
            if (data.player == null) data.player = new PlayerSaveData();
            if (data.vessel == null) data.vessel = new VesselSaveData();
            if (data.inventory == null) data.inventory = new InventorySaveData();
            if (data.progress == null) data.progress = new ProgressSaveData();
            if (data.dialogue == null) data.dialogue = new DialogueSaveData();
            if (data.economy == null) data.economy = new EconomySaveData();
            if (data.economy.processedTransactionIds == null) data.economy.processedTransactionIds = new List<string>();

            if (data.player.position == null) data.player.position = new Vector3SaveData();
            if (data.player.rotationEuler == null) data.player.rotationEuler = new Vector3SaveData();

            if (data.vessel.position == null) data.vessel.position = new Vector3SaveData();
            if (data.vessel.rotationEuler == null) data.vessel.rotationEuler = new Vector3SaveData();
            if (data.vessel.linearVelocity == null) data.vessel.linearVelocity = new Vector3SaveData();
            if (data.vessel.angularVelocity == null) data.vessel.angularVelocity = new Vector3SaveData();

            if (data.inventory.cargo == null) data.inventory.cargo = new List<InventoryStackSaveData>();
            if (data.inventory.equipment == null) data.inventory.equipment = new List<EquipmentSaveData>();
            if (data.inventory.placements == null) data.inventory.placements = new List<CargoPlacementSaveData>();
            if (data.inventory.damagedCells == null) data.inventory.damagedCells = new List<GridCellSaveData>();
            for (int placementIndex = 0; placementIndex < data.inventory.placements.Count; placementIndex++)
            {
                CargoPlacementSaveData placement = data.inventory.placements[placementIndex];
                if (placement != null && (placement.saleQuantity <= 0f || float.IsNaN(placement.saleQuantity) || float.IsInfinity(placement.saleQuantity)))
                    placement.saleQuantity = Math.Max(1, placement.quantity);
            }
            if (data.inventory.holdWidth <= 0) data.inventory.holdWidth = 7;
            if (data.inventory.holdHeight <= 0) data.inventory.holdHeight = 9;

            if (data.progress.discoveredEntryIds == null) data.progress.discoveredEntryIds = new List<string>();
            if (data.progress.unlockedRegionIds == null) data.progress.unlockedRegionIds = new List<string>();
            if (data.progress.quests == null) data.progress.quests = new List<QuestProgressSaveData>();
            if (data.progress.journalEntries == null) data.progress.journalEntries = new List<JournalEntrySaveData>();
            if (data.progress.npcStates == null) data.progress.npcStates = new List<NpcStateSaveData>();
            for (int npcIndex = 0; npcIndex < data.progress.npcStates.Count; npcIndex++)
            {
                NpcStateSaveData npc = data.progress.npcStates[npcIndex];
                if (npc != null && npc.relationshipMemoryIds == null)
                    npc.relationshipMemoryIds = new List<string>();
            }
            if (data.progress.regionalStocks == null) data.progress.regionalStocks = new List<RegionalStockSaveData>();
            if (data.progress.worldFlags == null) data.progress.worldFlags = new List<WorldFlagSaveData>();
            if (data.progress.eventCooldowns == null) data.progress.eventCooldowns = new List<EventCooldownSaveData>();
            if (data.progress.randomStreams == null) data.progress.randomStreams = new List<RandomStreamSaveData>();
            if (data.dialogue.floatVariables == null) data.dialogue.floatVariables = new List<YarnFloatVariableSaveData>();
            if (data.dialogue.stringVariables == null) data.dialogue.stringVariables = new List<YarnStringVariableSaveData>();
            if (data.dialogue.boolVariables == null) data.dialogue.boolVariables = new List<YarnBoolVariableSaveData>();
            if (data.dialogue.completedNodeIds == null) data.dialogue.completedNodeIds = new List<string>();

            for (int index = 0; index < data.progress.quests.Count; index++)
            {
                QuestProgressSaveData quest = data.progress.quests[index];
                if (quest != null && quest.objectives == null)
                {
                    quest.objectives = new List<ObjectiveProgressSaveData>();
                }
            }
        }
    }
}