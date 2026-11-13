using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>
    /// Owns the three physical vessel equipment slots. Equipped items leave the spatial hold,
    /// persist in InventorySaveData.equipment, and provide bounded gameplay modifiers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VesselEquipmentController : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        public const string EngineSlot = "engine";
        public const string RodSlot = "fishing-rod";
        public const string LanternSlot = "lantern";

        [Serializable]
        private sealed class SlotState
        {
            public string slotId;
            public string itemId;
            [Range(0f, 1f)] public float condition = 1f;
        }

        [SerializeField] private SpatialCargoHold cargo;
        [SerializeField] private List<SlotState> slots = new List<SlotState>();
        [SerializeField, Min(0f)] private float engineWearPerSecond = 0.000025f;
        [SerializeField, Min(0f)] private float rodWearPerCatch = 0.0025f;
        [SerializeField, Min(0f)] private float lanternWearPerNightSecond = 0.000015f;
        [SerializeField] private Light lanternLight;
        private SeaStateDirector seaState;

        public string EngineItemId => GetItemId(EngineSlot);
        public string RodItemId => GetItemId(RodSlot);
        public string LanternItemId => GetItemId(LanternSlot);
        public float EngineCondition => GetCondition(EngineSlot);
        public float RodCondition => GetCondition(RodSlot);
        public bool HasEngine => !string.IsNullOrEmpty(EngineItemId);
        public bool HasRod => !string.IsNullOrEmpty(RodItemId);
        public bool HasLantern => !string.IsNullOrEmpty(LanternItemId);
        public float FuelUseMultiplier => HasEngine
            ? Mathf.Lerp(1.20f, 0.82f, EngineCondition) : 1.25f;
        public float EnginePowerMultiplier => HasEngine
            ? Mathf.Lerp(0.78f, 1.12f, EngineCondition) : 0.72f;
        public float RodCastMultiplier => HasRod
            ? Mathf.Lerp(0.72f, 1.18f, RodCondition) : 0.72f;
        public float RodControlMultiplier => HasRod
            ? Mathf.Lerp(0.70f, 1.12f, RodCondition) : 0.70f;
        public float LanternCondition => GetCondition(LanternSlot);

        private void Awake()
        {
            ResolveReferences();
            EnsureSlots();
            seaState = FindFirstObjectByType<SeaStateDirector>();
            ApplyLantern();
        }

        private void Update()
        {
            if (HasLantern && LanternCondition > 0.01f)
            {
                if (seaState != null && seaState.IsNight)
                    DamageSlot(LanternSlot, lanternWearPerNightSecond * Time.deltaTime);
            }
            ApplyLantern();
        }

        private void ResolveReferences()
        {
            if (cargo == null) cargo = GetComponent<SpatialCargoHold>();
        }

        private void EnsureSlots()
        {
            if (slots == null) slots = new List<SlotState>();
            EnsureSlot(EngineSlot);
            EnsureSlot(RodSlot);
            EnsureSlot(LanternSlot);
        }

        private void EnsureSlot(string id)
        {
            if (FindSlot(id) == null) slots.Add(new SlotState { slotId = id, itemId = string.Empty, condition = 1f });
        }

        private SlotState FindSlot(string id)
        {
            if (slots == null) return null;
            for (int i = 0; i < slots.Count; i++)
                if (slots[i] != null && slots[i].slotId == id) return slots[i];
            return null;
        }

        private string GetItemId(string slotId)
        {
            SlotState slot = FindSlot(slotId);
            return slot != null ? slot.itemId : string.Empty;
        }

        private float GetCondition(string slotId)
        {
            SlotState slot = FindSlot(slotId);
            return slot != null && !string.IsNullOrEmpty(slot.itemId) ? Mathf.Clamp01(slot.condition) : 0f;
        }

        public static string GetSlotForItem(string itemId)
        {
            switch (itemId)
            {
                case "GEAR-ENGINE-01": return EngineSlot;
                case "GEAR-ROD-01": return RodSlot;
                case "GEAR-LAMP-01": return LanternSlot;
                default: return string.Empty;
            }
        }

        public bool TryEquip(string itemId, out string message)
        {
            ResolveReferences();
            EnsureSlots();
            string slotId = GetSlotForItem(itemId);
            if (string.IsNullOrEmpty(slotId) || cargo == null)
            {
                message = "That item does not fit a supported vessel equipment slot.";
                return false;
            }

            CargoPlacement incoming = null;
            IReadOnlyList<CargoPlacement> placements = cargo.Placements;
            for (int i = 0; i < placements.Count; i++)
                if (placements[i] != null && placements[i].itemId == itemId && placements[i].quantity == 1)
                { incoming = placements[i]; break; }
            if (incoming == null)
            {
                message = "The equipment must be aboard the vessel before it can be installed.";
                return false;
            }

            SlotState slot = FindSlot(slotId);
            if (slot.itemId == itemId)
            {
                message = "That equipment is already installed.";
                return false;
            }

            string oldItem = slot.itemId;
            float oldCondition = slot.condition;
            CargoPlacement removed;
            if (!cargo.TryRemove(incoming.instanceId, out removed))
            {
                message = "The cargo placement changed before installation. Nothing was equipped.";
                return false;
            }

            if (!string.IsNullOrEmpty(oldItem))
            {
                string returnedInstance;
                if (!cargo.TryPlaceFirstFit(oldItem, 1, oldCondition, 1f, out returnedInstance))
                {
                    string rollbackInstance;
                    if (!cargo.TryPlaceFirstFit(itemId, 1, removed.quality, removed.freshness,
                        out rollbackInstance, removed.saleQuantity))
                        Debug.LogError("Equipment swap rollback failed; inspect vessel cargo immediately.", this);
                    message = "The hold cannot store the replaced equipment. The installation was cancelled.";
                    return false;
                }
            }

            slot.itemId = itemId;
            slot.condition = Mathf.Clamp01(removed.quality);
            ApplyLantern();
            message = "INSTALLED  /  " + itemId + "  /  " + slotId.ToUpperInvariant();
            return true;
        }

        public bool TryUnequip(string slotId, out string message)
        {
            ResolveReferences();
            SlotState slot = FindSlot(slotId);
            if (slot == null || string.IsNullOrEmpty(slot.itemId) || cargo == null)
            {
                message = "That equipment slot is already empty.";
                return false;
            }

            string instanceId;
            if (!cargo.TryPlaceFirstFit(slot.itemId, 1, slot.condition, 1f, out instanceId))
            {
                message = "The hold has no free space for the installed equipment.";
                return false;
            }
            slot.itemId = string.Empty;
            slot.condition = 1f;
            ApplyLantern();
            message = "EQUIPMENT STORED  /  " + slotId.ToUpperInvariant();
            return true;
        }

        public void WearEngine(float deltaTime)
        {
            if (!HasEngine || !IsFinite(deltaTime) || deltaTime <= 0f) return;
            DamageSlot(EngineSlot, engineWearPerSecond * deltaTime);
        }

        public void WearRodForCatch()
        {
            if (HasRod) DamageSlot(RodSlot, rodWearPerCatch);
        }

        public void DamageSlot(string slotId, float amount)
        {
            SlotState slot = FindSlot(slotId);
            if (slot == null || string.IsNullOrEmpty(slot.itemId) || !IsFinite(amount) || amount <= 0f) return;
            slot.condition = Mathf.Clamp01(slot.condition - amount);
            ApplyLantern();
        }

        public long GetRepairQuoteCents(string slotId)
        {
            SlotState slot = FindSlot(slotId);
            if (slot == null || string.IsNullOrEmpty(slot.itemId) || slot.condition >= 0.9999f) return 0L;
            decimal missing = (decimal)(1f - slot.condition);
            decimal rate = slotId == EngineSlot ? 1800m : slotId == RodSlot ? 650m : 500m;
            decimal quote = decimal.Round(missing * rate, 0, MidpointRounding.AwayFromZero);
            return quote < 100m ? 100L : (long)quote;
        }

        public bool TryRepairSlot(string slotId, EconomyWalletController wallet, string transactionId,
            out long chargedCents, out string error)
        {
            chargedCents = 0L;
            long quote = GetRepairQuoteCents(slotId);
            if (wallet == null || quote <= 0L || string.IsNullOrWhiteSpace(transactionId) ||
                transactionId.Length > 128 || transactionId != transactionId.Trim() ||
                wallet.HasProcessedTransaction(transactionId))
            {
                error = "The equipment repair quote or transaction is invalid.";
                return false;
            }
            SlotState slot = FindSlot(slotId);
            if (slot == null || !wallet.TryApplyTransaction(transactionId, -quote))
            {
                error = "Insufficient funds. The equipment remains unchanged.";
                return false;
            }
            slot.condition = 1f;
            chargedCents = quote;
            ApplyLantern();
            error = null;
            return true;
        }

        private void ResolveLantern()
        {
            if (lanternLight != null) return;
            Transform existing = transform.Find("Installed Deck Lantern");
            if (existing != null) lanternLight = existing.GetComponent<Light>();
            if (lanternLight != null) return;

            GameObject lightObject = new GameObject("Installed Deck Lantern");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.6f, 0.8f);
            lanternLight = lightObject.AddComponent<Light>();
            lanternLight.type = LightType.Point;
            lanternLight.range = 18f;
            lanternLight.intensity = 1.8f;
            lanternLight.color = new Color(1f, 0.72f, 0.43f);
            lanternLight.shadows = LightShadows.None;
        }

        private void ApplyLantern()
        {
            if (lanternLight == null && HasLantern) ResolveLantern();
            if (lanternLight == null) return;
            bool equipped = HasLantern && LanternCondition > 0.01f;
            lanternLight.enabled = equipped;
            if (equipped)
            {
                lanternLight.intensity = Mathf.Lerp(0.35f, 1.8f, LanternCondition);
                lanternLight.range = Mathf.Lerp(6f, 18f, LanternCondition);
            }
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.inventory == null) throw new ArgumentNullException(nameof(data));
            if (data.inventory.equipment == null) data.inventory.equipment = new List<EquipmentSaveData>();
            data.inventory.equipment.Clear();
            EnsureSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                SlotState slot = slots[i];
                if (slot == null || string.IsNullOrEmpty(slot.itemId)) continue;
                data.inventory.equipment.Add(new EquipmentSaveData
                {
                    slotId = slot.slotId, itemId = slot.itemId, condition = slot.condition
                });
            }
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.inventory == null || data.inventory.equipment == null ||
                data.inventory.equipment.Count > 3)
            {
                error = "The save contains an invalid vessel equipment loadout.";
                return false;
            }
            HashSet<string> slotsSeen = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> itemsSeen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < data.inventory.equipment.Count; i++)
            {
                EquipmentSaveData saved = data.inventory.equipment[i];
                string expected = saved != null ? GetSlotForItem(saved.itemId) : string.Empty;
                if (saved == null || expected.Length == 0 || saved.slotId != expected ||
                    !slotsSeen.Add(saved.slotId) || !itemsSeen.Add(saved.itemId) ||
                    !IsFinite(saved.condition) || saved.condition < 0f || saved.condition > 1f)
                {
                    error = "The save contains an unknown, duplicate or malformed equipment slot.";
                    return false;
                }
            }
            return true;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.inventory == null || data.inventory.equipment == null) return;
            EnsureSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].itemId = string.Empty;
                slots[i].condition = 1f;
            }
            string error;
            if (!TryValidateSaveData(data, out error)) throw new InvalidOperationException(error);
            for (int i = 0; i < data.inventory.equipment.Count; i++)
            {
                EquipmentSaveData saved = data.inventory.equipment[i];
                SlotState slot = FindSlot(saved.slotId);
                slot.itemId = saved.itemId;
                slot.condition = Mathf.Clamp01(saved.condition);
            }
            ApplyLantern();
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
