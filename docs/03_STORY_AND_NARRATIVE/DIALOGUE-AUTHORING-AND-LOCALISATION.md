# Dialogue Authoring and Localisation

> Project: WHERE THE SOUNDINGS END  
> Status: Yarn Spinner and generated Greyhaven dialogue wiring implemented; authoring expanded, localisation pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-12

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

## Greyhaven resident content

`Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn` contains four resident nodes: the Harbourmaster, Mara Venn, Tomas Vale and Inez Sarmiento. Each node includes seven relationship-based greetings, everyday conversation, profession-specific facts, memorable sayings and additional optional subjects unlocked at Familiar (60), Trusted (75) and Ally (90).

The Yarn variables `$npc_relationship`, `$npc_can_talk`, `$npc_can_trade` and `$npc_buy_price_multiplier` are written from the active resident's C# `NpcAgent` before `StartDialogue`. The UI refreshes those variables after each relationship command. C# remains the authority for trust and the once-only event-memory ledger.

Relationship changes use `<<relationship "stable.event.id" delta>>`. The event ID is unique for each resident/action and cannot be farmed by reopening the same topic. The typed command is registered with `DialogueRunner.AddCommandHandler<string, int>`.

## Unity setup and verification

The generated Greyhaven builder imports the Yarn Project, creates the Dialogue Runner, registers the `checkpoint` and typed `relationship` commands, and attaches the custom dialogue presenter. It assigns each generated interaction the corresponding resident's `NpcAgent`. Hand-placed interactions auto-resolve a local agent; a missing agent is reported and the conversation prompt is disabled rather than using a neutral fallback.

After pulling changes, run **Tools > WHERE THE SOUNDINGS END > Rebuild Greyhaven Environment**. Then open the generated scene and verify:
1. The four resident triggers open their intended Yarn nodes.
2. Their relationship meter shows that resident's live score.
3. A test score change is reflected in the greeting and subsequent answer branches.
4. Conditional personal topics only appear at their intended score thresholds.
5. Relationship event commands change the meter once and do not repeat on reopening the same topic.
6. A missing NPC agent cannot start a conversation.

Yarn compilation and Play Mode interaction still require verification in Unity `6000.3.28f1`.

Unity-generated project/scene assets must be authored in the Editor rather than guessing serialized asset GUIDs.

## Localization

- Treat each Yarn line ID as a stable content key.
- Export source strings through Yarn's localization workflow.
- Keep translations separate from source scripts.
- Avoid hard-coded language branches in C#.
- Test choice length, line wrapping, timing and subtitles at all supported UI scales.
