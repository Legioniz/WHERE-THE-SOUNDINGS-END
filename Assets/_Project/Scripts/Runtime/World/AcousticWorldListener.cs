using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>Testable gameplay hearing falloff; not a physical sound-pressure model.</summary>
    public static class AcousticPerceptionMath
    {
        public static float CalculateHearingScore(float distanceMeters, float radiusMeters,
            float loudness, float sensitivity, float backgroundNoise)
        {
            if (float.IsNaN(distanceMeters) || float.IsInfinity(distanceMeters) || distanceMeters < 0f ||
                float.IsNaN(radiusMeters) || float.IsInfinity(radiusMeters) || radiusMeters <= 0f ||
                float.IsNaN(loudness) || float.IsInfinity(loudness) ||
                float.IsNaN(sensitivity) || float.IsInfinity(sensitivity) ||
                float.IsNaN(backgroundNoise) || float.IsInfinity(backgroundNoise))
                return 0f;

            if (distanceMeters >= radiusMeters) return 0f;
            float rangeFactor = 1f - Mathf.Clamp01(distanceMeters / radiusMeters);
            float usableSignal = Mathf.Clamp01(loudness) *
                                 Mathf.Clamp01(sensitivity) *
                                 (1f - Mathf.Clamp01(backgroundNoise));
            return Mathf.Clamp01(rangeFactor * usableSignal);
        }
    }

    /// <summary>
    /// Scene-level acoustic listener. It consumes gameplay signals, applies a small
    /// environmental disturbance, and republishes scored perception events for
    /// creature / navigation listeners. It never polls AudioSources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AcousticWorldListener : MonoBehaviour, ISaveParticipant
    {
        private const int MaximumRememberedEvents = 128;

        [SerializeField] private SeaStateDirector seaState;
        [SerializeField, Min(1f)] private float listeningRadiusMeters = 900f;
        [SerializeField, Range(0f, 1f)] private float sensitivity = 0.9f;
        [SerializeField, Range(0f, 1f)] private float backgroundNoise = 0.18f;
        [SerializeField, Min(0f)] private float pressureGain = 0.12f;
        [SerializeField, Min(0f)] private float pressureDecayPerGameMinute = 0.035f;
        [SerializeField, Range(0f, 1f)] private float minimumHearingScore = 0.04f;

        private readonly HashSet<string> heardEventIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> eventOrder = new Queue<string>();
        private string lastHeardEventId = string.Empty;
        private string lastHeardRegionId = string.Empty;
        private Vector3 lastHeardPosition;
        private double lastHeardGameTimestamp;
        private float acousticPressure;

        public event Action<AcousticSignal, float> SignalHeard;

        public float AcousticPressure => acousticPressure;
        public string LastHeardEventId => lastHeardEventId;
        public string LastHeardRegionId => lastHeardRegionId;
        public Vector3 LastHeardPosition => lastHeardPosition;
        public double LastHeardGameTimestamp => lastHeardGameTimestamp;

        public void Configure(SeaStateDirector director) { seaState = director; }

        private void Awake()
        {
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
        }

        private void OnEnable() { AcousticEventBus.SignalEmitted += HandleSignal; }
        private void OnDisable() { AcousticEventBus.SignalEmitted -= HandleSignal; }

        private void Update()
        {
            if (seaState == null || acousticPressure <= 0f) return;
            float gameDelta = Mathf.Max(0f, seaState.LastGameDeltaSeconds);
            if (gameDelta <= 0f) return;
            acousticPressure = Mathf.MoveTowards(
                acousticPressure, 0f, pressureDecayPerGameMinute * (gameDelta / 60f));
        }

        private void HandleSignal(AcousticSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.EventId) || heardEventIds.Contains(signal.EventId))
                return;

            float distance = Vector3.Distance(transform.position, signal.Position);
            float score = AcousticPerceptionMath.CalculateHearingScore(
                distance, Mathf.Min(listeningRadiusMeters, signal.RadiusMeters),
                signal.Loudness, sensitivity, backgroundNoise);
            if (score < minimumHearingScore) return;

            RememberEvent(signal.EventId);
            lastHeardEventId = signal.EventId;
            lastHeardRegionId = signal.RegionId;
            lastHeardPosition = signal.Position;
            lastHeardGameTimestamp = Math.Max(0.0, signal.GameTimestamp);
            acousticPressure = Mathf.Clamp01(acousticPressure + score * pressureGain);
            if (seaState != null) seaState.AddDisturbance(score * 0.018f);
            SignalHeard?.Invoke(signal, score);
        }

        private void RememberEvent(string eventId)
        {
            heardEventIds.Add(eventId);
            eventOrder.Enqueue(eventId);
            while (eventOrder.Count > MaximumRememberedEvents)
                heardEventIds.Remove(eventOrder.Dequeue());
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.world == null)
                throw new ArgumentNullException(nameof(data), "Save data must contain world state.");
            data.world.acousticPressure = acousticPressure;
            data.world.lastAcousticEventId = lastHeardEventId;
            data.world.lastAcousticRegionId = lastHeardRegionId;
            CopyVector(lastHeardPosition, data.world.lastAcousticPosition);
            data.world.lastAcousticGameTimestamp = lastHeardGameTimestamp;
            data.world.heardAcousticEventIds.Clear();
            data.world.heardAcousticEventIds.AddRange(eventOrder);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null) return;
            acousticPressure = Mathf.Clamp01(data.world.acousticPressure);
            lastHeardEventId = data.world.lastAcousticEventId ?? string.Empty;
            lastHeardRegionId = data.world.lastAcousticRegionId ?? string.Empty;
            lastHeardPosition = ToVector(data.world.lastAcousticPosition);
            lastHeardGameTimestamp = Math.Max(0.0, data.world.lastAcousticGameTimestamp);
            heardEventIds.Clear();
            eventOrder.Clear();
            if (data.world.heardAcousticEventIds == null) return;

            int start = Mathf.Max(0, data.world.heardAcousticEventIds.Count - MaximumRememberedEvents);
            for (int index = start; index < data.world.heardAcousticEventIds.Count; index++)
            {
                string id = data.world.heardAcousticEventIds[index];
                if (!string.IsNullOrWhiteSpace(id) && heardEventIds.Add(id)) eventOrder.Enqueue(id);
            }
        }

        private static void CopyVector(Vector3 source, Vector3SaveData destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.x = source.x; destination.y = source.y; destination.z = source.z;
        }

        private static Vector3 ToVector(Vector3SaveData source)
        {
            return source == null ? Vector3.zero : new Vector3(source.x, source.y, source.z);
        }
    }
}
