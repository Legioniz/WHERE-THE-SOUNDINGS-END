using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Keeps rare unexplained warning cues separate from ordinary incident selection.
    /// The cue is atmospheric only and must never communicate a location or action.
    /// </summary>
    public static class UnexplainedWarningPolicy
    {
        public const float Probability = 0.055f;

        public static bool ShouldTrigger(float roll)
        {
            return !float.IsNaN(roll) && !float.IsInfinity(roll) &&
                   roll >= 0f && roll < Probability;
        }

        public static float RemapIncidentRoll(float roll)
        {
            if (float.IsNaN(roll) || float.IsInfinity(roll))
                return 0f;

            return Mathf.Clamp01((roll - Probability) / (1f - Probability));
        }
    }
}
