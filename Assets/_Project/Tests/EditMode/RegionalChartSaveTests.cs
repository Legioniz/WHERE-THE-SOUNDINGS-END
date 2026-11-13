using NUnit.Framework;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class RegionalChartSaveTests
    {
        [Test]
        public void SchemaSixSave_MigratesToSevenWithoutInventingVisitedRegions()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            data.schemaVersion = 6;
            data.progress.unlockedRegionIds.Add("the_outer_shoals");

            Assert.That(SaveGameDataMigration.TryPrepare(data, out string error), Is.True, error);
            Assert.That(data.schemaVersion, Is.EqualTo(7));
            Assert.That(data.world.visitedRegionIds, Is.Empty);
            Assert.That(data.progress.unlockedRegionIds, Does.Contain("the_outer_shoals"));
        }

        [Test]
        public void SchemaSevenSave_PreservesVisitedRegionDiscovery()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            data.world.visitedRegionIds.Add("greyhaven");
            data.progress.unlockedRegionIds.Add("greyhaven");

            Assert.That(SaveGameDataMigration.TryPrepare(data, out string error), Is.True, error);
            Assert.That(data.world.visitedRegionIds, Does.Contain("greyhaven"));
            Assert.That(data.progress.unlockedRegionIds, Does.Contain("greyhaven"));
        }

        [Test]
        public void SchemaSevenSave_NormalizesNullVisitedRegionCollection()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            data.world.visitedRegionIds = null;

            Assert.That(SaveGameDataMigration.TryPrepare(data, out string error), Is.True, error);
            Assert.That(data.world.visitedRegionIds, Is.Not.Null);
            Assert.That(data.world.visitedRegionIds, Is.Empty);
        }
    }
}
