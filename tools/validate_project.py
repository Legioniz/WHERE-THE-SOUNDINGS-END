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
    "Assets/_Project/Scripts/Runtime/Core/WorldClock.cs",
    "Assets/_Project/Scripts/Runtime/Core/ActionDrivenClockPolicy.cs",
    "Assets/_Project/Scripts/Runtime/Core/Pcg32Random.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveGameData.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveFileService.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveGameCoordinator.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/AutosaveController.cs",
    "Assets/_Project/Scripts/Runtime/Inventory/CargoItemDefinition.cs",
    "Assets/_Project/Scripts/Runtime/Inventory/CargoGridGeometry.cs",
    "Assets/_Project/Scripts/Runtime/Inventory/SpatialCargoHold.cs",
    "Assets/_Project/Scripts/Runtime/World/SeaStateDirector.cs",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanSurveyMath.cs",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanSurveyRegion.cs",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanPhenomenonSite.cs",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanPhenomenonRules.cs",
    "Assets/_Project/Scripts/Runtime/World/AcousticEventBus.cs",
    "Assets/_Project/Scripts/Runtime/World/AcousticWorldListener.cs",
    "Assets/_Project/Scripts/Runtime/World/DeepOceanSiteInteractionController.cs",
    "Assets/_Project/Scripts/Runtime/World/SoundingRigController.cs",
    "Assets/_Project/Scripts/Runtime/World/VesselConditionController.cs",
    "Assets/_Project/Scripts/Runtime/World/HarvestingMinigameController.cs",
    "Assets/_Project/Scripts/Runtime/World/SalvageSite.cs",
    "Assets/_Project/Scripts/Runtime/World/PanicBirdSwarmVisual.cs",
    "Assets/_Project/Tests/EditMode/SeaStateAndCargoDamageTests.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/YarnDialogueSaveParticipant.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs",
    "Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs",
    "Assets/_Project/Dialogue/Greyhaven.yarnproject",
    "Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn",
    "Assets/_Project/Tests/EditMode/CargoGridGeometryTests.cs",
    "docs/06_GAMEPLAY_SYSTEMS/ACTION-DRIVEN-SEA-STATE.md",
    "Assets/_Project/Scripts/Runtime/Boat/BoatPrototypeController.cs",
    "Assets/_Project/Scripts/Runtime/Camera/CameraFollow2_5D.cs",
    "Assets/_Project/Scripts/Editor/WhereTheSoundingsEnd.Editor.asmdef",
    "Assets/_Project/Scripts/Editor/ProjectFoundationSetup.cs",
    "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs",
    "Assets/_Project/Scripts/Editor/DeepOceanSurveySandboxBuilder.cs",
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
        if "CurrentSchemaVersion = 5" not in save_source:
            failures.append("SaveGameData schema must be version 5 after atmospheric/acoustic persistence changes.")

        if "case 4:" not in save_source or "data.schemaVersion = 5;" not in save_source:
            failures.append("SaveGameData is missing the schema 4 to 5 atmosphere-state migration.")

        for required_text in (
            "weatherRemainingGameSeconds", "weatherShiftCooldownGameSeconds",
            "hallucinationCooldownGameSeconds", "birdEventPending",
            "atmosphereIncidentCountdownGameSeconds", "inspectedPhenomenonSiteIds",
            "heardAcousticEventIds",
        ):
            if required_text not in save_source:
                failures.append(f"WorldSaveData is missing persistent state: {required_text}")

        survey_source = (ROOT / "Assets/_Project/Scripts/Runtime/World/SoundingRigController.cs").read_text(encoding="utf-8")
        for required_text in ("SoundingPulse", "discoveredPhenomenonSiteIds", "evaluatedSurveyOpportunityIds", "AcousticEventBus.Publish", "IsIndependentLayerObservation", "CalculateSiteSignalConfidence"):
            if required_text not in survey_source:
                failures.append(f"SoundingRigController is missing expected survey feature: {required_text}")

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

        boat_source = (ROOT / "Assets/_Project/Scripts/Runtime/Boat/BoatPrototypeController.cs").read_text(encoding="utf-8")
        if "if (isActiveAndEnabled) BindInputActions();" not in boat_source:
            failures.append("BoatPrototypeController must bind input assigned by the environment builder after AddComponent.")

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
                "Ask for work.",
                '<<checkpoint "greyhaven.sounding_buoy_accepted">>',
            ):
                if required_text not in yarn_text:
                    failures.append(f"Harbourmaster Yarn source is missing: {required_text}")

        input_map = maps.get("Boat")
        if input_map is not None:
            action_ids = {action["name"]: action.get("id") for action in input_map.get("actions", [])}
            if action_ids.get("Inventory") != "b0a70000000000000000000000000007":
                failures.append("Inventory input action ID does not match the committed control map.")

        for path in ROOT.glob("Assets/_Project/**/*.asmdef"):
            definition = json.loads(path.read_text(encoding="utf-8"))
            if not definition.get("name"):
                failures.append(f"Assembly definition has no name: {path.relative_to(ROOT)}")
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
