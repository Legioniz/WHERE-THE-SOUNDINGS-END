using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WhereTheSoundingsEnd.Streaming
{
    /// <summary>
    /// Owns Addressables prefab handles and reference-counted leases.
    /// A loaded prefab remains resident while any consumer (including a pool) holds a lease.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AddressableAssetResidencyService : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float unusedAssetReleaseGraceSeconds = 8f;

        private sealed class Entry
        {
            public string Key;
            public AsyncOperationHandle<GameObject> Handle;
            public GameObject Asset;
            public readonly List<Action<PrefabLease, string>> Waiters =
                new List<Action<PrefabLease, string>>();
            public int LeaseCount;
            public float UnusedSince = -1f;
            public bool Loading;
        }

        public sealed class PrefabLease : IDisposable
        {
            private AddressableAssetResidencyService owner;
            private readonly string key;
            public GameObject Asset { get; private set; }
            public string Key => key;
            public bool IsValid => owner != null && Asset != null;

            internal PrefabLease(AddressableAssetResidencyService service, string address, GameObject asset)
            {
                owner = service;
                key = address;
                Asset = asset;
            }

            public void Dispose()
            {
                AddressableAssetResidencyService current = owner;
                if (current == null) return;
                owner = null;
                Asset = null;
                current.ReleaseLease(key);
            }
        }

        private readonly Dictionary<string, Entry> entries =
            new Dictionary<string, Entry>(StringComparer.Ordinal);
        private bool shuttingDown;

        public int LoadedAssetCount
        {
            get
            {
                int count = 0;
                foreach (Entry entry in entries.Values)
                    if (!entry.Loading && entry.Asset != null) count++;
                return count;
            }
        }

        public int ActiveLeaseCount
        {
            get
            {
                int count = 0;
                foreach (Entry entry in entries.Values) count += entry.LeaseCount;
                return count;
            }
        }

        public bool HasPendingOperations
        {
            get
            {
                foreach (Entry entry in entries.Values)
                    if (entry.Loading) return true;
                return false;
            }
        }

        public void AcquirePrefab(string address, Action<PrefabLease, string> completed)
        {
            if (completed == null) return;
            if (shuttingDown)
            {
                completed(null, "Addressable residency service is shutting down.");
                return;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                completed(null, "Addressable prefab address is empty.");
                return;
            }

            string key = address.Trim();
            Entry entry;
            if (entries.TryGetValue(key, out entry))
            {
                if (entry.Loading)
                {
                    entry.Waiters.Add(completed);
                    return;
                }

                if (entry.Asset != null)
                {
                    NotifyWaiter(completed, CreateLease(entry), string.Empty);
                    return;
                }

                entries.Remove(key);
            }

            entry = new Entry { Key = key, Loading = true };
            entry.Waiters.Add(completed);
            entries.Add(key, entry);

            try
            {
                entry.Handle = Addressables.LoadAssetAsync<GameObject>(key);
                entry.Handle.Completed += operation => CompleteLoad(entry, operation);
            }
            catch (Exception exception)
            {
                FailLoad(entry, "Addressables could not start '" + key + "': " + exception.Message);
            }
        }

        private void CompleteLoad(Entry entry, AsyncOperationHandle<GameObject> operation)
        {
            if (shuttingDown || entry == null) return;
            if (operation.Status != AsyncOperationStatus.Succeeded || operation.Result == null)
            {
                string reason = operation.OperationException != null
                    ? operation.OperationException.Message
                    : "Addressable prefab was not found or did not load as a GameObject.";
                FailLoad(entry, "Failed to load '" + entry.Key + "': " + reason);
                return;
            }

            entry.Asset = operation.Result;
            entry.Loading = false;
            entry.UnusedSince = Time.unscaledTime;
            Action<PrefabLease, string>[] waiters = entry.Waiters.ToArray();
            entry.Waiters.Clear();
            for (int i = 0; i < waiters.Length; i++)
            {
                if (shuttingDown) break;
                NotifyWaiter(waiters[i], CreateLease(entry), string.Empty);
            }
        }

        private void FailLoad(Entry entry, string reason)
        {
            if (entry == null) return;
            entry.Loading = false;
            if (entry.Handle.IsValid())
                Addressables.Release(entry.Handle);
            if (entries.ContainsKey(entry.Key))
                entries.Remove(entry.Key);

            Action<PrefabLease, string>[] waiters = entry.Waiters.ToArray();
            entry.Waiters.Clear();
            for (int i = 0; i < waiters.Length; i++)
                NotifyWaiter(waiters[i], null, reason);
            Debug.LogError("[AddressableResidency] " + reason, this);
        }

        private void NotifyWaiter(
            Action<PrefabLease, string> callback, PrefabLease lease, string error)
        {
            try
            {
                callback(lease, error);
            }
            catch (Exception exception)
            {
                if (lease != null) lease.Dispose();
                Debug.LogException(exception, this);
            }
        }

        private PrefabLease CreateLease(Entry entry)
        {
            entry.LeaseCount++;
            entry.UnusedSince = -1f;
            return new PrefabLease(this, entry.Key, entry.Asset);
        }

        private void ReleaseLease(string key)
        {
            Entry entry;
            if (!entries.TryGetValue(key, out entry)) return;
            entry.LeaseCount = Mathf.Max(0, entry.LeaseCount - 1);
            if (entry.LeaseCount == 0)
                entry.UnusedSince = Time.unscaledTime;
        }

        private void Update()
        {
            if (shuttingDown) return;
            float grace = Mathf.Max(0f, unusedAssetReleaseGraceSeconds);
            List<string> release = null;
            foreach (KeyValuePair<string, Entry> pair in entries)
            {
                Entry entry = pair.Value;
                if (entry.Loading || entry.LeaseCount > 0 || entry.UnusedSince < 0f)
                    continue;
                if (!AddressablePoolPolicy.ShouldReleaseUnused(entry.LeaseCount, Time.unscaledTime - entry.UnusedSince, grace))
                    continue;
                if (release == null) release = new List<string>();
                release.Add(pair.Key);
            }

            if (release == null) return;
            for (int i = 0; i < release.Count; i++)
                ReleaseEntry(release[i]);
        }

        public int ReleaseUnusedNow()
        {
            List<string> release = new List<string>();
            foreach (KeyValuePair<string, Entry> pair in entries)
                if (!pair.Value.Loading && pair.Value.LeaseCount == 0)
                    release.Add(pair.Key);
            for (int i = 0; i < release.Count; i++)
                ReleaseEntry(release[i]);
            return release.Count;
        }

        private void ReleaseEntry(string key)
        {
            Entry entry;
            if (!entries.TryGetValue(key, out entry) || entry.Loading || entry.LeaseCount > 0)
                return;
            entries.Remove(key);
            if (entry.Handle.IsValid())
                Addressables.Release(entry.Handle);
            entry.Asset = null;
        }

        private void OnDestroy()
        {
            shuttingDown = true;
            List<Entry> closing = new List<Entry>(entries.Values);
            entries.Clear();
            for (int entryIndex = 0; entryIndex < closing.Count; entryIndex++)
            {
                Entry entry = closing[entryIndex];
                Action<PrefabLease, string>[] waiters = entry.Waiters.ToArray();
                entry.Waiters.Clear();
                for (int waiterIndex = 0; waiterIndex < waiters.Length; waiterIndex++)
                    NotifyWaiter(waiters[waiterIndex], null, "Addressable residency service was destroyed.");
                if (entry.Handle.IsValid())
                    Addressables.Release(entry.Handle);
            }
        }
    }
}
