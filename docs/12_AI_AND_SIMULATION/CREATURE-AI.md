# Creature AI and Threat Behaviour

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## 1. Scope and design rule

The game is not populated by a monster behind every mystery. Only entities explicitly tagged **Hostile** may pursue or attack the player. Acoustic anomalies, ruins, natural hazards, rescue signals, and wildlife use event or environmental systems. This distinction prevents every side quest from becoming the same chase encounter.

Use a hierarchical finite-state machine (HFSM) for the first implementation. The Threat Director schedules *opportunities*; each threat owns local sensing, state transitions, movement, attack timing, and disengagement. Use authored waypoints and bounded steering for open-water creatures. Do not use NavMeshAgent for a free-swimming ocean threat unless it is moving on actual walkable geometry. Unity's Physics queries can perform filtered line-of-sight checks; use layer masks and explicit trigger-query behavior (Unity 6.0.6 Physics.Raycast reference: https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/physics/raycast).

## 2. Entity classifications

| Classification | Can detect player? | Can chase? | Can attack? | Examples |
|---|---:|---:|---:|---|
| Hostile creature | Yes | Yes, bounded | Yes | The Sleeper |
| Curious wildlife | Yes | Briefly / follow | No | Whale, fish school |
| Acoustic anomaly | Reacts to signals | No physical chase | Indirect hazard only | The Voice, The Reef |
| Environmental hazard | Uses position / weather / tide | No AI chase | Damages boat through physics | Channel current, collapsing ruin |
| Human rescue subject | Scripted interaction / state | No | No | Missing deckhand |
| Historical event | Time / tide / quest flags | No | No | Bell, submerged city |

## 3. Common threat state machine

`Dormant → Investigating → Warning → Stalking → Pursuing → Attacking → Recovering → Disengaging → Cooldown`

Not every threat uses every state. A creature may move from Investigating to Disengaging without ever attacking.

| State | Purpose | Allowed transition |
|---|---|---|
| Dormant | Wait in authored habitat | Investigating when a valid cue is sensed |
| Investigating | Approach a sound or disturbance, not necessarily the player | Warning / Dormant / Disengaging |
| Warning | Communicate presence and danger | Stalking / Disengaging |
| Stalking | Shadow the target and learn its movement | Pursuing / Disengaging |
| Pursuing | Attempt to close distance or intercept | Attacking / Stalking / Disengaging |
| Attacking | Execute one telegraphed attack | Recovering |
| Recovering | Commit to attack recovery; do not instantly chain hits | Pursuing / Disengaging |
| Disengaging | Break contact and return to a valid patrol region | Cooldown |
| Cooldown | Prevent immediate re-trigger | Dormant |

Transitions must be driven by explicit conditions and timers. No state may randomly teleport an entity next to the player.

### 3.1 Life behaviour is separate from threat behaviour

The threat state machine above governs a creature's relationship to the player during a potential hostile encounter. It is not the creature's complete daily life.

A creature may independently be active, resting, sleeping, feeding, hunting ordinary prey, recovering from injury, migrating, sheltering, or breeding. Those long-term ecological states are owned by the life-cycle controller. Short-term actions such as wandering, patrolling, stalking prey, or fleeing are tactical choices. Player-directed pursuit and attacks remain owned by the ThreatBrain.

For example, a shark-like predator may wake, leave a resting site, hunt a fish school, feed, and return to deeper water. This is ordinary feeding behaviour and must not automatically trigger a player encounter. The Sleeper's hunt of the boat is a distinct hostile profile.

Use the three-layer model and needs / schedule rules in [Creature Life Cycles and Ecological Behaviour](CREATURE-LIFE-CYCLES-AND-ECOLOGY.md). Do not duplicate its life-state machine inside this threat-state document.

## 4. Perception model

### 4.1 Visual sensing

For a threat with a forward direction, first test range, then field of view, then occlusion.

- Prototype total visual FOV: **100°** (50° either side).
- Prototype clear-water range: **70 m**.
- Fog / darkness modifies effective range; see [Fog and Visibility](../06_GAMEPLAY_SYSTEMS/FOG-AND-VISIBILITY.md).
- Visual checks occur at 4 Hz while the player is in a nearby active region, not every rendered frame.
- If a threat is large and deep-water adapted, sight may be limited; it relies more heavily on sound and pressure cues.

```csharp
Vector3 toTarget = (target.position - eye.position).normalized;
float dot = Vector3.Dot(eye.forward, toTarget);
bool insideCone = dot >= Mathf.Cos(halfFovDegrees * Mathf.Deg2Rad);
bool inRange = distance <= effectiveVisualRange;
bool clearLine = !Physics.Raycast(
    eye.position, toTarget, distance, occlusionMask,
    QueryTriggerInteraction.Ignore);
bool visuallyDetected = insideCone && inRange && clearLine;
```

Use separate layers for terrain, boat hulls, water-surface blockers, and harmless trigger volumes. Do not let decorative particles block sight.

### 4.2 Player visibility score

The AI does not see a binary target through all fog. Calculate a visibility score:

[
V = C \, L \, M \, O \, F
]

Where:
- (C = max(0, 1-d/R)), distance falloff.
- (L) = target contrast / lighting factor (0–1.25).
- (M) = movement exposure factor (0.65 idle/slow to 1.2 fast wake).
- (O) = occlusion (0 for fully blocked, 1 unobstructed).
- (F) = fog transmission (0–1).

Detection begins only when (V) exceeds the threat's threshold for the required dwell time. A single one-frame visibility spike does not acquire the player.

### 4.3 Acoustic sensing

Sound can be detected outside visual FOV. Every emitted sound creates an event:

`SoundEvent { sourceId, worldPosition, band, loudness, timestamp, eventType }`

Prototype received level:

[
A_r = rac{A_s \, W \, B \, O}{1+(d/D)^2}
]

- (A_s): source loudness (0–1).
- (W): water / weather propagation factor.
- (B): entity sensitivity to the sound band.
- (O): acoustic occlusion / masking factor (0–1).
- (d): distance.
- (D): authored attenuation distance.

Each threat has a minimum received level. Engine noise is persistent; a Sounding Rig ping is brief and distinct; collision and horn events are high-amplitude. A creature first investigates the *source location*, not the player's current position.

### 4.4 Memory and uncertainty

A threat stores:
- last seen position and timestamp;
- last heard position and sound category;
- confidence of current target estimate;
- last attack time;
- time since last direct detection.

Confidence decays over time. If the player moves after the last sound, the threat searches the last known point and likely escape routes; it does not know the player's exact location.

## 5. Movement and chase rules

For an ocean creature, use a 3D steering controller, not humanoid pathfinding:
1. Select a target point on a permitted water volume.
2. Predict target position by a short horizon (1.0–2.0 s).
3. Clamp turn rate and acceleration.
4. Avoid shore, rocks, and shallow regions using authored avoidance probes.
5. Do not cross into safe harbour or no-threat volumes.
6. Re-evaluate the path at 2–4 Hz.

Prototype tuning for The Sleeper:
- Stalking speed: 2.8–4.0 m/s.
- Pursuit speed: 6.5–7.5 m/s.
- Maximum pursuit duration: 12 s before reassessment.
- Minimum turn radius: 12–18 m.
- Break-off distance: 90 m, safe boundary, or loss of target confidence.
- After a strike: at least 6 s of attack recovery; no immediate second strike.
- Never accelerate instantaneously to match the boat.

These are initial game-feel values, not measured marine-animal physics. Tune against boat speed and route length.

## 6. Attack contract

An attack must have four readable stages:

1. **Tell:** a directional audio cue, wake, shadow, rig distortion, or hull vibration.
2. **Commit:** the creature chooses a collision lane; player has time to react.
3. **Impact:** one damage event with a defined damage budget.
4. **Recovery:** the creature passes, turns, or retreats; the player regains control.

Prototype strike:
- Telegraph: 2.2–3.0 s.
- Damage: 12–18% of current starter-boat hull integrity.
- Secondary effect: temporary steering instability or one equipment fault, never both at once.
- Attack cooldown: 18–24 s.
- Safety check: do not strike if player is already in a guaranteed lethal state, inside a safe zone, during a cutscene, or while input is locked by another interaction.

The attack should create a decision, not instantly end a voyage. A missed tell may still damage the boat, but the damage range and escape route must be communicated.

## 7. Counterplay and fairness

Valid counterplay includes:
- stop or reduce engine noise;
- stop repeated Sounding Rig pings;
- change heading after the creature commits;
- use a known current corridor;
- break visual contact with a rock formation or fog bank when acoustically quiet;
- use a one-time defensive boat tool if that feature is later approved.

Do not make darkness, silence, or a particular upgrade a guaranteed universal solution. The player should learn the creature's rule and choose a response.

## 8. Implementation architecture

Suggested Unity components:
- `ThreatBrain`: state machine and transition rules.
- `ThreatPerception`: visual cone, occlusion and sound-event intake.
- `ThreatMemory`: last-known data and confidence decay.
- `ThreatMotor`: acceleration, turn rate, water-volume constraints.
- `ThreatAttack`: telegraph, collision, damage, cooldown.
- `ThreatAudioEmitter`: state-driven audio events.
- `ThreatDefinition` ScriptableObject: tuning and references only.
- `ThreatRuntimeState`: mutable per-instance state; never store runtime state in a shared ScriptableObject.

Use fixed simulation steps for physical movement and throttled sensing for AI. Profile before adding jobs or complex acoustic simulation.

## 9. Acceptance tests

- A threat outside FOV can still investigate an audible ping.
- A wall / rock occludes visual sensing.
- Fog shortens visual detection without disabling audio sensing.
- Losing sight does not reveal the player's exact new position.
- A threat cannot attack before its telegraph.
- A threat cannot chain attacks inside its cooldown.
- A threat cannot enter safe harbour.
- Two runs with the same seed and event history produce the same event choice.
- Non-hostile quest phenomena never call the boat-damage API.
