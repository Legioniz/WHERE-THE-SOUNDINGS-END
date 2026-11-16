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

            GameObject fishSchool = AssetDatabase.LoadAssetAtPath<GameObject>(
                WhereTheSoundingsEnd.Editor.AddressableRuntimeContentBuilder.FishSchoolPrefabPath);
            GameObject debris = AssetDatabase.LoadAssetAtPath<GameObject>(
                WhereTheSoundingsEnd.Editor.AddressableRuntimeContentBuilder.FloatingDebrisPrefabPath);
            GameObject salvage = AssetDatabase.LoadAssetAtPath<GameObject>(
                WhereTheSoundingsEnd.Editor.AddressableRuntimeContentBuilder.SalvageVisualPrefabPath);
            GameObject wreck = AssetDatabase.LoadAssetAtPath<GameObject>(
                WhereTheSoundingsEnd.Editor.AddressableRuntimeContentBuilder.WreckDetailPrefabPath);

            Assert.That(fishSchool, Is.Not.Null);
            Assert.That(debris, Is.Not.Null);
            Assert.That(salvage, Is.Not.Null);
            Assert.That(wreck, Is.Not.Null);
            Assert.That(fishSchool.activeSelf, Is.False);
            Assert.That(debris.activeSelf, Is.False);
            Assert.That(salvage.activeSelf, Is.False);
            Assert.That(wreck.activeSelf, Is.False);
            Assert.That(fishSchool.transform.childCount, Is.GreaterThanOrEqualTo(10));
            Assert.That(debris.transform.childCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(salvage.transform.childCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(wreck.transform.childCount, Is.GreaterThanOrEqualTo(3));
        }
    }
}
