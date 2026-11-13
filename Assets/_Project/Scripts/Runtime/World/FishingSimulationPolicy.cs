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
