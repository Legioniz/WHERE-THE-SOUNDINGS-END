# Action-Driven Sea State, Panic and Harvesting

> Project: WHERE THE SOUNDINGS END  
> Status: Greyhaven prototype runtime implementation  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Action-driven clock

The sea clock is not a background timer. It advances only when the vessel is moving above the configured horizontal-speed threshold or a harvest action is active.

- Initial time: 07:00.
- Time scale: 72 game seconds per real second.
- Full 24-hour cycle: approximately 20 real minutes.
- Night: 19:00–05:00.
- Full stop: freezes time.
- Cargo interface and Yarn dialogue: freeze time.
- Fishing and dredging: advance time for the duration of their timing challenges.

The clock uses scaled gameplay delta, so a pause state cannot accidentally advance world time. It persists elapsed game seconds and time-of-day seconds in the versioned save.

## Panic

Panic is a 0–1 psychological-pressure value. It is not hunger, oxygen or a conventional survival bar.

- Night increases panic while time is advancing.
- Heavy fog adds pressure.
- Daylight gradually reduces panic.
- The sheltered Greyhaven harbor reduces panic faster during daylight.
- Panic persists through save/load.

Panic changes environmental presentation: fog density/colour and directional-light intensity. At high panic, non-colliding phantom reef shapes and leviathan-scale shadows can appear. At extreme panic, a phantom seabird event can steal one ordinary fish from the cargo hold after an omen. It cannot steal quest cargo or equipment. These hallucination visuals are not authoritative collision objects; actual threats remain the responsibility of creature/encounter systems.

AudioSources present when the director initializes have their pitch and volume warped by night and panic. Authored score, ambience, mixer snapshots and dedicated threat cues are not yet connected.

## Fishing

Press F while the boat is stopped or drifting slowly to begin a three-beat circular timing challenge. Press Space or Enter as the moving needle enters the highlighted arc.

The number of successful beats determines catch quality. The result is selected from the authored fish catalogue and inserted into the spatial hold through `TryPlaceFirstFit`. If the hold cannot fit the catch, the catch is lost; existing cargo is never rearranged.

Aberrant variants have distinct stable IDs and item definitions. Their prototype sale-value multipliers are metadata only until the market transaction system is implemented. Aberration probability rises with panic and sea disturbance, capped at 38%.

## Dredging

Press G within the interaction radius of a wreck/debris field while the boat is stopped. The Greyhaven outer-channel wreck site is the first authored site.

- Four timing beats.
- Three pulls per in-game day.
- Daily yield refreshes using the sea clock's day number.
- Results include waterlogged timber, salt-stiff canvas and corroded ship fittings.
- Each pull is consumed when the action begins, so a failed timing attempt still costs an opportunity.
- Remaining daily pulls persist by stable salvage-site ID.

## Vessel damage and shared grid

Engine, handline rod and deck lantern are installed in the same 7×9 grid as saleable cargo. They occupy cells and can be lost to impacts.

A sufficiently hard collision:
1. Reduces hull integrity and engine condition based on impact speed.
2. Converts the local impact point to a hold-grid cell.
3. Destroys the complete non-protected placement occupying that cell.
4. Marks the cell blocked until repaired.
5. Reduces vessel speed as engine condition deteriorates.

Quest-critical or explicitly non-damageable items cannot be destroyed by this path. Damaged cells, cargo placements, hull and engine condition persist through save/load. A repair API exists; repair-shop UI, material costs and equipment replacement are not yet implemented.

## Known prototype boundaries

- Fishing species are selected from the Greyhaven catalogue without habitat, depth, season or bait filtering yet.
- Fishing currently uses a shared timing challenge; rod type does not yet alter the minigame.
- Dredging is limited to a single wreck site and three daily pulls.
- Collision damage is a greybox rule and needs tuning against actual Unity physics.
- Hallucination events are visual prototype events; they do not replace the full Threat Director or legendary encounter scheduler.
- Audio response is a source-level hook, not a completed soundscape.
- Time, panic, vessel condition, cargo damage and salvage depletion are saved, but require in-Editor load/reload testing.
