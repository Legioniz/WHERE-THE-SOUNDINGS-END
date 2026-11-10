using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using WhereTheSoundingsEnd.Boat;
using WhereTheSoundingsEnd.CameraSystem;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// One-time, idempotent setup for project settings and the Greyhaven blockout scene.
    /// Run from Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation.
    /// </summary>
    public static class ProjectFoundationSetup
    {
        private const string ProjectRoot = "Assets/_Project";
        private const string ScenePath = ProjectRoot + "/Scenes/Greyhaven_Blockout.unity";
        private const string InputActionsPath = ProjectRoot + "/Settings/Input/PlayerControls.inputactions";
        private const string RenderingPath = ProjectRoot + "/Settings/Rendering";
        private const string RendererPath = RenderingPath + "/DefaultRenderer.asset";
        private const string PipelinePath = RenderingPath + "/DefaultPipeline.asset";
        private const string MaterialsPath = ProjectRoot + "/Art/Materials";

        [MenuItem("Tools/WHERE THE SOUNDINGS END/Initialize Project Foundation")]
        public static void Initialize()
        {
            EnsureProjectFolders();
            AssetDatabase.Refresh();

            ConfigureRendering();
            ConfigurePlayerSettings();

            InputActionAsset controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (controls == null)
            {
                throw new FileNotFoundException("Could not import the player input actions asset.", InputActionsPath);
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                CreateGreyhavenBlockout(controls);
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Debug.Log("WHERE THE SOUNDINGS END foundation initialized. Greyhaven is a blockout scene, not finished level art.");
        }

        private static void EnsureProjectFolders()
        {
            string[] folders =
            {
                ProjectRoot + "/Art/Materials",
                ProjectRoot + "/Art/Models",
                ProjectRoot + "/Art/Textures",
                ProjectRoot + "/Audio/Ambience",
                ProjectRoot + "/Audio/Music",
                ProjectRoot + "/Audio/SFX",
                ProjectRoot + "/Data/Creatures",
                ProjectRoot + "/Data/Items",
                ProjectRoot + "/Data/Regions",
                ProjectRoot + "/Data/Quests",
                ProjectRoot + "/Prefabs/Boat",
                ProjectRoot + "/Prefabs/Creatures",
                ProjectRoot + "/Prefabs/World",
                ProjectRoot + "/Scenes",
                ProjectRoot + "/Settings/Input",
                ProjectRoot + "/Settings/Rendering",
                ProjectRoot + "/Shaders",
                ProjectRoot + "/Tests/EditMode",
                ProjectRoot + "/Tests/PlayMode",
                "Assets/ThirdParty"
            };

            foreach (string folder in folders)
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string leaf = Path.GetFileName(folderPath);

            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
            {
                throw new IOException("Invalid Unity folder path: " + folderPath);
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void ConfigureRendering()
        {
            UniversalRendererData rendererData =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);

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

        private static void CreateGreyhavenBlockout(InputActionAsset controls)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material ocean = CreateMaterial("Blockout_Ocean.mat", new Color(0.025f, 0.105f, 0.13f));
            Material land = CreateMaterial("Blockout_Shore.mat", new Color(0.20f, 0.205f, 0.18f));
            Material dock = CreateMaterial("Blockout_Dock.mat", new Color(0.22f, 0.19f, 0.15f));
            Material hull = CreateMaterial("Blockout_Hull.mat", new Color(0.075f, 0.085f, 0.09f));
            Material marker = CreateMaterial("Blockout_Marker.mat", new Color(0.55f, 0.16f, 0.12f));

            GameObject world = new GameObject("Greyhaven_Blockout");
            GameObject oceanObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            oceanObject.name = "Ocean_Surface_Blockout";
            oceanObject.transform.SetParent(world.transform);
            oceanObject.transform.position = new Vector3(0.0f, -0.25f, 10.0f);
            oceanObject.transform.localScale = new Vector3(50.0f, 1.0f, 50.0f);
            oceanObject.GetComponent<Renderer>().sharedMaterial = ocean;

            CreateBlock(
                "Harbour_Dock",
                world.transform,
                new Vector3(0.0f, 0.1f, 19.0f),
                new Vector3(13.0f, 0.8f, 25.0f),
                dock);

            CreateBlock(
                "Greyhaven_Shoreline",
                world.transform,
                new Vector3(0.0f, -1.8f, 55.0f),
                new Vector3(150.0f, 4.0f, 65.0f),
                land);

            CreateBlock(
                "North_Pier",
                world.transform,
                new Vector3(-13.0f, 0.0f, 30.0f),
                new Vector3(4.0f, 0.7f, 24.0f),
                dock);

            GameObject lighthouseBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lighthouseBase.name = "Greyhaven_Lighthouse_Blockout";
            lighthouseBase.transform.SetParent(world.transform);
            lighthouseBase.transform.position = new Vector3(-31.0f, 4.0f, 43.0f);
            lighthouseBase.transform.localScale = new Vector3(2.0f, 4.0f, 2.0f);
            lighthouseBase.GetComponent<Renderer>().sharedMaterial = land;

            GameObject lighthouseLamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lighthouseLamp.name = "Lighthouse_Lamp_Marker";
            lighthouseLamp.transform.SetParent(lighthouseBase.transform);
            lighthouseLamp.transform.localPosition = new Vector3(0.0f, 1.0f, 0.0f);
            lighthouseLamp.transform.localScale = Vector3.one * 0.55f;
            lighthouseLamp.GetComponent<Renderer>().sharedMaterial = marker;
            Object.DestroyImmediate(lighthouseLamp.GetComponent<Collider>());

            GameObject buoy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            buoy.name = "Harbour_Buoy_Blockout";
            buoy.transform.SetParent(world.transform);
            buoy.transform.position = new Vector3(12.0f, 0.45f, 11.0f);
            buoy.transform.localScale = Vector3.one * 0.9f;
            buoy.GetComponent<Renderer>().sharedMaterial = marker;

            GameObject boat = new GameObject("Player_Boat_Blockout");
            boat.transform.position = new Vector3(0.0f, 0.75f, 0.0f);

            Rigidbody body = boat.AddComponent<Rigidbody>();
            body.mass = 1200.0f;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY |
                               RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationZ;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            GameObject hullMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullMesh.name = "Hull";
            hullMesh.transform.SetParent(boat.transform);
            hullMesh.transform.localPosition = Vector3.zero;
            hullMesh.transform.localScale = new Vector3(2.0f, 0.65f, 4.5f);
            hullMesh.GetComponent<Renderer>().sharedMaterial = hull;

            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.name = "Deck";
            deck.transform.SetParent(boat.transform);
            deck.transform.localPosition = new Vector3(0.0f, 0.38f, -0.45f);
            deck.transform.localScale = new Vector3(1.55f, 0.22f, 1.9f);
            deck.GetComponent<Renderer>().sharedMaterial = dock;
            Object.DestroyImmediate(deck.GetComponent<Collider>());

            BoatPrototypeController boatController = boat.AddComponent<BoatPrototypeController>();
            boatController.InputActions = controls;

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0.0f, 28.0f, -22.0f);
            cameraObject.transform.rotation = Quaternion.Euler(55.0f, 0.0f, 0.0f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 19.0f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500.0f;
            camera.backgroundColor = new Color(0.025f, 0.045f, 0.055f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();

            CameraFollow2_5D follow = cameraObject.AddComponent<CameraFollow2_5D>();
            follow.Target = boat.transform;

            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(42.0f, -28.0f, 0.0f);
            Light directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.15f;
            directionalLight.color = new Color(0.78f, 0.84f, 0.88f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.25f, 0.28f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.055f, 0.085f, 0.095f);
            RenderSettings.fogDensity = 0.0025f;

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static GameObject CreateBlock(
            string objectName,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = objectName;
            block.transform.SetParent(parent);
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }

        private static Material CreateMaterial(string fileName, Color color)
        {
            string path = MaterialsPath + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                throw new System.InvalidOperationException("Could not find a supported material shader.");
            }

            material = new Material(shader)
            {
                name = Path.GetFileNameWithoutExtension(fileName),
                color = color
            };

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.12f);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
