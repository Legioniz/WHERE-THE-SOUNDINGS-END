using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WhereTheSoundingsEnd.Core;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// First playable Sounding Rig pass. It reads authored regional bathymetry and
    /// acoustic profiles, distinguishes mid-water biological returns from seabed,
    /// discovers authored sites, and publishes a gameplay acoustic signal.
    /// It does not create terrain, collision, or lethal encounters.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SoundingRigController : MonoBehaviour, ISaveParticipant
    {
        [Header("References")]
        [SerializeField] private Rigidbody vesselBody;
        [SerializeField] private SeaStateDirector seaState;

        [Header("Pulse tuning")]
        [SerializeField, Min(0.5f)] private float pulseCooldownRealSeconds = 4f;
        [SerializeField, Min(0.1f)] private float surveyActionRealSeconds = 1.25f;
        [SerializeField, Min(1f)] private float maximumSurveySpeed = 5f;
        [SerializeField, Min(1f)] private float minimumLayerSeparationMeters = 90f;
        [SerializeField, Min(1f)] private float siteDetectionRangeMultiplier = 1f;

        [SerializeField] private InputActionAsset controls;
        private InputAction pulseAction;
        private DeepOceanSurveyRegion[] regions = Array.Empty<DeepOceanSurveyRegion>();
        private DeepOceanPhenomenonSite[] sites = Array.Empty<DeepOceanPhenomenonSite>();
        private readonly HashSet<string> discoveredSiteIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<string> evaluatedOpportunityIds = new List<string>();
        private float nextPulseAllowedAt;
        private float surveyingRemaining;
        private int pulseSequence;
        private string latestReading = "SOUNDING RIG: READY";
        private AudioSource audioSource;
        private AudioClip pingClip;
        private float previousLayerDepth = -1f;
        private float previousHeading;
        private double previousPingGameTime = -100000.0;
        private string previousLayerRegionId = string.Empty;
        private int consecutiveLayerEvidence;

        public int PulseSequence => pulseSequence;
        public string LatestReading => latestReading;
        public IReadOnlyCollection<string> DiscoveredSiteIds => discoveredSiteIds;

        public void Configure(InputActionAsset input, SeaStateDirector director, Rigidbody vessel)
        {
            controls = input;
            seaState = director;
            vesselBody = vessel;
        }

        private void Awake()
        {
            if (vesselBody == null) vesselBody = GetComponent<Rigidbody>();
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            regions = FindObjectsByType<DeepOceanSurveyRegion>(FindObjectsSortMode.None);
            sites = FindObjectsByType<DeepOceanPhenomenonSite>(FindObjectsSortMode.None);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.34f;
            pingClip = BuildPingClip();
        }

        private void Start()
        {
            if (controls != null) pulseAction = controls.FindAction("SoundingPulse", false);
        }

        private void Update()
        {
            if (surveyingRemaining > 0f)
            {
                surveyingRemaining -= Time.deltaTime;
                if (surveyingRemaining <= 0f && seaState != null)
                    seaState.SetSurveying(false);
            }

            if (pulseAction != null && pulseAction.WasPressedThisFrame())
                TryPulse();
        }

        private void TryPulse()
        {
            if (Time.timeScale <= 0.0001f || (seaState != null && seaState.IsDialogueActive) || Time.unscaledTime < nextPulseAllowedAt)
                return;

            if (vesselBody == null)
            {
                latestReading = "SOUNDING RIG: vessel reference unavailable.";
                return;
            }

            if (vesselBody.linearVelocity.magnitude > maximumSurveySpeed)
            {
                latestReading = "SOUNDING RIG: reduce speed before deploying the listening buoy.";
                return;
            }

            DeepOceanSurveyRegion region = FindCurrentRegion(vesselBody.position);
            if (region == null)
            {
                latestReading = "SOUNDING RIG: no calibrated regional profile. Chart coverage unavailable.";
                return;
            }

            nextPulseAllowedAt = Time.unscaledTime + pulseCooldownRealSeconds;
            surveyingRemaining = surveyActionRealSeconds;
            if (seaState != null) seaState.SetSurveying(true);

            pulseSequence++;
            string opportunityId = region.RegionId + ":day:" +
                (seaState != null ? seaState.DayNumber : 1) + ":sounding:" + pulseSequence;
            if (evaluatedOpportunityIds.Contains(opportunityId))
            {
                latestReading = "SOUNDING RIG: this survey opportunity is already recorded.";
                return;
            }
            evaluatedOpportunityIds.Add(opportunityId);
            if (evaluatedOpportunityIds.Count > 256)
                evaluatedOpportunityIds.RemoveAt(0);

            ulong seed = DeepOceanSurveyMath.StableKey(opportunityId);
            Pcg32Random random = new Pcg32Random(seed, 0x534F554E44494E47UL);
            float seabedDepth = region.NominalSeabedDepthMeters +
                ((float)random.NextDouble() * 2f - 1f) * region.SeabedVariationMeters;
            seabedDepth = Mathf.Max(2f, seabedDepth);

            float signalQuality = Mathf.Clamp01(1f - region.BackgroundNoise);
            float seabedConfidence = DeepOceanSurveyMath.CalculateReturnConfidence(
                1f, signalQuality, region.AcousticAttenuation);
            float layerConfidence = DeepOceanSurveyMath.CalculateReturnConfidence(
                region.LayerDensity, signalQuality, region.AcousticAttenuation);

            string reading = "SOUNDING — " + region.RegionId.ToUpperInvariant() +
                "\nSEABED RETURN: " + Mathf.RoundToInt(seabedDepth) + " m" +
                "  ·  CONFIDENCE " + Mathf.RoundToInt(seabedConfidence * 100f) + "%";

            if (region.HasScatteringLayer)
            {
                float hour = seaState != null ? seaState.CurrentHour : 12f;
                float layerDepth = region.GetLayerDepth(hour);
                bool distinct = DeepOceanSurveyMath.IsDistinctMidwaterReturn(
                    layerDepth, seabedDepth, minimumLayerSeparationMeters);

                if (distinct)
                {
                    float heading = vesselBody.rotation.eulerAngles.y;
                    bool independentPing = previousLayerDepth > 0f &&
                        string.Equals(previousLayerRegionId, region.RegionId, StringComparison.OrdinalIgnoreCase) &&
                        Math.Abs(Mathf.DeltaAngle(previousHeading, heading)) >= 25f &&
                        (seaState == null || seaState.ElapsedGameSeconds - previousPingGameTime <= 600.0);

                    consecutiveLayerEvidence = independentPing
                        ? consecutiveLayerEvidence + 1
                        : 1;
                    previousLayerDepth = layerDepth;
                    previousLayerRegionId = region.RegionId;
                    previousHeading = heading;
                    previousPingGameTime = seaState != null ? seaState.ElapsedGameSeconds : 0.0;

                    reading += "\nMID-WATER RETURN: " + Mathf.RoundToInt(layerDepth) + " m";
                    reading += consecutiveLayerEvidence >= 2
                        ? "\nINTERPRETATION: repeatable biological scattering layer. Not the seabed."
                        : "\nINTERPRETATION: possible biological layer. Repeat from another heading to verify.";
                    reading += "\nDENSITY: " + Mathf.RoundToInt(region.LayerDensity * 100f) +
                        "%  ·  RETURN CONFIDENCE " + Mathf.RoundToInt(layerConfidence * 100f) + "%";
                }
                else
                {
                    consecutiveLayerEvidence = 0;
                    previousLayerDepth = -1f;
                    previousLayerRegionId = string.Empty;
                    reading += "\nMID-WATER RETURN: not resolved above the bottom.";
                }
            }
            else
            {
                consecutiveLayerEvidence = 0;
                previousLayerDepth = -1f;
                previousLayerRegionId = string.Empty;
                reading += "\nWATER COLUMN: no configured deep scattering layer in this habitat.";
            }

            DeepOceanPhenomenonSite site = FindNearestSite(region.RegionId, vesselBody.position);
            if (site != null)
            {
                bool firstDiscovery = discoveredSiteIds.Add(site.SiteId);
                reading += firstDiscovery
                    ? "\nNEW SITE: " + site.DisplayName + " [" + site.EvidenceLabel + "]"
                    : "\nKNOWN SITE: " + site.DisplayName;
                reading += "\n" + site.FirstSurveyNote;
            }

            latestReading = reading;
            if (audioSource != null && pingClip != null) audioSource.PlayOneShot(pingClip);
            AcousticEventBus.Publish(new AcousticSignal(
                opportunityId, AcousticEventKind.SoundingPulse, region.RegionId,
                vesselBody.position, 0.72f, 900f, seaState != null ? seaState.ElapsedGameSeconds : 0.0));

            if (seaState != null) seaState.AddDisturbance(0.012f);
        }

        private DeepOceanSurveyRegion FindCurrentRegion(Vector3 position)
        {
            DeepOceanSurveyRegion nearest = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < regions.Length; i++)
            {
                DeepOceanSurveyRegion region = regions[i];
                if (region == null || !region.Contains(position)) continue;
                float distance = (region.transform.position - position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = region;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }

        private DeepOceanPhenomenonSite FindNearestSite(string regionId, Vector3 position)
        {
            DeepOceanPhenomenonSite nearest = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < sites.Length; i++)
            {
                DeepOceanPhenomenonSite site = sites[i];
                if (site == null || !site.IsDetectableFrom(position, regionId)) continue;
                float distance = (site.transform.position - position).sqrMagnitude;
                float range = site.DetectionRadius * siteDetectionRangeMultiplier;
                if (distance <= range * range && distance < bestDistance)
                {
                    nearest = site;
                    bestDistance = distance;
                }
            }
            return nearest;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.world == null) return;
            data.world.soundingPulseCount = pulseSequence;
            data.world.discoveredPhenomenonSiteIds.Clear();
            data.world.discoveredPhenomenonSiteIds.AddRange(discoveredSiteIds);
            data.world.evaluatedSurveyOpportunityIds.Clear();
            data.world.evaluatedSurveyOpportunityIds.AddRange(evaluatedOpportunityIds);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null) return;
            pulseSequence = Mathf.Max(0, data.world.soundingPulseCount);
            discoveredSiteIds.Clear();
            evaluatedOpportunityIds.Clear();
            if (data.world.discoveredPhenomenonSiteIds != null)
                for (int i = 0; i < data.world.discoveredPhenomenonSiteIds.Count; i++)
                    if (!string.IsNullOrWhiteSpace(data.world.discoveredPhenomenonSiteIds[i]))
                        discoveredSiteIds.Add(data.world.discoveredPhenomenonSiteIds[i]);
            if (data.world.evaluatedSurveyOpportunityIds != null)
                for (int i = 0; i < data.world.evaluatedSurveyOpportunityIds.Count; i++)
                    if (!string.IsNullOrWhiteSpace(data.world.evaluatedSurveyOpportunityIds[i]))
                        evaluatedOpportunityIds.Add(data.world.evaluatedSurveyOpportunityIds[i]);
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color(0.76f, 0.86f, 0.86f) }
            };
            GUI.Label(new Rect(20f, Screen.height - 148f, 510f, 102f),
                "SOUNDING RIG  [Q]\n" + latestReading, style);
        }

        private void OnDisable()
        {
            surveyingRemaining = 0f;
            if (seaState != null) seaState.SetSurveying(false);
        }

        private void OnDestroy()
        {
            if (pingClip != null) Destroy(pingClip);
        }

        private static AudioClip BuildPingClip()
        {
            const int sampleRate = 22050;
            const float duration = 0.48f;
            int count = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-t * 8f) * Mathf.Clamp01(t * 70f);
                float carrier = Mathf.Sin(2f * Mathf.PI * 1180f * t);
                float overtone = Mathf.Sin(2f * Mathf.PI * 2360f * t) * 0.18f;
                samples[i] = (carrier + overtone) * envelope * 0.24f;
            }
            AudioClip clip = AudioClip.Create("Sounding_Rig_Transmit_Ping", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}