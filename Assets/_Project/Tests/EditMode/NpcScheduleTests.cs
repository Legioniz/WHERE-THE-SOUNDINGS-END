using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.NPC;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class NpcScheduleTests
    {
        [TestCase(0, NpcRelationshipTier.Hostile)]
        [TestCase(14, NpcRelationshipTier.Hostile)]
        [TestCase(15, NpcRelationshipTier.Distrustful)]
        [TestCase(29, NpcRelationshipTier.Distrustful)]
        [TestCase(30, NpcRelationshipTier.Wary)]
        [TestCase(44, NpcRelationshipTier.Wary)]
        [TestCase(45, NpcRelationshipTier.Neutral)]
        [TestCase(59, NpcRelationshipTier.Neutral)]
        [TestCase(60, NpcRelationshipTier.Familiar)]
        [TestCase(74, NpcRelationshipTier.Familiar)]
        [TestCase(75, NpcRelationshipTier.Trusted)]
        [TestCase(89, NpcRelationshipTier.Trusted)]
        [TestCase(90, NpcRelationshipTier.Ally)]
        [TestCase(100, NpcRelationshipTier.Ally)]
        public void RelationshipRules_ResolveExpectedTier(int score, NpcRelationshipTier expected)
        {
            Assert.AreEqual(expected, NpcRelationshipRules.ResolveTier(score));
        }

        [Test]
        public void RelationshipRules_EnforceConversationAndTradeThresholds()
        {
            Assert.IsFalse(NpcRelationshipRules.CanTalk(14));
            Assert.IsTrue(NpcRelationshipRules.CanTalk(15));
            Assert.IsFalse(NpcRelationshipRules.CanTrade(29));
            Assert.IsTrue(NpcRelationshipRules.CanTrade(30));
            Assert.AreEqual(0f, NpcRelationshipRules.GetBuyPriceMultiplier(29));
            Assert.AreEqual(0.85f, NpcRelationshipRules.GetBuyPriceMultiplier(30));
            Assert.AreEqual(1.15f, NpcRelationshipRules.GetBuyPriceMultiplier(90));
        }

        [Test]
        public void RelationshipEvents_RejectMalformedIdsAndOutOfRangeDeltas()
        {
            GameObject gameObject = new GameObject("NPC_Relationship_Guard_Test");
            try
            {
                NpcAgent agent = gameObject.AddComponent<NpcAgent>();
                Assert.IsFalse(agent.TryApplyRelationshipEvent("", 1));
                Assert.IsFalse(agent.TryApplyRelationshipEvent("   ", 1));
                Assert.IsFalse(agent.TryApplyRelationshipEvent(" leading.space", 1));
                Assert.IsFalse(agent.TryApplyRelationshipEvent("event.too.large", 21));
                Assert.AreEqual(50, agent.RelationshipScore);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

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
            Assert.AreEqual(1, NpcScheduleResolver.ResolveIndex(schedule, 3.99f));
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
                Assert.IsTrue(agent.TryApplyRelationshipEvent("test.returned_borrowed_line", 4));
                Assert.IsFalse(agent.TryApplyRelationshipEvent("test.returned_borrowed_line", 4));
                Assert.AreEqual(41, agent.RelationshipScore);

                SaveGameData save = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
                agent.CaptureSaveData(save);

                Assert.AreEqual(1, save.progress.npcStates.Count);
                Assert.AreEqual("greyhaven.test_resident", save.progress.npcStates[0].npcId);
                Assert.AreEqual("41", save.progress.npcStates[0].relationshipId);
                Assert.AreEqual("greyhaven", save.progress.npcStates[0].lastKnownRegionId);
                CollectionAssert.Contains(save.progress.npcStates[0].relationshipMemoryIds, "test.returned_borrowed_line");

                agent.SetRelationshipScore(0);
                agent.RestoreSaveData(save);
                Assert.AreEqual(41, agent.RelationshipScore);
                CollectionAssert.Contains(agent.RelationshipMemoryIds, "test.returned_borrowed_line");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
