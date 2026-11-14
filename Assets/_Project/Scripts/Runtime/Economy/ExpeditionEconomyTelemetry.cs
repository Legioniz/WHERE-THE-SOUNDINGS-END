using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WhereTheSoundingsEnd.Interaction;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>
    /// Measures actual expedition transactions, game-clock duration and fuel consumption.
    /// A trip begins on propulsion and closes after the player finishes a dockside service session.
    /// This records evidence for balancing; it does not silently rebalance prices at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExpeditionEconomyTelemetry : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        private const string SaveFlagId = "economy.expedition-telemetry";
        private const string SamplesFlagId = "economy.expedition-balance-samples";
        private const int MaximumBalanceSamples = 50;

        [Serializable]
        private sealed class BalanceSample
        {
            public string departureRegionId;
            public string destinationRegionId;
            public long grossCents;
            public long expenseCents;
            public float fuelConsumed;
            public double durationSeconds;
            public int loadoutBand;
            public int catchCount;
            public int legalCatchCount;
            public int illegalCatchCount;
            public float catchWeightKg;
        }

        [SerializeField] private EconomyWalletController wallet;
        [SerializeField] private VesselFuelController fuel;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField] private DocksideHubController dockside;
        [SerializeField] private RegionalChartController regionalChart;
        [SerializeField] private VesselEquipmentController equipment;

        [SerializeField] private int completedTrips;
        [SerializeField] private long lifetimeGrossCents;
        [SerializeField] private long lifetimeExpenseCents;
        [SerializeField] private double lifetimeGameSeconds;
        [SerializeField] private float lifetimeFuelUsed;

        private EconomyWalletController subscribedWallet;
        private VesselFuelController subscribedFuel;
        private readonly List<BalanceSample> balanceSamples = new List<BalanceSample>();
        private bool tripActive;
        private bool docksideSessionObserved;
        private string tripStartRegionId = "greyhaven";
        private int tripCatchCount;
        private int tripLegalCatchCount;
        private int tripIllegalCatchCount;
        private float tripCatchWeightKg;
        private int tripLoadoutBand;
        private int lastTripLoadoutBand;
        private string lastTripStartRegionId = "greyhaven";
        private string lastTripDestinationRegionId = "greyhaven";
        private long tripGrossCents;
        private long tripExpenseCents;
        private float tripFuelConsumed;
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
        public long LifetimeNetCents => lifetimeGrossCents - lifetimeExpenseCents;
        public double LifetimeNetPerHourUsd => lifetimeGameSeconds > 0.01d
            ? (LifetimeNetCents / 100d) / (lifetimeGameSeconds / 3600d) : 0d;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            SubscribeWallet();
            SubscribeFuel();
        }

        private void OnDisable()
        {
            if (subscribedWallet != null)
                subscribedWallet.TransactionApplied -= OnTransactionApplied;
            if (subscribedFuel != null)
                subscribedFuel.FuelConsumed -= OnFuelConsumed;
            subscribedWallet = null;
            subscribedFuel = null;
        }

        private void Update()
        {
            ResolveReferences();
            SubscribeWallet();
            SubscribeFuel();
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
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (dockside == null) dockside = GetComponent<DocksideHubController>();
            if (dockside == null) dockside = FindFirstObjectByType<DocksideHubController>();
            if (regionalChart == null) regionalChart = GetComponent<RegionalChartController>();
            if (equipment == null) equipment = GetComponent<VesselEquipmentController>();
        }

        private void SubscribeWallet()
        {
            if (subscribedWallet == wallet) return;
            if (subscribedWallet != null)
                subscribedWallet.TransactionApplied -= OnTransactionApplied;
            subscribedWallet = wallet;
            if (subscribedWallet != null)
                subscribedWallet.TransactionApplied += OnTransactionApplied;
        }

        private void SubscribeFuel()
        {
            if (subscribedFuel == fuel) return;
            if (subscribedFuel != null)
                subscribedFuel.FuelConsumed -= OnFuelConsumed;
            subscribedFuel = fuel;
            if (subscribedFuel != null)
                subscribedFuel.FuelConsumed += OnFuelConsumed;
        }

        private void OnFuelConsumed(float units)
        {
            if (tripActive && IsFinite(units) && units > 0f)
                tripFuelConsumed = Mathf.Min(float.MaxValue, tripFuelConsumed + units);
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
            tripFuelConsumed = 0f;
            tripCatchCount = 0;
            tripLegalCatchCount = 0;
            tripIllegalCatchCount = 0;
            tripCatchWeightKg = 0f;
            tripStartRegionId = GetCurrentRegionId();
            tripLoadoutBand = GetLoadoutBand();
            tripStartGameSeconds = seaState != null ? seaState.ElapsedGameSeconds : 0d;
            tripStartRealtime = Time.unscaledTimeAsDouble;
        }

        public void RecordCatch(string itemId, float weightKg, bool legal)
        {
            if (!tripActive || string.IsNullOrWhiteSpace(itemId) || !IsFinite(weightKg) || weightKg <= 0f)
                return;
            if (tripCatchCount < int.MaxValue) tripCatchCount++;
            if (legal)
            {
                if (tripLegalCatchCount < int.MaxValue) tripLegalCatchCount++;
            }
            else if (tripIllegalCatchCount < int.MaxValue) tripIllegalCatchCount++;
            tripCatchWeightKg = Mathf.Min(float.MaxValue, tripCatchWeightKg + weightKg);
        }

        private string GetCurrentRegionId()
        {
            if (regionalChart == null) regionalChart = GetComponent<RegionalChartController>();
            return regionalChart != null && !string.IsNullOrWhiteSpace(regionalChart.CurrentRegionId)
                ? regionalChart.CurrentRegionId.Trim().ToLowerInvariant() : "greyhaven";
        }

        private void OnTransactionApplied(string transactionId, long deltaCents, long resultingBalance)
        {
            if (!tripActive || deltaCents == 0L) return;
            if (deltaCents < 0L && transactionId != null &&
                transactionId.StartsWith("dockside.inez.purchase.GEAR-", StringComparison.Ordinal))
                return; // Equipment is capital investment, not a recurring expedition operating cost.
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

            lastTripGrossCents = tripGrossCents;
            lastTripExpenseCents = tripExpenseCents;
            lastTripSeconds = duration;
            lastTripFuel = Mathf.Max(0f, tripFuelConsumed);
            lastTripStartRegionId = tripStartRegionId;
            lastTripDestinationRegionId = GetCurrentRegionId();
            lastTripLoadoutBand = tripLoadoutBand;
            BalanceSample sample = new BalanceSample
            {
                departureRegionId = lastTripStartRegionId,
                destinationRegionId = lastTripDestinationRegionId,
                grossCents = lastTripGrossCents,
                expenseCents = lastTripExpenseCents,
                fuelConsumed = lastTripFuel,
                durationSeconds = lastTripSeconds,
                loadoutBand = tripLoadoutBand,
                catchCount = tripCatchCount,
                legalCatchCount = tripLegalCatchCount,
                illegalCatchCount = tripIllegalCatchCount,
                catchWeightKg = tripCatchWeightKg
            };
            balanceSamples.Add(sample);
            if (balanceSamples.Count > MaximumBalanceSamples)
                balanceSamples.RemoveAt(0);
            completedTrips = completedTrips == int.MaxValue ? int.MaxValue : completedTrips + 1;
            lifetimeGrossCents = SafeAdd(lifetimeGrossCents, lastTripGrossCents);
            lifetimeExpenseCents = SafeAdd(lifetimeExpenseCents, lastTripExpenseCents);
            lifetimeGameSeconds = Math.Min(double.MaxValue, lifetimeGameSeconds + duration);
            lifetimeFuelUsed = Mathf.Min(float.MaxValue, lifetimeFuelUsed + lastTripFuel);
            tripActive = false;
            docksideSessionObserved = false;
            tripGrossCents = 0L;
            tripExpenseCents = 0L;
            tripCatchCount = 0;
            tripLegalCatchCount = 0;
            tripIllegalCatchCount = 0;
            tripCatchWeightKg = 0f;
        }

        public string BuildSummary()
        {
            if (tripActive)
                return "CURRENT EXPEDITION  /  GROSS $" + (tripGrossCents / 100m).ToString("0.00") +
                    "  /  COST $" + (tripExpenseCents / 100m).ToString("0.00") +
                    "  /  NET $" + ((tripGrossCents - tripExpenseCents) / 100m).ToString("0.00") +
                    "  /  FUEL " + tripFuelConsumed.ToString("0.0") +
                    "  /  LOADOUT " + GetLoadoutBandName(tripLoadoutBand) +
                    "  /  CATCHES " + tripCatchCount + " (LEGAL " + tripLegalCatchCount +
                    ", FLAGGED " + tripIllegalCatchCount + ")";
            if (completedTrips == 0)
                return "NO COMPLETED EXPEDITIONS RECORDED YET.";
            return "LAST EXPEDITION  /  GROSS $" + (lastTripGrossCents / 100m).ToString("0.00") +
                "  /  COST $" + (lastTripExpenseCents / 100m).ToString("0.00") +
                "  /  NET $" + (LastTripNetCents / 100m).ToString("0.00") +
                "  /  NET PER HOUR $" + LastTripNetPerHourUsd.ToString("0.00") +
                "  /  FUEL " + lastTripFuel.ToString("0.0") +
                "  /  LIFETIME NET $" + (LifetimeNetCents / 100m).ToString("0.00") +
                "  /  AVG NET PER HOUR $" + LifetimeNetPerHourUsd.ToString("0.00") +
                "  /  " + completedTrips + " TRIPS" +
                "  /  " + GetLoadoutBandName(lastTripLoadoutBand) +
                "  /  " + SeafoodMarketPolicy.GetMarketName(lastTripDestinationRegionId);
        }

        public string BuildBalanceReport()
        {
            if (balanceSamples.Count == 0)
                return "BALANCE DATA  /  No completed expedition samples yet.";

            Dictionary<string, List<BalanceSample>> grouped =
                new Dictionary<string, List<BalanceSample>>(StringComparer.Ordinal);
            for (int i = 0; i < balanceSamples.Count; i++)
            {
                BalanceSample sample = balanceSamples[i];
                if (sample == null) continue;
                List<BalanceSample> group;
                string groupKey = (sample.destinationRegionId ?? string.Empty) + "|" + sample.loadoutBand;
                if (!grouped.TryGetValue(groupKey, out group))
                {
                    group = new List<BalanceSample>();
                    grouped.Add(groupKey, group);
                }
                group.Add(sample);
            }

            List<string> lines = new List<string> { "MEASURED EXPEDITION BALANCE  /  " + balanceSamples.Count + " RECENT TRIPS" };
            foreach (KeyValuePair<string, List<BalanceSample>> pair in grouped)
            {
                decimal netTotal = 0m;
                double hours = 0d;
                float fuelTotal = 0f;
                int legal = 0;
                int catches = 0;
                List<double> tripNetPerHourSamples = new List<double>();
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    BalanceSample sample = pair.Value[i];
                    netTotal += (decimal)sample.grossCents - sample.expenseCents;
                    hours += sample.durationSeconds / 3600d;
                    fuelTotal += sample.fuelConsumed;
                    legal += sample.legalCatchCount;
                    catches += sample.catchCount;
                    double sampleHours = sample.durationSeconds / 3600d;
                    double sampleNetPerHour = sampleHours > 0.0001d
                        ? ((double)((decimal)sample.grossCents - sample.expenseCents) / 100d) / sampleHours
                        : 0d;
                    tripNetPerHourSamples.Add(sampleNetPerHour);
                }
                int count = pair.Value.Count;
                double netPerHour = hours > 0.0001d ? (double)(netTotal / 100m) / hours : 0d;
                double legalRate = catches > 0 ? (double)legal / catches * 100d : 0d;
                tripNetPerHourSamples.Sort();
                double medianNetPerHour = tripNetPerHourSamples.Count == 0 ? 0d :
                    (tripNetPerHourSamples.Count % 2 == 1
                        ? tripNetPerHourSamples[tripNetPerHourSamples.Count / 2]
                        : (tripNetPerHourSamples[tripNetPerHourSamples.Count / 2 - 1] +
                           tripNetPerHourSamples[tripNetPerHourSamples.Count / 2]) * 0.5d);
                double minNetPerHour = tripNetPerHourSamples.Count == 0 ? 0d : tripNetPerHourSamples[0];
                double maxNetPerHour = tripNetPerHourSamples.Count == 0 ? 0d :
                    tripNetPerHourSamples[tripNetPerHourSamples.Count - 1];
                string[] keyParts = pair.Key.Split('|');
                int loadoutBand;
                int.TryParse(keyParts.Length > 1 ? keyParts[1] : "0", out loadoutBand);
                lines.Add(SeafoodMarketPolicy.GetMarketName(keyParts[0]) + "  /  " +
                    GetLoadoutBandName(loadoutBand) + "  /  " + count +
                    " TRIPS  /  AVG NET $" + (netTotal / 100m / count).ToString("0.00") +
                    "  /  AVG NET-HR $" + netPerHour.ToString("0.00", CultureInfo.InvariantCulture) +
                    "  /  MEDIAN NET-HR $" + medianNetPerHour.ToString("0.00", CultureInfo.InvariantCulture) +
                    "  /  RANGE $" + minNetPerHour.ToString("0.00", CultureInfo.InvariantCulture) +
                    "–$" + maxNetPerHour.ToString("0.00", CultureInfo.InvariantCulture) +
                    "  /  AVG FUEL " + (fuelTotal / count).ToString("0.0", CultureInfo.InvariantCulture) +
                    "  /  LEGAL CATCH " + legalRate.ToString("0", CultureInfo.InvariantCulture) + "%");
            }
            return string.Join("\n", lines.ToArray());
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null) return;
            if (data.progress.worldFlags == null) data.progress.worldFlags = new List<WorldFlagSaveData>();
            data.progress.worldFlags.RemoveAll(flag => flag != null &&
                (flag.flagId == SaveFlagId || flag.flagId == SamplesFlagId));
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
                    lastTripFuel.ToString("R", CultureInfo.InvariantCulture),
                    tripActive ? "1" : "0",
                    tripGrossCents.ToString(CultureInfo.InvariantCulture),
                    tripExpenseCents.ToString(CultureInfo.InvariantCulture),
                    tripFuelConsumed.ToString("R", CultureInfo.InvariantCulture),
                    tripStartGameSeconds.ToString("R", CultureInfo.InvariantCulture),
                    tripStartRegionId ?? "greyhaven",
                    tripCatchCount.ToString(CultureInfo.InvariantCulture),
                    tripLegalCatchCount.ToString(CultureInfo.InvariantCulture),
                    tripIllegalCatchCount.ToString(CultureInfo.InvariantCulture),
                    tripCatchWeightKg.ToString("R", CultureInfo.InvariantCulture),
                    tripLoadoutBand.ToString(CultureInfo.InvariantCulture))
            });
            data.progress.worldFlags.Add(new WorldFlagSaveData
            {
                flagId = SamplesFlagId,
                value = true,
                stringValue = EncodeBalanceSamples()
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
                if (flag == null) continue;
                if (flag.flagId == SamplesFlagId)
                {
                    List<BalanceSample> decodedSamples;
                    if (!TryDecodeBalanceSamples(flag.stringValue, out decodedSamples) ||
                        decodedSamples.Count > MaximumBalanceSamples)
                    { error = "The save contains malformed expedition balance samples."; return false; }
                    continue;
                }
                if (flag.flagId != SaveFlagId) continue;
                string[] p = (flag.stringValue ?? string.Empty).Split('|');
                int trips;
                long gross, expenses, lastGross, lastExpenses;
                double seconds, lastSeconds;
                float fuelUsed, lastFuel;
                int active = 0;
                long activeGross = 0L;
                long activeExpenses = 0L;
                float activeFuel = 0f;
                double activeStart = 0d;
                if (!IsValidActiveRecord(p) ||
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
            balanceSamples.Clear();
            tripStartRegionId = "greyhaven";
            lastTripStartRegionId = "greyhaven";
            lastTripDestinationRegionId = "greyhaven";
            tripLoadoutBand = 0;
            lastTripLoadoutBand = 0;
            tripCatchCount = 0;
            tripLegalCatchCount = 0;
            tripIllegalCatchCount = 0;
            tripCatchWeightKg = 0f;
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
                if (flag == null) continue;
                if (flag.flagId == SamplesFlagId)
                {
                    List<BalanceSample> decodedSamples;
                    if (TryDecodeBalanceSamples(flag.stringValue, out decodedSamples))
                    {
                        balanceSamples.Clear();
                        balanceSamples.AddRange(decodedSamples);
                        if (balanceSamples.Count > 0)
                        {
                            BalanceSample latest = balanceSamples[balanceSamples.Count - 1];
                            lastTripStartRegionId = latest.departureRegionId;
                            lastTripDestinationRegionId = latest.destinationRegionId;
                            lastTripLoadoutBand = latest.loadoutBand;
                        }
                    }
                    continue;
                }
                if (flag.flagId != SaveFlagId) continue;
                string[] p = (flag.stringValue ?? string.Empty).Split('|');
                if (p.Length != 9 && p.Length != 14 && p.Length != 19 && p.Length != 20) return;
                int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out completedTrips);
                long.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out lifetimeGrossCents);
                long.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out lifetimeExpenseCents);
                double.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out lifetimeGameSeconds);
                float.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out lifetimeFuelUsed);
                long.TryParse(p[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out lastTripGrossCents);
                long.TryParse(p[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out lastTripExpenseCents);
                double.TryParse(p[7], NumberStyles.Float, CultureInfo.InvariantCulture, out lastTripSeconds);
                float.TryParse(p[8], NumberStyles.Float, CultureInfo.InvariantCulture, out lastTripFuel);
                if (p.Length == 14 || p.Length == 19 || p.Length == 20)
                {
                    int active;
                    int.TryParse(p[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out active);
                    tripActive = active == 1;
                    long.TryParse(p[10], NumberStyles.Integer, CultureInfo.InvariantCulture, out tripGrossCents);
                    long.TryParse(p[11], NumberStyles.Integer, CultureInfo.InvariantCulture, out tripExpenseCents);
                    float.TryParse(p[12], NumberStyles.Float, CultureInfo.InvariantCulture, out tripFuelConsumed);
                    double.TryParse(p[13], NumberStyles.Float, CultureInfo.InvariantCulture, out tripStartGameSeconds);
                    if (p.Length >= 19)
                    {
                        tripStartRegionId = p[14];
                        int.TryParse(p[15], NumberStyles.Integer, CultureInfo.InvariantCulture, out tripCatchCount);
                        int.TryParse(p[16], NumberStyles.Integer, CultureInfo.InvariantCulture, out tripLegalCatchCount);
                        int.TryParse(p[17], NumberStyles.Integer, CultureInfo.InvariantCulture, out tripIllegalCatchCount);
                        float.TryParse(p[18], NumberStyles.Float, CultureInfo.InvariantCulture, out tripCatchWeightKg);
                    }
                    else tripStartRegionId = "greyhaven";
                    if (p.Length == 20)
                        int.TryParse(p[19], NumberStyles.Integer, CultureInfo.InvariantCulture, out tripLoadoutBand);
                    tripStartRealtime = Time.unscaledTimeAsDouble;
                    docksideSessionObserved = false;
                }
                return;
            }
        }

        private string EncodeBalanceSamples()
        {
            // Compact bounded format keeps 50 samples safely below the save flag string limit.
            while (balanceSamples.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < balanceSamples.Count; i++)
                {
                    BalanceSample sample = balanceSamples[i];
                    if (sample == null) continue;
                    if (builder.Length > 0) builder.Append(';');
                    builder.Append(EncodeRegionCode(sample.departureRegionId)).Append(',')
                        .Append(EncodeRegionCode(sample.destinationRegionId)).Append(',')
                        .Append(sample.loadoutBand.ToString(CultureInfo.InvariantCulture)).Append(',')
                        .Append(sample.grossCents.ToString(CultureInfo.InvariantCulture)).Append(',')
                        .Append(sample.expenseCents.ToString(CultureInfo.InvariantCulture)).Append(',')
                        .Append(sample.fuelConsumed.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                        .Append(sample.durationSeconds.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                        .Append(sample.catchCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                        .Append(sample.legalCatchCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                        .Append(sample.illegalCatchCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                        .Append(sample.catchWeightKg.ToString("R", CultureInfo.InvariantCulture));
                }
                string encoded = builder.ToString();
                if (encoded.Length <= SaveGameData.MaximumStringLength)
                    return encoded;
                balanceSamples.RemoveAt(0);
            }
            return string.Empty;
        }

        private static bool TryDecodeBalanceSamples(string encoded, out List<BalanceSample> samples)
        {
            samples = new List<BalanceSample>();
            if (string.IsNullOrWhiteSpace(encoded)) return true;
            string[] records = encoded.Split(';');
            if (records.Length > MaximumBalanceSamples) return false;
            for (int i = 0; i < records.Length; i++)
            {
                string[] fields = records[i].Split(',');
                if (fields.Length != 11) return false;
                string departure = DecodeRegionCode(fields[0]);
                string destination = DecodeRegionCode(fields[1]);
                int band, catches, legal, illegal;
                long gross, expenses;
                float fuel, catchWeight;
                double duration;
                if (departure.Length == 0 || destination.Length == 0 ||
                    !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out band) ||
                    band < 0 || band > 2 ||
                    !long.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out gross) || gross < 0 ||
                    !long.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out expenses) || expenses < 0 ||
                    !float.TryParse(fields[5], NumberStyles.Float, CultureInfo.InvariantCulture, out fuel) || !IsFinite(fuel) || fuel < 0f ||
                    !double.TryParse(fields[6], NumberStyles.Float, CultureInfo.InvariantCulture, out duration) || !IsFinite(duration) || duration < 0d ||
                    !int.TryParse(fields[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out catches) || catches < 0 ||
                    !int.TryParse(fields[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out legal) || legal < 0 ||
                    !int.TryParse(fields[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out illegal) || illegal < 0 ||
                    legal + illegal > catches ||
                    !float.TryParse(fields[10], NumberStyles.Float, CultureInfo.InvariantCulture, out catchWeight) ||
                    !IsFinite(catchWeight) || catchWeight < 0f)
                    return false;
                samples.Add(new BalanceSample
                {
                    departureRegionId = departure,
                    destinationRegionId = destination,
                    loadoutBand = band,
                    grossCents = gross,
                    expenseCents = expenses,
                    fuelConsumed = fuel,
                    durationSeconds = duration,
                    catchCount = catches,
                    legalCatchCount = legal,
                    illegalCatchCount = illegal,
                    catchWeightKg = catchWeight
                });
            }
            return true;
        }

        private static string EncodeRegionCode(string regionId)
        {
            switch (regionId)
            {
                case "greyhaven": return "G";
                case "the_shallows": return "S";
                case "blackrock_reach": return "B";
                case "the_drowned_parish": return "D";
                case "the_outer_shoals": return "O";
                case "the_hollow_trench": return "H";
                default: return "U";
            }
        }

        private static string DecodeRegionCode(string code)
        {
            switch (code)
            {
                case "G": return "greyhaven";
                case "S": return "the_shallows";
                case "B": return "blackrock_reach";
                case "D": return "the_drowned_parish";
                case "O": return "the_outer_shoals";
                case "H": return "the_hollow_trench";
                case "U": return "unknown";
                default: return string.Empty;
            }
        }

        private int GetLoadoutBand()
        {
            if (equipment == null) equipment = GetComponent<VesselEquipmentController>();
            if (equipment == null) return 0;
            float combinedCapability = equipment.EnginePowerMultiplier * equipment.RodCastMultiplier;
            if (combinedCapability >= 1.15f) return 2; // Optimized: upgraded engine and rod in strong condition.
            if (combinedCapability >= 0.85f) return 1; // Average: serviceable mixed or upgraded loadout.
            return 0; // Starter: baseline or worn equipment.
        }

        private static string GetLoadoutBandName(int band)
        {
            switch (band)
            {
                case 2: return "OPTIMIZED";
                case 1: return "AVERAGE";
                default: return "STARTER";
            }
        }

        private static bool IsValidActiveRecord(string[] p)
        {
            if (p == null) return false;
            if (p.Length == 9) return true;
            if (p.Length != 14 && p.Length != 19 && p.Length != 20) return false;
            int active;
            long activeGross, activeExpenses;
            float activeFuel;
            double activeStart;
            if (!int.TryParse(p[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out active) ||
                (active != 0 && active != 1) ||
                !long.TryParse(p[10], NumberStyles.Integer, CultureInfo.InvariantCulture, out activeGross) || activeGross < 0 ||
                !long.TryParse(p[11], NumberStyles.Integer, CultureInfo.InvariantCulture, out activeExpenses) || activeExpenses < 0 ||
                !float.TryParse(p[12], NumberStyles.Float, CultureInfo.InvariantCulture, out activeFuel) || !IsFinite(activeFuel) || activeFuel < 0f ||
                !double.TryParse(p[13], NumberStyles.Float, CultureInfo.InvariantCulture, out activeStart) || !IsFinite(activeStart) || activeStart < 0d)
                return false;
            if (p.Length == 14) return true;
            int catches, legal, illegal;
            float catchWeight;
            if (string.IsNullOrWhiteSpace(p[14]) || p[14].Length > 128 ||
                !int.TryParse(p[15], NumberStyles.Integer, CultureInfo.InvariantCulture, out catches) || catches < 0 ||
                !int.TryParse(p[16], NumberStyles.Integer, CultureInfo.InvariantCulture, out legal) || legal < 0 ||
                !int.TryParse(p[17], NumberStyles.Integer, CultureInfo.InvariantCulture, out illegal) || illegal < 0 ||
                legal + illegal > catches ||
                !float.TryParse(p[18], NumberStyles.Float, CultureInfo.InvariantCulture, out catchWeight) ||
                !IsFinite(catchWeight) || catchWeight < 0f)
                return false;
            if (p.Length == 19) return true;
            int loadoutBand;
            return int.TryParse(p[19], NumberStyles.Integer, CultureInfo.InvariantCulture, out loadoutBand) &&
                   loadoutBand >= 0 && loadoutBand <= 2;
        }

        private static long SafeAdd(long a, long b) => long.MaxValue - a < b ? long.MaxValue : a + b;
        private static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static bool IsFinite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
    }
}
