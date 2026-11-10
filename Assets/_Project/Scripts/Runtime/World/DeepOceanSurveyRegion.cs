using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Survey profile attached to a region's water/world root.
    /// Values are authored approximations for gameplay, not navigational chart data.
    /// Add one to each playable region root and configure it for that region.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeepOceanSurveyRegion : MonoBehaviour
    {
        [Header("Identity and bounds")]
        [SerializeField] private string regionId = "greyhaven";
        [SerializeField, Min(1f)] private float surveyRadius = 170f;

        [Header("Bathymetry")]
        [SerializeField, Min(1f)] private float nominalSeabedDepthMeters = 18f;
        [SerializeField, Min(0f)] private float seabedVariationMeters = 3f;

        [Header("Deep scattering layer")]
        [SerializeField] private bool hasScatteringLayer;
        [SerializeField, Min(1f)] private float dayLayerDepthMeters = 420f;
        [SerializeField, Min(1f)] private float nightLayerDepthMeters = 170f;
        [SerializeField, Range(0f, 1f)] private float layerDensity = 0.8f;

        [Header("Acoustic conditions")]
        [SerializeField, Range(0f, 1f)] private float acousticAttenuation = 0.12f;
        [SerializeField, Range(0f, 1f)] private float backgroundNoise = 0.08f;

        public string RegionId => regionId;
        public float SurveyRadius => surveyRadius;
        public float NominalSeabedDepthMeters => nominalSeabedDepthMeters;
        public float SeabedVariationMeters => seabedVariationMeters;
        public bool HasScatteringLayer => hasScatteringLayer;
        public float LayerDensity => layerDensity;
        public float AcousticAttenuation => acousticAttenuation;
        public float BackgroundNoise => backgroundNoise;

        public bool Contains(Vector3 worldPosition)
        {
            Vector2 centre = new Vector2(transform.position.x, transform.position.z);
            Vector2 point = new Vector2(worldPosition.x, worldPosition.z);
            return (point - centre).sqrMagnitude <= surveyRadius * surveyRadius;
        }

        public float GetLayerDepth(float currentHour)
        {
            if (!hasScatteringLayer) return -1f;
            return DeepOceanSurveyMath.GetScatteringLayerDepth(
                currentHour, dayLayerDepthMeters, nightLayerDepthMeters);
        }

        public void ConfigureForRegion(string id)
        {
            regionId = string.IsNullOrWhiteSpace(id) ? "unknown_region" : id.Trim().ToLowerInvariant();
            switch (regionId)
            {
                case "greyhaven":
                    surveyRadius = 170f;
                    nominalSeabedDepthMeters = 18f;
                    seabedVariationMeters = 3f;
                    hasScatteringLayer = false;
                    layerDensity = 0f;
                    acousticAttenuation = 0.16f;
                    backgroundNoise = 0.14f;
                    break;
                case "the_shallows":
                    surveyRadius = 520f;
                    nominalSeabedDepthMeters = 52f;
                    seabedVariationMeters = 24f;
                    hasScatteringLayer = false;
                    layerDensity = 0f;
                    acousticAttenuation = 0.2f;
                    backgroundNoise = 0.16f;
                    break;
                case "blackrock_reach":
                    surveyRadius = 780f;
                    nominalSeabedDepthMeters = 210f;
                    seabedVariationMeters = 110f;
                    hasScatteringLayer = false;
                    layerDensity = 0f;
                    acousticAttenuation = 0.28f;
                    backgroundNoise = 0.25f;
                    break;
                case "the_drowned_parish":
                    surveyRadius = 640f;
                    nominalSeabedDepthMeters = 34f;
                    seabedVariationMeters = 18f;
                    hasScatteringLayer = false;
                    layerDensity = 0f;
                    acousticAttenuation = 0.32f;
                    backgroundNoise = 0.2f;
                    break;
                case "the_outer_shoals":
                    surveyRadius = 1400f;
                    nominalSeabedDepthMeters = 1450f;
                    seabedVariationMeters = 620f;
                    hasScatteringLayer = true;
                    dayLayerDepthMeters = 520f;
                    nightLayerDepthMeters = 190f;
                    layerDensity = 0.82f;
                    acousticAttenuation = 0.34f;
                    backgroundNoise = 0.22f;
                    break;
                case "the_hollow_trench":
                    surveyRadius = 1800f;
                    nominalSeabedDepthMeters = 3850f;
                    seabedVariationMeters = 1250f;
                    hasScatteringLayer = true;
                    dayLayerDepthMeters = 690f;
                    nightLayerDepthMeters = 250f;
                    layerDensity = 0.9f;
                    acousticAttenuation = 0.48f;
                    backgroundNoise = 0.3f;
                    break;
                default:
                    surveyRadius = 300f;
                    nominalSeabedDepthMeters = 100f;
                    seabedVariationMeters = 40f;
                    hasScatteringLayer = false;
                    layerDensity = 0f;
                    acousticAttenuation = 0.25f;
                    backgroundNoise = 0.18f;
                    break;
            }
        }

        public void ConfigureCustom(
            string id, float radius, float seabedDepth, float seabedVariation,
            bool scatteringLayer, float dayLayerDepth, float nightLayerDepth,
            float density, float attenuation, float noise)
        {
            regionId = string.IsNullOrWhiteSpace(id) ? "unknown_region" : id.Trim().ToLowerInvariant();
            surveyRadius = Mathf.Max(1f, radius);
            nominalSeabedDepthMeters = Mathf.Max(1f, seabedDepth);
            seabedVariationMeters = Mathf.Max(0f, seabedVariation);
            hasScatteringLayer = scatteringLayer;
            dayLayerDepthMeters = Mathf.Max(1f, dayLayerDepth);
            nightLayerDepthMeters = Mathf.Max(1f, nightLayerDepth);
            layerDensity = Mathf.Clamp01(density);
            acousticAttenuation = Mathf.Clamp01(attenuation);
            backgroundNoise = Mathf.Clamp01(noise);
        }
    }
}