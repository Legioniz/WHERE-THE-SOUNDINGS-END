using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.NPC;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>Transactional services for Tomas's repair shed and Inez's supply counter.</summary>
    [DisallowMultipleComponent]
    public sealed class GreyhavenVendorServices : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        public const string TomasId = "greyhaven.tomas_vale";
        public const string InezId = "greyhaven.inez_sarmiento";
        private const string TomasShift = "repair_shift";
        private const string InezShift = "net_mending";
        private const int MaximumStock = 100000;

        [SerializeField] private SpatialCargoHold cargo;
        [SerializeField] private EconomyWalletController wallet;
        [SerializeField] private VesselConditionController condition;
        [SerializeField] private VesselFuelController fuel;
        [SerializeField] private NpcAgent tomas;
        [SerializeField] private NpcAgent inez;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField] private List<MarketStockSaveData> supplyStock = new List<MarketStockSaveData>();
        [SerializeField, Min(0f)] private float hullRepairUsdPerPoint = 0.22f;
        [SerializeField, Min(0f)] private float engineRepairUsdPerPoint = 0.30f;
        [SerializeField, Min(0f)] private float fuelUsdPerUnit = 0.11f;
        [SerializeField, Min(0f)] private float holdCellRepairUsd = 1.25f;

        public float FuelUnits => fuel != null ? fuel.FuelUnits : 0f;
        public float FuelCapacity => fuel != null ? fuel.Capacity : 0f;
        public bool TomasAvailable => tomas != null && tomas.CanTrade &&
            string.Equals(tomas.ActivityId, TomasShift, StringComparison.Ordinal);
        public bool InezAvailable => inez != null && inez.CanTrade &&
            string.Equals(inez.ActivityId, InezShift, StringComparison.Ordinal);

        private void Awake()
        {
            if (cargo == null) cargo = GetComponent<SpatialCargoHold>();
            if (wallet == null) wallet = GetComponent<EconomyWalletController>();
            if (condition == null) condition = GetComponent<VesselConditionController>();
            if (fuel == null) fuel = GetComponent<VesselFuelController>();
            if (tomas == null || inez == null)
            {
                NpcAgent[] residents = FindObjectsByType<NpcAgent>(FindObjectsSortMode.None);
                for (int i = 0; i < residents.Length; i++)
                {
                    if (residents[i] == null) continue;
                    if (residents[i].NpcId == TomasId) tomas = residents[i];
                    else if (residents[i].NpcId == InezId) inez = residents[i];
                }
            }
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (supplyStock == null) supplyStock = new List<MarketStockSaveData>();
        }

        public long GetRepairQuoteCents()
        {
            if (condition == null) return 0L;
            decimal hull = (decimal)Mathf.Max(0f, 100f - condition.HullIntegrity);
            decimal engine = (decimal)Mathf.Max(0f, 100f - condition.EngineCondition);
            long baseQuote = SafeAdd(
                EconomyPolicy.CalculateRepairCostCents(hull, (decimal)hullRepairUsdPerPoint),
                EconomyPolicy.CalculateRepairCostCents(engine, (decimal)engineRepairUsdPerPoint));
            return ApplyDiscount(baseQuote, GetRelationshipDiscount(tomas != null ? tomas.RelationshipScore : 0));
        }

        public long GetRefuelQuoteCents(float requestedUnits)
        {
            if (fuel == null) return 0L;
            float amount = fuel.GetRefuelAmount(requestedUnits);
            long baseQuote = EconomyPolicy.CalculateFuelCostCents((decimal)amount, (decimal)fuelUsdPerUnit);
            return ApplyDiscount(baseQuote, GetRelationshipDiscount(tomas != null ? tomas.RelationshipScore : 0));
        }

        public bool TryRepairAll(string transactionId, out long chargedCents, out string error)
        {
            chargedCents = 0L;
            if (!TomasAvailable) { error = "Tomas is not available to trade or repair right now."; return false; }
            if (condition == null || wallet == null) { error = "Repair services are unavailable."; return false; }

            float hullMissing = Mathf.Max(0f, 100f - condition.HullIntegrity);
            float engineMissing = Mathf.Max(0f, 100f - condition.EngineCondition);
            long quote = GetRepairQuoteCents();
            if (quote <= 0L) { error = "The hull and engine are already in working condition."; return false; }
            if (!IsValidTransactionId(transactionId) || wallet.HasProcessedTransaction(transactionId))
            { error = "The repair transaction reference is invalid or already used."; return false; }
            if (!wallet.TryApplyTransaction(transactionId, -quote))
            { error = "Insufficient funds or wallet transaction rejected. No repairs were applied."; return false; }

            condition.RepairHull(hullMissing);
            condition.RepairEngine(engineMissing);
            chargedCents = quote;
            error = null;
            return true;
        }

        public bool TryRefuel(float requestedUnits, string transactionId, out float addedUnits,
            out long chargedCents, out string error)
        {
            addedUnits = 0f;
            chargedCents = 0L;
            if (!TomasAvailable) { error = "Tomas is not available to trade right now."; return false; }
            if (fuel == null || wallet == null) { error = "Fuel services are unavailable."; return false; }
            float amount = fuel.GetRefuelAmount(requestedUnits);
            long quote = EconomyPolicy.CalculateFuelCostCents((decimal)amount, (decimal)fuelUsdPerUnit);
            if (amount <= 0f || quote <= 0L) { error = "The tank is already full or the requested amount is invalid."; return false; }
            if (!IsValidTransactionId(transactionId) || wallet.HasProcessedTransaction(transactionId))
            { error = "The fuel transaction reference is invalid or already used."; return false; }
            if (!wallet.TryApplyTransaction(transactionId, -quote))
            { error = "Insufficient funds. No fuel was added."; return false; }

            fuel.AddFuel(amount);
            addedUnits = amount;
            chargedCents = quote;
            error = null;
            return true;
        }

        public bool TryRepairHoldCell(int x, int y, string transactionId,
            out long chargedCents, out string error)
        {
            chargedCents = 0L;
            if (!TomasAvailable) { error = "Tomas is not available to trade right now."; return false; }
            if (cargo == null || wallet == null || !cargo.IsCellBlocked(x, y))
            { error = "That hold cell does not require repair."; return false; }
            long quote = EconomyPolicy.CalculateRepairCostCents(1m, (decimal)holdCellRepairUsd);
            if (!IsValidTransactionId(transactionId) || wallet.HasProcessedTransaction(transactionId) ||
                !wallet.TryApplyTransaction(transactionId, -quote))
            { error = "The hold repair could not be paid for. No repair was applied."; return false; }
            if (!cargo.TryRepairCell(x, y))
                throw new InvalidOperationException("A validated hold cell changed during a synchronous repair transaction.");
            chargedCents = quote;
            error = null;
            return true;
        }

        public int GetStock(string itemId)
        {
            MarketStockSaveData entry = FindStock(itemId);
            return entry != null ? entry.currentStock : 0;
        }

        public long GetPurchaseQuoteCents(string itemId, int quantity)
        {
            if (!InezAvailable || cargo == null || quantity < 1 || quantity > 99) return 0L;
            CargoItemDefinition definition = cargo.GetItemDefinition(itemId);
            decimal unitPrice;
            if (definition == null || !TryGetSupplyPrice(itemId, out unitPrice)) return 0L;
            MarketStockSaveData entry = GetOrCreateStock(itemId);
            RefreshStock(entry);
            if (entry.currentStock < quantity) return 0L;
            decimal discount = GetRelationshipDiscount(inez.RelationshipScore);
            decimal totalUsd = unitPrice * quantity * (1m - discount);
            decimal cents = decimal.Round(totalUsd * 100m, 0, MidpointRounding.AwayFromZero);
            return cents <= 0m || cents >= long.MaxValue ? 0L : (long)cents;
        }

        public bool TryPurchase(string itemId, int quantity, string transactionId,
            out long chargedCents, out string error)
        {
            chargedCents = 0L;
            if (!InezAvailable) { error = "Inez is not available to trade right now."; return false; }
            if (cargo == null || wallet == null) { error = "The supply counter cannot access cargo or wallet data."; return false; }
            CargoItemDefinition definition = cargo.GetItemDefinition(itemId);
            if (definition == null || definition.Category != CargoItemCategory.Equipment ||
                definition.IsQuestCritical || definition.IsAberrant)
            { error = "Inez does not stock that item."; return false; }
            if (!IsValidTransactionId(transactionId) || wallet.HasProcessedTransaction(transactionId))
            { error = "The purchase transaction reference is invalid or already used."; return false; }

            long quote = GetPurchaseQuoteCents(itemId, quantity);
            if (quote <= 0L) { error = "Insufficient stock or invalid purchase quantity."; return false; }
            MarketStockSaveData entry = GetOrCreateStock(itemId);
            string placementId;
            if (!cargo.TryPlaceFirstFit(itemId, quantity, 1f, 1f, out placementId))
            { error = "There is not enough free cargo space for this purchase."; return false; }

            if (!wallet.TryApplyTransaction(transactionId, -quote))
            {
                CargoPlacement rollback;
                cargo.TryRemove(placementId, out rollback);
                error = "Insufficient funds or duplicate transaction. No goods were purchased.";
                return false;
            }

            entry.currentStock -= quantity;
            chargedCents = quote;
            error = null;
            return true;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.economy == null) throw new ArgumentNullException(nameof(data));
            if (data.economy.marketStocks == null) data.economy.marketStocks = new List<MarketStockSaveData>();
            data.economy.marketStocks.RemoveAll(entry => entry == null || entry.vendorId == InezId);
            for (int i = 0; i < supplyStock.Count; i++)
            {
                MarketStockSaveData entry = supplyStock[i];
                if (entry == null) continue;
                data.economy.marketStocks.Add(new MarketStockSaveData
                {
                    vendorId = InezId, itemId = entry.itemId,
                    currentStock = entry.currentStock, targetStock = entry.targetStock,
                    lastRestockDay = entry.lastRestockDay
                });
            }
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.economy == null || data.economy.marketStocks == null ||
                data.economy.marketStocks.Count > SaveGameDataMigration.MaximumCollectionEntries)
            { error = "The save contains invalid Inez supply stock."; return false; }
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < data.economy.marketStocks.Count; i++)
            {
                MarketStockSaveData entry = data.economy.marketStocks[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.vendorId) ||
                    string.IsNullOrWhiteSpace(entry.itemId) || entry.currentStock < 0 ||
                    entry.currentStock > MaximumStock || entry.targetStock < 1 ||
                    entry.targetStock > MaximumStock || !seen.Add(entry.vendorId + "|" + entry.itemId))
                { error = "The save contains malformed or duplicate vendor inventory."; return false; }
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
                if (entry.vendorId != InezId) continue;
                restored.Add(new MarketStockSaveData
                {
                    vendorId = InezId, itemId = entry.itemId,
                    currentStock = entry.currentStock, targetStock = entry.targetStock,
                    lastRestockDay = entry.lastRestockDay
                });
            }
            supplyStock = restored;
        }

        private MarketStockSaveData FindStock(string itemId)
        {
            for (int i = 0; i < supplyStock.Count; i++)
                if (supplyStock[i] != null && supplyStock[i].itemId == itemId) return supplyStock[i];
            return null;
        }

        private MarketStockSaveData GetOrCreateStock(string itemId)
        {
            MarketStockSaveData found = FindStock(itemId);
            if (found != null) return found;
            MarketStockSaveData created = new MarketStockSaveData
            {
                vendorId = InezId, itemId = itemId, currentStock = 8, targetStock = 8
            };
            supplyStock.Add(created);
            return created;
        }

        private void RefreshStock(MarketStockSaveData entry)
        {
            if (entry == null || seaState == null) return;
            int currentDay = Math.Max(1, seaState.DayNumber);
            if (entry.lastRestockDay <= 0) entry.lastRestockDay = currentDay;
            int elapsedDays = currentDay - entry.lastRestockDay;
            if (elapsedDays <= 0) return;
            int dailyRestock = Math.Max(1, Mathf.CeilToInt(entry.targetStock * 0.25f));
            long restored = (long)entry.currentStock + (long)dailyRestock * elapsedDays;
            entry.currentStock = (int)Math.Min(entry.targetStock, Math.Min(MaximumStock, restored));
            entry.lastRestockDay = currentDay;
        }

        private static bool TryGetSupplyPrice(string itemId, out decimal price)
        {
            switch (itemId)
            {
                case "GEAR-ROD-01": price = 12.00m; return true;
                case "GEAR-LAMP-01": price = 8.50m; return true;
                case "GEAR-ENGINE-01": price = 85.00m; return true;
                case "SUP-BAIT-WORM": price = 0.45m; return true;
                case "SUP-BAIT-FISH": price = 0.85m; return true;
                case "SUP-BAIT-SHELLFISH": price = 0.65m; return true;
                case "SUP-BAIT-LURE": price = 3.50m; return true;
                default: price = 0m; return false;
            }
        }

        private static decimal GetRelationshipDiscount(int score)
        {
            if (score >= 90) return 0.15m;
            if (score >= 75) return 0.10m;
            if (score >= 60) return 0.05m;
            return 0m;
        }

        private static long ApplyDiscount(long cents, decimal discount)
        {
            if (cents <= 0L) return 0L;
            decimal result = decimal.Round(cents * (1m - Math.Max(0m, Math.Min(0.20m, discount))),
                0, MidpointRounding.AwayFromZero);
            return result <= 0m ? 0L : (long)result;
        }

        private static long SafeAdd(long a, long b)
        {
            if (long.MaxValue - a < b) return long.MaxValue;
            return a + b;
        }

        private static bool IsValidTransactionId(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value == value.Trim() && value.Length <= 128;
        }
    }
}
