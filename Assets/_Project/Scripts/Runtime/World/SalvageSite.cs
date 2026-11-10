using UnityEngine;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// A localized wreck/debris field with a small daily salvage yield.
    /// Repeated dredging is possible, but the site cannot be farmed indefinitely in one day.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SalvageSite : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private string siteId = "greyhaven.outer_channel_wreck";
        [SerializeField, Min(1)] private int pullsPerDay = 3;
        [SerializeField, Min(1f)] private float interactionRadius = 13f;
        private int remainingPulls;
        private int lastRefillDay = 1;

        public string SiteId => siteId;
        public float InteractionRadius => interactionRadius;
        public int RemainingPulls => remainingPulls;

        private void Awake()
        {
            remainingPulls = pullsPerDay;
        }

        public bool TryConsumePull(int currentDay)
        {
            if (currentDay > lastRefillDay)
            {
                remainingPulls = pullsPerDay;
                lastRefillDay = currentDay;
            }

            if (remainingPulls <= 0) return false;
            remainingPulls--;
            return true;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            SalvageSiteSaveData saved = data.world.salvageSites.Find(item =>
                item != null && item.siteId == siteId);
            if (saved == null)
            {
                saved = new SalvageSiteSaveData { siteId = siteId };
                data.world.salvageSites.Add(saved);
            }
            saved.remainingPulls = remainingPulls;
            saved.lastRefillDay = lastRefillDay;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null || data.world.salvageSites == null) return;
            SalvageSiteSaveData saved = data.world.salvageSites.Find(item =>
                item != null && item.siteId == siteId);
            if (saved == null) return;
            remainingPulls = Mathf.Clamp(saved.remainingPulls, 0, pullsPerDay);
            lastRefillDay = Mathf.Max(1, saved.lastRefillDay);
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