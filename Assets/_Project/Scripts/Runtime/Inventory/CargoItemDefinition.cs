using System;
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
        DamagedCargo,
        Seaweed
    }

    [Flags]
    public enum FishHabitat
    {
        None = 0,
        Nearshore = 1,
        Kelp = 2,
        OpenWater = 4,
        DeepWater = 8,
        Estuary = 16,
        All = Nearshore | Kelp | OpenWater | DeepWater | Estuary
    }

    public enum SeafoodMarketClass
    {
        Inshore,
        Reef,
        Pelagic,
        Deepwater,
        Invertebrate,
        Seaweed
    }

    public enum FishPreferredTechnique
    {
        Any,
        StillFishing,
        BottomFishing,
        FloatFishing,
        Jigging,
        Trolling,
        Drifting,
        Handlining
    }

    /// <summary>
    /// Authored shape and handling data for one cargo type. Fish-specific fields are ignored
    /// for non-fish cargo and provide the data contract used by the fishing simulation.
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
        [SerializeField] private bool isAberrant;
        [SerializeField, Min(1f)] private float sellValueMultiplier = 1f;
        [Header("Economy")]
        [SerializeField, Min(0f)] private float baseSalePriceUsd;
        [SerializeField] private string saleUnitLabel = "unit";
        [SerializeField] private bool ordinarySaleEligible;
        [SerializeField] private SeafoodMarketClass marketClass = SeafoodMarketClass.Inshore;
        [SerializeField] private string[] availableRegionIds = Array.Empty<string>();
        [SerializeField, Min(0f)] private float minimumLegalCatchWeightKg = 0.05f;
        [SerializeField] private bool protectedCatch;
        [SerializeField] private string requiredPermitId = string.Empty;
        [SerializeField] private int allowedTechniqueMask;

        [Header("Fish ecology and encounter")]
        [SerializeField] private FishHabitat habitats = FishHabitat.All;
        [SerializeField] private FishPreferredTechnique preferredTechnique = FishPreferredTechnique.Any;
        [SerializeField] private string preferredBaitId = "any";
        [SerializeField, Min(0f)] private float minimumDepthMetres;
        [SerializeField, Min(0f)] private float maximumDepthMetres = 250f;
        [SerializeField, Range(0.1f, 2f)] private float resistance = 1f;
        [SerializeField, Range(0f, 1f)] private float burstFrequency = 0.35f;
        [SerializeField, Min(1)] private int initialPopulation = 36;
        [SerializeField, Min(0)] private int dailyPopulationRecovery = 2;
        [SerializeField, Range(0f, 1f)] private float migrationRate = 0.12f;
        [SerializeField, Min(0.1f)] private float baseCatchSize = 1f;
        [SerializeField, Min(0.1f)] private float maximumCatchSize = 3f;

        public string ItemId { get { return itemId; } }
        public string DisplayName { get { return displayName; } }
        public CargoItemCategory Category { get { return category; } }
        public Vector2Int[] OccupiedCells { get { return occupiedCells; } }
        public float FreshnessLossPerGameMinute { get { return freshnessLossPerGameMinute; } }
        public bool CanBeDamaged { get { return canBeDamaged; } }
        public bool IsQuestCritical { get { return isQuestCritical; } }
        public bool IsAberrant { get { return isAberrant; } }
        public float SellValueMultiplier { get { return Mathf.Max(1f, sellValueMultiplier); } }
        public float BaseSalePriceUsd { get { return Mathf.Max(0f, baseSalePriceUsd); } }
        public string SaleUnitLabel { get { return string.IsNullOrWhiteSpace(saleUnitLabel) ? "unit" : saleUnitLabel.Trim(); } }
        public bool OrdinarySaleEligible { get { return ordinarySaleEligible && !isQuestCritical && category != CargoItemCategory.Quest; } }
        public SeafoodMarketClass MarketClass { get { return marketClass; } }
        public string[] AvailableRegionIds { get { return availableRegionIds ?? Array.Empty<string>(); } }
        public float MinimumLegalCatchWeightKg { get { return Mathf.Max(0f, minimumLegalCatchWeightKg); } }
        public bool ProtectedCatch { get { return protectedCatch; } }
        public string RequiredPermitId { get { return requiredPermitId ?? string.Empty; } }
        public int AllowedTechniqueMask { get { return Mathf.Max(0, allowedTechniqueMask); } }
        public bool IsAvailableInRegion(string regionId)
        {
            if (availableRegionIds == null || availableRegionIds.Length == 0) return true;
            if (string.IsNullOrWhiteSpace(regionId)) return false;
            for (int i = 0; i < availableRegionIds.Length; i++)
                if (string.Equals(availableRegionIds[i], regionId, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
        public FishHabitat Habitats { get { return habitats == FishHabitat.None ? FishHabitat.All : habitats; } }
        public FishPreferredTechnique PreferredTechnique { get { return preferredTechnique; } }
        public string PreferredBaitId { get { return string.IsNullOrWhiteSpace(preferredBaitId) ? "any" : preferredBaitId; } }
        public float MinimumDepthMetres { get { return minimumDepthMetres; } }
        public float MaximumDepthMetres { get { return Mathf.Max(minimumDepthMetres, maximumDepthMetres); } }
        public float Resistance { get { return resistance; } }
        public float BurstFrequency { get { return burstFrequency; } }
        public int InitialPopulation { get { return initialPopulation; } }
        public int DailyPopulationRecovery { get { return dailyPopulationRecovery; } }
        public float MigrationRate { get { return migrationRate; } }
        public float BaseCatchSize { get { return baseCatchSize; } }
        public float MaximumCatchSize { get { return Mathf.Max(baseCatchSize, maximumCatchSize); } }

        private void OnValidate()
        {
            if (occupiedCells == null || occupiedCells.Length == 0)
                occupiedCells = new[] { Vector2Int.zero };

            if (string.IsNullOrWhiteSpace(saleUnitLabel)) saleUnitLabel = "unit";

            if (string.IsNullOrWhiteSpace(itemId))
                itemId = name.ToLowerInvariant().Replace(' ', '_');

            maximumDepthMetres = Mathf.Max(minimumDepthMetres, maximumDepthMetres);
            maximumCatchSize = Mathf.Max(baseCatchSize, maximumCatchSize);
            initialPopulation = Mathf.Max(1, initialPopulation);
        }
    }
}
