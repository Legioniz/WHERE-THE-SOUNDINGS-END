# Dialogue authoring

The project uses Yarn Spinner for Unity as its dialogue authoring/runtime package.

## Package

Pinned in Packages/manifest.json:

- `dev.yarnspinner.unity` — Yarn Spinner for Unity v3.2.8
- Upstream: https://github.com/YarnSpinnerTool/YarnSpinner-Unity

Unity 6 is supported by Yarn Spinner 3.x (Unity 2022.3 or later is the documented minimum). Package import requires Unity Editor access and Git installed locally when using the Git URL.

## First import

1. Open the project with the pinned Unity Editor version.
2. Allow Package Manager to resolve the Yarn Spinner Git dependency.
3. In the Project window, create a Yarn Project asset using the Yarn Spinner asset creation menu.
4. Add files from `Assets/_Project/Dialogue/Source/` to that Yarn Project.
5. Add a Dialogue Runner to the Greyhaven scene and assign the Yarn Project.
6. Add `YarnDialogueSaveParticipant` to the same persistent dialogue-system GameObject and assign that Dialogue Runner.
7. Start the `Greyhaven_Harbourmaster` node through the Dialogue Runner when the harbourmaster interaction is wired.

The Yarn Project asset and scene references must be generated/assigned in Unity; they are intentionally not fabricated as hand-written Unity YAML.

## State and persistence

Use stable, namespaced Yarn variable names such as `$gh_met_harbourmaster`. The `YarnDialogueSaveParticipant` exports Yarn's typed float/string/bool dictionaries into the existing SaveGameData document. It does not write a second dialogue save file.

Yarn smart variables are derived values and are not stored; persist the underlying state variables instead. A save taken during an active conversation stores variables but does not resume a partially displayed line. For resumable conversations, save a stable node/checkpoint ID and explicitly restart that node after the scene and dialogue runner are ready.

## Writing rules

- Keep dialogue in `.yarn` files, not C# string literals.
- Use stable node names and variable names.
- Keep story flags separate from presentation-only state.
- Use commands/functions for world mutations; never let dialogue directly own authoritative inventory, currency, quest or world state.
- Give every choice a meaningful consequence, information reveal, or deliberate character signal.
- Keep localization keys stable and avoid embedding language-specific logic in C#.
