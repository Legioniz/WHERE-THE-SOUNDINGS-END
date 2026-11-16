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
        public const string PhantomPrefabPath = "Assets/_Project/Prefabs/Runtime/Panic_Hallucination_Shadow.prefab";
        public const string BirdSwarmPrefabPath = "Assets/_Project/Prefabs/Runtime/Panic_Bird_Swarm.prefab";
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
            RegisterAddressable(PhantomPrefabPath, PhantomAddress);
            RegisterAddressable(BirdSwarmPrefabPath, BirdSwarmAddress);
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
