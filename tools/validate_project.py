#!/usr/bin/env python3
"""Static integrity checks for the Unity scaffold; this is not a Unity build."""

from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPECTED_EDITOR = "6000.3.28f1"
EXPECTED_PACKAGES = {
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
    "Assets/_Project/Scripts/Runtime/Economy/EconomyWalletController.cs",
    "Assets/_Project/Scripts/Runtime/Economy/MaraFishMarketController.cs",
    "Assets/_Project/Tests/EditMode/EconomyPolicyTests.cs",
    "Assets/_Project/Tests/EditMode/MaraMarketTransactionTests.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenFrontEndUI.cs",
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
        if "CurrentSchemaVersion = 8" not in save_source:
            failures.append("SaveGameData schema must be version 8 after economy wallet persistence changes.")

        if "case 4:" not in save_source or "data.schemaVersion = 5;" not in save_source:
            failures.append("SaveGameData is missing the schema 4 to 5 atmosphere-state migration.")
        if "case 5:" not in save_source or "data.schemaVersion = 6;" not in save_source:
            failures.append("SaveGameData is missing the schema 5 to 6 NPC relationship-memory migration.")
        if "case 6:" not in save_source or "data.schemaVersion = 7;" not in save_source or "visitedRegionIds" not in save_source:
            failures.append("SaveGameData is missing the schema 6 to 7 regional chart discovery migration.")
        if "case 7:" not in save_source or "data.schemaVersion = 8;" not in save_source or "walletCents" not in save_source:
            failures.append("SaveGameData is missing the schema 7 to 8 economy wallet migration.")

        for required_text in (
            "weatherRemainingGameSeconds", "weatherShiftCooldownGameSeconds",
            "hallucinationCooldownGameSeconds", "birdEventPending",
            "atmosphereIncidentCountdownGameSeconds", "inspectedPhenomenonSiteIds",
            "heardAcousticEventIds",
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
            "state.PendingLoad = true",
        ):
            if required_text not in streaming_source:
                failures.append(f"AreaStreamingDirector is missing streaming safeguard: {required_text}")

        area_root_source = (ROOT / "Assets/_Project/Scripts/Runtime/Streaming/StreamedAreaSceneRoot.cs").read_text(encoding="utf-8")
        for required_text in (
            "contentRevision", "TryValidate", "GetComponentsInChildren<Renderer>",
            "sharedMesh == null", "material.shader == null", "shader.isSupported",
            "subMeshCount", "Rigidbody",
        ):
            if required_text not in area_root_source:
                failures.append(f"StreamedAreaSceneRoot is missing asset integrity check: {required_text}")

        area_policy_tests = (ROOT / "Assets/_Project/Tests/EditMode/AreaStreamingPolicyTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "ValidRadii_RequireUnloadRadiusBeyondPreloadRadius",
            "ShouldPreload_UsesInclusiveInnerBoundary",
            "ShouldUnload_UsesOuterBoundaryAndPreservesHysteresisBand",
            "DistanceRules_RejectNonFiniteValues",
        ):
            if f"void {required_test}()" not in area_policy_tests:
                failures.append(f"AreaStreamingPolicyTests is missing regression: {required_test}")

        streaming_playmode = (ROOT / "Assets/_Project/Tests/PlayMode/GreyhavenAreaStreamingPlayModeTests.cs").read_text(encoding="utf-8")
        for required_test in (
            "AdditiveAreasPreloadAndRetireWhilePersistentGameplaySceneStaysLoaded",
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
            "BuildLighthouseArtifactCluster", "BuildWreckArtifactCluster",
            "StreamedAreaRevision = \"3\"",
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
            "Build_BespokeMaritimePassCreatesDistinctRegionalAssetsWithoutGameplayComponents",
            "rendererCount, Is.GreaterThanOrEqualTo(250)",
            "GetComponentsInChildren<Collider>", "GetComponentsInChildren<MonoBehaviour>",
            "rendererCount, Is.GreaterThanOrEqualTo(60)",
        ):
            if required_text not in artifact_tests:
                failures.append(f"GreyhavenEnvironmentalArtifactTests is missing coverage: {required_text}")

        dockside_source = (ROOT / "Assets/_Project/Scripts/Runtime/Interaction/DocksideHubController.cs").read_text(encoding="utf-8")
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
        ):
            if required_text not in dockside_source:
                failures.append(f"DocksideHubController is missing expected interaction feature: {required_text}")

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
        if "Install Greyhaven area streaming?" not in setup_source:
            failures.append("Existing generated Greyhaven scenes must require explicit confirmation before destructive streaming rebuild.")

        area_scene_builder = (ROOT / "Assets/_Project/Scripts/Editor/DeepOceanSurveySandboxBuilder.cs").read_text(encoding="utf-8")
        if "Greyhaven_Lighthouse.unity" not in area_scene_builder:
            failures.append("Deep-ocean build-list refresh must preserve all additive Greyhaven area scenes.")

        sea_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/SeaStateDirector.cs").read_text(encoding="utf-8")
        for required_text in (
            "movementThreshold", "SetHarvesting", "panicLevel", "timeOfDaySeconds",
            "worldClock.Advance", "LastGameDeltaSeconds", "weatherShiftCooldownGameSeconds",
            "birdEventCountdownGameSeconds", "latestOmen",
        ):
            if required_text not in sea_source:
                failures.append(f"SeaStateDirector is missing expected runtime feature: {required_text}")

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

        for required_text in ("inspectionCooldownRealSeconds", "DeepOceanInspectionPolicy", "IsCooldownReady", "TryClaimInteraction"):
            if required_text not in site_interaction:
                failures.append(f"DeepOceanSiteInteractionController is missing rate-limit/action guards: {required_text}")

        if "TryClaimInteraction" not in sea_source or "IsGameplayActionBusy" not in sea_source:
            failures.append("SeaStateDirector must coordinate exclusive gameplay action claims.")

        ui_source = (ROOT / "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs").read_text(encoding="utf-8")
        for required_text in ("TryClaimInteraction", "BeginDialogue", "_inventoryOpen"):
            if required_text not in ui_source:
                failures.append(f"GreyhavenRuntimeUI is missing exclusive dialogue/inventory guards: {required_text}")

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

    except (ValueError, KeyError, TypeError, json.JSONDecodeError) as error:
        failures.append(str(error))

    gitignore = ROOT / ".gitignore"
    if gitignore.exists():
        text = gitignore.read_text(encoding="utf-8")
        for pattern in ("[Ll]ibrary/", "[Tt]emp/", "[Oo]bj/"):
            if pattern not in text:
                failures.append(f".gitignore is missing {pattern}")





    if failures:
        for failure in failures:
            print(f"FAIL: {failure}")
        return 1

    print("PASS: Unity project scaffold integrity checks completed.")
    print("NOTE: This validates files and configuration only; it does not compile or build with Unity.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
