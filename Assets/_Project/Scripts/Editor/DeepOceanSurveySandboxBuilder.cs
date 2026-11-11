using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using WhereTheSoundingsEnd.Boat;
using WhereTheSoundingsEnd.CameraSystem;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// Generates isolated survey ranges for the Outer Shoals and Hollow Trench.
    /// They are functional survey prototypes, not finished levels or a travel network.
    /// </summary>
    public static class DeepOceanSurveySandboxBuilder
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string MaterialsFolder = "Assets/_Project/Art/Materials";

        private sealed class SiteDefinition
        {
            public string Id;
            public DeepOceanPhenomenonType Type;
            public string Name;
            public float Depth;
            public Vector3 Position;
            public string Note;
            public string Evidence;
        }

        public static void EnsureScenes(InputActionAsset controls)
        {
            if (controls == null) throw new ArgumentNullException(nameof(controls));
            EnsureScene(controls, "the_outer_shoals", SceneFolder + "/OuterShoals_SurveySandbox.unity");
            EnsureScene(controls, "the_hollow_trench", SceneFolder + "/HollowTrench_SurveySandbox.unity");
            RegisterBuildScenes();
        }

        [MenuItem("Tools/WHERE THE SOUNDINGS END/Rebuild Deep Ocean Survey Sandboxes")]
        public static void RebuildSandboxes()
        {
            InputActionAsset controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/_Project/Settings/Input/PlayerControls.inputactions");
            if (controls == null) throw new FileNotFoundException("PlayerControls.inputactions is missing.");
            if (!EditorUtility.DisplayDialog(
                "Rebuild deep-ocean survey sandboxes?",
                "This replaces the generated Outer Shoals and Hollow Trench test scenes. Manual edits will be lost.",
                "Rebuild", "Cancel")) return;

            BuildScene(controls, "the_outer_shoals", SceneFolder + "/OuterShoals_SurveySandbox.unity");
            BuildScene(controls, "the_hollow_trench", SceneFolder + "/HollowTrench_SurveySandbox.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RegisterBuildScenes();
        }

        private static void EnsureScene(InputActionAsset controls, string regionId, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) BuildScene(controls, regionId, path);
        }

        private static void BuildScene(InputActionAsset controls, string regionId, string path)
        {
            EnsureFolder(SceneFolder);
            EnsureFolder(MaterialsFolder);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            bool trench = regionId == "the_hollow_trench";
            Material ocean = GetOrCreateMaterial(
                trench ? "Survey_TrenchWater.mat" : "Survey_OuterShoalsWater.mat",
                trench ? new Color(0.006f, 0.025f, 0.045f) : new Color(0.012f, 0.055f, 0.075f));
            Material markerMaterial = GetOrCreateMaterial(
                trench ? "Survey_TrenchSiteMarker.mat" : "Survey_OuterShoalsSiteMarker.mat",
                trench ? new Color(0.82f, 0.34f, 0.16f) : new Color(0.45f, 0.72f, 0.64f));

            GameObject world = new GameObject(trench
                ? "WORLD — The Hollow Trench Survey Range"
                : "WORLD — The Outer Shoals Survey Range");

            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Sheltered_Bay_Surface";
            water.transform.position = new Vector3(0f, -0.45f, 0f);
            water.transform.localScale = new Vector3(220f, 1f, 220f);
            water.GetComponent<Renderer>().sharedMaterial = ocean;
            Collider waterCollider = water.GetComponent<Collider>();
            if (waterCollider != null) UnityEngine.Object.DestroyImmediate(waterCollider);
            water.transform.SetParent(world.transform, true);
            MaritimeSurfaceBehaviour waterSurface = water.AddComponent<MaritimeSurfaceBehaviour>();
            DeepOceanSurveyRegion profile = water.AddComponent<DeepOceanSurveyRegion>();
            profile.ConfigureForRegion(regionId);

            GameObject boat = new GameObject("Player_Boat_Survey_Prototype");
            boat.transform.SetParent(world.transform, false);
            boat.transform.position = new Vector3(0f, -0.1f, -45f);
            Rigidbody body = boat.AddComponent<Rigidbody>();
            body.mass = 1200f;
            body.useGravity = true;
            body.constraints = RigidbodyConstraints.None;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearDamping = 0.04f;
            body.angularDamping = 0.18f;
            body.centerOfMass = new Vector3(0f, -0.10f, -0.12f);
            // One root collision hull owns contact; the rendered cube below is visual only.
            BoxCollider hull = boat.AddComponent<BoxCollider>();
            hull.center = new Vector3(0f, 0.04f, 0f);
            hull.size = new Vector3(2.2f, 0.8f, 6f);

            BoatVisualModelBuilder.Build(boat.transform);

            BoatPrototypeController boatController = boat.AddComponent<BoatPrototypeController>();
            boatController.InputActions = controls;
            boat.AddComponent<BoatMotorEffects>();
            BoatHydrodynamics hydrodynamics = boat.GetComponent<BoatHydrodynamics>();
            hydrodynamics.Configure(waterSurface);
            SpatialCargoHold cargoHold = boat.AddComponent<SpatialCargoHold>();
            boat.AddComponent<VesselConditionController>();

            SeaStateDirector seaState = world.AddComponent<SeaStateDirector>();
            seaState.Configure(body, null, null, cargoHold);
            world.AddComponent<SaveGameCoordinator>();
            world.AddComponent<AutosaveController>();
            AcousticWorldListener acoustic = world.AddComponent<AcousticWorldListener>();
            acoustic.Configure(seaState);
            world.AddComponent<FishermanAtmosphereDirector>();

            CreateSites(world.transform, regionId, markerMaterial);
            SoundingRigController soundingRig = boat.AddComponent<SoundingRigController>();
            soundingRig.Configure(controls, seaState, body);
            DeepOceanSiteInteractionController interactions =
                boat.AddComponent<DeepOceanSiteInteractionController>();
            interactions.Configure(controls, seaState);

            BuildCameraAndLight(world.transform, boat.transform, trench, waterSurface);
            RenderSettings.ambientLight = trench
                ? new Color(0.035f, 0.055f, 0.075f)
                : new Color(0.11f, 0.16f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = trench
                ? new Color(0.008f, 0.018f, 0.032f)
                : new Color(0.025f, 0.075f, 0.09f);
            RenderSettings.fogDensity = trench ? 0.0035f : 0.0017f;
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateSites(Transform parent, string regionId, Material markerMaterial)
        {
            SiteDefinition[] sites = regionId == "the_hollow_trench"
                ? new[]
                {
                    new SiteDefinition { Id="hollow_trench.brine_pool_01", Type=DeepOceanPhenomenonType.BrinePool,
                        Name="The Still Brine", Depth=3850f, Position=new Vector3(95f,0.1f,25f),
                        Note="A dense, chemically distinct bottom return. The brine surface is unnaturally still; this marker records survey evidence only.",
                        Evidence="Established science" },
                    new SiteDefinition { Id="hollow_trench.cold_seep_01", Type=DeepOceanPhenomenonType.ColdSeep,
                        Name="Black Slope Seep", Depth=3420f, Position=new Vector3(-135f,0.1f,105f),
                        Note="A clustered return consistent with fluid escape through sediment. Seep chemistry and local fauna require a separate controller.",
                        Evidence="Established science" },
                    new SiteDefinition { Id="hollow_trench.gas_hydrate_01", Type=DeepOceanPhenomenonType.GasHydrate,
                        Name="The Frozen Vent", Depth=4100f, Position=new Vector3(155f,0.1f,-145f),
                        Note="A high-reflectivity deposit is charted along the trench slope. Pressure and temperature stability are not simulated yet.",
                        Evidence="Established science" },
                    new SiteDefinition { Id="hollow_trench.whale_fall_01", Type=DeepOceanPhenomenonType.WhaleFall,
                        Name="The Bone Orchard", Depth=3650f, Position=new Vector3(-195f,0.1f,-165f),
                        Note="A broad organic structure rests on the bottom. Ecological succession is not yet simulated.",
                        Evidence="Established science" }
                }
                : new[]
                {
                    new SiteDefinition { Id="outer_shoals.whale_fall_01", Type=DeepOceanPhenomenonType.WhaleFall,
                        Name="The Long Descent", Depth=1920f, Position=new Vector3(95f,0.1f,25f),
                        Note="A large, low-lying target is consistent with a whale fall. The return does not establish whether active scavengers remain.",
                        Evidence="Established science" },
                    new SiteDefinition { Id="outer_shoals.hydrothermal_vent_01", Type=DeepOceanPhenomenonType.HydrothermalVent,
                        Name="Ashlight Vent Field", Depth=1640f, Position=new Vector3(-135f,0.1f,105f),
                        Note="Several narrow returns cluster along a volcanic ridge. Heat, mineral chemistry and vent fauna require a separate physical controller.",
                        Evidence="Established science" },
                    new SiteDefinition { Id="outer_shoals.cold_seep_01", Type=DeepOceanPhenomenonType.ColdSeep,
                        Name="The Quiet Seep", Depth=2470f, Position=new Vector3(155f,0.1f,-145f),
                        Note="A diffuse bottom return suggests fluid release along the slope. This marker does not simulate methane release or seep ecology.",
                        Evidence="Established science" },
                    new SiteDefinition { Id="outer_shoals.derelict_01", Type=DeepOceanPhenomenonType.DerelictVessel,
                        Name="The Uncharted Hull", Depth=1780f, Position=new Vector3(-195f,0.1f,-165f),
                        Note="A long rigid return lies across the bottom. Collision, entanglement and recovery hazards are not implemented by this marker.",
                        Evidence="Fictional world site" }
                };

            for (int i = 0; i < sites.Length; i++)
            {
                SiteDefinition definition = sites[i];
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "Survey_Site_" + definition.Id;
                marker.transform.SetParent(parent, false);
                marker.transform.position = definition.Position;
                marker.transform.localScale = new Vector3(3f, 0.55f, 3f);
                marker.GetComponent<Renderer>().sharedMaterial = markerMaterial;
                Collider collider = marker.GetComponent<Collider>();
                if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
                DeepOceanPhenomenonSite site = marker.AddComponent<DeepOceanPhenomenonSite>();
                site.Configure(definition.Id, regionId, definition.Type, definition.Name,
                    definition.Depth, 260f, 0.78f, definition.Note, definition.Evidence);
            }
        }

        private static void BuildCameraAndLight(
            Transform parent,
            Transform boat,
            bool trench,
            MaritimeSurfaceBehaviour waterSurface)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = boat.position + new Vector3(0f, 72f, -55f);
            cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 42f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 2500f;
            camera.backgroundColor = trench ? new Color(0.004f,0.014f,0.025f) : new Color(0.018f,0.055f,0.07f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();

            CameraFollow2_5D follow = cameraObject.AddComponent<CameraFollow2_5D>();
            follow.Target = boat;
            follow.Offset = new Vector3(0f, 72f, -55f);
            follow.CameraEulerAngles = new Vector3(55f, 0f, 0f);
            follow.SmoothTime = 0.3f;
            follow.SeaSurface = waterSurface;

            GameObject lightObject = new GameObject("Deep_Water_Directional_Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = trench ? 0.22f : 0.42f;
            light.color = trench ? new Color(0.28f,0.4f,0.52f) : new Color(0.48f,0.65f,0.68f);
            light.shadows = LightShadows.None;
        }

        private static Material GetOrCreateMaterial(string fileName, Color colour)
        {
            string path = MaterialsFolder + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader == null) throw new InvalidOperationException("No supported material shader was found.");
                material = new Material(shader);
                material.name = Path.GetFileNameWithoutExtension(fileName);
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = colour;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void RegisterBuildScenes()
        {
            string[] required =
            {
                "Assets/_Project/Scenes/Greyhaven_Blockout.unity",
                SceneFolder + "/Greyhaven_Terrain.unity",
                SceneFolder + "/Greyhaven_Harbour.unity",
                SceneFolder + "/Greyhaven_Village.unity",
                SceneFolder + "/Greyhaven_Estuary_West.unity",
                SceneFolder + "/Greyhaven_Estuary_East.unity",
                SceneFolder + "/Greyhaven_Lighthouse.unity",
                SceneFolder + "/OuterShoals_SurveySandbox.unity",
                SceneFolder + "/HollowTrench_SurveySandbox.unity"
            };
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < required.Length; i++)
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(required[i]) != null)
                    scenes.Add(new EditorBuildSettingsScene(required[i], true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string leaf = Path.GetFileName(folderPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
                throw new IOException("Invalid Unity folder path: " + folderPath);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
