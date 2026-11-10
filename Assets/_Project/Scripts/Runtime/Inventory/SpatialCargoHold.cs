using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Inventory
{
    [Serializable]
    public sealed class CargoPlacement
    {
        public string instanceId;
        public string itemId;
        public int anchorX;
        public int anchorY;
        [Range(0, 3)] public int rotationQuarterTurns;
        [Min(1)] public int quantity = 1;
        [Range(0f, 1f)] public float quality = 1f;
        [Range(0f, 1f)] public float freshness = 1f;
    }

    /// <summary>
    /// Data-first spatial hold. UI can query the same placement rules without owning inventory state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpatialCargoHold : MonoBehaviour, ISaveParticipant
    {
        [Header("Hold dimensions")]
        [SerializeField, Min(1)] private int width = 7;
        [SerializeField, Min(1)] private int height = 9;
        [SerializeField] private Vector2Int[] blockedCells = Array.Empty<Vector2Int>();

        [Header("Item catalogue")]
        [SerializeField] private List<CargoItemDefinition> itemDefinitions = new List<CargoItemDefinition>();

        [Header("Runtime cargo")]
        [SerializeField] private List<CargoPlacement> placements = new List<CargoPlacement>();

        private readonly Dictionary<string, CargoItemDefinition> _definitions =
            new Dictionary<string, CargoItemDefinition>(StringComparer.Ordinal);

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public IReadOnlyList<CargoPlacement> Placements { get { return placements; } }

        private void Awake()
        {
            RebuildDefinitionLookup();
        }

        private void OnValidate()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            RebuildDefinitionLookup();
        }

        public bool TryPlace(
            string itemId,
            int x,
            int y,
            int rotationQuarterTurns,
            int quantity,
            float quality,
            float freshness,
            out string instanceId)
        {
            instanceId = null;
            CargoItemDefinition definition = FindDefinition(itemId);
            if (definition == null || quantity < 1) return false;

            bool[,] occupied = BuildOccupiedGrid(null);
            bool[,] blocked = CargoGridGeometry.BuildBlockedGrid(width, height, blockedCells);
            Vector2Int[] shape = CargoGridGeometry.RotateAndNormalize(
                definition.OccupiedCells, rotationQuarterTurns);

            if (!CargoGridGeometry.CanPlace(
                width, height, occupied, blocked, shape, new Vector2Int(x, y)))
                return false;

            instanceId = Guid.NewGuid().ToString("N");
            placements.Add(new CargoPlacement
            {
                instanceId = instanceId,
                itemId = definition.ItemId,
                anchorX = x,
                anchorY = y,
                rotationQuarterTurns = ((rotationQuarterTurns % 4) + 4) % 4,
                quantity = quantity,
                quality = Mathf.Clamp01(quality),
                freshness = Mathf.Clamp01(freshness)
            });
            return true;
        }

        public bool TryMove(string instanceId, int x, int y, int rotationQuarterTurns)
        {
            CargoPlacement placement = FindPlacement(instanceId);
            if (placement == null) return false;

            CargoItemDefinition definition = FindDefinition(placement.itemId);
            if (definition == null) return false;

            bool[,] occupied = BuildOccupiedGrid(instanceId);
            bool[,] blocked = CargoGridGeometry.BuildBlockedGrid(width, height, blockedCells);
            Vector2Int[] shape = CargoGridGeometry.RotateAndNormalize(
                definition.OccupiedCells, rotationQuarterTurns);

            if (!CargoGridGeometry.CanPlace(
                width, height, occupied, blocked, shape, new Vector2Int(x, y)))
                return false;

            placement.anchorX = x;
            placement.anchorY = y;
            placement.rotationQuarterTurns = ((rotationQuarterTurns % 4) + 4) % 4;
            return true;
        }

        public bool TryRemove(string instanceId, out CargoPlacement removed)
        {
            int index = placements.FindIndex(item =>
                item != null && string.Equals(item.instanceId, instanceId, StringComparison.Ordinal));

            if (index < 0)
            {
                removed = null;
                return false;
            }

            removed = placements[index];
            placements.RemoveAt(index);
            return true;
        }

        public bool CanFit(string itemId, int x, int y, int rotationQuarterTurns)
        {
            CargoItemDefinition definition = FindDefinition(itemId);
            if (definition == null) return false;

            return CargoGridGeometry.CanPlace(
                width,
                height,
                BuildOccupiedGrid(null),
                CargoGridGeometry.BuildBlockedGrid(width, height, blockedCells),
                CargoGridGeometry.RotateAndNormalize(definition.OccupiedCells, rotationQuarterTurns),
                new Vector2Int(x, y));
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.inventory == null)
                throw new ArgumentNullException(nameof(data), "Save data must contain inventory data.");

            data.inventory.holdWidth = width;
            data.inventory.holdHeight = height;
            data.inventory.placements.Clear();

            for (int i = 0; i < placements.Count; i++)
            {
                CargoPlacement item = placements[i];
                if (item == null) continue;

                data.inventory.placements.Add(new CargoPlacementSaveData
                {
                    instanceId = item.instanceId,
                    itemId = item.itemId,
                    anchorX = item.anchorX,
                    anchorY = item.anchorY,
                    rotationQuarterTurns = item.rotationQuarterTurns,
                    quantity = item.quantity,
                    quality = item.quality,
                    freshness = item.freshness
                });
            }
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.inventory == null) return;

            // Hold upgrades are authored progression. Never resize the live hold from untrusted save data.
            placements.Clear();
            if (data.inventory.placements == null) return;

            for (int i = 0; i < data.inventory.placements.Count; i++)
            {
                CargoPlacementSaveData saved = data.inventory.placements[i];
                if (saved == null || string.IsNullOrWhiteSpace(saved.itemId) || saved.quantity < 1)
                    continue;

                CargoItemDefinition definition = FindDefinition(saved.itemId);
                if (definition == null)
                {
                    Debug.LogWarning("Skipped unknown saved cargo item '" + saved.itemId + "'.", this);
                    continue;
                }

                bool[,] occupied = BuildOccupiedGrid(null);
                bool[,] blocked = CargoGridGeometry.BuildBlockedGrid(width, height, blockedCells);
                Vector2Int[] shape = CargoGridGeometry.RotateAndNormalize(
                    definition.OccupiedCells, saved.rotationQuarterTurns);

                if (!CargoGridGeometry.CanPlace(
                    width, height, occupied, blocked, shape,
                    new Vector2Int(saved.anchorX, saved.anchorY)))
                {
                    Debug.LogWarning("Skipped cargo placement that no longer fits: " + saved.itemId, this);
                    continue;
                }

                placements.Add(new CargoPlacement
                {
                    instanceId = string.IsNullOrWhiteSpace(saved.instanceId)
                        ? Guid.NewGuid().ToString("N")
                        : saved.instanceId,
                    itemId = saved.itemId,
                    anchorX = saved.anchorX,
                    anchorY = saved.anchorY,
                    rotationQuarterTurns = ((saved.rotationQuarterTurns % 4) + 4) % 4,
                    quantity = saved.quantity,
                    quality = Mathf.Clamp01(saved.quality),
                    freshness = Mathf.Clamp01(saved.freshness)
                });
            }
        }

        private bool[,] BuildOccupiedGrid(string ignoredInstanceId)
        {
            bool[,] occupied = new bool[width, height];

            for (int i = 0; i < placements.Count; i++)
            {
                CargoPlacement placement = placements[i];
                if (placement == null ||
                    string.Equals(placement.instanceId, ignoredInstanceId, StringComparison.Ordinal))
                    continue;

                CargoItemDefinition definition = FindDefinition(placement.itemId);
                if (definition == null) continue;

                Vector2Int[] shape = CargoGridGeometry.RotateAndNormalize(
                    definition.OccupiedCells, placement.rotationQuarterTurns);

                for (int cellIndex = 0; cellIndex < shape.Length; cellIndex++)
                {
                    Vector2Int cell = new Vector2Int(placement.anchorX, placement.anchorY) + shape[cellIndex];
                    if (cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height)
                        occupied[cell.x, cell.y] = true;
                }
            }

            return occupied;
        }

        private CargoPlacement FindPlacement(string instanceId)
        {
            return placements.Find(item =>
                item != null && string.Equals(item.instanceId, instanceId, StringComparison.Ordinal));
        }

        private CargoItemDefinition FindDefinition(string itemId)
        {
            if (_definitions.Count != itemDefinitions.Count)
                RebuildDefinitionLookup();

            CargoItemDefinition definition;
            return _definitions.TryGetValue(itemId ?? string.Empty, out definition)
                ? definition
                : null;
        }

        private void RebuildDefinitionLookup()
        {
            _definitions.Clear();
            if (itemDefinitions == null) return;

            for (int i = 0; i < itemDefinitions.Count; i++)
            {
                CargoItemDefinition definition = itemDefinitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.ItemId)) continue;

                if (_definitions.ContainsKey(definition.ItemId))
                {
                    Debug.LogError("Duplicate cargo item ID: " + definition.ItemId, this);
                    continue;
                }

                _definitions.Add(definition.ItemId, definition);
            }
        }
    }
}
