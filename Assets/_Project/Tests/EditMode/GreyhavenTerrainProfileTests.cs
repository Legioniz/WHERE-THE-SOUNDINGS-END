using NUnit.Framework;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class GreyhavenTerrainProfileTests
    {
        [Test]
        public void Shoreline_IsDeterministicAndStaysWithinTheBayEnvelope()
        {
            for (int x = -110; x <= 110; x += 10)
            {
                float first = GreyhavenTerrainProfile.ShorelineZ(x);
                float second = GreyhavenTerrainProfile.ShorelineZ(x);

                Assert.That(first, Is.EqualTo(second));
                Assert.That(first, Is.InRange(-25f, 3f));
            }
        }

        [Test]
        public void Heightfield_ProducesDistinctHeadlandsAndAnInlandSpine()
        {
            float westHeadland = GreyhavenTerrainProfile.HeightAt(-72f, -62f);
            float eastRidge = GreyhavenTerrainProfile.HeightAt(70f, -71f);
            float inlandSpine = GreyhavenTerrainProfile.HeightAt(4f, -99f);
            float coastalShelf = GreyhavenTerrainProfile.HeightAt(
                0f, GreyhavenTerrainProfile.ShorelineZ(0f) - 1f);

            Assert.That(westHeadland, Is.GreaterThan(8f));
            Assert.That(eastRidge, Is.GreaterThan(6f));
            Assert.That(inlandSpine, Is.GreaterThan(4f));
            Assert.That(coastalShelf, Is.LessThan(1.5f));
        }

        [Test]
        public void Heightfield_SamplesRemainFiniteAndWithinConfiguredElevationBounds()
        {
            for (int x = -110; x <= 110; x += 11)
            {
                float shore = GreyhavenTerrainProfile.ShorelineZ(x);
                for (int step = 0; step <= 10; step++)
                {
                    float z = -112f + (shore + 112f) * (step / 10f);
                    float height = GreyhavenTerrainProfile.HeightAt(x, z);

                    Assert.That(float.IsNaN(height) || float.IsInfinity(height), Is.False);
                    Assert.That(height, Is.InRange(0.22f, 24f));
                }
            }
        }

        [Test]
        public void SurfaceClassifier_SeparatesBeachVegetationAndExposedRock()
        {
            Assert.That(
                GreyhavenTerrainProfile.ClassifySurface(0.7f, 8f, 5f),
                Is.EqualTo(GreyhavenTerrainSurface.Sand));

            Assert.That(
                GreyhavenTerrainProfile.ClassifySurface(5f, 12f, 30f),
                Is.EqualTo(GreyhavenTerrainSurface.Grass));

            Assert.That(
                GreyhavenTerrainProfile.ClassifySurface(5f, 44f, 30f),
                Is.EqualTo(GreyhavenTerrainSurface.Basalt));

            Assert.That(
                GreyhavenTerrainProfile.ClassifySurface(0.8f, 28f, 30f),
                Is.EqualTo(GreyhavenTerrainSurface.Soil));
        }

        [Test]
        public void InvalidSamples_FallBackToSafeNonNanValues()
        {
            Assert.That(GreyhavenTerrainProfile.HeightAt(float.NaN, 0f), Is.EqualTo(0.26f));
            Assert.That(GreyhavenTerrainProfile.ShorelineZ(float.PositiveInfinity), Is.EqualTo(-18f));
            Assert.That(
                GreyhavenTerrainProfile.ClassifySurface(float.NaN, 0f, 0f),
                Is.EqualTo(GreyhavenTerrainSurface.Soil));
        }
    }
}
