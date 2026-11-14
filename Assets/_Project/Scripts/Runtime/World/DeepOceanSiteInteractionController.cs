using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// First physical interaction layer for authored deep-ocean sites.
    /// Players can approach a marker and inspect its field note; physical hazard effects
    /// remain owned by future type-specific controllers.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DeepOceanSiteInteractionController : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField, Min(1f)] private float interactionDistance = 16f;
        [SerializeField, Min(0.25f)] private float inspectionCooldownRealSeconds = 1.25f;

        private float nextInspectionAllowedAt;
        private Rigidbody vesselBody;
        private InputAction interactAction;
        private DeepOceanPhenomenonSite[] sites = Array.Empty<DeepOceanPhenomenonSite>();
        private HistoricalArtifact[] historicalArtifacts = Array.Empty<HistoricalArtifact>();
        private readonly HashSet<string> inspectedSiteIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> inspectedHistoricalArtifactIds = new HashSet<string>(StringComparer.Ordinal);
        private string latestInspection = "APPROACH A SURVEY SITE AND PRESS E TO INSPECT.";

        public IReadOnlyCollection<string> InspectedSiteIds => inspectedSiteIds;
        public IReadOnlyCollection<string> InspectedHistoricalArtifactIds => inspectedHistoricalArtifactIds;
        public string LatestInspection => latestInspection;

        public void Configure(InputActionAsset input, SeaStateDirector director)
        {
            controls = input;
            seaState = director;
        }

        private void Awake()
        {
            vesselBody = GetComponent<Rigidbody>();
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            sites = FindObjectsByType<DeepOceanPhenomenonSite>(FindObjectsSortMode.None);
            historicalArtifacts = FindObjectsByType<HistoricalArtifact>(FindObjectsSortMode.None);
        }

        private void Start()
        {
            if (controls != null) interactAction = controls.FindAction("Interact", false);
        }

        private void Update()
        {
            if (interactAction != null && interactAction.WasPressedThisFrame())
                TryInspectNearestSite();
        }

        private void TryInspectNearestSite()
        {
            if (Time.timeScale <= 0.0001f ||
                !DeepOceanInspectionPolicy.IsCooldownReady(Time.unscaledTime, nextInspectionAllowedAt))
                return;

            DeepOceanPhenomenonSite nearestSite = null;
            HistoricalArtifact nearestArtifact = null;
            float bestDistance = interactionDistance * interactionDistance;
            Vector2 vesselPosition = new Vector2(vesselBody.position.x, vesselBody.position.z);

            for (int index = 0; index < sites.Length; index++)
            {
                DeepOceanPhenomenonSite site = sites[index];
                if (site == null || !site.IsValidForRegion) continue;
                Vector2 sitePosition = new Vector2(site.transform.position.x, site.transform.position.z);
                float distance = (sitePosition - vesselPosition).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    nearestSite = site;
                    nearestArtifact = null;
                    bestDistance = distance;
                }
            }

            for (int index = 0; index < historicalArtifacts.Length; index++)
            {
                HistoricalArtifact artifact = historicalArtifacts[index];
                if (artifact == null || !artifact.IsValid) continue;
                Vector2 artifactPosition = new Vector2(artifact.transform.position.x, artifact.transform.position.z);
                float distance = (artifactPosition - vesselPosition).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    nearestArtifact = artifact;
                    nearestSite = null;
                    bestDistance = distance;
                }
            }

            if (nearestSite == null && nearestArtifact == null)
            {
                latestInspection = "NO SURVEY SITE OR HISTORICAL ARTIFACT WITHIN SAFE INSPECTION RANGE.";
                return;
            }

            // Claim only after a valid target is found; an empty interaction must not
            // steal the same-frame input from a nearby harbour NPC or dockside action.
            if (seaState != null && !seaState.TryClaimInteraction())
                return;

            nextInspectionAllowedAt = DeepOceanInspectionPolicy.GetNextAllowedTime(
                Time.unscaledTime, inspectionCooldownRealSeconds);

            if (nearestArtifact != null)
            {
                bool firstInspection;
                TryRecordHistoricalArtifact(nearestArtifact, out firstInspection);
                return;
            }

            bool firstSiteInspection = inspectedSiteIds.Add(nearestSite.SiteId);
            latestInspection = (firstSiteInspection ? "FIELD NOTE RECORDED: " : "KNOWN SITE: ") +
                nearestSite.DisplayName + " [" + nearestSite.EvidenceLabel + "]\n" + nearestSite.FirstSurveyNote;
            // Survey-site disturbance is a one-time consequence; archive reading never adds it.
            if (seaState != null && firstSiteInspection)
                seaState.AddDisturbance(DeepOceanInspectionPolicy.GetDisturbance(firstSiteInspection));
        }

        /// <summary>
        /// Records a read without treating it as a Sounding Rig discovery or causing sea disturbance.
        /// </summary>
        public bool TryRecordHistoricalArtifact(HistoricalArtifact artifact, out bool firstInspection)
        {
            firstInspection = false;
            if (artifact == null || !artifact.IsValid) return false;

            firstInspection = inspectedHistoricalArtifactIds.Add(artifact.ArtifactId);
            latestInspection = artifact.BuildInspectionText(!firstInspection);
            return true;
        }
            if (Time.timeScale <= 0.0001f ||
                !DeepOceanInspectionPolicy.IsCooldownReady(Time.unscaledTime, nextInspectionAllowedAt))
                return;

            DeepOceanPhenomenonSite nearest = null;
            float bestDistance = interactionDistance * interactionDistance;
            Vector2 vesselPosition = new Vector2(vesselBody.position.x, vesselBody.position.z);
            for (int index = 0; index < sites.Length; index++)
            {
                DeepOceanPhenomenonSite site = sites[index];
                if (site == null || !site.IsValidForRegion) continue;
                Vector2 sitePosition = new Vector2(site.transform.position.x, site.transform.position.z);
                float distance = (sitePosition - vesselPosition).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    nearest = site;
                    bestDistance = distance;
                }
            }

            if (nearest == null)
            {
                latestInspection = "NO SURVEY SITE WITHIN SAFE INSPECTION RANGE.";
                return;
            }

            // Claim only after a valid target is found; an empty interaction must not
            // steal the same-frame input from a nearby harbour NPC.
            if (seaState != null && !seaState.TryClaimInteraction())
                return;

            nextInspectionAllowedAt = DeepOceanInspectionPolicy.GetNextAllowedTime(
                Time.unscaledTime, inspectionCooldownRealSeconds);

            bool firstInspection = inspectedSiteIds.Add(nearest.SiteId);
            latestInspection = (firstInspection ? "FIELD NOTE RECORDED: " : "KNOWN SITE: ") +
                nearest.DisplayName + " [" + nearest.EvidenceLabel + "]\n" + nearest.FirstSurveyNote;
            // A site's disturbance is a one-time discovery consequence, never a repeat-interaction farm.
            if (seaState != null && firstInspection)
                seaState.AddDisturbance(DeepOceanInspectionPolicy.GetDisturbance(firstInspection));
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.world == null)
                throw new ArgumentNullException(nameof(data), "Save data must contain world state.");
            data.world.inspectedPhenomenonSiteIds.Clear();
            data.world.inspectedPhenomenonSiteIds.AddRange(inspectedSiteIds);
            data.world.inspectedHistoricalArtifactIds.Clear();
            data.world.inspectedHistoricalArtifactIds.AddRange(inspectedHistoricalArtifactIds);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null) return;
            inspectedSiteIds.Clear();
            if (data.world.inspectedPhenomenonSiteIds == null) return;
            for (int index = 0; index < data.world.inspectedPhenomenonSiteIds.Count; index++)
            {
                string id = data.world.inspectedPhenomenonSiteIds[index];
                if (!string.IsNullOrWhiteSpace(id)) inspectedSiteIds.Add(id);
            }
            inspectedHistoricalArtifactIds.Clear();
            if (data.world.inspectedHistoricalArtifactIds == null) return;
            for (int index = 0; index < data.world.inspectedHistoricalArtifactIds.Count; index++)
            {
                string id = data.world.inspectedHistoricalArtifactIds[index];
                if (!string.IsNullOrWhiteSpace(id)) inspectedHistoricalArtifactIds.Add(id);
            }
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(latestInspection)) return;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12, wordWrap = true,
                normal = { textColor = new Color(0.82f, 0.86f, 0.84f) }
            };
            GUI.Label(new Rect(20f, Screen.height - 250f, 560f, 150f), latestInspection, style);
        }
    }

    public static class DeepOceanInspectionPolicy
    {
        public static bool IsCooldownReady(float currentUnscaledTime, float nextAllowedAt)
        {
            return !float.IsNaN(currentUnscaledTime) && !float.IsInfinity(currentUnscaledTime) &&
                   !float.IsNaN(nextAllowedAt) && !float.IsInfinity(nextAllowedAt) &&
                   currentUnscaledTime >= nextAllowedAt;
        }

        public static float GetNextAllowedTime(float currentUnscaledTime, float cooldownSeconds)
        {
            if (float.IsNaN(currentUnscaledTime) || float.IsInfinity(currentUnscaledTime))
                return float.PositiveInfinity;

            float safeCooldown = float.IsNaN(cooldownSeconds) || float.IsInfinity(cooldownSeconds)
                ? 1.25f
                : Mathf.Max(0.25f, cooldownSeconds);
            return currentUnscaledTime + safeCooldown;
        }

        public static float GetDisturbance(bool firstInspection)
        {
            return firstInspection ? 0.008f : 0f;
        }
    }
}
