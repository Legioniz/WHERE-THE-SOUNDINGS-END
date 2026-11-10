# Dialogue Authoring and Localisation

> Project: WHERE THE SOUNDINGS END  
> Status: Yarn Spinner integrated; scene wiring pending  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Tool choice

Yarn Spinner for Unity v3.2.8 is pinned in Packages/manifest.json. Yarn scripts live as editable `.yarn` source files and are compiled through a Yarn Project asset in Unity. The Unity package supplies the Dialogue Runner, presenters, commands/functions and localization workflow.

Official documentation:

- Installation: https://www.yarnspinner.dev/docs/unity/01-installation/
- Unity runtime overview: https://www.yarnspinner.dev/docs/unity/
- Variables and storage: https://yarnspinner.dev/docs/unity/05-connecting-to-your-game/04-variables-and-storage/

## Narrative architecture

- Yarn owns authored dialogue, branching, conditional choices and dialogue-specific variables.
- C# owns authoritative gameplay state: inventory, currency, quest rewards, world flags, NPC simulation and interactions.
- Yarn commands/functions request gameplay actions through narrow C# services; they do not duplicate the game's source of truth.
- Variables use namespaced IDs, for example `$gh_met_harbourmaster`.
- Dialogue state is saved in the same versioned SaveGameData document as the campaign. Do not create a separate dialogue save file.
- The current persistence participant stores typed float, string and bool variables.
- Smart variables are derived and should not be persisted.
- Saving stores variables and progression flags, not the current character-by-character presentation cursor. Resumable dialogue must use an explicit stable node/checkpoint ID.

## Initial Greyhaven content

`Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn` establishes a first branching harbourmaster conversation. It introduces missing boats, the silent lighthouse bell, the sounding buoy job and channel navigation warnings.

## Unity setup required

1. Resolve Yarn Spinner package in Package Manager.
2. Create a Yarn Project asset.
3. Add the source Yarn file to that project.
4. Place a Dialogue Runner in the Greyhaven scene and assign the Yarn Project.
5. Assign that Dialogue Runner to YarnDialogueSaveParticipant on Save_Game_Coordinator.
6. Wire the harbourmaster interaction to start the named node.

Unity-generated project/scene assets must be authored in the Editor rather than guessing serialized asset GUIDs.

## Localization

- Treat each Yarn line ID as a stable content key.
- Export source strings through Yarn's localization workflow.
- Keep translations separate from source scripts.
- Avoid hard-coded language branches in C#.
- Test choice length, line wrapping, timing and subtitles at all supported UI scales.
