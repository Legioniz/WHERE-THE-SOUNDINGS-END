using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Boat;
using WhereTheSoundingsEnd.Interaction;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>
    /// Measures actual expedition transactions, game-clock duration and fuel consumption.
    /// A trip begins on propulsion and closes when the player opens the dockside service hub.
    /// This records evidence for balancing; it does not silently rebalance prices at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExpeditionEconomyTelemetry : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        private const string SaveFlagId = "economy.expedition-telemetry";

        [SerializeField] private EconomyWalletController wallet;
        [SerializeField] private VesselFuelController fuel;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField] private DocksideHubController dockside;
        [SerializeField] private BoatPrototypeController boat;

        [SerializeField] private int completedTrips;
        [SerializeField] private long lifetimeGrossCents;
        [SerializeField] private long lifetimeExpenseCents;
        [SerializeField] private double lifetimeGameSeconds;
        [SerializeField] private float lifetimeFuelUsed;

        private bool tripActive;
        private bool docksideSessionObserved;
        private long tripGrossCents;
        private long tripExpenseCents;
        private float tripStartFuel;
        private double tripStartGameSeconds;
        private double tripStartRealtime;
        private long lastTripGrossCents;
        private long lastTripExpenseCents;
        private double lastTripSeconds;
        private float lastTripFuel;

        public bool TripActive => tripActive;
        public int CompletedTrips => completedTrips;
        public long LastTripNetCents => lastTripGrossCents - lastTripExpenseCents;
        public double LastTripNetPerHourUsd => lastTripSeconds > 0.01d
            ? (LastTripNetCents / 100d) / (lastTripSeconds / 3600d) : 0d;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            SubscribeWallet();
        }

        private void OnDisable()
        {
            if (wallet != null) wallet.TransactionApplied -= OnTransactionApplied;
        }

        private void Update()
        {
            ResolveReferences();
            SubscribeWallet();
            if (tripActive && dockside != null)
            {
                if (dockside.IsMenuOpen) docksideSessionObserved = true;
                else if (docksideSessionObserved)
                {
                    docksideSessionObserved = false;
                    CompleteTrip();
                }
            }
        }

        private void ResolveReferences()
        {
            if (wallet == null) wallet = GetComponent<EconomyWalletController>();
            if (fuel == null) fuel = GetComponent<VesselFuelController>();
            if (boat == null) boat = GetComponent<BoatPrototypeController>();
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (dockside == null) dockside = GetComponent<DocksideHubController>();
            if (dockside == null) dockside = FindFirstObjectByType<DocksideHubController>();
        }

        private void SubscribeWallet()
        {
            if (wallet == null) return;
            wallet.TransactionApplied -= OnTransactionApplied;
            wallet.TransactionApplied += OnTransactionApplied;
        }

        public void ObservePropulsion(float throttle)
        {
            if (tripActive || Time.timeScale <= 0.0001f || !IsFinite(throttle) || Mathf.Abs(throttle) < 0.05f)
                return;
            ResolveReferences();
            if (wallet == null || fuel == null) return;
            tripActive = true;
            tripGrossCents = 0L;
            tripExpenseCents = 0L;
            tripStartFuel = fuel.FuelUnits;
            tripStartGameSeconds = seaState != null ? seaState.ElapsedGameSeconds : 0d;
            tripStartRealtime = Time.unscaledTimeAsDouble;
        }

        private void OnTransactionApplied(string transactionId, long deltaCents, long resultingBalance)
        {
            if (!tripActive || deltaCents == 0L) return;
            if (deltaCents > 0L)
                tripGrossCents = SafeAdd(tripGrossCents, deltaCents);
            else if (deltaCents < 0L && deltaCents != long.MinValue)
                tripExpenseCents = SafeAdd(tripExpenseCents, -deltaCents);
        }

        public void CompleteTrip()
        {
            if (!tripActive) return;
            double now = seaState != null ? seaState.ElapsedGameSeconds : tripStartGameSeconds;
            double duration = Math.Max(0d, now - tripStartGameSeconds);
            if (duration < 0.01d) duration = Math.Max(0d, Time.unscaledTimeAsDouble - tripStartRealtime);
            float fuelNow = fuel != null ? fuel.FuelUnits : tripStartFuel;

            lastTripGrossCents = tripGrossCents;
            lastTripExpenseCents = tripExpenseCents;
            lastTripSeconds = duration;
            lastTripFuel = Mathf.Max(0f, tripStartFuel - fuelNow);
            completedTrips = completedTrips == int.MaxValue ? int.MaxValue : completedTrips + 1;
            lifetimeGrossCents = SafeAdd(lifetimeGrossCents, lastTripGrossCents);
            lifetimeExpenseCents = SafeAdd(lifetimeExpenseCents, lastTripExpenseCents);
            lifetimeGameSeconds = Math.Min(double.MaxValue, lifetimeGameSeconds + duration);
            lifetimeFuelUsed = Mathf.Min(float.MaxValue, lifetimeFuelUsed + lastTripFuel);
            tripActive = false;
            docksideSessionObserved = false;
            tripGrossCents = 0L;
            tripExpenseCents = 0L;
        }

        public string BuildSummary()
        {
            if (completedTrips == 0)
                return tripActive ? "EXPEDITION IN PROGRESS  /  GROSS $" +
                    (tripGrossCents / 100m).ToString("0.00") + "  /  COST $" +
                    (tripExpenseCents / 100m).ToString("0.00") : "NO COMPLETED EXPEDITIONS RECORDED YET.";
            return "LAST EXPEDITION  /  GROSS $" + (lastTripGrossCents / 100m).ToString("0.00") +
                "  /  COST $" + (lastTripExpenseCents / 100m).ToString("0.00") +
                "  /  NET $" + (LastTripNetCents / 100m).ToString("0.00") +
                "  /  NET PER HOUR $" + LastTripNetPerHourUsd.ToString("0.00") +
                "  /  FUEL " + lastTripFuel.ToString("0.0") +
                "  /  " + completedTrips + " TRIPS RECORDED";
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null) return;
            if (data.progress.worldFlags == null) data.progress.worldFlags = new List<WorldFlagSaveData>();
            data.progress.worldFlags.RemoveAll(flag => flag != null && flag.flagId == SaveFlagId);
            data.progress.worldFlags.Add(new WorldFlagSaveData
            {
                flagId = SaveFlagId,
                value = true,
                stringValue = string.Join("|",
                    completedTrips.ToString(CultureInfo.InvariantCulture),
                    lifetimeGrossCents.ToString(CultureInfo.InvariantCulture),
                    lifetimeExpenseCents.ToString(CultureInfo.InvariantCulture),
                    lifetimeGameSeconds.ToString("R", CultureInfo.InvariantCulture),
                    lifetimeFuelUsed.ToString("R", CultureInfo.InvariantCulture),
                    lastTripGrossCents.ToString(CultureInfo.InvariantCulture),
                    lastTripExpenseCents.ToString(CultureInfo.InvariantCulture),
                    lastTripSeconds.ToString("R", CultureInfo.InvariantCulture),
                    lastTripFuel.ToString("R", CultureInfo.InvariantCulture))
            });
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.progress == null || data.progress.worldFlags == null)
            {
                error = "The save does not contain expedition economy telemetry.";
                return false;
            }
            for (int i = 0; i < data.progress.worldFlags.Count; i++)
            {
                WorldFlagSaveData flag = data.progress.worldFlags[i];
                if (flag == null || flag.flagId != SaveFlagId) continue;
                string[] p = (flag.stringValue ?? string.Empty).Split('|');
                int trips;
                long gross, expenses, lastGross, lastExpenses;
                double seconds, lastSeconds;
                float fuelUsed, lastFuel;
                if (p.Length != 9 ||
                    !int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out trips) || trips < 0 ||
                    !long.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out gross) || gross < 0 ||
                    !long.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out expenses) || expenses < 0 ||
                    !double.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out seconds) || !IsFinite(seconds) || seconds < 0d ||
                    !float.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out fuelUsed) || !IsFinite(fuelUsed) || fuelUsed < 0f ||
                    !long.TryParse(p[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out lastGross) || lastGross < 0 ||
                    !long.TryParse(p[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out lastExpenses) || lastExpenses < 0 ||
                    !double.TryParse(p[7], NumberStyles.Float, CultureInfo.InvariantCulture, out lastSeconds) || !IsFinite(lastSeconds) || lastSeconds < 0d ||
                    !float.TryParse(p[8], NumberStyles.Float, CultureInfo.InvariantCulture, out lastFuel) || !IsFinite(lastFuel) || lastFuel < 0f)
                {
                    error = "The save contains malformed expedition economy telemetry.";
                    return false;
                }
            }
            return true;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            completedTrips = 0;
            lifetimeGrossCents = 0L;
            lifetimeExpenseCents = 0L;
            lifetimeGameSeconds = 0d;
            lifetimeFuelUsed = 0f;
            lastTripGrossCents = 0L;
            lastTripExpenseCents = 0L;
            lastTripSeconds = 0d;
            lastTripFuel = 0f;
            tripActive = false;
            docksideSessionObserved = false;
            if (data == null || data.progress == null || data.progress.worldFlags == null) return;
            for (int i = 0; i < data.progress.worldFlags.Count; i++)
            {
                WorldFlagSaveData flag = data.progress.worldFlags[i];
                if (flag == null || flag.flagId != SaveFlagId) continue;
                string[] p = (flag.stringValue ?? string.Empty).Split('|');
                if (p.Length != 9) return;
                int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out completedTrips);
                long.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out lifetimeGrossCents);
                long.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out lifetimeExpenseCents);
                double.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out lifetimeGameSeconds);
                float.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out lifetimeFuelUsed);
                long.TryParse(p[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out lastTripGrossCents);
                long.TryParse(p[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out lastTripExpenseCents);
                double.TryParse(p[7], NumberStyles.Float, CultureInfo.InvariantCulture, out lastTripSeconds);
                float.TryParse(p[8], NumberStyles.Float, CultureInfo.InvariantCulture, out lastTripFuel);
                return;
            }
        }

        private static long SafeAdd(long a, long b) => long.MaxValue - a < b ? long.MaxValue : a + b;
        private static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static bool IsFinite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
    }
}
