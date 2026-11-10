using System;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Pure calculations shared by sonar presentation, ecology and Edit Mode tests.
    /// The scattering layer is a biological water-column return, never a terrain surface.
    /// </summary>
    public static class DeepOceanSurveyMath
    {
        /// <summary>
        /// Returns a smooth 0..1 night factor. Dawn and dusk are transitions, not instant switches.
        /// The profile is intentionally simple until region-specific photoperiod data exists.
        /// </summary>
        public static float GetNightWeight(float hour)
        {
            hour = NormalizeHour(hour);
            return (float)(0.5 + 0.5 * Math.Cos(hour / 24.0 * Math.PI * 2.0));
        }

        public static float GetScatteringLayerDepth(float hour, float dayDepthMeters, float nightDepthMeters)
        {
            float nightWeight = GetNightWeight(hour);
            return dayDepthMeters + (nightDepthMeters - dayDepthMeters) * nightWeight;
        }

        public static bool IsDistinctMidwaterReturn(float layerDepthMeters, float seabedDepthMeters, float minimumSeparationMeters)
        {
            if (layerDepthMeters <= 0f || seabedDepthMeters <= 0f || minimumSeparationMeters < 0f)
                return false;

            return seabedDepthMeters - layerDepthMeters >= minimumSeparationMeters;
        }

        public static float CalculateReturnConfidence(float density, float signalQuality, float attenuation)
        {
            float usableSignal = Clamp01(signalQuality) * (1f - Clamp01(attenuation));
            return Clamp01(usableSignal * (0.35f + Clamp01(density) * 0.65f));
        }

        public static bool IsIndependentLayerObservation(
            string previousRegionId,
            string currentRegionId,
            float previousHeading,
            float currentHeading,
            double elapsedSeconds,
            double maximumAgeSeconds,
            float minimumHeadingChangeDegrees)
        {
            if (string.IsNullOrWhiteSpace(previousRegionId) ||
                string.IsNullOrWhiteSpace(currentRegionId) ||
                !string.Equals(previousRegionId, currentRegionId, StringComparison.OrdinalIgnoreCase))
                return false;

            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) ||
                elapsedSeconds < 0.0 || elapsedSeconds > maximumAgeSeconds)
                return false;

            return Math.Abs(currentHeading - previousHeading) >= minimumHeadingChangeDegrees &&
                   Math.Abs(MathfDeltaAngle(previousHeading, currentHeading)) >= minimumHeadingChangeDegrees;
        }

        private static float MathfDeltaAngle(float current, float target)
        {
            float delta = (target - current) % 360f;
            if (delta > 180f) delta -= 360f;
            if (delta < -180f) delta += 360f;
            return delta;
        }

        /// <summary>Stable FNV-1a key for a named survey opportunity; independent of runtime RNG order.</summary>
        public static ulong StableKey(string value)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            if (string.IsNullOrEmpty(value)) return hash;

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash = unchecked(hash * prime);
            }
            return hash;
        }

        public static float NormalizeHour(float hour)
        {
            float normalized = hour % 24f;
            return normalized < 0f ? normalized + 24f : normalized;
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}