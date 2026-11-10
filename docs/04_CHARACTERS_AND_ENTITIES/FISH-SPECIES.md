# Fish Species

> Project: WHERE THE SOUNDINGS END  
> Status: Working index — full roster in the catchable seafood catalog  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-10

## Authoritative roster

The complete proposed roster is maintained in [Catchable Seafood and Market Species Catalog](CATCHABLE-SEAFOOD-CATALOG.md). That document is the source of truth for species IDs, scientific names, regional habitat, depth range, catch method, rarity, trade tier, protection rules, and prototype selection.

## Proposed catalog size

- **100 fish entries:** freshwater river fish, estuary and mangrove fish, reef and rocky-coast fish, sandy-shelf demersal fish, coastal pelagic schools, offshore pelagic fish, and upper deep-slope snappers / jobfishes.
- **47 invertebrate entries:** shrimp, prawns, crabs, lobsters, squid, cuttlefish, clams, oysters, conchs, sea cucumbers, jellyfish, and scallops.
- **8 seaweed / algae records:** three broad market categories plus five species-level cultivation or gathering records. Broad categories are not duplicate stock species.
- Protected / release-only wildlife and anomalous research specimens remain separate from ordinary market catches.

## Data requirements

Every implemented species needs:
- Stable species ID and display name.
- Scientific name and real-world / fictional status.
- Region, habitat tags, and depth band.
- Temperature / current preference where relevant.
- Activity period, weather affinity, and seasonal window.
- Migration route and ecological role.
- Diet and predator / prey relationships.
- Compatible bait, lure, rod, net, trap, jig, and other gear.
- Size distribution, abundance, and regional stock ID.
- Rarity class, market tier, sale unit, and buyer compatibility.
- Catch difficulty, handling class, and freshness decay profile.
- Minimum retain size, quota, seasonal closure, or protected-status rule where used.
- Research source and notes.

## Design rules

1. Do not present fictional creatures as real biological species.
2. A listed fish is not automatically available in every region or season.
3. Catch probability depends on regional stock, habitat suitability, gear, time, tide, weather, and player technique.
4. Deep-slope fish are not the same as hadal-trench fauna. Routine deep-drop fishing is limited to authored upper-slope stations and suitable depths.
5. Freshwater species stay in Greyhaven's upper river and freshwater reaches; they do not spawn in saltwater reef zones.
6. Selling value depends on quantity, condition, freshness, market demand, and legal eligibility.
7. Protected wildlife, controlled harvests, and anomalous entities are not ordinary fish-market goods.
8. Regional stock changes through catch, predation, migration, spawning, and recovery. See [Fish Population and Food-Web Model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md).

The roster is proposed and requires final review against the locked world climate, map, and in-game conservation rules. It is not compiled Unity data yet.
