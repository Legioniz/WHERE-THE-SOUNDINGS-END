using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Yarn.Unity;
using WhereTheSoundingsEnd.Boat;
using WhereTheSoundingsEnd.CameraSystem;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Narrative;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// Deterministic procedural greybox for Greyhaven. Geometry is for scale and navigation tests,
    /// not final level art. Run through ProjectFoundationSetup so input and build settings are valid.
    /// </summary>
    public static class GreyhavenEnvironmentBuilder
    {
        private const string MaterialsPath = "Assets/_Project/Art/Materials";
        private const string CargoDefinitionsPath = "Assets/_Project/Data/Inventory";
        private const string DialogueProjectPath = "Assets/_Project/Dialogue/Greyhaven.yarnproject";
        private const string PanelSettingsPath = "Assets/_Project/Settings/UI/GreyhavenPanelSettings.asset";

        public static void Build(InputActionAsset controls, string scenePath)
        {
            if (controls == null) throw new ArgumentNullException(nameof(controls));
            EnsureFolder(MaterialsPath);
            List<CargoItemDefinition> cargoCatalog = BuildCargoCatalog();
            AssetDatabase.SaveAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material ocean = Mat("Environment_Ocean.mat", new Color(0.025f, 0.105f, 0.13f), 0.78f);
            Material sand = Mat("Environment_ShoreSand.mat", new Color(0.34f, 0.29f, 0.21f), 0.08f);
            Material soil = Mat("Environment_Soil.mat", new Color(0.19f, 0.18f, 0.15f), 0.0f);
            Material grass = Mat("Environment_Grass.mat", new Color(0.17f, 0.22f, 0.17f), 0.02f);
            Material rock = Mat("Environment_Basalt.mat", new Color(0.105f, 0.12f, 0.13f), 0.06f);
            Material wood = Mat("Environment_WeatheredWood.mat", new Color(0.25f, 0.19f, 0.13f), 0.04f);
            Material roof = Mat("Environment_Roof.mat", new Color(0.13f, 0.15f, 0.16f), 0.12f);
            Material concrete = Mat("Environment_Concrete.mat", new Color(0.31f, 0.32f, 0.30f), 0.08f);
            Material glass = Mat("Environment_WindowGlass.mat", new Color(0.12f, 0.24f, 0.27f), 0.72f);
            Material metal = Mat("Environment_Metal.mat", new Color(0.16f, 0.19f, 0.20f), 0.55f);
            Material red = Mat("Environment_MarkerRed.mat", new Color(0.62f, 0.12f, 0.09f), 0.18f);
            Material green = Mat("Environment_MarkerGreen.mat", new Color(0.12f, 0.38f, 0.25f), 0.16f);
            Material hull = Mat("Environment_BoatHull.mat", new Color(0.065f, 0.085f, 0.09f), 0.22f);
            Material foliage = Mat("Environment_MangroveFoliage.mat", new Color(0.12f, 0.20f, 0.15f), 0.0f);
            Material beacon = Mat("Environment_LighthouseBeacon.mat", new Color(0.95f, 0.62f, 0.29f), 0.18f);

            GameObject world = new GameObject("GREYHAVEN — Sheltered Bay Blockout");
            Transform terrain = Group("01_Terrain_and_Shore", world.transform);
            Transform water = Group("02_Ocean_and_Harbour_Water", world.transform);
            Transform harbor = Group("03_Piers_and_Breakwater", world.transform);
            Transform village = Group("04_Village_and_Services", world.transform);
            Transform estuary = Group("05_Estuary_and_Vegetation", world.transform);
            Transform landmarks = Group("06_Navigation_Landmarks", world.transform);
            Transform player = Group("07_Player_Prototype", world.transform);
            Transform lighting = Group("08_Lighting_and_Atmosphere", world.transform);

            BuildWater(water, ocean);
            BuildSalvageSite(water, wood, metal);
            BuildTerrain(terrain, soil, sand, grass, rock);
            BuildBreakwater(harbor, rock, concrete);
            BuildPiers(harbor, wood, metal);
            BuildVillage(village, wood, roof, concrete, glass, metal);
            BuildEstuary(estuary, soil, wood, foliage, rock);
            BuildLighthouse(landmarks, concrete, roof, red, beacon);
            BuildBuoys(landmarks, red, green, metal);
            BuildBoat(player, controls, hull, wood, metal, cargoCatalog);
            BuildCameraAndLight(lighting, player);

            YarnDialogueSaveParticipant dialogueSave = BuildSaveCoordinator(world.transform);
            SpatialCargoHold cargoHold = player.Find("Player_Boat_Blockout").GetComponent<SpatialCargoHold>();
            DialogueRunner dialogueRunner = BuildDialogueSystem(world.transform, cargoHold, controls);
            dialogueSave.Runner = dialogueRunner;
            BuildWorldSystems(world.transform, player.Find("Player_Boat_Blockout"), controls, dialogueRunner);
            BuildHarbourmaster(village, wood, concrete, dialogueRunner,
                dialogueRunner.GetComponent<GreyhavenRuntimeUI>(), controls);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.30f, 0.35f, 0.37f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.23f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.075f, 0.085f, 0.08f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.15f, 0.20f, 0.21f);
            RenderSettings.fogDensity = 0.0018f;

            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        }


        private static List<CargoItemDefinition> BuildCargoCatalog()
        {
            EnsureFolder("Assets/_Project/Data");
            EnsureFolder(CargoDefinitionsPath);

            List<CargoItemDefinition> definitions = new List<CargoItemDefinition>
            {
                CreateCargoDefinition("FSH-001", "Milkfish", CargoItemCategory.Fish,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2 }, 0.006f),
                CreateCargoDefinition("FSH-002", "Flathead Grey Mullet", CargoItemCategory.Fish,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2 }, 0.0065f),
                CreateCargoDefinition("FSH-003", "Asian Seabass", CargoItemCategory.Fish,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2,
                            Vector2Int.up, Vector2Int.up + Vector2Int.right }, 0.007f),
                CreateCargoDefinition("FSH-005", "Silver Sillago", CargoItemCategory.Fish,
                    new[] { Vector2Int.zero, Vector2Int.right }, 0.005f),
                CreateCargoDefinition("FSH-007", "Splendid Ponyfish", CargoItemCategory.Fish,
                    new[] { Vector2Int.zero }, 0.004f),
                CreateCargoDefinition("SALVAGE-WOOD-01", "Waterlogged Timber", CargoItemCategory.Salvage,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.up + Vector2Int.right }, 0f),
                CreateCargoDefinition("SALVAGE-CLOTH-01", "Salt-Stiff Canvas", CargoItemCategory.Salvage,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up }, 0f),
                CreateCargoDefinition("SALVAGE-SCRAP-01", "Corroded Ship Fittings", CargoItemCategory.Salvage,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.up + Vector2Int.right, Vector2Int.right * 2 }, 0f),
                CreateCargoDefinition("QUEST-SEALED-PARCEL", "Sealed Survey Parcel", CargoItemCategory.Quest,
                    new[] { Vector2Int.zero, Vector2Int.up }, 0f, true),
                CreateCargoDefinition("ABR-FSH-001", "Milkfish — Glass-Eyed", CargoItemCategory.Fish,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2, Vector2Int.up }, 0.009f, false, true, 3.2f),
                CreateCargoDefinition("ABR-FSH-003", "Asian Seabass — Split-Jawed", CargoItemCategory.Fish,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2,
                            Vector2Int.up, Vector2Int.up + Vector2Int.right, Vector2Int.up + Vector2Int.right * 2 }, 0.011f, false, true, 3.8f),
                CreateCargoDefinition("ABR-FSH-005", "Silver Sillago — Lantern-Eyed", CargoItemCategory.Fish,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up }, 0.008f, false, true, 2.8f),
                CreateCargoDefinition("GEAR-ENGINE-01", "Outboard Engine", CargoItemCategory.Equipment,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2,
                            Vector2Int.up, Vector2Int.up + Vector2Int.right, Vector2Int.up + Vector2Int.right * 2 }, 0f),
                CreateCargoDefinition("GEAR-ROD-01", "Handline Rod", CargoItemCategory.Equipment,
                    new[] { Vector2Int.zero, Vector2Int.up, Vector2Int.up * 2, Vector2Int.up * 3 }, 0f),
                CreateCargoDefinition("GEAR-LAMP-01", "Deck Lantern", CargoItemCategory.Equipment,
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.up + Vector2Int.right }, 0f)
            };

            return definitions;
        }

        private static CargoItemDefinition CreateCargoDefinition(
            string itemId,
            string displayName,
            CargoItemCategory category,
            Vector2Int[] cells,
            float freshnessLossPerGameMinute,
            bool questCritical = false,
            bool aberrant = false,
            float sellValueMultiplier = 1f)
        {
            string path = CargoDefinitionsPath + "/" + itemId + ".asset";
            CargoItemDefinition definition = AssetDatabase.LoadAssetAtPath<CargoItemDefinition>(path);
            bool isNewDefinition = definition == null;
            if (isNewDefinition)
            {
                definition = ScriptableObject.CreateInstance<CargoItemDefinition>();
                definition.name = itemId;
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("category").enumValueIndex = (int)category;
            serialized.FindProperty("freshnessLossPerGameMinute").floatValue = freshnessLossPerGameMinute;
            SerializedProperty questCriticalProperty = serialized.FindProperty("isQuestCritical");
            if (questCriticalProperty != null) questCriticalProperty.boolValue = questCritical;
            SerializedProperty aberrantProperty = serialized.FindProperty("isAberrant");
            if (aberrantProperty != null) aberrantProperty.boolValue = aberrant;
            SerializedProperty multiplierProperty = serialized.FindProperty("sellValueMultiplier");
            if (multiplierProperty != null) multiplierProperty.floatValue = sellValueMultiplier;
            SerializedProperty shape = serialized.FindProperty("occupiedCells");
            shape.arraySize = cells.Length;
            for (int i = 0; i < cells.Length; i++)
            {
                SerializedProperty cell = shape.GetArrayElementAtIndex(i);
                cell.vector2IntValue = cells[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (isNewDefinition)
                AssetDatabase.CreateAsset(definition, path);
            else
                EditorUtility.SetDirty(definition);
            return definition;
        }

        private static YarnDialogueSaveParticipant BuildSaveCoordinator(Transform parent)
        {
            GameObject coordinator = new GameObject("Save_Game_Coordinator");
            coordinator.transform.SetParent(parent, false);
            coordinator.AddComponent<SaveGameCoordinator>();
            coordinator.AddComponent<AutosaveController>();
            return coordinator.AddComponent<YarnDialogueSaveParticipant>();
        }

        private static DialogueRunner BuildDialogueSystem(
            Transform parent,
            SpatialCargoHold cargoHold,
            InputActionAsset controls)
        {
            if (cargoHold == null)
                throw new InvalidOperationException("Greyhaven cargo hold was not created.");
            if (AssetDatabase.LoadAssetAtPath<YarnProject>(DialogueProjectPath) == null)
            {
                AssetDatabase.ImportAsset(DialogueProjectPath, ImportAssetOptions.ForceSynchronousImport);
            }

            YarnProject project = AssetDatabase.LoadAssetAtPath<YarnProject>(DialogueProjectPath);
            if (project == null)
                throw new InvalidOperationException(
                    "Yarn Spinner could not import " + DialogueProjectPath +
                    ". Check the Yarn Spinner package import and compile errors.");

            EnsureFolder("Assets/_Project/Settings/UI");
            PanelSettings panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                panel.name = "GreyhavenPanelSettings";
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }

            GameObject dialogueRoot = new GameObject("Dialogue_System_and_Player_Interface");
            dialogueRoot.transform.SetParent(parent, false);

            UIDocument document = dialogueRoot.AddComponent<UIDocument>();
            document.panelSettings = panel;

            DialogueRunner runner = dialogueRoot.AddComponent<DialogueRunner>();
            runner.SetProject(project);

            GreyhavenRuntimeUI runtimeUI = dialogueRoot.AddComponent<GreyhavenRuntimeUI>();
            runtimeUI.Document = document;
            runtimeUI.Runner = runner;
            runtimeUI.CargoHold = cargoHold;
            runtimeUI.Controls = controls;
            runner.DialoguePresenters = new List<DialoguePresenterBase> { runtimeUI };
            runner.AddCommandHandler("checkpoint", new Action<string>(runtimeUI.RequestCheckpointAutosave));

            return runner;
        }

        private static void BuildWorldSystems(
            Transform parent,
            Transform boat,
            InputActionAsset controls,
            DialogueRunner dialogueRunner)
        {
            if (boat == null) throw new InvalidOperationException("Greyhaven boat root was not created.");
            Rigidbody body = boat.GetComponent<Rigidbody>();
            SpatialCargoHold hold = boat.GetComponent<SpatialCargoHold>();
            HarvestingMinigameController harvesting = boat.gameObject.AddComponent<HarvestingMinigameController>();
            SeaStateDirector seaState = parent.gameObject.AddComponent<SeaStateDirector>();
            harvesting.Configure(controls, hold, seaState);
            seaState.Configure(body, dialogueRunner, harvesting, hold);

            SoundingRigController soundingRig = boat.gameObject.AddComponent<SoundingRigController>();
            soundingRig.Configure(controls, seaState, body);

            boat.gameObject.AddComponent<VesselConditionController>();

            AcousticWorldListener acousticListener = parent.gameObject.AddComponent<AcousticWorldListener>();
            acousticListener.Configure(seaState);

            DeepOceanSiteInteractionController siteInteractions =
                boat.gameObject.AddComponent<DeepOceanSiteInteractionController>();
            siteInteractions.Configure(controls, seaState);
        }

        private static void BuildHarbourmaster(
            Transform parent,
            Material clothing,
            Material trim,
            DialogueRunner runner,
            GreyhavenRuntimeUI runtimeUI,
            InputActionAsset controls)
        {
            GameObject npc = new GameObject("Harbourmaster_NPC");
            npc.transform.SetParent(parent, false);
            npc.transform.position = new Vector3(
                -5f,
                HeightAt(-5f, -10f),
                -10f);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Harbourmaster_Body_Blockout";
            body.transform.SetParent(npc.transform, false);
            body.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            body.transform.localScale = new Vector3(0.72f, 1.0f, 0.72f);
            body.GetComponent<Renderer>().sharedMaterial = clothing;
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Harbourmaster_Head_Blockout";
            head.transform.SetParent(npc.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            head.transform.localScale = Vector3.one * 0.52f;
            head.GetComponent<Renderer>().sharedMaterial = trim;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());

            SphereCollider interactionRadius = npc.AddComponent<SphereCollider>();
            interactionRadius.isTrigger = true;
            interactionRadius.radius = 18f;
            interactionRadius.center = new Vector3(0f, 1f, 0f);

            HarbourmasterInteraction interaction = npc.AddComponent<HarbourmasterInteraction>();
            interaction.Runner = runner;
            interaction.Interface = runtimeUI;
            interaction.Controls = controls;
        }

        private static void BuildWater(Transform parent, Material material)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Sheltered_Bay_Surface";
            plane.transform.SetParent(parent, false);
            plane.transform.position = new Vector3(0f, -0.45f, 105f);
            plane.transform.localScale = new Vector3(29f, 1f, 27f);
            plane.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(plane.GetComponent<Collider>());

            // Greyhaven is a shallow coastal survey area. It intentionally has no deep scattering layer.
            DeepOceanSurveyRegion surveyRegion = plane.AddComponent<DeepOceanSurveyRegion>();
            surveyRegion.ConfigureForRegion("greyhaven");
        }

        private static void BuildTerrain(Transform parent, Material soil, Material sand, Material grass, Material rock)
        {
            const int nx = 80;
            const int nz = 48;
            const float minX = -110f;
            const float maxX = 110f;
            const float backZ = -112f;

            Vector3[] vertices = new Vector3[(nx + 1) * (nz + 1)];
            int[] triangles = new int[nx * nz * 6];

            for (int ix = 0; ix <= nx; ix++)
            {
                float x = Mathf.Lerp(minX, maxX, ix / (float)nx);
                float shore = ShorelineZ(x);
                for (int iz = 0; iz <= nz; iz++)
                {
                    float z = Mathf.Lerp(backZ, shore, iz / (float)nz);
                    vertices[ix * (nz + 1) + iz] = new Vector3(x, HeightAt(x, z), z);
                }
            }

            int ti = 0;
            for (int ix = 0; ix < nx; ix++)
            {
                for (int iz = 0; iz < nz; iz++)
                {
                    int a = ix * (nz + 1) + iz;
                    int b = (ix + 1) * (nz + 1) + iz;
                    int c = a + 1;
                    int d = b + 1;
                    triangles[ti++] = a; triangles[ti++] = c; triangles[ti++] = b;
                    triangles[ti++] = b; triangles[ti++] = c; triangles[ti++] = d;
                }
            }

            Mesh mesh = new Mesh { name = "Greyhaven_Procedural_Terrain", indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject land = new GameObject("Irregular_Volcanic_Island_Terrain");
            land.transform.SetParent(parent, false);
            land.AddComponent<MeshFilter>().sharedMesh = mesh;
            land.AddComponent<MeshRenderer>().sharedMaterial = soil;
            land.AddComponent<MeshCollider>().sharedMesh = mesh;

            for (int i = 0; i < 36; i++)
            {
                float x = Mathf.Lerp(-105f, 105f, i / 35f);
                float z = ShorelineZ(x) - 2.0f;
                Block("Sandy_Shore_Segment_" + i.ToString("00"), parent,
                    new Vector3(x, HeightAt(x, z) + 0.08f, z),
                    new Vector3(6.4f, 0.16f, 3.0f), sand);
            }

            System.Random rng = new System.Random(5107);
            for (int i = 0; i < 24; i++)
            {
                float x = -104f + i * 9f;
                float z = -88f + (float)rng.NextDouble() * 17f;
                GameObject mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mound.name = "Inland_Basalt_Outcrop_" + i.ToString("00");
                mound.transform.SetParent(parent, false);
                mound.transform.position = new Vector3(x, HeightAt(x, z) + 1.1f, z);
                mound.transform.localScale = new Vector3(
                    7f + (float)rng.NextDouble() * 8f,
                    2.5f + (float)rng.NextDouble() * 4.5f,
                    7f + (float)rng.NextDouble() * 9f);
                mound.GetComponent<Renderer>().sharedMaterial = i % 5 == 0 ? grass : rock;
            }
        }

        private static float ShorelineZ(float x)
        {
            return -18f + 13f * Mathf.Exp(-(x * x) / 1800f) + 1.8f * Mathf.Sin(x * 0.045f);
        }

        private static float HeightAt(float x, float z)
        {
            float west = 13f * Mathf.Exp(-((x + 68f) * (x + 68f)) / 900f - ((z + 63f) * (z + 63f)) / 2100f);
            float east = 10f * Mathf.Exp(-((x - 69f) * (x - 69f)) / 1100f - ((z + 68f) * (z + 68f)) / 2300f);
            float rear = 5f * Mathf.Exp(-(x * x) / 2600f - ((z + 105f) * (z + 105f)) / 600f);
            float noise = (Mathf.PerlinNoise((x + 180f) * 0.025f, (z + 180f) * 0.025f) - 0.5f) * 1.1f;
            return Mathf.Max(0.25f, 0.45f + west + east + rear + noise);
        }

        private static void BuildBreakwater(Transform parent, Material rock, Material concrete)
        {
            System.Random rng = new System.Random(8042);
            BuildArm(parent, -94f, -27f, rock, rng, "West");
            BuildArm(parent, 27f, 94f, rock, rng, "East");
            Block("West_Breakwater_Walkway", parent, new Vector3(-73f, 1f, -13f), new Vector3(35f, 1.4f, 4.5f), concrete);
            Block("East_Breakwater_Walkway", parent, new Vector3(73f, 1f, -13f), new Vector3(35f, 1.4f, 4.5f), concrete);
        }

        private static void BuildArm(Transform parent, float startX, float endX, Material rock, System.Random rng, string side)
        {
            for (int i = 0; i < 17; i++)
            {
                float x = Mathf.Lerp(startX, endX, i / 16f);
                float z = ShorelineZ(x) + 2.7f;
                GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                boulder.name = side + "_Breakwater_Basalt_" + i.ToString("00");
                boulder.transform.SetParent(parent, false);
                boulder.transform.position = new Vector3(x, 0.2f + (float)rng.NextDouble() * 0.35f, z);
                boulder.transform.localScale = new Vector3(
                    5.2f + (float)rng.NextDouble() * 4f,
                    2.2f + (float)rng.NextDouble() * 2.3f,
                    5f + (float)rng.NextDouble() * 4.5f);
                boulder.GetComponent<Renderer>().sharedMaterial = rock;
            }
        }

        private static void BuildPiers(Transform parent, Material wood, Material metal)
        {
            Block("Main_Pier_Deck", parent, new Vector3(0f, 0.45f, 13f), new Vector3(8.5f, 0.75f, 42f), wood);
            Block("Outer_T_Pier_Deck", parent, new Vector3(0f, 0.48f, 34f), new Vector3(27f, 0.8f, 8f), wood);
            Block("West_Finger_Pier", parent, new Vector3(-15f, 0.42f, 21f), new Vector3(4.2f, 0.7f, 23f), wood);
            Block("East_Finger_Pier", parent, new Vector3(15f, 0.42f, 21f), new Vector3(4.2f, 0.7f, 23f), wood);

            for (int z = -6; z <= 38; z += 8)
                for (int x = -4; x <= 4; x += 8)
                    Cylinder("Pier_Pile_" + x + "_" + z, parent, new Vector3(x, -0.45f, z),
                        new Vector3(0.48f, 1.4f, 0.48f), metal);

            for (int x = -12; x <= 12; x += 8)
                Cylinder("Outer_Pier_Pile_" + x, parent, new Vector3(x, -0.35f, 34f),
                    new Vector3(0.55f, 1.45f, 0.55f), metal);

            Transform rack = Group("Fisher_Net_Rack", parent);
            rack.position = new Vector3(-31f, HeightAt(-31f, -30f), -30f);
            LocalCube(rack, "Rack_Post_Left", new Vector3(-2.2f, 1.5f, 0f), new Vector3(0.35f, 3f, 0.35f), wood);
            LocalCube(rack, "Rack_Post_Right", new Vector3(2.2f, 1.5f, 0f), new Vector3(0.35f, 3f, 0.35f), wood);
            LocalCube(rack, "Rack_Beam", new Vector3(0f, 2.85f, 0f), new Vector3(4.8f, 0.35f, 0.35f), wood);
            for (int i = 0; i < 5; i++)
                LocalCube(rack, "Net_Hanging_Rib_" + i, new Vector3(-1.6f + i * 0.8f, 1.15f, 0.25f),
                    new Vector3(0.06f, 2.2f, 0.08f), metal);

            Crates(parent, new Vector3(-25f, HeightAt(-25f, -27f) + 0.2f, -27f), wood);
        }

        private static void BuildVillage(Transform parent, Material wood, Material roof, Material concrete, Material glass, Material metal)
        {
            Building(parent, "Greyhaven_Fish_Market", new Vector3(-27f, 0f, -35f), 0f, 10f, 5.2f, 8f, wood, roof, glass, concrete);
            Building(parent, "Harbour_Office", new Vector3(21f, 0f, -32f), 0f, 8f, 4.8f, 7f, concrete, roof, glass, metal);
            Building(parent, "Boat_Repair_Shed", new Vector3(1f, 0f, -46f), 0f, 11f, 5f, 10f, wood, roof, glass, metal);
            Building(parent, "Net_Mender_Shed", new Vector3(-48f, 0f, -52f), 12f, 7f, 4.2f, 6f, wood, roof, glass, concrete);
            Building(parent, "Harbour_Inn", new Vector3(39f, 0f, -55f), -8f, 10f, 5.4f, 8f, wood, roof, glass, concrete);
            Building(parent, "Village_House_West", new Vector3(-55f, 0f, -73f), 5f, 7f, 4.2f, 6f, wood, roof, glass, concrete);
            Building(parent, "Village_House_Centre", new Vector3(-19f, 0f, -78f), -4f, 7f, 4.2f, 6f, wood, roof, glass, concrete);
            Building(parent, "Village_House_East", new Vector3(22f, 0f, -78f), 7f, 7f, 4.2f, 6f, wood, roof, glass, concrete);
            Building(parent, "Village_House_Ridge", new Vector3(55f, 0f, -72f), -10f, 7f, 4.2f, 6f, wood, roof, glass, concrete);

            Road(parent, new Vector3(-28f, 0f, -35f), new Vector3(0f, 0f, -43f), 3.6f, concrete);
            Road(parent, new Vector3(0f, 0f, -43f), new Vector3(22f, 0f, -32f), 3.6f, concrete);
            Road(parent, new Vector3(0f, 0f, -43f), new Vector3(39f, 0f, -55f), 3.1f, concrete);
            Road(parent, new Vector3(0f, 0f, -43f), new Vector3(-48f, 0f, -52f), 3.1f, concrete);
            Road(parent, new Vector3(0f, 0f, -43f), new Vector3(0f, 0f, -76f), 3f, concrete);

            Crates(parent, new Vector3(-33f, HeightAt(-33f, -34f) + 0.25f, -34f), wood);
            Crates(parent, new Vector3(24f, HeightAt(24f, -34f) + 0.2f, -34f), metal);
            StreetLamp(parent, new Vector3(-7f, HeightAt(-7f, -48f), -48f), metal);
            StreetLamp(parent, new Vector3(29f, HeightAt(29f, -46f), -46f), metal);
            StreetLamp(parent, new Vector3(-42f, HeightAt(-42f, -62f), -62f), metal);
        }

        private static void Building(Transform parent, string name, Vector3 position, float yaw,
            float width, float height, float depth, Material wall, Material roof, Material glass, Material trim)
        {
            Transform b = Group(name, parent);
            b.position = new Vector3(position.x, HeightAt(position.x, position.z), position.z);
            b.rotation = Quaternion.Euler(0f, yaw, 0f);
            LocalCube(b, "Walls", new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth), wall);
            LocalCube(b, "Roof_Left_Slope", new Vector3(-width * 0.245f, height + 0.95f, 0f),
                new Vector3(width * 0.58f, 0.45f, depth + 1.1f), roof, new Vector3(0f, 0f, 30f));
            LocalCube(b, "Roof_Right_Slope", new Vector3(width * 0.245f, height + 0.95f, 0f),
                new Vector3(width * 0.58f, 0.45f, depth + 1.1f), roof, new Vector3(0f, 0f, -30f));
            LocalCube(b, "Front_Door", new Vector3(0f, 1.2f, depth * 0.5f + 0.08f), new Vector3(1.15f, 2.4f, 0.16f), trim);
            LocalCube(b, "Window_Left", new Vector3(-width * 0.29f, height * 0.62f, depth * 0.5f + 0.09f),
                new Vector3(1.25f, 1.05f, 0.13f), glass);
            LocalCube(b, "Window_Right", new Vector3(width * 0.29f, height * 0.62f, depth * 0.5f + 0.09f),
                new Vector3(1.25f, 1.05f, 0.13f), glass);
            LocalCube(b, "Sign_Board", new Vector3(0f, height + 0.25f, depth * 0.5f + 0.18f),
                new Vector3(width * 0.72f, 0.65f, 0.18f), trim);
        }

        private static void Road(Transform parent, Vector3 start, Vector3 end, float width, Material material)
        {
            Vector3 delta = end - start;
            Vector3 mid = (start + end) * 0.5f;
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Village_Road_Segment";
            road.transform.SetParent(parent, false);
            road.transform.position = new Vector3(mid.x, HeightAt(mid.x, mid.z) + 0.08f, mid.z);
            road.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            road.transform.localScale = new Vector3(width, 0.16f, delta.magnitude);
            road.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void StreetLamp(Transform parent, Vector3 p, Material metal)
        {
            Cylinder("Street_Lamp_Post", parent, p + Vector3.up * 2.3f, new Vector3(0.14f, 2.3f, 0.14f), metal);
            GameObject lightObject = new GameObject("Street_Lamp_Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = p + Vector3.up * 4.5f;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.76f, 0.68f, 0.48f);
            light.intensity = 1.1f;
            light.range = 11f;
            light.shadows = LightShadows.None;
        }

        private static void BuildEstuary(Transform parent, Material soil, Material wood, Material foliage, Material rootMaterial)
        {
            Block("Western_Mudflat", parent, new Vector3(-72f, 0.32f, -23f), new Vector3(27f, 0.3f, 13f), soil);
            Block("Eastern_Estuary_Sediment", parent, new Vector3(72f, 0.32f, -25f), new Vector3(30f, 0.3f, 14f), soil);

            System.Random rng = new System.Random(2199);
            for (int i = 0; i < 16; i++)
            {
                float x = 58f + (float)rng.NextDouble() * 30f;
                float z = -38f + (float)rng.NextDouble() * 15f;
                Mangrove(parent, new Vector3(x, HeightAt(x, z), z), wood, foliage, rootMaterial, i);
            }
            for (int i = 0; i < 9; i++)
            {
                float x = -92f + (float)rng.NextDouble() * 18f;
                float z = -46f + (float)rng.NextDouble() * 24f;
                Mangrove(parent, new Vector3(x, HeightAt(x, z), z), wood, foliage, rootMaterial, 100 + i);
            }
        }

        private static void Mangrove(Transform parent, Vector3 p, Material wood, Material foliage, Material roots, int id)
        {
            float trunk = 4f + (id % 4) * 0.35f;
            Cylinder("Mangrove_Trunk_" + id, parent, p + Vector3.up * trunk * 0.5f,
                new Vector3(0.42f, trunk * 0.5f, 0.42f), wood);

            for (int r = 0; r < 4; r++)
            {
                Vector3 outward = Quaternion.Euler(0f, r * 90f + (id % 3) * 13f, 0f) * Vector3.forward;
                Vector3 start = p + Vector3.up * 2.1f;
                Vector3 end = p + outward * 2.2f + Vector3.up * 0.15f;
                CylinderBetween("Mangrove_Prop_Root_" + id + "_" + r, parent, start, end, 0.28f, roots);
            }

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Mangrove_Canopy_" + id;
            crown.transform.SetParent(parent, false);
            crown.transform.position = p + Vector3.up * (trunk + 1f);
            crown.transform.localScale = new Vector3(5.2f, 3.1f, 5f);
            crown.GetComponent<Renderer>().sharedMaterial = foliage;
            UnityEngine.Object.DestroyImmediate(crown.GetComponent<Collider>());
        }

        private static void BuildLighthouse(Transform parent, Material concrete, Material roof, Material red, Material beacon)
        {
            float x = -86f;
            float z = -22f;
            float ground = HeightAt(x, z);

            Cylinder("Greyhaven_Lighthouse_Tower", parent, new Vector3(x, ground + 12.5f, z),
                new Vector3(3.6f, 12.5f, 3.6f), concrete);
            Cylinder("Lighthouse_Lantern_Room", parent, new Vector3(x, ground + 25.4f, z),
                new Vector3(2.7f, 1f, 2.7f), roof);
            GameObject roofCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            roofCap.name = "Lighthouse_Roof_Cap";
            roofCap.transform.SetParent(parent, false);
            roofCap.transform.position = new Vector3(x, ground + 27.1f, z);
            roofCap.transform.localScale = new Vector3(3.3f, 1.4f, 3.3f);
            roofCap.GetComponent<Renderer>().sharedMaterial = red;
            UnityEngine.Object.DestroyImmediate(roofCap.GetComponent<Collider>());
            Sphere("Lighthouse_Beacon_Visual", parent, new Vector3(x, ground + 25.4f, z), Vector3.one * 0.75f, beacon, false);

            GameObject lightObject = new GameObject("Lighthouse_Bearing_Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = new Vector3(x, ground + 25.4f, z);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.67f, 0.38f);
            light.intensity = 3f;
            light.range = 58f;
            light.shadows = LightShadows.None;

            Block("Lighthouse_Foundation", parent, new Vector3(x, ground + 0.55f, z),
                new Vector3(7f, 1.1f, 7f), concrete);
        }

        private static void BuildBuoys(Transform parent, Material red, Material green, Material metal)
        {
            Buoy(parent, "West_Channel_Buoy", new Vector3(-27f, -0.15f, 28f), red, metal);
            Buoy(parent, "East_Channel_Buoy", new Vector3(27f, -0.15f, 28f), green, metal);
            Buoy(parent, "Outer_Approach_Buoy", new Vector3(0f, -0.15f, 68f), red, metal);
        }

        private static void Buoy(Transform parent, string name, Vector3 p, Material colour, Material metal)
        {
            Cylinder(name, parent, p + Vector3.up * 0.85f, new Vector3(1.05f, 0.85f, 1.05f), colour);
            Sphere(name + "_Top_Marker", parent, p + Vector3.up * 2f, Vector3.one * 0.7f, colour, false);
            Cylinder(name + "_Antenna", parent, p + Vector3.up * 2.6f, new Vector3(0.09f, 0.85f, 0.09f), metal);
        }

        private static void BuildSalvageSite(Transform parent, Material timber, Material metal)
        {
            GameObject site = new GameObject("Greyhaven_Outer_Channel_Wreck_Field");
            site.transform.SetParent(parent, false);
            site.transform.position = new Vector3(30f, -0.25f, 55f);
            SalvageSite salvage = site.AddComponent<SalvageSite>();
            DeepOceanPhenomenonSite surveyMarker = site.AddComponent<DeepOceanPhenomenonSite>();
            surveyMarker.Configure(
                "greyhaven.outer_channel_wreck",
                "greyhaven",
                DeepOceanPhenomenonType.DerelictVessel,
                "Outer Channel Wreck Field",
                18f,
                58f,
                0.72f,
                "A low, broken return rests close to the channel floor. Its shape matches the wreck field marked on the harbour chart; no movement is detected.",
                "Fictional world site");

            GameObject brokenHull = Block("Wrecked_Hull_Remnant", site.transform,
                site.transform.position + new Vector3(0f, -0.55f, 0f), new Vector3(5.5f, 0.7f, 2.4f), timber);
            brokenHull.transform.localRotation = Quaternion.Euler(0f, 18f, 7f);
            Block("Wrecked_Deck_Plate", site.transform,
                site.transform.position + new Vector3(1.2f, -0.05f, 0.8f), new Vector3(2.6f, 0.18f, 1.1f), metal);
            Block("Wrecked_Beam_A", site.transform,
                site.transform.position + new Vector3(-1.8f, -0.15f, -1.1f), new Vector3(0.35f, 0.3f, 3.2f), timber);
            Block("Wrecked_Beam_B", site.transform,
                site.transform.position + new Vector3(2.0f, -0.2f, -1.3f), new Vector3(0.3f, 0.25f, 2.5f), timber);

            SphereCollider radius = site.AddComponent<SphereCollider>();
            radius.isTrigger = true;
            radius.radius = salvage.InteractionRadius;
        }

        private static void BuildBoat(Transform parent, InputActionAsset controls, Material hull, Material deck, Material metal, List<CargoItemDefinition> cargoCatalog)
        {
            GameObject boat = new GameObject("Player_Boat_Blockout");
            boat.transform.SetParent(parent, false);
            boat.transform.position = new Vector3(10f, 0.8f, 14f);

            Rigidbody body = boat.AddComponent<Rigidbody>();
            body.mass = 1200f;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY |
                               RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationZ;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            GameObject hullMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullMesh.name = "Hull";
            hullMesh.transform.SetParent(boat.transform, false);
            hullMesh.transform.localScale = new Vector3(2.1f, 0.62f, 5.6f);
            hullMesh.GetComponent<Renderer>().sharedMaterial = hull;

            GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bow.name = "Raised_Bow";
            bow.transform.SetParent(boat.transform, false);
            bow.transform.localPosition = new Vector3(0f, 0.25f, 2f);
            bow.transform.localScale = new Vector3(1.55f, 0.5f, 1.25f);
            bow.GetComponent<Renderer>().sharedMaterial = hull;

            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Small_Wheelhouse";
            cabin.transform.SetParent(boat.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 0.72f, -0.45f);
            cabin.transform.localScale = new Vector3(1.25f, 0.95f, 1.45f);
            cabin.GetComponent<Renderer>().sharedMaterial = deck;
            UnityEngine.Object.DestroyImmediate(cabin.GetComponent<Collider>());

            GameObject engine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            engine.name = "Outboard_Engine";
            engine.transform.SetParent(boat.transform, false);
            engine.transform.localPosition = new Vector3(0f, -0.12f, -3f);
            engine.transform.localScale = new Vector3(0.65f, 0.65f, 0.55f);
            engine.GetComponent<Renderer>().sharedMaterial = metal;
            UnityEngine.Object.DestroyImmediate(engine.GetComponent<Collider>());

            BoatPrototypeController controller = boat.AddComponent<BoatPrototypeController>();
            controller.InputActions = controls;
            SpatialCargoHold cargoHold = boat.AddComponent<SpatialCargoHold>();
            cargoHold.SetItemDefinitions(cargoCatalog);
            cargoHold.TryPlace("GEAR-ENGINE-01", 0, 0, 0, 1, 1f, 1f, out _);
            cargoHold.TryPlace("GEAR-ROD-01", 3, 0, 0, 1, 1f, 1f, out _);
            cargoHold.TryPlace("GEAR-LAMP-01", 5, 0, 0, 1, 1f, 1f, out _);
        }

        private static void BuildCameraAndLight(Transform parent, Transform playerRoot)
        {
            Transform boat = playerRoot.Find("Player_Boat_Blockout");
            if (boat == null) throw new InvalidOperationException("Greyhaven player boat was not created.");

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = boat.position + new Vector3(0f, 48f, -36f);
            cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 35f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            camera.backgroundColor = new Color(0.045f, 0.075f, 0.085f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();

            CameraFollow2_5D follow = cameraObject.AddComponent<CameraFollow2_5D>();
            follow.Target = boat;
            follow.Offset = new Vector3(0f, 48f, -36f);
            follow.CameraEulerAngles = new Vector3(55f, 0f, 0f);
            follow.SmoothTime = 0.28f;

            GameObject sunObject = new GameObject("Overcast_Directional_Light");
            sunObject.transform.SetParent(parent, false);
            sunObject.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.92f;
            sun.color = new Color(0.76f, 0.82f, 0.84f);
            sun.shadows = LightShadows.Soft;
        }

        private static void Crates(Transform parent, Vector3 p, Material material)
        {
            for (int row = 0; row < 2; row++)
                for (int col = 0; col < 3; col++)
                    Block("Fish_Crate_" + row + "_" + col, parent,
                        p + new Vector3(col * 0.9f, row * 0.72f, 0f), new Vector3(0.8f, 0.68f, 0.75f), material);
        }

        private static GameObject Block(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static GameObject Cylinder(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static GameObject Sphere(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static void CylinderBetween(string name, Transform parent, Vector3 start, Vector3 end, float diameter, Material material)
        {
            Vector3 delta = end - start;
            GameObject go = Cylinder(name, parent, (start + end) * 0.5f,
                new Vector3(diameter, delta.magnitude * 0.5f, diameter), material);
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }

        private static GameObject LocalCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Vector3? rotation = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            if (rotation.HasValue) go.transform.localRotation = Quaternion.Euler(rotation.Value);
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static Transform Group(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Material Mat(string fileName, Color colour, float smoothness)
        {
            string path = MaterialsPath + "/" + fileName;
            Material current = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (current != null) return current;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("No supported shader was found for Greyhaven materials.");

            Material material = new Material(shader)
            {
                name = System.IO.Path.GetFileNameWithoutExtension(fileName),
                color = colour,
                enableInstancing = true
            };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            string leaf = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
                throw new InvalidOperationException("Invalid Unity asset folder: " + path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}