using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Records regions only when the vessel physically enters their authored survey bounds.
    /// The chart reveals only visited regions and sites already discovered by the Sounding Rig.
    /// It never creates bearings, compass guidance, or markers for unknown content.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RegionalChartController : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        [SerializeField] private Rigidbody vesselBody;
        [SerializeField, Min(0.1f)] private float regionRefreshSeconds = 0.5f;

        private readonly HashSet<string> visitedRegionIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> unlockedRegionIds = new HashSet<string>(StringComparer.Ordinal);
        private DeepOceanSurveyRegion[] regions = Array.Empty<DeepOceanSurveyRegion>();
        private DeepOceanPhenomenonSite[] sites = Array.Empty<DeepOceanPhenomenonSite>();
        private SoundingRigController soundingRig;
        private float nextRegionRefresh;
        private DeepOceanSurveyRegion currentRegion;
        private string latestDiscovery = string.Empty;

        public string CurrentRegionId => currentRegion != null ? currentRegion.RegionId : string.Empty;
        public string CurrentRegionName => GetDisplayName(CurrentRegionId);
        public bool HasCurrentChart => currentRegion != null &&
            RegionalChartPolicy.IsRegionKnown(visitedRegionIds, currentRegion.RegionId);
        public Vector3 CurrentRegionCenter => currentRegion != null ? currentRegion.transform.position : Vector3.zero;
        public float CurrentRegionRadius => currentRegion != null ? currentRegion.SurveyRadius : 1f;
        public Vector3 VesselPosition => vesselBody != null ? vesselBody.position : transform.position;
        public string LatestDiscovery => latestDiscovery;
        public IReadOnlyCollection<string> VisitedRegionIds => visitedRegionIds;
        public IReadOnlyCollection<string> UnlockedRegionIds => unlockedRegionIds;

        private void Awake()
        {
            if (vesselBody == null) vesselBody = GetComponent<Rigidbody>();
            RefreshSceneReferences();
        }

        private void Start()
        {
            // Resolve the initial harbour chart before any early checkpoint can be requested.
            RefreshSceneReferences();
            ResolveCurrentRegion();
        }

        private void OnEnable()
        {
            nextRegionRefresh = 0f;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRegionRefresh) return;
            nextRegionRefresh = Time.unscaledTime + Mathf.Max(0.1f, regionRefreshSeconds);
            RefreshSceneReferences();
            ResolveCurrentRegion();
        }

        private void RefreshSceneReferences()
        {
            regions = FindObjectsByType<DeepOceanSurveyRegion>(FindObjectsSortMode.None);
            sites = FindObjectsByType<DeepOceanPhenomenonSite>(FindObjectsSortMode.None);
            if (soundingRig == null) soundingRig = FindFirstObjectByType<SoundingRigController>();
        }

        private void ResolveCurrentRegion()
        {
            if (vesselBody == null) return;
            DeepOceanSurveyRegion nearest = null;
            float nearestDistance = float.MaxValue;
            Vector2 vessel = new Vector2(vesselBody.position.x, vesselBody.position.z);
            for (int i = 0; i < regions.Length; i++)
            {
                DeepOceanSurveyRegion region = regions[i];
                if (region == null || !region.Contains(vesselBody.position)) continue;
                Vector2 centre = new Vector2(region.transform.position.x, region.transform.position.z);
                float distance = (centre - vessel).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = region;
                    nearestDistance = distance;
                }
            }

            if (nearest != currentRegion)
                currentRegion = nearest;

            // Re-check even when the active region reference is unchanged. A transactional
            // save restore may replace the visit ledger while leaving this scene component alive.
            if (currentRegion != null &&
                RegionalChartPolicy.TryRecordVisit(visitedRegionIds, unlockedRegionIds, currentRegion.RegionId))
            {
                latestDiscovery = "CHART RECORDED: " + GetDisplayName(currentRegion.RegionId) +
                    ". You have entered waters that can now be added to your chart.";
            }
        }

        /// <summary>
        /// Records an earned permit without marking the region as visited or revealing its
        /// location. Physical entry remains the only way to add a visited chart record.
        /// </summary>
        public bool HasPermit(string regionId)
        {
            string id = RegionalChartPolicy.NormalizeId(regionId);
            return id.Length > 0 && unlockedRegionIds.Contains(id);
        }

        public bool GrantChartPermit(string regionId)
        {
            string id = RegionalChartPolicy.NormalizeId(regionId);
            if (id.Length == 0) return false;
            return unlockedRegionIds.Add(id);
        }

        public List<DeepOceanPhenomenonSite> GetKnownSitesForCurrentRegion()
        {
            List<DeepOceanPhenomenonSite> known = new List<DeepOceanPhenomenonSite>();
            if (!HasCurrentChart || soundingRig == null) return known;
            IReadOnlyCollection<string> discovered = soundingRig.DiscoveredSiteIds;
            for (int i = 0; i < sites.Length; i++)
            {
                DeepOceanPhenomenonSite site = sites[i];
                if (site == null || !site.IsValidForRegion ||
                    !string.Equals(site.RegionId, CurrentRegionId, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (RegionalChartPolicy.ShouldRevealSite(visitedRegionIds, discovered, site.RegionId, site.SiteId))
                    known.Add(site);
            }
            return known;
        }

        public Vector2 GetChartPosition(Vector3 worldPosition)
        {
            if (!HasCurrentChart) return new Vector2(0.5f, 0.5f);
            float diameter = Mathf.Max(2f, CurrentRegionRadius * 2f);
            Vector3 centre = CurrentRegionCenter;
            return new Vector2(
                Mathf.Clamp01(0.5f + (worldPosition.x - centre.x) / diameter),
                Mathf.Clamp01(0.5f - (worldPosition.z - centre.z) / diameter));
        }

        public List<string> GetVisitedRegionNames()
        {
            List<string> names = new List<string>();
            foreach (string id in visitedRegionIds) names.Add(GetDisplayName(id));
            names.Sort(StringComparer.Ordinal);
            return names;
        }

        public List<string> GetPermittedRegionNames()
        {
            List<string> names = new List<string>();
            foreach (string id in unlockedRegionIds)
                if (!visitedRegionIds.Contains(id) && IsChartRegionId(id))
                    names.Add(GetDisplayName(id));
            names.Sort(StringComparer.Ordinal);
            return names;
        }

        public List<string> GetPermittedHarvestNames()
        {
            List<string> names = new List<string>();
            if (unlockedRegionIds.Contains("reef_stewardship")) names.Add("Reef Stewardship");
            if (unlockedRegionIds.Contains("licensed_sea_cucumber")) names.Add("Licensed Sea Cucumber Harvest");
            if (unlockedRegionIds.Contains("managed_harvest")) names.Add("Managed Abalone Harvest");
            names.Sort(StringComparer.Ordinal);
            return names;
        }

        private static bool IsChartRegionId(string id)
        {
            switch (RegionalChartPolicy.NormalizeId(id))
            {
                case "greyhaven":
                case "the_shallows":
                case "blackrock_reach":
                case "the_drowned_parish":
                case "the_outer_shoals":
                case "the_hollow_trench":
                    return true;
                default:
                    return false;
            }
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.world == null || data.progress == null)
                throw new ArgumentNullException(nameof(data), "Save data must contain world and progress state.");
            data.world.visitedRegionIds.Clear();
            data.world.visitedRegionIds.AddRange(visitedRegionIds);
            foreach (string id in unlockedRegionIds)
                if (!data.progress.unlockedRegionIds.Contains(id))
                    data.progress.unlockedRegionIds.Add(id);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null || data.progress == null) return;
            currentRegion = null;
            latestDiscovery = string.Empty;
            visitedRegionIds.Clear();
            unlockedRegionIds.Clear();
            if (data.world.visitedRegionIds != null)
                for (int i = 0; i < data.world.visitedRegionIds.Count; i++)
                {
                    string id = RegionalChartPolicy.NormalizeId(data.world.visitedRegionIds[i]);
                    if (id.Length > 0) visitedRegionIds.Add(id);
                }
            if (data.progress.unlockedRegionIds != null)
                for (int i = 0; i < data.progress.unlockedRegionIds.Count; i++)
                {
                    string id = RegionalChartPolicy.NormalizeId(data.progress.unlockedRegionIds[i]);
                    if (id.Length > 0) unlockedRegionIds.Add(id);
                }
            // A visit is itself a chart unlock; repair legacy/incomplete combinations safely.
            foreach (string id in visitedRegionIds) unlockedRegionIds.Add(id);
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.world == null || data.progress == null ||
                data.world.visitedRegionIds == null || data.progress.unlockedRegionIds == null)
            {
                error = "The regional chart save collections are missing.";
                return false;
            }
            if (data.world.visitedRegionIds.Count > SaveGameDataMigration.MaximumCollectionEntries ||
                data.progress.unlockedRegionIds.Count > SaveGameDataMigration.MaximumCollectionEntries)
            {
                error = "The regional chart contains too many region records.";
                return false;
            }
            for (int i = 0; i < data.world.visitedRegionIds.Count; i++)
                if (string.IsNullOrWhiteSpace(data.world.visitedRegionIds[i]) ||
                    data.world.visitedRegionIds[i].Length > 128)
                {
                    error = "The regional chart contains an invalid visited region ID.";
                    return false;
                }
            for (int i = 0; i < data.progress.unlockedRegionIds.Count; i++)
                if (string.IsNullOrWhiteSpace(data.progress.unlockedRegionIds[i]) ||
                    data.progress.unlockedRegionIds[i].Length > 128)
                {
                    error = "The regional chart contains an invalid unlocked region ID.";
                    return false;
                }
            return true;
        }

        public static string GetDisplayName(string regionId)
        {
            switch (RegionalChartPolicy.NormalizeId(regionId))
            {
                case "greyhaven": return "Greyhaven";
                case "the_shallows": return "The Shallows";
                case "blackrock_reach": return "Blackrock Reach";
                case "the_drowned_parish": return "The Drowned Parish";
                case "the_outer_shoals": return "The Outer Shoals";
                case "the_hollow_trench": return "The Hollow Trench";
                default: return string.IsNullOrWhiteSpace(regionId) ? "Uncharted Waters" : "Unknown Waters";
            }
        }
    }
}
