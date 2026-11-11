using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Editor;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class BoatVisualModelTests
    {
        [Test]
        public void WorkboatVisualModel_BuildsTaperedHullAndRaisedWheelhouse()
        {
            GameObject vessel = new GameObject("Workboat_Model_Test");
            try
            {
                Transform visual = BoatVisualModelBuilder.Build(vessel.transform);
                Assert.IsNotNull(visual);

                Transform hull = visual.Find("Hull_Shell");
                Assert.IsNotNull(hull);
                MeshFilter hullFilter = hull.GetComponent<MeshFilter>();
                Assert.IsNotNull(hullFilter);
                Assert.IsNotNull(hullFilter.sharedMesh);
                Assert.That(hullFilter.sharedMesh.vertexCount, Is.GreaterThan(40));
                Assert.That(hullFilter.sharedMesh.bounds.size.x, Is.InRange(1.8f, 2.2f));
                Assert.That(hullFilter.sharedMesh.bounds.size.z, Is.InRange(5.4f, 5.7f));

                Transform cabin = visual.Find("Wheelhouse_Shell");
                Assert.IsNotNull(cabin);
                Assert.IsNotNull(cabin.GetComponent<MeshRenderer>());
                Assert.IsNotNull(visual.Find("Forward_Windscreen"));
                Assert.IsNotNull(visual.Find("Outboard_Engine_Cowl"));
                Assert.IsNotNull(visual.Find("Outboard_Propeller_Rotor"));
                Assert.IsNotNull(visual.Find("Outboard_Propeller_Rotor/Outboard_Propeller_Blade_A"));
                Assert.IsNotNull(visual.Find("Outboard_Propeller_Rotor/Outboard_Propeller_Blade_B"));
            }
            finally
            {
                Object.DestroyImmediate(vessel);
            }
        }

        [Test]
        public void WorkboatVisualModel_KeepsAllDecorativePartsColliderFree()
        {
            GameObject vessel = new GameObject("Workboat_Collider_Test");
            try
            {
                BoxCollider rootHull = vessel.AddComponent<BoxCollider>();
                Transform visual = BoatVisualModelBuilder.Build(vessel.transform);

                Assert.AreSame(rootHull, vessel.GetComponent<BoxCollider>());
                Assert.AreEqual(0, visual.GetComponentsInChildren<Collider>(true).Length);
                Assert.AreEqual(0, visual.GetComponentsInChildren<Rigidbody>(true).Length);
            }
            finally
            {
                Object.DestroyImmediate(vessel);
            }
        }
    }
}
