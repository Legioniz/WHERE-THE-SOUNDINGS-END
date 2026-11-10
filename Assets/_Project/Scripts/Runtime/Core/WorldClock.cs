using System;

namespace WhereTheSoundingsEnd.Core
{
    /// <summary>
    /// Engine-independent elapsed-time primitive. It does not define a day length or calendar.
    /// </summary>
    public sealed class WorldClock
    {
        private double _timeScale = 1.0;

        public double ElapsedSeconds { get; private set; }

        public double TimeScale
        {
            get => _timeScale;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value < 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "Time scale must be finite and cannot be negative.");
                }

                _timeScale = value;
            }
        }

        public event Action<double> Advanced;

        public void Advance(double realSeconds)
        {
            if (double.IsNaN(realSeconds) ||
                double.IsInfinity(realSeconds) ||
                realSeconds < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(realSeconds),
                    "Elapsed real time must be finite and cannot be negative.");
            }

            if (realSeconds == 0.0 || _timeScale == 0.0)
            {
                return;
            }

            ElapsedSeconds += realSeconds * _timeScale;
            Advanced?.Invoke(ElapsedSeconds);
        }

        public void Reset(double elapsedSeconds = 0.0)
        {
            if (double.IsNaN(elapsedSeconds) ||
                double.IsInfinity(elapsedSeconds) ||
                elapsedSeconds < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(elapsedSeconds),
                    "Elapsed time must be finite and cannot be negative.");
            }

            ElapsedSeconds = elapsedSeconds;
            Advanced?.Invoke(ElapsedSeconds);
        }
    }
}
