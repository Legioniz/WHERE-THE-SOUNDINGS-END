# Balancing and Simulation

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

Use Python for Monte Carlo sampling, parameter sweeps, CSV output, and plots. Use Unity EditMode tests to validate runtime rules. Measure catch distributions, zero-catch rate, rare-catch frequency, stock recovery, net trip profit, time-to-upgrade, encounters/hour, simultaneous threats, lethal-event frequency, quiet intervals, signal false-positive/negative rates, region unlock time, and campaign completion distributions. Run many reproducible seeds and several player profiles; report medians, percentiles, and failure cases, not only averages. Keep offline and runtime formulas aligned.

## 2. Population and overfishing simulation matrix

Population tests must cover both the local fishing ground and the island-wide aggregate. Run reproducible time-series simulations rather than checking a single end-state.

### 2.1 Required scenarios

| Scenario | Duration | Main question |
|---|---:|---|
| No fishing | 30 / 90 / 180 / 360 game days | Does the stock approach carrying capacity without exceeding it? |
| Light effort | 90 days | Can the population replace ordinary harvest? |
| Heavy effort, one ground | 30 / 90 days | Does local catch rate and breeding stock decline while connected grounds remain healthy? |
| Heavy effort, every ground | 90 / 180 days | Can the player cause a regional depletion event? |
| Adult-selective gear | 90 / 180 days | Does spawning-stock biomass and later recruitment fall? |
| Juvenile-selective gear | 90 / 180 days | Does the later adult cohort weaken after a maturation delay? |
| Recovery after fishing stops | 30 / 90 / 180 / 360 days | Do fast-, medium-, and slow-cycle species recover at different rates? |
| Nursery habitat damaged | 90 / 180 days | Does poor habitat slow recruitment even when adult stock survives? |
| Migration into depleted patch | 30 / 90 days | Does the source lose exactly what the destination gains? |
| Save/load and sleep fast-forward | Multi-month | Do all daily updates run once and produce deterministic results? |

### 2.2 Metrics

Record daily abundance, biomass, spawning-stock biomass, juvenile/adult ratio, mean size, recruitment buffer, fishing removals, natural production, predation, migration, habitat quality, vulnerable stock ratio, catch multiplier, zero-catch probability, average landed weight, net revenue per hour, and time to recover.

### 2.3 Failure conditions

- An ordinary short trip causes an unrecoverable island-wide extinction.
- A player can restore a slow deepwater population by sleeping for only a few days.
- Daily growth creates new individuals instead of adding body mass.
- An unlimited juvenile supply replaces all harvested adults immediately.
- A heavily depleted ground returns to full stock on scene reload.
- The player's catch is subtracted from every ground in a region.
- Migration adds stock to the destination without subtracting it from the source.
- Fishing pressure remains labelled healthy while removals exceed natural production for a prolonged period.
- An apparently healthy stock has poor catch rates because the same abundance penalty is applied twice.
- The player can farm a dynamic scarcity price premium and make overfishing more profitable than rotating grounds.

The formulas and acceptance criteria are authoritative in [Fish Population and Food-Web Model](FISH-POPULATION-MODEL.md).
