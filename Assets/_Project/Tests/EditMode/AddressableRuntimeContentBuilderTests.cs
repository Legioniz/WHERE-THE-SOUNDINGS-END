using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class AddressableRuntimeContentBuilderTests
    {
        [Test]
        public void EnsureRuntimePrefabs_CreatesInactivePrefabsForAddressablePooling()
        {
            WhereTheSoundingsEnd.Editor.AddressableRuntimeContentBuilder.EnsureRuntimePrefabs();

            GameObject shadow = AssetDatabase.LoadAssetAtPath<GameObject>(
                WhereTheSoundingsEnd.Editor.AddressableRuntimeContentBuilder.PhantomPrefabPath);
            GameObject birds = AssetDatabase.LoadAssetAtPath<GameObject>(
                WhereTheSoundingsEnd.Editor.AddressableRuntimeContentBuilder.BirdSwarmPrefabPath);

            Assert.That(shadow, Is.Not.Null);
            Assert.That(birds, Is.Not.Null);
            Assert.That(shadow.activeSelf, Is.False);
            Assert.That(birds.activeSelf, Is.False);
            Assert.That(birds.GetComponent<WhereTheSoundingsEnd.World.PanicBirdSwarmVisual>(), Is.Not.Null);
        }
    }
}
