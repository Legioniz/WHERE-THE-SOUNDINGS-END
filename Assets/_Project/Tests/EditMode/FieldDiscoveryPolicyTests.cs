using System.Collections.Generic;
using NUnit.Framework;
using WhereTheSoundingsEnd.Narrative;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class FieldDiscoveryPolicyTests
    {
        [Test]
        public void TryRegister_AcceptsFirstId_AndRejectsDuplicates()
        {
            HashSet<string> discovered = new HashSet<string>();

            Assert.IsTrue(FieldDiscoveryPolicy.TryRegister(discovered, "FSH-101"));
            Assert.IsFalse(FieldDiscoveryPolicy.TryRegister(discovered, "FSH-101"));
            Assert.AreEqual(1, discovered.Count);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void TryRegister_RejectsBlankIds(string itemId)
        {
            Assert.IsFalse(FieldDiscoveryPolicy.TryRegister(new HashSet<string>(), itemId));
        }

        [Test]
        public void IsRegistered_UsesExactStableIds()
        {
            HashSet<string> discovered = new HashSet<string> { "SALVAGE-ARCHIVE-01" };

            Assert.IsTrue(FieldDiscoveryPolicy.IsRegistered(discovered, "SALVAGE-ARCHIVE-01"));
            Assert.IsFalse(FieldDiscoveryPolicy.IsRegistered(discovered, "salvage-archive-01"));
        }
    }
}
