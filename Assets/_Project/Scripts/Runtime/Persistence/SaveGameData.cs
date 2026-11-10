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
        public const int CurrentSchemaVersion = 4;

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
    public sealed class WorldSaveData
    {
        public double elapsedGameSeconds;
        public int dayNumber = 1;
        public double timeOfDaySeconds;
        public string activeRegionId = "greyhaven";
        public string weatherId = "clear";
        public string tideStateId = "unknown";
        public float seaDisturbance;
        public float panicLevel;
        public List<SalvageSiteSaveData> salvageSites = new List<SalvageSiteSaveData>();
        public int soundingPulseCount;
        public List<string> discoveredPhenomenonSiteIds = new List<string>();
        public List<string> evaluatedSurveyOpportunityIds = new List<string>();
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

                    default:
                        error = "No migration path exists for save schema version " + data.schemaVersion + ".";
                        return false;
                }
            }

            NormalizeDefaults(data);

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
                if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity < 0)
                {
                    error = "The save contains an invalid cargo entry at index " + index + ".";
                    return false;
                }
            }

            if (float.IsNaN(data.world.panicLevel) || float.IsInfinity(data.world.panicLevel) ||
                data.world.panicLevel < 0f || data.world.panicLevel > 1f)
            {
                error = "The save contains an invalid panic value.";
                return false;
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
                    float.IsNaN(placement.quality) ||
                    float.IsInfinity(placement.quality) ||
                    float.IsNaN(placement.freshness) ||
                    float.IsInfinity(placement.freshness))
                {
                    error = "The save contains an invalid spatial cargo placement at index " + index + ".";
                    return false;
                }
            }

            for (int index = 0; index < data.dialogue.floatVariables.Count; index++)
            {
                YarnFloatVariableSaveData variable = data.dialogue.floatVariables[index];
                if (variable == null || string.IsNullOrWhiteSpace(variable.variableName) ||
                    float.IsNaN(variable.value) || float.IsInfinity(variable.value))
                {
                    error = "The save contains an invalid Yarn float variable at index " + index + ".";
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
            if (data.world.soundingPulseCount < 0) data.world.soundingPulseCount = 0;
            data.world.panicLevel = Math.Max(0f, Math.Min(1f, data.world.panicLevel));
            if (data.player == null) data.player = new PlayerSaveData();
            if (data.vessel == null) data.vessel = new VesselSaveData();
            if (data.inventory == null) data.inventory = new InventorySaveData();
            if (data.progress == null) data.progress = new ProgressSaveData();
            if (data.dialogue == null) data.dialogue = new DialogueSaveData();

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
            if (data.inventory.holdWidth <= 0) data.inventory.holdWidth = 7;
            if (data.inventory.holdHeight <= 0) data.inventory.holdHeight = 9;

            if (data.progress.discoveredEntryIds == null) data.progress.discoveredEntryIds = new List<string>();
            if (data.progress.unlockedRegionIds == null) data.progress.unlockedRegionIds = new List<string>();
            if (data.progress.quests == null) data.progress.quests = new List<QuestProgressSaveData>();
            if (data.progress.journalEntries == null) data.progress.journalEntries = new List<JournalEntrySaveData>();
            if (data.progress.npcStates == null) data.progress.npcStates = new List<NpcStateSaveData>();
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