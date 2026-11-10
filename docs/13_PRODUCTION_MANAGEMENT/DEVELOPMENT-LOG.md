# Development Log

> Project: WHERE THE SOUNDINGS END  
> Status: Active working log  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-11

## 2026-11-11 — Movement-aware ocean camera

**Commit scope:** Upgrade the Greyhaven camera from a position-only fixed-angle follower to a velocity-aware, sea-responsive orthographic camera.

### Implemented
- Added horizontal Rigidbody velocity look-ahead with a hard distance cap. Reverse and drift frame in the actual travel direction.
- Added subtle yaw-rate camera banking with clamped angles and damped rotation.
- Added shared-wave sampling for camera heave, pitch, and roll. The camera samples the same MaritimeSurfaceBehaviour used by dead-calm and wrong-way-swell incidents.
- Made the Greyhaven Editor builder create the surface explicitly and wire it into the camera, while retaining the atmosphere director's existing incident ownership.
- Added Edit Mode tests for movement look-ahead, stationary behaviour, clamping, invalid tuning, and turn-bank direction.
- Updated camera awareness and technical design documentation, plus the root implementation summary.

### Verification
- Repository source and current scene-generation path were inspected before editing.
- GitHub Actions static project and C# syntax validation will run on the push.
- Unity 6000.3.28f1 compilation, Edit Mode test execution, Play Mode feel/readability checks, reduced-motion review, and Windows build remain unverified in this environment.

### Compatibility and risk
- No save schema or persisted identifiers changed.
- Orthographic projection, camera pitch, initial offset, boat Rigidbody ownership, action-driven sea clock, and atmosphere incident scheduling remain unchanged.
- Wave response is a presentation effect only; it does not imply physical buoyancy or wave forces on the prototype boat.
