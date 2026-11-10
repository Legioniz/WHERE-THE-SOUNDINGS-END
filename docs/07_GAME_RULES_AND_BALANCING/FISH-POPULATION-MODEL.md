# Fish Population Model

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

Start with aggregate stock units by species and region. Use bounded logistic recovery N(next)=clamp(N+r*N*(1-N/K)*dt-catch-mortality,0,K), or compare with a simpler linear recovery model. Habitat suitability may combine depth, temperature, current, time, and weather factors in [0,1]. Migration moves stock between authored neighboring zones. Track stock trajectory, catch per trip, effort per catch, zero-catch trips, rare species rate, and recovery time. Test heavy-fishing, no-fishing, and mixed scenarios.
