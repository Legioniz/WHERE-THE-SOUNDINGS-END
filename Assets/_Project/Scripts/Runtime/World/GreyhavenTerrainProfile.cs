using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    public enum GreyhavenTerrainSurface
    {
        Soil = 0,
        Sand = 1,
        Grass = 2,
        Basalt = 3
    }

    /// <summary>
    /// Deterministic, shared heightfield profile for the Greyhaven island blockout.
    /// The profile describes the prototype landform only; it is not chart-grade geography.
    /// </summary>
    public static class GreyhavenTerrainProfile
    {
        public const int GridColumns = 144;
        public const int GridRows = 96;
        public const float MinimumX = -110f;
        public const float MaximumX = 110f;
        public const float BackZ = -112f;

        public static float ShorelineZ(float x)
        {
            if (!IsFinite(x)) return -18f;

            float broadCoast = (Mathf.PerlinNoise((x + 320f) * 0.016f, 0.43f) - 0.5f) * 3.1f;
            float headlandDetail = (Mathf.PerlinNoise((x + 91f) * 0.052f, 0.81f) - 0.5f) * 1.1f;
            return -18f
                   + 13f * Mathf.Exp(-(x * x) / 1800f)
                   + 1.8f * Mathf.Sin(x * 0.045f)
                   + broadCoast
                   + headlandDetail;
        }

        public static float InlandDistance(float x, float z)
        {
            if (!IsFinite(x) || !IsFinite(z)) return 0f;
            return ShorelineZ(x) - z;
        }

        public static float HeightAt(float x, float z)
        {
            if (!IsFinite(x) || !IsFinite(z)) return 0.26f;

            float inlandDistance = Mathf.Max(0f, InlandDistance(x, z));
            float coastalBlend = Mathf.SmoothStep(0f, 22f, inlandDistance);

            float westHeadland = 15.5f * Gaussian(x, -72f, 29f, z, -62f, 43f);
            float eastRidge = 12f * Gaussian(x, 70f, 32f, z, -71f, 47f);
            float inlandSpine = 8.5f * Gaussian(x, 4f, 48f, z, -99f, 23f);
            float rearTerrace = 4.2f * Gaussian(x, -8f, 88f, z, -109f, 17f);

            float broadNoise = (Mathf.PerlinNoise((x + 310f) * 0.014f, (z + 290f) * 0.014f) - 0.5f) * 3.4f;
            float mediumNoise = (Mathf.PerlinNoise((x + 80f) * 0.048f, (z + 160f) * 0.048f) - 0.5f) * 1.15f;
            float fineNoise = (Mathf.PerlinNoise((x + 240f) * 0.105f, (z + 60f) * 0.105f) - 0.5f) * 0.35f;

            float ridgeSignal = 1f - Mathf.Abs(
                2f * Mathf.PerlinNoise((x + 520f) * 0.023f, (z + 410f) * 0.023f) - 1f);
            float ridgedDetail = Mathf.Pow(ridgeSignal, 2.2f) * 1.45f;

            float elevation = 0.26f + coastalBlend *
                (westHeadland + eastRidge + inlandSpine + rearTerrace
                 + broadNoise + mediumNoise + fineNoise + ridgedDetail);

            return Mathf.Clamp(elevation, 0.22f, 24f);
        }

        public static GreyhavenTerrainSurface ClassifySurface(
            float elevation,
            float slopeDegrees,
            float inlandDistance)
        {
            if (!IsFinite(elevation) || !IsFinite(slopeDegrees) || !IsFinite(inlandDistance))
                return GreyhavenTerrainSurface.Soil;

            if (slopeDegrees >= 39f || elevation >= 13.5f)
                return GreyhavenTerrainSurface.Basalt;

            if (inlandDistance <= 12f && elevation <= 1.8f && slopeDegrees <= 22f)
                return GreyhavenTerrainSurface.Sand;

            if (elevation >= 1.8f && slopeDegrees <= 34f)
                return GreyhavenTerrainSurface.Grass;

            return GreyhavenTerrainSurface.Soil;
        }

        private static float Gaussian(float x, float centreX, float spreadX, float z, float centreZ, float spreadZ)
        {
            float dx = (x - centreX) / spreadX;
            float dz = (z - centreZ) / spreadZ;
            return Mathf.Exp(-(dx * dx) - (dz * dz));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
