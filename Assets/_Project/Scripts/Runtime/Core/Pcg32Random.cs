using System;

namespace WhereTheSoundingsEnd.Core
{
    /// <summary>
    /// PCG-XSH-RR 32-bit generator. Keep this algorithm stable after save data depends on it.
    /// </summary>
    public sealed class Pcg32Random
    {
        private const ulong Multiplier = 6364136223846793005UL;
        private ulong _state;
        private readonly ulong _increment;

        public Pcg32Random(ulong seed, ulong stream = 54UL)
        {
            _state = 0UL;
            _increment = unchecked((stream << 1) | 1UL);
            NextUInt();
            _state = unchecked(_state + seed);
            NextUInt();
        }

        public uint NextUInt()
        {
            ulong oldState = _state;
            _state = unchecked(oldState * Multiplier + _increment);
            uint shifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rotation = (int)(oldState >> 59);

            return (shifted >> rotation) |
                   (shifted << ((-rotation) & 31));
        }

        /// <summary>Returns a value in the half-open interval [0, 1).</summary>
        public double NextDouble()
        {
            return NextUInt() * (1.0 / 4294967296.0);
        }

        /// <summary>Returns an unbiased integer in [minInclusive, maxExclusive).</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxExclusive),
                    "The exclusive maximum must be greater than the minimum.");
            }

            uint range = (uint)((long)maxExclusive - minInclusive);
            uint threshold = unchecked(0U - range) % range;
            uint sample;

            do
            {
                sample = NextUInt();
            }
            while (sample < threshold);

            long result = (long)minInclusive + (sample % range);
            return (int)result;
        }
    }
}
