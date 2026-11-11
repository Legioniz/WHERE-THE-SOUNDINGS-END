using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// A localized wreck/debris field with a finite daily yield.
    /// Dredging reserves a pull first and spends it only after the reward is placed.
    /// Reservations are transient: incomplete actions never reduce persisted daily stock.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SalvageSite : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private string siteId = "greyhaven.outer_channel_wreck";
        [SerializeField, Min(1)] private int pullsPerDay = 3;
        [SerializeField, Min(1f)] private float interactionRadius = 13f;

        private int remainingPulls;
        private int lastRefillDay = 1;
        private bool initialized;
        private readonly Dictionary<string, int> reservations = new Dictionary<string, int>(StringComparer.Ordinal);

        public string SiteId => siteId;
        public float InteractionRadius => interactionRadius;
        public int RemainingPulls
        {
            get
            {
                EnsureInitialized();
                return remainingPulls;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (initialized) return;
            pullsPerDay = Mathf.Max(1, pullsPerDay);
            interactionRadius = Mathf.Max(1f, interactionRadius);
            remainingPulls = pullsPerDay;
            lastRefillDay = Mathf.Max(1, lastRefillDay);
            initialized = true;
        }

        public bool TryReservePull(int currentDay, out string reservationId)
        {
            EnsureInitialized();
            RefreshForDay(currentDay);
            reservationId = null;

            int reservedForToday = CountReservationsForDay(lastRefillDay);
            if (remainingPulls - reservedForToday <= 0) return false;

            reservationId = Guid.NewGuid().ToString("N");
            reservations.Add(reservationId, lastRefillDay);
            return true;
        }

        public bool CommitReservedPull(string reservationId, int currentDay)
        {
            EnsureInitialized();
            RefreshForDay(currentDay);

            int reservedDay;
            if (string.IsNullOrWhiteSpace(reservationId) ||
                !reservations.TryGetValue(reservationId, out reservedDay))
                return false;

            reservations.Remove(reservationId);
            if (reservedDay != lastRefillDay || remainingPulls <= 0)
                return false;

            remainingPulls--;
            return true;
        }

        public void ReleaseReservedPull(string reservationId)
        {
            if (string.IsNullOrWhiteSpace(reservationId)) return;
            reservations.Remove(reservationId);
        }

        public bool IsWithinInteractionRange(Vector3 position)
        {
            return Vector3.Distance(position, transform.position) <= interactionRadius;
        }

        // Kept for simple callers that intentionally consume stock immediately.
        public bool TryConsumePull(int currentDay)
        {
            string reservationId;
            return TryReservePull(currentDay, out reservationId) &&
                   CommitReservedPull(reservationId, currentDay);
        }

        private void RefreshForDay(int currentDay)
        {
            int day = Mathf.Max(1, currentDay);
            if (day <= lastRefillDay) return;

            remainingPulls = pullsPerDay;
            lastRefillDay = day;

            List<string> expired = new List<string>();
            foreach (KeyValuePair<string, int> reservation in reservations)
                if (reservation.Value < day)
                    expired.Add(reservation.Key);

            for (int index = 0; index < expired.Count; index++)
                reservations.Remove(expired[index]);
        }

        private int CountReservationsForDay(int day)
        {
            int count = 0;
            foreach (KeyValuePair<string, int> reservation in reservations)
                if (reservation.Value == day) count++;
            return count;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            EnsureInitialized();
            SalvageSiteSaveData saved = data.world.salvageSites.Find(item =>
                item != null && item.siteId == siteId);
            if (saved == null)
            {
                saved = new SalvageSiteSaveData { siteId = siteId };
                data.world.salvageSites.Add(saved);
            }

            // Only committed pulls are persisted. An interrupted reservation is refundable.
            saved.remainingPulls = remainingPulls;
            saved.lastRefillDay = lastRefillDay;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null || data.world.salvageSites == null) return;
            EnsureInitialized();
            SalvageSiteSaveData saved = data.world.salvageSites.Find(item =>
                item != null && item.siteId == siteId);
            if (saved == null) return;

            remainingPulls = Mathf.Clamp(saved.remainingPulls, 0, pullsPerDay);
            lastRefillDay = Mathf.Max(1, saved.lastRefillDay);
            reservations.Clear();
        }

        public static SalvageSite FindNearest(Vector3 position, out float distance)
        {
            SalvageSite[] sites = FindObjectsByType<SalvageSite>(FindObjectsSortMode.None);
            SalvageSite nearest = null;
            distance = float.MaxValue;
            for (int i = 0; i < sites.Length; i++)
            {
                if (sites[i] == null) continue;
                float candidate = Vector3.Distance(position, sites[i].transform.position);
                if (candidate <= sites[i].interactionRadius && candidate < distance)
                {
                    nearest = sites[i];
                    distance = candidate;
                }
            }
            return nearest;
        }
    }
}