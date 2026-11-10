using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    public enum DeepOceanPhenomenonType
    {
        BiologicalLayer,
        BrinePool,
        HydrothermalVent,
        ColdSeep,
        GasHydrate,
        WhaleFall,
        Bioluminescence,
        WaterMassFront,
        DerelictVessel,
        DamagedFishingGear,
        UnclassifiedAcousticReturn
    }

    /// <summary>
    /// Lightweight authored marker for a discoverable natural or human maritime site.
    /// It provides survey evidence only; physical hazards require a separate interaction controller.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeepOceanPhenomenonSite : MonoBehaviour
    {
        [SerializeField] private string siteId = "site.unconfigured";
        [SerializeField] private string regionId = "the_outer_shoals";
        [SerializeField] private DeepOceanPhenomenonType phenomenonType;
        [SerializeField] private string displayName = "Unclassified return";
        [SerializeField, Min(0f)] private float depthMeters = 100f;
        [SerializeField, Min(1f)] private float detectionRadius = 95f;
        [SerializeField, Range(0f, 1f)] private float signalStrength = 0.75f;
        [SerializeField, TextArea(2, 5)] private string firstSurveyNote =
            "The return is repeatable, but its source has not been identified.";
        [SerializeField] private string evidenceLabel = "Established science";

        public string SiteId => siteId;
        public string RegionId => regionId;
        public DeepOceanPhenomenonType PhenomenonType => phenomenonType;
        public string DisplayName => displayName;
        public float DepthMeters => depthMeters;
        public float DetectionRadius => detectionRadius;
        public float SignalStrength => signalStrength;
        public string FirstSurveyNote => firstSurveyNote;
        public string EvidenceLabel => evidenceLabel;

        public void Configure(
            string id, string region, DeepOceanPhenomenonType type, string title,
            float depth, float radius, float strength, string note, string evidence)
        {
            siteId = string.IsNullOrWhiteSpace(id) ? "site.unconfigured" : id.Trim();
            regionId = string.IsNullOrWhiteSpace(region) ? "unknown_region" : region.Trim().ToLowerInvariant();
            phenomenonType = type;
            displayName = string.IsNullOrWhiteSpace(title) ? "Unclassified return" : title;
            depthMeters = Mathf.Max(0f, depth);
            detectionRadius = Mathf.Max(1f, radius);
            signalStrength = Mathf.Clamp01(strength);
            firstSurveyNote = note ?? string.Empty;
            evidenceLabel = string.IsNullOrWhiteSpace(evidence) ? "Unclassified" : evidence;
        }

        public bool IsDetectableFrom(Vector3 position, string activeRegionId)
        {
            if (!string.Equals(regionId, activeRegionId, System.StringComparison.OrdinalIgnoreCase))
                return false;
            Vector2 a = new Vector2(position.x, position.z);
            Vector2 b = new Vector2(transform.position.x, transform.position.z);
            return (a - b).sqrMagnitude <= detectionRadius * detectionRadius;
        }
    }
}