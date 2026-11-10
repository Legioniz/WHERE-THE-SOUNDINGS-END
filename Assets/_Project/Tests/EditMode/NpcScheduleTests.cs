using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.NPC;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class NpcScheduleTests
    {
        [Test]
        public void ScheduleResolver_SelectsDayAndOvernightActivities()
        {
            List<NpcScheduleEntry> schedule = new List<NpcScheduleEntry>
            {
                new NpcScheduleEntry { startHour = 4f, endHour = 16f, activityId = "market_shift" },
                new NpcScheduleEntry { startHour = 16f, endHour = 4f, activityId = "off_duty" }
            };

            Assert.AreEqual(0, NpcScheduleResolver.ResolveIndex(schedule, 4f));
            Assert.AreEqual(0, NpcScheduleResolver.ResolveIndex(schedule, 12.5f));
            Assert.AreEqual(1, NpcScheduleResolver.ResolveIndex(schedule, 16f));
            Assert.AreEqual(1, NpcScheduleResolver.ResolveIndex(schedule, 23.5f));
            Assert.AreEqual(0, NpcScheduleResolver.ResolveIndex(schedule, 3.99f));
        }

        [Test]
        public void ScheduleResolver_ReturnsMinusOneWhenNoEntryCoversHour()
        {
            List<NpcScheduleEntry> schedule = new List<NpcScheduleEntry>
            {
                new NpcScheduleEntry { startHour = 8f, endHour = 12f, activityId = "morning_work" }
            };

            Assert.AreEqual(-1, NpcScheduleResolver.ResolveIndex(schedule, 18f));
        }

        [Test]
        public void NpcAgent_PersistsStableIdentityAndRelationship()
        {
            GameObject gameObject = new GameObject("NPC_Test");
            try
            {
                NpcAgent agent = gameObject.AddComponent<NpcAgent>();
                agent.Configure("greyhaven.test_resident", "Test Resident", "resident",
                    "greyhaven", null, new List<NpcScheduleEntry>());
                agent.SetRelationshipScore(37);

                SaveGameData save = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
                agent.CaptureSaveData(save);

                Assert.AreEqual(1, save.progress.npcStates.Count);
                Assert.AreEqual("greyhaven.test_resident", save.progress.npcStates[0].npcId);
                Assert.AreEqual("37", save.progress.npcStates[0].relationshipId);
                Assert.AreEqual("greyhaven", save.progress.npcStates[0].lastKnownRegionId);

                agent.SetRelationshipScore(0);
                agent.RestoreSaveData(save);
                Assert.AreEqual(37, agent.RelationshipScore);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
