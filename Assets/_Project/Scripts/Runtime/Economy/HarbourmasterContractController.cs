using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.NPC;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>
    /// Small, persistent Greyhaven delivery board. Contracts consume one authored salvage
    /// placement and pay through the vessel wallet in the same synchronous cargo transaction.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HarbourmasterContractController : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        private const string SaveFlagId = "harbourmaster.contract-board";
        private const int ContractCount = 3;

        [Serializable]
        private sealed class ContractDefinition
        {
            public string id;
            public string title;
            public string description;
            public string itemId;
            public string itemName;
            public long rewardCents;
        }

        private static readonly ContractDefinition[] Contracts =
        {
            new ContractDefinition
            {
                id = "channel-marker-fittings",
                title = "A Marker That Will Not Hold",
                description = "Bring one set of Corroded Ship Fittings. The western channel marker has begun to lean again.",
                itemId = "SALVAGE-SCRAP-01",
                itemName = "Corroded Ship Fittings",
                rewardCents = 850
            },
            new ContractDefinition
            {
                id = "weatherproof-ledger",
                title = "Keep the Records Dry",
                description = "Bring one bundle of Salt-Stiff Canvas. The office needs a weather cover for its exposed logbooks.",
                itemId = "SALVAGE-CLOTH-01",
                itemName = "Salt-Stiff Canvas",
                rewardCents = 500
            },
            new ContractDefinition
            {
                id = "quay-timber",
                title = "Patch the Quay",
                description = "Bring one piece of Waterlogged Timber. The outer edge of the landing needs a temporary brace.",
                itemId = "SALVAGE-WOOD-01",
                itemName = "Waterlogged Timber",
                rewardCents = 700
            }
        };

        [SerializeField] private SpatialCargoHold cargo;
        [SerializeField] private EconomyWalletController wallet;
        [SerializeField, Range(-1, ContractCount - 1)] private int activeContractIndex = -1;
        [SerializeField] private int completedMask;

        public bool HasActiveContract => activeContractIndex >= 0 && activeContractIndex < Contracts.Length;
        public string ActiveTitle => HasActiveContract ? Contracts[activeContractIndex].title : string.Empty;
        public string ActiveDescription => HasActiveContract ? Contracts[activeContractIndex].description : string.Empty;
        public long ActiveRewardCents => HasActiveContract ? Contracts[activeContractIndex].rewardCents : 0L;

        private void Awake()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (cargo == null) cargo = GetComponent<SpatialCargoHold>();
            if (wallet == null) wallet = GetComponent<EconomyWalletController>();
        }

        public bool IsCompleted(int index)
        {
            return index >= 0 && index < Contracts.Length && (completedMask & (1 << index)) != 0;
        }

        public bool IsAvailableTo(NpcAgent harbourmaster)
        {
            return harbourmaster != null && harbourmaster.CanTrade;
        }

        public string GetBoardText(NpcAgent harbourmaster)
        {
            if (!IsAvailableTo(harbourmaster))
                return "The Harbourmaster is occupied. The contract ledger is closed for now.";
            if (HasActiveContract)
                return ActiveTitle + "\n" + ActiveDescription + "\n\nREWARD  /  " +
                       FormatMoney(ActiveRewardCents) + " USD\nDeliver the requested salvage to complete the contract.";
            for (int i = 0; i < Contracts.Length; i++)
                if (!IsCompleted(i))
                    return "AVAILABLE  /  " + Contracts[i].title + "\n" + Contracts[i].description +
                           "\n\nREWARD  /  " + FormatMoney(Contracts[i].rewardCents) + " USD";
            return "All current harbour contracts have been completed. The office has no further work to offer.";
        }

        public bool TryAcceptNext(NpcAgent harbourmaster, out string message)
        {
            ResolveReferences();
            if (!IsAvailableTo(harbourmaster))
            {
                message = "The Harbourmaster is not available to issue work right now.";
                return false;
            }
            if (HasActiveContract)
            {
                message = "Finish the active contract before accepting another.";
                return false;
            }
            for (int i = 0; i < Contracts.Length; i++)
            {
                if (IsCompleted(i)) continue;
                activeContractIndex = i;
                message = "CONTRACT ACCEPTED  /  " + Contracts[i].title;
                return true;
            }
            message = "There are no outstanding harbour contracts.";
            return false;
        }

        public bool TryCompleteActive(NpcAgent harbourmaster, out long rewardCents, out string message)
        {
            ResolveReferences();
            rewardCents = 0L;
            if (!IsAvailableTo(harbourmaster))
            {
                message = "The Harbourmaster is not available to receive the delivery.";
                return false;
            }
            if (!HasActiveContract || cargo == null || wallet == null)
            {
                message = "There is no active contract or the vessel services are unavailable.";
                return false;
            }

            ContractDefinition contract = Contracts[activeContractIndex];
            CargoPlacement selected = null;
            IReadOnlyList<CargoPlacement> placements = cargo.Placements;
            for (int i = 0; i < placements.Count; i++)
            {
                CargoPlacement placement = placements[i];
                if (placement != null && placement.itemId == contract.itemId && placement.quantity == 1)
                {
                    selected = placement;
                    break;
                }
            }
            if (selected == null)
            {
                message = "DELIVERY MISSING  /  Bring one unit of " + contract.itemName + ".";
                return false;
            }

            int completedIndex = activeContractIndex;
            string transactionId = "harbourmaster.contract." + contract.id;
            List<CargoPlacement> removed;
            bool committed = cargo.TryCommitRemoval(
                new[] { selected.instanceId },
                () => wallet.TryApplyTransaction(transactionId, contract.rewardCents),
                out removed);
            if (!committed)
            {
                message = "DELIVERY NOT COMMITTED  /  Cargo and wallet were left unchanged.";
                return false;
            }

            completedMask |= 1 << completedIndex;
            activeContractIndex = -1;
            rewardCents = contract.rewardCents;
            message = "CONTRACT COMPLETE  /  " + contract.title + "  /  +" +
                      FormatMoney(rewardCents) + " USD.";
            return true;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null) return;
            if (data.progress.worldFlags == null)
                data.progress.worldFlags = new List<WorldFlagSaveData>();
            data.progress.worldFlags.RemoveAll(flag => flag != null && flag.flagId == SaveFlagId);
            data.progress.worldFlags.Add(new WorldFlagSaveData
            {
                flagId = SaveFlagId,
                value = true,
                stringValue = activeContractIndex.ToString() + "|" + completedMask.ToString()
            });
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.progress == null || data.progress.worldFlags == null)
            {
                error = "The save does not contain valid Harbourmaster contract data.";
                return false;
            }
            for (int i = 0; i < data.progress.worldFlags.Count; i++)
            {
                WorldFlagSaveData flag = data.progress.worldFlags[i];
                if (flag == null || flag.flagId != SaveFlagId) continue;
                string[] parts = (flag.stringValue ?? string.Empty).Split('|');
                int active;
                int completed;
                if (parts.Length != 2 || !int.TryParse(parts[0], out active) ||
                    !int.TryParse(parts[1], out completed) || active < -1 ||
                    active >= Contracts.Length || completed < 0 ||
                    (completed & ~((1 << ContractCount) - 1)) != 0 ||
                    (active >= 0 && (completed & (1 << active)) != 0))
                {
                    error = "The save contains an invalid Harbourmaster contract state.";
                    return false;
                }
            }
            return true;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.progress == null || data.progress.worldFlags == null) return;
            activeContractIndex = -1;
            completedMask = 0;
            for (int i = 0; i < data.progress.worldFlags.Count; i++)
            {
                WorldFlagSaveData flag = data.progress.worldFlags[i];
                if (flag == null || flag.flagId != SaveFlagId) continue;
                string[] parts = (flag.stringValue ?? string.Empty).Split('|');
                int active;
                int completed;
                if (parts.Length != 2 || !int.TryParse(parts[0], out active) ||
                    !int.TryParse(parts[1], out completed)) continue;
                if (active >= -1 && active < Contracts.Length &&
                    completed >= 0 && (completed & ~((1 << ContractCount) - 1)) == 0 &&
                    (active < 0 || (completed & (1 << active)) == 0))
                {
                    activeContractIndex = active;
                    completedMask = completed;
                }
                return;
            }
        }

        private static string FormatMoney(long cents)
        {
            return (cents / 100m).ToString("0.00");
        }
    }
}
