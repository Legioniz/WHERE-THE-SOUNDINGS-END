using UnityEngine;
using WhereTheSoundingsEnd.Inventory;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>Deterministic rules shared by the fishing runtime and Edit Mode tests.</summary>
    public static class FishingSimulationPolicy
    {
        public static bool IsTechniqueCompatible(FishPreferredTechnique preferred, FishingTechnique actual)
        {
            return preferred == FishPreferredTechnique.Any ||
                   (int)preferred == (int)actual + 1;
        }

        public static float CalculateAberrationChance(float panicLevel, float seaDisturbance)
        {
            float panic = Mathf.Clamp01(panicLevel);
            float disturbance = Mathf.Clamp01(seaDisturbance);
            return Mathf.Clamp(0.025f + panic * 0.22f + disturbance * 0.08f, 0.025f, 0.325f);
        }

        /// <summary>
        /// Converts a per-second event rate into a frame-duration probability using the
        /// exponential survival model. Splitting a time interval across frames preserves
        /// the same cumulative event probability.
        /// </summary>
        public static float CalculateEventChance(float eventsPerSecond, float deltaSeconds)
        {
            if (eventsPerSecond <= 0f || deltaSeconds <= 0f) return 0f;
            return Mathf.Clamp01(1f - Mathf.Exp(-eventsPerSecond * deltaSeconds));
        }

        /// <summary>Returns a relative candidate weight, not an absolute percentage.</summary>
        public static float CalculateCandidateWeight(
            float population, bool techniqueMatches, bool baitMatches, bool isAberrant)
        {
            if (population <= 0f) return 0f;
            float weight = population;
            if (!techniqueMatches) weight *= 0.35f;
            if (!baitMatches) weight *= 0.28f;
            if (isAberrant) weight *= 0.15f;
            return weight;
        }

        /// <summary>
        /// Converts an audible event into local ecological disturbance. Loudness and distance
        /// are bounded so a distant or quiet signal cannot empty a fishing ground.
        /// </summary>
        public static float CalculateAcousticDisturbanceGain(float loudness, float distance, float effectiveRadius)
        {
            if (effectiveRadius <= 0f || distance < 0f || distance > effectiveRadius) return 0f;
            float proximity = 1f - Mathf.Clamp01(distance / effectiveRadius);
            return Mathf.Clamp01(loudness) * proximity * 0.22f;
        }

        /// <summary>Exponential half-life decay; independent of frame rate.</summary>
        public static float DecayDisturbance(float disturbance, float elapsedSeconds, float halfLifeSeconds)
        {
            float current = Mathf.Clamp01(disturbance);
            if (current <= 0f || elapsedSeconds <= 0f) return current;
            if (halfLifeSeconds <= 0f) return 0f;
            return Mathf.Clamp01(current * Mathf.Pow(0.5f, elapsedSeconds / halfLifeSeconds));
        }

        public static float CalculateDisturbanceAvoidanceChance(float disturbance)
        {
            return Mathf.Clamp01(disturbance) * 0.65f;
        }

        public static float CalculateDisturbedBiteWaitMultiplier(float disturbance)
        {
            return Mathf.Clamp(1f + Mathf.Clamp01(disturbance) * 0.8f, 1f, 1.8f);
        }

        public static float CalculateTimingWindow(float baseWindow, float resistance, float tackleControl)
        {
            float difficultyPenalty = Mathf.Clamp(resistance, 0f, 3f) * 0.025f;
            float equipmentBonus = (Mathf.Clamp(tackleControl, 0.5f, 1.5f) - 1f) * 0.035f;
            return Mathf.Clamp(baseWindow - difficultyPenalty + equipmentBonus, 0.055f, 0.18f);
        }

        public static int CalculateMigrationAmount(int population, float migrationRate, float roll)
        {
            if (population <= 1 || migrationRate <= 0f) return 0;
            float expectedAmount = population * Mathf.Clamp01(migrationRate);
            int amount = Mathf.FloorToInt(expectedAmount);
            float fractionalPart = expectedAmount - amount;
            if (roll < fractionalPart) amount++;
            return Mathf.Clamp(amount, 0, Mathf.Max(1, population / 3));
        }

        public static float CalculateCatchQuality(int successfulBeats, float finalTension)
        {
            float timingContribution = Mathf.Clamp(successfulBeats, 0, 9) * 0.065f;
            float overloadPenalty = Mathf.Max(0f, finalTension - 0.65f) * 0.18f;
            return Mathf.Clamp01(0.45f + timingContribution - overloadPenalty);
        }

        public static bool IsWithinDepth(float depthMetres, float minimumDepth, float maximumDepth)
        {
            return depthMetres >= Mathf.Max(0f, minimumDepth) &&
                   depthMetres <= Mathf.Max(minimumDepth, maximumDepth);
        }

        public static float CalculateCatchSize(float minimumSize, float maximumSize, float quality, float sizeRoll)
        {
            float low = Mathf.Max(0.1f, minimumSize);
            float high = Mathf.Max(low, maximumSize);
            return Mathf.Lerp(low, high, Mathf.Clamp01(quality * Mathf.Clamp01(sizeRoll)));
        }
    }
}
