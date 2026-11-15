using UnityEngine;

namespace WhereTheSoundingsEnd.Streaming
{
    /// <summary>Pure distance rules for area preloading and unload hysteresis.</summary>
    public static class AreaStreamingPolicy
    {
        public static bool IsValidRadii(float preloadRadius, float unloadRadius)
        {
            return IsFinite(preloadRadius) &&
                   IsFinite(unloadRadius) &&
                   preloadRadius > 0f &&
                   unloadRadius > preloadRadius;
        }

        public static bool ShouldPreload(float distance, float preloadRadius)
        {
            return IsFinite(distance) &&
                   IsFinite(preloadRadius) &&
                   distance >= 0f &&
                   preloadRadius > 0f &&
                   distance <= preloadRadius;
        }

        public static bool ShouldUnload(float distance, float unloadRadius)
        {
            return IsFinite(distance) &&
                   IsFinite(unloadRadius) &&
                   distance >= 0f &&
                   unloadRadius > 0f &&
                   distance > unloadRadius;
        }

        /// <summary>Disables area rendering outside preload range while retaining loaded scene assets for quick re-entry.</summary>
        public static bool ShouldStandby(float distance, float preloadRadius)
        {
            return IsFinite(distance) && IsFinite(preloadRadius) &&
                   distance >= 0f && preloadRadius > 0f && distance > preloadRadius;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
