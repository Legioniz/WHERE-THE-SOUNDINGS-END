using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using WhereTheSoundingsEnd.Boat;
using WhereTheSoundingsEnd.CameraSystem;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Interaction;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.Streaming;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// Unity-side preflight for generated scene integrity and the persistent gameplay object graph.
    /// This complements static CI checks; it does not replace Editor compilation or runtime tests.
    /// </summary>
    public static class ProjectReferenceIntegrityValidator
    {
        private const string PersistentScene = "Assets/_Project/Scenes/Greyhaven_Blockout.unity";
        private const string InputActionsPath = "Assets/_Project/Settings/Input/PlayerControls.inputactions";

        private static readonly string[] RequiredBuildScenes =
        {
            PersistentScene,
            "Assets/_Project/Scenes/Greyhaven_Terrain.unity",
            "Assets/_Project/Scenes/Greyhaven_Harbour.unity",
            "Assets/_Project/Scenes/Greyhaven_Village.unity",
            "Assets/_Project/Scenes/Greyhaven_Estuary_West.unity",
            "Assets/_Project/Scenes/Greyhaven_Estuary_East.unity",
            "Assets/_Project/Scenes/Greyhaven_Lighthouse.unity",
            "Assets/_Project/Scenes/OuterShoals_SurveySandbox.unity",
            "Assets/_Project/Scenes/HollowTrench_SurveySandbox.unity"
        };

        [MenuItem("Tools/WHERE THE SOUNDINGS END/Validate Project References")]
        public static void ValidateFromMenu()
        {
            try
            {
                ValidateOrThrow();
                Debug.Log("Project reference-integrity preflight passed for all nine generated scenes.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Project reference-integrity preflight failed: " + exception.Message);
                throw;
            }
        }

        public static void ValidateOrThrow()
        {
            ValidateBuildSettings();
            InputActionAsset controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (controls == null)
                throw new InvalidOperationException("The player Input Actions asset could not be imported: " + InputActionsPath);

            string areaError;
            if (!GreyhavenEnvironmentBuilder.HasGeneratedAreaScenes(out areaError))
                throw new InvalidOperationException("Greyhaven additive-area validation failed: " + areaError);

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                for (int i = 0; i < RequiredBuildScenes.Length; i++)
                    ValidateScene(RequiredBuildScenes[i]);

                Scene persistent = SceneManager.GetSceneByPath(PersistentScene);
                ValidatePersistentGameplayGraph(persistent, controls);
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }

        private static void ValidateBuildSettings()
        {
            HashSet<string> enabled = new HashSet<string>(StringComparer.Ordinal);
            EditorBuildSettingsScene[] configured = EditorBuildSettings.scenes;
            for (int i = 0; i < configured.Length; i++)
                if (configured[i].enabled)
                    enabled.Add(configured[i].path);

            for (int i = 0; i < RequiredBuildScenes.Length; i++)
            {
                string path = RequiredBuildScenes[i];
                if (!enabled.Contains(path))
                    throw new InvalidOperationException("Required generated scene is not enabled in Build Settings: " + path);
                if (!File.Exists(path) || AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                    throw new FileNotFoundException("Required generated scene is missing or not imported: " + path, path);
            }
        }

        private static void ValidateScene(string path)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("Unity could not open generated scene: " + path);

            GameObject[] roots = scene.GetRootGameObjects();
            if (roots == null || roots.Length == 0)
                throw new InvalidOperationException("Generated scene has no root GameObjects: " + path);

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] hierarchy = roots[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int objectIndex = 0; objectIndex < hierarchy.Length; objectIndex++)
                {
                    GameObject current = hierarchy[objectIndex].gameObject;
                    int missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(current);
                    if (missingScripts > 0)
                        throw new InvalidOperationException(
                            "Generated scene '" + path + "' contains " + missingScripts +
                            " missing MonoBehaviour script reference(s) on '" + current.name + "'.");
                }
            }

            string[] dependencies = AssetDatabase.GetDependencies(path, true);
            for (int dependencyIndex = 0; dependencyIndex < dependencies.Length; dependencyIndex++)
            {
                string dependency = dependencies[dependencyIndex];
                if (dependency.StartsWith("Assets/", StringComparison.Ordinal) &&
                    AssetDatabase.LoadMainAssetAtPath(dependency) == null)
                    throw new InvalidOperationException(
                        "Generated scene '" + path + "' has an unresolved project asset dependency: " + dependency);
            }
        }

        private static void ValidatePersistentGameplayGraph(Scene scene, InputActionAsset expectedControls)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("Persistent Greyhaven gameplay scene is not loaded for validation.");

            BoatPrototypeController boat = FindComponent<BoatPrototypeController>(scene);
            Require(boat, "player BoatPrototypeController");
            if (boat.InputActions != expectedControls)
                throw new InvalidOperationException("Player vessel does not reference the imported PlayerControls.inputactions asset.");

            Require(boat.GetComponent<Rigidbody>(), "player vessel Rigidbody");
            Require(boat.GetComponent<BoatHydrodynamics>(), "player vessel BoatHydrodynamics");
            Require(boat.GetComponent<SpatialCargoHold>(), "player vessel SpatialCargoHold");
            Require(boat.GetComponent<HarvestingMinigameController>(), "player vessel HarvestingMinigameController");
            Require(boat.GetComponent<SoundingRigController>(), "player vessel SoundingRigController");
            Require(boat.GetComponent<VesselConditionController>(), "player vessel VesselConditionController");
            Require(boat.GetComponent<VesselFuelController>(), "player vessel VesselFuelController");
            Require(boat.GetComponent<EconomyWalletController>(), "player vessel EconomyWalletController");
            Require(boat.GetComponent<MaraFishMarketController>(), "player vessel MaraFishMarketController");
            Require(boat.GetComponent<GreyhavenVendorServices>(), "player vessel GreyhavenVendorServices");
            Require(boat.GetComponent<HarbourmasterResearchController>(), "player vessel HarbourmasterResearchController");
            Require(boat.GetComponent<VesselEquipmentController>(), "player vessel VesselEquipmentController");
            Require(boat.GetComponent<ExpeditionEconomyTelemetry>(), "player vessel ExpeditionEconomyTelemetry");
            Require(boat.GetComponent<RegionalChartController>(), "player vessel RegionalChartController");

            Camera camera = FindComponent<Camera>(scene);
            Require(camera, "gameplay Camera");
            if (!camera.CompareTag("MainCamera"))
                throw new InvalidOperationException("Gameplay Camera is not tagged MainCamera.");
            CameraFollow2_5D follow = camera.GetComponent<CameraFollow2_5D>();
            Require(follow, "CameraFollow2_5D");
            if (follow.Target != boat.transform)
                throw new InvalidOperationException("CameraFollow2_5D target is not the generated player vessel.");

            Require(FindComponent<SeaStateDirector>(scene), "SeaStateDirector");
            Require(FindComponent<SaveGameCoordinator>(scene), "SaveGameCoordinator");
            Require(FindComponent<AutosaveController>(scene), "AutosaveController");
            Require(FindComponent<AreaStreamingDirector>(scene), "AreaStreamingDirector");
            Require(FindComponent<DocksideHubController>(scene), "DocksideHubController");
        }

        private static T FindComponent<T>(Scene scene) where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }

            return null;
        }

        private static void Require(UnityEngine.Object value, string description)
        {
            if (value == null)
                throw new InvalidOperationException("Persistent Greyhaven scene is missing required reference: " + description + ".");
        }
    }
}
