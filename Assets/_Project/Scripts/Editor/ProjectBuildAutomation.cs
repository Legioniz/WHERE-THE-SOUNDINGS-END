using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// Deterministic player-build entry point for CI and local release verification.
    /// Generated scenes are prepared before the Unity Windows Development Player is compiled.
    /// </summary>
    public static class ProjectBuildAutomation
    {
        private const string OutputDirectory = "Builds/Windows";
        private const string OutputPath = OutputDirectory + "/WHERE THE SOUNDINGS END.exe";

        private static readonly string[] RequiredScenes =
        {
            "Assets/_Project/Scenes/Greyhaven_Blockout.unity",
            "Assets/_Project/Scenes/Greyhaven_Terrain.unity",
            "Assets/_Project/Scenes/Greyhaven_Harbour.unity",
            "Assets/_Project/Scenes/Greyhaven_Village.unity",
            "Assets/_Project/Scenes/Greyhaven_Estuary_West.unity",
            "Assets/_Project/Scenes/Greyhaven_Estuary_East.unity",
            "Assets/_Project/Scenes/Greyhaven_Lighthouse.unity",
            "Assets/_Project/Scenes/OuterShoals_SurveySandbox.unity",
            "Assets/_Project/Scenes/HollowTrench_SurveySandbox.unity"
        };

        public static void BuildWindowsDevelopment()
        {
            ProjectFoundationSetup.InitializeForAutomation();
            ProjectReferenceIntegrityValidator.ValidateOrThrow();
            BuildAddressableContent();

            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            foreach (string requiredScene in RequiredScenes)
            {
                if (!enabledScenes.Contains(requiredScene))
                    throw new InvalidOperationException(
                        "Required scene is not enabled in Build Settings: " + requiredScene);
            }

            foreach (string scenePath in enabledScenes)
            {
                if (!File.Exists(scenePath))
                    throw new FileNotFoundException(
                        "An enabled Build Settings scene is missing from the generated project.",
                        scenePath);
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Standalone,
                    BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("Unity could not switch to the Windows 64-bit build target.");

            Directory.CreateDirectory(OutputDirectory);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = enabledScenes,
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(
                    "Windows Development Build failed: " + report.summary.result +
                    " (errors: " + report.summary.totalErrors + ").");

            Debug.Log(
                "Windows Development Build succeeded: " + OutputPath +
                " | Size: " + report.summary.totalSize + " bytes" +
                " | Duration: " + report.summary.totalTime);

        private static void BuildAddressableContent()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                throw new InvalidOperationException(
                    "Addressables settings are missing. Run Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation.");

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (result == null || !string.IsNullOrEmpty(result.Error))
                throw new InvalidOperationException(
                    "Addressables content build failed: " +
                    (result != null ? result.Error : "Unity returned no build result."));

            Debug.Log("Addressables content build succeeded. Addressable prefab content is ready for the Windows Player.");
        }
        }
    }
}
