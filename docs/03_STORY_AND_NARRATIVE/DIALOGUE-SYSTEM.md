# Dialogue System

> Project: WHERE THE SOUNDINGS END  
> Status: Yarn Spinner runtime loop and four Greyhaven resident conversations implemented; authoring/content expansion pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Runtime stack

The project pins Yarn Spinner for Unity to `dev.yarnspinner.unity` v3.2.8. Narrative source is written in `.yarn` files and included by the Unity Yarn project at `Assets/_Project/Dialogue/Greyhaven.yarnproject`.

The Greyhaven scene generator imports the Yarn project, creates a `DialogueRunner`, attaches `GreyhavenRuntimeUI` as its custom `DialoguePresenterBase`, and registers the game's `checkpoint` command. Yarn controls dialogue branching and variable evaluation; the presenter owns only the UI and input that display lines and choices.

## First interaction loop

1. The boat enters the Harbourmaster's trigger radius near the Greyhaven quay.
2. The HUD displays an E prompt.
3. Press **E** to start node `Greyhaven_Harbourmaster`.
4. The dialogue panel displays the speaker and line. Press **Space**, **Enter**, or CONTINUE to advance.
5. When Yarn offers choices, they appear as buttons. Click a choice or press **1–5**.
6. Yarn evaluates the selected branch, updates persistent variables, and continues until the node ends.
7. The dialogue panel closes and scaled gameplay time resumes.

The starter NPC is a primitive blockout representation. Interaction is from the boat because the project does not yet have a walkable on-foot character controller.

## Implemented Greyhaven resident dialogue

The generated scene now exposes four boat-proximity interactions:

- Harbourmaster — missing boats, the silent lighthouse bell, channel advice and the sounding-buoy recovery job.
- Mara Venn — fish condition, unusual catches, the missing crews and honest sale assessment.
- Tomas Vale — repair advice, engine sounds, hull damage and unexplained knocks.
- Inez Sarmiento — damaged nets, missing crews, local working rules and preserving evidence.

Each resident has a separate Yarn node and first-meeting/trust variables. Their authored content is in `Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn`.

## Implemented Harbourmaster branches

Source: `Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn`.

- First meeting and repeat greeting.
- Missing boats warning, with a nested question about the silent lighthouse bell.
- Channel advice about the shallow western channel, eastern broken stone and engine noise near mangroves.
- A sounding-buoy recovery job, with a repeat response after acceptance.
- A quiet/no-question branch.

The Harbourmaster variables track first meeting, whether the missing-boat warning and bell question were heard, whether the buoy job was accepted, and a small trust value. These are deliberately stable Yarn variable names and are saved with the existing campaign save.

## Persistence and commands

`YarnDialogueSaveParticipant` captures and restores Yarn float, string and bool variables through `SaveGameData.dialogue`. This preserves the Harbourmaster's branch state across manual saves and the dedicated autosave. The current conversation line/presenter cursor is not persisted; loading resumes the campaign state, not a mid-sentence UI frame.

When the player accepts the sounding-buoy job, Yarn runs:

`<<checkpoint "greyhaven.sounding_buoy_accepted">>`

The registered C# command requests an immediate dedicated autosave. This does not overwrite one of the five manual slots.

## Localization

The Yarn project declares English as its base language and uses Yarn's line IDs/localized-line pipeline. The project is localization-ready at the content format level; no translated string tables or second language have been authored or tested.

## Content and production boundaries

- The first NPC schedule and relationship runtime foundation is implemented in `Assets/_Project/Scripts/Runtime/NPC/NpcAgent.cs`; only the four Greyhaven residents are configured.
- Relationship values are saved but are not yet modified by dialogue choices or connected to shop prices, quests or reputation.
- NPC movement uses short direct waypoints, not NavMesh pathfinding. On-foot interaction, interiors, animation, voice-over, facial animation and localization packages are not connected.
- Dialogue choices update Yarn variables; broader quest state and world changes must be connected through explicit commands/functions and authoritative gameplay systems.
- Yarn compilation, line presentation and option selection must be verified in Unity 6000.3.28f1 after package import. Static GitHub checks do not compile Yarn or C#.

## Related systems

- `docs/06_GAMEPLAY_SYSTEMS/SAVE-AND-LOAD.md`
- `docs/08_TECHNICAL_DESIGN/SAVE-DATA-STRUCTURE.md`
- `Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/YarnDialogueSaveParticipant.cs`
