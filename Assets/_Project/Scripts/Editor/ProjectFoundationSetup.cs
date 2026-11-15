using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// One-time, idempotent setup for project settings and the Greyhaven environment.
    /// </summary>
    public static class ProjectFoundationSetup
    {
        private const string ProjectRoot = "Assets/_Project";
        private const string ScenePath = ProjectRoot + "/Scenes/Greyhaven_Blockout.unity";
        private const string InputActionsPath = ProjectRoot + "/Settings/Input/PlayerControls.inputactions";
        private const string RenderingPath = ProjectRoot + "/Settings/Rendering";
        private const string RendererPath = RenderingPath + "/DefaultRenderer.asset";
        private const string PipelinePath = RenderingPath + "/DefaultPipeline.asset";

        [MenuItem("Tools/WHERE THE SOUNDINGS END/Initialize Project Foundation")]
        public static void Initialize()
        {
            InitializeInternal(false);
        }

        /// <summary>
        /// Non-interactive setup entry point for a fresh checkout and licensed CI.
        /// In batch mode, stale generated scenes are rebuilt instead of showing a modal dialog.
        /// </summary>
        public static void InitializeForAutomation()
        {
            InitializeInternal(true);
        }

        private static void InitializeInternal(bool unattended)
        {
            EnsureProjectFolders();
            AssetDatabase.Refresh();
            ConfigureRendering();
            ConfigurePlayerSettings();

            InputActionAsset controls = LoadControls();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                GreyhavenEnvironmentBuilder.Build(controls, ScenePath);
            }
            else
            {
                string validationError;
                if (!GreyhavenEnvironmentBuilder.HasGeneratedAreaScenes(out validationError))
                {
                    bool rebuild = unattended || Application.isBatchMode;
                    if (!rebuild)
                    {
                        rebuild = EditorUtility.DisplayDialog(
                            "Rebuild stale Greyhaven area scenes?",
                            "One or more generated Greyhaven area scenes are missing, stale, or invalid.\n\n" +
                            validationError + "\n\n" +
                            "Rebuilding replaces the generated Greyhaven blockout and all six additive area scenes. " +
                            "Manual edits to generated Greyhaven content will be lost.",
                            "Rebuild generated Greyhaven",
                            "Keep current scenes");
                    }

                    if (rebuild)
                        GreyhavenEnvironmentBuilder.Build(controls, ScenePath);
                    else
                        Debug.LogWarning(
                            "Greyhaven area-scene preflight failed. Existing scenes were kept; affected areas may be rejected at runtime. " +
                            validationError);
                }
            }

            RegisterBuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("Project foundation initialized with the Greyhaven coastal greybox. This is not finished level art.");
        }

        [MenuItem("Tools/WHERE THE SOUNDINGS END/Rebuild Greyhaven Environment")]
        public static void RebuildEnvironment()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog(
                    "Rebuild Greyhaven?",
                    "This replaces the generated Greyhaven scene. Manual edits in that scene will be lost. Continue?",
                    "Rebuild",
                    "Cancel"))
                return;

            EnsureProjectFolders();
            InputActionAsset controls = LoadControls();
            GreyhavenEnvironmentBuilder.Build(controls, ScenePath);
            RegisterBuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static InputActionAsset LoadControls()
        {
            InputActionAsset controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (controls == null)
                throw new FileNotFoundException("Could not import the player input actions asset.", InputActionsPath);
            return controls;
        }

        private static void RegisterBuildScene()
        {
            // The sandbox builder registers Greyhaven first, followed by each generated deep-water scene.
            DeepOceanSurveySandboxBuilder.EnsureScenes(LoadControls());
        }

        private static void EnsureProjectFolders()
        {
            string[] folders =
            {
                ProjectRoot + "/Art/Materials", ProjectRoot + "/Art/Models", ProjectRoot + "/Art/Textures",
                ProjectRoot + "/Audio/Ambience", ProjectRoot + "/Audio/Music", ProjectRoot + "/Audio/SFX",
                ProjectRoot + "/Data/Creatures", ProjectRoot + "/Data/Items", ProjectRoot + "/Data/Regions",
                ProjectRoot + "/Data/Quests", ProjectRoot + "/Prefabs/Boat", ProjectRoot + "/Prefabs/Creatures",
                ProjectRoot + "/Prefabs/World", ProjectRoot + "/Scenes", ProjectRoot + "/Settings/Input",
                ProjectRoot + "/Settings/Rendering", ProjectRoot + "/Shaders", ProjectRoot + "/Tests/EditMode",
                ProjectRoot + "/Tests/PlayMode", "Assets/ThirdParty"
            };
            foreach (string folder in folders) EnsureFolder(folder);
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

        private static void ConfigureRendering()
        {
            UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            UniversalRenderPipelineAsset pipeline =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline);
            EditorUtility.SetDirty(rendererData);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Legioniz";
            PlayerSettings.productName = "WHERE THE SOUNDINGS END";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.activeInputHandler = PlayerSettings.ActiveInputHandler.InputSystemPackage;
            PlayerSettings.SetApplicationIdentifier(
                BuildTargetGroup.Standalone,
                "com.legioniz.wherethesoundingsend");
        }
    }

    /// <summary>
    /// A fresh clone has no generated Unity scenes. Bootstrap those generated scenes once
    /// after the Editor has finished importing the project so the first Play action has a scene.
    /// Existing projects are never silently rebuilt by this first-open hook.
    /// </summary>
    [InitializeOnLoad]
    internal static class ProjectFirstOpenBootstrap
    {
        private const string GreyhavenScenePath = "Assets/_Project/Scenes/Greyhaven_Blockout.unity";

        static ProjectFirstOpenBootstrap()
        {
            EditorApplication.delayCall += BootstrapFreshCheckout;
        }

        private static void BootstrapFreshCheckout()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling ||
                AssetDatabase.IsAssetImportWorkerProcess())
                return;

            // Only a missing persistent scene identifies a fresh checkout. Stale existing scenes
            // still use the explicit menu confirmation unless Unity is running unattended.
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GreyhavenScenePath) != null)
                return;

            try
            {
                ProjectFoundationSetup.InitializeForAutomation();
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    "WHERE THE SOUNDINGS END could not generate its first Greyhaven scene. " +
                    "Open Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation after resolving the error.");
                Debug.LogException(exception);
            }
        }
    }

}