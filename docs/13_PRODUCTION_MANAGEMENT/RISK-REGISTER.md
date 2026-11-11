# Risk Register

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

| Risk | Impact | Mitigation |
|---|---|---|
| Scope creep | Very high | MVP, vertical-slice gate, written trade-offs |
| Solo asset workload | High | Modular reuse, limited unique content |
| Weak fishing loop | Very high | Prototype and playtest first |
| Complex simulation | High | Aggregate data, coarse updates, profile first |
| Unreadable horror | High | Learnable rules, clear tells, fairness tests |
| Performance | High | Target hardware, budgets, profiler |
| Save corruption | Very high | Versioned DTOs, atomic writes, backups, migrations |
| Asset licensing | High | Asset register and audit |
| Burnout | High | Small milestones and realistic scope |
| Commercial uncertainty | Medium/high | Validate audience and store appeal |
| Unity semantic compile gap | Very high | CI syntax checks cannot resolve Unity/package symbols; require a clean import/compile in the pinned editor before accepting M0 |
| Generated project reproducibility | High | Fresh checkout currently lacks scenes, build settings, `.meta` files and a package lock; validate bootstrap and commit stable Unity metadata |
| Missing runtime integration evidence | Very high | Run existing Edit Mode tests, add Play Mode coverage, test save/restore and produce a clean Windows build before expanding scope |
| Main-branch enforcement unavailable/unverified | Medium/high | Current private-repository rulesets API reports a plan restriction; keep PR discipline and make an explicit owner plan/visibility decision |
| Vertical-slice scope expansion | Very high | Limit the first slice to Greyhaven, one NPC/quest arc, one threat and one complete expedition/economy/save loop |
