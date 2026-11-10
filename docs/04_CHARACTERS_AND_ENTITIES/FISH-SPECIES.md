# Fish Species

> Project: WHERE THE SOUNDINGS END  
> Status: Working index — full roster in the catchable seafood catalog  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Authoritative roster

The detailed fish roster is maintained in [Catchable Seafood and Market Species Catalog](CATCHABLE-SEAFOOD-CATALOG.md). That document is the source of truth for the proposed 56 fish entries, scientific names, regional habitat, catch method, rarity, trade tier, protected status, and prototype selection.

## Data requirements

Every implemented fish species needs:
- Stable species ID and display name.
- Scientific name and real-world / fictional status.
- Region, habitat tags, and depth band.
- Temperature / current preference where relevant.
- Activity period, weather affinity, and seasonal window.
- Migration route and ecological role.
- Diet and predator / prey relationships.
- Compatible bait, lure, rod, net, and other gear.
- Size distribution, abundance, and regional stock ID.
- Rarity class, market tier, sale unit, and buyer compatibility.
- Catch difficulty, handling class, and freshness decay profile.
- Minimum retain size, quota, seasonal closure, or protected-status rule where applicable.
- Research source and notes.

## Design rules

1. Do not present fictional creatures as real biological species.
2. A listed fish is not automatically available in every region or season.
3. Catch probability depends on regional stock, habitat suitability, gear, time, tide, weather, and player technique.
4. Selling value depends on quantity, condition, freshness, market demand, and legal eligibility.
5. Protected wildlife and anomalous entities are not ordinary fish-market goods.
6. Regional stock changes through catch, predation, migration, spawning, and recovery. See [Fish Population and Food-Web Model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md).

## Planned content

- 56 ordinary fish entries in the first proposed catalog.
- 21 catchable invertebrate entries in the same market catalog.
- 3 optional managed seaweed harvests, tracked separately from animal catch.
- Protected / release-only wildlife and anomalous research specimens are handled separately.

The roster is proposed and requires final review against the locked world climate, map, and in-game conservation rules. It is not compiled Unity data yet.
