# Progression System

> Project: WHERE THE SOUNDINGS END  
> Status: Regional chart-discovery foundation implemented; wider campaign progression remains partial  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## 1. Progression principles

Progression should come from what the player has actually done and learned. The game must distinguish:
- **Unknown:** the vessel has never entered the region's authored survey bounds. Its name, position and map marks remain hidden.
- **Visited / chart unlocked:** the vessel physically entered the region. The region is recorded in the logbook and becomes selectable as a known chart entry.
- **Site discovered:** the Sounding Rig has received an authored site's signal. Only then may that site be marked on the chart.
- **Inspected:** the vessel approached and inspected the site, adding the existing field-note record. Discovery and inspection remain separate actions.

The first regional unlock is automatic on physical entry. It is not granted by merely opening the map, reading a global region list or loading a test scene.

## 2. Regional discovery progression

RegionalChartController resolves the active DeepOceanSurveyRegion containing the vessel. On first entry, it records the stable region ID in WorldSaveData.visitedRegionIds and ProgressSaveData.unlockedRegionIds. Repeat entries are idempotent. The chart UI only lists visited regions.

The chart does not reveal a locked region's name, silhouette, marker, compass bearing or route. No unknown site can appear as a question-mark pin. A region's local plot begins without site marks; the Sounding Rig adds a mark only after its stable site ID appears in the discovery set.

The isolated Outer Shoals and Hollow Trench scenes remain test ranges. Chart unlocking does not implement regional sailing, scene travel, route gates or a world-scale map.

## 3. Existing progression state

SaveGameData.ProgressSaveData already owns discovered journal entries, unlocked region IDs, quests, journal records, NPC state, regional stock, world flags, cooldowns and deterministic random streams. The regional chart uses the existing unlockedRegionIds list and adds the separate visitedRegionIds world record. It does not repurpose journal discoveries or NPC relationship values.

Quest progression, economy upgrades, equipment unlocks, region travel and campaign gates remain incomplete. Add them as explicit stable-ID progression records when their gameplay owners exist; do not infer them from UI visibility.

## 4. Save compatibility

Save schema v7 adds WorldSaveData.visitedRegionIds. The v6-to-v7 migration creates an empty visited list and preserves any existing unlockedRegionIds. Existing saves therefore do not reveal regions that were never physically visited. The chart controller validates and persists its own records through ISaveParticipant / ISaveDataValidator.

See [Regional Chart and Discovery](REGIONAL-CHART-AND-DISCOVERY.md) and [Save Data Structure](../08_TECHNICAL_DESIGN/SAVE-DATA-STRUCTURE.md).

## 5. Future progression work

- Authored discovery requirements for regions that should not unlock on simple boundary entry.
- NPC rumours and radio transmissions that provide uncertain leads without revealing exact positions.
- Chart upgrades that improve scale/precision only after the player earns reliable evidence.
- Quest stages that require independently verified evidence, not a single sonar ping.
- Region travel and route planning after a connected world coordinate system exists.
- Journal, quest and chart UI integration with clear unknown / suspected / discovered / inspected states.

Do not add compass guidance for unknown destinations as a convenience feature. Uncertainty is an intentional part of navigation and exploration.
