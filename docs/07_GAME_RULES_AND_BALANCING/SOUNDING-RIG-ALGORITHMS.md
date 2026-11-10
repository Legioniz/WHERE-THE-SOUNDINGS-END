# Sounding Rig Algorithms

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

Pipeline: deploy → emit pulse → generate returns → apply distance attenuation/noise/occlusion → extract delay/amplitude/frequency/repetition features → compare candidate profiles → return ranked hypotheses/confidence → apply entity reactions → record observation. Use authored signal profiles and a modest noise model first. A normalized feature-distance score is enough for a prototype; Bayesian updating may follow later. Do not attempt full wave acoustics. Test noise, distance, occlusion, false positives, and reactive anomalies.
