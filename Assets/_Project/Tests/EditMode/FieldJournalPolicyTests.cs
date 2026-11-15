using NUnit.Framework;
using WhereTheSoundingsEnd.Narrative;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class FieldJournalPolicyTests
    {
        [Test]
        public void CreateEntryId_UsesStableNamespaceAndTrimmedItemId()
        {
            Assert.AreEqual("field-discovery:FSH-101", FieldJournalPolicy.CreateEntryId(" FSH-101 "));
            Assert.AreEqual(string.Empty, FieldJournalPolicy.CreateEntryId(" "));
        }

        [Test]
        public void TryGetItemId_RejectsOtherNamespacesAndBlankSuffixes()
        {
            string itemId;
            Assert.IsFalse(FieldJournalPolicy.TryGetItemId("quest:FSH-101", out itemId));
            Assert.IsFalse(FieldJournalPolicy.TryGetItemId("field-discovery:  ", out itemId));
            Assert.IsTrue(FieldJournalPolicy.TryGetItemId("field-discovery:SALVAGE-ARCHIVE-01", out itemId));
            Assert.AreEqual("SALVAGE-ARCHIVE-01", itemId);
        }

        [Test]
        public void NormalizeRegionId_UsesUnknownFallbackAndStableCase()
        {
            Assert.AreEqual("the_outer_shoals", FieldJournalPolicy.NormalizeRegionId(" THE_OUTER_SHOALS "));
            Assert.AreEqual("unknown", FieldJournalPolicy.NormalizeRegionId(null));
        }
    }
}
