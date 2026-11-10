using UnityEngine;

namespace WhereTheSoundingsEnd.Inventory
{
    public enum CargoItemCategory
    {
        Fish,
        Shellfish,
        Cephalopod,
        Salvage,
        Quest,
        Equipment,
        DamagedCargo
    }

    /// <summary>
    /// Authored shape and handling data for one cargo type. Shapes are authored as occupied grid cells.
    /// </summary>
    [CreateAssetMenu(fileName = "CargoItem", menuName = "WHERE THE SOUNDINGS END/Inventory/Cargo Item")]
    public sealed class CargoItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private CargoItemCategory category;
        [SerializeField] private Vector2Int[] occupiedCells =
        {
            Vector2Int.zero,
            Vector2Int.right
        };
        [SerializeField, Min(0f)] private float freshnessLossPerGameMinute;
        [SerializeField] private bool canBeDamaged = true;
        [SerializeField] private bool isQuestCritical;

        public string ItemId { get { return itemId; } }
        public string DisplayName { get { return displayName; } }
        public CargoItemCategory Category { get { return category; } }
        public Vector2Int[] OccupiedCells { get { return occupiedCells; } }
        public float FreshnessLossPerGameMinute { get { return freshnessLossPerGameMinute; } }
        public bool CanBeDamaged { get { return canBeDamaged; } }
        public bool IsQuestCritical { get { return isQuestCritical; } }

        private void OnValidate()
        {
            if (occupiedCells == null || occupiedCells.Length == 0)
                occupiedCells = new[] { Vector2Int.zero };

            if (string.IsNullOrWhiteSpace(itemId))
                itemId = name.ToLowerInvariant().Replace(' ', '_');
        }
    }
}
