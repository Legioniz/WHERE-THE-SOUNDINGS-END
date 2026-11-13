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
using WhereTheSoundingsEnd.NPC;
using WhereTheSoundingsEnd.World;
using WhereTheSoundingsEnd.Streaming;
using WhereTheSoundingsEnd.Interaction;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// Deterministic procedural greybox for Greyhaven. Geometry is for scale and navigation tests,
    /// not final level art. Run through ProjectFoundationSetup so input and build settings are valid.
    /// </summary>
    public static class GreyhavenEnvironmentBuilder
    {
        private const string MaterialsPath = "Assets/_Project/Art/Materials";
        private const string MeshesPath = "Assets/_Project/Art/Meshes";
        private const string StreamedSceneFolder = "Assets/_Project/Scenes";
        private const string StreamedAreaRevision = "3";
        private static readonly string[] StreamedAreaScenePaths =
        {
            StreamedSceneFolder + "/Greyhaven_Terrain.unity",
            StreamedSceneFolder + "/Greyhaven_Harbour.unity",
            StreamedSceneFolder + "/Greyhaven_Village.unity",
            StreamedSceneFolder + "/Greyhaven_Estuary_West.unity",
            StreamedSceneFolder + "/Greyhaven_Estuary_East.unity",
            StreamedSceneFolder + "/Greyhaven_Lighthouse.unity"
        };

        public static bool HasGeneratedAreaScenes()
        {
            for (int i = 0; i < StreamedAreaScenePaths.Length; i++)
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StreamedAreaScenePaths[i]) == null)
                    return false;
            return true;
        }
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
            Material foliage = Mat("Environment_MangroveFoliage.mat", new Color(0.12f, 0.20f, 0.15f), 0.0f);
            Material beacon = Mat("Environment_LighthouseBeacon.mat", new Color(0.95f, 0.62f, 0.29f), 0.18f);
            Material chartPaper = Mat("Environment_ChartPaper.mat", new Color(0.72f, 0.68f, 0.56f), 0.04f);
            Material chartWater = Mat("Environment_ChartWater.mat", new Color(0.34f, 0.48f, 0.49f), 0.02f);
            Material chartLand = Mat("Environment_ChartLand.mat", new Color(0.48f, 0.49f, 0.38f), 0.02f);
            Material chartInk = Mat("Environment_ChartInk.mat", new Color(0.10f, 0.16f, 0.16f), 0.02f);
            Material chartAccent = Mat("Environment_ChartAccent.mat", new Color(0.58f, 0.16f, 0.11f), 0.04f);
            Material rust = Mat("Environment_RustedIron.mat", new Color(0.24f, 0.16f, 0.11f), 0.22f);
            Material ceramic = Mat("Environment_ChippedCeramic.mat", new Color(0.43f, 0.39f, 0.32f), 0.12f);
            Material cloth = Mat("Environment_SaltFadedCanvas.mat", new Color(0.39f, 0.38f, 0.31f), 0.02f);
            Material rope = Mat("Environment_TarredRope.mat", new Color(0.18f, 0.14f, 0.10f), 0.04f);

            GameObject world = new GameObject("GREYHAVEN — Sheltered Bay Blockout");
            Transform terrain = Group("01_Terrain_and_Shore", world.transform);
            Transform water = Group("02_Ocean_and_Harbour_Water", world.transform);
            Transform harbor = Group("03_Piers_and_Breakwater", world.transform);
            Transform village = Group("04_Village_and_Services", world.transform);
            Transform estuary = Group("05_Estuary_and_Vegetation", world.transform);
            Transform landmarks = Group("06_Navigation_Landmarks", world.transform);
            Transform player = Group("07_Player_Prototype", world.transform);
            Transform lighting = Group("08_Lighting_and_Atmosphere", world.transform);

            WhereTheSoundingsEnd.World.MaritimeSurfaceBehaviour seaSurface = BuildWater(water, ocean);
            BuildSalvageSite(water, wood, metal);
            BuildTerrain(terrain, soil, sand, grass, rock);
            BuildBreakwater(harbor, rock, concrete);
            BuildPiers(harbor, wood, metal, chartPaper, chartWater, chartLand, chartInk, chartAccent);
            BuildHarbourmasterQuayFurniture(harbor, wood, metal, chartPaper, chartInk, chartAccent);
            BuildVillage(village, wood, roof, concrete, glass, metal);
            BuildResidentFurniture(village, wood, concrete, metal, glass, chartPaper, chartInk, chartAccent);
            BuildEstuary(estuary, soil, wood, foliage, rock);
            BuildLighthouse(landmarks, concrete, roof, red, beacon);
            BuildBuoys(landmarks, red, green, metal);
            BuildEnvironmentalArtifacts(harbor, village, estuary, landmarks, water,
                wood, metal, glass, chartPaper, chartInk, rust, ceramic, cloth, rope);
            GreyhavenMaritimeAssetBuilder.Build(harbor, village, estuary, landmarks,
                wood, metal, rust, ceramic, cloth, rope, foliage, rock, glass);
            BuildBoat(player, controls, cargoCatalog, seaSurface);
            BuildCameraAndLight(lighting, player, seaSurface);

            YarnDialogueSaveParticipant dialogueSave = BuildSaveCoordinator(world.transform);
            SpatialCargoHold cargoHold = player.Find("Player_Boat_Blockout").GetComponent<SpatialCargoHold>();
            DialogueRunner dialogueRunner = BuildDialogueSystem(world.transform, cargoHold, controls);
            dialogueSave.Runner = dialogueRunner;
            BuildWorldSystems(world.transform, player.Find("Player_Boat_Blockout"), controls, dialogueRunner);
            GreyhavenRuntimeUI runtimeUI = dialogueRunner.GetComponent<GreyhavenRuntimeUI>();
            BuildHarbourmaster(village, wood, concrete, dialogueRunner, runtimeUI, controls);
            SeaStateDirector npcClock = world.GetComponent<SeaStateDirector>();
            BuildVillageResident(village, "Mara_Venn_NPC", "Mara Venn", "greyhaven.mara_venn",
                "fishmonger", "Greyhaven_Fishmonger", new Vector3(-27f, HeightAt(-27f, -28.5f), -28.5f),
                new Vector3(-23f, HeightAt(-23f, -29f), -29f), wood, concrete,
                dialogueRunner, runtimeUI, controls, npcClock, 4f, 16f, "market_shift");
            BuildVillageResident(village, "Tomas_Vale_NPC", "Tomas Vale", "greyhaven.tomas_vale",
                "mechanic", "Greyhaven_Mechanic", new Vector3(1f, HeightAt(1f, -40f), -40f),
                new Vector3(5f, HeightAt(5f, -38f), -38f), metal, concrete,
                dialogueRunner, runtimeUI, controls, npcClock, 6f, 18f, "repair_shift");
            BuildVillageResident(village, "Inez_Sarmiento_NPC", "Inez Sarmiento", "greyhaven.inez_sarmiento",
                "net_mender", "Greyhaven_NetMender", new Vector3(-48f, HeightAt(-48f, -46f), -46f),
                new Vector3(-44f, HeightAt(-44f, -46f), -46f), wood, concrete,
                dialogueRunner, runtimeUI, controls, npcClock, 7f, 17f, "net_mending");

            BuildDocksideHub(world.transform, player.Find("Player_Boat_Blockout"), controls,
                dialogueRunner, runtimeUI, npcClock);

            BuildAreaStreaming(world.transform, player.Find("Player_Boat_Blockout"),
                terrain, harbor, village, estuary, landmarks, scene);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.30f, 0.35f, 0.37f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.23f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.075f, 0.085f, 0.08f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.15f, 0.20f, 0.21f);
            RenderSettings.fogDensity = 0.0018f;

            SceneManager.SetActiveScene(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            for (int i = 0; i < StreamedAreaScenePaths.Length; i++)
                buildScenes.Add(new EditorBuildSettingsScene(StreamedAreaScenePaths[i], true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }


        private static void BuildAreaStreaming(
            Transform world, Transform boat, Transform terrain, Transform harbour,
            Transform village, Transform estuary, Transform landmarks, Scene persistentScene)
        {
            EnsureFolder(StreamedSceneFolder);
            BuildAreaSceneFromChildren(terrain, "greyhaven.terrain", StreamedAreaScenePaths[0], null, persistentScene);
            BuildAreaSceneFromChildren(harbour, "greyhaven.harbour", StreamedAreaScenePaths[1], null, persistentScene);
            BuildAreaSceneFromChildren(village, "greyhaven.village", StreamedAreaScenePaths[2],
                candidate => !candidate.name.EndsWith("_Anchor", StringComparison.Ordinal), persistentScene);
            BuildAreaSceneFromChildren(estuary, "greyhaven.estuary.west", StreamedAreaScenePaths[3],
                candidate => candidate.transform.position.x < 0f, persistentScene);
            BuildAreaSceneFromChildren(estuary, "greyhaven.estuary.east", StreamedAreaScenePaths[4],
                candidate => candidate.transform.position.x >= 0f, persistentScene);
            BuildAreaSceneFromChildren(landmarks, "greyhaven.lighthouse", StreamedAreaScenePaths[5],
                candidate => candidate.name.IndexOf("Lighthouse", StringComparison.OrdinalIgnoreCase) >= 0,
                persistentScene);

            AreaStreamingDirector director = world.gameObject.AddComponent<AreaStreamingDirector>();
            director.Configure(boat, new List<StreamedAreaDefinition>
            {
                new StreamedAreaDefinition("greyhaven.terrain", StreamedAreaScenePaths[0], StreamedAreaRevision,
                    new Vector3(0f, 0f, -58f), 132f, 185f, 4f, true),
                new StreamedAreaDefinition("greyhaven.harbour", StreamedAreaScenePaths[1], StreamedAreaRevision,
                    new Vector3(0f, 0f, 18f), 112f, 155f, 3f, true),
                new StreamedAreaDefinition("greyhaven.village", StreamedAreaScenePaths[2], StreamedAreaRevision,
                    new Vector3(0f, 0f, -54f), 96f, 138f, 4f, true),
                new StreamedAreaDefinition("greyhaven.estuary.west", StreamedAreaScenePaths[3], StreamedAreaRevision,
                    new Vector3(-80f, 0f, -36f), 62f, 88f, 4f, false),
                new StreamedAreaDefinition("greyhaven.estuary.east", StreamedAreaScenePaths[4], StreamedAreaRevision,
                    new Vector3(78f, 0f, -36f), 62f, 88f, 4f, false),
                new StreamedAreaDefinition("greyhaven.lighthouse", StreamedAreaScenePaths[5], StreamedAreaRevision,
                    new Vector3(-86f, 0f, -22f), 68f, 96f, 4f, false)
            });
            EditorUtility.SetDirty(director);
        }

        private static void BuildAreaSceneFromChildren(
            Transform source, string areaId, string scenePath,
            Func<GameObject, bool> filter, Scene persistentScene)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            List<GameObject> candidates = new List<GameObject>();
            for (int i = 0; i < source.childCount; i++)
            {
                GameObject candidate = source.GetChild(i).gameObject;
                if (candidate == null || (filter != null && !filter(candidate)) ||
                    !IsStaticStreamableHierarchy(candidate)) continue;
                candidates.Add(candidate);
            }
            if (candidates.Count == 0)
                throw new InvalidOperationException("No static geometry was found for streamed area '" + areaId + "'.");

            AssetDatabase.DeleteAsset(scenePath);
            Scene areaScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                GameObject areaObject = new GameObject("STREAMED_AREA — " + areaId);
                SceneManager.MoveGameObjectToScene(areaObject, areaScene);
                StreamedAreaSceneRoot areaRoot = areaObject.AddComponent<StreamedAreaSceneRoot>();
                areaRoot.Configure(areaId, StreamedAreaRevision);
                EditorUtility.SetDirty(areaRoot);
                areaObject.SetActive(false);

                for (int i = 0; i < candidates.Count; i++)
                {
                    GameObject candidate = candidates[i];
                    if (candidate == null) continue;
                    candidate.transform.SetParent(null, true);
                    SceneManager.MoveGameObjectToScene(candidate, areaScene);
                    candidate.transform.SetParent(areaObject.transform, true);
                }
                SceneManager.SetActiveScene(areaScene);
                EditorSceneManager.SaveScene(areaScene, scenePath);
            }
            finally
            {
                if (persistentScene.IsValid() && persistentScene.isLoaded)
                    SceneManager.SetActiveScene(persistentScene);
                if (areaScene.IsValid() && areaScene.isLoaded)
                    EditorSceneManager.CloseScene(areaScene, true);
            }
        }

        private static bool IsStaticStreamableHierarchy(GameObject candidate)
        {
            if (candidate == null) return false;
            if (candidate.GetComponentsInChildren<MonoBehaviour>(true).Length > 0) return false;
            return candidate.GetComponentsInChildren<Rigidbody>(true).Length == 0;
        }

        private static Mesh SaveGeneratedMeshAsset(string fileName, Mesh mesh)
        {
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder(MeshesPath);
            string path = MeshesPath + "/" + fileName;
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                UnityEngine.Object.DestroyImmediate(mesh);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
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
                    new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.up + Vector2Int.right }, 0f),
                CreateCargoDefinition("SUP-BAIT-WORM", "Worm Bait", CargoItemCategory.Equipment,
                    new[] { Vector2Int.zero }, 0f),
                CreateCargoDefinition("SUP-BAIT-FISH", "Small Fish Bait", CargoItemCategory.Equipment,
                    new[] { Vector2Int.zero, Vector2Int.right }, 0f),
                CreateCargoDefinition("SUP-BAIT-SHELLFISH", "Shellfish Bait", CargoItemCategory.Equipment,
                    new[] { Vector2Int.zero }, 0f),
                CreateCargoDefinition("SUP-BAIT-LURE", "Reusable Lure", CargoItemCategory.Equipment,
                    new[] { Vector2Int.zero, Vector2Int.right }, 0f)
            };

            ConfigureFishEcology(definitions);
            ConfigureEconomy(definitions);
            return definitions;
        }

        private static void ConfigureEconomy(List<CargoItemDefinition> definitions)
        {
            // Initial Greyhaven species use the authoritative schedule's USD/kg values.
            // Unlisted, aberrant, salvage, quest and equipment records remain ineligible
            // until their legal buyer contract and price unit are explicitly authored.
            Dictionary<string, float> usdPerKg = new Dictionary<string, float>(StringComparer.Ordinal)
            {
                { "FSH-001", 2.30f },
                { "FSH-002", 1.98f },
                { "FSH-003", 3.49f },
                { "FSH-005", 2.14f },
                { "FSH-007", 0.95f }
            };

            for (int i = 0; i < definitions.Count; i++)
            {
                CargoItemDefinition definition = definitions[i];
                if (definition == null) continue;

                float price;
                bool eligible = usdPerKg.TryGetValue(definition.ItemId, out price);
                SerializedObject serialized = new SerializedObject(definition);
                serialized.FindProperty("baseSalePriceUsd").floatValue = eligible ? price : 0f;
                serialized.FindProperty("saleUnitLabel").stringValue = eligible ? "kg" : "unit";
                serialized.FindProperty("ordinarySaleEligible").boolValue = eligible;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }
        }

        private static void ConfigureFishEcology(List<CargoItemDefinition> definitions)
        {
            SetFishEcology(definitions, "FSH-001",
                (int)(FishHabitat.Estuary | FishHabitat.Nearshore), FishPreferredTechnique.Drifting,
                "worm", 0f, 12f, 0.75f, 0.22f, 55, 4, 0.08f, 0.6f, 4f);
            SetFishEcology(definitions, "FSH-002",
                (int)(FishHabitat.Estuary | FishHabitat.Nearshore), FishPreferredTechnique.FloatFishing,
                "worm", 0f, 10f, 0.65f, 0.18f, 70, 5, 0.12f, 0.5f, 2.5f);
            SetFishEcology(definitions, "FSH-003",
                (int)(FishHabitat.Nearshore | FishHabitat.Kelp | FishHabitat.OpenWater), FishPreferredTechnique.Trolling,
                "small-fish", 5f, 65f, 1.45f, 0.72f, 28, 2, 0.25f, 1.2f, 15f);
            SetFishEcology(definitions, "FSH-005",
                (int)(FishHabitat.Nearshore | FishHabitat.Estuary), FishPreferredTechnique.BottomFishing,
                "worm", 1f, 18f, 0.5f, 0.12f, 80, 7, 0.05f, 0.2f, 1.2f);
            SetFishEcology(definitions, "FSH-007",
                (int)(FishHabitat.Nearshore | FishHabitat.Kelp), FishPreferredTechnique.FloatFishing,
                "shellfish", 2f, 20f, 0.35f, 0.08f, 90, 8, 0.06f, 0.1f, 0.6f);

            SetFishEcology(definitions, "ABR-FSH-001",
                (int)(FishHabitat.Estuary | FishHabitat.Nearshore), FishPreferredTechnique.Drifting,
                "worm", 0f, 18f, 1.35f, 0.62f, 4, 0, 0.02f, 0.8f, 5f);
            SetFishEcology(definitions, "ABR-FSH-003",
                (int)(FishHabitat.Nearshore | FishHabitat.Kelp | FishHabitat.OpenWater), FishPreferredTechnique.Trolling,
                "small-fish", 5f, 90f, 1.8f, 0.85f, 2, 0, 0.03f, 2f, 18f);
            SetFishEcology(definitions, "ABR-FSH-005",
                (int)(FishHabitat.Nearshore | FishHabitat.Kelp), FishPreferredTechnique.Jigging,
                "lure", 8f, 35f, 1.2f, 0.55f, 3, 0, 0.04f, 0.4f, 1.8f);
        }

        private static void SetFishEcology(
            List<CargoItemDefinition> definitions,
            string itemId,
            int habitats,
            FishPreferredTechnique technique,
            string bait,
            float minimumDepth,
            float maximumDepth,
            float resistance,
            float burstFrequency,
            int initialPopulation,
            int dailyRecovery,
            float migrationRate,
            float minimumSize,
            float maximumSize)
        {
            CargoItemDefinition definition = definitions.Find(item => item != null && item.ItemId == itemId);
            if (definition == null) return;
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("habitats").intValue = habitats;
            serialized.FindProperty("preferredTechnique").enumValueIndex = (int)technique;
            serialized.FindProperty("preferredBaitId").stringValue = bait;
            serialized.FindProperty("minimumDepthMetres").floatValue = minimumDepth;
            serialized.FindProperty("maximumDepthMetres").floatValue = maximumDepth;
            serialized.FindProperty("resistance").floatValue = resistance;
            serialized.FindProperty("burstFrequency").floatValue = burstFrequency;
            serialized.FindProperty("initialPopulation").intValue = initialPopulation;
            serialized.FindProperty("dailyPopulationRecovery").intValue = dailyRecovery;
            serialized.FindProperty("migrationRate").floatValue = migrationRate;
            serialized.FindProperty("baseCatchSize").floatValue = minimumSize;
            serialized.FindProperty("maximumCatchSize").floatValue = maximumSize;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
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
            dialogueRoot.AddComponent<GreyhavenFrontEndUI>();
            runner.DialoguePresenters = new List<DialoguePresenterBase> { runtimeUI };
            runner.AddCommandHandler("checkpoint", new Action<string>(runtimeUI.RequestCheckpointAutosave));
            runner.AddCommandHandler<string, int>("relationship", runtimeUI.ApplyRelationshipCommand);

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
            boat.gameObject.AddComponent<WhereTheSoundingsEnd.Economy.EconomyWalletController>();
            boat.gameObject.AddComponent<WhereTheSoundingsEnd.Economy.VesselFuelController>();
            boat.gameObject.AddComponent<WhereTheSoundingsEnd.Economy.MaraFishMarketController>();
            boat.gameObject.AddComponent<WhereTheSoundingsEnd.Economy.GreyhavenVendorServices>();

            AcousticWorldListener acousticListener = parent.gameObject.AddComponent<AcousticWorldListener>();
            acousticListener.Configure(seaState);

            DeepOceanSiteInteractionController siteInteractions =
                boat.gameObject.AddComponent<DeepOceanSiteInteractionController>();
            siteInteractions.Configure(controls, seaState);

            // Persistent chart discovery belongs to the vessel, not streamed scenery roots.
            boat.gameObject.AddComponent<RegionalChartController>();
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
            // Day shift: quay watch on the inner main pier, within the marked harbour service area.
            npc.transform.position = new Vector3(-1.6f, 0.84f, 12f);

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
            interaction.ExternallyManagedByDocksideHub = true;

            Transform officeAnchor = CreateNpcAnchor(parent, "Harbourmaster_Office_Anchor",
                new Vector3(21f, HeightAt(21f, -28.5f), -28.5f));
            NpcAgent agent = npc.AddComponent<NpcAgent>();
            agent.Configure(
                "greyhaven.harbourmaster",
                "Harbourmaster",
                "harbourmaster",
                "greyhaven",
                parent.root.GetComponent<SeaStateDirector>(),
                new List<NpcScheduleEntry>
                {
                    new NpcScheduleEntry { startHour = 5f, endHour = 18f, activityId = "quay_watch", destination = npc.transform },
                    new NpcScheduleEntry { startHour = 18f, endHour = 5f, activityId = "office_duty", destination = officeAnchor }
                });
            interaction.Agent = agent;
        }

        private static void BuildVillageResident(
            Transform parent,
            string objectName,
            string displayName,
            string npcId,
            string occupationId,
            string dialogueNode,
            Vector3 position,
            Vector3 secondaryPosition,
            Material clothing,
            Material trim,
            DialogueRunner runner,
            GreyhavenRuntimeUI runtimeUI,
            InputActionAsset controls,
            SeaStateDirector seaState,
            float dayStart,
            float dayEnd,
            string dayActivity)
        {
            GameObject npc = new GameObject(objectName);
            npc.transform.SetParent(parent, false);
            npc.transform.position = position;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = objectName + "_Body_Blockout";
            body.transform.SetParent(npc.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.68f, 0.95f, 0.68f);
            body.GetComponent<Renderer>().sharedMaterial = clothing;
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = objectName + "_Head_Blockout";
            head.transform.SetParent(npc.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.02f, 0f);
            head.transform.localScale = Vector3.one * 0.48f;
            head.GetComponent<Renderer>().sharedMaterial = trim;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());

            SphereCollider interactionRadius = npc.AddComponent<SphereCollider>();
            interactionRadius.isTrigger = true;
            interactionRadius.radius = 14f;
            interactionRadius.center = new Vector3(0f, 1f, 0f);

            HarbourmasterInteraction interaction = npc.AddComponent<HarbourmasterInteraction>();
            interaction.Runner = runner;
            interaction.Interface = runtimeUI;
            interaction.Controls = controls;
            interaction.DialogueNode = dialogueNode;
            interaction.DisplayName = displayName;
            interaction.ExternallyManagedByDocksideHub = true;

            Transform secondaryAnchor = CreateNpcAnchor(parent, objectName + "_Secondary_Anchor", secondaryPosition);
            NpcAgent agent = npc.AddComponent<NpcAgent>();
            agent.Configure(
                npcId,
                displayName,
                occupationId,
                "greyhaven",
                seaState,
                new List<NpcScheduleEntry>
                {
                    new NpcScheduleEntry { startHour = dayStart, endHour = dayEnd, activityId = dayActivity, destination = npc.transform },
                    new NpcScheduleEntry { startHour = dayEnd, endHour = dayStart, activityId = "off_duty", destination = secondaryAnchor }
                });
            interaction.Agent = agent;
        }

        private static void BuildDocksideHub(
            Transform world,
            Transform boat,
            InputActionAsset controls,
            DialogueRunner dialogueRunner,
            GreyhavenRuntimeUI runtimeUI,
            SeaStateDirector seaState)
        {
            if (world == null || boat == null || dialogueRunner == null || runtimeUI == null || controls == null)
                throw new InvalidOperationException("Greyhaven dockside hub is missing a required scene reference.");

            PanelSettings panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
                throw new InvalidOperationException("Greyhaven PanelSettings asset is missing.");

            GameObject hubObject = new GameObject("Dockside_Interaction_Hub");
            hubObject.transform.SetParent(world, false);
            UIDocument document = hubObject.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.sortingOrder = 50;

            NpcAgent harbourmaster = world.Find("04_Village_and_Services/Harbourmaster_NPC")?.GetComponent<NpcAgent>();
            NpcAgent mara = world.Find("04_Village_and_Services/Mara_Venn_NPC")?.GetComponent<NpcAgent>();
            NpcAgent tomas = world.Find("04_Village_and_Services/Tomas_Vale_NPC")?.GetComponent<NpcAgent>();
            NpcAgent inez = world.Find("04_Village_and_Services/Inez_Sarmiento_NPC")?.GetComponent<NpcAgent>();

            List<DocksideLocationDefinition> locations = new List<DocksideLocationDefinition>
            {
                new DocksideLocationDefinition(
                    "greyhaven.dock", "Main Dock", "Mooring and harbour approach",
                    "The familiar working end of Greyhaven: damp timber, mooring lines, tide marks and the fixed harbour chart. The vessel remains your point of access; no disembarking is required.",
                    new Vector3(0f, 0f, 12f), 26f, string.Empty, null,
                    new List<DocksideFurnitureOption>
                    {
                        new DocksideFurnitureOption("dock.tide-register", "Tide Register",
                            "Read the latest action-clock time, day, sea disturbance and recorded omen.", DocksideFurnitureAction.ReadTideLog),
                        new DocksideFurnitureOption("dock.harbour-chart", "Harbour Chart",
                            "Review the fixed settlement bearings and named service locations.", DocksideFurnitureAction.ReadHarbourChart),
                        new DocksideFurnitureOption("dock.cargo-hatch", "Cargo Hatch",
                            "Inspect the catch and salvage currently carried aboard.", DocksideFurnitureAction.InspectCatch)
                    }),
                new DocksideLocationDefinition(
                    "greyhaven.harbour-office", "Harbour Office", "Records, notices and the Harbourmaster",
                    "A compact office with a tide-log desk, chronometer, channel chart and record cabinets. Paperwork is stacked by date and bearing; the room feels maintained rather than decorated.",
                    new Vector3(21f, 0f, -28.5f), 24f, "Greyhaven_Harbourmaster", harbourmaster,
                    new List<DocksideFurnitureOption>
                    {
                        new DocksideFurnitureOption("office.tide-desk", "Tide-Log Desk",
                            "The Harbourmaster's daily register records harbour conditions and the latest unusual report.", DocksideFurnitureAction.ReadTideLog),
                        new DocksideFurnitureOption("office.channel-chart", "Channel Chart",
                            "Read the local harbour chart and the resident workplace key.", DocksideFurnitureAction.ReadHarbourChart),
                        new DocksideFurnitureOption("office.noticeboard", "Noticeboard",
                            "Read current harbour notices and practical warnings.", DocksideFurnitureAction.ReadNoticeboard)
                    }),
                new DocksideLocationDefinition(
                    "greyhaven.fish-market", "Fish Market", "Catch inspection and Mara Venn",
                    "A cool, salt-stained market hall. The weighing scale sits beside a sorting counter, tally board and sample jars. Mara keeps the catch grades neat and notices details other people overlook.",
                    new Vector3(-27f, 0f, -28.5f), 24f, "Greyhaven_Fishmonger", mara,
                    new List<DocksideFurnitureOption>
                    {
                        new DocksideFurnitureOption("market.weighing-scale", "Weighing Scale",
                            "Inspect each cargo placement, quantity, freshness and unusual specimen flags without unloading it.", DocksideFurnitureAction.InspectCatch),
                        new DocksideFurnitureOption("market.tally-board", "Catch Tally",
                            "Review the current cargo manifest and catch condition.", DocksideFurnitureAction.InspectCatch),
                        new DocksideFurnitureOption("market.sample-jars", "Sample Jars",
                            "Read Mara's notes on grading, freshness and keeping strange specimens separate.", DocksideFurnitureAction.ReadCatchNotes)
                    }),
                new DocksideLocationDefinition(
                    "greyhaven.boat-repair", "Boat Repair Shed", "Vessel diagnostics and Tomas Vale",
                    "A broad work bay with an engine bench, vice, tool wall and spare propeller. Grease-darkened notes are pinned beside the bench; every tool has a place.",
                    new Vector3(1f, 0f, -40f), 24f, "Greyhaven_Mechanic", tomas,
                    new List<DocksideFurnitureOption>
                    {
                        new DocksideFurnitureOption("repair.engine-bench", "Engine Workbench",
                            "Read the vessel's live hull and engine condition and its latest damage report.", DocksideFurnitureAction.InspectVessel),
                        new DocksideFurnitureOption("repair.tool-wall", "Tool Wall",
                            "Review the current vessel diagnosis before deciding whether to continue an expedition.", DocksideFurnitureAction.InspectVessel),
                        new DocksideFurnitureOption("repair.propeller", "Spare Propeller",
                            "Read Tomas's engine-load and propeller inspection notes.", DocksideFurnitureAction.ReadEngineNotes)
                    }),
                new DocksideLocationDefinition(
                    "greyhaven.net-mender", "Net Mender Shed", "Network and Inez Sarmiento",
                    "A narrow workroom built around a net loom, knot board, thread spools and a low mending stool. The space is orderly, with repaired mesh hanging where the light can reach it.",
                    new Vector3(-48f, 0f, -46f), 25f, "Greyhaven_NetMender", inez,
                    new List<DocksideFurnitureOption>
                    {
                        new DocksideFurnitureOption("net.loom", "Net Loom",
                            "Read Inez's practical notes on mesh inspection and line wear.", DocksideFurnitureAction.ReadNetNotes),
                        new DocksideFurnitureOption("net.knot-board", "Knot Board",
                            "Review the basic knot and line-care guide.", DocksideFurnitureAction.ReadKnotGuide),
                        new DocksideFurnitureOption("net.thread-spools", "Thread Spools",
                            "Inspect the displayed work supplies. No net-material inventory is consumed or granted.", DocksideFurnitureAction.InspectNetSupplies)
                    }),
                new DocksideLocationDefinition(
                    "greyhaven.harbour-inn", "Harbour Inn", "Guest ledger and local rumours",
                    "A warm room with a shared table, worn benches, a guest ledger and a lantern kept ready for the evening watch. It is the village's social centre, not a luxury hotel.",
                    new Vector3(39f, 0f, -55f), 25f, string.Empty, null,
                    new List<DocksideFurnitureOption>
                    {
                        new DocksideFurnitureOption("inn.guest-ledger", "Guest Ledger",
                            "Write a safe harbour checkpoint to the existing autosave system.", DocksideFurnitureAction.SaveHarbourLog),
                        new DocksideFurnitureOption("inn.shared-table", "Shared Table",
                            "Read the small rumours and remarks that circulate around the harbour.", DocksideFurnitureAction.ReadRumours),
                        new DocksideFurnitureOption("inn.lantern", "Watch Lantern",
                            "Inspect the current watch conditions and night status.", DocksideFurnitureAction.ReadNightWatch)
                    }),
                new DocksideLocationDefinition(
                    "greyhaven.lighthouse", "Western Lighthouse", "Beacon and keeper's records",
                    "The lighthouse stands on the western headland above the outer approach. Its beacon mechanism and keeper's log can be inspected from the vessel; the tower is not a walkable interior.",
                    new Vector3(-86f, 0f, -22f), 27f, string.Empty, null,
                    new List<DocksideFurnitureOption>
                    {
                        new DocksideFurnitureOption("lighthouse.beacon", "Beacon Mechanism",
                            "Inspect the lighthouse's current presentation-only beacon status.", DocksideFurnitureAction.InspectBeacon),
                        new DocksideFurnitureOption("lighthouse.logbook", "Keeper's Log",
                            "Read a short local navigation note from the lighthouse log.", DocksideFurnitureAction.ReadLighthouseLog)
                    })
            };

            DocksideHubController hub = hubObject.AddComponent<DocksideHubController>();
            hub.Configure(document, runtimeUI, boat.GetComponent<Rigidbody>(), controls,
                boat.GetComponent<SpatialCargoHold>(), boat.GetComponent<VesselConditionController>(),
                seaState, world.GetComponentInChildren<SaveGameCoordinator>(true), locations);
            EditorUtility.SetDirty(hub);
        }

        private static Transform CreateNpcAnchor(Transform parent, string anchorName, Vector3 position)
        {
            GameObject anchor = new GameObject(anchorName);
            anchor.transform.SetParent(parent, false);
            anchor.transform.position = position;
            return anchor.transform;
        }

        private static WhereTheSoundingsEnd.World.MaritimeSurfaceBehaviour BuildWater(Transform parent, Material material)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Sheltered_Bay_Surface";
            plane.transform.SetParent(parent, false);
            plane.transform.position = new Vector3(0f, -0.45f, 105f);
            plane.transform.localScale = new Vector3(29f, 1f, 27f);
            plane.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(plane.GetComponent<Collider>());
            WhereTheSoundingsEnd.World.MaritimeSurfaceBehaviour seaSurface =
                plane.AddComponent<WhereTheSoundingsEnd.World.MaritimeSurfaceBehaviour>();

            // Greyhaven is a shallow coastal survey area. It intentionally has no deep scattering layer.
            DeepOceanSurveyRegion surveyRegion = plane.AddComponent<DeepOceanSurveyRegion>();
            surveyRegion.ConfigureForRegion("greyhaven");
            return seaSurface;
        }

        private static void BuildTerrain(Transform parent, Material soil, Material sand, Material grass, Material rock)
        {
            const int nx = GreyhavenTerrainProfile.GridColumns;
            const int nz = GreyhavenTerrainProfile.GridRows;

            Vector3[] vertices = new Vector3[(nx + 1) * (nz + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            List<int>[] surfaceTriangles =
            {
                new List<int>(),
                new List<int>(),
                new List<int>(),
                new List<int>()
            };

            for (int ix = 0; ix <= nx; ix++)
            {
                float x = Mathf.Lerp(
                    GreyhavenTerrainProfile.MinimumX,
                    GreyhavenTerrainProfile.MaximumX,
                    ix / (float)nx);
                float shore = GreyhavenTerrainProfile.ShorelineZ(x);

                for (int iz = 0; iz <= nz; iz++)
                {
                    float z = Mathf.Lerp(GreyhavenTerrainProfile.BackZ, shore, iz / (float)nz);
                    int index = ix * (nz + 1) + iz;
                    vertices[index] = new Vector3(x, GreyhavenTerrainProfile.HeightAt(x, z), z);
                    uvs[index] = new Vector2(x / 9f, z / 9f);
                }
            }

            for (int ix = 0; ix < nx; ix++)
            {
                for (int iz = 0; iz < nz; iz++)
                {
                    int a = ix * (nz + 1) + iz;
                    int b = (ix + 1) * (nz + 1) + iz;
                    int c = a + 1;
                    int d = b + 1;

                    AddTerrainTriangle(surfaceTriangles, vertices, a, c, b);
                    AddTerrainTriangle(surfaceTriangles, vertices, b, c, d);
                }
            }

            Mesh mesh = new Mesh
            {
                name = "Greyhaven_Procedural_Terrain",
                indexFormat = IndexFormat.UInt32
            };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.subMeshCount = surfaceTriangles.Length;
            for (int surface = 0; surface < surfaceTriangles.Length; surface++)
                mesh.SetTriangles(surfaceTriangles[surface], surface, true);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh = SaveGeneratedMeshAsset("Greyhaven_Procedural_Terrain.asset", mesh);

            GameObject land = new GameObject("Irregular_Volcanic_Island_Terrain");
            land.transform.SetParent(parent, false);
            land.AddComponent<MeshFilter>().sharedMesh = mesh;
            land.AddComponent<MeshRenderer>().sharedMaterials = new[] { soil, sand, grass, rock };
            land.AddComponent<MeshCollider>().sharedMesh = mesh;

            System.Random rng = new System.Random(5107);
            for (int i = 0; i < 24; i++)
            {
                float x = -104f + i * 9f;
                float z = -88f + (float)rng.NextDouble() * 17f;
                Mesh rockMesh = CreateFacetedRockMesh("Greyhaven_Basalt_Outcrop_" + i.ToString("00"), rng);
                rockMesh = SaveGeneratedMeshAsset("Greyhaven_Basalt_Outcrop_" + i.ToString("00") + ".asset", rockMesh);

                GameObject mound = new GameObject("Inland_Basalt_Outcrop_" + i.ToString("00"));
                mound.transform.SetParent(parent, false);
                mound.transform.position = new Vector3(x, HeightAt(x, z) + 1.1f, z);
                mound.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                mound.transform.localScale = new Vector3(
                    7f + (float)rng.NextDouble() * 8f,
                    2.5f + (float)rng.NextDouble() * 4.5f,
                    7f + (float)rng.NextDouble() * 9f);
                mound.AddComponent<MeshFilter>().sharedMesh = rockMesh;
                mound.AddComponent<MeshRenderer>().sharedMaterial = i % 5 == 0 ? grass : rock;
                mound.AddComponent<MeshCollider>().sharedMesh = rockMesh;
            }
        }

        private static void AddTerrainTriangle(
            List<int>[] surfaceTriangles,
            Vector3[] vertices,
            int a,
            int b,
            int c)
        {
            Vector3 normal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]).normalized;
            if (normal.y < 0f) normal = -normal;

            Vector3 centre = (vertices[a] + vertices[b] + vertices[c]) / 3f;
            float slopeDegrees = Vector3.Angle(normal, Vector3.up);
            GreyhavenTerrainSurface surface = GreyhavenTerrainProfile.ClassifySurface(
                centre.y,
                slopeDegrees,
                GreyhavenTerrainProfile.InlandDistance(centre.x, centre.z));

            surfaceTriangles[(int)surface].Add(a);
            surfaceTriangles[(int)surface].Add(b);
            surfaceTriangles[(int)surface].Add(c);
        }

        private static Mesh CreateFacetedRockMesh(string name, System.Random rng)
        {
            const float t = 1.61803398875f;
            Vector3[] vertices =
            {
                new Vector3(-1f, t, 0f), new Vector3(1f, t, 0f),
                new Vector3(-1f, -t, 0f), new Vector3(1f, -t, 0f),
                new Vector3(0f, -1f, t), new Vector3(0f, 1f, t),
                new Vector3(0f, -1f, -t), new Vector3(0f, 1f, -t),
                new Vector3(t, 0f, -1f), new Vector3(t, 0f, 1f),
                new Vector3(-t, 0f, -1f), new Vector3(-t, 0f, 1f)
            };
            int[] triangles =
            {
                0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
                1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
                3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
                4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
            };

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 direction = vertices[i].normalized;
                float size = 0.84f + (float)rng.NextDouble() * 0.32f;
                direction = Vector3.Scale(direction * size,
                    new Vector3(1f, 0.72f + (float)rng.NextDouble() * 0.22f, 0.92f + (float)rng.NextDouble() * 0.16f));
                vertices[i] = direction;
            }

            Mesh mesh = new Mesh { name = name };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static float ShorelineZ(float x)
        {
            return GreyhavenTerrainProfile.ShorelineZ(x);
        }

        private static float HeightAt(float x, float z)
        {
            return GreyhavenTerrainProfile.HeightAt(x, z);
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

        private static void BuildPiers(
            Transform parent,
            Material wood,
            Material metal,
            Material chartPaper,
            Material chartWater,
            Material chartLand,
            Material chartInk,
            Material chartAccent)
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
            BuildDockMap(parent, wood, metal, chartPaper, chartWater, chartLand, chartInk, chartAccent);
        }

        private static void BuildVillage(Transform parent, Material wood, Material roof, Material concrete, Material glass, Material metal)
        {
            Building(parent, "Greyhaven_Fish_Market", new Vector3(-27f, 0f, -35f), 0f, 10f, 5.2f, 8f, wood, roof, glass, concrete, "FISH MARKET");
            Building(parent, "Harbour_Office", new Vector3(21f, 0f, -32f), 0f, 8f, 4.8f, 7f, concrete, roof, glass, metal, "HARBOUR OFFICE");
            Building(parent, "Boat_Repair_Shed", new Vector3(1f, 0f, -46f), 0f, 11f, 5f, 10f, wood, roof, glass, metal, "BOAT REPAIR");
            Building(parent, "Net_Mender_Shed", new Vector3(-48f, 0f, -52f), 12f, 7f, 4.2f, 6f, wood, roof, glass, concrete, "NET MENDER");
            Building(parent, "Harbour_Inn", new Vector3(39f, 0f, -55f), -8f, 10f, 5.4f, 8f, wood, roof, glass, concrete, "HARBOUR INN");
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

        // Static decorative workstations remain collider-free and safe for additive area extraction.
        private static void BuildHarbourmasterQuayFurniture(
            Transform parent, Material wood, Material metal, Material paper, Material ink, Material accent)
        {
            Transform station = Group("Harbourmaster_Quay_Log_Station", parent);
            station.position = new Vector3(2.35f, 0.84f, 10.2f);
            VisualCube(station, "Quay_Desk_Top", new Vector3(0f, 0.98f, 0f),
                new Vector3(2.15f, 0.14f, 1.05f), wood);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    VisualCube(station, "Quay_Desk_Leg_" + x + "_" + z,
                        new Vector3(x * 0.88f, 0.48f, z * 0.38f),
                        new Vector3(0.13f, 0.96f, 0.13f), metal);
            VisualCube(station, "Open_Tide_Logbook", new Vector3(-0.35f, 1.08f, 0.02f),
                new Vector3(0.72f, 0.045f, 0.52f), paper);
            VisualCube(station, "Logbook_Ink_Rule", new Vector3(-0.35f, 1.11f, 0.02f),
                new Vector3(0.54f, 0.012f, 0.018f), ink);
            VisualCube(station, "Brass_Chronometer_Case", new Vector3(0.55f, 1.19f, -0.12f),
                new Vector3(0.42f, 0.32f, 0.38f), accent);
            VisualCube(station, "Chronometer_Face", new Vector3(0.55f, 1.2f, 0.078f),
                new Vector3(0.28f, 0.22f, 0.025f), paper);
            VisualCube(station, "Tide_Chart_Roller", new Vector3(0f, 1.13f, 0.36f),
                new Vector3(1.55f, 0.08f, 0.1f), metal);
            VisualCube(station, "Quay_Notice_Board", new Vector3(0f, 1.82f, -0.62f),
                new Vector3(1.8f, 1.25f, 0.12f), wood);
            VisualCube(station, "Pinned_Channel_Chart", new Vector3(0f, 1.84f, -0.545f),
                new Vector3(1.42f, 0.92f, 0.025f), paper);
            VisualCube(station, "Chart_Red_Pencil_Mark", new Vector3(0.2f, 1.85f, -0.525f),
                new Vector3(0.38f, 0.025f, 0.018f), accent);
            VisualCube(station, "Quay_Stool_Seat", new Vector3(-1.65f, 0.72f, 0.35f),
                new Vector3(0.72f, 0.14f, 0.72f), wood);
            VisualCube(station, "Quay_Stool_Pedestal", new Vector3(-1.65f, 0.38f, 0.35f),
                new Vector3(0.14f, 0.68f, 0.14f), metal);
        }

        private static void BuildResidentFurniture(
            Transform parent, Material wood, Material concrete, Material metal,
            Material glass, Material paper, Material ink, Material accent)
        {
            BuildHarbourOfficeRecordStation(parent, wood, metal, paper, ink, accent);
            BuildMaraWorkstation(parent, wood, metal, glass, paper, ink, accent);
            BuildTomasWorkshop(parent, wood, metal, concrete, glass, accent);
            BuildInezMendingStation(parent, wood, metal, glass, paper, accent);
            BuildInnCommonFurniture(parent, wood, metal, glass);
        }

        private static Transform FurnitureStation(Transform parent, string name, Vector3 position, float yaw = 0f)
        {
            Transform station = Group(name, parent);
            station.position = new Vector3(position.x, HeightAt(position.x, position.z), position.z);
            station.rotation = Quaternion.Euler(0f, yaw, 0f);
            return station;
        }

        private static void BuildHarbourOfficeRecordStation(
            Transform parent, Material wood, Material metal, Material paper, Material ink, Material accent)
        {
            Transform station = FurnitureStation(parent, "Harbourmaster_Off_Duty_Record_Corner",
                new Vector3(26f, 0f, -28.6f));
            VisualCube(station, "Record_Cabinet_Body", new Vector3(0f, 0.82f, 0f),
                new Vector3(1.45f, 1.64f, 0.62f), wood);
            VisualCube(station, "Cabinet_Top", new Vector3(0f, 1.68f, 0f),
                new Vector3(1.58f, 0.12f, 0.72f), metal);
            for (int shelf = 0; shelf < 3; shelf++)
            {
                float y = 0.38f + shelf * 0.42f;
                VisualCube(station, "Tide_Register_Spine_" + shelf,
                    new Vector3(-0.18f, y, -0.33f), new Vector3(0.22f, 0.32f, 0.08f), accent);
                VisualCube(station, "Tide_Register_Pages_" + shelf,
                    new Vector3(0.18f, y, -0.33f), new Vector3(0.48f, 0.3f, 0.12f), paper);
            }
            VisualCube(station, "Office_Weather_Board", new Vector3(1.35f, 1.45f, -0.15f),
                new Vector3(0.12f, 1.55f, 1.2f), wood);
            VisualCube(station, "Weather_Board_Chart", new Vector3(1.275f, 1.45f, -0.15f),
                new Vector3(0.025f, 1.2f, 0.9f), paper);
            VisualCube(station, "Weather_Board_Annotations", new Vector3(1.25f, 1.48f, -0.2f),
                new Vector3(0.018f, 0.035f, 0.58f), ink);
        }

        private static void BuildMaraWorkstation(
            Transform parent, Material wood, Material metal, Material glass, Material paper, Material ink, Material accent)
        {
            Transform station = FurnitureStation(parent, "Mara_Venn_Market_Workstation",
                new Vector3(-21.8f, 0f, -29.7f));
            VisualCube(station, "Catch_Sorting_Table", new Vector3(0f, 0.88f, 0f),
                new Vector3(3f, 0.16f, 1.2f), wood);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    VisualCube(station, "Sorting_Table_Leg_" + x + "_" + z,
                        new Vector3(x * 1.28f, 0.43f, z * 0.43f),
                        new Vector3(0.14f, 0.86f, 0.14f), metal);
            VisualCube(station, "Brass_Produce_Scale_Base", new Vector3(-0.82f, 1.04f, -0.08f),
                new Vector3(0.72f, 0.16f, 0.62f), metal);
            VisualCube(station, "Scale_Weighing_Pan", new Vector3(-0.82f, 1.18f, -0.08f),
                new Vector3(0.56f, 0.06f, 0.48f), accent);
            VisualCube(station, "Scale_Dial", new Vector3(-0.82f, 1.42f, -0.35f),
                new Vector3(0.38f, 0.34f, 0.12f), paper);
            VisualCube(station, "Catch_Tally_Board", new Vector3(0f, 1.8f, -0.78f),
                new Vector3(1.65f, 1f, 0.12f), wood);
            VisualCube(station, "Tally_Board_Paper", new Vector3(0f, 1.8f, -0.705f),
                new Vector3(1.35f, 0.76f, 0.025f), paper);
            for (int row = 0; row < 4; row++)
                VisualCube(station, "Tally_Mark_" + row,
                    new Vector3(-0.32f + row * 0.19f, 1.72f, -0.68f),
                    new Vector3(0.025f, 0.34f, 0.018f), ink);
            VisualCube(station, "Sea_Glass_Collection_Shelf", new Vector3(1.12f, 1.22f, -0.08f),
                new Vector3(0.82f, 0.12f, 0.42f), metal);
            for (int jar = 0; jar < 3; jar++)
                VisualCube(station, "Sea_Glass_Sample_Jar_" + jar,
                    new Vector3(0.82f + jar * 0.28f, 1.42f, -0.08f),
                    new Vector3(0.18f, 0.34f, 0.18f), glass);
            VisualCube(station, "Personal_Shell_Collection_Box", new Vector3(1.1f, 0.56f, 0.82f),
                new Vector3(0.72f, 0.5f, 0.58f), accent);
            VisualCube(station, "Market_Crate_Stool", new Vector3(-1.85f, 0.36f, 0.75f),
                new Vector3(0.72f, 0.72f, 0.72f), wood);
        }

        private static void BuildTomasWorkshop(
            Transform parent, Material wood, Material metal, Material concrete, Material glass, Material accent)
        {
            Transform station = FurnitureStation(parent, "Tomas_Vale_Repair_Workbench",
                new Vector3(9f, 0f, -43f));
            VisualCube(station, "Heavy_Repair_Bench_Top", new Vector3(0f, 1.02f, 0f),
                new Vector3(3.7f, 0.2f, 1.25f), wood);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    VisualCube(station, "Repair_Bench_Leg_" + x + "_" + z,
                        new Vector3(x * 1.62f, 0.5f, z * 0.48f),
                        new Vector3(0.22f, 1f, 0.22f), metal);
            VisualCube(station, "Engine_Block_Test_Housing", new Vector3(-0.62f, 1.35f, 0.02f),
                new Vector3(0.92f, 0.72f, 0.76f), concrete);
            VisualCube(station, "Engine_Cylinder_Head", new Vector3(-0.62f, 1.76f, 0.02f),
                new Vector3(0.64f, 0.16f, 0.56f), metal);
            VisualCube(station, "Engine_Carburettor_Cap", new Vector3(-0.62f, 1.92f, 0.02f),
                new Vector3(0.28f, 0.16f, 0.28f), accent);
            VisualCube(station, "Bench_Vice_Base", new Vector3(1.25f, 1.18f, -0.1f),
                new Vector3(0.52f, 0.2f, 0.42f), metal);
            VisualCube(station, "Bench_Vice_Jaws", new Vector3(1.25f, 1.38f, -0.1f),
                new Vector3(0.62f, 0.16f, 0.18f), metal);
            VisualCube(station, "Workshop_Pegboard", new Vector3(0f, 1.9f, -0.82f),
                new Vector3(3.5f, 1.5f, 0.12f), wood);
            for (int tool = 0; tool < 5; tool++)
            {
                float x = -1.25f + tool * 0.62f;
                VisualCube(station, "Hanging_Tool_Handle_" + tool,
                    new Vector3(x, 1.85f, -0.72f), new Vector3(0.07f, 0.62f, 0.07f), accent);
                VisualCube(station, "Hanging_Tool_Head_" + tool,
                    new Vector3(x, 2.2f, -0.72f), new Vector3(0.28f, 0.12f, 0.1f), metal);
            }
            VisualCube(station, "Radio_Experiment_Casing", new Vector3(1.18f, 1.42f, 0.24f),
                new Vector3(0.62f, 0.48f, 0.5f), metal);
            VisualCube(station, "Radio_Tuning_Window", new Vector3(1.18f, 1.46f, 0.5f),
                new Vector3(0.38f, 0.18f, 0.025f), glass);
            VisualCube(station, "Radio_Antenna", new Vector3(1.18f, 1.92f, 0.2f),
                new Vector3(0.045f, 0.62f, 0.045f), accent);
            VisualCube(station, "Spare_Propeller_Blades_Horizontal", new Vector3(-1.42f, 0.24f, 0.85f),
                new Vector3(1.2f, 0.12f, 0.18f), metal);
            VisualCube(station, "Spare_Propeller_Blades_Vertical", new Vector3(-1.42f, 0.24f, 0.85f),
                new Vector3(0.18f, 0.12f, 1.2f), metal);
        }

        private static void BuildInezMendingStation(
            Transform parent, Material wood, Material metal, Material glass, Material paper, Material accent)
        {
            Transform station = FurnitureStation(parent, "Inez_Sarmiento_Net_Mending_Workspace",
                new Vector3(-41.8f, 0f, -46.8f), 12f);
            VisualCube(station, "Net_Loom_Work_Table", new Vector3(0f, 0.9f, 0f),
                new Vector3(3f, 0.16f, 1f), wood);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    VisualCube(station, "Loom_Table_Leg_" + x + "_" + z,
                        new Vector3(x * 1.28f, 0.43f, z * 0.36f),
                        new Vector3(0.14f, 0.86f, 0.14f), metal);
            VisualCube(station, "Net_Mending_Frame_Top", new Vector3(0f, 2.12f, -0.68f),
                new Vector3(2.7f, 0.16f, 0.16f), wood);
            for (int side = -1; side <= 1; side += 2)
                VisualCube(station, "Net_Mending_Frame_Post_" + side,
                    new Vector3(side * 1.25f, 1.52f, -0.68f),
                    new Vector3(0.14f, 1.35f, 0.14f), wood);
            for (int strand = 0; strand < 7; strand++)
                VisualCube(station, "Net_Loom_Vertical_Line_" + strand,
                    new Vector3(-1.08f + strand * 0.36f, 1.52f, -0.57f),
                    new Vector3(0.025f, 1.08f, 0.025f), glass);
            for (int strand = 0; strand < 4; strand++)
                VisualCube(station, "Net_Loom_Horizontal_Line_" + strand,
                    new Vector3(0f, 1.08f + strand * 0.28f, -0.57f),
                    new Vector3(2.2f, 0.025f, 0.025f), glass);
            VisualCube(station, "Knot_Sample_Board", new Vector3(1.82f, 1.55f, -0.2f),
                new Vector3(0.12f, 1.45f, 1.1f), wood);
            for (int knot = 0; knot < 4; knot++)
                VisualCube(station, "Pinned_Knot_Sample_" + knot,
                    new Vector3(1.74f, 1.75f, -0.56f + knot * 0.24f),
                    new Vector3(0.05f, 0.12f, 0.12f), accent);
            VisualCube(station, "Line_Spool_Rack", new Vector3(-1.95f, 1.2f, 0f),
                new Vector3(0.14f, 1.5f, 0.14f), metal);
            for (int spool = 0; spool < 4; spool++)
                VisualCube(station, "Thread_Spool_" + spool,
                    new Vector3(-1.95f, 0.62f + spool * 0.34f, 0.22f),
                    new Vector3(0.32f, 0.16f, 0.32f), spool % 2 == 0 ? paper : accent);
            VisualCube(station, "Sea_Glass_Sorting_Tray", new Vector3(0.72f, 1.05f, 0.18f),
                new Vector3(0.72f, 0.08f, 0.56f), glass);
            VisualCube(station, "Mending_Stool_Seat", new Vector3(-1.65f, 0.68f, 0.92f),
                new Vector3(0.68f, 0.14f, 0.68f), wood);
            VisualCube(station, "Mending_Stool_Legs", new Vector3(-1.65f, 0.34f, 0.92f),
                new Vector3(0.46f, 0.64f, 0.46f), metal);
        }

        private static void BuildInnCommonFurniture(Transform parent, Material wood, Material metal, Material glass)
        {
            Transform station = FurnitureStation(parent, "Harbour_Inn_Common_Area_Furniture",
                new Vector3(45f, 0f, -49.5f), -8f);
            VisualCube(station, "Common_Room_Table_Top", new Vector3(0f, 0.86f, 0f),
                new Vector3(2.1f, 0.14f, 1.35f), wood);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    VisualCube(station, "Common_Room_Table_Leg_" + x + "_" + z,
                        new Vector3(x * 0.88f, 0.42f, z * 0.52f),
                        new Vector3(0.14f, 0.82f, 0.14f), metal);
            for (int seat = 0; seat < 2; seat++)
            {
                float x = seat == 0 ? -1.75f : 1.75f;
                VisualCube(station, "Inn_Bench_Seat_" + seat, new Vector3(x, 0.62f, 0f),
                    new Vector3(0.72f, 0.14f, 1.2f), wood);
                VisualCube(station, "Inn_Bench_Back_" + seat, new Vector3(x, 1.08f, -0.48f),
                    new Vector3(0.72f, 0.82f, 0.14f), wood);
            }
            VisualCube(station, "Inn_Table_Lantern_Base", new Vector3(0f, 1.02f, 0f),
                new Vector3(0.28f, 0.12f, 0.28f), metal);
            VisualCube(station, "Inn_Table_Lantern_Glass", new Vector3(0f, 1.26f, 0f),
                new Vector3(0.2f, 0.36f, 0.2f), glass);
        }

        private static void Building(
            Transform parent,
            string name,
            Vector3 position,
            float yaw,
            float width,
            float height,
            float depth,
            Material wall,
            Material roof,
            Material glass,
            Material trim,
            string signText = null)
        {
            Transform building = Group(name, parent);
            building.position = new Vector3(position.x, HeightAt(position.x, position.z), position.z);
            building.rotation = Quaternion.Euler(0f, yaw, 0f);

            VisualCube(building, "Stone_Foundation", new Vector3(0f, 0.16f, 0f),
                new Vector3(width + 0.42f, 0.32f, depth + 0.42f), trim);

            LocalCube(building, "Main_Wall_Mass", new Vector3(0f, height * 0.5f, 0f),
                new Vector3(width, height, depth), wall);

            BuildGableRoof(building, width, height, depth, wall, roof, trim);

            float front = depth * 0.5f;
            float sidePostX = width * 0.5f - 0.18f;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int end = -1; end <= 1; end += 2)
                {
                    VisualCube(building, "Corner_Post_" + side + "_" + end,
                        new Vector3(side * sidePostX, height * 0.5f, end * (depth * 0.5f - 0.16f)),
                        new Vector3(0.22f, height + 0.06f, 0.22f), trim);
                }
            }

            VisualCube(building, "Front_Door", new Vector3(0f, 1.2f, front + 0.12f),
                new Vector3(1.18f, 2.4f, 0.16f), trim);
            VisualCube(building, "Door_Frame_Left", new Vector3(-0.68f, 1.22f, front + 0.2f),
                new Vector3(0.16f, 2.48f, 0.2f), wall);
            VisualCube(building, "Door_Frame_Right", new Vector3(0.68f, 1.22f, front + 0.2f),
                new Vector3(0.16f, 2.48f, 0.2f), wall);
            VisualCube(building, "Door_Frame_Lintel", new Vector3(0f, 2.48f, front + 0.2f),
                new Vector3(1.52f, 0.18f, 0.2f), wall);
            VisualCube(building, "Door_Step", new Vector3(0f, 0.14f, front + 0.58f),
                new Vector3(1.75f, 0.24f, 0.82f), trim);
            VisualCube(building, "Door_Handle", new Vector3(0.38f, 1.25f, front + 0.23f),
                new Vector3(0.07f, 0.2f, 0.07f), glass);

            BuildFrontWindow(building, "Front_Window_Left",
                new Vector3(-width * 0.29f, height * 0.62f, front + 0.13f), glass, trim, wall);
            BuildFrontWindow(building, "Front_Window_Right",
                new Vector3(width * 0.29f, height * 0.62f, front + 0.13f), glass, trim, wall);

            VisualCube(building, "Front_Eave_Beam", new Vector3(0f, height + 0.08f, front + 0.08f),
                new Vector3(width + 0.7f, 0.2f, 0.22f), trim);

            if (!string.IsNullOrWhiteSpace(signText))
            {
                VisualCube(building, "Service_Sign_Plaque",
                    new Vector3(0f, height - 0.55f, front + 0.25f),
                    new Vector3(Mathf.Min(width * 0.78f, 7.7f), 0.66f, 0.13f), trim);
                AddWorldLabel(building, "Service_Sign_Text", signText,
                    new Vector3(0f, height - 0.55f, front + 0.33f),
                    Quaternion.identity, 0.075f, 42, TextAnchor.MiddleCenter, TextAlignment.Center,
                    new Color(0.08f, 0.11f, 0.11f));
            }

            BuildBuildingStyle(building, name, width, height, depth, wall, roof, glass, trim);
        }

        private static void BuildGableRoof(
            Transform parent,
            float width,
            float height,
            float depth,
            Material wall,
            Material roof,
            Material trim)
        {
            float halfWidth = width * 0.5f + 0.45f;
            float front = depth * 0.5f + 0.38f;
            float back = -depth * 0.5f - 0.38f;
            float eave = height + 0.16f;
            float rise = Mathf.Clamp(width * 0.18f, 1.3f, 2.15f);
            float ridge = eave + rise;

            Vector3[] roofVertices =
            {
                new Vector3(-halfWidth, eave, front),
                new Vector3(0f, ridge, front),
                new Vector3(0f, ridge, back),
                new Vector3(-halfWidth, eave, back),
                new Vector3(0f, ridge, front),
                new Vector3(halfWidth, eave, front),
                new Vector3(halfWidth, eave, back),
                new Vector3(0f, ridge, back)
            };
            int[] roofTriangles =
            {
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7
            };

            Mesh roofMesh = new Mesh { name = parent.name + "_Pitched_Roof" };
            roofMesh.vertices = roofVertices;
            roofMesh.triangles = roofTriangles;
            roofMesh.RecalculateNormals();
            roofMesh.RecalculateBounds();

            GameObject roofObject = new GameObject("Pitched_Roof_Mesh");
            roofObject.transform.SetParent(parent, false);
            roofObject.AddComponent<MeshFilter>().sharedMesh = roofMesh;
            roofObject.AddComponent<MeshRenderer>().sharedMaterial = roof;

            Vector3[] gableVertices =
            {
                new Vector3(-width * 0.5f, height, front),
                new Vector3(0f, ridge, front),
                new Vector3(width * 0.5f, height, front),
                new Vector3(-width * 0.5f, height, back),
                new Vector3(0f, ridge, back),
                new Vector3(width * 0.5f, height, back)
            };
            int[] gableTriangles = { 0, 2, 1, 3, 4, 5 };
            Mesh gableMesh = new Mesh { name = parent.name + "_Gable_End_Caps" };
            gableMesh.vertices = gableVertices;
            gableMesh.triangles = gableTriangles;
            gableMesh.RecalculateNormals();
            gableMesh.RecalculateBounds();

            GameObject gables = new GameObject("Gable_End_Caps");
            gables.transform.SetParent(parent, false);
            gables.AddComponent<MeshFilter>().sharedMesh = gableMesh;
            gables.AddComponent<MeshRenderer>().sharedMaterial = wall;

            VisualCube(parent, "Roof_Ridge_Cap", new Vector3(0f, ridge + 0.04f, 0f),
                new Vector3(0.2f, 0.16f, depth + 1.0f), trim);
            VisualCube(parent, "Roof_Eave_Left", new Vector3(-width * 0.5f - 0.2f, eave - 0.05f, 0f),
                new Vector3(0.18f, 0.22f, depth + 0.9f), trim);
            VisualCube(parent, "Roof_Eave_Right", new Vector3(width * 0.5f + 0.2f, eave - 0.05f, 0f),
                new Vector3(0.18f, 0.22f, depth + 0.9f), trim);
        }

        private static void BuildFrontWindow(
            Transform parent,
            string name,
            Vector3 centre,
            Material glass,
            Material wood,
            Material trim)
        {
            VisualCube(parent, name + "_Glass", centre, new Vector3(1.22f, 1.02f, 0.11f), glass);
            VisualCube(parent, name + "_Frame_Left",
                centre + new Vector3(-0.69f, 0f, 0.015f), new Vector3(0.14f, 1.26f, 0.16f), wood);
            VisualCube(parent, name + "_Frame_Right",
                centre + new Vector3(0.69f, 0f, 0.015f), new Vector3(0.14f, 1.26f, 0.16f), wood);
            VisualCube(parent, name + "_Frame_Top",
                centre + new Vector3(0f, 0.58f, 0.015f), new Vector3(1.52f, 0.14f, 0.16f), wood);
            VisualCube(parent, name + "_Frame_Sill",
                centre + new Vector3(0f, -0.58f, 0.06f), new Vector3(1.62f, 0.16f, 0.28f), trim);
            VisualCube(parent, name + "_Mullion",
                centre + new Vector3(0f, 0f, 0.05f), new Vector3(0.07f, 1.0f, 0.17f), wood);
            VisualCube(parent, name + "_Shutter_Left",
                centre + new Vector3(-0.9f, 0f, 0.03f), new Vector3(0.23f, 1.15f, 0.12f), trim);
            VisualCube(parent, name + "_Shutter_Right",
                centre + new Vector3(0.9f, 0f, 0.03f), new Vector3(0.23f, 1.15f, 0.12f), trim);
        }

        private static void BuildBuildingStyle(
            Transform parent,
            string name,
            float width,
            float height,
            float depth,
            Material wall,
            Material roof,
            Material glass,
            Material trim)
        {
            float front = depth * 0.5f;

            if (name.Contains("Fish_Market"))
            {
                VisualCube(parent, "Market_Canopy", new Vector3(0f, height + 0.9f, front + 1.25f),
                    new Vector3(width * 0.82f, 0.18f, 2.7f), roof, Quaternion.Euler(7f, 0f, 0f));
                for (int side = -1; side <= 1; side += 2)
                    VisualCube(parent, "Market_Canopy_Post_" + side,
                        new Vector3(side * width * 0.37f, height * 0.48f, front + 2.35f),
                        new Vector3(0.18f, height * 0.92f, 0.18f), trim);
                VisualCube(parent, "Market_Service_Counter",
                    new Vector3(0f, 0.92f, front + 0.88f),
                    new Vector3(width * 0.64f, 1.25f, 0.72f), trim);
                VisualCube(parent, "Market_Counter_Top",
                    new Vector3(0f, 1.62f, front + 0.88f),
                    new Vector3(width * 0.68f, 0.12f, 0.86f), roof);
                for (int i = -1; i <= 1; i += 2)
                {
                    VisualCube(parent, "Market_Crate_" + i,
                        new Vector3(i * 3.1f, 0.46f, front + 1.15f),
                        new Vector3(0.9f, 0.82f, 0.85f), wall);
                }
                VisualCube(parent, "Market_Fish_Tray",
                    new Vector3(0f, 1.78f, front + 0.92f),
                    new Vector3(1.4f, 0.12f, 0.5f), glass);
            }
            else if (name.Contains("Harbour_Office"))
            {
                VisualCube(parent, "Office_Entry_Porch", new Vector3(0f, 0.18f, front + 1.0f),
                    new Vector3(width * 0.76f, 0.24f, 2.0f), trim);
                for (int side = -1; side <= 1; side += 2)
                    VisualCube(parent, "Office_Porch_Post_" + side,
                        new Vector3(side * width * 0.31f, 1.55f, front + 1.7f),
                        new Vector3(0.2f, 3.1f, 0.2f), wall);
                VisualCube(parent, "Office_Porch_Roof",
                    new Vector3(0f, 3.18f, front + 1.55f),
                    new Vector3(width * 0.9f, 0.2f, 2.35f), roof, Quaternion.Euler(5f, 0f, 0f));
                VisualCube(parent, "Office_Bell_Frame",
                    new Vector3(width * 0.35f, height + 1.25f, -depth * 0.25f),
                    new Vector3(0.16f, 2.2f, 0.16f), trim);
                VisualCube(parent, "Office_Bell_Beam",
                    new Vector3(width * 0.35f, height + 2.25f, -depth * 0.25f),
                    new Vector3(1.35f, 0.15f, 0.15f), trim);
                VisualCube(parent, "Office_Bell_Rope",
                    new Vector3(width * 0.35f, height + 1.45f, -depth * 0.25f),
                    new Vector3(0.035f, 1.45f, 0.035f), glass);
            }
            else if (name.Contains("Boat_Repair"))
            {
                VisualCube(parent, "Repair_Bay_Dark_Opening",
                    new Vector3(0f, 2.12f, front + 0.105f), new Vector3(5.6f, 3.95f, 0.12f), wall);
                for (int side = -1; side <= 1; side += 2)
                    VisualCube(parent, "Repair_Sliding_Door_" + side,
                        new Vector3(side * 2.25f, 2.1f, front + 0.2f),
                        new Vector3(0.92f, 3.75f, 0.2f), trim);
                VisualCube(parent, "Repair_Hoist_Rail",
                    new Vector3(0f, height - 0.65f, front + 0.75f),
                    new Vector3(width * 0.72f, 0.18f, 0.18f), trim);
                for (int side = -1; side <= 1; side += 2)
                {
                    VisualCube(parent, "Repair_Hoist_Cable_" + side,
                        new Vector3(side * 1.3f, height - 1.55f, front + 0.75f),
                        new Vector3(0.045f, 1.65f, 0.045f), glass);
                }
                VisualCube(parent, "Repair_Hoist_Hook",
                    new Vector3(0f, height - 2.48f, front + 0.75f),
                    new Vector3(0.42f, 0.12f, 0.12f), trim);
                VisualCube(parent, "Repair_Tool_Bench",
                    new Vector3(width * 0.36f, 1.0f, front + 0.85f),
                    new Vector3(1.65f, 1.0f, 0.65f), wall);
            }
            else if (name.Contains("Net_Mender"))
            {
                VisualCube(parent, "Net_Mender_Lean_To",
                    new Vector3(width * 0.5f + 1.25f, 2.45f, 0f),
                    new Vector3(2.8f, 0.16f, depth + 0.8f), roof, Quaternion.Euler(0f, 0f, -5f));
                for (int side = -1; side <= 1; side += 2)
                    VisualCube(parent, "Net_Mender_Lean_To_Post_" + side,
                        new Vector3(width * 0.5f + 2.45f, 1.15f, side * (depth * 0.35f)),
                        new Vector3(0.16f, 2.3f, 0.16f), trim);
                VisualCube(parent, "Net_Mender_Work_Table",
                    new Vector3(width * 0.5f + 1.4f, 0.9f, 0f),
                    new Vector3(2.4f, 0.2f, 2.0f), trim);
                for (int i = 0; i < 7; i++)
                {
                    float z = -depth * 0.34f + i * (depth * 0.68f / 6f);
                    VisualCube(parent, "Hanging_Net_Line_" + i,
                        new Vector3(width * 0.5f + 1.95f, 1.45f, z),
                        new Vector3(0.035f, 1.55f, 0.035f), glass);
                }
            }
            else if (name.Contains("Harbour_Inn"))
            {
                VisualCube(parent, "Inn_Veranda_Deck",
                    new Vector3(0f, 0.18f, front + 1.55f),
                    new Vector3(width + 1.25f, 0.24f, 2.9f), trim);
                for (int side = -1; side <= 1; side += 2)
                {
                    for (int post = -1; post <= 1; post += 2)
                    {
                        VisualCube(parent, "Inn_Veranda_Post_" + side + "_" + post,
                            new Vector3(side * (width * 0.5f + 0.38f), 1.65f, front + 0.45f + (post + 1) * 0.95f),
                            new Vector3(0.18f, 3.3f, 0.18f), wall);
                    }
                    VisualCube(parent, "Inn_Veranda_Rail_" + side,
                        new Vector3(side * (width * 0.5f + 0.38f), 0.88f, front + 1.7f),
                        new Vector3(0.14f, 0.16f, 2.35f), trim);
                }
                VisualCube(parent, "Inn_Bench",
                    new Vector3(width * 0.25f, 0.62f, front + 1.7f),
                    new Vector3(2.3f, 0.18f, 0.65f), wall);
            }
            else
            {
                VisualCube(parent, "House_Porch_Deck",
                    new Vector3(0f, 0.15f, front + 0.95f),
                    new Vector3(width * 0.74f, 0.22f, 1.8f), trim);
                for (int side = -1; side <= 1; side += 2)
                {
                    VisualCube(parent, "House_Porch_Post_" + side,
                        new Vector3(side * width * 0.3f, 1.35f, front + 1.55f),
                        new Vector3(0.16f, 2.7f, 0.16f), wall);
                    VisualCube(parent, "House_Porch_Rail_" + side,
                        new Vector3(side * width * 0.3f, 0.78f, front + 1.05f),
                        new Vector3(0.1f, 0.12f, 0.95f), trim);
                }
                VisualCube(parent, "House_Chimney",
                    new Vector3(-width * 0.28f, height + 1.45f, -depth * 0.18f),
                    new Vector3(0.7f, 2.6f, 0.75f), trim);
            }
        }

        private static void BuildDockMap(
            Transform parent,
            Material wood,
            Material metal,
            Material chartPaper,
            Material chartWater,
            Material chartLand,
            Material chartInk,
            Material chartAccent)
        {
            Transform chart = Group("Greyhaven_Dock_Chart_Table", parent);
            // The table is on the east side of the T-pier, clear of the central mooring lane.
            chart.position = new Vector3(7.35f, 0.88f, 33f);

            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    VisualCube(chart, "Chart_Table_Leg_" + x + "_" + z,
                        new Vector3(x * 3.25f, 0.42f, z * 1.95f),
                        new Vector3(0.24f, 0.84f, 0.24f), metal);
                }
            }

            VisualCube(chart, "Chart_Table_Frame",
                new Vector3(0f, 0.86f, 0f), new Vector3(7.55f, 0.18f, 4.85f), wood);
            VisualCube(chart, "Chart_Parchment_Surface",
                new Vector3(0f, 0.98f, 0f), new Vector3(7.3f, 0.06f, 4.6f), chartPaper);

            // A muted sea field makes the chart visibly distinct from the dock timbers.
            VisualCube(chart, "Chart_Sea_Field",
                new Vector3(-0.25f, 1.02f, 0f), new Vector3(5.65f, 0.025f, 3.55f), chartWater);

            GameObject island = new GameObject("Chart_Greyhaven_Island_Silhouette");
            island.transform.SetParent(chart, false);
            island.AddComponent<MeshFilter>().sharedMesh = CreateDockChartLandMesh();
            island.AddComponent<MeshRenderer>().sharedMaterial = chartLand;

            // Coastline and a restrained road network are drawn directly into the physical chart.
            Vector2 previous = DockChartPoint(-110f, GreyhavenTerrainProfile.ShorelineZ(-110f));
            for (int i = 1; i <= 80; i++)
            {
                float x = Mathf.Lerp(-110f, 110f, i / 80f);
                Vector2 next = DockChartPoint(x, GreyhavenTerrainProfile.ShorelineZ(x));
                DockChartLine(chart, "Chart_Shoreline_" + i, previous, next, 0.045f, chartInk);
                previous = next;
            }

            Vector2 villageHub = DockChartPoint(0f, -43f);
            Vector2 dockPoint = DockChartPoint(7.35f, 33f);
            Vector2 quayWatch = DockChartPoint(-1.6f, 12f);
            Vector2 market = DockChartPoint(-27f, -35f);
            Vector2 office = DockChartPoint(21f, -32f);
            Vector2 repair = DockChartPoint(1f, -46f);
            Vector2 netMender = DockChartPoint(-48f, -52f);
            Vector2 inn = DockChartPoint(39f, -55f);
            Vector2 lighthouse = DockChartPoint(-86f, -22f);
            Vector2 wreck = DockChartPoint(30f, 55f);

            DockChartLine(chart, "Chart_Road_Dock_To_Village", dockPoint, villageHub, 0.055f, chartAccent);
            DockChartLine(chart, "Chart_Road_Market", villageHub, market, 0.035f, chartInk);
            DockChartLine(chart, "Chart_Road_Office", villageHub, office, 0.035f, chartInk);
            DockChartLine(chart, "Chart_Road_Repair", villageHub, repair, 0.035f, chartInk);
            DockChartLine(chart, "Chart_Road_Net_Mender", villageHub, netMender, 0.035f, chartInk);
            DockChartLine(chart, "Chart_Road_Inn", villageHub, inn, 0.035f, chartInk);
            DockChartLine(chart, "Chart_Lighthouse_Path", lighthouse, market, 0.025f, chartInk);

            DockChartMarker(chart, "Chart_Marker_Dock_You_Are_Here", dockPoint, chartAccent, 0.23f);
            DockChartMarker(chart, "Chart_Marker_Harbourmaster_Quay", quayWatch, chartAccent, 0.17f);
            DockChartMarker(chart, "Chart_Marker_Market_Mara", market, chartInk, 0.16f);
            DockChartMarker(chart, "Chart_Marker_Office", office, chartInk, 0.16f);
            DockChartMarker(chart, "Chart_Marker_Repair_Tomas", repair, chartInk, 0.16f);
            DockChartMarker(chart, "Chart_Marker_Net_Mender_Inez", netMender, chartInk, 0.16f);
            DockChartMarker(chart, "Chart_Marker_Inn", inn, chartInk, 0.14f);
            DockChartMarker(chart, "Chart_Marker_Lighthouse", lighthouse, chartAccent, 0.18f);
            DockChartMarker(chart, "Chart_Marker_Outer_Channel_Wreck", wreck, chartAccent, 0.16f);

            DockChartPinLabel(chart, "Chart_Pin_D", "D", dockPoint, chartAccent);
            DockChartPinLabel(chart, "Chart_Pin_H", "H", quayWatch, chartAccent);
            DockChartPinLabel(chart, "Chart_Pin_O", "O", office, chartInk);
            DockChartPinLabel(chart, "Chart_Pin_M", "M", market, chartInk);
            DockChartPinLabel(chart, "Chart_Pin_T", "T", repair, chartInk);
            DockChartPinLabel(chart, "Chart_Pin_I", "I", netMender, chartInk);
            DockChartPinLabel(chart, "Chart_Pin_L", "L", lighthouse, chartAccent);
            DockChartPinLabel(chart, "Chart_Pin_W", "W", wreck, chartAccent);

            AddWorldLabel(chart, "Chart_Title", "GREYHAVEN  /  HARBOUR CHART",
                new Vector3(-3.48f, 1.105f, 2.06f), Quaternion.Euler(-90f, 0f, 0f),
                0.055f, 48, TextAnchor.MiddleLeft, TextAlignment.Left, chartInk.color);
            AddWorldLabel(chart, "Chart_North_Arrow", "N  ↑",
                new Vector3(2.17f, 1.105f, 1.57f), Quaternion.Euler(-90f, 0f, 0f),
                0.055f, 42, TextAnchor.MiddleCenter, TextAlignment.Center, chartInk.color);

            AddWorldLabel(chart, "Chart_Key_Header", "KEY / PEOPLE",
                new Vector3(2.72f, 1.105f, 1.05f), Quaternion.Euler(-90f, 0f, 0f),
                0.045f, 40, TextAnchor.MiddleLeft, TextAlignment.Left, chartInk.color);
            AddWorldLabel(chart, "Chart_Key_Dock", "D / DOCK CHART TABLE",
                new Vector3(2.72f, 1.105f, 0.68f), Quaternion.Euler(-90f, 0f, 0f),
                0.032f, 30, TextAnchor.MiddleLeft, TextAlignment.Left, chartAccent.color);
            AddWorldLabel(chart, "Chart_Key_Harbourmaster_Quay", "H / HARBOURMASTER QUAY WATCH",
                new Vector3(2.72f, 1.105f, 0.34f), Quaternion.Euler(-90f, 0f, 0f),
                0.030f, 28, TextAnchor.MiddleLeft, TextAlignment.Left, chartInk.color);
            AddWorldLabel(chart, "Chart_Key_Office", "O / HARBOUR OFFICE",
                new Vector3(2.72f, 1.105f, 0.00f), Quaternion.Euler(-90f, 0f, 0f),
                0.032f, 30, TextAnchor.MiddleLeft, TextAlignment.Left, chartInk.color);
            AddWorldLabel(chart, "Chart_Key_Mara", "M / MARA VENN - MARKET",
                new Vector3(2.72f, 1.105f, -0.34f), Quaternion.Euler(-90f, 0f, 0f),
                0.032f, 30, TextAnchor.MiddleLeft, TextAlignment.Left, chartInk.color);
            AddWorldLabel(chart, "Chart_Key_Tomas", "T / TOMAS VALE - REPAIR",
                new Vector3(2.72f, 1.105f, -0.68f), Quaternion.Euler(-90f, 0f, 0f),
                0.032f, 30, TextAnchor.MiddleLeft, TextAlignment.Left, chartInk.color);
            AddWorldLabel(chart, "Chart_Key_Inez", "I / INEZ - NET SHED",
                new Vector3(2.72f, 1.105f, -1.02f), Quaternion.Euler(-90f, 0f, 0f),
                0.032f, 30, TextAnchor.MiddleLeft, TextAlignment.Left, chartInk.color);
            AddWorldLabel(chart, "Chart_Key_Landmark", "L / WEST LIGHTHOUSE",
                new Vector3(2.72f, 1.105f, -1.36f), Quaternion.Euler(-90f, 0f, 0f),
                0.032f, 30, TextAnchor.MiddleLeft, TextAlignment.Left, chartInk.color);
            AddWorldLabel(chart, "Chart_Key_Wreck", "W / OUTER WRECK FIELD",
                new Vector3(2.72f, 1.105f, -1.70f), Quaternion.Euler(-90f, 0f, 0f),
                0.032f, 30, TextAnchor.MiddleLeft, TextAlignment.Left, chartAccent.color);

            VisualCube(chart, "Chart_Frame_Top", new Vector3(0f, 1.035f, 2.23f),
                new Vector3(7.35f, 0.09f, 0.12f), wood);
            VisualCube(chart, "Chart_Frame_Bottom", new Vector3(0f, 1.035f, -2.23f),
                new Vector3(7.35f, 0.09f, 0.12f), wood);
            VisualCube(chart, "Chart_Frame_Left", new Vector3(-3.62f, 1.035f, 0f),
                new Vector3(0.12f, 0.09f, 4.5f), wood);
            VisualCube(chart, "Chart_Frame_Right", new Vector3(3.62f, 1.035f, 0f),
                new Vector3(0.12f, 0.09f, 4.5f), wood);
        }

        private static Mesh CreateDockChartLandMesh()
        {
            const int columns = 72;
            const int rows = 28;
            Vector3[] vertices = new Vector3[(columns + 1) * (rows + 1)];
            int[] triangles = new int[columns * rows * 6];

            for (int ix = 0; ix <= columns; ix++)
            {
                float x = Mathf.Lerp(-110f, 110f, ix / (float)columns);
                float shore = GreyhavenTerrainProfile.ShorelineZ(x);
                for (int iz = 0; iz <= rows; iz++)
                {
                    float z = Mathf.Lerp(GreyhavenTerrainProfile.BackZ, shore, iz / (float)rows);
                    int index = ix * (rows + 1) + iz;
                    vertices[index] = new Vector3(x * 0.0225f, 1.055f, -1.45f + (z + 112f) * 0.018f);
                }
            }

            int ti = 0;
            for (int ix = 0; ix < columns; ix++)
            {
                for (int iz = 0; iz < rows; iz++)
                {
                    int a = ix * (rows + 1) + iz;
                    int b = (ix + 1) * (rows + 1) + iz;
                    int c = a + 1;
                    int d = b + 1;
                    triangles[ti++] = a; triangles[ti++] = c; triangles[ti++] = b;
                    triangles[ti++] = b; triangles[ti++] = c; triangles[ti++] = d;
                }
            }

            Mesh mesh = new Mesh { name = "Greyhaven_Dock_Chart_Land" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector2 DockChartPoint(float worldX, float worldZ)
        {
            return new Vector2(worldX * 0.0225f, -1.45f + (worldZ + 112f) * 0.018f);
        }

        private static void DockChartLine(
            Transform parent,
            string name,
            Vector2 start,
            Vector2 end,
            float thickness,
            Material material)
        {
            Vector2 delta = end - start;
            if (delta.sqrMagnitude < 0.0001f) return;

            float yaw = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
            VisualCube(parent, name, new Vector3((start.x + end.x) * 0.5f, 1.075f, (start.y + end.y) * 0.5f),
                new Vector3(delta.magnitude, 0.012f, thickness), material,
                Quaternion.Euler(0f, yaw, 0f));
        }

        private static void DockChartMarker(
            Transform parent,
            string name,
            Vector2 point,
            Material material,
            float diameter)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = name;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = new Vector3(point.x, 1.095f, point.y);
            marker.transform.localScale = new Vector3(diameter, 0.025f, diameter);
            marker.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
        }

        private static void DockChartPinLabel(
            Transform parent,
            string name,
            string label,
            Vector2 point,
            Material material)
        {
            AddWorldLabel(parent, name, label,
                new Vector3(point.x + 0.12f, 1.11f, point.y + 0.07f),
                Quaternion.Euler(-90f, 0f, 0f),
                0.038f, 34, TextAnchor.MiddleLeft, TextAlignment.Left, material.color);
        }

        private static TextMesh AddWorldLabel(
            Transform parent,
            string name,
            string value,
            Vector3 localPosition,
            Quaternion localRotation,
            float characterSize,
            int fontSize,
            TextAnchor anchor,
            TextAlignment alignment,
            Color colour)
        {
            GameObject label = new GameObject(name);
            label.transform.SetParent(parent, false);
            label.transform.localPosition = localPosition;
            label.transform.localRotation = localRotation;

            TextMesh text = label.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) text.font = font;

            text.text = value;
            text.fontSize = fontSize;
            text.characterSize = characterSize;
            text.anchor = anchor;
            text.alignment = alignment;
            text.color = colour;
            text.richText = false;
            text.lineSpacing = 1f;
            text.GetComponent<MeshRenderer>().sharedMaterial = text.font != null ? text.font.material : null;
            return text;
        }

        private static GameObject VisualCube(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Quaternion? localRotation = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            if (localRotation.HasValue) go.transform.localRotation = localRotation.Value;
            go.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
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


        /// <summary>
        /// Adds deterministic, non-interactive environmental storytelling to the six streamed
        /// Greyhaven areas. Every artifact is collider-free static geometry; gameplay state is
        /// deliberately not inferred from visual prop names or appearance.
        /// </summary>
        public static void BuildEnvironmentalArtifacts(
            Transform harbour, Transform village, Transform estuary, Transform landmarks, Transform water,
            Material wood, Material metal, Material glass, Material paper, Material ink,
            Material rust, Material ceramic, Material cloth, Material rope)
        {
            if (harbour == null || village == null || estuary == null || landmarks == null || water == null)
                throw new ArgumentNullException("All Greyhaven artifact area roots are required.");
            if (wood == null || metal == null || glass == null || paper == null || ink == null ||
                rust == null || ceramic == null || cloth == null || rope == null)
                throw new ArgumentNullException("All Greyhaven artifact materials are required.");

            BuildHarbourArtifactCluster(harbour, wood, metal, glass, rust, ceramic, cloth, rope);
            BuildVillageArtifactClusters(village, wood, metal, glass, paper, ink, rust, ceramic, cloth, rope);
            BuildEstuaryArtifactClusters(estuary, wood, metal, glass, rust, ceramic, cloth, rope);
            BuildLighthouseArtifactCluster(landmarks, wood, metal, glass, rust, ceramic, cloth, rope);
            BuildWreckArtifactCluster(water, wood, metal, glass, rust, ceramic, cloth, rope);
        }

        private static void BuildHarbourArtifactCluster(
            Transform parent, Material wood, Material metal, Material glass,
            Material rust, Material ceramic, Material cloth, Material rope)
        {
            Transform cluster = Group("Harbour_Environmental_Artifacts", parent);

            Vector3 crate = new Vector3(-9.2f, 0.92f, 32.8f);
            ArtifactCube(cluster, "Salt_Split_Deck_Crate", crate, new Vector3(1.15f, 0.82f, 0.9f), wood, 11f);
            ArtifactCube(cluster, "Crate_Broken_Slat", crate + new Vector3(0.05f, 0.43f, 0.48f),
                new Vector3(0.9f, 0.08f, 0.12f), cloth, -7f);
            ArtifactCylinder(cluster, "Net_Repair_Spool", crate + new Vector3(-0.22f, 0.52f, -0.08f),
                new Vector3(0.32f, 0.12f, 0.32f), rope, Quaternion.Euler(90f, 0f, 0f));
            ArtifactCylinder(cluster, "Spool_Worn_Core", crate + new Vector3(-0.22f, 0.52f, -0.08f),
                new Vector3(0.09f, 0.14f, 0.09f), wood, Quaternion.Euler(90f, 0f, 0f));
            ArtifactSphere(cluster, "Sea_Glass_Float_Green", crate + new Vector3(0.28f, 0.49f, 0.1f),
                new Vector3(0.22f, 0.22f, 0.22f), glass);
            ArtifactSphere(cluster, "Sea_Glass_Float_Frosted", crate + new Vector3(0.43f, 0.47f, -0.18f),
                new Vector3(0.16f, 0.16f, 0.16f), ceramic);

            Vector3 lantern = new Vector3(9.5f, 0.92f, 36.4f);
            ArtifactCube(cluster, "Quay_Lantern_Base", lantern, new Vector3(0.42f, 0.16f, 0.42f), rust, 8f);
            ArtifactCube(cluster, "Quay_Lantern_Glass", lantern + Vector3.up * 0.25f,
                new Vector3(0.24f, 0.34f, 0.24f), glass);
            ArtifactCube(cluster, "Quay_Lantern_Cap", lantern + Vector3.up * 0.47f,
                new Vector3(0.38f, 0.08f, 0.38f), rust, -4f);
            ArtifactCylinder(cluster, "Mooring_Rope_Coil_Outer", new Vector3(10.4f, 0.54f, 30.6f),
                new Vector3(0.72f, 0.12f, 0.72f), rope, Quaternion.Euler(90f, 0f, 0f));
            ArtifactCylinder(cluster, "Mooring_Rope_Coil_Inner", new Vector3(10.4f, 0.56f, 30.6f),
                new Vector3(0.34f, 0.13f, 0.34f), wood, Quaternion.Euler(90f, 0f, 0f));
            ArtifactCube(cluster, "Discarded_Oar_Blade", new Vector3(-11.6f, 0.72f, 37f),
                new Vector3(0.34f, 0.12f, 1.35f), wood, 24f);
            ArtifactCube(cluster, "Oar_Cracked_Shaft", new Vector3(-11.55f, 0.75f, 35.8f),
                new Vector3(0.11f, 0.1f, 1.8f), rust, 24f);
            ArtifactCube(cluster, "Pier_Fender_Rubber", new Vector3(3.9f, 0.85f, 18.2f),
                new Vector3(0.26f, 0.42f, 0.26f), rust, 12f);
            ArtifactCylinder(cluster, "Pier_Fender_Rope_Knot", new Vector3(3.9f, 1.08f, 18.2f),
                new Vector3(0.08f, 0.22f, 0.08f), rope, Quaternion.identity);
            ArtifactCube(cluster, "Unclaimed_Ceramic_Cup", new Vector3(-3.2f, 0.88f, 38.1f),
                new Vector3(0.2f, 0.22f, 0.2f), ceramic, 19f);
        }

        private static void BuildVillageArtifactClusters(
            Transform parent, Material wood, Material metal, Material glass, Material paper,
            Material ink, Material rust, Material ceramic, Material cloth, Material rope)
        {
            Transform root = Group("Village_Environmental_Artifacts", parent);
            Transform market = Group("Market_Backlane_Artifacts", root);
            float marketGround = HeightAt(-35f, -35f);
            ArtifactCube(market, "Fish_Crate_Stack_Lower", new Vector3(-35f, marketGround + 0.42f, -35f),
                new Vector3(1.1f, 0.82f, 0.9f), wood, 7f);
            ArtifactCube(market, "Fish_Crate_Stack_Upper", new Vector3(-34.88f, marketGround + 1.18f, -35.08f),
                new Vector3(0.88f, 0.62f, 0.78f), wood, -8f);
            ArtifactCube(market, "Fish_Crate_Slatted_Lid", new Vector3(-34.88f, marketGround + 1.5f, -35.08f),
                new Vector3(0.92f, 0.06f, 0.82f), cloth, -8f);
            ArtifactCylinder(market, "Market_Brine_Jar", new Vector3(-33.8f, marketGround + 0.42f, -35.3f),
                new Vector3(0.24f, 0.38f, 0.24f), glass, Quaternion.identity);
            ArtifactCube(market, "Market_Tally_Slate", new Vector3(-36.1f, marketGround + 0.68f, -34.2f),
                new Vector3(0.55f, 0.05f, 0.38f), ink, 13f);
            ArtifactCube(market, "Tally_Slate_Chalk_Dust", new Vector3(-36.1f, marketGround + 0.714f, -34.2f),
                new Vector3(0.28f, 0.008f, 0.025f), paper, 13f);

            Transform repair = Group("Repair_Shed_Scrap_Artifacts", root);
            float repairGround = HeightAt(9f, -48f);
            ArtifactCylinder(repair, "Retired_Propeller_Hub", new Vector3(9f, repairGround + 0.22f, -48f),
                new Vector3(0.32f, 0.22f, 0.32f), rust, Quaternion.identity);
            ArtifactCube(repair, "Retired_Propeller_Blade_A", new Vector3(9f, repairGround + 0.24f, -48f),
                new Vector3(0.12f, 0.07f, 1.45f), metal, 0f);
            ArtifactCube(repair, "Retired_Propeller_Blade_B", new Vector3(9f, repairGround + 0.24f, -48f),
                new Vector3(1.25f, 0.07f, 0.12f), metal, 0f);
            ArtifactCube(repair, "Radio_Casing_Without_Dial", new Vector3(11.2f, repairGround + 0.42f, -47.4f),
                new Vector3(0.72f, 0.62f, 0.48f), rust, -9f);
            ArtifactCube(repair, "Radio_Dark_Glass", new Vector3(11.2f, repairGround + 0.45f, -47.14f),
                new Vector3(0.44f, 0.34f, 0.025f), glass, -9f);
            ArtifactCylinder(repair, "Loose_Bearing_Race", new Vector3(8.1f, repairGround + 0.12f, -46.4f),
                new Vector3(0.48f, 0.06f, 0.48f), metal, Quaternion.Euler(90f, 0f, 0f));
            ArtifactCube(repair, "Oil_Stained_Canvas", new Vector3(10.1f, repairGround + 0.08f, -49.2f),
                new Vector3(0.85f, 0.035f, 0.62f), cloth, 17f);

            Transform netShed = Group("Net_Mender_Yard_Artifacts", root);
            float netGround = HeightAt(-55f, -53f);
            ArtifactCube(netShed, "Net_Weights_Crate", new Vector3(-55f, netGround + 0.32f, -53f),
                new Vector3(0.88f, 0.62f, 0.72f), wood, -12f);
            for (int i = 0; i < 4; i++)
            {
                ArtifactCylinder(netShed, "Spare_Net_Weight_" + i,
                    new Vector3(-55.3f + i * 0.2f, netGround + 0.68f, -53f),
                    new Vector3(0.12f, 0.09f, 0.12f), rust, Quaternion.identity);
            }
            ArtifactCube(netShed, "Old_Net_Float_Rope", new Vector3(-53.7f, netGround + 0.14f, -52.2f),
                new Vector3(1.6f, 0.06f, 0.06f), rope, -16f);
            ArtifactSphere(netShed, "Net_Float_Amber", new Vector3(-54.1f, netGround + 0.28f, -52.2f),
                new Vector3(0.22f, 0.22f, 0.22f), ceramic);
            ArtifactSphere(netShed, "Net_Float_Faded", new Vector3(-53.4f, netGround + 0.28f, -52.2f),
                new Vector3(0.2f, 0.2f, 0.2f), cloth);

            Transform inn = Group("Inn_Threshold_Artifacts", root);
            float innGround = HeightAt(45f, -59f);
            ArtifactCylinder(inn, "Rain_Barrel", new Vector3(45f, innGround + 0.48f, -59f),
                new Vector3(0.72f, 0.48f, 0.72f), wood, Quaternion.identity);
            ArtifactCube(inn, "Barrel_Band_Upper", new Vector3(45f, innGround + 0.78f, -59f),
                new Vector3(0.76f, 0.06f, 0.76f), rust);
            ArtifactCube(inn, "Barrel_Band_Lower", new Vector3(45f, innGround + 0.28f, -59f),
                new Vector3(0.76f, 0.06f, 0.76f), rust);
            ArtifactCube(inn, "Rain_Basin", new Vector3(46.2f, innGround + 0.1f, -58.6f),
                new Vector3(0.62f, 0.12f, 0.48f), ceramic, 8f);
            ArtifactCube(inn, "Folded_Oilskin_Coat", new Vector3(43.6f, innGround + 0.16f, -58.4f),
                new Vector3(0.8f, 0.16f, 0.55f), cloth, -11f);
        }

        private static void BuildEstuaryArtifactClusters(
            Transform parent, Material wood, Material metal, Material glass,
            Material rust, Material ceramic, Material cloth, Material rope)
        {
            Transform root = Group("Estuary_Environmental_Artifacts", parent);
            Transform east = Group("Eastern_Mudflat_Debris_Artifacts", root);
            float eastGround = HeightAt(68f, -27f);
            ArtifactCube(east, "Tidal_Fish_Trap_Frame_Base", new Vector3(68f, eastGround + 0.16f, -27f),
                new Vector3(1.5f, 0.12f, 1.05f), wood, 13f);
            for (int i = 0; i < 5; i++)
            {
                ArtifactCube(east, "Fish_Trap_Rib_" + i, new Vector3(67.45f + i * 0.28f, eastGround + 0.45f, -27f),
                    new Vector3(0.055f, 0.55f, 0.055f), rope, 13f);
            }
            ArtifactCube(east, "Fish_Trap_Broken_Top", new Vector3(68f, eastGround + 0.76f, -27.1f),
                new Vector3(1.35f, 0.07f, 0.08f), wood, 13f);
            ArtifactSphere(east, "Mudflat_Glass_Float", new Vector3(69.1f, eastGround + 0.2f, -26.5f),
                new Vector3(0.24f, 0.24f, 0.24f), glass);
            ArtifactCube(east, "Mudflat_Driftwood", new Vector3(66.2f, eastGround + 0.16f, -28.4f),
                new Vector3(2.2f, 0.18f, 0.22f), wood, 31f);
            ArtifactCube(east, "Mudflat_Driftwood_Splinter", new Vector3(65.9f, eastGround + 0.22f, -28.6f),
                new Vector3(0.08f, 0.18f, 0.9f), rust, 38f);
            ArtifactCube(east, "Salt_Bottle_Remnant", new Vector3(70.2f, eastGround + 0.12f, -27.8f),
                new Vector3(0.12f, 0.3f, 0.12f), ceramic, 16f);

            Transform west = Group("Western_Mudflat_Debris_Artifacts", root);
            float westGround = HeightAt(-76f, -27f);
            ArtifactCube(west, "Old_Tide_Stake", new Vector3(-76f, westGround + 0.7f, -27f),
                new Vector3(0.12f, 1.4f, 0.12f), wood, -8f);
            ArtifactCube(west, "Tide_Stake_Broken_Cap", new Vector3(-76.08f, westGround + 1.36f, -27f),
                new Vector3(0.22f, 0.12f, 0.22f), rust, -8f);
            ArtifactCube(west, "Half_Buried_Boot", new Vector3(-74.4f, westGround + 0.1f, -28f),
                new Vector3(0.42f, 0.2f, 0.22f), cloth, 22f);
            ArtifactSphere(west, "Mudflat_Ceramic_Shard_A", new Vector3(-75.3f, westGround + 0.06f, -26.2f),
                new Vector3(0.2f, 0.035f, 0.14f), ceramic);
            ArtifactSphere(west, "Mudflat_Ceramic_Shard_B", new Vector3(-75.0f, westGround + 0.05f, -26.35f),
                new Vector3(0.12f, 0.03f, 0.1f), ceramic);
            ArtifactCylinder(west, "Weathered_Rope_Bundle", new Vector3(-73.8f, westGround + 0.08f, -27.2f),
                new Vector3(0.48f, 0.12f, 0.48f), rope, Quaternion.Euler(90f, 0f, 0f));
        }

        private static void BuildLighthouseArtifactCluster(
            Transform parent, Material wood, Material metal, Material glass,
            Material rust, Material ceramic, Material cloth, Material rope)
        {
            Transform cluster = Group("Lighthouse_Keepers_Abandoned_Artifacts", parent);
            float ground = HeightAt(-80f, -27f);
            ArtifactCube(cluster, "Keeper_Lantern_Foot", new Vector3(-80f, ground + 0.12f, -27f),
                new Vector3(0.46f, 0.18f, 0.42f), rust, 7f);
            ArtifactCube(cluster, "Keeper_Lantern_Glass", new Vector3(-80f, ground + 0.37f, -27f),
                new Vector3(0.28f, 0.34f, 0.25f), glass, 7f);
            ArtifactCube(cluster, "Keeper_Lantern_Cap", new Vector3(-80f, ground + 0.58f, -27f),
                new Vector3(0.4f, 0.08f, 0.38f), metal, 7f);
            ArtifactCube(cluster, "Storm_Shutter_Panel", new Vector3(-78.4f, ground + 0.24f, -28.2f),
                new Vector3(0.95f, 0.08f, 0.62f), wood, 29f);
            ArtifactCube(cluster, "Shutter_Rusted_Hinge", new Vector3(-78.5f, ground + 0.31f, -28.2f),
                new Vector3(0.08f, 0.26f, 0.08f), rust, 29f);
            ArtifactCube(cluster, "Signal_Mirror_Backplate", new Vector3(-81.4f, ground + 0.18f, -25.4f),
                new Vector3(0.46f, 0.08f, 0.36f), rust, -13f);
            ArtifactCube(cluster, "Signal_Mirror_Cracked_Face", new Vector3(-81.4f, ground + 0.23f, -25.4f),
                new Vector3(0.38f, 0.025f, 0.29f), glass, -13f);
            ArtifactCube(cluster, "Keeper_Log_Case", new Vector3(-82.4f, ground + 0.16f, -27.6f),
                new Vector3(0.56f, 0.12f, 0.4f), ceramic, 18f);
            ArtifactCube(cluster, "Log_Case_Leather_Straps", new Vector3(-82.4f, ground + 0.23f, -27.6f),
                new Vector3(0.62f, 0.025f, 0.08f), cloth, 18f);
            ArtifactCylinder(cluster, "Keeper_Rope_Ring", new Vector3(-79.1f, ground + 0.08f, -25.2f),
                new Vector3(0.58f, 0.1f, 0.58f), rope, Quaternion.Euler(90f, 0f, 0f));
        }

        private static void BuildWreckArtifactCluster(
            Transform parent, Material wood, Material metal, Material glass,
            Material rust, Material ceramic, Material cloth, Material rope)
        {
            Transform wreck = parent.Find("Greyhaven_Outer_Channel_Wreck_Field");
            if (wreck == null) throw new InvalidOperationException("Greyhaven wreck field must exist before its artifact dressing is built.");
            Transform cluster = Group("Wreck_Environmental_Artifacts", wreck);

            ArtifactCylinder(cluster, "Wreck_Bell_Rim", new Vector3(-1.35f, -0.62f, 0.55f),
                new Vector3(0.52f, 0.16f, 0.52f), rust, Quaternion.identity);
            ArtifactCylinder(cluster, "Wreck_Bell_Neck", new Vector3(-1.35f, -0.39f, 0.55f),
                new Vector3(0.18f, 0.22f, 0.18f), rust, Quaternion.identity);
            ArtifactSphere(cluster, "Wreck_Bell_Clapper", new Vector3(-1.35f, -0.82f, 0.55f),
                new Vector3(0.12f, 0.12f, 0.12f), metal);
            ArtifactCube(cluster, "Wreck_Ceramic_Plate_Fragment", new Vector3(1.72f, -0.68f, 1.18f),
                new Vector3(0.48f, 0.06f, 0.32f), ceramic, 31f);
            ArtifactCube(cluster, "Wreck_Personal_Case", new Vector3(0.55f, -0.78f, -0.82f),
                new Vector3(0.72f, 0.24f, 0.42f), wood, -16f);
            ArtifactCube(cluster, "Wreck_Case_Metal_Binding", new Vector3(0.55f, -0.64f, -0.82f),
                new Vector3(0.76f, 0.045f, 0.08f), rust, -16f);
            ArtifactSphere(cluster, "Wreck_Frosted_Glass_Shard", new Vector3(2.2f, -0.72f, -0.6f),
                new Vector3(0.24f, 0.08f, 0.12f), glass);
            ArtifactCube(cluster, "Wreck_Salt_Stiff_Canvas", new Vector3(-2.1f, -0.68f, -0.45f),
                new Vector3(0.8f, 0.06f, 0.48f), cloth, 22f);
            ArtifactCylinder(cluster, "Wreck_Rope_Coil", new Vector3(-2.3f, -0.82f, 0.9f),
                new Vector3(0.54f, 0.12f, 0.54f), rope, Quaternion.Euler(90f, 0f, 0f));
            ArtifactCube(cluster, "Wreck_Deck_Handrail_Stub", new Vector3(0.15f, -0.28f, 1.28f),
                new Vector3(0.12f, 0.62f, 0.12f), rust, 12f);
        }

        private static GameObject ArtifactCube(
            Transform parent, string name, Vector3 position, Vector3 scale, Material material, float yaw = 0f)
        {
            return VisualCube(parent, name, position, scale, material, Quaternion.Euler(0f, yaw, 0f));
        }

        private static GameObject ArtifactCylinder(
            Transform parent, string name, Vector3 position, Vector3 scale, Material material, Quaternion rotation)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.transform.localRotation = rotation;
            go.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject ArtifactSphere(
            Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static void BuildBoat(Transform parent, InputActionAsset controls, List<CargoItemDefinition> cargoCatalog, MaritimeSurfaceBehaviour seaSurface)
        {
            GameObject boat = new GameObject("Player_Boat_Blockout");
            boat.transform.SetParent(parent, false);
            boat.transform.position = new Vector3(10f, -0.1f, 14f);

            Rigidbody body = boat.AddComponent<Rigidbody>();
            body.mass = 1200f;
            body.useGravity = true;
            body.constraints = RigidbodyConstraints.None;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearDamping = 0.04f;
            body.angularDamping = 0.18f;
            body.centerOfMass = new Vector3(0f, -0.10f, -0.12f);

            // One stable waterline proxy owns collision; the shaped visual hull is decorative.
            BoxCollider collisionHull = boat.AddComponent<BoxCollider>();
            collisionHull.center = new Vector3(0f, 0.04f, 0f);
            collisionHull.size = new Vector3(2.1f, 0.8f, 5.6f);

            BoatVisualModelBuilder.Build(boat.transform);

            BoatPrototypeController controller = boat.AddComponent<BoatPrototypeController>();
            controller.InputActions = controls;
            boat.AddComponent<BoatMotorEffects>();
            BoatHydrodynamics hydrodynamics = boat.GetComponent<BoatHydrodynamics>();
            hydrodynamics.Configure(seaSurface);
            SpatialCargoHold cargoHold = boat.AddComponent<SpatialCargoHold>();
            cargoHold.SetItemDefinitions(cargoCatalog);
            cargoHold.TryPlace("GEAR-ENGINE-01", 0, 0, 0, 1, 1f, 1f, out _);
            cargoHold.TryPlace("GEAR-ROD-01", 3, 0, 0, 1, 1f, 1f, out _);
            cargoHold.TryPlace("GEAR-LAMP-01", 5, 0, 0, 1, 1f, 1f, out _);
        }

        private static void BuildCameraAndLight(
            Transform parent,
            Transform playerRoot,
            WhereTheSoundingsEnd.World.MaritimeSurfaceBehaviour seaSurface)
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
            follow.SeaSurface = seaSurface;

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