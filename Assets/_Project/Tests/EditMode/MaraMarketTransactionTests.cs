using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class MaraMarketTransactionTests
    {
        private GameObject owner;
        private SpatialCargoHold hold;
        private CargoItemDefinition definition;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Market transaction test");
            hold = owner.AddComponent<SpatialCargoHold>();
            definition = ScriptableObject.CreateInstance<CargoItemDefinition>();
            SetPrivateField(definition, "itemId", "TEST-FISH");
            SetPrivateField(definition, "displayName", "Test Fish");
            SetPrivateField(definition, "occupiedCells", new[] { Vector2Int.zero });
            hold.SetItemDefinitions(new[] { definition });
        }

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
            if (definition != null) Object.DestroyImmediate(definition);
        }

        [Test]
        public void FailedCommitLeavesEverySelectedCargoInstanceUntouched()
        {
            string first;
            string second;
            Assert.IsTrue(hold.TryPlace("TEST-FISH", 0, 0, 0, 1, 1f, 1f, out first));
            Assert.IsTrue(hold.TryPlace("TEST-FISH", 1, 0, 0, 1, 1f, 1f, out second));

            List<CargoPlacement> removed;
            bool result = hold.TryCommitRemoval(new[] { first, second }, () => false, out removed);

            Assert.IsFalse(result);
            Assert.IsNull(removed);
            Assert.AreEqual(2, hold.Placements.Count);
        }

        [Test]
        public void SuccessfulCommitRemovesAllSelectedCargoTogether()
        {
            string first;
            string second;
            Assert.IsTrue(hold.TryPlace("TEST-FISH", 0, 0, 0, 1, 1f, 1f, out first));
            Assert.IsTrue(hold.TryPlace("TEST-FISH", 1, 0, 0, 1, 1f, 1f, out second));
            int commitCount = 0;

            List<CargoPlacement> removed;
            bool result = hold.TryCommitRemoval(new[] { first, second }, () =>
            {
                commitCount++;
                return true;
            }, out removed);

            Assert.IsTrue(result);
            Assert.AreEqual(1, commitCount);
            Assert.AreEqual(2, removed.Count);
            Assert.AreEqual(0, hold.Placements.Count);
        }

        [Test]
        public void DuplicateOrMissingCargoReferenceRejectsWholeCommit()
        {
            string first;
            Assert.IsTrue(hold.TryPlace("TEST-FISH", 0, 0, 0, 1, 1f, 1f, out first));
            List<CargoPlacement> removed;
            bool called = false;

            Assert.IsFalse(hold.TryCommitRemoval(new[] { first, first }, () =>
            {
                called = true;
                return true;
            }, out removed));
            Assert.IsFalse(called);
            Assert.AreEqual(1, hold.Placements.Count);

            Assert.IsFalse(hold.TryCommitRemoval(new[] { first, "missing" }, () =>
            {
                called = true;
                return true;
            }, out removed));
            Assert.IsFalse(called);
            Assert.AreEqual(1, hold.Placements.Count);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "Expected serialized field " + fieldName);
            field.SetValue(target, value);
        }
    }
}
