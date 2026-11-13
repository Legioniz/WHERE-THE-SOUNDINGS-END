# Dialogue System

> Project: WHERE THE SOUNDINGS END  
> Status: Four relationship-reactive resident conversations and dockside hub handoff implemented; Unity/Yarn runtime verification pending  
> Owner: Project lead  
> Version: 0.6  
> Last reviewed: 2026-11-13

## Runtime stack

The project pins Yarn Spinner for Unity to `dev.yarnspinner.unity` v3.2.8. Narrative source is written in `.yarn` files and included by the Unity Yarn project at `Assets/_Project/Dialogue/Greyhaven.yarnproject`.

The Greyhaven scene generator imports the Yarn project, creates a `DialogueRunner`, attaches `GreyhavenRuntimeUI` as its custom `DialoguePresenterBase`, and registers the `checkpoint` and `relationship` commands. Each interaction passes its resident's `NpcAgent` to the presenter. The presenter seeds Yarn's shared relationship variables from that agent; C# remains authoritative for score changes and memory deduplication.

## First interaction loop

1. Bring the vessel to a named shore approach and reduce horizontal speed to 0.85 m/s or below.
2. The HUD displays an E prompt for the nearest accessible dockside location.
3. Press **E** to open the dockside hub, select Harbour Office, then choose SPEAK WITH HARBOURMASTER to start node `Greyhaven_Harbourmaster`.
4. The dialogue panel displays the speaker and line. Press **Space**, **Enter**, or CONTINUE to advance.
5. When Yarn offers choices, they appear as buttons. Click a choice or press **1–5**.
6. Yarn evaluates the selected branch, updates persistent variables, and continues until the node ends.
7. The dialogue panel closes and scaled gameplay time resumes.

The player remains aboard the vessel. The hub presents authored location interiors and furniture actions as a menu; it does not create walkable rooms or an on-foot character controller. The menu closes before dialogue begins so the Yarn presenter can own the dialogue pause and relationship state.

## Implemented Greyhaven resident dialogue

The generated scene exposes four resident conversations through the persistent seven-location dockside hub. The generated NPC trigger components remain available for hand-authored scenes but are marked as externally managed in Greyhaven, preventing duplicate E prompts and competing ownership of the shared Interact action:

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
- NPC movement uses short direct waypoints, not NavMesh pathfinding. Walkable on-foot interaction and physical 3D interiors, animation, voice-over, facial animation and translated localization packages are not connected. Menu-based interior descriptions and furniture actions are implemented by DocksideHubController.
- Dialogue choices update Yarn variables; broader quest state and world changes must be connected through explicit commands/functions and authoritative gameplay systems.
- Repository checks can verify that all four node sources contain the seven relationship tier boundaries, a character quote option and representative daily topics. These checks do not parse or compile Yarn.
- Yarn compilation, line presentation, conditional-option filtering, command dispatch and all seven trust tiers must still be verified in Unity 6000.3.28f1 after package import. Static GitHub C# syntax checks do not compile Yarn or resolve Unity symbols.

## Related systems

- `docs/08_TECHNICAL_DESIGN/GREYHAVEN-DOCKSIDE-INTERACTION-HUB.md`

- `docs/06_GAMEPLAY_SYSTEMS/SAVE-AND-LOAD.md`
- `docs/08_TECHNICAL_DESIGN/SAVE-DATA-STRUCTURE.md`
- `Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/HarbourmasterInteraction.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/YarnDialogueSaveParticipant.cs`

## Greyhaven lore and cross-witness dialogue expansion

The current Yarn source extends all four resident nodes with occupation-specific testimony and personal texture. Four persistent Yarn bools record whether the player has heard the Harbourmaster's bell-log account, Mara's cold-centred catch observations, Tomas's returned-hull measurements, and Inez's account of the empty but untorn net. A comparison topic is conditionally offered after any clue is discovered; it gives a partial response until all four records are known, then carefully distinguishes corroborating observations from proof of a shared cause.

Core evidence is available at neutral-or-better trust and does not award relationship points. Additional personal details remain trust-gated: the Harbourmaster's chart practice, Mara's market philosophy, Tomas's radio-receiver hobby, and Inez's woven-line birds. No new C# command, quest service, NPC identity, save field or schema migration is introduced. The existing Yarn save participant persists the new typed bool variables through the current dialogue save document.

**Verification boundary:** source-level declarations, node placement and flag use are implemented. Yarn compilation, conditional-option behaviour, save/load restoration and in-game dialogue presentation have not yet been tested in Unity 6000.3.28f1.
