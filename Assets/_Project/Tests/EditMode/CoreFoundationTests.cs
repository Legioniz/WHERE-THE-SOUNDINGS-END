using System;
using NUnit.Framework;
using WhereTheSoundingsEnd.Core;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class WorldClockTests
    {
        [Test]
        public void Advance_AppliesTimeScale()
        {
            var clock = new WorldClock { TimeScale = 4.0 };
            clock.Advance(2.5);
            Assert.That(clock.ElapsedSeconds, Is.EqualTo(10.0).Within(0.000001));
        }

        [Test]
        public void ZeroTimeScale_PausesSimulationTime()
        {
            var clock = new WorldClock { TimeScale = 0.0 };
            clock.Advance(30.0);
            Assert.That(clock.ElapsedSeconds, Is.Zero);
        }

        [Test]
        public void Advance_RejectsNegativeElapsedTime()
        {
            var clock = new WorldClock();
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(-0.1));
        }

        [Test]
        public void TimeScale_RejectsNonFiniteValues()
        {
            var clock = new WorldClock();
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.TimeScale = double.NaN);
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.TimeScale = double.PositiveInfinity);
        }
    }

    public sealed class Pcg32RandomTests
    {
        [Test]
        public void SameSeedAndStream_ProduceSameSequence()
        {
            var first = new Pcg32Random(123456UL, 42UL);
            var second = new Pcg32Random(123456UL, 42UL);

            for (int index = 0; index < 100; index++)
            {
                Assert.That(first.NextUInt(), Is.EqualTo(second.NextUInt()));
            }
        }

        [Test]
        public void DifferentStreams_ProduceDifferentSequences()
        {
            var first = new Pcg32Random(123456UL, 1UL);
            var second = new Pcg32Random(123456UL, 2UL);
            Assert.That(first.NextUInt(), Is.Not.EqualTo(second.NextUInt()));
        }

        [Test]
        public void NextDouble_StaysWithinHalfOpenUnitInterval()
        {
            var random = new Pcg32Random(7UL);

            for (int index = 0; index < 1000; index++)
            {
                double value = random.NextDouble();
                Assert.That(value, Is.GreaterThanOrEqualTo(0.0));
                Assert.That(value, Is.LessThan(1.0));
            }
        }

        [Test]
        public void NextInt_StaysWithinRequestedRange()
        {
            var random = new Pcg32Random(99UL);

            for (int index = 0; index < 1000; index++)
            {
                int value = random.NextInt(-3, 8);
                Assert.That(value, Is.InRange(-3, 7));
            }
        }

        [Test]
        public void NextInt_RejectsAnEmptyRange()
        {
            var random = new Pcg32Random(1UL);
            Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(5, 5));
        }
    }
}
