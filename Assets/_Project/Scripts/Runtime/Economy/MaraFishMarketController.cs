using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Interaction;
using WhereTheSoundingsEnd.NPC;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Economy
{
    [Serializable]
    public sealed class MarketSaleLine
    {
        public string instanceId;
        public string itemId;
        public string displayName;
        public float saleQuantity;
        public string unitLabel;
        public decimal demandFactor;
        public int buyerStock;
        public int buyerTargetStock;
        public long lineTotalCents;
        public string marketRegionId;
        public string marketName;
        public decimal destinationFactor;
        public SeafoodMarketClass marketClass;
    }

    /// <summary>
    /// Greyhaven's buyer-side seafood market. It owns buyer stock/demand only;
    /// cargo geometry, wallet balance, NPC trust and save coordination remain owned
    /// by their existing runtime systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraFishMarketController : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        public const string VendorId = "greyhaven.mara_venn";
        private const string MarketShiftActivity = "market_shift";
        private const int DefaultTargetStock = 20;
        private const int MaximumStockPerItem = 100000;

        [SerializeField] private SpatialCargoHold cargoHold;
        [SerializeField] private EconomyWalletController wallet;
        [SerializeField] private NpcAgent mara;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField] private RegionalChartController regionalChart;
        [SerializeField] private DocksideHubController dockside;
        [SerializeField, Min(0.01f)] private float destinationFactor = 1f;
        [SerializeField, Min(0f)] private float scarcityResponse = 0.35f;
        [SerializeField] private List<MarketStockSaveData> stock = new List<MarketStockSaveData>();

        public long WalletCents => wallet != null ? wallet.WalletCents : 0L;
        public string ActiveMarketRegionId => ResolveMarketRegionId();
        public string ActiveMarketName => SeafoodMarketPolicy.GetMarketName(ActiveMarketRegionId);
        public bool IsOpen
        {
            get
            {
                string region = ResolveMarketRegionId();
                if (region == "greyhaven")
                    return mara != null && mara.CanTrade &&
                           string.Equals(mara.ActivityId, MarketShiftActivity, StringComparison.Ordinal);
                return dockside != null && dockside.IsMenuOpen &&
                       SeafoodMarketPolicy.IsSupportedRegion(region);
            }
        }

        private void OnValidate()
        {
            if (!IsFinite(destinationFactor)) destinationFactor = 1f;
            destinationFactor = Mathf.Clamp(destinationFactor, 0.01f, 100f);
            if (!IsFinite(scarcityResponse)) scarcityResponse = 0.35f;
            scarcityResponse = Mathf.Clamp(scarcityResponse, 0f, 10f);
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (stock == null) stock = new List<MarketStockSaveData>();
        }

        private void Awake()
        {
            if (cargoHold == null) cargoHold = GetComponent<SpatialCargoHold>();
            if (wallet == null) wallet = GetComponent<EconomyWalletController>();
            if (regionalChart == null) regionalChart = GetComponent<RegionalChartController>();
            if (dockside == null) dockside = GetComponent<DocksideHubController>();
            if (mara == null)
            {
                NpcAgent[] residents = FindObjectsByType<NpcAgent>(FindObjectsSortMode.None);
                for (int i = 0; i < residents.Length; i++)
                    if (residents[i] != null && residents[i].NpcId == VendorId)
                    {
                        mara = residents[i];
                        break;
                    }
            }
            if (stock == null) stock = new List<MarketStockSaveData>();
        }

        public bool TryBuildOffer(IList<string> instanceIds, out List<MarketSaleLine> lines,
            out long totalCents, out string error)
        {
            lines = new List<MarketSaleLine>();
            totalCents = 0L;
            error = null;

            if (!IsOpen) { error = "No regional seafood buyer is available during this dockside session."; return false; }
            string marketRegionId = ResolveMarketRegionId();
            if (!SeafoodMarketPolicy.IsSupportedRegion(marketRegionId))
            { error = "This destination has no authored seafood market."; return false; }
            if (cargoHold == null || wallet == null) { error = "The market cannot access the vessel's cargo or wallet."; return false; }
            if (instanceIds == null || instanceIds.Count == 0) { error = "Select at least one cargo item."; return false; }

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < instanceIds.Count; i++)
            {
                string id = instanceIds[i];
                if (string.IsNullOrWhiteSpace(id) || !seen.Add(id))
                { error = "The selection contains an empty or duplicate cargo reference."; return false; }

                CargoPlacement placement = FindPlacement(id);
                if (placement == null) { error = "A selected cargo item is no longer aboard."; return false; }

                CargoItemDefinition definition = cargoHold.GetItemDefinition(placement.itemId);
                if (definition == null || !definition.OrdinarySaleEligible || definition.IsAberrant ||
                    (!string.IsNullOrWhiteSpace(placement.catchRegionId) && !placement.catchLegal) ||
                    !IsFinite(placement.saleQuantity) || placement.saleQuantity <= 0f ||
                    placement.saleQuantity > 1000000f ||
                    !IsFinite(definition.BaseSalePriceUsd) || definition.BaseSalePriceUsd <= 0f ||
                    definition.BaseSalePriceUsd > 1000000f ||
                    (definition.Category != CargoItemCategory.Fish &&
                     definition.Category != CargoItemCategory.Shellfish &&
                     definition.Category != CargoItemCategory.Cephalopod))
                { error = "Mara does not accept this cargo as ordinary seafood."; return false; }

                MarketStockSaveData itemStock = GetOrCreateStock(definition.ItemId);
                RefreshStock(itemStock);
                decimal destinationMarketFactor = SeafoodMarketPolicy.GetDestinationFactor(
                    marketRegionId, definition.MarketClass);
                if (destinationMarketFactor <= 0m)
                { error = "This destination does not trade the selected seafood class."; return false; }
                decimal demand = EconomyPolicy.CalculateDemandFactor(
                    itemStock.targetStock, itemStock.currentStock, (decimal)scarcityResponse);

                decimal qualityFactor = 0.60m + 0.60m * (decimal)Mathf.Clamp01(placement.quality);
                decimal freshnessFactor = 0.35m + 0.65m * (decimal)Mathf.Clamp01(placement.freshness);
                long lineTotal = EconomyPolicy.CalculateSaleLineCents(
                    (decimal)placement.saleQuantity,
                    (decimal)definition.BaseSalePriceUsd,
                    (decimal)destinationFactor * destinationMarketFactor *
                    GetRelationshipSaleFactor(marketRegionId == "greyhaven" && mara != null ? mara.RelationshipScore : 0),
                    qualityFactor,
                    freshnessFactor,
                    demand,
                    1m,
                    true);

                if (lineTotal <= 0L) { error = "The selected item has no valid sale value."; return false; }
                if (long.MaxValue - totalCents < lineTotal)
                { error = "The sale total exceeds the supported wallet range."; return false; }

                totalCents += lineTotal;
                lines.Add(new MarketSaleLine
                {
                    instanceId = placement.instanceId,
                    itemId = definition.ItemId,
                    displayName = definition.DisplayName,
                    saleQuantity = placement.saleQuantity,
                    unitLabel = definition.SaleUnitLabel,
                    demandFactor = demand,
                    buyerStock = itemStock.currentStock,
                    buyerTargetStock = itemStock.targetStock,
                    lineTotalCents = lineTotal,
                    marketRegionId = marketRegionId,
                    marketName = SeafoodMarketPolicy.GetMarketName(marketRegionId),
                    destinationFactor = destinationMarketFactor,
                    marketClass = definition.MarketClass
                });
            }

            return totalCents > 0L;
        }

        public bool TrySell(IList<string> instanceIds, string transactionId,
            out List<MarketSaleLine> receipt, out long paidCents, out string error)
        {
            receipt = null;
            paidCents = 0L;
            if (string.IsNullOrWhiteSpace(transactionId) ||
                !string.Equals(transactionId, transactionId.Trim(), StringComparison.Ordinal) ||
                transactionId.Length > 128)
            { error = "A valid transaction reference is required."; return false; }

            if (wallet != null && wallet.HasProcessedTransaction(transactionId))
            { error = "This sale has already been processed."; return false; }

            List<MarketSaleLine> offer;
            long total;
            if (!TryBuildOffer(instanceIds, out offer, out total, out error)) return false;
            if (wallet == null || total <= 0L || wallet.WalletCents > 1000000000000L - total)
            { error = "The wallet cannot accept this transaction."; return false; }

            List<CargoPlacement> removed;
            bool committed = cargoHold.TryCommitRemoval(instanceIds, () =>
                wallet.TryApplyTransaction(transactionId, total), out removed);
            if (!committed)
            { error = "The sale could not be committed. No cargo or money was changed."; return false; }

            for (int i = 0; i < removed.Count; i++)
            {
                CargoPlacement placement = removed[i];
                MarketStockSaveData itemStock = GetOrCreateStock(placement.itemId);
                long nextStock = (long)itemStock.currentStock + Math.Max(1, placement.quantity);
                itemStock.currentStock = (int)Math.Min(MaximumStockPerItem, nextStock);
            }

            receipt = offer;
            paidCents = total;
            error = null;
            return true;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.economy == null)
                throw new ArgumentNullException(nameof(data));
            if (data.economy.marketStocks == null)
                data.economy.marketStocks = new List<MarketStockSaveData>();
            // Preserve other vendors' market state when additional profiles are added.
            data.economy.marketStocks.RemoveAll(entry => entry == null ||
                entry.vendorId == VendorId ||
                (entry.vendorId != null && entry.vendorId.StartsWith("regional_market.", StringComparison.Ordinal)));
            for (int i = 0; i < stock.Count; i++)
            {
                MarketStockSaveData entry = stock[i];
                if (entry == null) continue;
                data.economy.marketStocks.Add(new MarketStockSaveData
                {
                    vendorId = entry.vendorId,
                    itemId = entry.itemId,
                    currentStock = entry.currentStock,
                    targetStock = entry.targetStock,
                    lastRestockDay = entry.lastRestockDay
                });
            }
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.economy == null || data.economy.marketStocks == null ||
                data.economy.marketStocks.Count > SaveGameDataMigration.MaximumCollectionEntries)
            { error = "The save contains invalid market stock data."; return false; }

            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < data.economy.marketStocks.Count; i++)
            {
                MarketStockSaveData entry = data.economy.marketStocks[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.vendorId) ||
                    string.IsNullOrWhiteSpace(entry.itemId) || entry.currentStock < 0 ||
                    entry.currentStock > MaximumStockPerItem || entry.targetStock < 1 || entry.lastRestockDay < 0 ||
                    entry.targetStock > MaximumStockPerItem ||
                    !ids.Add(entry.vendorId + "|" + entry.itemId))
                { error = "The save contains malformed or duplicate vendor stock."; return false; }
            }
            return true;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.economy == null || data.economy.marketStocks == null) return;
            string error;
            if (!TryValidateSaveData(data, out error)) throw new InvalidOperationException(error);

            List<MarketStockSaveData> restored = new List<MarketStockSaveData>();
            for (int i = 0; i < data.economy.marketStocks.Count; i++)
            {
                MarketStockSaveData entry = data.economy.marketStocks[i];
                if (entry.vendorId != VendorId &&
                    (entry.vendorId == null || !entry.vendorId.StartsWith("regional_market.", StringComparison.Ordinal)))
                    continue;
                restored.Add(new MarketStockSaveData
                {
                    vendorId = entry.vendorId == VendorId ? "regional_market.greyhaven" : entry.vendorId,
                    itemId = entry.itemId,
                    currentStock = entry.currentStock,
                    targetStock = entry.targetStock,
                    lastRestockDay = entry.lastRestockDay
                });
            }
            stock = restored;
        }

        private static decimal GetRelationshipSaleFactor(int score)
        {
            if (score >= 90) return 1.10m;
            if (score >= 75) return 1.05m;
            if (score >= 60) return 1.02m;
            return 1.00m;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private CargoPlacement FindPlacement(string instanceId)
        {
            IReadOnlyList<CargoPlacement> placements = cargoHold.Placements;
            for (int i = 0; i < placements.Count; i++)
                if (placements[i] != null && placements[i].instanceId == instanceId)
                    return placements[i];
            return null;
        }

        private string ResolveMarketRegionId()
        {
            if (regionalChart == null) regionalChart = GetComponent<RegionalChartController>();
            string region = regionalChart != null ? regionalChart.CurrentRegionId : string.Empty;
            if (!string.IsNullOrWhiteSpace(region)) return region.Trim().ToLowerInvariant();
            return mara != null ? "greyhaven" : string.Empty;
        }

        private string GetActiveVendorId()
        {
            string region = ResolveMarketRegionId();
            return SeafoodMarketPolicy.IsSupportedRegion(region)
                ? "regional_market." + region
                : VendorId;
        }

        private MarketStockSaveData GetOrCreateStock(string itemId)
        {
            for (int i = 0; i < stock.Count; i++)
                if (stock[i] != null && stock[i].itemId == itemId &&
                    stock[i].vendorId == GetActiveVendorId())
                    return stock[i];

            MarketStockSaveData created = new MarketStockSaveData
            {
                vendorId = GetActiveVendorId(),
                itemId = itemId,
                currentStock = DefaultTargetStock,
                targetStock = DefaultTargetStock,
                lastRestockDay = seaState != null ? seaState.DayNumber : 1
            };
            stock.Add(created);
            return created;
        }

        /// <summary>Restocks a bounded fraction of buyer capacity once per completed game day.</summary>
        private void RefreshStock(MarketStockSaveData entry)
        {
            if (entry == null || seaState == null) return;
            int currentDay = Math.Max(1, seaState.DayNumber);
            if (entry.lastRestockDay <= 0) entry.lastRestockDay = currentDay;
            int elapsedDays = currentDay - entry.lastRestockDay;
            if (elapsedDays <= 0) return;

            int dailyRestock = Math.Max(1, Mathf.CeilToInt(entry.targetStock * 0.35f));
            long restored = (long)entry.currentStock + (long)dailyRestock * elapsedDays;
            entry.currentStock = (int)Math.Min(entry.targetStock, Math.Min(MaximumStockPerItem, restored));
            entry.lastRestockDay = currentDay;
        }
    }
}
