# Seasonal Maritime Events

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed game calendar — non-canon  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Design intent

A recurring calendar can make the ocean feel inhabited and changing. The calendar below is a **fictional gameplay schedule**, not a claim that these events occur in the same months across real-world cultures or oceans.

## Proposed event schedule

| Game season | Candidate event | Gameplay use | Trigger / caution |
|---|---|---|---|
| Early warm season | Migratory call | Whale-song navigation, seasonal fish movement | Use a regional schedule established by the fictional ecology |
| Wet / storm season | The Long Weather | Reduced visibility, stronger currents, harbour preparation | Forecast and warnings must make severe conditions legible |
| Midyear turning | The Bell Tide | A submerged bell or structure can be heard under rare tide/weather conditions | Do not assume a universal solstice ritual |
| Late warm season | Green Water | Plankton bloom changes water color and fish distribution | Do not equate every bloom with toxicity |
| Cooling season | The Long Return | Migratory fish and lost boats create new clues | Balance availability and fishing pressure |
| Dark season | The Quiet Water | Reduced ambient activity makes acoustic cues more prominent | Maintain safe, readable navigation cues |
| Rare alignment | The Second Sounding | A deep signal becomes detectable during a specific conjunction of time, tide, and weather | The trigger must be discoverable and not depend on real-world astrology |

## Calendar rules

- All schedules are authored in the game's own calendar and local region data.
- A quest may be available over a window, not only on one exact frame or one real-world date.
- The player should be able to learn likely windows from NPC dialogue, charts, observation, or prior records.
- A missed event should recur or have a meaningful alternate route unless the narrative explicitly requires a one-time consequence.
- Do not use random seasonal gates to block main-story progression.
- Event timing must be deterministic for a save file and testable with a fixed simulation seed.

## Prototype target

Start with only two event states: ordinary conditions and one special acoustic event. Add a full seasonal calendar only after the core day/night, weather, fish migration, and save/load systems are stable.

## Related design

- [World Bible](WORLD-BIBLE.md)
- [Living Current](../06_GAMEPLAY_SYSTEMS/LIVING-CURRENT.md)
- [Weather System](../06_GAMEPLAY_SYSTEMS/WEATHER-SYSTEM.md)
- [Fog and Visibility](../06_GAMEPLAY_SYSTEMS/FOG-AND-VISIBILITY.md)
- [Maritime Mysteries quests](../03_STORY_AND_NARRATIVE/SIDE_QUESTS/MARITIME_MYSTERIES/README.md)
