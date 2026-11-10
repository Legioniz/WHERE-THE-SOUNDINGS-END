# Probability and RNG

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

Filter candidates by hard eligibility; assign non-negative weights; define a no-candidate fallback; draw at a meaningful event boundary; record candidate set, weights, seed/stream, and outcome in debug builds. Use independent seeded streams for world generation, fishing, threat decisions, and ambient events when reproducibility matters. Every chance declares its time/attempt unit. Tests cover normalization, zero/invalid weights, empty lists, deterministic seeds, and empirical distribution tolerance.
