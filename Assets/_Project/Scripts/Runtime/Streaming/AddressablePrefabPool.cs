using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhereTheSoundingsEnd.Streaming
{
    /// <summary>Optional reset hooks for objects reused by AddressablePrefabPool.</summary>
    public interface IPooledObject
    {
        void OnPoolRent();
        void OnPoolReturn();
    }

    /// <summary>
    /// Bounded pools of Addressable prefabs. Each pool holds one residency lease for as long as
    /// its instances exist, so prefab dependencies cannot be released while pooled instances use them.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AddressablePrefabPool : MonoBehaviour
    {
        private sealed class PoolState
        {
            public string Key;
            public GameObject Prefab;
            public Transform Root;
            public AddressableAssetResidencyService.PrefabLease Lease;
            public readonly Queue<GameObject> Available = new Queue<GameObject>();
            public readonly HashSet<GameObject> All = new HashSet<GameObject>();
            public readonly HashSet<GameObject> Active = new HashSet<GameObject>();
            public int Maximum;
            public bool Loading;
        }

        private readonly Dictionary<string, PoolState> pools =
            new Dictionary<string, PoolState>(StringComparer.Ordinal);
        private AddressableAssetResidencyService residency;
        private bool shuttingDown;

        public int PoolCount => pools.Count;

        public void Configure(AddressableAssetResidencyService service)
        {
            residency = service;
        }

        private void Awake()
        {
            if (residency == null)
                residency = FindFirstObjectByType<AddressableAssetResidencyService>();
        }

        public void WarmPool(
            string address, int prewarmCount, int maximumSize, Action<bool, string> completed = null)
        {
            if (shuttingDown)
            {
                if (completed != null) completed(false, "Prefab pool is shutting down.");
                return;
            }
            if (residency == null)
            {
                if (completed != null) completed(false, "No AddressableAssetResidencyService is available.");
                return;
            }
            if (string.IsNullOrWhiteSpace(address))
            {
                if (completed != null) completed(false, "Pool address is empty.");
                return;
            }

            string key = address.Trim();
            PoolState existing;
            if (pools.TryGetValue(key, out existing))
            {
                if (completed != null)
                    completed(!existing.Loading, existing.Loading ? "Pool is still loading." : string.Empty);
                return;
            }

            int maximum = Mathf.Max(1, maximumSize);
            int prewarm = AddressablePoolPolicy.NormalizePrewarmCount(prewarmCount, maximum);
            PoolState state = new PoolState
            {
                Key = key,
                Maximum = maximum,
                Loading = true,
                Root = new GameObject("POOL_" + key.Replace('/', '_')).transform
            };
            state.Root.SetParent(transform, false);
            pools.Add(key, state);

            residency.AcquirePrefab(key, (lease, error) =>
            {
                if (shuttingDown || state == null)
                {
                    if (lease != null) lease.Dispose();
                    return;
                }
                if (lease == null)
                {
                    pools.Remove(key);
                    Destroy(state.Root.gameObject);
                    if (completed != null) completed(false, error);
                    return;
                }

                state.Lease = lease;
                state.Prefab = lease.Asset;
                if (state.Prefab == null)
                {
                    DisposePool(state);
                    pools.Remove(key);
                    if (completed != null) completed(false, "Loaded prefab lease returned no asset.");
                    return;
                }

                for (int i = 0; i < prewarm; i++)
                    CreateInstance(state);
                state.Loading = false;
                if (completed != null) completed(true, string.Empty);
            });
        }

        public bool IsReady(string address)
        {
            PoolState state;
            return !string.IsNullOrWhiteSpace(address) &&
                   pools.TryGetValue(address.Trim(), out state) &&
                   !state.Loading && state.Prefab != null;
        }

        public int GetAvailableCount(string address)
        {
            PoolState state;
            return !string.IsNullOrWhiteSpace(address) &&
                   pools.TryGetValue(address.Trim(), out state)
                ? state.Available.Count
                : 0;
        }

        public int GetActiveCount(string address)
        {
            PoolState state;
            return !string.IsNullOrWhiteSpace(address) &&
                   pools.TryGetValue(address.Trim(), out state)
                ? state.Active.Count
                : 0;
        }

        public bool TryRent(string address, Vector3 position, Quaternion rotation, out GameObject instance)
        {
            instance = null;
            if (string.IsNullOrWhiteSpace(address)) return false;
            PoolState state;
            if (!pools.TryGetValue(address.Trim(), out state) || state.Loading || state.Prefab == null)
                return false;

            while (state.Available.Count > 0 && instance == null)
                instance = state.Available.Dequeue();
            if (instance == null)
            {
                if (state.All.Count >= state.Maximum) return false;
                instance = CreateInstance(state);
            }
            if (instance == null) return false;

            state.Active.Add(instance);
            instance.transform.SetParent(null, true);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.localScale = state.Prefab.transform.localScale;
            instance.SetActive(true);
            IPooledObject[] hooks = instance.GetComponents<IPooledObject>();
            for (int i = 0; i < hooks.Length; i++) hooks[i].OnPoolRent();
            return true;
        }

        public bool Return(GameObject instance)
        {
            if (instance == null) return false;
            foreach (PoolState state in pools.Values)
            {
                if (!state.Active.Remove(instance)) continue;
                IPooledObject[] hooks = instance.GetComponents<IPooledObject>();
                for (int i = 0; i < hooks.Length; i++) hooks[i].OnPoolReturn();
                instance.SetActive(false);
                instance.transform.SetParent(state.Root, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = state.Prefab != null
                    ? state.Prefab.transform.localScale
                    : Vector3.one;
                state.Available.Enqueue(instance);
                return true;
            }
            return false;
        }

        public void ClearPool(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return;
            PoolState state;
            if (!pools.TryGetValue(address.Trim(), out state)) return;
            pools.Remove(state.Key);
            DisposePool(state);
        }

        private GameObject CreateInstance(PoolState state)
        {
            if (state == null || state.Prefab == null || state.All.Count >= state.Maximum)
                return null;
            GameObject instance = Instantiate(state.Prefab, state.Root);
            instance.name = state.Prefab.name + "_Pooled";
            instance.SetActive(false);
            state.All.Add(instance);
            state.Available.Enqueue(instance);
            return instance;
        }

        private void DisposePool(PoolState state)
        {
            if (state == null) return;
            foreach (GameObject instance in state.All)
                if (instance != null) Destroy(instance);
            state.All.Clear();
            state.Active.Clear();
            state.Available.Clear();
            if (state.Root != null) Destroy(state.Root.gameObject);
            if (state.Lease != null) state.Lease.Dispose();
            state.Lease = null;
            state.Prefab = null;
        }

        private void OnDestroy()
        {
            shuttingDown = true;
            foreach (PoolState state in pools.Values)
                DisposePool(state);
            pools.Clear();
        }
    }

    public static class AddressablePoolPolicy
    {
        public static int NormalizePrewarmCount(int requested, int maximum)
        {
            int safeMaximum = Mathf.Max(1, maximum);
            return Mathf.Clamp(requested, 0, safeMaximum);
        }

        public static bool ShouldReleaseUnused(int referenceCount, float elapsedSeconds, float graceSeconds)
        {
            return referenceCount <= 0 &&
                   !float.IsNaN(elapsedSeconds) && !float.IsInfinity(elapsedSeconds) &&
                   !float.IsNaN(graceSeconds) && !float.IsInfinity(graceSeconds) &&
                   elapsedSeconds >= Mathf.Max(0f, graceSeconds);
        }
    }
}
