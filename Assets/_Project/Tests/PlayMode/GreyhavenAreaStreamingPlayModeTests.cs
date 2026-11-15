using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using WhereTheSoundingsEnd.Boat;
using WhereTheSoundingsEnd.CameraSystem;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.Streaming;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Tests.PlayMode
{
    public sealed class GreyhavenAreaStreamingPlayModeTests
    {
        private Scene _greyhavenScene;
        private bool _loadedSceneForTest;
        private Rigidbody _boatBody;
        private bool _originalKinematic;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _greyhavenScene = SceneManager.GetSceneByName("Greyhaven_Blockout");
            if (!_greyhavenScene.IsValid() || !_greyhavenScene.isLoaded)
            {
                AsyncOperation load = SceneManager.LoadSceneAsync("Greyhaven_Blockout", LoadSceneMode.Additive);
                Assert.That(load, Is.Not.Null, "Greyhaven_Blockout must be included in Build Settings.");
                yield return load;
                _loadedSceneForTest = true;
                _greyhavenScene = SceneManager.GetSceneByName("Greyhaven_Blockout");
            }

            Assert.That(_greyhavenScene.IsValid() && _greyhavenScene.isLoaded, Is.True);
            SceneManager.SetActiveScene(_greyhavenScene);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_boatBody != null)
            {
                _boatBody.isKinematic = _originalKinematic;
                _boatBody.position = _originalPosition;
                _boatBody.rotation = _originalRotation;
            }

            if (_loadedSceneForTest && _greyhavenScene.IsValid() && _greyhavenScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(_greyhavenScene);
                if (unload != null) yield return unload;
            }

            _loadedSceneForTest = false;
            _boatBody = null;
        }


        [UnityTest]
        public IEnumerator GreyhavenStartsWithPlayerControlledVesselAndCoreLoop()
        {
            BoatPrototypeController boat = FindInScene<BoatPrototypeController>(_greyhavenScene);
            Assert.That(boat, Is.Not.Null, "The generated Greyhaven scene must contain the player workboat.");
            Assert.That(boat.enabled, Is.True);
            Assert.That(boat.InputActions, Is.Not.Null, "Boat controls must be assigned by the scene builder.");

            Rigidbody body = boat.GetComponent<Rigidbody>();
            Assert.That(body, Is.Not.Null);
            Assert.That(body.isKinematic, Is.False, "The player boat must be physics driven.");
            Assert.That(boat.GetComponent<BoatHydrodynamics>(), Is.Not.Null);
            Assert.That(boat.GetComponent<SpatialCargoHold>(), Is.Not.Null);
            Assert.That(boat.GetComponent<HarvestingMinigameController>(), Is.Not.Null);
            Assert.That(boat.GetComponent<SoundingRigController>(), Is.Not.Null);
            Assert.That(boat.GetComponent<VesselConditionController>(), Is.Not.Null);
            Assert.That(boat.GetComponent<VesselFuelController>(), Is.Not.Null);
            Assert.That(boat.GetComponent<EconomyWalletController>(), Is.Not.Null);
            Assert.That(boat.GetComponent<MaraFishMarketController>(), Is.Not.Null);
            Assert.That(boat.GetComponent<GreyhavenVendorServices>(), Is.Not.Null);

            Camera camera = FindInScene<Camera>(_greyhavenScene);
            Assert.That(camera, Is.Not.Null, "A gameplay camera must be present.");
            Assert.That(camera.CompareTag("MainCamera"), Is.True);
            CameraFollow2_5D follow = camera.GetComponent<CameraFollow2_5D>();
            Assert.That(follow, Is.Not.Null);
            Assert.That(follow.Target, Is.EqualTo(boat.transform));

            Assert.That(FindInScene<SeaStateDirector>(_greyhavenScene), Is.Not.Null);
            Assert.That(FindInScene<SaveGameCoordinator>(_greyhavenScene), Is.Not.Null);
            Assert.That(FindInScene<AutosaveController>(_greyhavenScene), Is.Not.Null);
            Assert.That(FindInScene<AreaStreamingDirector>(_greyhavenScene), Is.Not.Null);

            yield return null;
            Assert.That(body == null || body.isKinematic, Is.False,
                "The vessel must remain physics controlled after the first runtime frame.");
        }

        [UnityTest]
        public IEnumerator AdditiveAreasPreloadAndRetireWhilePersistentGameplaySceneStaysLoaded()
        {
            AreaStreamingDirector director = FindInScene<AreaStreamingDirector>(_greyhavenScene);
            BoatPrototypeController boat = FindInScene<BoatPrototypeController>(_greyhavenScene);
            Assert.That(director, Is.Not.Null, "Greyhaven must have one AreaStreamingDirector.");
            Assert.That(boat, Is.Not.Null, "Greyhaven must retain its persistent vessel controller.");

            _boatBody = boat.GetComponent<Rigidbody>();
            Assert.That(_boatBody, Is.Not.Null);
            _originalKinematic = _boatBody.isKinematic;
            _originalPosition = _boatBody.position;
            _originalRotation = _boatBody.rotation;
            _boatBody.isKinematic = true;

            float deadline = Time.realtimeSinceStartup + 30f;
            while (director.LoadedAreaCount < 3 && Time.realtimeSinceStartup < deadline)
                yield return null;

            Assert.That(director.LoadedAreaCount, Is.GreaterThanOrEqualTo(3),
                "Terrain, harbour and village should preload at startup.");
            Assert.That(director.FailedAreaCount, Is.Zero);

            _boatBody.position = new Vector3(-86f, _originalPosition.y, -22f);
            boat.transform.position = _boatBody.position;
            Physics.SyncTransforms();

            deadline = Time.realtimeSinceStartup + 30f;
            while (!director.IsAreaLoaded("greyhaven.lighthouse") && Time.realtimeSinceStartup < deadline)
                yield return null;

            Assert.That(director.IsAreaLoaded("greyhaven.lighthouse"), Is.True,
                "The lighthouse scene should preload before the vessel reaches its area.");
            Assert.That(_greyhavenScene.isLoaded, Is.True,
                "The persistent gameplay scene must remain loaded during additive transitions.");

            _boatBody.position = _originalPosition;
            boat.transform.position = _originalPosition;
            Physics.SyncTransforms();

            deadline = Time.realtimeSinceStartup + 20f;
            while (director.IsAreaLoaded("greyhaven.lighthouse") && Time.realtimeSinceStartup < deadline)
                yield return null;

            Assert.That(director.IsAreaLoaded("greyhaven.lighthouse"), Is.False,
                "A distant lighthouse area should unload after its grace period.");
            Assert.That(director.LoadedAreaCount, Is.GreaterThanOrEqualTo(3),
                "Returning to Greyhaven should restore its startup areas.");
            Assert.That(director.FailedAreaCount, Is.Zero,
                "Repeated transitions must not leave an area in a failed state.");
            Assert.That(_greyhavenScene.isLoaded, Is.True);
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(true);
                if (component != null) return component;
            }

            return null;
        }
    }
}
