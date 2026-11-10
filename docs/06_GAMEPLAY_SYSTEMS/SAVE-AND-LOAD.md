# Save and Load

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

Use versioned save data: schema version, save ID, timestamp, game time, player, vessel, equipment, cargo, discoveries, map/journal, quests, NPC state, regional stocks, world flags, event cooldowns, and RNG states where deterministic continuation matters. Use stable IDs. Implement atomic writes, backup rotation, validation, corruption handling, and migration tests.
