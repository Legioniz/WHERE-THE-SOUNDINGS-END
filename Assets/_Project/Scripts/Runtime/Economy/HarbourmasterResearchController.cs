using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.NPC;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>
    /// Converts discovered acoustic sites and recovered evidence into persistent harbour
    /// research submissions and chart permits. It never reveals unvisited chart locations.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HarbourmasterResearchController : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        private const string SaveFlagId = "harbourmaster.research-board";

        [Serializable]
        private sealed class ResearchDefinition
        {
            public string id;
            public string title;
            public string description;
            public string itemId;
            public string itemName;
            public int requiredDiscoveries;
            public string permitRegionId;
            public string permitName;
            public long rewardCents;
        }

        private static readonly ResearchDefinition[] Research =
        {
            new ResearchDefinition
            {
                id = "coastal-acoustic-report", title = "The Inner Water Record",
                description = "Submit one set of Corroded Ship Fittings and at least one confirmed acoustic site.",
                itemId = "SALVAGE-SCRAP-01", itemName = "Corroded Ship Fittings",
                requiredDiscoveries = 1, permitRegionId = "the_shallows",
                permitName = "The Shallows chart permit", rewardCents = 1500
            },
            new ResearchDefinition
            {
                id = "estuary-survey-dossier", title = "Where the Brackish Current Turns",
                description = "Submit one bundle of Salt-Stiff Canvas and at least three confirmed acoustic sites.",
                itemId = "SALVAGE-CLOTH-01", itemName = "Salt-Stiff Canvas",
                requiredDiscoveries = 3, permitRegionId = "blackrock_reach",
                permitName = "Blackrock Reach chart permit", rewardCents = 3000
            },
            new ResearchDefinition
            {
                id = "trench-resonance-archive", title = "A Sound Beneath the Soundings",
                description = "Submit one piece of Waterlogged Timber and at least five confirmed acoustic sites.",
                itemId = "SALVAGE-WOOD-01", itemName = "Waterlogged Timber",
                requiredDiscoveries = 5, permitRegionId = "the_outer_shoals",
                permitName = "Outer Shoals chart permit", rewardCents = 5000
            }
        };

        [SerializeField] private SpatialCargoHold cargo;
        [SerializeField] private EconomyWalletController wallet;
        [SerializeField] private SoundingRigController soundingRig;
        [SerializeField] private RegionalChartController regionalChart;
        [SerializeField] private int submittedMask;

        public int DiscoveryCount => soundingRig != null ? soundingRig.DiscoveredSiteIds.Count : 0;
        public bool CanSubmitNext
        {
            get
            {
                int index = NextAvailableIndex();
                return index >= 0 && DiscoveryCount >= Research[index].requiredDiscoveries &&
                    cargo != null && cargo.GetAvailableQuantity(Research[index].itemId) > 0;
            }
        }

        private void Awake()
        {
            if (cargo == null) cargo = GetComponent<SpatialCargoHold>();
            if (wallet == null) wallet = GetComponent<EconomyWalletController>();
            if (soundingRig == null) soundingRig = GetComponent<SoundingRigController>();
            if (soundingRig == null) soundingRig = FindFirstObjectByType<SoundingRigController>();
            if (regionalChart == null) regionalChart = FindFirstObjectByType<RegionalChartController>();
        }

        public bool IsSubmitted(int index)
        {
            return index >= 0 && index < Research.Length && (submittedMask & (1 << index)) != 0;
        }

        public string GetBoardText(NpcAgent harbourmaster)
        {
            if (harbourmaster == null || !harbourmaster.CanTrade)
                return "The Harbourmaster is occupied. Research submissions are closed.";
            int next = NextAvailableIndex();
            if (next < 0)
                return "ALL CURRENT RESEARCH SUBMISSIONS COMPLETE. The recorded permits remain in your campaign chart.";
            ResearchDefinition item = Research[next];
            int discoveries = DiscoveryCount;
            return "RESEARCH  /  " + item.title + "\n" + item.description +
                "\n\nEVIDENCE  /  " + discoveries + " of " + item.requiredDiscoveries +
                " acoustic sites\nDELIVERY  /  " + item.itemName +
                "\nPERMIT  /  " + item.permitName +
                "\nRESEARCH STIPEND  /  $" + (item.rewardCents / 100m).ToString("0.00");
        }

        public bool TrySubmitNext(NpcAgent harbourmaster, out string message)
        {
            if (harbourmaster == null || !harbourmaster.CanTrade)
            {
                message = "The Harbourmaster is not available to receive research.";
                return false;
            }
            if (cargo == null || wallet == null)
            {
                message = "Research services cannot access the vessel cargo or wallet.";
                return false;
            }

            int index = NextAvailableIndex();
            if (index < 0)
            {
                message = "All current research submissions have been completed.";
                return false;
            }
            ResearchDefinition item = Research[index];
            if (DiscoveryCount < item.requiredDiscoveries)
            {
                message = "INSUFFICIENT EVIDENCE  /  " + item.requiredDiscoveries +
                    " confirmed acoustic sites are required; current record is " + DiscoveryCount + ".";
                return false;
            }

            CargoPlacement selected = null;
            IReadOnlyList<CargoPlacement> placements = cargo.Placements;
            for (int i = 0; i < placements.Count; i++)
                if (placements[i] != null && placements[i].itemId == item.itemId && placements[i].quantity == 1)
                { selected = placements[i]; break; }
            if (selected == null)
            {
                message = "RESEARCH MATERIAL MISSING  /  Bring one " + item.itemName + ".";
                return false;
            }

            string transactionId = "harbourmaster.research." + item.id;
            List<CargoPlacement> removed;
            bool committed = cargo.TryCommitRemoval(new[] { selected.instanceId },
                () => wallet.TryApplyTransaction(transactionId, item.rewardCents), out removed);
            if (!committed)
            {
                message = "RESEARCH SUBMISSION NOT COMMITTED. Cargo and wallet were left unchanged.";
                return false;
            }

            submittedMask |= 1 << index;
            ApplyEarnedPermits();
            message = "RESEARCH ACCEPTED  /  " + item.title + "  /  +" +
                (item.rewardCents / 100m).ToString("0.00") + " USD  /  " + item.permitName + " recorded.";
            return true;
        }

        private void ApplyEarnedPermits()
        {
            if (regionalChart == null) regionalChart = FindFirstObjectByType<RegionalChartController>();
            if (regionalChart == null) return;
            for (int i = 0; i < Research.Length; i++)
                if (IsSubmitted(i)) regionalChart.GrantChartPermit(Research[i].permitRegionId);
            if (IsSubmitted(0)) regionalChart.GrantChartPermit("reef_stewardship");
            if (IsSubmitted(1)) regionalChart.GrantChartPermit("licensed_sea_cucumber");
            if (IsSubmitted(2)) regionalChart.GrantChartPermit("managed_harvest");
        }

        private int NextAvailableIndex()
        {
            for (int i = 0; i < Research.Length; i++)
                if (!IsSubmitted(i)) return i;
            return -1;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null) return;
            if (data.progress.worldFlags == null) data.progress.worldFlags = new List<WorldFlagSaveData>();
            if (data.progress.unlockedRegionIds == null) data.progress.unlockedRegionIds = new List<string>();
            data.progress.worldFlags.RemoveAll(flag => flag != null && flag.flagId == SaveFlagId);
            data.progress.worldFlags.Add(new WorldFlagSaveData
            {
                flagId = SaveFlagId, value = true, stringValue = submittedMask.ToString()
            });
            for (int i = 0; i < Research.Length; i++)
                if (IsSubmitted(i) && !data.progress.unlockedRegionIds.Contains(Research[i].permitRegionId))
                    data.progress.unlockedRegionIds.Add(Research[i].permitRegionId);
            if (IsSubmitted(0) && !data.progress.unlockedRegionIds.Contains("reef_stewardship"))
                data.progress.unlockedRegionIds.Add("reef_stewardship");
            if (IsSubmitted(1) && !data.progress.unlockedRegionIds.Contains("licensed_sea_cucumber"))
                data.progress.unlockedRegionIds.Add("licensed_sea_cucumber");
            if (IsSubmitted(2) && !data.progress.unlockedRegionIds.Contains("managed_harvest"))
                data.progress.unlockedRegionIds.Add("managed_harvest");
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.progress == null || data.progress.worldFlags == null ||
                data.progress.unlockedRegionIds == null)
            {
                error = "The save does not contain valid Harbourmaster research data.";
                return false;
            }
            for (int i = 0; i < data.progress.worldFlags.Count; i++)
            {
                WorldFlagSaveData flag = data.progress.worldFlags[i];
                if (flag == null || flag.flagId != SaveFlagId) continue;
                int mask;
                if (!int.TryParse(flag.stringValue, out mask) || mask < 0 ||
                    (mask & ~((1 << Research.Length) - 1)) != 0)
                {
                    error = "The save contains an invalid Harbourmaster research submission mask.";
                    return false;
                }
            }
            return true;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            submittedMask = 0;
            if (data == null || data.progress == null || data.progress.worldFlags == null) return;
            for (int i = 0; i < data.progress.worldFlags.Count; i++)
            {
                WorldFlagSaveData flag = data.progress.worldFlags[i];
                if (flag == null || flag.flagId != SaveFlagId) continue;
                int mask;
                if (int.TryParse(flag.stringValue, out mask) && mask >= 0 &&
                    (mask & ~((1 << Research.Length) - 1)) == 0)
                    submittedMask = mask;
                ApplyEarnedPermits();
                return;
            }
        }
    }
}
