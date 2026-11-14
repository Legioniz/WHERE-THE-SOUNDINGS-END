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
        public void WreckArtifacts_UseWreckLocalSpaceInsteadOfWorldOrigin()
        {
            GreyhavenEnvironmentBuilder.BuildEnvironmentalArtifacts(
                roots[0].transform, roots[1].transform, roots[2].transform,
                roots[3].transform, roots[4].transform,
                materials[0], materials[1], materials[2], materials[3], materials[4],
                materials[5], materials[6], materials[7], materials[8]);

            Transform wreck = roots[4].transform.Find("Greyhaven_Outer_Channel_Wreck_Field");
            Assert.That(wreck.Find("Wreck_Environmental_Artifacts/Wreck_Keel_Spine_Fragment"), Is.Not.Null);
            Assert.That(wreck.Find("Wreck_Environmental_Artifacts/Wreck_Broken_Rib_Port_A"), Is.Not.Null);
            wreck.position = new Vector3(30f, -0.25f, 55f);
            Transform bell = wreck.Find("Wreck_Environmental_Artifacts/Wreck_Bell_Rim");
            Assert.That(bell, Is.Not.Null);
            Assert.That(bell.localPosition.x, Is.EqualTo(-1.35f).Within(0.001f));
            Assert.That(bell.localPosition.y, Is.EqualTo(-0.62f).Within(0.001f));
            Assert.That(bell.localPosition.z, Is.EqualTo(0.55f).Within(0.001f));
            Assert.That(bell.position.x, Is.EqualTo(28.65f).Within(0.001f));
            Assert.That(bell.position.y, Is.EqualTo(-0.87f).Within(0.001f));
            Assert.That(bell.position.z, Is.EqualTo(55.55f).Within(0.001f));
        }

        [Test]
        public void Build_BespokeMaritimePassCreatesDistinctRegionalAssetsWithoutGameplayComponents()
        {
            GreyhavenMaritimeAssetBuilder.Build(
                roots[0].transform, roots[1].transform, roots[2].transform, roots[3].transform,
                materials[0], materials[1], materials[2], materials[3], materials[4],
                materials[5], materials[6], materials[7], materials[8]);

            Assert.That(roots[0].transform.Find("Harbour_Bespoke_Maritime_Assets/West_Mooring_Bollard_Shaft"), Is.Not.Null);
            Assert.That(roots[1].transform.Find("Village_Working_Life_Assets/Repair_Shed_Hand_Winch_Bed"), Is.Not.Null);
            Assert.That(roots[2].transform.Find("Estuary_Ecological_And_Shoreline_Assets/Mangrove_Roots_And_Canopy_Variants/East_Mangrove_01"), Is.Not.Null);
            Assert.That(roots[2].transform.Find("Estuary_Ecological_And_Shoreline_Assets/Tide_Line_Wrack_And_Debris/Storm_Tossed_Driftwood_A_Trunk"), Is.Not.Null);
            Assert.That(roots[3].transform.Find("Lighthouse_Headland_Maritime_Assets/Keepers_Weather_Station_Tripod_Leg_0"), Is.Not.Null);

            int rendererCount = 0;
            for (int i = 0; i < 4; i++)
            {
                rendererCount += roots[i].GetComponentsInChildren<Renderer>(true).Length;
                Assert.That(roots[i].GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(roots[i].GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
                Assert.That(roots[i].GetComponentsInChildren<Rigidbody>(true), Is.Empty);
                Assert.That(roots[i].GetComponentsInChildren<Light>(true), Is.Empty);
            }
            Assert.That(rendererCount, Is.GreaterThanOrEqualTo(250), "The art pass should create a substantial but static set of assembled assets.");
        }

        [Test]
        public void EstuaryRemains_AreSplitAcrossStreamingOwnershipRoots()
        {
            GreyhavenEnvironmentBuilder.BuildEnvironmentalArtifacts(
                roots[0].transform, roots[1].transform, roots[2].transform,
                roots[3].transform, roots[4].transform,
                materials[0], materials[1], materials[2], materials[3], materials[4],
                materials[5], materials[6], materials[7], materials[8]);

            Transform west = roots[2].transform.Find("Estuary_Environmental_Artifacts");
            Transform east = roots[2].transform.Find("Eastern_Mudflat_Debris_Artifacts");
            Assert.That(west, Is.Not.Null);
            Assert.That(east, Is.Not.Null);
            Assert.That(west.parent, Is.SameAs(roots[2].transform));
            Assert.That(east.parent, Is.SameAs(roots[2].transform));
            Assert.That(west.position.x, Is.LessThan(0f));
            Assert.That(east.position.x, Is.GreaterThan(0f));
            Assert.That(west.Find("Western_Mudflat_Debris_Artifacts/Lower_Inlet_Landing_Remains/Landing_Stone_Footing_0"), Is.Not.Null);
            Assert.That(west.Find("Western_Mudflat_Debris_Artifacts/Old_Lower_Settlement_Foundation_Remains/Storehouse_Foundation_North_Wall"), Is.Not.Null);
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
                Assert.That(roots[i].GetComponentsInChildren<Rigidbody>(true), Is.Empty,
                    "Environmental artifact dressing must remain static.");
                Assert.That(roots[i].GetComponentsInChildren<Light>(true), Is.Empty,
                    "Environmental artifact dressing must not create per-prop lights.");

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
