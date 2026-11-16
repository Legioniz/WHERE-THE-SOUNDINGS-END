using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// Creates deterministic reusable transient prefabs and registers them in the default
    /// local Addressables group. Generated assets are reproducible from a clean checkout.
    /// </summary>
    public static class AddressableRuntimeContentBuilder
    {
        public const string PhantomAddress = "runtime.panic.shadow";
        public const string BirdSwarmAddress = "runtime.panic.bird_swarm";
        public const string FishSchoolAddress = "runtime.world.fish_school";
        public const string FloatingDebrisAddress = "runtime.world.floating_debris";
        public const string SalvageVisualAddress = "runtime.world.salvage_visual";
        public const string WreckDetailAddress = "runtime.world.wreck_detail";
        public const string PhantomPrefabPath = "Assets/_Project/Prefabs/Runtime/Panic_Hallucination_Shadow.prefab";
        public const string BirdSwarmPrefabPath = "Assets/_Project/Prefabs/Runtime/Panic_Bird_Swarm.prefab";
        public const string FishSchoolPrefabPath = "Assets/_Project/Prefabs/Runtime/World_Fish_School.prefab";
        public const string FloatingDebrisPrefabPath = "Assets/_Project/Prefabs/Runtime/World_Floating_Debris.prefab";
        public const string SalvageVisualPrefabPath = "Assets/_Project/Prefabs/Runtime/World_Salvage_Visual.prefab";
        public const string WreckDetailPrefabPath = "Assets/_Project/Prefabs/Runtime/World_Wreck_Detail.prefab";
        private const string MaterialPath = "Assets/_Project/Art/Materials/Runtime_Phantom_Charcoal.mat";

        public static void EnsureRuntimePrefabs()
        {
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Prefabs/Runtime");
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder("Assets/_Project/Art/Materials");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader == null)
                    throw new System.InvalidOperationException("No supported shader is available to build runtime phantom prefabs.");
                material = new Material(shader);
                material.name = "Runtime_Phantom_Charcoal";
                material.color = new Color(0.015f, 0.025f, 0.03f, 0.7f);
                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
                material.renderQueue = 3000;
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            EnsurePhantomPrefab(material);
            EnsureBirdSwarmPrefab(material);
            Material fishMaterial = EnsureMaterial("Runtime_World_Fish.mat", new Color(0.28f, 0.39f, 0.42f));
            Material debrisMaterial = EnsureMaterial("Runtime_World_Debris.mat", new Color(0.29f, 0.22f, 0.15f));
            Material salvageMaterial = EnsureMaterial("Runtime_World_Salvage.mat", new Color(0.31f, 0.25f, 0.18f));
            Material wreckMaterial = EnsureMaterial("Runtime_World_Wreck.mat", new Color(0.19f, 0.16f, 0.13f));
            EnsureFishSchoolPrefab(fishMaterial);
            EnsureFloatingDebrisPrefab(debrisMaterial);
            EnsureSalvageVisualPrefab(salvageMaterial);
            EnsureWreckDetailPrefab(wreckMaterial);
            RegisterAddressable(PhantomPrefabPath, PhantomAddress);
            RegisterAddressable(BirdSwarmPrefabPath, BirdSwarmAddress);
            RegisterAddressable(FishSchoolPrefabPath, FishSchoolAddress);
            RegisterAddressable(FloatingDebrisPrefabPath, FloatingDebrisAddress);
            RegisterAddressable(SalvageVisualPrefabPath, SalvageVisualAddress);
            RegisterAddressable(WreckDetailPrefabPath, WreckDetailAddress);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsurePhantomPrefab(Material material)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PhantomPrefabPath) != null) return;
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "Panic_Hallucination_Shadow";
            root.transform.localScale = new Vector3(22f, 1.8f, 46f);
            Collider collider = root.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            root.GetComponent<Renderer>().sharedMaterial = material;
            root.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, PhantomPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void EnsureBirdSwarmPrefab(Material material)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BirdSwarmPrefabPath) != null) return;
            GameObject root = new GameObject("Panic_Bird_Swarm");
            root.AddComponent<PanicBirdSwarmVisual>();
            for (int i = 0; i < 7; i++)
            {
                GameObject bird = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bird.name = "Phantom_Seabird_" + (i + 1).ToString("00");
                bird.transform.SetParent(root.transform, false);
                bird.transform.localPosition = new Vector3(-4f + i * 1.3f, (i % 3) * 0.65f, (i % 2 == 0 ? -2f : 2f));
                bird.transform.localScale = new Vector3(0.45f, 0.18f, 0.8f);
                Collider collider = bird.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
                bird.GetComponent<Renderer>().sharedMaterial = material;
            }
            root.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, BirdSwarmPrefabPath);
            Object.DestroyImmediate(root);
        }


        private static Material EnsureMaterial(string fileName, Color color)
        {
            string path = "Assets/_Project/Art/Materials/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
                throw new System.InvalidOperationException("No supported shader is available for runtime world visuals.");
            material = new Material(shader) { name = Path.GetFileNameWithoutExtension(fileName), color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject CreateVisualRoot(string name)
        {
            return new GameObject(name);
        }

        private static void AddPrimitive(Transform parent, PrimitiveType type, string name,
            Vector3 position, Vector3 scale, Material material, Vector3? euler = null)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            if (euler.HasValue) part.transform.localRotation = Quaternion.Euler(euler.Value);
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }

        private static void SaveInactivePrefab(GameObject root, string path)
        {
            root.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void EnsureFishSchoolPrefab(Material material)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(FishSchoolPrefabPath) != null) return;
            GameObject root = CreateVisualRoot("World_Fish_School");
            for (int i = 0; i < 14; i++)
            {
                float angle = i * 2.39996323f;
                float radius = 0.7f + (i % 4) * 0.32f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(i * 1.7f) * 0.32f,
                    Mathf.Sin(angle) * radius);
                AddPrimitive(root.transform, PrimitiveType.Capsule, "School_Fish_" + (i + 1).ToString("00"),
                    position, new Vector3(0.22f, 0.11f, 0.58f), material,
                    new Vector3(0f, -angle * Mathf.Rad2Deg, 90f));
            }
            SaveInactivePrefab(root, FishSchoolPrefabPath);
        }

        private static void EnsureFloatingDebrisPrefab(Material material)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(FloatingDebrisPrefabPath) != null) return;
            GameObject root = CreateVisualRoot("World_Floating_Debris");
            AddPrimitive(root.transform, PrimitiveType.Cube, "Driftwood_Main",
                Vector3.zero, new Vector3(2.8f, 0.18f, 0.32f), material, new Vector3(5f, 0f, 11f));
            AddPrimitive(root.transform, PrimitiveType.Cube, "Driftwood_Splinter",
                new Vector3(0.7f, 0.16f, 0.22f), new Vector3(1.1f, 0.12f, 0.18f), material,
                new Vector3(0f, 0f, -18f));
            AddPrimitive(root.transform, PrimitiveType.Cylinder, "Rope_Float",
                new Vector3(-0.8f, 0.12f, -0.28f), new Vector3(0.12f, 0.18f, 0.12f), material);
            SaveInactivePrefab(root, FloatingDebrisPrefabPath);
        }

        private static void EnsureSalvageVisualPrefab(Material material)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SalvageVisualPrefabPath) != null) return;
            GameObject root = CreateVisualRoot("World_Salvage_Visual");
            AddPrimitive(root.transform, PrimitiveType.Cube, "Waterlogged_Crate",
                new Vector3(0f, 0.38f, 0f), new Vector3(1.15f, 0.76f, 0.9f), material);
            AddPrimitive(root.transform, PrimitiveType.Cylinder, "Recovered_Metal_Canister",
                new Vector3(0.82f, 0.34f, 0.18f), new Vector3(0.24f, 0.36f, 0.24f), material,
                new Vector3(0f, 0f, 90f));
            AddPrimitive(root.transform, PrimitiveType.Cube, "Loose_Salvage_Plate",
                new Vector3(-0.55f, 0.12f, 0.48f), new Vector3(0.8f, 0.08f, 0.5f), material,
                new Vector3(9f, 17f, 4f));
            SaveInactivePrefab(root, SalvageVisualPrefabPath);
        }

        private static void EnsureWreckDetailPrefab(Material material)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(WreckDetailPrefabPath) != null) return;
            GameObject root = CreateVisualRoot("World_Wreck_Detail");
            AddPrimitive(root.transform, PrimitiveType.Cylinder, "Broken_Hull_Rib_Left",
                new Vector3(-0.65f, 0.15f, 0f), new Vector3(0.12f, 1.15f, 0.12f), material,
                new Vector3(0f, 0f, 78f));
            AddPrimitive(root.transform, PrimitiveType.Cylinder, "Broken_Hull_Rib_Right",
                new Vector3(0.65f, 0.12f, 0.05f), new Vector3(0.12f, 1.05f, 0.12f), material,
                new Vector3(0f, 0f, -72f));
            AddPrimitive(root.transform, PrimitiveType.Cube, "Sheared_Crossmember",
                new Vector3(0f, 0.48f, 0f), new Vector3(1.9f, 0.14f, 0.16f), material,
                new Vector3(0f, 0f, 7f));
            SaveInactivePrefab(root, WreckDetailPrefabPath);
        }

        private static void RegisterAddressable(string assetPath, string address)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null || settings.DefaultGroup == null)
                throw new System.InvalidOperationException("Addressables default group is unavailable.");

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                throw new System.InvalidOperationException("Generated runtime prefab has no asset GUID: " + assetPath);

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            if (entry == null)
                throw new System.InvalidOperationException("Could not register runtime prefab as Addressable: " + assetPath);
            entry.address = address;
            EditorUtility.SetDirty(settings);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string leaf = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
                throw new IOException("Invalid generated asset folder: " + path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
