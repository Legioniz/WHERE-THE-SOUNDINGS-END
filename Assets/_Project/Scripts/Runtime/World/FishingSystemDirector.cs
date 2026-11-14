using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    public enum FishingTechnique
    {
        StillFishing,
        BottomFishing,
        FloatFishing,
        Jigging,
        Trolling,
        Drifting,
        Handlining,
        SeaweedHarvesting
    }

    public enum FishingEncounterPhase
    {
        Idle,
        WaitingForBite,
        HookWindow,
        Fighting
    }

    /// <summary>
    /// Owns the fishing encounter lifecycle, equipment loadout, catch resolution and
    /// aggregate regional fish stocks. Dredging remains owned by HarvestingMinigameController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FishingSystemDirector : MonoBehaviour, ISaveParticipant
    {
        [Serializable]
        private sealed class GearChoice
        {
            public string id;
            public string label;
            public float castMultiplier;
            public float tensionLimit;
            public float control;
            public float retrieveSpeed = 1f;
            public int unlockAtCatches;
        }

        private static readonly GearChoice[] Rods =
        {
            new GearChoice { id = "shore-rod", label = "Shore rod", castMultiplier = 1f, tensionLimit = 0.85f, control = 0.9f, unlockAtCatches = 0 },
            new GearChoice { id = "carbon-rod", label = "Carbon rod", castMultiplier = 1.25f, tensionLimit = 1.05f, control = 1.05f, unlockAtCatches = 5 },
            new GearChoice { id = "deepwater-rod", label = "Deepwater rod", castMultiplier = 1.1f, tensionLimit = 1.35f, control = 0.85f, unlockAtCatches = 12 }
        };

        private static readonly GearChoice[] Lines =
        {
            new GearChoice { id = "monofilament", label = "Monofilament", castMultiplier = 1f, tensionLimit = 0.9f, control = 1f, unlockAtCatches = 0 },
            new GearChoice { id = "braided-line", label = "Braided line", castMultiplier = 1.05f, tensionLimit = 1.2f, control = 0.95f, unlockAtCatches = 5 },
            new GearChoice { id = "heavy-leader", label = "Heavy leader", castMultiplier = 0.9f, tensionLimit = 1.45f, control = 0.8f, unlockAtCatches = 12 }
        };

        private static readonly GearChoice[] Hooks =
        {
            new GearChoice { id = "standard-hook", label = "Standard hook", castMultiplier = 1f, tensionLimit = 1f, control = 1f, unlockAtCatches = 0 },
            new GearChoice { id = "circle-hook", label = "Circle hook", castMultiplier = 0.95f, tensionLimit = 1.05f, control = 1.15f, unlockAtCatches = 5 },
            new GearChoice { id = "heavy-hook", label = "Heavy hook", castMultiplier = 0.9f, tensionLimit = 1.2f, control = 0.9f, unlockAtCatches = 12 }
        };

        private static readonly GearChoice[] Reels =
        {
            new GearChoice { id = "standard-reel", label = "Standard reel", tensionLimit = 1f, control = 0.95f, retrieveSpeed = 1f, unlockAtCatches = 0 },
            new GearChoice { id = "sealed-reel", label = "Sealed reel", tensionLimit = 1.2f, control = 1.05f, retrieveSpeed = 1.15f, unlockAtCatches = 5 },
            new GearChoice { id = "deep-drag-reel", label = "Deep-drag reel", tensionLimit = 1.45f, control = 0.9f, retrieveSpeed = 1.25f, unlockAtCatches = 12 }
        };

        private static readonly string[] Baits = { "worm", "small-fish", "shellfish", "lure" };
        private static readonly string[] BaitItemIds = { "SUP-BAIT-WORM", "SUP-BAIT-FISH", "SUP-BAIT-SHELLFISH", "SUP-BAIT-LURE" };
        private static readonly FishHabitat[] HabitatOrder =
        {
            FishHabitat.Estuary, FishHabitat.Nearshore, FishHabitat.Kelp,
            FishHabitat.OpenWater, FishHabitat.DeepWater
        };
        private static readonly string[] BaitLabels = { "Worm", "Small fish", "Shellfish", "Lure" };
        private static readonly FishingTechnique[] Techniques = (FishingTechnique[])Enum.GetValues(typeof(FishingTechnique));

        [SerializeField] private InputActionAsset controls;
        [SerializeField] private SpatialCargoHold cargoHold;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField, Min(1f)] private float baseCastDistance = 24f;
        [SerializeField, Min(0.2f)] private float biteWaitMinimum = 3.5f;
        [SerializeField, Min(0.2f)] private float biteWaitMaximum = 8f;
        [SerializeField, Min(0.2f)] private float fightBeatDuration = 1.35f;
        [SerializeField, Range(0.02f, 0.4f)] private float strikeWindow = 0.12f;

        private InputAction harvestBeatAction;
        private VesselEquipmentController equipment;
        private RegionalChartController regionalChart;
        private EconomyWalletController wallet;
        private ExpeditionEconomyTelemetry economyTelemetry;
        private FishingEncounterPhase phase;
        private FishingTechnique technique = FishingTechnique.StillFishing;
        private int rodIndex;
        private int lineIndex;
        private int hookIndex;
        private int reelIndex;
        private int baitIndex;
        private int totalCatches;
        private float waitTimer;
        private float phaseTimer;
        private float targetPhase;
        private float tension;
        private float fatigue;
        private float fightProgress;
        private int fightBeats;
        private int successfulBeats;
        private int missedBites;
        private float currentHookWindow;
        private float currentFightWindow;
        private float selectedCastDistance;
        private float estimatedDepth;
        private double lastMigrationGameSeconds;
        private FishHabitat activeHabitat;
        private CargoItemDefinition hookedFish;
        private string status = "F: prepare and cast";
        private readonly Dictionary<string, int> populations = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, double> populationTimes = new Dictionary<string, double>(StringComparer.Ordinal);

        public FishingEncounterPhase Phase { get { return phase; } }
        public FishingTechnique Technique { get { return technique; } }
        public string Status { get { return status; } }
        public float FightProgress { get { return fightProgress; } }
        public float LineTension { get { return tension; } }

        public void Configure(InputActionAsset input, SpatialCargoHold hold, SeaStateDirector director)
        {
            controls = input;
            cargoHold = hold;
            seaState = director;
            harvestBeatAction = controls != null ? controls.FindAction("HarvestBeat", false) : null;
        }

        private void Awake()
        {
            if (cargoHold == null) cargoHold = GetComponent<SpatialCargoHold>();
            equipment = GetComponent<VesselEquipmentController>();
            regionalChart = GetComponent<RegionalChartController>();
            wallet = GetComponent<EconomyWalletController>();
            economyTelemetry = GetComponent<ExpeditionEconomyTelemetry>();
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (harvestBeatAction == null && controls != null)
                harvestBeatAction = controls.FindAction("HarvestBeat", false);
        }

        public bool TryStartFishing()
        {
            if (phase != FishingEncounterPhase.Idle || cargoHold == null ||
                Time.timeScale <= 0.0001f || (seaState != null && seaState.PanicLevel > 0.98f))
                return false;

            Rigidbody vessel = GetComponent<Rigidbody>();
            float speed = vessel != null ? new Vector2(vessel.linearVelocity.x, vessel.linearVelocity.z).magnitude : 0f;
            if ((technique == FishingTechnique.Trolling && speed < 1.5f) ||
                (technique != FishingTechnique.Trolling && technique != FishingTechnique.Drifting && speed > 1.5f))
            {
                status = technique == FishingTechnique.Trolling
                    ? "Trolling requires the vessel to be moving."
                    : "Slow the vessel before setting the line.";
                return false;
            }

            bool seaweedHarvest = technique == FishingTechnique.SeaweedHarvesting;
            if (equipment == null) equipment = GetComponent<VesselEquipmentController>();
            float installedRodRange = seaweedHarvest ? 1f :
                (equipment != null ? equipment.RodCastMultiplier : 0.72f);
            selectedCastDistance = baseCastDistance * Rods[rodIndex].castMultiplier *
                                   Lines[lineIndex].castMultiplier * TechniqueCastMultiplier(technique) * installedRodRange;
            Vector3 castPoint = transform.position + transform.forward * selectedCastDistance;
            activeHabitat = DetermineHabitat(castPoint);
            if (seaweedHarvest && activeHabitat != FishHabitat.Kelp)
            {
                status = "SEAWEED HARVESTING REQUIRES A KELP BED. Change grounds before setting the line.";
                return false;
            }

            string baitItemId = BaitItemIds[Mathf.Clamp(baitIndex, 0, BaitItemIds.Length - 1)];
            if (!seaweedHarvest && cargoHold.GetAvailableQuantity(baitItemId) < 1)
            {
                status = "NO BAIT — return to Inez and purchase " + BaitLabels[baitIndex] + " bait.";
                return false;
            }

            if (seaState != null && !seaState.TryClaimInteraction())
            {
                status = "Finish the current interaction first.";
                return false;
            }

            if (!seaweedHarvest && !cargoHold.TryConsumeOne(baitItemId))
            {
                status = "BAIT INVENTORY CHANGED — no cast was made.";
                return false;
            }

            EnsurePopulationStocks();
            estimatedDepth = EstimateDepth(activeHabitat) * TechniqueDepthMultiplier(technique);
            hookedFish = null;
            tension = 0.35f;
            fatigue = 0f;
            fightProgress = 0f;
            fightBeats = 0;
            successfulBeats = 0;
            missedBites = 0;
            waitTimer = UnityEngine.Random.Range(biteWaitMinimum, Mathf.Max(biteWaitMinimum, biteWaitMaximum)) *
                        TechniqueBiteWaitMultiplier(technique);
            phase = FishingEncounterPhase.WaitingForBite;
            status = "LINE DEPLOYED — " + technique + " · " + activeHabitat + " · " +
                     Mathf.RoundToInt(selectedCastDistance) + " m cast. Watch for a bite.";
            if (seaState != null) seaState.SetHarvesting(true);
            return true;
        }

        private void Update()
        {
            AdvanceMigration();
            if (phase == FishingEncounterPhase.Idle) return;
            if (Time.timeScale <= 0.0001f || (seaState != null && seaState.IsDialogueActive))
            {
                CancelFishing("FISHING INTERRUPTED. The line has been retrieved.");
                return;
            }

            Rigidbody vessel = GetComponent<Rigidbody>();
            if (vessel != null && technique != FishingTechnique.Trolling && technique != FishingTechnique.Drifting &&
                new Vector2(vessel.linearVelocity.x, vessel.linearVelocity.z).magnitude > 1.5f)
            {
                CancelFishing("THE VESSEL MOVED — retrieve the line before changing grounds.");
                return;
            }

            if (phase == FishingEncounterPhase.WaitingForBite)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f) PresentBite();
                return;
            }

            phaseTimer += Time.deltaTime;
            if (phase == FishingEncounterPhase.HookWindow)
            {
                if (harvestBeatAction != null && harvestBeatAction.WasPressedThisFrame())
                {
                    float hookPosition = Mathf.Clamp01(phaseTimer / 1.25f);
                    if (Mathf.Abs(hookPosition - targetPhase) <= currentHookWindow)
                        BeginFight();
                    else
                        MissedBite();
                }
                else if (phaseTimer >= 1.25f)
                    MissedBite();
                return;
            }

            UpdateFightTension();
            if (harvestBeatAction != null && harvestBeatAction.WasPressedThisFrame())
                RegisterFightBeat();

            if (phaseTimer >= fightBeatDuration)
                ResolveFightBeat(false);
        }

        private void PresentBite()
        {
            hookedFish = SelectFish();
            if (hookedFish == null)
            {
                status = "NO BITE — the water is quiet here. Try another ground or technique.";
                waitTimer = UnityEngine.Random.Range(biteWaitMinimum * 0.7f, biteWaitMaximum * 1.1f) *
                            TechniqueBiteWaitMultiplier(technique);
                return;
            }

            phase = FishingEncounterPhase.HookWindow;
            phaseTimer = 0f;
            targetPhase = UnityEngine.Random.Range(0.25f, 0.75f);
            currentHookWindow = FishingSimulationPolicy.CalculateTimingWindow(strikeWindow, hookedFish.Resistance, (Rods[rodIndex].control + Hooks[hookIndex].control) * 0.5f * (equipment != null ? equipment.RodControlMultiplier : 0.70f));
            status = hookedFish.Category == CargoItemCategory.Seaweed
                ? "KELP BED CONTACT — harvest window is open. Pull at the marked moment."
                : "BITE! " + hookedFish.DisplayName + " is testing the bait — strike when the marker crosses the narrow target.";
        }

        private void BeginFight()
        {
            phase = FishingEncounterPhase.Fighting;
            phaseTimer = 0f;
            fightBeats = 0;
            targetPhase = UnityEngine.Random.Range(0.2f, 0.8f);
            currentFightWindow = FishingSimulationPolicy.CalculateTimingWindow(strikeWindow, hookedFish.Resistance * 0.85f, (Rods[rodIndex].control + Hooks[hookIndex].control + Reels[reelIndex].control) / 3f * (equipment != null ? equipment.RodControlMultiplier : 0.70f));
            tension = Mathf.Clamp(0.3f + hookedFish.Resistance * 0.12f, 0.2f, 0.65f);
            status = hookedFish.Category == CargoItemCategory.Seaweed
                ? "SEAWEED HARVEST — keep the line steady and gather the fronds. SPACE / ENTER at the marked moment."
                : hookedFish.DisplayName.ToUpperInvariant() + " HOOKED — manage tension and tire it out. SPACE / ENTER at the marked moment.";
        }

        private void UpdateFightTension()
        {
            float burstChance = FishingSimulationPolicy.CalculateEventChance(
                hookedFish.BurstFrequency * 1.15f, Time.deltaTime);
            float burst = UnityEngine.Random.value < burstChance ? 2.8f : 1f;
            float resistance = hookedFish.Resistance;
            float rodLimit = Rods[rodIndex].tensionLimit;
            float lineLimit = Lines[lineIndex].tensionLimit;
            tension += resistance * burst * Time.deltaTime * 0.105f;
            tension = Mathf.Clamp(tension + fatigue * Time.deltaTime * 0.012f, 0f, 1.5f);
            tension = Mathf.Clamp(tension, 0f, 1.5f);

            float safeLimit = Mathf.Min(1.4f, (rodLimit + lineLimit + Reels[reelIndex].tensionLimit) / 3f);
            if (tension > safeLimit)
            {
                CancelFishing("LINE SNAPPED — " + hookedFish.DisplayName + " escaped. Choose stronger tackle.");
                return;
            }

            if (tension < 0.08f && fightProgress < 0.5f)
            {
                fatigue = Mathf.Max(0f, fatigue - Time.deltaTime * 0.05f);
                fightProgress = Mathf.Max(0f, fightProgress - Time.deltaTime * 0.015f);
            }
        }

        private void RegisterFightBeat()
        {
            float phasePosition = Mathf.Clamp01(phaseTimer / fightBeatDuration);
            bool timed = Mathf.Abs(phasePosition - targetPhase) <= currentFightWindow;
            ResolveFightBeat(timed);
        }

        private void ResolveFightBeat(bool successful)
        {
            if (phase != FishingEncounterPhase.Fighting) return;
            if (successful)
            {
                successfulBeats++;
                fatigue += 0.15f * Hooks[hookIndex].control * Rods[rodIndex].control;
                fightProgress += 0.15f * Hooks[hookIndex].control * Reels[reelIndex].retrieveSpeed;
                tension = Mathf.Max(0.12f, tension - 0.13f * Lines[lineIndex].control);
                status = "GOOD CONTROL — the fish is tiring. Keep the line steady.";
            }
            else
            {
                tension += 0.18f * hookedFish.Resistance;
                fightProgress = Mathf.Max(0f, fightProgress - 0.075f);
                fatigue = Mathf.Max(0f, fatigue - 0.05f);
                status = "MISSED THE PULL — tension rises and the fish regains strength.";
            }

            fightBeats++;
            if (fightProgress >= 0.98f || fatigue >= 0.95f)
            {
                LandCatch();
                return;
            }

            if (fightBeats >= 9)
            {
                CancelFishing("THE FISH GOT AWAY — it outlasted the tackle.");
                return;
            }

            phaseTimer = 0f;
            targetPhase = UnityEngine.Random.Range(0.12f, 0.88f);
            currentFightWindow = FishingSimulationPolicy.CalculateTimingWindow(strikeWindow, hookedFish.Resistance * (1f + fightBeats * 0.035f), (Rods[rodIndex].control + Hooks[hookIndex].control + Reels[reelIndex].control) / 3f);
        }

        private void MissedBite()
        {
            missedBites++;
            if (missedBites >= 2)
            {
                CancelFishing("THE FISH SPOOKED — two missed strikes alerted it. Retrieve the line and change your approach.");
                return;
            }
            phase = FishingEncounterPhase.WaitingForBite;
            hookedFish = null;
            waitTimer = UnityEngine.Random.Range(biteWaitMinimum * 0.8f, biteWaitMaximum) *
                        TechniqueBiteWaitMultiplier(technique);
            status = "THE BITE WAS MISSED — the bait is still out. Wait for another chance.";
        }

        private static bool IsCatchableSeafood(CargoItemCategory category)
        {
            return category == CargoItemCategory.Fish ||
                   category == CargoItemCategory.Shellfish ||
                   category == CargoItemCategory.Cephalopod ||
                   category == CargoItemCategory.Seaweed;
        }

        private CargoItemDefinition SelectFish()
        {
            if (regionalChart == null) regionalChart = GetComponent<RegionalChartController>();
            List<CargoItemDefinition> candidates = new List<CargoItemDefinition>();
            List<float> weights = new List<float>();
            float total = 0f;
            bool aberrationRolled = false;
            bool aberrationAllowed = false;
            IReadOnlyList<CargoItemDefinition> definitions = cargoHold.ItemDefinitions;

            for (int i = 0; i < definitions.Count; i++)
            {
                CargoItemDefinition fish = definitions[i];
                if (fish == null || !IsCatchableSeafood(fish.Category)) continue;
                if (technique == FishingTechnique.SeaweedHarvesting
                    ? fish.Category != CargoItemCategory.Seaweed
                    : fish.Category == CargoItemCategory.Seaweed) continue;
                string regionId = regionalChart != null && !string.IsNullOrWhiteSpace(regionalChart.CurrentRegionId)
                    ? regionalChart.CurrentRegionId : "greyhaven";
                if (!fish.IsAvailableInRegion(regionId)) continue;
                if ((fish.Habitats & activeHabitat) == 0) continue;
                if (!FishingSimulationPolicy.IsWithinDepth(estimatedDepth, fish.MinimumDepthMetres, fish.MaximumDepthMetres)) continue;

                float stock = GetPopulation(fish, activeHabitat);
                if (stock <= 0f) continue;
                if (fish.IsAberrant)
                {
                    // One shared gate per bite-selection attempt: catalogue size must not
                    // multiply the aberration event chance.
                    if (!aberrationRolled)
                    {
                        aberrationAllowed = seaState != null && seaState.RollAberration();
                        aberrationRolled = true;
                    }
                    if (!aberrationAllowed) continue;
                }

                bool techniqueMatches = fish.PreferredTechnique == FishPreferredTechnique.Any ||
                                        TechniqueMatches(fish.PreferredTechnique, technique);
                bool baitMatches = fish.PreferredBaitId == "any" ||
                                   fish.PreferredBaitId == Baits[baitIndex];
                float weight = FishingSimulationPolicy.CalculateCandidateWeight(
                    stock, techniqueMatches, baitMatches, fish.IsAberrant);

                candidates.Add(fish);
                weights.Add(weight);
                total += weight;
            }

            if (candidates.Count == 0 || total <= 0f) return null;
            float roll = UnityEngine.Random.value * total;
            for (int i = 0; i < candidates.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return candidates[i];
            }
            return candidates[candidates.Count - 1];
        }

        private void LandCatch()
        {
            if (regionalChart == null) regionalChart = GetComponent<RegionalChartController>();
            if (wallet == null) wallet = GetComponent<EconomyWalletController>();
            if (economyTelemetry == null) economyTelemetry = GetComponent<ExpeditionEconomyTelemetry>();
            float quality = FishingSimulationPolicy.CalculateCatchQuality(successfulBeats, tension);
            float size = FishingSimulationPolicy.CalculateCatchSize(hookedFish.BaseCatchSize,
                hookedFish.MaximumCatchSize, quality, UnityEngine.Random.Range(0.65f, 1f));
            string instanceId;
            float saleQuantity = hookedFish.ItemId == "INV-019" ? 1f : size;
            if (!cargoHold.TryPlaceFirstFit(hookedFish.ItemId, 1, quality, 1f, out instanceId, saleQuantity))
            {
                CancelFishing("HOLD FULL — the catch slips away before it can be secured.");
                return;
            }

            string regionId = regionalChart != null && !string.IsNullOrWhiteSpace(regionalChart.CurrentRegionId)
                ? regionalChart.CurrentRegionId : "greyhaven";
            CatchLegalityResult legality = CatchLegalityPolicy.Evaluate(
                hookedFish, regionId, technique, size, regionalChart);
            cargoHold.TrySetCatchLegality(instanceId, regionId, technique.ToString(),
                size, legality.IsLegal, legality.Reason);

            bool fineCollected = legality.IsLegal || legality.FineCents <= 0L;
            if (!legality.IsLegal && wallet != null && legality.FineCents > 0L)
                fineCollected = wallet.TryApplyTransaction(
                    "catch.legality.fine." + Guid.NewGuid().ToString("N"), -legality.FineCents);

            ChangePopulation(hookedFish, activeHabitat, -1);
            if (hookedFish.Category != CargoItemCategory.Seaweed)
                totalCatches++;
            if (equipment != null && hookedFish.Category != CargoItemCategory.Seaweed)
                equipment.WearRodForCatch();
            if (economyTelemetry != null)
                economyTelemetry.RecordCatch(hookedFish.ItemId, size, legality.IsLegal);
            string grade = quality >= 0.9f ? "PRISTINE" : quality >= 0.75f ? "GOOD" : quality >= 0.55f ? "FAIR" : "POOR";
            string species = hookedFish.DisplayName;
            status = legality.IsLegal
                ? (hookedFish.Category == CargoItemCategory.Seaweed ? "HARVESTED: " : "LANDED: ") +
                  species + " · " + size.ToString("0.0") + " kg · " + grade +
                  " · " + Mathf.RoundToInt(quality * 100f) + "%"
                : "ILLEGAL LANDING: " + species + " · " + legality.Reason +
                  (fineCollected ? " · FINE PAID $" : " · FINE NOT COLLECTED $") +
                  (legality.FineCents / 100m).ToString("0.00") +
                  " · THIS CATCH CANNOT BE SOLD.";
            FinishFishing();
            hookedFish = null;
        }

        private void FinishFishing()
        {
            phase = FishingEncounterPhase.Idle;
            if (seaState != null)
            {
                seaState.SetHarvesting(false);
                seaState.AddDisturbance(0.035f);
                seaState.AddPanic(0.004f);
            }
        }

        private void CancelFishing(string message)
        {
            if (phase == FishingEncounterPhase.Idle) return;
            status = message;
            FinishFishing();
            hookedFish = null;
        }

        private FishHabitat DetermineHabitat(Vector3 castPoint)
        {
            if (castPoint.x < -35f && castPoint.z < -18f) return FishHabitat.Estuary;
            float distance = new Vector2(castPoint.x, castPoint.z).magnitude;
            if (distance > 105f) return FishHabitat.DeepWater;
            if (distance > 48f) return FishHabitat.OpenWater;
            if (castPoint.z > 20f && castPoint.z < 70f) return FishHabitat.Kelp;
            return FishHabitat.Nearshore;
        }

        private static float EstimateDepth(FishHabitat habitat)
        {
            switch (habitat)
            {
                case FishHabitat.Estuary: return 3f;
                case FishHabitat.Nearshore: return 7f;
                case FishHabitat.Kelp: return 14f;
                case FishHabitat.OpenWater: return 48f;
                case FishHabitat.DeepWater: return 125f;
                default: return 8f;
            }
        }

        private static float TechniqueDepthMultiplier(FishingTechnique value)
        {
            switch (value)
            {
                case FishingTechnique.BottomFishing: return 1.45f;
                case FishingTechnique.FloatFishing: return 0.35f;
                case FishingTechnique.Jigging: return 0.85f;
                case FishingTechnique.Trolling: return 0.7f;
                case FishingTechnique.Handlining: return 0.8f;
                case FishingTechnique.SeaweedHarvesting: return 0.45f;
                default: return 1f;
            }
        }

        private static float TechniqueBiteWaitMultiplier(FishingTechnique value)
        {
            switch (value)
            {
                case FishingTechnique.Jigging: return 0.75f;
                case FishingTechnique.Trolling: return 0.85f;
                case FishingTechnique.Drifting: return 0.9f;
                case FishingTechnique.BottomFishing: return 1.15f;
                case FishingTechnique.FloatFishing: return 1.1f;
                case FishingTechnique.Handlining: return 1.2f;
                case FishingTechnique.SeaweedHarvesting: return 0.65f;
                default: return 1f;
            }
        }

        private static float TechniqueCastMultiplier(FishingTechnique value)
        {
            switch (value)
            {
                case FishingTechnique.BottomFishing: return 0.8f;
                case FishingTechnique.FloatFishing: return 0.9f;
                case FishingTechnique.Jigging: return 0.85f;
                case FishingTechnique.Trolling: return 1.35f;
                case FishingTechnique.Drifting: return 1.1f;
                case FishingTechnique.Handlining: return 0.65f;
                case FishingTechnique.SeaweedHarvesting: return 0.45f;
                default: return 1f;
            }
        }

        private static bool TechniqueMatches(FishPreferredTechnique preferred, FishingTechnique actual)
        {
            return FishingSimulationPolicy.IsTechniqueCompatible(preferred, actual);
        }

        private void EnsurePopulationStocks()
        {
            if (cargoHold == null) return;
            IReadOnlyList<CargoItemDefinition> definitions = cargoHold.ItemDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                CargoItemDefinition fish = definitions[i];
                if (fish == null || !IsCatchableSeafood(fish.Category)) continue;
                for (int h = 0; h < HabitatOrder.Length; h++)
                {
                    FishHabitat habitat = HabitatOrder[h];
                    if ((fish.Habitats & habitat) != 0)
                        GetPopulation(fish, habitat);
                }
            }
        }

        private void AdvanceMigration()
        {
            if (seaState == null || cargoHold == null) return;
            double now = seaState.ElapsedGameSeconds;
            if (lastMigrationGameSeconds <= 0d)
            {
                lastMigrationGameSeconds = now;
                return;
            }

            const double migrationIntervalSeconds = 21600d;
            int elapsedIntervals = (int)Math.Floor(
                Math.Max(0d, now - lastMigrationGameSeconds) / migrationIntervalSeconds);
            if (elapsedIntervals <= 0) return;

            // Bound work after a very long offline gap; remaining intervals are processed
            // on subsequent frames without discarding elapsed simulation time.
            int intervalsToProcess = Math.Min(elapsedIntervals, 365);
            IReadOnlyList<CargoItemDefinition> definitions = cargoHold.ItemDefinitions;
            for (int interval = 0; interval < intervalsToProcess; interval++)
            {
                for (int i = 0; i < definitions.Count; i++)
                {
                    CargoItemDefinition fish = definitions[i];
                    if (fish == null || !IsCatchableSeafood(fish.Category)) continue;

                    int[] transfers = new int[HabitatOrder.Length];
                    for (int source = 0; source < HabitatOrder.Length; source++)
                    {
                        FishHabitat sourceHabitat = HabitatOrder[source];
                        if ((fish.Habitats & sourceHabitat) == 0) continue;

                        int destination = -1;
                        for (int offset = 1; offset < HabitatOrder.Length; offset++)
                        {
                            int candidate = (source + offset) % HabitatOrder.Length;
                            if ((fish.Habitats & HabitatOrder[candidate]) != 0)
                            {
                                destination = candidate;
                                break;
                            }
                        }
                        if (destination < 0) continue;

                        int stock = Mathf.RoundToInt(GetPopulation(fish, sourceHabitat));
                        int amount = FishingSimulationPolicy.CalculateMigrationAmount(stock, fish.MigrationRate, UnityEngine.Random.value);
                        transfers[source] -= amount;
                        transfers[destination] += amount;
                    }

                    for (int h = 0; h < HabitatOrder.Length; h++)
                    {
                        if (transfers[h] == 0) continue;
                        string key = PopulationKey(fish.ItemId, HabitatOrder[h]);
                        GetPopulation(fish, HabitatOrder[h]);
                        populations[key] = Mathf.Max(0, populations[key] + transfers[h]);
                    }
                }
            }

            lastMigrationGameSeconds += intervalsToProcess * migrationIntervalSeconds;
        }

        private float GetPopulation(CargoItemDefinition fish, FishHabitat habitat)
        {
            string key = PopulationKey(fish.ItemId, habitat);
            int population;
            if (!populations.TryGetValue(key, out population))
            {
                population = fish.InitialPopulation;
                populations[key] = population;
                populationTimes[key] = seaState != null ? seaState.ElapsedGameSeconds : 0d;
            }
            SimulatePopulation(fish, habitat, key);
            return populations[key];
        }

        private void SimulatePopulation(CargoItemDefinition fish, FishHabitat habitat, string key)
        {
            double now = seaState != null ? seaState.ElapsedGameSeconds : 0d;
            double previous;
            if (!populationTimes.TryGetValue(key, out previous))
            {
                populationTimes[key] = now;
                return;
            }

            double elapsedDays = Math.Max(0d, now - previous) / 86400d;
            int completeDays = (int)Math.Floor(elapsedDays);
            if (completeDays <= 0) return;
            int recovery = fish.DailyPopulationRecovery * completeDays;
            populations[key] = Mathf.Clamp(populations[key] + recovery, 0, fish.InitialPopulation * 2);
            // Preserve fractional-day remainder instead of rounding it away on each query.
            populationTimes[key] = previous + completeDays * 86400d;
        }

        private void ChangePopulation(CargoItemDefinition fish, FishHabitat habitat, int delta)
        {
            string key = PopulationKey(fish.ItemId, habitat);
            GetPopulation(fish, habitat);
            populations[key] = Mathf.Max(0, populations[key] + delta);
        }

        private static string PopulationKey(string speciesId, FishHabitat habitat)
        {
            return speciesId + "|" + habitat;
        }

        private void OnDisable()
        {
            if (phase != FishingEncounterPhase.Idle)
                CancelFishing("FISHING INTERRUPTED — the line has been retrieved.");
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = Color.white }
            };

            if (phase == FishingEncounterPhase.Idle)
            {
                float panelX = Screen.width - 348f;
                GUI.Box(new Rect(panelX, Screen.height - 168, 330, 146), GUIContent.none);
                GUI.Label(new Rect(panelX + 10, Screen.height - 162, 310, 20), "FISHING LOADOUT", style);
                GUIStyle statusStyle = new GUIStyle(style) { fontSize = 10 };
                GUI.Label(new Rect(panelX + 10, Screen.height - 140, 310, 18), status, statusStyle);
                if (GUI.Button(new Rect(panelX + 10, Screen.height - 112, 150, 22), "Rod: " + Rods[rodIndex].label + "  ‹"))
                    rodIndex = NextUnlocked(Rods, rodIndex, totalCatches);
                if (GUI.Button(new Rect(panelX + 170, Screen.height - 112, 150, 22), "Line: " + Lines[lineIndex].label + "  ‹"))
                    lineIndex = NextUnlocked(Lines, lineIndex, totalCatches);
                if (GUI.Button(new Rect(panelX + 10, Screen.height - 86, 150, 22), "Hook: " + Hooks[hookIndex].label + "  ‹"))
                    hookIndex = NextUnlocked(Hooks, hookIndex, totalCatches);
                if (GUI.Button(new Rect(panelX + 170, Screen.height - 86, 150, 22), "Reel: " + Reels[reelIndex].label + "  ‹"))
                    reelIndex = NextUnlocked(Reels, reelIndex, totalCatches);
                int selectedBait = Mathf.Clamp(baitIndex, 0, Baits.Length - 1);
                int baitCount = cargoHold != null ? cargoHold.GetAvailableQuantity(BaitItemIds[selectedBait]) : 0;
                string baitStatus = technique == FishingTechnique.SeaweedHarvesting
                    ? "  /  NOT REQUIRED" : (baitCount > 0 ? " x" + baitCount : "  /  EMPTY");
                if (GUI.Button(new Rect(panelX + 10, Screen.height - 60, 150, 22),
                    "Bait: " + BaitLabels[selectedBait] + baitStatus + "  ‹"))
                    baitIndex = (baitIndex + 1) % Baits.Length;
                if (GUI.Button(new Rect(panelX + 170, Screen.height - 60, 150, 22), "Method: " + technique + "  ‹"))
                    technique = Techniques[(Array.IndexOf(Techniques, technique) + 1) % Techniques.Length];
            }
            else
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 190, Screen.height - 170, 380, 126), GUIContent.none);
                GUI.Label(new Rect(Screen.width * 0.5f - 178, Screen.height - 162, 356, 24), status, style);
                if (phase == FishingEncounterPhase.Fighting)
                {
                    GUI.Label(new Rect(Screen.width * 0.5f - 178, Screen.height - 132, 356, 20),
                        "FIGHT " + (fightBeats + 1) + "/9  ·  PROGRESS " + Mathf.RoundToInt(fightProgress * 100f) + "%", style);
                    GUI.Label(new Rect(Screen.width * 0.5f - 178, Screen.height - 108, 356, 20),
                        "LINE TENSION " + Mathf.RoundToInt(tension * 100f) + "%  ·  TIME THE PULL", style);
                    Rect bar = new Rect(Screen.width * 0.5f - 170, Screen.height - 82, 340, 12);
                    GUI.Box(bar, GUIContent.none);
                    GUI.Box(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(fightProgress), bar.height), GUIContent.none);
                    float markerX = bar.x + bar.width * targetPhase;
                    GUI.Box(new Rect(markerX - 2f, bar.y - 5f, 4f, bar.height + 10f), GUIContent.none);
                    float needleX = bar.x + bar.width * Mathf.Clamp01(phaseTimer / fightBeatDuration);
                    GUI.Box(new Rect(needleX - 1f, bar.y - 8f, 2f, bar.height + 16f), GUIContent.none);
                }
                else if (phase == FishingEncounterPhase.HookWindow)
                {
                    GUI.Label(new Rect(Screen.width * 0.5f - 178, Screen.height - 132, 356, 20),
                        "SPACE / ENTER — STRIKE IN THE MARKED WINDOW", style);
                    Rect hookBar = new Rect(Screen.width * 0.5f - 170, Screen.height - 94, 340, 12);
                    GUI.Box(hookBar, GUIContent.none);
                    float hookTargetX = hookBar.x + hookBar.width * targetPhase;
                    GUI.Box(new Rect(hookTargetX - 3f, hookBar.y - 5f, 6f, hookBar.height + 10f), GUIContent.none);
                    float hookNeedleX = hookBar.x + hookBar.width * Mathf.Clamp01(phaseTimer / 1.25f);
                    GUI.Box(new Rect(hookNeedleX - 1f, hookBar.y - 8f, 2f, hookBar.height + 16f), GUIContent.none);
                }
                else
                    GUI.Label(new Rect(Screen.width * 0.5f - 178, Screen.height - 132, 356, 20),
                        "WAITING FOR A BITE…", style);

                if (GUI.Button(new Rect(Screen.width * 0.5f + 65, Screen.height - 66, 105, 24), "RETRIEVE LINE"))
                    CancelFishing("LINE RETRIEVED — no catch secured.");
            }
        }

        private static int NextUnlocked(GearChoice[] choices, int current, int catches)
        {
            for (int offset = 1; offset <= choices.Length; offset++)
            {
                int candidate = (current + offset) % choices.Length;
                if (choices[candidate].unlockAtCatches <= catches) return candidate;
            }
            return current;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null) return;
            for (int i = 0; i < data.progress.regionalStocks.Count; i++)
            {
                RegionalStockSaveData existing = data.progress.regionalStocks[i];
                if (existing != null && existing.regionId != null && existing.regionId.StartsWith("greyhaven-fishing/", StringComparison.Ordinal))
                    data.progress.regionalStocks.RemoveAt(i--);
            }

            foreach (KeyValuePair<string, int> pair in populations)
            {
                string[] parts = pair.Key.Split('|');
                if (parts.Length != 2) continue;
                data.progress.regionalStocks.Add(new RegionalStockSaveData
                {
                    regionId = "greyhaven-fishing/" + parts[1],
                    speciesId = parts[0],
                    population = pair.Value,
                    lastSimulatedGameSeconds = populationTimes.ContainsKey(pair.Key) ? populationTimes[pair.Key] : 0d
                });
            }

            data.progress.worldFlags.RemoveAll(flag => flag != null &&
                (flag.flagId == "fishing.total-catches" || flag.flagId == "fishing.technique" ||
                 flag.flagId == "fishing.rod" || flag.flagId == "fishing.line" ||
                 flag.flagId == "fishing.hook" || flag.flagId == "fishing.reel" || flag.flagId == "fishing.bait" ||
                 flag.flagId == "fishing.migration-time"));
            AddFlag(data, "fishing.total-catches", totalCatches.ToString());
            AddFlag(data, "fishing.technique", technique.ToString());
            AddFlag(data, "fishing.rod", Rods[rodIndex].id);
            AddFlag(data, "fishing.line", Lines[lineIndex].id);
            AddFlag(data, "fishing.hook", Hooks[hookIndex].id);
            AddFlag(data, "fishing.reel", Reels[reelIndex].id);
            AddFlag(data, "fishing.bait", Baits[baitIndex]);
            AddFlag(data, "fishing.migration-time", lastMigrationGameSeconds.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null) return;
            populations.Clear();
            populationTimes.Clear();
            for (int i = 0; i < data.progress.regionalStocks.Count; i++)
            {
                RegionalStockSaveData stock = data.progress.regionalStocks[i];
                if (stock == null || stock.regionId == null ||
                    !stock.regionId.StartsWith("greyhaven-fishing/", StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(stock.speciesId)) continue;
                string habitatName = stock.regionId.Substring("greyhaven-fishing/".Length);
                FishHabitat habitat;
                if (!Enum.TryParse(habitatName, out habitat)) continue;
                string key = PopulationKey(stock.speciesId, habitat);
                populations[key] = Mathf.Max(0, stock.population);
                populationTimes[key] = Math.Max(0d, stock.lastSimulatedGameSeconds);
            }

            totalCatches = Mathf.Max(0, ReadIntFlag(data, "fishing.total-catches"));
            technique = ReadEnumFlag(data, "fishing.technique", FishingTechnique.StillFishing);
            rodIndex = FindChoice(Rods, ReadFlag(data, "fishing.rod", "shore-rod"));
            lineIndex = FindChoice(Lines, ReadFlag(data, "fishing.line", "monofilament"));
            hookIndex = FindChoice(Hooks, ReadFlag(data, "fishing.hook", "standard-hook"));
            reelIndex = FindChoice(Reels, ReadFlag(data, "fishing.reel", "standard-reel"));
            if (Rods[rodIndex].unlockAtCatches > totalCatches) rodIndex = 0;
            if (Lines[lineIndex].unlockAtCatches > totalCatches) lineIndex = 0;
            if (Hooks[hookIndex].unlockAtCatches > totalCatches) hookIndex = 0;
            if (Reels[reelIndex].unlockAtCatches > totalCatches) reelIndex = 0;
            baitIndex = Array.IndexOf(Baits, ReadFlag(data, "fishing.bait", "worm"));
            if (baitIndex < 0) baitIndex = 0;
            double.TryParse(ReadFlag(data, "fishing.migration-time", "0"), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out lastMigrationGameSeconds);
        }

        private static void AddFlag(SaveGameData data, string id, string value)
        {
            data.progress.worldFlags.Add(new WorldFlagSaveData { flagId = id, value = true, stringValue = value });
        }

        private static string ReadFlag(SaveGameData data, string id, string fallback)
        {
            for (int i = 0; i < data.progress.worldFlags.Count; i++)
                if (data.progress.worldFlags[i] != null && data.progress.worldFlags[i].flagId == id)
                    return data.progress.worldFlags[i].stringValue ?? fallback;
            return fallback;
        }

        private static int ReadIntFlag(SaveGameData data, string id)
        {
            int value;
            return int.TryParse(ReadFlag(data, id, "0"), out value) ? value : 0;
        }

        private static T ReadEnumFlag<T>(SaveGameData data, string id, T fallback) where T : struct
        {
            T value;
            return Enum.TryParse(ReadFlag(data, id, fallback.ToString()), out value) ? value : fallback;
        }

        private static int FindChoice(GearChoice[] choices, string id)
        {
            for (int i = 0; i < choices.Length; i++)
                if (choices[i].id == id) return i;
            return 0;
        }
    }
}
