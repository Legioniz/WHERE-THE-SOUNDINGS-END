# Dialogue System

> Project: WHERE THE SOUNDINGS END  
> Status: Four relationship-reactive Greyhaven resident conversations expanded across seven trust tiers; runtime verification pending  
> Owner: Project lead  
> Version: 0.4  
> Last reviewed: 2026-11-12

## Runtime stack

The project pins Yarn Spinner for Unity to `dev.yarnspinner.unity` v3.2.8. Narrative source is written in `.yarn` files and included by the Unity Yarn project at `Assets/_Project/Dialogue/Greyhaven.yarnproject`.

The Greyhaven scene generator imports the Yarn project, creates a `DialogueRunner`, attaches `GreyhavenRuntimeUI` as its custom `DialoguePresenterBase`, and registers the `checkpoint` and `relationship` commands. Each interaction passes its resident's `NpcAgent` to the presenter. The presenter seeds Yarn's shared relationship variables from that agent; C# remains authoritative for score changes and memory deduplication.

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

- Harbourmaster — missing boats, the silent lighthouse bell, channel markers, watchkeeping, weather signs and the sounding-buoy recovery job.
- Mara Venn — fish species, catch quality, freshness, market routines, unusual catches, missing crews and honest sale assessment.
- Tomas Vale — engine sounds, outboard checks, repair habits, hull damage, chalk inspection marks and unexplained knocks.
- Inez Sarmiento — net construction, line wear, knot checks, float lines, missing crews, local working rules and preserving evidence.

Each resident has a separate Yarn node and first-meeting/clue flags. Dialogue branches on the active NPC's live trust value at the first greeting and throughout topic responses. Each resident has a different greeting for Hostile, Distrustful, Wary, Neutral, Familiar, Trusted and Ally tiers. Conditional options reveal additional personal subjects at scores 60, 75 and 90. Positive and negative choices issue stable `<<relationship "event.id" delta>>` commands, with the NPC memory ledger preventing a second reward or penalty for the same event.

## Relationship-reactive dialogue content

Source: `Assets/_Project/Dialogue/Source/Greyhaven_Harbourmaster.yarn`.

Each resident now has a larger set of conversational choices. Examples of authored subjects and in-character lines include:

- Harbourmaster: normal watch duties, channel markers, weather watching, the returned boats, the silent bell and a saying about keeping a safe harbour.
- Mara: common local catch, how she inspects fish, how the market runs, preserving freshness, her missing crew's crate and a saying about freshness being a promise to the buyer.
- Tomas: healthy outboard sounds, pre-departure checks, the returned hull's scoring, chalk marks on fasteners and a saying about listening before an engine fails.
- Inez: how she checks net damage, float-line routines, signs of fraying versus cuts, her mother's knot advice and a saying about checking the knot nobody sees.

Lower relationship tiers receive shorter, more guarded answers. Familiar residents can ask about routines and work habits. Trusted residents receive more personal observations; allies can ask for the most private optional details. The characters do not reveal the whole mystery, and practical observations are kept separate from conclusions.

The Harbourmaster's existing flags still track first meeting, whether the missing-boat warning and bell question were heard, and whether the buoy job was accepted. The old trust variable remains for compatibility. The shared `$npc_relationship` variable drives the active resident's dialogue branches; the C# NPC state is authoritative.

## Persistence and commands

`YarnDialogueSaveParticipant` captures and restores Yarn float, string and bool variables through `SaveGameData.dialogue`. This preserves the Harbourmaster's branch state across manual saves and the dedicated autosave. The current conversation line/presenter cursor is not persisted; loading resumes the campaign state, not a mid-sentence UI frame.

When the player accepts the sounding-buoy job, Yarn runs:

`<<checkpoint "greyhaven.sounding_buoy_accepted">>`

The registered C# command requests an immediate dedicated autosave. This does not overwrite one of the five manual slots.

## Localization

The Yarn project declares English as its base language and uses Yarn's line IDs/localized-line pipeline. The project is localization-ready at the content format level; no translated string tables or second language have been authored or tested.

## Content and production boundaries

- Individual trust tiers, a visible 0–100 meter, relationship-event commands, once-only event memory and per-resident conversation/trade gates are implemented for all four Greyhaven residents.
- The future market can read `CanTrade` and `BuyPriceMultiplier`; sale transactions, price application, repairs, quest rewards and global community reputation remain unimplemented.
- NPC movement uses short direct waypoints, not NavMesh pathfinding. On-foot interaction, interiors, animation, voice-over, facial animation and localization packages are not connected.
- Dialogue choices update Yarn variables; broader quest state and world changes must be connected through explicit commands/functions and authoritative gameplay systems.
- Repository checks can verify that all four node sources contain the seven relationship tier boundaries, a character quote option and representative daily topics. These checks do not parse or compile Yarn.
- Yarn compilation, line presentation, conditional-option filtering, command dispatch and all seven trust tiers must still be verified in Unity 6000.3.28f1 after package import. Static GitHub C# syntax checks do not compile Yarn or resolve Unity symbols.

## Related systems

- `docs/06_GAMEPLAY_SYSTEMS/SAVE-AND-LOAD.md`
- `docs/08_TECHNICAL_DESIGN/SAVE-DATA-STRUCTURE.md`
- `Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/YarnDialogueSaveParticipant.cs`
