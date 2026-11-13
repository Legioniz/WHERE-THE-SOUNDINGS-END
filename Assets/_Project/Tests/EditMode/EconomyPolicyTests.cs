using NUnit.Framework;
using WhereTheSoundingsEnd.Economy;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class EconomyPolicyTests
    {
        [Test]
        public void SaleLine_UsesDocumentedModifiersAndRoundsToCents()
        {
            long cents = EconomyPolicy.CalculateSaleLineCents(
                2m, 2.30m, 1.08m, 1m, 1m, 1m, 1m, true);

            Assert.That(cents, Is.EqualTo(497L));
        }

        [Test]
        public void SaleLine_RejectsIneligibleOrInvalidQuantity()
        {
            Assert.That(EconomyPolicy.CalculateSaleLineCents(
                2m, 2.30m, 1m, 1m, 1m, 1m, 1m, false), Is.EqualTo(0L));
            Assert.That(EconomyPolicy.CalculateSaleLineCents(
                0m, 2.30m, 1m, 1m, 1m, 1m, 1m, true), Is.EqualTo(0L));
        }

        [Test]
        public void SaleLine_ClampsModifiersToEconomyRules()
        {
            long cents = EconomyPolicy.CalculateSaleLineCents(
                1m, 10m, 1m, 9m, 0m, 9m, 9m, true);

            Assert.That(cents, Is.EqualTo(652L));
        }

        [Test]
        public void DemandFactor_IsBoundedAndMovesWithStock()
        {
            Assert.That(EconomyPolicy.CalculateDemandFactor(100, 0, 0.5m), Is.EqualTo(1.35m));
            Assert.That(EconomyPolicy.CalculateDemandFactor(100, 100, 0.5m), Is.EqualTo(1.00m));
            Assert.That(EconomyPolicy.CalculateDemandFactor(100, 500, 0.5m), Is.EqualTo(0.75m));
        }

        [Test]
        public void WalletDelta_RejectsOverspendAndOverflowWithoutMutation()
        {
            long result;
            Assert.That(EconomyPolicy.TryApplyWalletDelta(500L, -501L, out result), Is.False);
            Assert.That(result, Is.EqualTo(500L));
            Assert.That(EconomyPolicy.TryApplyWalletDelta(long.MaxValue, 1L, out result), Is.False);
            Assert.That(result, Is.EqualTo(long.MaxValue));
            Assert.That(EconomyPolicy.TryApplyWalletDelta(500L, -200L, out result), Is.True);
            Assert.That(result, Is.EqualTo(300L));
        }

        [Test]
        public void RepairAndFuelQuotes_AreRoundedToCents()
        {
            Assert.That(EconomyPolicy.CalculateRepairCostCents(2.5m, 1.25m), Is.EqualTo(313L));
            Assert.That(EconomyPolicy.CalculateFuelCostCents(3m, 0.75m), Is.EqualTo(225L));
        }
    }
}
