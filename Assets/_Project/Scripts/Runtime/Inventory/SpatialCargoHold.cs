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
        [Min(0.001f)] public float saleQuantity = 1f;
    }

    /// <summary>
    /// Data-first spatial hold. UI can query the same placement rules without owning inventory state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpatialCargoHold : MonoBehaviour, ISaveParticipant, ISaveDataValidator
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
        public IReadOnlyList<CargoItemDefinition> ItemDefinitions { get { return itemDefinitions; } }

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

        public void SetItemDefinitions(IEnumerable<CargoItemDefinition> definitions)
        {
            if (itemDefinitions == null)
                itemDefinitions = new List<CargoItemDefinition>();
            itemDefinitions.Clear();
            if (definitions != null)
            {
                foreach (CargoItemDefinition definition in definitions)
                    if (definition != null) itemDefinitions.Add(definition);
            }
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
            out string instanceId,
            float saleQuantity = -1f)
        {
            instanceId = null;
            CargoItemDefinition definition = FindDefinition(itemId);
            if (saleQuantity == -1f) saleQuantity = quantity;
            if (definition == null || quantity < 1 || !IsFinite(saleQuantity) || saleQuantity <= 0f) return false;

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
                freshness = Mathf.Clamp01(freshness),
                saleQuantity = IsFinite(saleQuantity) ? Mathf.Max(0.001f, saleQuantity) : 1f
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

        /// <summary>
        /// Commits a multi-item removal only when every requested instance is present and
        /// the supplied wallet/ledger commit succeeds. Validation and commit run synchronously
        /// on Unity's main thread; no cargo is removed on a failed commit.
        /// </summary>
        public bool TryCommitRemoval(IList<string> instanceIds, Func<bool> commit, out List<CargoPlacement> removed)
        {
            removed = null;
            if (instanceIds == null || instanceIds.Count == 0 || commit == null)
                return false;

            HashSet<string> uniqueIds = new HashSet<string>(StringComparer.Ordinal);
            List<int> indices = new List<int>(instanceIds.Count);
            List<CargoPlacement> candidates = new List<CargoPlacement>(instanceIds.Count);
            for (int requestedIndex = 0; requestedIndex < instanceIds.Count; requestedIndex++)
            {
                string id = instanceIds[requestedIndex];
                if (string.IsNullOrWhiteSpace(id) || !uniqueIds.Add(id))
                    return false;

                int foundIndex = placements.FindIndex(item =>
                    item != null && string.Equals(item.instanceId, id, StringComparison.Ordinal));
                if (foundIndex < 0)
                    return false;

                indices.Add(foundIndex);
                candidates.Add(placements[foundIndex]);
            }

            bool committed;
            try { committed = commit(); }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                return false;
            }

            if (!committed)
                return false;

            // Remove from highest index downward so earlier indices remain valid.
            indices.Sort();
            for (int i = indices.Count - 1; i >= 0; i--)
                placements.RemoveAt(indices[i]);

            removed = candidates;
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

        /// <summary>
        /// Places a newly received catch in the first available location without rearranging existing cargo.
        /// The UI and future fishing system can use this same acquisition path.
        /// </summary>
        public bool TryPlaceFirstFit(
            string itemId,
            int quantity,
            float quality,
            float freshness,
            out string instanceId,
            float saleQuantity = -1f)
        {
            instanceId = null;
            if (saleQuantity == -1f) saleQuantity = quantity;
            if (FindDefinition(itemId) == null || quantity < 1 || !IsFinite(saleQuantity) || saleQuantity <= 0f) return false;

            for (int rotation = 0; rotation < 4; rotation++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (TryPlace(itemId, x, y, rotation, quantity, quality, freshness, out instanceId, saleQuantity))
                            return true;
                    }
                }
            }

            return false;
        }

        public CargoItemDefinition GetItemDefinition(string itemId)
        {
            return FindDefinition(itemId);
        }

        public bool IsCellBlocked(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height || blockedCells == null)
                return false;

            for (int i = 0; i < blockedCells.Length; i++)
            {
                if (blockedCells[i].x == x && blockedCells[i].y == y)
                    return true;
            }

            return false;
        }

        public bool TryGetPlacementAtCell(int x, int y, out CargoPlacement result)
        {
            result = null;
            if (x < 0 || y < 0 || x >= width || y >= height) return false;

            Vector2Int target = new Vector2Int(x, y);
            for (int i = 0; i < placements.Count; i++)
            {
                CargoPlacement placement = placements[i];
                if (placement == null) continue;

                CargoItemDefinition definition = FindDefinition(placement.itemId);
                if (definition == null) continue;

                Vector2Int[] shape = CargoGridGeometry.RotateAndNormalize(
                    definition.OccupiedCells,
                    placement.rotationQuarterTurns);

                for (int cellIndex = 0; cellIndex < shape.Length; cellIndex++)
                {
                    if (new Vector2Int(placement.anchorX, placement.anchorY) + shape[cellIndex] == target)
                    {
                        result = placement;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Damages one physical hold cell. Cargo occupying that cell is destroyed unless it is quest-critical.
        /// Equipment and ordinary cargo use the same placement grid and therefore share damage risk.
        /// </summary>
        public bool TryDamageCell(int x, int y, out CargoPlacement destroyedCargo)
        {
            destroyedCargo = null;
            if (x < 0 || y < 0 || x >= width || y >= height || IsCellBlocked(x, y))
                return false;

            CargoPlacement occupying;
            if (TryGetPlacementAtCell(x, y, out occupying))
            {
                CargoItemDefinition definition = FindDefinition(occupying.itemId);
                if (definition == null || definition.IsQuestCritical || !definition.CanBeDamaged)
                    return false;

                CargoPlacement removed;
                if (TryRemove(occupying.instanceId, out removed))
                    destroyedCargo = removed;
            }

            List<Vector2Int> cells = new List<Vector2Int>();
            if (blockedCells != null) cells.AddRange(blockedCells);
            cells.Add(new Vector2Int(x, y));
            blockedCells = cells.ToArray();
            return true;
        }

        public bool TryRepairCell(int x, int y)
        {
            if (!IsCellBlocked(x, y)) return false;
            List<Vector2Int> cells = new List<Vector2Int>();
            for (int i = 0; blockedCells != null && i < blockedCells.Length; i++)
                if (blockedCells[i] != new Vector2Int(x, y))
                    cells.Add(blockedCells[i]);
            blockedCells = cells.ToArray();
            return true;
        }

        public bool IsEquipment(string instanceId)
        {
            CargoPlacement placement = FindPlacement(instanceId);
            CargoItemDefinition definition = placement == null ? null : FindDefinition(placement.itemId);
            return definition != null && definition.Category == CargoItemCategory.Equipment;
        }

        public int GetUsedCellCount()
        {
            bool[,] occupied = BuildOccupiedGrid(null);
            int count = 0;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (occupied[x, y]) count++;
            return count;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.inventory == null)
                throw new ArgumentNullException(nameof(data), "Save data must contain inventory data.");

            data.inventory.holdWidth = width;
            data.inventory.holdHeight = height;
            data.inventory.placements.Clear();
            data.inventory.damagedCells.Clear();
            for (int cellIndex = 0; blockedCells != null && cellIndex < blockedCells.Length; cellIndex++)
                data.inventory.damagedCells.Add(new GridCellSaveData { x = blockedCells[cellIndex].x, y = blockedCells[cellIndex].y });

            HashSet<string> instanceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < placements.Count; i++)
            {
                CargoPlacement item = placements[i];
                if (item == null ||
                    !IsValidInstanceId(item.instanceId) ||
                    !instanceIds.Add(item.instanceId))
                    throw new InvalidOperationException("The live cargo hold contains a malformed or duplicate instance ID.");

                data.inventory.placements.Add(new CargoPlacementSaveData
                {
                    instanceId = item.instanceId,
                    itemId = item.itemId,
                    anchorX = item.anchorX,
                    anchorY = item.anchorY,
                    rotationQuarterTurns = item.rotationQuarterTurns,
                    quantity = item.quantity,
                    quality = item.quality,
                    freshness = item.freshness,
                    saleQuantity = item.saleQuantity
                });
            }
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.inventory == null ||
                data.inventory.placements == null || data.inventory.damagedCells == null)
            {
                error = "The save does not contain a complete cargo-hold record.";
                return false;
            }

            List<Vector2Int> restoredDamage = new List<Vector2Int>();
            HashSet<long> damagedKeys = new HashSet<long>();
            for (int i = 0; i < data.inventory.damagedCells.Count; i++)
            {
                GridCellSaveData cell = data.inventory.damagedCells[i];
                if (cell == null || cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height)
                {
                    error = "The save contains a damaged-cell coordinate outside the authored hold.";
                    return false;
                }

                long key = ((long)cell.x << 32) | (uint)cell.y;
                if (!damagedKeys.Add(key))
                {
                    error = "The save contains a duplicate damaged-cell coordinate.";
                    return false;
                }
                restoredDamage.Add(new Vector2Int(cell.x, cell.y));
            }

            Vector2Int[] damageArray = restoredDamage.ToArray();
            bool[,] blocked = CargoGridGeometry.BuildBlockedGrid(width, height, damageArray);
            bool[,] occupied = new bool[width, height];
            HashSet<string> instanceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < data.inventory.placements.Count; i++)
            {
                CargoPlacementSaveData saved = data.inventory.placements[i];
                if (saved == null || !IsValidInstanceId(saved.instanceId) ||
                    !instanceIds.Add(saved.instanceId) ||
                    string.IsNullOrWhiteSpace(saved.itemId) || saved.quantity < 1 ||
                    saved.rotationQuarterTurns < 0 || saved.rotationQuarterTurns > 3 ||
                    !IsNormalized(saved.quality) || !IsNormalized(saved.freshness) ||
                    !IsFinite(saved.saleQuantity) || saved.saleQuantity <= 0f)
                {
                    error = "The save contains a malformed or duplicate cargo instance at index " + i + ".";
                    return false;
                }

                CargoItemDefinition definition = FindDefinition(saved.itemId);
                if (definition == null)
                {
                    error = "The save refers to an unknown cargo definition '" + saved.itemId + "'.";
                    return false;
                }

                Vector2Int[] shape = CargoGridGeometry.RotateAndNormalize(
                    definition.OccupiedCells, saved.rotationQuarterTurns);
                Vector2Int anchor = new Vector2Int(saved.anchorX, saved.anchorY);
                if (!CargoGridGeometry.CanPlace(width, height, occupied, blocked, shape, anchor))
                {
                    error = "The saved cargo placement does not fit the current hold: " + saved.itemId;
                    return false;
                }

                for (int cellIndex = 0; cellIndex < shape.Length; cellIndex++)
                {
                    Vector2Int cell = anchor + shape[cellIndex];
                    occupied[cell.x, cell.y] = true;
                }
            }

            return true;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.inventory == null) return;

            string error;
            if (!TryValidateSaveData(data, out error))
                throw new InvalidOperationException(error);

            // Build replacement collections first. A failed validation can never clear live cargo.
            List<Vector2Int> restoredDamage = new List<Vector2Int>();
            for (int i = 0; i < data.inventory.damagedCells.Count; i++)
            {
                GridCellSaveData cell = data.inventory.damagedCells[i];
                restoredDamage.Add(new Vector2Int(cell.x, cell.y));
            }

            List<CargoPlacement> restoredPlacements = new List<CargoPlacement>(data.inventory.placements.Count);
            for (int i = 0; i < data.inventory.placements.Count; i++)
            {
                CargoPlacementSaveData saved = data.inventory.placements[i];
                restoredPlacements.Add(new CargoPlacement
                {
                    instanceId = saved.instanceId,
                    itemId = saved.itemId,
                    anchorX = saved.anchorX,
                    anchorY = saved.anchorY,
                    rotationQuarterTurns = saved.rotationQuarterTurns,
                    quantity = saved.quantity,
                    quality = saved.quality,
                    freshness = saved.freshness,
                    saleQuantity = saved.saleQuantity
                });
            }

            blockedCells = restoredDamage.ToArray();
            placements = restoredPlacements;
        }

        private static bool IsValidInstanceId(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId) ||
                !string.Equals(instanceId, instanceId.Trim(), StringComparison.Ordinal))
                return false;

            Guid parsed;
            bool valid = Guid.TryParseExact(instanceId, "N", out parsed) ||
                         Guid.TryParseExact(instanceId, "D", out parsed);
            return valid && parsed != Guid.Empty;
        }

        private static bool IsFinite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }

        private static bool IsNormalized(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value <= 1f;
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
