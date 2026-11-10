# Data Architecture

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

Use ScriptableObjects for authored definitions: fish, equipment, region profiles, weather, quests, and threat configurations. Do not use shared ScriptableObject assets as mutable per-save state. Copy changing data into runtime models and serialize through explicit versioned DTOs. Use stable IDs and validate references/content fields.
