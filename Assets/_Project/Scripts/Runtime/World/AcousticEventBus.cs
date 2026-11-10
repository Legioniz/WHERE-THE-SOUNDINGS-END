using System;

namespace WhereTheSoundingsEnd.World
{
    public enum AcousticEventKind
    {
        SoundingPulse,
        EngineNoise,
        HullImpact,
        FishingGearNoise,
        WinchNoise,
        AnimalCall,
        EnvironmentalReturn
    }

    /// <summary>
    /// Immutable gameplay sound metadata. Audio playback is a presentation concern;
    /// perception systems subscribe here rather than scanning AudioSources every frame.
    /// </summary>
    public readonly struct AcousticSignal
    {
        public readonly string EventId;
        public readonly AcousticEventKind Kind;
        public readonly string RegionId;
        public readonly UnityEngine.Vector3 Position;
        public readonly float Loudness;
        public readonly float RadiusMeters;
        public readonly double GameTimestamp;

        public AcousticSignal(
            string eventId, AcousticEventKind kind, string regionId,
            UnityEngine.Vector3 position, float loudness, float radiusMeters, double gameTimestamp)
        {
            EventId = eventId ?? string.Empty;
            Kind = kind;
            RegionId = regionId ?? string.Empty;
            Position = position;
            Loudness = UnityEngine.Mathf.Clamp01(loudness);
            RadiusMeters = UnityEngine.Mathf.Max(0f, radiusMeters);
            GameTimestamp = gameTimestamp;
        }
    }

    /// <summary>
    /// Small scene-independent event bus. Subscribers must unsubscribe on disable/destroy.
    /// Do not use this bus for cosmetic sound playback or per-frame audio updates.
    /// </summary>
    public static class AcousticEventBus
    {
        public static event Action<AcousticSignal> SignalEmitted;

        public static void Publish(AcousticSignal signal)
        {
            Action<AcousticSignal> handler = SignalEmitted;
            if (handler != null) handler.Invoke(signal);
        }

        public static void ClearSubscribersForTests()
        {
            SignalEmitted = null;
        }
    }
}