using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
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
