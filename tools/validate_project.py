#!/usr/bin/env python3
"""Static integrity checks for the Unity scaffold; this is not a Unity build."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPECTED_EDITOR = "6000.3.28f1"
EXPECTED_PACKAGES = {
    "com.unity.addressables": "2.10.3",
    "com.unity.inputsystem": "1.20.1",
    "com.unity.render-pipelines.universal": "17.3.0",
    "com.unity.test-framework": "1.4.6",
    "dev.yarnspinner.unity": "https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git#v3.2.8",
}

REQUIRED_FILES = [
    "ProjectSettings/ProjectVersion.txt",
    "Packages/manifest.json",
    "Assets/_Project/Settings/Input/PlayerControls.inputactions",
    "Assets/_Project/Scripts/Runtime/WhereTheSoundingsEnd.Runtime.asmdef",
    "Assets/_Project/Scripts/Runtime/Interaction/DocksideHubController.cs",
    "Assets/_Project/Scripts/Runtime/Core/WorldClock.cs",
    "Assets/_Project/Scripts/Runtime/Core/ActionDrivenClockPolicy.cs",
    "Assets/_Project/Scripts/Runtime/Core/Pcg32Random.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveGameData.cs",
    "Assets/_Project/Scripts/Runtime/Economy/EconomyPolicy.cs",
    "Assets/_Project/Scripts/Runtime/Economy/SeafoodMarketPolicy.cs",
    "Assets/_Project/Scripts/Runtime/Economy/CatchLegalityPolicy.cs",
    "Assets/_Project/Scripts/Runtime/Economy/EconomyWalletController.cs",
    "Assets/_Project/Scripts/Runtime/Economy/MaraFishMarketController.cs",
    "Assets/_Project/Scripts/Runtime/Economy/VesselFuelController.cs",
    "Assets/_Project/Scripts/Runtime/Economy/GreyhavenVendorServices.cs",
    "Assets/_Project/Scripts/Runtime/Economy/HarbourmasterContractController.cs",
    "Assets/_Project/Scripts/Runtime/Economy/HarbourmasterResearchController.cs",
    "Assets/_Project/Scripts/Runtime/Economy/VesselEquipmentController.cs",
    "Assets/_Project/Scripts/Runtime/Economy/ExpeditionEconomyTelemetry.cs",
    "Assets/_Project/Tests/EditMode/HarbourmasterContractSaveTests.cs",
    "Assets/_Project/Tests/EditMode/HarbourmasterResearchSaveTests.cs",
    "Assets/_Project/Tests/EditMode/VesselEquipmentIntegrationTests.cs",
    "Assets/_Project/Tests/EditMode/ExpeditionEconomyTelemetryTests.cs",
    "Assets/_Project/Tests/EditMode/EconomyPolicyTests.cs",
    "Assets/_Project/Tests/EditMode/MaraMarketTransactionTests.cs",
    "Assets/_Project/Tests/EditMode/SeafoodMarketPolicyTests.cs",
    "Assets/_Project/Tests/EditMode/CatchLegalityPolicyTests.cs",
    "Assets/_Project/Tests/EditMode/CatchLegalityCargoSaveTests.cs",
    "Assets/_Project/Tests/EditMode/EconomyRuntimeIntegrationTests.cs",
    "docs/13_PRODUCTION_MANAGEMENT/ECONOMY-INTEGRATION-AUDIT.md",
    "docs/07_GAME_RULES_AND_BALANCING/ECONOMY-BALANCING.md",
    "docs/07_GAME_RULES_AND_BALANCING/SEAFOOD-SELLING-PRICE-SCHEDULE.md",
    "docs/04_CHARACTERS_AND_ENTITIES/CATCHABLE-SEAFOOD-CATALOG.md",
    "docs/08_TECHNICAL_DESIGN/ECONOMY-AND-TRADING-SYSTEM.md",
    "docs/08_TECHNICAL_DESIGN/SAVE-DATA-STRUCTURE.md",
    "docs/13_PRODUCTION_MANAGEMENT/TASK-BACKLOG.md",
    "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenFrontEndUI.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/CinematicSequencePolicy.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/FieldDiscoveryPolicy.cs",
    "Assets/_Project/Tests/EditMode/FieldDiscoveryPolicyTests.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/FieldJournalPolicy.cs",
    "Assets/_Project/Tests/EditMode/FieldJournalPolicyTests.cs",
    "Assets/_Project/Tests/EditMode/OpeningCinematicPolicyTests.cs",
    "docs/03_STORY_AND_NARRATIVE/OPENING-CINEMATICS.md",
    "docs/03_STORY_AND_NARRATIVE/FIELD-DISCOVERY-CINEMATICS.md",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveFileService.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveGameCoordinator.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveRestoreTransaction.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/AutosaveController.cs",
    "Assets/_Project/Scripts/Runtime/Inventory/CargoItemDefinition.cs",
    "Assets/_Project/Scripts/Runtime/Inventory/CargoGridGeometry.cs",
    "Assets/_Project/Scripts/Runtime/Inventory/SpatialCargoHold.cs",
    "Assets/_Project/Scripts/Runtime/World/SeaStateDirector.cs",
    "Assets/_Project/Scripts/Runtime/World/UnexplainedWarningPolicy.cs",
    "Assets/_Project/Scripts/Runtime/Streaming/AreaStreamingDirector.cs",
    "Assets/_Project/Scripts/Runtime/Streaming/AddressableAssetResidencyService.cs",
    "Assets/_Project/Scripts/Runtime/Streaming/AddressablePrefabPool.cs",
    "Assets/_Project/Tests/EditMode/AddressablePoolPolicyTests.cs",
    "Assets/_Project/Tests/EditMode/AddressableRuntimeContentBuilderTests.cs",
    "Assets/_Project/Tests/PlayMode/AddressablePrefabPoolPlayModeTests.cs",
    "Assets/_Project/Scripts/Runtime/Streaming/AreaStreamingPolicy.cs",
    "Assets/_Project/Scripts/Runtime/Streaming/StreamedAreaSceneRoot.cs",
    "Assets/_Project/Tests/EditMode/AreaStreamingPolicyTests.cs",
    "Assets/_Project/Tests/EditMode/DocksideAccessPolicyTests.cs",
    "Assets/_Project/Tests/PlayMode/WhereTheSoundingsEnd.Tests.PlayMode.asmdef",
    "Assets/_Project/Tests/PlayMode/GreyhavenAreaStreamingPlayModeTests.cs",
    "docs/08_TECHNICAL_DESIGN/AREA-STREAMING-AND-ASSET-RESIDENCY.md",
    "docs/08_TECHNICAL_DESIGN/GREYHAVEN-DOCKSIDE-INTERACTION-HUB.md",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanSurveyMath.cs",
    "Assets/_Project/Scripts/Runtime/World/RegionalChartPolicy.cs",
    "Assets/_Project/Scripts/Runtime/World/RegionalChartController.cs",
    "Assets/_Project/Tests/EditMode/RegionalChartPolicyTests.cs",
    "Assets/_Project/Tests/EditMode/RegionalChartSaveTests.cs",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanSurveyRegion.cs",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanPhenomenonSite.cs",
    "Assets/_Project/Scripts/Runtime/World/HistoricalArtifact.cs",
    "Assets/_Project/Scripts/Runtime/World/HistoricalArtifact.cs.meta",
    "docs/05_WORLD_AND_LEVEL_DESIGN/HISTORICAL-ARTIFACTS-AND-ARCHIVAL-EVIDENCE.md",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanPhenomenonRules.cs",
    "Assets/_Project/Scripts/Runtime/World/AcousticEventBus.cs",
    "Assets/_Project/Scripts/Runtime/World/AcousticWorldListener.cs",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanSiteInteractionController.cs",
    "Assets/_Project/Scripts/Runtime/World/SoundingRigController.cs",
    "Assets/_Project/Scripts/Runtime/World/VesselConditionController.cs",
    "Assets/_Project/Scripts/Runtime/World/HarvestingMinigameController.cs",
    "Assets/_Project/Scripts/Runtime/World/FishingSystemDirector.cs",
    "Assets/_Project/Scripts/Runtime/World/FishingSimulationPolicy.cs",
    "Assets/_Project/Tests/EditMode/FishingSimulationPolicyTests.cs",
    "docs/08_TECHNICAL_DESIGN/FISHING-SIMULATION.md",
    "Assets/_Project/Scripts/Runtime/World/SalvageSite.cs",
    "Assets/_Project/Scripts/Runtime/World/PanicBirdSwarmVisual.cs",
    "Assets/_Project/Tests/EditMode/SeaStateAndCargoDamageTests.cs",
    "Assets/_Project/Tests/EditMode/BoatPhysicsTests.cs",
    "Assets/_Project/Tests/EditMode/MaritimeSurfaceMathTests.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/YarnDialogueSaveParticipant.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs",
    "Assets/_Project/Scripts/Runtime/NPC/NpcAgent.cs",
    "Assets/_Project/Tests/EditMode/NpcScheduleTests.cs",
    "docs/12_AI_AND_SIMULATION/GREYHAVEN-NPC-SYSTEM.md",
    "Assets/_Project/Dialogue/Greyhaven.yarnproject",
    "Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn",
    "Assets/_Project/Tests/EditMode/CargoGridGeometryTests.cs",
    "docs/06_GAMEPLAY_SYSTEMS/ACTION-DRIVEN-SEA-STATE.md",
    "Assets/_Project/Scripts/Runtime/Boat/BoatPrototypeController.cs",
    "Assets/_Project/Scripts/Runtime/Boat/BoatMotorTelemetryMath.cs",
    "Assets/_Project/Scripts/Runtime/Boat/BoatMotorEffects.cs",
    "Assets/_Project/Scripts/Runtime/Boat/BoatHydrodynamics.cs",
    "Assets/_Project/Scripts/Runtime/Camera/CameraFollow2_5D.cs",
    "Assets/_Project/Scripts/Editor/WhereTheSoundingsEnd.Editor.asmdef",
    "Assets/_Project/Scripts/Editor/ProjectFoundationSetup.cs",
    "Assets/_Project/Scripts/Editor/ProjectBuildAutomation.cs",
    "Assets/_Project/Scripts/Editor/ProjectReferenceIntegrityValidator.cs",
    "Assets/_Project/Scripts/Editor/AddressableRuntimeContentBuilder.cs",
    ".github/workflows/unity-verification.yml",
    "docs/14_TESTING_AND_QUALITY/UNITY-PLAYABLE-VALIDATION.md",
    "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs",
    "Assets/_Project/Scripts/Editor/GreyhavenMaritimeAssetBuilder.cs",
    "Assets/_Project/Tests/EditMode/GreyhavenEnvironmentalArtifactTests.cs",
    "Assets/_Project/Scripts/Editor/DeepOceanSurveySandboxBuilder.cs",
    "Assets/_Project/Scripts/Editor/BoatVisualModelBuilder.cs",
    "Assets/_Project/Tests/EditMode/BoatVisualModelTests.cs",
    "Assets/_Project/Tests/EditMode/BoatMotorTelemetryTests.cs",
    "Assets/_Project/Tests/EditMode/WhereTheSoundingsEnd.Tests.EditMode.asmdef",
    "Assets/_Project/Tests/EditMode/CoreFoundationTests.cs",
    "Assets/_Project/Tests/EditMode/SaveFileServiceTests.cs",
    "docs/05_WORLD_AND_LEVEL_DESIGN/GREYHAVEN.md",
    "docs/06_GAMEPLAY_SYSTEMS/DEEP-OCEAN-PHENOMENA-AND-MARINER-EXPERIENCES.md",
    "docs/07_GAME_RULES_AND_BALANCING/SOUNDING-RIG-ALGORITHMS.md",
    "docs/08_TECHNICAL_DESIGN/DEEP-OCEAN-SURVEY-IMPLEMENTATION.md",
]


def load_json(relative_path: str) -> dict:
    path = ROOT / relative_path
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as error:
        raise ValueError(f"{relative_path}: {error}") from error


def main() -> int:
    failures: list[str] = []
    for relative_path in REQUIRED_FILES:
        if not (ROOT / relative_path).is_file():
            failures.append(f"Missing required file: {relative_path}")

    version_path = ROOT / "ProjectSettings/ProjectVersion.txt"
    if version_path.exists():
        version_text = version_path.read_text(encoding="utf-8")
        if f"m_EditorVersion: {EXPECTED_EDITOR}" not in version_text:
            failures.append(f"Project editor version is not pinned to {EXPECTED_EDITOR}.")

    try:
        manifest = load_json("Packages/manifest.json")
        dependencies = manifest.get("dependencies", {})
        for package, expected in EXPECTED_PACKAGES.items():
            actual = dependencies.get(package)
            if actual != expected:
                failures.append(f"{package}: expected {expected}, found {actual!r}")

        # Validate assembly names and references as a graph, not merely as existing files.
        assembly_specs = {
            "Assets/_Project/Scripts/Runtime/WhereTheSoundingsEnd.Runtime.asmdef": {
                "name": "WhereTheSoundingsEnd.Runtime",
                "references": {"Unity.InputSystem", "YarnSpinner.Unity", "Unity.Addressables", "Unity.ResourceManager"},
            },
            "Assets/_Project/Scripts/Editor/WhereTheSoundingsEnd.Editor.asmdef": {
                "name": "WhereTheSoundingsEnd.Editor",
                "references": {
                    "WhereTheSoundingsEnd.Runtime",
                    "Unity.RenderPipelines.Universal.Runtime",
                    "Unity.InputSystem",
                    "YarnSpinner.Unity",
                    "Unity.Addressables.Editor",
                },
            },
            "Assets/_Project/Tests/EditMode/WhereTheSoundingsEnd.Tests.EditMode.asmdef": {
                "name": "WhereTheSoundingsEnd.Tests.EditMode",
                "references": {"WhereTheSoundingsEnd.Runtime", "WhereTheSoundingsEnd.Editor"},
            },
            "Assets/_Project/Tests/PlayMode/WhereTheSoundingsEnd.Tests.PlayMode.asmdef": {
                "name": "WhereTheSoundingsEnd.Tests.PlayMode",
                "references": {"WhereTheSoundingsEnd.Runtime"},
            },
        }
        for assembly_path, expected in assembly_specs.items():
            assembly = load_json(assembly_path)
            if assembly.get("name") != expected["name"]:
                failures.append(f"{assembly_path}: assembly name does not match its expected identity.")
            actual_references = set(assembly.get("references", []))
            missing_references = expected["references"] - actual_references
            if missing_references:
                failures.append(
                    f"{assembly_path}: missing assembly references: {', '.join(sorted(missing_references))}"
                )
            if not assembly.get("autoReferenced", False) and "Tests." not in expected["name"]:
                failures.append(f"{assembly_path}: production assembly must remain auto-referenced.")

        yarn_project = load_json("Assets/_Project/Dialogue/Greyhaven.yarnproject")
        yarn_sources = yarn_project.get("sourceFiles", [])
        if not yarn_sources or not any(
            list((ROOT / "Assets/_Project/Dialogue").glob(pattern))
            for pattern in yarn_sources
        ):
            failures.append("Greyhaven.yarnproject sourceFiles pattern does not resolve to any Yarn source files.")

        reference_validator = (
            ROOT / "Assets/_Project/Scripts/Editor/ProjectReferenceIntegrityValidator.cs"
        ).read_text(encoding="utf-8")
        for required_text in (
            "ValidateOrThrow", "ValidateBuildSettings", "ValidatePersistentGameplayGraph",
            "GetMonoBehavioursWithMissingScriptCount", "GetDependencies",
            "GreyhavenEnvironmentBuilder.HasGeneratedAreaScenes",
            "CameraFollow2_5D target is not the generated player vessel",
            "AddressableAssetResidencyService", "AddressablePrefabPool",
            "ValidateAddressableRuntimeContent", "FindAssetEntry",
        ):
            if required_text not in reference_validator:
                failures.append(f"Unity reference-integrity preflight is missing: {required_text}")

        build_automation = (
            ROOT / "Assets/_Project/Scripts/Editor/ProjectBuildAutomation.cs"
        ).read_text(encoding="utf-8")
        if "ProjectReferenceIntegrityValidator.ValidateOrThrow();" not in build_automation:
            failures.append("Windows build automation must run the Unity reference-integrity preflight.")

        actions = load_json("Assets/_Project/Settings/Input/PlayerControls.inputactions")
        maps = {item["name"]: item for item in actions.get("maps", [])}
        boat_map = maps.get("Boat")
        if boat_map is None:
            failures.append("PlayerControls.inputactions has no Boat action map.")
        else:
            action_names = {action["name"] for action in boat_map.get("actions", [])}
            expected_actions = {"Move", "Interact", "Fish", "Dredge", "SoundingPulse", "Pause", "Inventory", "HarvestBeat"}
            if not expected_actions.issubset(action_names):
                failures.append("Boat action map is missing one or more required actions.")
            harvest_bindings = [
                binding.get("path")
                for binding in boat_map.get("bindings", [])
                if binding.get("action") == "HarvestBeat"
            ]
            for required_binding in (
                "<Keyboard>/space", "<Keyboard>/enter",
                "<Gamepad>/buttonEast",
            ):
                if required_binding not in harvest_bindings:
                    failures.append(f"HarvestBeat is missing {required_binding}.")

            identifiers = [
                item.get("id")
                for item in boat_map.get("actions", []) + boat_map.get("bindings", [])
            ]
            if len(identifiers) != len(set(identifiers)):
                failures.append("Input action/binding identifiers must be unique.")

        save_source = (ROOT / "Assets/_Project/Scripts/Runtime/Persistence/SaveGameData.cs").read_text(encoding="utf-8")
        if "CurrentSchemaVersion = 9" not in save_source:
            failures.append("SaveGameData schema must be version 9 after historical artifact inspection persistence.")

        if "case 4:" not in save_source or "data.schemaVersion = 5;" not in save_source:
            failures.append("SaveGameData is missing the schema 4 to 5 atmosphere-state migration.")
        if "case 5:" not in save_source or "data.schemaVersion = 6;" not in save_source:
            failures.append("SaveGameData is missing the schema 5 to 6 NPC relationship-memory migration.")
        if "case 6:" not in save_source or "data.schemaVersion = 7;" not in save_source or "visitedRegionIds" not in save_source:
            failures.append("SaveGameData is missing the schema 6 to 7 regional chart discovery migration.")
        if "case 7:" not in save_source or "data.schemaVersion = 8;" not in save_source or "walletCents" not in save_source:
            failures.append("SaveGameData is missing the schema 7 to 8 economy wallet migration.")
        if "case 8:" not in save_source or "data.schemaVersion = 9;" not in save_source or "inspectedHistoricalArtifactIds" not in save_source:
            failures.append("SaveGameData is missing the schema 8 to 9 historical artifact inspection migration.")

        for required_text in (
            "weatherRemainingGameSeconds", "weatherShiftCooldownGameSeconds",
            "hallucinationCooldownGameSeconds", "birdEventPending",
            "atmosphereIncidentCountdownGameSeconds", "inspectedPhenomenonSiteIds",
            "inspectedHistoricalArtifactIds", "heardAcousticEventIds",
        ):
            if required_text not in save_source:
                failures.append(f"WorldSaveData is missing persistent state: {required_text}")

        warning_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/FishermanAtmosphereDirector.cs").read_text(encoding="utf-8")
        for required_text in ("UnexplainedWarningPolicy.ShouldTrigger", "TriggerUnexplainedWarning", "DrawUnexplainedIndicator"):
            if required_text not in warning_source:
                failures.append(f"FishermanAtmosphereDirector is missing unexplained warning behavior: {required_text}")
        warning_policy_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/UnexplainedWarningPolicy.cs").read_text(encoding="utf-8")
        if "Probability = 0.055f" not in warning_policy_source or "RemapIncidentRoll" not in warning_policy_source:
            failures.append("UnexplainedWarningPolicy is missing its rare trigger band or incident remapping.")
        warning_tests = (ROOT / "Assets/_Project/Tests/EditMode/SeaStateAndCargoDamageTests.cs").read_text(encoding="utf-8")
        if "UnexplainedWarningPolicy_UsesRareBoundedTriggerBand" not in warning_tests or "UnexplainedWarningPolicy_RemapsRemainingIncidentRange" not in warning_tests:
            failures.append("Unexplained warning policy regression tests are missing.")

        survey_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/SoundingRigController.cs").read_text(encoding="utf-8")
        for required_text in ("SoundingPulse", "discoveredPhenomenonSiteIds", "evaluatedSurveyOpportunityIds", "AcousticEventBus.Publish", "IsIndependentLayerObservation", "CalculateSiteSignalConfidence"):
            if required_text not in survey_source:
                failures.append(f"SoundingRigController is missing expected survey feature: {required_text}")

        chart_policy_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/RegionalChartPolicy.cs").read_text(encoding="utf-8")
        for required_text in ("IsRegionKnown", "ShouldRevealSite", "TryRecordVisit"):
            if required_text not in chart_policy_source:
                failures.append(f"RegionalChartPolicy is missing discovery rule: {required_text}")

        chart_controller_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/RegionalChartController.cs").read_text(encoding="utf-8")
        for required_text in ("visitedRegionIds", "unlockedRegionIds", "GetKnownSitesForCurrentRegion", "ISaveParticipant", "ISaveDataValidator", "TryRecordVisit"):
            if required_text not in chart_controller_source:
                failures.append(f"RegionalChartController is missing chart feature: {required_text}")
        if "RegionalChartController" not in (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8"):
            failures.append("Greyhaven builder does not attach regional chart discovery to the persistent vessel.")
        if "RegionalChartController" not in (ROOT / "Assets/_Project/Scripts/Editor/DeepOceanSurveySandboxBuilder.cs").read_text(encoding="utf-8"):
            failures.append("Deep-ocean survey sandboxes do not attach regional chart discovery.")
        front_end_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenFrontEndUI.cs").read_text(encoding="utf-8")
        if "GameplayUI.IsChartOpen" not in front_end_source:
            failures.append("GreyhavenFrontEndUI must reject pause opening over the regional chart.")

        chart_ui_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs").read_text(encoding="utf-8")
        for required_text in ("BuildChartPanel", "ToggleChart", "NO MARKS  /  NO BEARINGS", "GetKnownSitesForCurrentRegion"):
            if required_text not in chart_ui_source:
                failures.append(f"GreyhavenRuntimeUI is missing fog-of-war chart presentation: {required_text}")

        site_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/DeepOceanPhenomenonSite.cs").read_text(encoding="utf-8")
        if "DeepOceanPhenomenonRules.IsAllowed" not in site_source:
            failures.append("DeepOceanPhenomenonSite must enforce DeepOceanPhenomenonRules.")

        rules_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/DeepOceanPhenomenonRules.cs").read_text(encoding="utf-8")
        for required_type in ("BrinePool", "HydrothermalVent", "ColdSeep", "GasHydrate", "WhaleFall", "WaterMassFront"):
            if required_type not in rules_source:
                failures.append(f"DeepOceanPhenomenonRules is missing eligibility rule: {required_type}")

        region_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/DeepOceanSurveyRegion.cs").read_text(encoding="utf-8")
        for required_region in ("greyhaven", "the_outer_shoals", "the_hollow_trench"):
            if required_region not in region_source:
                failures.append(f"DeepOceanSurveyRegion is missing regional profile: {required_region}")

        streaming_source = (ROOT / "Assets/_Project/Scripts/Runtime/Streaming/AreaStreamingDirector.cs").read_text(encoding="utf-8")
        for required_text in (
            "LoadSceneAsync(scenePath, LoadSceneMode.Additive)",
            "Application.CanStreamedLevelBeLoaded",
            "AreaStreamingPolicy.ShouldPreload",
            "AreaStreamingPolicy.ShouldUnload",
            "UnloadGraceSeconds",
            "StreamedAreaSceneRoot",
            "RefreshSceneTargets",
            "roots.Length != 1",
            "Profiler.GetTotalAllocatedMemoryLong",
            "Profiler.GetTotalReservedMemoryLong",
            "Profiler.GetRuntimeMemorySizeLong",
            "LogAreaLoadMetrics", "LogAreaUnloadCompleteMetrics",
            "state.PendingLoad = true", "RequestArea(string areaId)", "ReleaseArea(string areaId)",
            "RetryFailedArea(string areaId)", "AreaStreamingPolicy.ShouldStandby",
            "EnterStandby", "IsAreaInStandby", "Resources.UnloadUnusedAssets",
            "assetCleanupQuietSeconds", "assetCleanupCooldownSeconds", "assetCleanupRoutine != null",
            "addressableResidency.HasPendingOperations",
        ):
            if required_text not in streaming_source:
                failures.append(f"AreaStreamingDirector is missing streaming safeguard: {required_text}")

        addressable_builder = (ROOT / "Assets/_Project/Scripts/Editor/AddressableRuntimeContentBuilder.cs").read_text(encoding="utf-8")
        for required_text in (
            "EnsureRuntimePrefabs", "runtime.panic.shadow", "runtime.panic.bird_swarm",
            "CreateOrMoveEntry", "SaveAsPrefabAsset",
        ):
            if required_text not in addressable_builder:
                failures.append(f"AddressableRuntimeContentBuilder is missing generated content step: {required_text}")

        build_automation_source = (ROOT / "Assets/_Project/Scripts/Editor/ProjectBuildAutomation.cs").read_text(encoding="utf-8")
        if "AddressableAssetSettings.BuildPlayerContent" not in build_automation_source or "BuildAddressablesContentForVerification" not in build_automation_source:
            failures.append("Player and runtime-test workflows must build Addressables content before execution.")
        if "BuildAddressablesContentForVerification" not in (ROOT / ".github/workflows/unity-verification.yml").read_text(encoding="utf-8"):
            failures.append("Unity verification workflow must build Addressables content before Play Mode tests.")
        setup_source = (ROOT / "Assets/_Project/Scripts/Editor/ProjectFoundationSetup.cs").read_text(encoding="utf-8")
        if "AddressableRuntimeContentBuilder.EnsureRuntimePrefabs();" not in setup_source:
            failures.append("Project foundation setup must generate and register runtime Addressables prefabs.")

        sea_state_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/SeaStateDirector.cs").read_text(encoding="utf-8")
        for required_text in (
            "runtime.panic.shadow", "runtime.panic.bird_swarm",
            "addressablePrefabPool.WarmPool", "addressablePrefabPool.TryRent", "EnsureTransientPool",
        ):
            if required_text not in sea_state_source:
                failures.append(f"SeaStateDirector is missing Addressable transient prefab integration: {required_text}")

        residency_source = (ROOT / "Assets/_Project/Scripts/Runtime/Streaming/AddressableAssetResidencyService.cs").read_text(encoding="utf-8")
        for required_text in (
            "Addressables.LoadAssetAsync<GameObject>", "Addressables.Release(entry.Handle)",
            "LeaseCount", "HasPendingOperations", "ReleaseUnusedNow",
            "PrefabLease", "ShouldReleaseUnused",
        ):
            if required_text not in residency_source:
                failures.append(f"AddressableAssetResidencyService is missing residency safeguard: {required_text}")

        pool_source = (ROOT / "Assets/_Project/Scripts/Runtime/Streaming/AddressablePrefabPool.cs").read_text(encoding="utf-8")
        for required_text in (
            "WarmPool", "TryRent", "Return(GameObject instance)", "ClearPool",
            "state.Maximum", "IPooledObject", "state.Lease.Dispose()",
            "idlePoolReleaseSeconds", "ShouldReleaseUnused",
        ):
            if required_text not in pool_source:
                failures.append(f"AddressablePrefabPool is missing pooling safeguard: {required_text}")

        generated_prefab_tests = (ROOT / "Assets/_Project/Tests/EditMode/AddressableRuntimeContentBuilderTests.cs").read_text(encoding="utf-8")
        if "EnsureRuntimePrefabs_CreatesInactivePrefabsForAddressablePooling" not in generated_prefab_tests:
            failures.append("AddressableRuntimeContentBuilderTests is missing generated-prefab verification.")

        pool_playmode_tests = (ROOT / "Assets/_Project/Tests/PlayMode/AddressablePrefabPoolPlayModeTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "WarmPool_RentsReturnsAndEnforcesCapacityWithoutDuplicateRentals",
            "runtime.panic.shadow", "pool.ClearPool", "Is.SameAs(first)",
        ):
            if required_test not in pool_playmode_tests:
                failures.append(f"AddressablePrefabPoolPlayModeTests is missing lifecycle check: {required_test}")

        pool_tests = (ROOT / "Assets/_Project/Tests/EditMode/AddressablePoolPolicyTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "NormalizePrewarmCount_ClampsToSafePoolCapacity",
            "ShouldReleaseUnused_RequiresZeroReferencesAndElapsedGrace",
            "ShouldReleaseUnused_RejectsNonFiniteTimingValues",
        ):
            if required_test not in pool_tests:
                failures.append(f"AddressablePoolPolicyTests is missing regression: {required_test}")

        sea_state_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/SeaStateDirector.cs").read_text(encoding="utf-8")
        for required_text in (
            "pooledPhantom", "pooledBirdSwarm", "UpdateTransientVisualPools",
            "phantomRemaining", "birdSwarmRemaining",
        ):
            if required_text not in sea_state_source:
                failures.append(f"SeaStateDirector is missing pooled hallucination visual behavior: {required_text}")

        area_root_source = (ROOT / "Assets/_Project/Scripts/Runtime/Streaming/StreamedAreaSceneRoot.cs").read_text(encoding="utf-8")
        for required_text in (
            "contentRevision", "TryValidate", "GetComponentsInChildren<Renderer>",
            "sharedMesh == null", "material.shader == null", "shader.isSupported",
            "subMeshCount", "Rigidbody", "MatchesIdentityAndRevision",
        ):
            if required_text not in area_root_source:
                failures.append(f"StreamedAreaSceneRoot is missing asset integrity check: {required_text}")

        area_policy_tests = (ROOT / "Assets/_Project/Tests/EditMode/AreaStreamingPolicyTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "ValidRadii_RequireUnloadRadiusBeyondPreloadRadius",
            "ShouldPreload_UsesInclusiveInnerBoundary",
            "ShouldUnload_UsesOuterBoundaryAndPreservesHysteresisBand",
            "DistanceRules_RejectNonFiniteValues",
            "ShouldStandby_OnlyOutsidePreloadRadiusAndRejectsInvalidDistances",
            "StreamedSceneIdentityAndRevision_AcceptsExactCurrentContent",
            "StreamedSceneIdentityAndRevision_RejectsStaleRevisionOrWrongArea",
        ):
            if f"void {required_test}()" not in area_policy_tests:
                failures.append(f"AreaStreamingPolicyTests is missing regression: {required_test}")

        streaming_playmode = (ROOT / "Assets/_Project/Tests/PlayMode/GreyhavenAreaStreamingPlayModeTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "AdditiveAreasPreloadAndRetireWhilePersistentGameplaySceneStaysLoaded",
            "ExplicitAreaRequestsWakeStandbyScenesAndReleaseReturnsThemToDistancePolicy",
            "RequestArea(\"greyhaven.lighthouse\")", "ReleaseArea(\"greyhaven.lighthouse\")",
            "IsAreaInStandby(\"greyhaven.lighthouse\")",
            "IsAreaLoaded(\"greyhaven.lighthouse\")",
            "FailedAreaCount",
        ):
            if required_test not in streaming_playmode:
                failures.append(f"GreyhavenAreaStreamingPlayModeTests is missing transition check: {required_test}")

        greyhaven_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
        for required_text in (
            "BuildAreaStreaming(world.transform",
            "Greyhaven_Terrain.unity", "Greyhaven_Harbour.unity",
            "Greyhaven_Village.unity", "Greyhaven_Estuary_West.unity",
            "Greyhaven_Estuary_East.unity", "Greyhaven_Lighthouse.unity",
            "IsStaticStreamableHierarchy", "SaveGeneratedMeshAsset",
            "BuildHarbourmasterQuayFurniture", "BuildResidentFurniture",
            "Mara_Venn_Market_Workstation", "Tomas_Vale_Repair_Workbench",
            "Inez_Sarmiento_Net_Mending_Workspace", "Harbourmaster_Quay_Log_Station",
            "Sea_Glass_Collection_Shelf", "Radio_Experiment_Casing", "Knot_Sample_Board",
            "BuildEnvironmentalArtifacts", "BuildHarbourArtifactCluster",
            "BuildVillageArtifactClusters", "BuildEstuaryArtifactClusters",
            "BuildAbandonedLowerLandingRemains", "BuildLowerSettlementFoundationRemains",
            "BuildLighthouseArtifactCluster", "BuildWreckArtifactCluster",
            "Wreck_Keel_Spine_Fragment", "Wreck_Broken_Rib_Port_A",
            "BuildHistoricalArtifacts", "Historical_Artifact_Inspection_Anchors",
            "StreamedAreaRevision = \"5\"",
            "StreamedAreaIds", "HasGeneratedAreaScenes(out string error)",
            "EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive)",
            "areaRoot.TryValidate(StreamedAreaIds[index], StreamedAreaRevision",
        ):
            if required_text not in greyhaven_builder:
                failures.append(f"Greyhaven area builder is missing: {required_text}")

        maritime_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenMaritimeAssetBuilder.cs").read_text(encoding="utf-8")
        for required_text in (
            "BuildBollard", "BuildLobsterPot", "BuildMangrove", "BuildReedClump",
            "BuildWeatherStation", "BuildNetRack", "WearPatch", "RemoveCollider",
            "Estuary_Ecological_And_Shoreline_Assets", "Village_Working_Life_Assets",
            "Harbour_Bespoke_Maritime_Assets", "Lighthouse_Headland_Maritime_Assets",
        ):
            if required_text not in maritime_builder:
                failures.append(f"GreyhavenMaritimeAssetBuilder is missing art feature: {required_text}")

        artifact_tests = (ROOT / "Assets/_Project/Tests/EditMode/GreyhavenEnvironmentalArtifactTests.cs").read_text(encoding="utf-8")
        for required_text in (
            "BuildEnvironmentalArtifacts_CreatesDistrictDressingWithoutGameplayComponents",
            "BuildHistoricalArtifacts_CreatesSixTypedAnchorsAndStaticEvidenceClusters",
            "Historical_Artifact_Inspection_Anchors", "Keeper_Logbook_Cover", "Recovered_Recorder_Housing",
            "EstuaryRemains_AreSplitAcrossStreamingOwnershipRoots",
            "Lower_Inlet_Landing_Remains", "Old_Lower_Settlement_Foundation_Remains",
            "Build_BespokeMaritimePassCreatesDistinctRegionalAssetsWithoutGameplayComponents",
            "rendererCount, Is.GreaterThanOrEqualTo(250)",
            "GetComponentsInChildren<Collider>", "GetComponentsInChildren<MonoBehaviour>",
            "rendererCount, Is.GreaterThanOrEqualTo(60)",
        ):
            if required_text not in artifact_tests:
                failures.append(f"GreyhavenEnvironmentalArtifactTests is missing coverage: {required_text}")

        dockside_source = (ROOT / "Assets/_Project/Scripts/Runtime/Interaction/DocksideHubController.cs").read_text(encoding="utf-8")
        for required_text in ("RELEASE FLAGGED CATCHES", "line.destinationFactor", "line.contractOnlySale", "BuildBalanceReport"):
            if required_text not in dockside_source:
                failures.append(f"DocksideHubController is missing seafood/telemetry presentation: {required_text}")
        for required_text in (
            "DocksideAccessPolicy.IsAccessible",
            "MaximumDockingSpeed = 0.85f",
            "TryClaimInteraction",
            "Time.timeScale = 0f",
            "RequestCheckpointAutosave",
            "BuildCatchInspection",
            "BuildVesselInspection",
            "DocksideModalPolicy.CanOpenDocksideMenu",
            "pauseAction.WasPressedThisFrame()",
            "private void OnDisable()",
            "DefaultExecutionOrder(-100)",
            "ReadHistoricalArtifact",
            "HistoricalArtifactId",
            "ResolveHistoricalArtifact",
        ):
            if required_text not in dockside_source:
                failures.append(f"DocksideHubController is missing expected interaction feature: {required_text}")

        archive_option_tests = (ROOT / "Assets/_Project/Tests/EditMode/SeaStateAndCargoDamageTests.cs").read_text(encoding="utf-8")
        if "DocksideArchiveOption_RetainsStableHistoricalArtifactId" not in archive_option_tests:
            failures.append("Historical artifact archive menu option regression test is missing.")

        dockside_tests = (ROOT / "Assets/_Project/Tests/EditMode/DocksideAccessPolicyTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "StationaryVesselInsideShoreApproachCanOpenLocation",
            "VesselOutsideHorizontalApproachCannotOpenLocation",
            "MovingVesselCannotOpenLocationEvenWhenNearby",
            "VerticalSeparationDoesNotChangeShoreApproachDistance",
            "InvalidOrNonFiniteInputsAreRejected",
            "DocksideMenuCannotOpenOverInventoryDialogueGameplayActionOrFrontEnd",
            "InventoryCannotOpenOverDocksideOrFrontEndOrDialogue",
            "PauseMenuCannotOpenOverDocksideInventoryOrDialogue",
        ):
            if f"void {required_test}()" not in dockside_tests:
                failures.append(f"DocksideAccessPolicyTests is missing regression: {required_test}")

        dockside_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
        for required_text in (
            "BuildDocksideHub(world.transform",
            "Dockside_Interaction_Hub",
            "Main Dock", "Harbour Office", "Fish Market",
            "Boat Repair Shed", "Net Mender Shed", "Harbour Inn",
            "Western Lighthouse", "Greyhaven_Harbourmaster",
            "ExternallyManagedByDocksideHub = true",
        ):
            if required_text not in dockside_builder:
                failures.append(f"Greyhaven builder is missing dockside integration: {required_text}")

        resident_interaction = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs").read_text(encoding="utf-8")
        if "externallyManagedByDocksideHub" not in resident_interaction:
            failures.append("HarbourmasterInteraction must support external dockside hub ownership.")

        runtime_ui_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs").read_text(encoding="utf-8")
        for required_text in (
            "DocksideModalPolicy.CanOpenInventory",
            "docksideHub.IsMenuOpen",
            "frontEnd.IsOverlayOpen",
            "public bool IsDialogueOpen",
        ):
            if required_text not in runtime_ui_source:
                failures.append(f"GreyhavenRuntimeUI is missing modal exclusion: {required_text}")

        front_end_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenFrontEndUI.cs").read_text(encoding="utf-8")
        for required_text in (
            "DocksideModalPolicy.CanOpenPauseMenu",
            "GameplayUI.IsInventoryOpen",
            "GameplayUI.IsDialogueOpen",
            "public bool IsOverlayOpen",
        ):
            if required_text not in front_end_source:
                failures.append(f"GreyhavenFrontEndUI is missing modal exclusion: {required_text}")

        resident_interaction = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs").read_text(encoding="utf-8")
        if "if (externallyManagedByDocksideHub || controls == null) return;" not in resident_interaction:
            failures.append("Externally managed NPC triggers must not compete for the shared Interact action.")

        setup_source = (ROOT / "Assets/_Project/Scripts/Editor/ProjectFoundationSetup.cs").read_text(encoding="utf-8")
        for required_text in (
            "HasGeneratedAreaScenes(out validationError)",
            "Rebuild stale Greyhaven area scenes?",
            "Keep current scenes",
            "area-scene preflight failed",
        ):
            if required_text not in setup_source:
                failures.append(f"ProjectFoundationSetup is missing generated-scene preflight safeguard: {required_text}")

        for required_text in (
            "ProjectFirstOpenBootstrap : AssetPostprocessor",
            "OnPostprocessAllAssets(",
            "didDomainReload",
            "EditorApplication.delayCall -= BootstrapFreshCheckout",
        ):
            if required_text not in setup_source:
                failures.append(f"Fresh-checkout bootstrap is missing import-safe initialization: {required_text}")
        if "[InitializeOnLoad]" in setup_source:
            failures.append("Fresh-checkout bootstrap must not load assets from InitializeOnLoad before asset import completes.")

        area_scene_builder = (ROOT / "Assets/_Project/Scripts/Editor/DeepOceanSurveySandboxBuilder.cs").read_text(encoding="utf-8")
        if "Greyhaven_Lighthouse.unity" not in area_scene_builder:
            failures.append("Deep-ocean build-list refresh must preserve all additive Greyhaven area scenes.")

        sea_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/SeaStateDirector.cs").read_text(encoding="utf-8")
        for required_text in (
            "movementThreshold", "SetHarvesting", "panicLevel", "timeOfDaySeconds",
            "worldClock.Advance", "LastGameDeltaSeconds", "weatherShiftCooldownGameSeconds",
            "birdEventCountdownGameSeconds", "latestOmen", "RefreshSceneTargets",
            "RestoreSoundscapeBaselines",
        ):
            if required_text not in sea_source:
                failures.append(f"SeaStateDirector is missing expected runtime feature: {required_text}")

        sea_tests = (ROOT / "Assets/_Project/Tests/EditMode/SeaStateAndCargoDamageTests.cs").read_text(encoding="utf-8")
        if "SeaStateSceneRefresh_PreservesOriginalAudioBaselineAcrossRepeatedAreaTransitions" not in sea_tests:
            failures.append("SeaStateDirector is missing its streamed-scene audio baseline regression test.")

        harvest_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/HarvestingMinigameController.cs").read_text(encoding="utf-8")
        if 'FindAction("HarvestBeat"' not in harvest_source or "harvestBeatAction.WasPressedThisFrame()" not in harvest_source:
            failures.append("HarvestingMinigameController must use the shared keyboard/gamepad HarvestBeat action.")
        if "Keyboard.current" in harvest_source:
            failures.append("HarvestingMinigameController must not hard-code keyboard-only beat input.")

        acoustic_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/AcousticWorldListener.cs").read_text(encoding="utf-8")
        for required_text in ("AcousticEventBus.SignalEmitted += HandleSignal", "AcousticEventBus.SignalEmitted -= HandleSignal", "AcousticPerceptionMath.CalculateHearingScore", "SignalHeard?.Invoke", "heardEventIds"):
            if required_text not in acoustic_source:
                failures.append(f"AcousticWorldListener is missing expected event-driven behaviour: {required_text}")

        site_interaction = (ROOT / "Assets/_Project/Scripts/Runtime/World/DeepOceanSiteInteractionController.cs").read_text(encoding="utf-8")
        if 'FindAction("Interact"' not in site_interaction or "inspectedPhenomenonSiteIds" not in site_interaction:
            failures.append("DeepOceanSiteInteractionController must support input and persistent site inspection.")

        sandbox_source = (ROOT / "Assets/_Project/Scripts/Editor/DeepOceanSurveySandboxBuilder.cs").read_text(encoding="utf-8")
        for required_text in ("OuterShoals_SurveySandbox.unity", "HollowTrench_SurveySandbox.unity", "ConfigureForRegion(regionId)", "CreateSites"):
            if required_text not in sandbox_source:
                failures.append(f"Deep-ocean sandbox builder is missing: {required_text}")

        builder_source = (ROOT / "Assets/_Project/Scripts/Editor/ProjectFoundationSetup.cs").read_text(encoding="utf-8")
        if "DeepOceanSurveySandboxBuilder.EnsureScenes(LoadControls())" not in builder_source:
            failures.append("Project setup must generate the missing deep-ocean survey sandbox scenes.")

        boat_visual_source = (ROOT / "Assets/_Project/Scripts/Editor/BoatVisualModelBuilder.cs").read_text(encoding="utf-8")
        for required_text in ("BuildHullMesh", "BuildDeckMesh", "BuildCabinMesh", "SaveMeshAsset", "RemoveCollider", "BuildWheelhouse", "BuildBowRails"):
            if required_text not in boat_visual_source:
                failures.append(f"BoatVisualModelBuilder is missing its procedural workboat model feature: {required_text}")

        greyhaven_visual_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
        survey_visual_builder = (ROOT / "Assets/_Project/Scripts/Editor/DeepOceanSurveySandboxBuilder.cs").read_text(encoding="utf-8")
        if "BoatVisualModelBuilder.Build(boat.transform)" not in greyhaven_visual_builder:
            failures.append("Greyhaven must use the shared procedural workboat visual model.")
        if "BoatVisualModelBuilder.Build(boat.transform)" not in survey_visual_builder:
            failures.append("Deep-ocean survey scenes must use the shared procedural workboat visual model.")

        boat_visual_tests = (ROOT / "Assets/_Project/Tests/EditMode/BoatVisualModelTests.cs").read_text(encoding="utf-8")
        for required_test in ("WorkboatVisualModel_BuildsTaperedHullAndRaisedWheelhouse", "WorkboatVisualModel_KeepsAllDecorativePartsColliderFree"):
            if f"void {required_test}()" not in boat_visual_tests:
                failures.append(f"BoatVisualModelTests is missing regression test: {required_test}")

        boat_source = (ROOT / "Assets/_Project/Scripts/Runtime/Boat/BoatPrototypeController.cs").read_text(encoding="utf-8")
        if "if (isActiveAndEnabled) BindInputActions();" not in boat_source:
            failures.append("BoatPrototypeController must bind input assigned by the environment builder after AddComponent.")

        boat_hydro = (ROOT / "Assets/_Project/Scripts/Runtime/Boat/BoatHydrodynamics.cs").read_text(encoding="utf-8")
        for required_text in (
            "TrySampleSurface", "AddForceAtPosition", "Physics.gravity",
            "CalculateBuoyancyForce", "buoyancyOffsets",
            "ClampSubmersionDepth(depth, maximumSubmersion)",
        ):
            if required_text not in boat_hydro:
                failures.append(f"BoatHydrodynamics is missing expected water-force behaviour: {required_text}")

        boat_source = (ROOT / "Assets/_Project/Scripts/Runtime/Boat/BoatPrototypeController.cs").read_text(encoding="utf-8")
        for required_text in (
            "BoatMovementMath.CalculatePropulsionAcceleration", "body.AddForce(",
            "body.AddTorque(", "CalculateRudderAuthority", "lateralResistance",
            "quadraticForwardDrag",
        ):
            if required_text not in boat_source:
                failures.append(f"BoatPrototypeController is missing physics-driven movement: {required_text}")
        fixed_update = boat_source.split("private void FixedUpdate()", 1)[1].split("public void CaptureSaveData", 1)[0]
        if "body.linearVelocity =" in fixed_update or "body.angularVelocity =" in fixed_update:
            failures.append("Boat movement must apply forces/torques instead of setting velocity directly.")

        motor_math_source = (ROOT / "Assets/_Project/Scripts/Runtime/Boat/BoatMotorTelemetryMath.cs").read_text(encoding="utf-8")
        for required_text in (
            "CalculateSpeedRatio", "CalculateEngineLoad", "CalculateEngineRpmNormalized",
            "MinimumLoadAtLowSpeed", "float.IsNaN", "float.IsInfinity",
        ):
            if required_text not in motor_math_source:
                failures.append(f"BoatMotorTelemetryMath is missing bounded telemetry logic: {required_text}")

        motor_effects_source = (ROOT / "Assets/_Project/Scripts/Runtime/Boat/BoatMotorEffects.cs").read_text(encoding="utf-8")
        for required_text in (
            "CurrentEngineLoad", "CurrentSpeedRatio", "NormalizedEngineRpm",
            "Outboard_Propeller_Rotor", "Outboard_Engine_Cowl",
            "maximumPropWashRate", "maximumSurfaceSprayRate",
            "ParticleSystemSimulationSpace.World", "StopEmittingAndClear", "main.cullingMode",
        ):
            if required_text not in motor_effects_source:
                failures.append(f"BoatMotorEffects is missing physics-driven visual feedback: {required_text}")
        if "AddForce(" in motor_effects_source or "AddTorque(" in motor_effects_source or "linearVelocity =" in motor_effects_source:
            failures.append("BoatMotorEffects must remain presentation-only and must not modify vessel physics.")

        for builder_name in ("GreyhavenEnvironmentBuilder.cs", "DeepOceanSurveySandboxBuilder.cs"):
            builder_source = (ROOT / "Assets/_Project/Scripts/Editor" / builder_name).read_text(encoding="utf-8")
            if "boat.AddComponent<BoatMotorEffects>()" not in builder_source:
                failures.append(f"{builder_name} must attach BoatMotorEffects to its generated vessel.")

        visual_model_tests = (ROOT / "Assets/_Project/Tests/EditMode/BoatVisualModelTests.cs").read_text(encoding="utf-8")
        if 'Assert.IsNotNull(visual.Find("Outboard_Propeller_Rotor"))' not in visual_model_tests:
            failures.append("BoatVisualModelTests must verify the animated outboard propeller pivot.")

        water_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/MaritimeSurfaceBehaviour.cs").read_text(encoding="utf-8")
        for required_text in (
            "TrySampleSurface", "EvaluateWaveHeight", "SampleSurface", "worldSurfaceVelocity",
            "previousAmplitudeBlend", "previousWaveDirection", "AdvancePhase",
            "BlendDirection", "EstimateVerticalVelocity", "ApplyWaveMesh();",
        ):
            if required_text not in water_source:
                failures.append(f"MaritimeSurfaceBehaviour is missing coherent shared-wave behaviour: {required_text}")
        if "Time.time * waveSpeed" in water_source:
            failures.append("MaritimeSurfaceBehaviour must not reset wave phase when an incident changes direction.")
        surface_tests = (ROOT / "Assets/_Project/Tests/EditMode/MaritimeSurfaceMathTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "SurfaceVelocity_UsesTheFixedSimulationInterval",
            "WaveRenderInterpolation_ClampsScalarBlendToSnapshotRange",
            "WaveRenderInterpolation_BendsDirectionBetweenPhysicsSnapshots",
        ):
            if f"void {required_test}()" not in surface_tests:
                failures.append(f"MaritimeSurfaceMathTests is missing regression test: {required_test}")
        for required_text in (
            "[DefaultExecutionOrder(-100)]", "private void FixedUpdate()",
            "float dt = Time.fixedDeltaTime;", "surfaceSampleDeltaTime = dt;",
            "private void LateUpdate()", "GetInterpolatedWaveState",
            "InterpolateDirection", "InterpolateScalar",
        ):
            if required_text not in water_source:
                failures.append(f"MaritimeSurfaceBehaviour is missing fixed-step/render interpolation contract: {required_text}")
        if "Time.deltaTime" in water_source:
            failures.append("MaritimeSurfaceBehaviour wave state must not advance on the render-frame clock.")
        fixed_wave_update = water_source.split("private void FixedUpdate()", 1)[1].split("private void LateUpdate()", 1)[0]
        if "Time.fixedDeltaTime" not in fixed_wave_update or "surfaceSampleDeltaTime = dt;" not in fixed_wave_update:
            failures.append("MaritimeSurfaceBehaviour must estimate surface velocity over its fixed physics step.")
        mesh_update = water_source.split("private void ApplyWaveMesh()", 1)[1].split("private void GetInterpolatedWaveState", 1)[0]
        if "GetInterpolatedWaveState" not in mesh_update:
            failures.append("The visible water mesh must interpolate between fixed wave snapshots.")

        vessel_condition = (ROOT / "Assets/_Project/Scripts/Runtime/World/VesselConditionController.cs").read_text(encoding="utf-8")
        for required_text in (
            "VesselImpactMath.CalculateNormalImpactSpeed", "pendingImpactSpeed",
            "body.GetPointVelocity(contact.point)", "otherBody.GetPointVelocity(contact.point)",
            "damagePerImpactSpeed <= 0f", "private void FixedUpdate()", "AcousticEventKind.HullImpact",
            "AcousticEventBus.Publish", "ApplyImpact(impactSpeed, contactPoint)",
        ):
            if required_text not in vessel_condition:
                failures.append(f"VesselConditionController is missing collision integration: {required_text}")
        if "body.linearVelocity *=" in vessel_condition or "body.angularVelocity *=" in vessel_condition:
            failures.append("VesselConditionController must not override Rigidbody collision-solver velocity.")

        impact_tests = (ROOT / "Assets/_Project/Tests/EditMode/SeaStateAndCargoDamageTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "CollisionImpact_UsesContactNormalAndIgnoresTangentialScraping",
            "CollisionImpact_RejectsInvalidContactNormalsAndVelocities",
            "CollisionDamage_UsesThresholdAndCapsImpactDamage",
            "CollisionDamage_RejectsInvalidSpeedsAndKeepsEngineDamageProportional",
        ):
            if f"void {required_test}" not in impact_tests:
                failures.append(f"SeaStateAndCargoDamageTests is missing regression test: {required_test}")

        greyhaven_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
        for required_text in (
            "BoxCollider collisionHull = boat.AddComponent<BoxCollider>();",
            "collisionHull.center = new Vector3(0f, 0.04f, 0f);",
            "collisionHull.size = new Vector3(2.1f, 0.8f, 5.6f);",
            "BoatVisualModelBuilder.Build(boat.transform)",
        ):
            if required_text not in greyhaven_builder:
                failures.append(f"Greyhaven must preserve one root collision proxy and use the shared visual boat model: {required_text}")

        camera_source = (ROOT / "Assets/_Project/Scripts/Runtime/Camera/CameraFollow2_5D.cs").read_text(encoding="utf-8")
        if "TrySampleSurface(" not in camera_source or "waveBobInfluence" in camera_source:
            failures.append("Camera must sample the active wave surface without adding a second wave-height bob.")

        greyhaven_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
        if ("hydrodynamics.Configure(seaSurface)" not in greyhaven_builder or
                "body.useGravity = true;" not in greyhaven_builder or
                "body.interpolation = RigidbodyInterpolation.Interpolate;" not in greyhaven_builder):
            failures.append("Greyhaven must connect buoyancy, enable gravity and interpolate the vessel Rigidbody.")

        deep_builder = (ROOT / "Assets/_Project/Scripts/Editor/DeepOceanSurveySandboxBuilder.cs").read_text(encoding="utf-8")
        if ("hydrodynamics.Configure(waterSurface)" not in deep_builder or
                "body.useGravity = true;" not in deep_builder or
                "body.interpolation = RigidbodyInterpolation.Interpolate;" not in deep_builder):
            failures.append("Deep-ocean survey boats must connect buoyancy, enable gravity and interpolate the vessel Rigidbody.")
        if "follow.SeaSurface = waterSurface;" not in deep_builder:
            failures.append("Deep-ocean survey cameras must sample the same water surface as buoyancy.")

        if "vesselCondition = GetComponent<VesselConditionController>();" not in boat_source:
            failures.append("BoatPrototypeController must resolve VesselConditionController after generated-scene component wiring.")

        atmosphere_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/FishermanAtmosphereDirector.cs").read_text(encoding="utf-8")
        if "seaState.LastGameDeltaSeconds" not in atmosphere_source or "realDelta * 72f" in atmosphere_source:
            failures.append("FishermanAtmosphereDirector must schedule incidents from the authoritative simulation clock.")

        if "worldClock.ElapsedSeconds / 86400.0" not in sea_source or "elapsedGameSeconds / 86400.0" in sea_source:
            failures.append("SeaStateDirector save capture must derive day number from WorldClock.ElapsedSeconds.")

        if "ISaveParticipant" not in atmosphere_source or "atmosphereFalseLightPosition" not in atmosphere_source:
            failures.append("FishermanAtmosphereDirector must persist its active incident presentation state.")

        yarn_project = load_json("Assets/_Project/Dialogue/Greyhaven.yarnproject")
        if yarn_project.get("projectFileVersion") != 3:
            failures.append("Greyhaven Yarn project must use projectFileVersion 3.")
        if "Source/**/*.yarn" not in yarn_project.get("sourceFiles", []):
            failures.append("Greyhaven Yarn project must include Source/**/*.yarn.")

        yarn_source = ROOT / "Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn"
        if yarn_source.exists():
            yarn_text = yarn_source.read_text(encoding="utf-8")
            for required_text in (
                "title: Greyhaven_Harbourmaster",
                "Ask about the missing boats.",
                "Ask what a normal harbour day looks like.",
                "Ask for one of his harbour sayings.",
                "Ask for work.",
                '<<checkpoint "greyhaven.sounding_buoy_accepted">>',
            ):
                if required_text not in yarn_text:
                    failures.append(f"Harbourmaster Yarn source is missing: {required_text}")

        if yarn_source.exists():
            for required_relationship_text in (
                "<<declare $npc_relationship = 50>>",
                "<<relationship ",
                "$npc_can_trade",
                "mara.misrepresented_unusual_catch",
                "tomas.dismissed_repair_advice",
                "inez.dismissed_net_evidence",
                "A clean table is cheaper than a spoiled catch.",
                "An engine tells you its troubles twice",
                "A net is only as strong as the knot nobody checks.",
                "Ask what she does after closing.",
                "Ask why he marks repaired bolts with chalk.",
                "Ask about her mother's knots.",
            ):
                if required_relationship_text not in yarn_text:
                    failures.append(f"Greyhaven Yarn relationship content is missing: {required_relationship_text}")

            npc_nodes = (
                "Greyhaven_Harbourmaster",
                "Greyhaven_Fishmonger",
                "Greyhaven_Mechanic",
                "Greyhaven_NetMender",
            )
            ally_topics = {
                "Greyhaven_Harbourmaster": "-> Ask what he has written in the log about you.",
                "Greyhaven_Fishmonger": "-> Ask to see the notes she keeps about strange catches.",
                "Greyhaven_Mechanic": "-> Ask what repair he refuses to hurry.",
                "Greyhaven_NetMender": "-> Ask what she would teach someone who wants to earn her trust.",
            }
            for npc_node in npc_nodes:
                marker = f"title: {npc_node}"
                if marker not in yarn_text:
                    failures.append(f"Greyhaven Yarn source is missing resident dialogue node: {npc_node}")
                    continue

                start = yarn_text.index(marker)
                end = yarn_text.find(chr(10) + "===", start)
                node_text = yarn_text[start:] if end < 0 else yarn_text[start:end]
                for tier_condition in (
                    "$npc_relationship < 15",
                    "$npc_relationship < 30",
                    "$npc_relationship < 45",
                    "$npc_relationship < 60",
                    "$npc_relationship < 75",
                    "$npc_relationship < 90",
                ):
                    if tier_condition not in node_text:
                        failures.append(f"{npc_node} is missing relationship-specific dialogue for tier boundary {tier_condition}.")
                if "-> Ask for one of" not in node_text:
                    failures.append(f"{npc_node} is missing a character-specific quote option.")
                if ally_topics[npc_node] not in node_text:
                    failures.append(f"{npc_node} is missing its Ally-only personal topic.")

            clue_flags = (
                "gh_clue_bell_log",
                "gh_clue_cold_catch",
                "gh_clue_hull_marks",
                "gh_clue_net_return",
            )
            for clue_flag in clue_flags:
                if f"<<declare ${clue_flag} = false>>" not in yarn_text:
                    failures.append(f"Greyhaven Yarn is missing persistent clue declaration: {clue_flag}")
                if f"<<set ${clue_flag} = true>>" not in yarn_text:
                    failures.append(f"Greyhaven Yarn is missing clue discovery state update: {clue_flag}")

            clue_topics = {
                "Greyhaven_Harbourmaster": "Ask what the lighthouse log says about the bell.",
                "Greyhaven_Fishmonger": "Ask why some catches arrive cold in the middle.",
                "Greyhaven_Mechanic": "Ask him to compare the returned hull's marks with a rock strike.",
                "Greyhaven_NetMender": "Ask what the missing crew's returned net tells her.",
            }
            for npc_node, clue_topic in clue_topics.items():
                start = yarn_text.index(f"title: {npc_node}")
                end = yarn_text.find(chr(10) + "===", start)
                node_text = yarn_text[start:] if end < 0 else yarn_text[start:end]
                if clue_topic not in node_text:
                    failures.append(f"{npc_node} is missing its occupation-specific evidence topic.")
                if "-> Compare the records from the quay." not in node_text:
                    failures.append(f"{npc_node} is missing the cross-witness evidence comparison.")

            if yarn_text.count("-> Compare the records from the quay.") != 4:
                failures.append("Greyhaven Yarn must expose exactly one cross-witness comparison in each resident node.")

        npc_source = (ROOT / "Assets/_Project/Scripts/Runtime/NPC/NpcAgent.cs").read_text(encoding="utf-8")
        for required_text in (
            "NpcScheduleResolver", "SeaStateDirector", "ISaveParticipant",
            "NpcStateSaveData", "ModifyRelationship", "TryApplyRelationshipEvent",
            "NpcRelationshipRules", "relationshipMemoryIds", "walkingSpeed",
        ):
            if required_text not in npc_source:
                failures.append(f"NpcAgent is missing expected runtime feature: {required_text}")

        npc_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
        for resident_id in (
            "greyhaven.harbourmaster", "greyhaven.mara_venn",
            "greyhaven.tomas_vale", "greyhaven.inez_sarmiento",
        ):
            if resident_id not in npc_builder:
                failures.append(f"Greyhaven scene builder is missing resident ID: {resident_id}")
        if "BuildVillageResident" not in npc_builder or "CreateNpcAnchor" not in npc_builder:
            failures.append("Greyhaven scene builder must generate scheduled resident NPCs and waypoint anchors.")
        if npc_builder.count("interaction.Agent = agent;") < 2:
            failures.append("Greyhaven scene builder must explicitly assign the generated resident NpcAgent to both interaction construction paths.")

        interaction_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs").read_text(encoding="utf-8")
        if "public string DialogueNode" not in interaction_source or "public string DisplayName" not in interaction_source:
            failures.append("Resident interaction must support per-NPC dialogue nodes and display names.")
        if "npcAgent.CanTalk" not in interaction_source or "runtimeUI.BeginDialogue(dialogueNode, npcAgent)" not in interaction_source:
            failures.append("Resident interaction must enforce per-NPC conversation trust and pass the active agent.")
        if "npcAgent = GetComponent<NpcAgent>()" not in interaction_source or "NPC DATA UNAVAILABLE" not in interaction_source:
            failures.append("Resident interaction must resolve or visibly reject a missing NpcAgent instead of starting neutral fallback dialogue.")

        ui_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs").read_text(encoding="utf-8")
        for required_text in ("ApplyRelationshipCommand", "SyncRelationshipVariables", "RefreshRelationshipMeter", "_activeNpc"):
            if required_text not in ui_source:
                failures.append(f"GreyhavenRuntimeUI is missing relationship dialogue integration: {required_text}")

        dialogue_start_index = ui_source.find("dialogueRunner.StartDialogue(")
        relationship_sync_index = ui_source.rfind("SyncRelationshipVariables();", 0, dialogue_start_index)
        if dialogue_start_index < 0 or relationship_sync_index < 0:
            failures.append("GreyhavenRuntimeUI must seed the active NPC relationship before Yarn starts the dialogue.")

        if 'runner.AddCommandHandler<string, int>("relationship", runtimeUI.ApplyRelationshipCommand)' not in npc_builder:
            failures.append("Greyhaven scene builder must register the correctly typed Yarn relationship command.")

        input_map = maps.get("Boat")
        if input_map is not None:
            action_ids = {action["name"]: action.get("id") for action in input_map.get("actions", [])}
            if action_ids.get("Inventory") != "b0a70000000000000000000000000007":
                failures.append("Inventory input action ID does not match the committed control map.")

        assembly_definitions = {}
        for path in ROOT.glob("Assets/_Project/**/*.asmdef"):
            definition = json.loads(path.read_text(encoding="utf-8"))
            assembly_name = definition.get("name")
            if not assembly_name:
                failures.append(f"Assembly definition has no name: {path.relative_to(ROOT)}")
                continue
            if assembly_name in assembly_definitions:
                failures.append(f"Duplicate assembly definition name: {assembly_name}")
            assembly_definitions[assembly_name] = (path, definition)

        external_assemblies = {
            "Unity.InputSystem",
            "Unity.RenderPipelines.Universal.Runtime",
            "YarnSpinner.Unity",
        }
        for assembly_name, (path, definition) in assembly_definitions.items():
            for reference in definition.get("references", []):
                if reference not in assembly_definitions and reference not in external_assemblies:
                    failures.append(
                        f"{path.relative_to(ROOT)} references unresolved assembly: {reference}"
                    )

        runtime_assembly = assembly_definitions.get("WhereTheSoundingsEnd.Runtime")
        if runtime_assembly is not None and runtime_assembly[1].get("includePlatforms"):
            failures.append("Runtime assembly must not be restricted to editor-only platforms.")

        editor_assembly = assembly_definitions.get("WhereTheSoundingsEnd.Editor")
        if editor_assembly is not None and "Editor" not in editor_assembly[1].get("includePlatforms", []):
            failures.append("Editor assembly must be restricted to the Editor platform.")

        test_assembly = assembly_definitions.get("WhereTheSoundingsEnd.Tests.EditMode")
        if test_assembly is not None:
            if "Editor" not in test_assembly[1].get("includePlatforms", []):
                failures.append("Edit Mode test assembly must be restricted to the Editor platform.")
            if "TestAssemblies" not in test_assembly[1].get("optionalUnityReferences", []):
                failures.append("Edit Mode test assembly must reference Unity Test Framework assemblies.")
        save_file_source = (ROOT / "Assets/_Project/Scripts/Runtime/Persistence/SaveFileService.cs").read_text(encoding="utf-8")
        for required_text in ("MaximumSaveFileBytes", "stream.Length > MaximumSaveFileBytes", "new UTF8Encoding(false, true)"):
            if required_text not in save_file_source:
                failures.append(f"SaveFileService is missing bounded, strict save-file reading: {required_text}")

        for required_text in (
            "MaximumCollectionEntries", "IsValidVector", "IsValidCargoInstanceId",
            "ValidateUniqueIds", "duplicate cargo instance",
        ):
            if required_text not in save_source:
                failures.append(f"SaveGameData validation is missing: {required_text}")

        transaction_source = (ROOT / "Assets/_Project/Scripts/Runtime/Persistence/SaveRestoreTransaction.cs").read_text(encoding="utf-8")
        for required_text in ("ISaveDataValidator", "CaptureSaveData(rollback)", "TryValidateSaveData(rollback", "RestoreSaveData(rollback)", "participants.Count - 1; index >= 0"):
            if required_text not in transaction_source:
                failures.append(f"SaveRestoreTransaction is missing atomic restore protection: {required_text}")

        cargo_source = (ROOT / "Assets/_Project/Scripts/Runtime/Inventory/SpatialCargoHold.cs").read_text(encoding="utf-8")
        for required_text in ("ISaveDataValidator", "TryValidateSaveData", "IsValidInstanceId", "HashSet<string> instanceIds"):
            if required_text not in cargo_source:
                failures.append(f"SpatialCargoHold is missing cargo identity validation: {required_text}")

        salvage_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/SalvageSite.cs").read_text(encoding="utf-8")
        for required_text in ("TryReservePull", "CommitReservedPull", "ReleaseReservedPull", "reservations"):
            if required_text not in salvage_source:
                failures.append(f"SalvageSite is missing transactional pull handling: {required_text}")

        for required_text in ("IsWithinInteractionRange", "TryReservePull", "CommitReservedPull", "ReleaseSalvageReservation"):
            if required_text not in harvest_source:
                failures.append(f"HarvestingMinigameController is missing salvage transaction guards: {required_text}")

        for required_text in ("inspectionCooldownRealSeconds", "DeepOceanInspectionPolicy", "IsCooldownReady", "TryClaimInteraction", "HistoricalArtifact", "TryRecordHistoricalArtifact", "inspectedHistoricalArtifactIds"):
            if required_text not in site_interaction:
                failures.append(f"DeepOceanSiteInteractionController is missing inspection feature: {required_text}")

        if "TryClaimInteraction" not in sea_source or "IsGameplayActionBusy" not in sea_source:
            failures.append("SeaStateDirector must coordinate exclusive gameplay action claims.")

        ui_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs").read_text(encoding="utf-8")
        for required_text in ("TryClaimInteraction", "BeginDialogue", "_inventoryOpen"):
            if required_text not in ui_source:
                failures.append(f"GreyhavenRuntimeUI is missing exclusive dialogue/inventory guards: {required_text}")

        front_end_cinematic_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenFrontEndUI.cs").read_text(encoding="utf-8")
        for required_text in (
            "BuildOpeningBeats", "BuildFirstVoyageBeats", "TryPlayFirstContact",
            "wse.openingCinematicSeen", "REPLAY OPENING RECORD", "SKIP  /  CONTINUE",
            "Time.unscaledDeltaTime", "FadeCinematicOverlay", "RestoreCinematicCamera",
            "cinematicCameraFollow.enabled = false", "Gamepad.current",
            "cinematicBeatCounter", "cinematicProgressFill", "GetVisibleCharacterCount",
            "wse.reduceCinematicMotion", "Reduce cinematic camera movement",
        ):
            if required_text not in front_end_cinematic_source:
                failures.append(f"GreyhavenFrontEndUI is missing cinematic integration: {required_text}")

        if "frontEnd.TryPlayFirstContact" not in ui_source or "BeginDialogueAfterFirstContact" not in ui_source:
            failures.append("GreyhavenRuntimeUI must run first-contact presentation before starting the existing Yarn dialogue.")

        cinematic_policy_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/CinematicSequencePolicy.cs").read_text(encoding="utf-8")
        if "TryRegisterFirstContact" not in cinematic_policy_source or "MaximumNpcIdLength" not in cinematic_policy_source:
            failures.append("CinematicSequencePolicy is missing stable-ID validation and first-contact registration.")
        if "GetVisibleCharacterCount" not in cinematic_policy_source:
            failures.append("CinematicSequencePolicy is missing the deterministic caption reveal policy.")

        cinematic_tests = (ROOT / "Assets/_Project/Tests/EditMode/OpeningCinematicPolicyTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "TryRegisterFirstContact_AcceptsStableIdOnlyOnce",
            "TryRegisterFirstContact_AcceptsDistinctResidents",
            "TryRegisterFirstContact_RejectsBlankOrUntrimmedId",
            "TryRegisterFirstContact_RejectsOversizedId",
            "TryRegisterFirstContact_RejectsNullSet",
            "GetVisibleCharacterCount_ClampsRevealProgress",
            "GetVisibleCharacterCount_HandlesInvalidTimingValues",
        ):
            if f"void {required_test}" not in cinematic_tests:
                failures.append(f"OpeningCinematicPolicyTests is missing regression: {required_test}")


        save_tests = (ROOT / "Assets/_Project/Tests/EditMode/SaveFileServiceTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "Load_RejectsOversizedPrimaryFileBeforeDeserialization",
            "Save_RejectsNonFiniteVesselPhysicsValues",
            "Save_RejectsDuplicateCargoInstanceIdentifiers",
            "Save_RejectsEmptyCargoInstanceGuid",
            "Save_RejectsOversizedCollectionsBeforeSerialization",
            "Save_RejectsMalformedQuestCollectionEntries",
            "Restore_RollsBackEveryParticipantWhenALaterParticipantThrows",
            "Restore_RejectsAnUnsafeRollbackSnapshotBeforeMutatingParticipants",
        ):
            if f"void {required_test}()" not in save_tests:
                failures.append(f"SaveFileServiceTests is missing regression test: {required_test}")

        for required_test in (
            "SalvageReservation_DoesNotSpendStockUntilCommit",
            "SalvageReservation_CannotReserveMorePullsThanDailyStock",
            "InspectionDisturbancePolicy_OnlyFirstDiscoveryAddsDisturbance",
            "InspectionRateLimit_RejectsRepeatedAttemptsUntilCooldownExpires",
            "InteractionClaim_AllowsOnlyOneNewInteractionPerFrame",
            "InteractionClaim_BlocksWhileHarvestOrSurveyIsActive",
            "SalvageReservation_IsNotPersistedAsSpentBeforeCommit",
            "CargoValidation_RejectsDuplicateInstanceIdsBeforeRestore",
        ):
            if f"void {required_test}()" not in impact_tests:
                failures.append(f"SeaStateAndCargoDamageTests is missing regression test: {required_test}")

        motor_tests = (ROOT / "Assets/_Project/Tests/EditMode/BoatMotorTelemetryTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "EngineLoad_IsZeroWhenThrottleIsReleasedEvenAtHighSpeed",
            "EngineLoad_IncreasesAsTheBoatBuildsSpeedUnderThrottle",
            "EngineLoad_ScalesWithThrottleDemand",
            "ReverseSpeed_UsesTheLowerReverseSpeedLimit",
            "EngineRpmProxy_RetainsIdleAndReachesMaximumAtFullThrottle",
            "EngineTelemetry_RejectsInvalidInputsAndRemainsBounded",
        ):
            if f"void {required_test}()" not in motor_tests:
                failures.append(f"BoatMotorTelemetryTests is missing: {required_test}")

        for required_text in ("journalEntries.Count", "randomStreams.Count", "worldFlags.ConvertAll", "eventCooldowns.ConvertAll"):
            if required_text not in save_source:
                failures.append(f"SaveGameData is missing malformed collection validation: {required_text}")

        # Cross-system economy wiring: these checks protect the actual vessel -> service ->
        # transaction -> save path, not merely the presence of individual controller files.
        wallet_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/EconomyWalletController.cs").read_text(encoding="utf-8")
        for required_text in (
            "TransactionApplied", "Delegate[] listeners = handler.GetInvocationList()",
            "((Action<string, long, long>)listeners[i])",
            "Wallet transaction listener failed:",
            "HasProcessedTransaction", "ISaveParticipant", "ISaveDataValidator",
        ):
            if required_text not in wallet_source:
                failures.append(f"EconomyWalletController is missing transaction integration safeguard: {required_text}")

        fuel_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/VesselFuelController.cs").read_text(encoding="utf-8")
        for required_text in (
            "FuelConsumed", "Delegate[] listeners = handler.GetInvocationList()",
            "((Action<float>)listeners[i])", "Fuel-consumption listener failed:",
            "equipment.FuelUseMultiplier", "equipment.WearEngine",
            "emergencyFuelUnits", "ISaveParticipant", "ISaveDataValidator",
        ):
            if required_text not in fuel_source:
                failures.append(f"VesselFuelController is missing fuel/equipment event integration: {required_text}")

        equipment_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/VesselEquipmentController.cs").read_text(encoding="utf-8")
        for required_text in (
            "TryEquip", "TryUnequip", "TryRepairSlot", "EnginePowerMultiplier",
            "RodCastMultiplier", "RodControlMultiplier", "WearRodForCatch",
            "CaptureSaveData", "RestoreSaveData", "TryValidateSaveData",
        ):
            if required_text not in equipment_source:
                failures.append(f"VesselEquipmentController is missing loadout integration: {required_text}")

        boat_source = (ROOT / "Assets/_Project/Scripts/Runtime/Boat/BoatPrototypeController.cs").read_text(encoding="utf-8")
        for required_text in (
            "economyTelemetry.ObservePropulsion(throttle)",
            "fuelController.ConsumePropulsion(throttle, CurrentEngineLoad, Time.fixedDeltaTime)",
            "equipmentController.EnginePowerMultiplier",
        ):
            if required_text not in boat_source:
                failures.append(f"BoatPrototypeController is disconnected from expedition economy: {required_text}")

        fishing_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/FishingSystemDirector.cs").read_text(encoding="utf-8")
        for required_text in (
            "equipment.RodCastMultiplier", "equipment.RodControlMultiplier",
            "equipment.WearRodForCatch",
        ):
            if required_text not in fishing_source:
                failures.append(f"FishingSystemDirector is disconnected from installed rod equipment: {required_text}")

        harvest_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/HarvestingMinigameController.cs").read_text(encoding="utf-8")
        for required_text in (
            "GetComponent<FishingSystemDirector>()",
            "gameObject.AddComponent<FishingSystemDirector>()",
            "fishingSystem.Configure(input, hold, director)",
            "fishingSystem.TryStartFishing()",
        ):
            if required_text not in harvest_source:
                failures.append(f"HarvestingMinigameController is disconnected from fishing runtime: {required_text}")

        # The generated catalog must stay in lockstep with the authored seafood schedule.
        seafood_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
        catalog_match = re.search(r'const string rows = @"(.*?)";', seafood_builder, re.DOTALL)
        if catalog_match is None:
            failures.append("GreyhavenEnvironmentBuilder is missing the embedded seafood catalog.")
        else:
            catalog_rows = [line for line in catalog_match.group(1).splitlines() if line.strip()]
            catalog_by_id = {}
            catalog_counts = {"FSH": 0, "INV": 0, "ALG": 0}
            for row in catalog_rows:
                fields = row.split("|")
                if len(fields) != 14:
                    failures.append(f"Malformed generated seafood catalog row: {row}")
                    continue
                item_id = fields[0]
                prefix = item_id.split("-", 1)[0]
                if prefix not in catalog_counts:
                    failures.append(f"Unknown seafood catalog prefix: {item_id}")
                    continue
                catalog_counts[prefix] += 1
                if item_id in catalog_by_id:
                    failures.append(f"Duplicate generated seafood catalog ID: {item_id}")
                catalog_by_id[item_id] = fields
            if catalog_counts != {"FSH": 105, "INV": 47, "ALG": 8}:
                failures.append(f"Seafood catalog counts are incomplete: {catalog_counts}")

            price_doc = (ROOT / "docs/07_GAME_RULES_AND_BALANCING/SEAFOOD-SELLING-PRICE-SCHEDULE.md").read_text(encoding="utf-8")
            scheduled = {}
            for line in price_doc.splitlines():
                if not re.match(r"^\|\s*(FSH|INV|ALG)-", line):
                    continue
                fields = [part.strip() for part in line.split("|")]
                if len(fields) < 6:
                    failures.append(f"Malformed seafood price schedule row: {line}")
                    continue
                price_match = re.search(r"\$([0-9.]+)", fields[5])
                if price_match:
                    scheduled[fields[1]] = (price_match.group(1), fields[4])
            if set(catalog_by_id) != set(scheduled):
                failures.append("Generated seafood catalog IDs do not exactly match the authoritative price schedule.")
            for item_id, fields in catalog_by_id.items():
                expected = scheduled.get(item_id)
                if expected and (fields[2] != expected[0] or fields[3] != expected[1]):
                    failures.append(f"{item_id} price/market class differs from the authoritative schedule.")

        if "seafoodUsdPerKg" in seafood_builder:
            failures.append("Legacy five-fish ConfigureEconomy table must not overwrite the expanded seafood catalog.")
        for required_text in (
            "AddExpandedSeafoodCatalog(definitions)", "SeaweedHarvesting",
            'FindProperty("minimumLegalCatchWeightKg")',
            'FindProperty("availableRegionIds")', 'FindProperty("contractOnlySale")',
            'FindProperty("requiredHookId")',
        ):
            if required_text not in seafood_builder:
                failures.append(f"GreyhavenEnvironmentBuilder is missing seafood catalog integration: {required_text}")

        market_policy = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/SeafoodMarketPolicy.cs").read_text(encoding="utf-8")
        for required_text in (
            "GetDestinationFactor", "greyhaven", "the_shallows", "blackrock_reach",
            "the_drowned_parish", "the_outer_shoals", "the_hollow_trench",
            "1.08m", "1.20m", "1.25m", "1.40m",
        ):
            if required_text not in market_policy:
                failures.append(f"SeafoodMarketPolicy is missing destination market data: {required_text}")

        legality_policy = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/CatchLegalityPolicy.cs").read_text(encoding="utf-8")
        for required_text in (
            "CatchViolation.ClosedArea", "CatchViolation.OutsideSpeciesRange",
            "CatchViolation.Undersized", "CatchViolation.ProtectedSpecies",
            "CatchViolation.MissingHarvestPermit", "CatchViolation.ProhibitedGear", "CatchViolation.ProhibitedTechnique",
            "MinimumLegalCatchWeightKg", "RequiredHookId", "HasPermit", "the_outer_shoals",
        ):
            if required_text not in legality_policy:
                failures.append(f"CatchLegalityPolicy is missing an authored legality rule: {required_text}")

        save_source = (ROOT / "Assets/_Project/Scripts/Runtime/Persistence/SaveGameData.cs").read_text(encoding="utf-8")
        for required_text in ("catchRegionId", "catchTechniqueId", "catchLegal", "catchLegalityReason", "catchSizeKg"):
            if required_text not in save_source:
                failures.append(f"CargoPlacementSaveData is missing catch provenance: {required_text}")

        cargo_source = (ROOT / "Assets/_Project/Scripts/Runtime/Inventory/SpatialCargoHold.cs").read_text(encoding="utf-8")
        for required_text in ("TrySetCatchLegality", "catchRegionId = item.catchRegionId", "catchLegal = string.IsNullOrWhiteSpace(saved.catchRegionId) || saved.catchLegal"):
            if required_text not in cargo_source:
                failures.append(f"SpatialCargoHold is missing catch provenance persistence: {required_text}")

        chart_controller_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/RegionalChartController.cs").read_text(encoding="utf-8")
        for required_text in ("HasPermit", "GetPermittedHarvestNames", "IsChartRegionId"):
            if required_text not in chart_controller_source:
                failures.append(f"RegionalChartController is missing harvest permit support: {required_text}")

        research_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/HarbourmasterResearchController.cs").read_text(encoding="utf-8")
        for required_text in ('GrantChartPermit("reef_stewardship")',
                              'GrantChartPermit("licensed_sea_cucumber")',
                              'GrantChartPermit("managed_harvest")'):
            if required_text not in research_source:
                failures.append(f"Harbourmaster research is missing harvest permit progression: {required_text}")

        fishing_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/FishingSystemDirector.cs").read_text(encoding="utf-8")
        for required_text in (
            "CatchLegalityPolicy.Evaluate", "TrySetCatchLegality", "economyTelemetry.RecordCatch",
            "fish.IsAvailableInRegion(regionId)", "FishingTechnique.SeaweedHarvesting",
            "fish.Category == CargoItemCategory.Seaweed",
            "Hooks[hookIndex].id",
            'float saleQuantity = hookedFish.ItemId == "INV-019" ? 1f : size;',
            "FINE NOT COLLECTED", "FINE PAID",
        ):
            if required_text not in fishing_source:
                failures.append(f"FishingSystemDirector is missing seafood/legality integration: {required_text}")

        telemetry_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/ExpeditionEconomyTelemetry.cs").read_text(encoding="utf-8")
        for required_text in (
            "SamplesFlagId", "MaximumBalanceSamples", "RecordCatch", "BuildBalanceReport",
            "departureRegionId", "destinationRegionId", "legalCatchCount", "loadoutBand",
            "GetLoadoutBandName", "IsValidActiveRecord", "EncodeBalanceSamples", "TryDecodeBalanceSamples", "MEDIAN NET-HR",
        ):
            if required_text not in telemetry_source:
                failures.append(f"ExpeditionEconomyTelemetry is missing measured balance sampling: {required_text}")

        market_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/MaraFishMarketController.cs").read_text(encoding="utf-8")
        for required_text in (
            "OrdinarySaleEligible", "IsAberrant", "CalculateDemandFactor",
            "SeafoodMarketPolicy.GetDestinationFactor", "regional_market.", "ContractOnlySale",
            "TryCommitRemoval", "wallet.TryApplyTransaction", "RefreshStock",
        ):
            if required_text not in market_source:
                failures.append(f"MaraFishMarketController is missing buyer/cargo/wallet integration: {required_text}")

        vendor_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/GreyhavenVendorServices.cs").read_text(encoding="utf-8")
        for required_text in (
            "TryPurchase", "TrySellSalvage", "TryRefuel", "TryRepairAll",
            "TryRepairEquipmentSlot", "TryPlaceFirstFit", "wallet.TryApplyTransaction",
            "cargo.TryRemove", "supplyStock",
        ):
            if required_text not in vendor_source:
                failures.append(f"GreyhavenVendorServices is missing purchase/service integration: {required_text}")

        contract_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/HarbourmasterContractController.cs").read_text(encoding="utf-8")
        for required_text in (
            "TryAcceptNext", "TryCompleteActive", "TryCommitRemoval",
            "wallet.TryApplyTransaction", "CaptureSaveData", "RestoreSaveData",
            "ISaveParticipant", "ISaveDataValidator",
        ):
            if required_text not in contract_source:
                failures.append(f"HarbourmasterContractController is missing delivery/save integration: {required_text}")

        research_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/HarbourmasterResearchController.cs").read_text(encoding="utf-8")
        for required_text in (
            "DiscoveredSiteIds", "TryCommitRemoval", "wallet.TryApplyTransaction",
            "ApplyEarnedPermits", "GrantChartPermit", "unlockedRegionIds",
            "ISaveParticipant", "ISaveDataValidator",
        ):
            if required_text not in research_source:
                failures.append(f"HarbourmasterResearchController is missing evidence/permit/save integration: {required_text}")

        telemetry_source = (ROOT / "Assets/_Project/Scripts/Runtime/Economy/ExpeditionEconomyTelemetry.cs").read_text(encoding="utf-8")
        for required_text in (
            "subscribedWallet.TransactionApplied", "subscribedFuel.FuelConsumed",
            "ObservePropulsion", "OnTransactionApplied", "OnFuelConsumed",
            "subscribedWallet", "subscribedFuel",
            "if (subscribedWallet == wallet) return;", "if (subscribedFuel == fuel) return;",
            "dockside.IsMenuOpen", "CompleteTrip", "CaptureSaveData", "RestoreSaveData",
        ):
            if required_text not in telemetry_source:
                failures.append(f"ExpeditionEconomyTelemetry is missing trip accounting integration: {required_text}")

        economy_builder = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
        for required_component in (
            "EconomyWalletController", "VesselFuelController", "VesselEquipmentController",
            "HarbourmasterResearchController", "MaraFishMarketController",
            "GreyhavenVendorServices", "ExpeditionEconomyTelemetry", "RegionalChartController",
        ):
            if (
                f"AddComponent<WhereTheSoundingsEnd.Economy.{required_component}>()" not in economy_builder
                and f"AddComponent<{required_component}>()" not in economy_builder
            ):
                failures.append(f"Greyhaven generated vessel is missing economy component: {required_component}")

        dockside_source = (ROOT / "Assets/_Project/Scripts/Runtime/Interaction/DocksideHubController.cs").read_text(encoding="utf-8")
        for required_text in (
            "GetComponent<MaraFishMarketController>()",
            "GetComponent<GreyhavenVendorServices>()",
            "GetComponent<HarbourmasterContractController>()",
            "AddComponent<HarbourmasterContractController>()",
            "TryAcceptNext", "TryCompleteActive",
            "GetComponent<HarbourmasterResearchController>()",
            "GetComponent<VesselEquipmentController>()",
            "TrySubmitNext", "TrySell", "TryPurchase",
            "TryRepairEquipmentSlot", "TryEquip", "TryUnequip",
            "ExpeditionEconomyTelemetry",
        ):
            if required_text not in dockside_source:
                failures.append(f"DocksideHubController is missing economy service wiring: {required_text}")

        seafood_tests = (ROOT / "Assets/_Project/Tests/EditMode/SeafoodMarketPolicyTests.cs").read_text(encoding="utf-8")
        for required_test in ("DestinationFactorMatchesAuthoredMarketMatrix", "EveryEstablishedRegionHasAMarketAndUnknownRegionHasNoPrice"):
            if f"void {required_test}(" not in seafood_tests:
                failures.append(f"SeafoodMarketPolicyTests is missing: {required_test}")

        legality_tests = (ROOT / "Assets/_Project/Tests/EditMode/CatchLegalityPolicyTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "StandardCatchInsideAuthoredRangeIsLegal", "UndersizedCatchIsFlaggedWithAuthoredFine",
            "SpeciesOutsideItsAuthoredRegionIsFlagged", "HollowTrenchRequiresOuterShoalsResearchPermit",
            "ProtectedSpeciesRequiresPermitAndAuthorizedTechnique", "ContractOnlyHarvestRequiresItsSpecificPermit",
        ):
            if f"void {required_test}()" not in legality_tests:
                failures.append(f"CatchLegalityPolicyTests is missing: {required_test}")
        if "CatchViolation.ProhibitedGear" not in legality_tests or "circle-hook" not in legality_tests:
            failures.append("CatchLegalityPolicyTests is missing the required-hook legality regression.")

        cargo_legality_tests = (ROOT / "Assets/_Project/Tests/EditMode/CatchLegalityCargoSaveTests.cs").read_text(encoding="utf-8")
        if "IllegalCatchProvenanceSurvivesSaveAndRestore" not in cargo_legality_tests:
            failures.append("CatchLegalityCargoSaveTests is missing its save/restore regression.")

        journal_policy_tests = (ROOT / "Assets/_Project/Tests/EditMode/FieldJournalPolicyTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "CreateEntryId_UsesStableNamespaceAndTrimmedItemId",
            "TryGetItemId_RejectsOtherNamespacesAndBlankSuffixes",
            "FindingIds_UseTheirOwnStableNamespace",
            "NormalizeRegionId_UsesUnknownFallbackAndStableCase",
        ):
            if f"void {required_test}()" not in journal_policy_tests:
                failures.append(f"FieldJournalPolicyTests is missing: {required_test}")

        journal_ui = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs").read_text(encoding="utf-8")
        for required_text in (
            '"FIELD JOURNAL  [J]"', "BuildJournalPanel", "ShowJournalEntry",
            "ShowJournalFinding", "MarkFieldJournalEntryRead", "MarkFindingRead",
            "UNEXPLAINED FINDINGS", "FormatJournalDate",
        ):
            if required_text not in journal_ui:
                failures.append(f"Field journal UI is missing: {required_text}")

        fishing_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/FishingSystemDirector.cs").read_text(encoding="utf-8")
        sounding_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/SoundingRigController.cs").read_text(encoding="utf-8")
        for source, required_texts, label in (
            (fishing_source, ("fieldJournalEntries", "CreateEntryId", "recordedAtUtc", "MarkFieldJournalEntryRead"), "fishing journal"),
            (sounding_source, ("discoveredSiteRecords", "CreateFindingEntryId", "recordedAtUtc", "MarkFindingRead"), "survey journal"),
        ):
            for required_text in required_texts:
                if required_text not in source:
                    failures.append(f"{label} is missing persistence/read integration: {required_text}")

        telemetry_tests = (ROOT / "Assets/_Project/Tests/EditMode/ExpeditionEconomyTelemetryTests.cs").read_text(encoding="utf-8")
        if "CompletedBalanceSamplesStayWithinSaveFlagLimit" not in telemetry_tests:
            failures.append("ExpeditionEconomyTelemetryTests is missing bounded 50-sample persistence coverage.")

        fishing_tests = (ROOT / "Assets/_Project/Tests/EditMode/FishingSimulationPolicyTests.cs").read_text(encoding="utf-8")
        if "FishPreferredTechnique.SeaweedHarvesting" not in fishing_tests:
            failures.append("FishingSimulationPolicyTests is missing seaweed harvest technique compatibility.")

        research_tests = (ROOT / "Assets/_Project/Tests/EditMode/HarbourmasterResearchSaveTests.cs").read_text(encoding="utf-8")
        for permit in ("reef_stewardship", "licensed_sea_cucumber"):
            if permit not in research_tests:
                failures.append(f"HarbourmasterResearchSaveTests is missing harvest permit persistence coverage: {permit}")


        setup_source = (ROOT / "Assets/_Project/Scripts/Editor/ProjectFoundationSetup.cs").read_text(encoding="utf-8")
        for required_text in (
            "InitializeForAutomation",
            "ProjectFirstOpenBootstrap : AssetPostprocessor",
            "OnPostprocessAllAssets(",
            "didDomainReload",
            "EditorApplication.delayCall",
            "Application.isBatchMode",
        ):
            if required_text not in setup_source:
                failures.append(f"ProjectFoundationSetup is missing first-open/CI bootstrap behavior: {required_text}")

        build_source = (ROOT / "Assets/_Project/Scripts/Editor/ProjectBuildAutomation.cs").read_text(encoding="utf-8")
        for required_text in ("BuildWindowsDevelopment", "StandaloneWindows64", "BuildOptions.Development", "BuildResult.Succeeded"):
            if required_text not in build_source:
                failures.append(f"ProjectBuildAutomation is missing Windows build contract: {required_text}")

        play_mode_source = (ROOT / "Assets/_Project/Tests/PlayMode/GreyhavenAreaStreamingPlayModeTests.cs").read_text(encoding="utf-8")
        for required_test in ("GreyhavenStartsWithPlayerControlledVesselAndCoreLoop", "AdditiveAreasPreloadAndRetireWhilePersistentGameplaySceneStaysLoaded"):
            if f"IEnumerator {required_test}()" not in play_mode_source:
                failures.append(f"Greyhaven Play Mode coverage is missing: {required_test}")

        unity_workflow = (ROOT / ".github/workflows/unity-verification.yml").read_text(encoding="utf-8")
        for required_text in ("self-hosted", "unity-6000.3.28f1", "-runTests", "-testPlatform EditMode", "-testPlatform PlayMode", "BuildWindowsDevelopment", "Get-Command"):
            if required_text not in unity_workflow:
                failures.append(f"Unity verification workflow is missing: {required_text}")
        if "secrets." in unity_workflow:
            failures.append("Unity verification workflow must keep Unity licensing on the self-hosted runner, not repository secrets.")

        economy_tests = (ROOT / "Assets/_Project/Tests/EditMode/EconomyRuntimeIntegrationTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "WalletListenerFailureDoesNotSuppressLaterListenersOrRollbackPayment",
            "FuelListenerFailureDoesNotSuppressLaterListenersOrChangeConsumedAmount",
            "WalletRejectsDuplicateTransactionWithoutEmittingSecondEvent",
        ):
            if f"void {required_test}()" not in economy_tests:
                failures.append(f"EconomyRuntimeIntegrationTests is missing integration regression: {required_test}")

    except (ValueError, KeyError, TypeError, json.JSONDecodeError) as error:
        failures.append(str(error))

    gitignore = ROOT / ".gitignore"
    if gitignore.exists():
        text = gitignore.read_text(encoding="utf-8")
        for pattern in ("[Ll]ibrary/", "[Tt]emp/", "[Oo]bj/"):
            if pattern not in text:
                failures.append(f".gitignore is missing {pattern}")





    cinematic_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenFrontEndUI.cs").read_text(encoding="utf-8")
    fishing_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/FishingSystemDirector.cs").read_text(encoding="utf-8")
    builder_source = (ROOT / "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs").read_text(encoding="utf-8")
    for item_id in ("FSH-101", "FSH-102", "FSH-103", "FSH-104", "FSH-105",
                    "SALVAGE-ARCHIVE-01", "SALVAGE-ARCHIVE-02",
                    "SALVAGE-ARCHIVE-03", "SALVAGE-ARCHIVE-04"):
        if item_id not in builder_source:
            failures.append(f"Generated field-discovery catalogue is missing {item_id}.")
    if "TryPlayFieldDiscovery" not in cinematic_source or "CreateEntryId" not in fishing_source:
        failures.append("First-time field discoveries are not connected to cinematic presentation and save persistence.")

    if failures:
        for failure in failures:
            print(f"FAIL: {failure}")
        return 1

    print("PASS: Unity project scaffold integrity checks completed.")
    print("NOTE: This validates files and configuration only; it does not compile or build with Unity.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
