using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Editor;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class GreyhavenEnvironmentalArtifactTests
    {
        private readonly List<GameObject> roots = new List<GameObject>();
        private readonly List<Material> materials = new List<Material>();
        private Shader shader;

        [SetUp]
        public void SetUp()
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Assert.That(shader, Is.Not.Null, "A supported test shader is required.");

            for (int i = 0; i < 9; i++)
                materials.Add(new Material(shader));

            for (int i = 0; i < 5; i++)
                roots.Add(new GameObject("Artifact_Test_Area_" + i));

            GameObject wreck = new GameObject("Greyhaven_Outer_Channel_Wreck_Field");
            wreck.transform.SetParent(roots[4].transform, false);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < roots.Count; i++)
                if (roots[i] != null) Object.DestroyImmediate(roots[i]);
            for (int i = 0; i < materials.Count; i++)
                if (materials[i] != null) Object.DestroyImmediate(materials[i]);
            roots.Clear();
            materials.Clear();
        }

        [Test]
        public void BuildEnvironmentalArtifacts_CreatesDistrictDressingWithoutGameplayComponents()
        {
            GreyhavenEnvironmentBuilder.BuildEnvironmentalArtifacts(
                roots[0].transform, roots[1].transform, roots[2].transform,
                roots[3].transform, roots[4].transform,
                materials[0], materials[1], materials[2], materials[3], materials[4],
                materials[5], materials[6], materials[7], materials[8]);

            Assert.That(roots[0].transform.Find("Harbour_Environmental_Artifacts"), Is.Not.Null);
            Assert.That(roots[1].transform.Find("Village_Environmental_Artifacts"), Is.Not.Null);
            Assert.That(roots[2].transform.Find("Estuary_Environmental_Artifacts"), Is.Not.Null);
            Assert.That(roots[3].transform.Find("Lighthouse_Keepers_Abandoned_Artifacts"), Is.Not.Null);
            Assert.That(roots[4].transform.Find("Greyhaven_Outer_Channel_Wreck_Field/Wreck_Environmental_Artifacts"), Is.Not.Null);

            int rendererCount = 0;
            for (int i = 0; i < roots.Count; i++)
            {
                Renderer[] renderers = roots[i].GetComponentsInChildren<Renderer>(true);
                rendererCount += renderers.Length;
                Assert.That(roots[i].GetComponentsInChildren<Collider>(true), Is.Empty,
                    "Environmental artifact dressing must not add gameplay collision.");
                Assert.That(roots[i].GetComponentsInChildren<MonoBehaviour>(true), Is.Empty,
                    "Environmental artifact dressing must not add runtime gameplay components.");

                foreach (Renderer renderer in renderers)
                {
                    Vector3 position = renderer.transform.position;
                    Assert.That(float.IsNaN(position.x) || float.IsInfinity(position.x), Is.False);
                    Assert.That(float.IsNaN(position.y) || float.IsInfinity(position.y), Is.False);
                    Assert.That(float.IsNaN(position.z) || float.IsInfinity(position.z), Is.False);
                }
            }

            Assert.That(rendererCount, Is.GreaterThanOrEqualTo(60),
                "The Greyhaven pass should provide a visibly meaningful set of artifact details.");
        }
    }
}
