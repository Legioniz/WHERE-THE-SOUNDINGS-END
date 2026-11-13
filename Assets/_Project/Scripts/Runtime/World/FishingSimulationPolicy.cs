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

        public static float CalculateTimingWindow(float baseWindow, float resistance, float tackleControl)
        {
            float difficultyPenalty = Mathf.Clamp(resistance, 0f, 3f) * 0.025f;
            float equipmentBonus = (Mathf.Clamp(tackleControl, 0.5f, 1.5f) - 1f) * 0.035f;
            return Mathf.Clamp(baseWindow - difficultyPenalty + equipmentBonus, 0.055f, 0.18f);
        }

        public static int CalculateMigrationAmount(int population, float migrationRate)
        {
            if (population <= 1 || migrationRate <= 0f) return 0;
            int amount = Mathf.FloorToInt(population * Mathf.Clamp01(migrationRate));
            if (amount == 0 && population >= 8) amount = 1;
            return Mathf.Clamp(amount, 0, population / 3);
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
