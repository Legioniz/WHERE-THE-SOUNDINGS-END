using System;
using System.Collections.Generic;
using UnityEngine;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>
    /// Persistent wallet owner for the player vessel. Transactions are idempotent and
    /// validate the complete balance change before mutating either balance or ledger.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EconomyWalletController : MonoBehaviour, ISaveParticipant, ISaveDataValidator
    {
        [SerializeField, Min(0)] private long walletCents;
        [SerializeField] private List<string> processedTransactionIds = new List<string>();

        private readonly HashSet<string> transactionLookup = new HashSet<string>(StringComparer.Ordinal);

        public long WalletCents { get { return walletCents; } }

        private void Awake()
        {
            RebuildTransactionLookup();
        }

        private void OnValidate()
        {
            walletCents = Math.Max(0L, walletCents);
            if (processedTransactionIds == null)
                processedTransactionIds = new List<string>();
        }

        public bool HasProcessedTransaction(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) return false;
            RebuildTransactionLookupIfNeeded();
            return transactionLookup.Contains(transactionId);
        }

        public bool TryApplyTransaction(string transactionId, long deltaCents)
        {
            if (string.IsNullOrWhiteSpace(transactionId) ||
                !string.Equals(transactionId, transactionId.Trim(), StringComparison.Ordinal) ||
                deltaCents == 0L || processedTransactionIds == null ||
                processedTransactionIds.Count >= SaveGameDataMigration.MaximumCollectionEntries)
                return false;

            RebuildTransactionLookupIfNeeded();
            if (transactionLookup.Contains(transactionId)) return false;

            long nextBalance;
            if (!EconomyPolicy.TryApplyWalletDelta(walletCents, deltaCents, out nextBalance))
                return false;

            // Both mutations happen only after every validation has passed.
            walletCents = nextBalance;
            processedTransactionIds.Add(transactionId);
            transactionLookup.Add(transactionId);
            return true;
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.economy == null)
                throw new ArgumentNullException(nameof(data), "Save data must contain economy data.");

            data.economy.walletCents = walletCents;
            data.economy.processedTransactionIds.Clear();
            data.economy.processedTransactionIds.AddRange(processedTransactionIds);
        }

        public bool TryValidateSaveData(SaveGameData data, out string error)
        {
            error = null;
            if (data == null || data.economy == null ||
                data.economy.walletCents < 0L || data.economy.walletCents > 1000000000000L ||
                data.economy.processedTransactionIds == null ||
                data.economy.processedTransactionIds.Count > SaveGameDataMigration.MaximumCollectionEntries)
            {
                error = "The save contains invalid economy wallet data.";
                return false;
            }

            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < data.economy.processedTransactionIds.Count; i++)
            {
                string id = data.economy.processedTransactionIds[i];
                if (string.IsNullOrWhiteSpace(id) || id.Length > 128 ||
                    !string.Equals(id, id.Trim(), StringComparison.Ordinal) || !ids.Add(id))
                {
                    error = "The save contains a malformed or duplicate economy transaction ID.";
                    return false;
                }
            }

            return true;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.economy == null) return;

            string error;
            if (!TryValidateSaveData(data, out error))
                throw new InvalidOperationException(error);

            // Replace the live ledger only after the incoming record is fully validated.
            List<string> restoredIds = new List<string>(data.economy.processedTransactionIds);
            walletCents = data.economy.walletCents;
            processedTransactionIds = restoredIds;
            RebuildTransactionLookup();
        }

        private void RebuildTransactionLookupIfNeeded()
        {
            if (transactionLookup.Count != (processedTransactionIds != null ? processedTransactionIds.Count : 0))
                RebuildTransactionLookup();
        }

        private void RebuildTransactionLookup()
        {
            transactionLookup.Clear();
            if (processedTransactionIds == null)
                processedTransactionIds = new List<string>();

            for (int i = 0; i < processedTransactionIds.Count; i++)
            {
                string id = processedTransactionIds[i];
                if (!string.IsNullOrWhiteSpace(id))
                    transactionLookup.Add(id);
            }
        }
    }
}
