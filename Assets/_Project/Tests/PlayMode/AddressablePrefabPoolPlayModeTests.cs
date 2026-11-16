using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WhereTheSoundingsEnd.Streaming;

namespace WhereTheSoundingsEnd.Tests.PlayMode
{
    public sealed class AddressablePrefabPoolPlayModeTests
    {
        private GameObject root;
        private AddressablePrefabPool pool;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("Addressable_Pool_PlayMode_Test");
            AddressableAssetResidencyService residency = root.AddComponent<AddressableAssetResidencyService>();
            pool = root.AddComponent<AddressablePrefabPool>();
            pool.Configure(residency);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WarmPool_RentsReturnsAndEnforcesCapacityWithoutDuplicateRentals()
        {
            bool completed = false;
            bool succeeded = false;
            string error = string.Empty;
            pool.WarmPool("runtime.panic.shadow", 1, 1, (success, reason) =>
            {
                completed = true;
                succeeded = success;
                error = reason;
            });

            float deadline = Time.realtimeSinceStartup + 20f;
            while (!completed && Time.realtimeSinceStartup < deadline)
                yield return null;

            Assert.That(completed, Is.True, "Addressables did not finish loading the generated shadow prefab.");
            Assert.That(succeeded, Is.True, error);
            Assert.That(pool.IsReady("runtime.panic.shadow"), Is.True);
            Assert.That(pool.GetAvailableCount("runtime.panic.shadow"), Is.EqualTo(1));

            GameObject first;
            Assert.That(pool.TryRent("runtime.panic.shadow", Vector3.one, Quaternion.identity, out first), Is.True);
            Assert.That(first, Is.Not.Null);
            Assert.That(first.activeSelf, Is.True);
            Assert.That(pool.GetActiveCount("runtime.panic.shadow"), Is.EqualTo(1));

            GameObject overflow;
            Assert.That(pool.TryRent("runtime.panic.shadow", Vector3.zero, Quaternion.identity, out overflow), Is.False,
                "The pool must refuse to exceed its configured maximum.");
            Assert.That(pool.Return(first), Is.True);
            Assert.That(pool.Return(first), Is.False, "A returned instance cannot be returned twice.");
            Assert.That(pool.GetAvailableCount("runtime.panic.shadow"), Is.EqualTo(1));

            GameObject reused;
            Assert.That(pool.TryRent("runtime.panic.shadow", Vector3.zero, Quaternion.identity, out reused), Is.True);
            Assert.That(reused, Is.SameAs(first), "A returned object should be reused, not instantiated again.");
            Assert.That(pool.Return(reused), Is.True);

            pool.ClearPool("runtime.panic.shadow");
            Assert.That(pool.PoolCount, Is.Zero);
        }
    }
}
