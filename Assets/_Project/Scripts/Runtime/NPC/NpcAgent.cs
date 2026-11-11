using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.NPC
{
    [Serializable]
    public sealed class NpcScheduleEntry
    {
        [Range(0f, 24f)] public float startHour;
        [Range(0f, 24f)] public float endHour = 24f;
        public string activityId = "working";
        public Transform destination;

        public bool ContainsHour(float hour)
        {
            hour = Mathf.Repeat(hour, 24f);
            if (Mathf.Approximately(startHour, endHour)) return true;
            if (startHour < endHour)
                return hour >= startHour && hour < endHour;
            return hour >= startHour || hour < endHour;
        }
    }

    public static class NpcScheduleResolver
    {
        public static int ResolveIndex(IList<NpcScheduleEntry> entries, float hour)
        {
            if (entries == null) return -1;
            for (int i = 0; i < entries.Count; i++)
            {
                NpcScheduleEntry entry = entries[i];
                if (entry != null && entry.ContainsHour(hour))
                    return i;
            }
            return -1;
        }
    }

    public enum NpcRelationshipTier
    {
        Hostile, Distrustful, Wary, Neutral, Familiar, Trusted, Ally
    }

    /// <summary>Per-resident trust thresholds shared by dialogue and future service systems.</summary>
    public static class NpcRelationshipRules
    {
        public static NpcRelationshipTier ResolveTier(int score)
        {
            score = Mathf.Clamp(score, 0, 100);
            if (score < 15) return NpcRelationshipTier.Hostile;
            if (score < 30) return NpcRelationshipTier.Distrustful;
            if (score < 45) return NpcRelationshipTier.Wary;
            if (score < 60) return NpcRelationshipTier.Neutral;
            if (score < 75) return NpcRelationshipTier.Familiar;
            if (score < 90) return NpcRelationshipTier.Trusted;
            return NpcRelationshipTier.Ally;
        }

        public static bool CanTalk(int score) { return score >= 15; }
        public static bool CanTrade(int score) { return score >= 30; }

        public static float GetBuyPriceMultiplier(int score)
        {
            if (score < 30) return 0f;
            if (score < 45) return 0.85f;
            if (score < 60) return 1.00f;
            if (score < 75) return 1.05f;
            if (score < 90) return 1.10f;
            return 1.15f;
        }

        public static string GetTierLabel(int score) { return ResolveTier(score).ToString(); }
    }

    /// <summary>
    /// Data-driven village resident. Schedules use the authoritative action-driven sea clock.
    /// Movement is deliberately waypoint-based until Greyhaven has a baked navigation surface.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcAgent : MonoBehaviour, ISaveParticipant
    {
        [Header("Identity")]
        [SerializeField] private string npcId = "greyhaven.resident";
        [SerializeField] private string displayName = "Village Resident";
        [SerializeField] private string occupationId = "resident";
        [SerializeField] private string regionId = "greyhaven";

        [Header("Simulation")]
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField] private List<NpcScheduleEntry> schedule = new List<NpcScheduleEntry>();
        [SerializeField, Min(0.05f)] private float walkingSpeed = 1.15f;
        [SerializeField, Range(0, 100)] private int relationshipScore = 50;
        [SerializeField] private bool faceMovementDirection = true;

        private readonly HashSet<string> relationshipMemoryIds = new HashSet<string>(StringComparer.Ordinal);
        private int _activeScheduleIndex = -1;
        private string _activityId = "idle";

        public string NpcId => npcId;
        public string DisplayName => displayName;
        public string OccupationId => occupationId;
        public string ActivityId => _activityId;
        public int RelationshipScore => relationshipScore;
        public NpcRelationshipTier RelationshipTier => NpcRelationshipRules.ResolveTier(relationshipScore);
        public bool CanTalk => NpcRelationshipRules.CanTalk(relationshipScore);
        public bool CanTrade => NpcRelationshipRules.CanTrade(relationshipScore);
        public float BuyPriceMultiplier => NpcRelationshipRules.GetBuyPriceMultiplier(relationshipScore);
        public IReadOnlyCollection<string> RelationshipMemoryIds => relationshipMemoryIds;
        public IReadOnlyList<NpcScheduleEntry> Schedule => schedule;

        public void Configure(
            string stableId,
            string name,
            string occupation,
            string region,
            SeaStateDirector clock,
            List<NpcScheduleEntry> dailySchedule)
        {
            npcId = stableId;
            displayName = name;
            occupationId = occupation;
            regionId = region;
            seaState = clock;
            schedule = dailySchedule ?? new List<NpcScheduleEntry>();
            _activeScheduleIndex = -1;
        }

        public void SetRelationshipScore(int value)
        {
            relationshipScore = Mathf.Clamp(value, 0, 100);
        }

        public void ModifyRelationship(int delta)
        {
            relationshipScore = Mathf.Clamp(relationshipScore + delta, 0, 100);
        }

        /// <summary>Applies each named relationship event once per resident.</summary>
        public bool TryApplyRelationshipEvent(string memoryId, int delta)
        {
            if (string.IsNullOrWhiteSpace(memoryId) ||
                !string.Equals(memoryId, memoryId.Trim(), StringComparison.Ordinal) ||
                memoryId.Length > 256 || delta == 0 || delta < -20 || delta > 20 ||
                relationshipMemoryIds.Count >= SaveGameDataMigration.MaximumNpcRelationshipMemories)
                return false;

            if (!relationshipMemoryIds.Add(memoryId))
                return false;

            ModifyRelationship(delta);
            return true;
        }

        private void Start()
        {
            if (seaState != null)
                ApplySchedule(NpcScheduleResolver.ResolveIndex(schedule, seaState.CurrentHour));
        }

        private void Update()
        {
            if (seaState == null || !seaState.IsAdvancing || schedule == null)
                return;

            int nextIndex = NpcScheduleResolver.ResolveIndex(schedule, seaState.CurrentHour);
            if (nextIndex != _activeScheduleIndex)
                ApplySchedule(nextIndex);

            if (_activeScheduleIndex < 0 || _activeScheduleIndex >= schedule.Count)
                return;

            Transform destination = schedule[_activeScheduleIndex].destination;
            if (destination == null)
                return;

            Vector3 target = destination.position;
            Vector3 previous = transform.position;
            transform.position = Vector3.MoveTowards(
                previous,
                target,
                walkingSpeed * Time.deltaTime);

            Vector3 direction = transform.position - previous;
            direction.y = 0f;
            if (faceMovementDirection && direction.sqrMagnitude > 0.0004f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction.normalized, Vector3.up),
                    8f * Time.deltaTime);
        }

        private void ApplySchedule(int index)
        {
            _activeScheduleIndex = index;
            _activityId = index >= 0 && index < schedule.Count &&
                          schedule[index] != null &&
                          !string.IsNullOrWhiteSpace(schedule[index].activityId)
                ? schedule[index].activityId
                : "idle";
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null || string.IsNullOrWhiteSpace(npcId))
                return;

            NpcStateSaveData state = FindOrCreateState(data);
            state.stateId = _activityId;
            state.relationshipId = relationshipScore.ToString(CultureInfo.InvariantCulture);
            state.lastKnownRegionId = regionId;
            if (state.relationshipMemoryIds == null)
                state.relationshipMemoryIds = new List<string>();
            state.relationshipMemoryIds.Clear();
            foreach (string memoryId in relationshipMemoryIds)
                state.relationshipMemoryIds.Add(memoryId);
            state.relationshipMemoryIds.Sort(StringComparer.Ordinal);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null || data.progress.npcStates == null)
                return;

            for (int i = 0; i < data.progress.npcStates.Count; i++)
            {
                NpcStateSaveData state = data.progress.npcStates[i];
                if (state == null || !string.Equals(state.npcId, npcId, StringComparison.Ordinal))
                    continue;

                int restoredRelationship;
                if (int.TryParse(state.relationshipId, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out restoredRelationship))
                    relationshipScore = Mathf.Clamp(restoredRelationship, 0, 100);

                relationshipMemoryIds.Clear();
                if (state.relationshipMemoryIds != null)
                {
                    for (int memoryIndex = 0;
                         memoryIndex < state.relationshipMemoryIds.Count &&
                         relationshipMemoryIds.Count < SaveGameDataMigration.MaximumNpcRelationshipMemories;
                         memoryIndex++)
                    {
                        string memoryId = state.relationshipMemoryIds[memoryIndex];
                        if (!string.IsNullOrWhiteSpace(memoryId) && memoryId.Length <= 256 &&
                            string.Equals(memoryId, memoryId.Trim(), StringComparison.Ordinal))
                            relationshipMemoryIds.Add(memoryId);
                    }
                }

                if (!string.IsNullOrWhiteSpace(state.lastKnownRegionId))
                    regionId = state.lastKnownRegionId;

                _activityId = string.IsNullOrWhiteSpace(state.stateId) ? "idle" : state.stateId;
                _activeScheduleIndex = -1;
                return;
            }
        }

        private NpcStateSaveData FindOrCreateState(SaveGameData data)
        {
            if (data.progress.npcStates == null)
                data.progress.npcStates = new List<NpcStateSaveData>();

            for (int i = 0; i < data.progress.npcStates.Count; i++)
            {
                NpcStateSaveData existing = data.progress.npcStates[i];
                if (existing != null && string.Equals(existing.npcId, npcId, StringComparison.Ordinal))
                    return existing;
            }

            NpcStateSaveData created = new NpcStateSaveData { npcId = npcId };
            data.progress.npcStates.Add(created);
            return created;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (schedule == null) return;
            Gizmos.color = new Color(0.25f, 0.75f, 0.68f, 0.8f);
            for (int i = 0; i < schedule.Count; i++)
            {
                if (schedule[i] == null || schedule[i].destination == null) continue;
                Gizmos.DrawLine(transform.position, schedule[i].destination.position);
                Gizmos.DrawWireSphere(schedule[i].destination.position, 0.35f);
            }
        }
#endif
    }
}
