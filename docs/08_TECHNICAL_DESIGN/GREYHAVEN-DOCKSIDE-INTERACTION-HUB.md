# Greyhaven Dockside Interaction Hub

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime menu implementation authored; Unity verification pending  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-13

## 1. Player promise

Greyhaven remains a boat-only prototype. The player does not disembark or control an on-foot avatar. From a stationary vessel inside a location's shore-approach radius, the player presses **Interact (E)** to open a location menu. The menu presents a stylised interior description and a list of furniture/object interactions.

The interior is a menu presentation, not a rendered walkable room or a separate streamed scene. Building shells and resident props remain part of the existing generated greybox. This deliberately avoids adding a second movement/camera mode or loading extra interior scenes.

## 2. Access and state

DocksideHubController lives in the persistent Greyhaven scene, not in a streamed static area root.

A location can open when all of the following are true:

- The vessel is within the location's horizontal X/Z access radius.
- Horizontal Rigidbody speed is at or below 0.85 m/s.
- The location definition and access point are valid.
- The hub is not already open and another exclusive gameplay action is not active.

The access policy rejects NaN/infinite values, negative speed/radius and invalid thresholds. Vertical separation is intentionally ignored because the authored points are shore-approach references, not walkable floor coordinates.

Opening the menu claims the existing SeaStateDirector interaction gate and sets Time.timeScale to zero. Closing it restores the exact prior time scale. The menu also closes before starting Yarn dialogue, allowing the existing dialogue presenter to acquire its own pause and relationship state. The boat is therefore stationary while browsing; no movement or action-driven world time can continue behind the panel.

The hub is the only input owner for the generated Greyhaven resident interactions. HarbourmasterInteraction remains available for hand-authored scenes, but the generated Greyhaven NPC triggers are marked as externally managed to prevent duplicate E-key prompts or simultaneous dialogue starts.

## 3. Locations and furniture

| Location | Menu objects | Implemented interaction |
|---|---|---|
| Main Dock | Tide Register, Harbour Chart, Cargo Hatch | Read current action-clock/sea-state report; review the fixed settlement chart; inspect carried cargo |
| Harbour Office | Tide-Log Desk, Channel Chart, Noticeboard | Read sea-state report, bearings and harbour notices; speak with Harbourmaster |
| Fish Market | Weighing Scale, Catch Tally, Sample Jars | Inspect cargo item names, quantities, freshness and aberrant/quest flags; read Mara's grading notes; speak with Mara |
| Boat Repair Shed | Engine Workbench, Tool Wall, Spare Propeller | Inspect hull/engine condition and last damage report; read Tomas's engine notes; speak with Tomas |
| Net Mender Shed | Net Loom, Knot Board, Thread Spools | Read net-care guidance and inspect supply display; speak with Inez |
| Harbour Inn | Guest Ledger, Shared Table, Watch Lantern | Request a checkpoint through the existing autosave service; read local rumours and watch status |
| Western Lighthouse | Beacon Mechanism, Keeper's Log | Inspect beacon presentation status and read a local navigation note |

The menu's location list can be used from a valid dockside access point. Each resident action still checks the existing NpcAgent.CanTalk gate and starts the resident's existing Yarn node with that exact NpcAgent. Stable NPC IDs, schedules, relationship scores, memory ledgers and save DTOs are unchanged.

Furniture actions are deliberately honest about incomplete gameplay systems:
- Market inspection is not a sale. No item is removed and no currency is granted.
- Repair inspection is not a repair. No free hull/engine changes are applied.
- Net notes do not craft equipment or create a net-material inventory.
- The lighthouse is not a player-operated beacon.
- The Inn guest ledger uses the existing checkpoint autosave; it does not introduce a second save format.
- Residential houses are not represented as visitable interiors because no residents or visit actions are assigned there yet.

These actions provide functional inspection, authored information, existing resident dialogue, and a real autosave checkpoint without fabricating an economy, crafting, repair-cost, sleep or quest system.

## 4. Runtime ownership

- DocksideHubController owns proximity eligibility, the E-key entry point, menu presentation, temporary UI pause and furniture action routing.
- DocksideAccessPolicy is a pure deterministic access rule covered by Edit Mode tests.
- GreyhavenRuntimeUI remains the Yarn presenter and relationship-meter owner.
- NpcAgent remains the authoritative resident identity, schedule, trust and memory owner.
- SpatialCargoHold remains the cargo source of truth.
- VesselConditionController remains the hull/engine condition source of truth.
- SeaStateDirector remains the action-driven clock and sea-state source of truth.
- AutosaveController remains the checkpoint persistence owner.
- GreyhavenEnvironmentBuilder generates the persistent hub UI and binds existing NPC/gameplay components. Static streamed area roots remain script-free and Rigidbody-free.

No save schema, stable ID, input action, package or generated-area-root contract changes.

## 5. Verification and remaining work

Automated Edit Mode tests cover stationary access, distance rejection, speed rejection, horizontal-only range, and invalid numeric input. Repository CI can check project contracts and C# syntax, but it cannot prove Unity semantic compilation, UI rendering, input focus, generated-scene wiring, or a playable dock interaction.

After pulling the branch, open Unity 6000.3.28f1, run **Tools > WHERE THE SOUNDINGS END > Rebuild Greyhaven Environment**, then verify:
1. The generated Dockside_Interaction_Hub UIDocument is present in the persistent scene and renders above the field HUD.
2. E opens the menu only while the vessel is stationary and within an access radius.
3. Escape closes the menu and restores the prior time scale.
4. Each location and furniture item renders without clipping at supported resolutions.
5. Resident buttons start the correct Yarn node and display the correct relationship meter.
6. Guest Ledger writes through the existing autosave coordinator.
7. Inventory, fishing, dredging, sounding, dialogue and streaming continue to use their existing exclusive action and save rules.

The menu is a functional greybox UI, not final interior art. Final illustrated/3D interiors, trade transactions, paid repairs, net equipment, sleep/time-skip, residential visits, accessibility review and controller-focused menu navigation remain future work.
