using UnityEngine;

namespace WhereTheSoundingsEnd.Streaming
{
    /// <summary>
    /// Routes short-lived world presentation through bounded Addressable pools.
    /// It owns visuals only: fish stocks, salvage reservations, wreck interactions and save data
    /// remain in their persistent gameplay systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldVisualPoolDirector : MonoBehaviour
    {
        public const string FishSchoolAddress = "runtime.world.fish_school";
        public const string FloatingDebrisAddress = "runtime.world.floating_debris";
        public const string SalvageVisualAddress = "runtime.world.salvage_visual";
        public const string WreckDetailAddress = "runtime.world.wreck_detail";

        [SerializeField, Min(0)] private int fishSchoolPrewarm = 2;
        [SerializeField, Min(1)] private int fishSchoolCapacity = 8;
        [SerializeField, Min(0)] private int debrisPrewarm = 2;
        [SerializeField, Min(1)] private int debrisCapacity = 12;
        [SerializeField, Min(0)] private int salvagePrewarm = 1;
        [SerializeField, Min(1)] private int salvageCapacity = 8;
        [SerializeField, Min(0)] private int wreckPrewarm = 1;
        [SerializeField, Min(1)] private int wreckCapacity = 10;

        private AddressablePrefabPool pool;
        private bool warmRequested;

        public void Configure(AddressablePrefabPool prefabPool)
        {
            pool = prefabPool;
        }

        private void Awake()
        {
            if (pool == null) pool = FindFirstObjectByType<AddressablePrefabPool>();
        }

        private void Start()
        {
            WarmPools();
        }

        public void WarmPools()
        {
            if (pool == null || warmRequested) return;
            warmRequested = true;
            pool.WarmPool(FishSchoolAddress, fishSchoolPrewarm, fishSchoolCapacity);
            pool.WarmPool(FloatingDebrisAddress, debrisPrewarm, debrisCapacity);
            pool.WarmPool(SalvageVisualAddress, salvagePrewarm, salvageCapacity);
            pool.WarmPool(WreckDetailAddress, wreckPrewarm, wreckCapacity);
        }

        public bool TrySpawnFishSchool(Vector3 position, Quaternion rotation, Transform parent, out GameObject visual)
        {
            return TrySpawn(FishSchoolAddress, position, rotation, parent, out visual);
        }

        public bool TrySpawnFloatingDebris(Vector3 position, Quaternion rotation, Transform parent, out GameObject visual)
        {
            return TrySpawn(FloatingDebrisAddress, position, rotation, parent, out visual);
        }

        public bool TrySpawnSalvageVisual(Vector3 position, Quaternion rotation, Transform parent, out GameObject visual)
        {
            return TrySpawn(SalvageVisualAddress, position, rotation, parent, out visual);
        }

        public bool TrySpawnWreckDetail(Vector3 position, Quaternion rotation, Transform parent, out GameObject visual)
        {
            return TrySpawn(WreckDetailAddress, position, rotation, parent, out visual);
        }

        public bool ReturnVisual(GameObject visual)
        {
            return pool != null && pool.Return(visual);
        }

        private bool TrySpawn(string address, Vector3 position, Quaternion rotation, Transform parent, out GameObject visual)
        {
            visual = null;
            if (pool == null) return false;
            if (!pool.IsReady(address))
            {
                pool.WarmPool(address, 0, GetCapacity(address));
                return false;
            }
            return pool.TryRent(address, position, rotation, parent, out visual);
        }

        private int GetCapacity(string address)
        {
            if (address == FishSchoolAddress) return Mathf.Max(1, fishSchoolCapacity);
            if (address == FloatingDebrisAddress) return Mathf.Max(1, debrisCapacity);
            if (address == SalvageVisualAddress) return Mathf.Max(1, salvageCapacity);
            return Mathf.Max(1, wreckCapacity);
        }
    }
}