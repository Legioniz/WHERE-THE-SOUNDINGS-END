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
        public bool IsValidForRegion => DeepOceanPhenomenonRules.IsAllowed(phenomenonType, regionId, depthMeters);

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
            if (!IsValidForRegion || !string.Equals(regionId, activeRegionId, System.StringComparison.OrdinalIgnoreCase))
                return false;
            Vector2 a = new Vector2(position.x, position.z);
            Vector2 b = new Vector2(transform.position.x, transform.position.z);
            return (a - b).sqrMagnitude <= detectionRadius * detectionRadius;
        }
    }

    public enum HistoricalArtifactType
    {
        Letter,
        Logbook,
        Map,
        Photograph,
        Tool,
        SurveyEquipment
    }

    /// <summary>
    /// Stable, close-range archival object. Visual props are generated separately and remain
    /// collider-free; this anchor owns readable evidence and the inspection identity only.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HistoricalArtifact : MonoBehaviour
    {
        [SerializeField] private string artifactId = string.Empty;
        [SerializeField] private string regionId = "greyhaven";
        [SerializeField] private HistoricalArtifactType artifactType;
        [SerializeField] private string displayName = "Uncatalogued object";
        [SerializeField] private string dateOrPeriod = "Date uncertain";
        [SerializeField, TextArea(1, 3)] private string conditionSummary = "Condition not assessed.";
        [SerializeField, TextArea(1, 3)] private string provenance = "Origin unverified.";
        [SerializeField, TextArea(3, 8)] private string readingText = string.Empty;

        public string ArtifactId => artifactId;
        public string RegionId => regionId;
        public HistoricalArtifactType ArtifactType => artifactType;
        public string DisplayName => displayName;
        public string DateOrPeriod => dateOrPeriod;
        public string ConditionSummary => conditionSummary;
        public string Provenance => provenance;
        public string ReadingText => readingText;
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(artifactId) && artifactId.Length <= 256 &&
            !string.IsNullOrWhiteSpace(regionId) && regionId.Length <= 128 &&
            !string.IsNullOrWhiteSpace(displayName);

        public void Configure(
            string id, string region, HistoricalArtifactType type, string title,
            string period, string condition, string source, string transcriptionOrNote)
        {
            artifactId = string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();
            regionId = string.IsNullOrWhiteSpace(region) ? string.Empty : region.Trim().ToLowerInvariant();
            artifactType = type;
            displayName = string.IsNullOrWhiteSpace(title) ? "Uncatalogued object" : title.Trim();
            dateOrPeriod = string.IsNullOrWhiteSpace(period) ? "Date uncertain" : period.Trim();
            conditionSummary = string.IsNullOrWhiteSpace(condition) ? "Condition not assessed." : condition.Trim();
            provenance = string.IsNullOrWhiteSpace(source) ? "Origin unverified." : source.Trim();
            readingText = transcriptionOrNote ?? string.Empty;
        }

        public string BuildInspectionText(bool previouslyInspected)
        {
            string heading = previouslyInspected
                ? "PREVIOUSLY EXAMINED: "
                : "HISTORICAL ARTIFACT RECORDED: ";
            return heading + displayName +
                " [" + artifactType.ToString().ToUpperInvariant() + "]\n" +
                dateOrPeriod + " — " + conditionSummary + "\n" +
                "Provenance: " + provenance + "\n" + readingText;
        }
    }


}