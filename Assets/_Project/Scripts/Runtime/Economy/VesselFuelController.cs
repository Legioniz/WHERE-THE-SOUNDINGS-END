using System;
using UnityEngine;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>
    /// Vessel-owned fuel state. Fuel is consumed only while meaningful propulsion load is applied;
    /// idle, docked engine-off, paused and zero-throttle time do not drain the tank.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VesselFuelController : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        public const float DefaultCapacity = 100f;
        [SerializeField, Min(1f)] private float capacity = DefaultCapacity;
        [SerializeField, Min(0f)] private float fuelUnits = DefaultCapacity;
        [SerializeField, Min(0f)] private float emergencyFuelUnits = 2f;
        [SerializeField, Min(0f)] private float emergencyReserveCapacity = 2f;
        [SerializeField, Min(0f)] private float unitsPerSecondAtFullLoad = 0.0065f;
        private VesselEquipmentController equipment;

        public event Action<float> FuelConsumed;
        public float Capacity => capacity;
        public float FuelUnits => fuelUnits;
        public float FuelRatio => capacity > 0f ? Mathf.Clamp01(fuelUnits / capacity) : 0f;
        public float EmergencyFuelUnits => emergencyFuelUnits;
        public bool CanPropel => fuelUnits > 0.0001f || emergencyFuelUnits > 0.0001f;
        public float EstimatedFullLoadMinutes
        {
            get
            {
                if (unitsPerSecondAtFullLoad <= 0f) return float.PositiveInfinity;
                if (equipment == null) equipment = GetComponent<VesselEquipmentController>();
                float efficiency = equipment != null ? equipment.FuelUseMultiplier : 1.25f;
                return fuelUnits / (unitsPerSecondAtFullLoad * Mathf.Max(0.01f, efficiency)) / 60f;
            }
        }

        private void Awake()
        {
            equipment = GetComponent<VesselEquipmentController>();
        }

        private void OnValidate()
        {
            if (!IsFinite(capacity) || capacity < 1f) capacity = DefaultCapacity;
            if (!IsFinite(fuelUnits)) fuelUnits = capacity;
            fuelUnits = Mathf.Clamp(fuelUnits, 0f, capacity);
            if (!IsFinite(emergencyReserveCapacity) || emergencyReserveCapacity < 0f) emergencyReserveCapacity = 2f;
            if (!IsFinite(emergencyFuelUnits)) emergencyFuelUnits = emergencyReserveCapacity;
            emergencyFuelUnits = Mathf.Clamp(emergencyFuelUnits, 0f, emergencyReserveCapacity);
            if (!IsFinite(unitsPerSecondAtFullLoad) || unitsPerSecondAtFullLoad < 0f)
                unitsPerSecondAtFullLoad = 0.0065f;
        }

        public float ConsumePropulsion(float throttle, float engineLoad, float deltaTime)
        {
            if (!IsFinite(throttle) || !IsFinite(engineLoad) || !IsFinite(deltaTime) ||
                deltaTime <= 0f || !CanPropel || Mathf.Abs(throttle) < 0.05f)
                return 0f;

            float load = Mathf.Clamp01(engineLoad);
            if (load <= 0.01f) return 0f;
            if (equipment == null) equipment = GetComponent<VesselEquipmentController>();
            float efficiency = equipment != null ? equipment.FuelUseMultiplier : 1.25f;
            float requested = unitsPerSecondAtFullLoad * efficiency * load * Mathf.Clamp01(Mathf.Abs(throttle)) * deltaTime;
            if (equipment != null) equipment.WearEngine(deltaTime * load * Mathf.Clamp01(Mathf.Abs(throttle)));
            if (fuelUnits > 0f)
            {
                float consumed = Mathf.Min(fuelUnits, requested);
                fuelUnits = Mathf.Max(0f, fuelUnits - consumed);
                NotifyFuelConsumed(consumed);
                return consumed;
            }

            // A small isolated reserve supports a low-power return after the main tank empties.
            float emergencyRequested = requested * 0.25f;
            float emergencyConsumed = Mathf.Min(emergencyFuelUnits, emergencyRequested);
            emergencyFuelUnits = Mathf.Max(0f, emergencyFuelUnits - emergencyConsumed);
            NotifyFuelConsumed(emergencyConsumed);
            return emergencyConsumed;
        }

        private void NotifyFuelConsumed(float amount)
        {
            if (amount <= 0f) return;
            Action<float> handler = FuelConsumed;
            if (handler == null) return;
            try { handler(amount); }
            catch (Exception exception) { Debug.LogException(exception, this); }
        }

        public float GetRefuelAmount(float requestedUnits)
        {
            if (!IsFinite(requestedUnits) || requestedUnits <= 0f) return 0f;
            return Mathf.Min(requestedUnits, Mathf.Max(0f, capacity - fuelUnits));
        }

        public void AddFuel(float units)
        {
            if (!IsFinite(units) || units <= 0f) return;
            fuelUnits = Mathf.Clamp(fuelUnits + units, 0f, capacity);
            emergencyFuelUnits = emergencyReserveCapacity;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.vessel == null) throw new ArgumentNullException(nameof(data));
            data.vessel.fuel = fuelUnits;
            data.vessel.emergencyFuel = emergencyFuelUnits;
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.vessel == null || !IsFinite(data.vessel.fuel) ||
                data.vessel.fuel < 0f || data.vessel.fuel > capacity + 0.001f ||
                !IsFinite(data.vessel.emergencyFuel) || data.vessel.emergencyFuel < 0f ||
                data.vessel.emergencyFuel > emergencyReserveCapacity + 0.001f)
            { error = "The save contains invalid vessel fuel."; return false; }
            return true;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.vessel == null) return;
            string error;
            if (!TryValidateSaveData(data, out error)) throw new InvalidOperationException(error);
            fuelUnits = Mathf.Clamp(data.vessel.fuel, 0f, capacity);
            emergencyFuelUnits = Mathf.Clamp(data.vessel.emergencyFuel, 0f, emergencyReserveCapacity);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
