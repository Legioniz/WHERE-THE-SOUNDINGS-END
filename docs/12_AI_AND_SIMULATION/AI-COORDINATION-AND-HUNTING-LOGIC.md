# AI Coordination and Hunting Logic

> Project: WHERE THE SOUNDINGS END  
> Status: Research-informed design specification  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10  
> Scope: Design and implementation guidance; not executable Unity code

## 1. Purpose and design target

This document defines how intelligent threats should sense, investigate, stalk, test, pursue, attack, coordinate, and disengage. The aim is not to make every creature aggressive. The aim is to make each creature appear to have motives, limitations, memory, and a distinct way of interacting with the player.

The core design principle is:

**A creature should act on what it believes, not on information the game knows.**

A threat may hear an engine or a Sounding Rig pulse, investigate the source, lose the trail, search an old position, or infer a likely escape route. It must not receive the player's live transform after losing perception. This makes its intelligence legible and gives the player room to misdirect it.

Only explicitly hostile entities may use the full pursuit and attack pipeline. Wildlife, acoustic anomalies, environmental hazards, and historical events must not become interchangeable chase encounters.

## 2. Research basis and limits

The architecture adapts established techniques rather than trying to reproduce animal cognition literally:

- Craig Reynolds' steering-behaviour model separates goal selection from steering and locomotion. It describes useful behaviours such as pursuit, evasion, obstacle avoidance, and group motion. This is the foundation for the creature motor, not a complete decision system.
- Pursuit-evasion research in partially observable environments demonstrates why limited sensing, obstacles, and restricted information sharing matter. Team members should not behave as though they possess perfect shared coordinates.
- Recent multi-agent pursuit studies identify useful coordinated actions such as engaging, flanking, driving, ambushing, chasing, and intercepting. These actions are adapted here as authored tactical options, not copied as a machine-learning controller.
- Research on cooperative pursuit with communication constraints reinforces that communication topology and stale information affect coordination. Our creatures therefore share compact, delayed belief updates rather than a perfect live target feed.

These studies use simplified mathematical or robotic environments. They do **not** prove that a fictional sea creature would behave this way, and their numerical results must not be transplanted directly into the game. The behavioural timings and thresholds below are prototype design values to be tuned in playtests.

## 3. Recommended architecture

Use a hybrid architecture:

| Layer | Responsibility | Owns |
|---|---|---|
| Threat Director | Encounter eligibility, event pacing, cooldowns, safe-zone and spawn validation | Whether an encounter may begin |
| Hierarchical state machine (HFSM) | High-level behavioural phase and legal transitions | What phase the creature is in |
| Utility evaluator | Chooses between valid actions inside a phase | Which action best serves its current motive |
| Perception | Visual, acoustic, wake, light, and disturbance evidence | What the creature can sense |
| Belief / memory | Last-known position, confidence, age, and source reliability | What the creature currently believes |
| Steering motor | Pursuit, circling, interception, obstacle avoidance, current compensation | How the creature moves |
| Coordination layer | Limited information exchange and tactical role assignment | How a group behaves together |
| Attack controller | Telegraph, commitment, impact, recovery, cooldown | How damage is resolved |
| Audio / animation presenter | State-specific sound, wake, silhouette, and movement cues | How the player reads intent |

Do not use a single monolithic enemy script. Keep authoring data in ScriptableObjects and per-instance mutable state in runtime objects. The existing [Creature AI specification](CREATURE-AI.md) and [Unity implementation blueprint](../08_TECHNICAL_DESIGN/AI-IMPLEMENTATION-BLUEPRINT.md) remain the baseline contracts.

## 4. Behavioural escalation

Recommended generic progression:

`Dormant → Detecting → Investigating → Warning → Stalking → Pursuing → AttackTelegraph → Attacking → Recovering → Disengaging → Cooldown`

A creature may skip or repeat optional phases only through explicit transition rules. It may leave after investigating, warn without attacking, stalk and lose interest, or pursue and then abort. This is essential: **not every encounter should become a chase**.

| Phase | Internal motive | Example behaviour | Player-readable cue |
|---|---|---|---|
| Dormant | Rest, patrol, or wait | Remain in habitat or follow an authored route | Environmental traces |
| Detecting | Verify a possible cue | Turn toward a distant pulse; pause to listen | Brief rig response or ripple |
| Investigating | Locate the source | Approach the last sound position, not the live player | Bearing changes; distant call |
| Warning | Communicate presence or territory | Reveal silhouette, circle outside attack range, vocalize | Clear directional audio / wake |
| Stalking | Observe and test | Shadow the boat, cross behind it, probe likely escape routes | Repeated peripheral sightings |
| Pursuing | Close or intercept | Predict a short future position and take a viable water lane | Directional cues intensify |
| AttackTelegraph | Commit to one attack lane | Accelerate, line up, expose a readable wake | Unambiguous tell and reaction window |
| Attacking | Resolve one bounded attack | Ram, strike equipment, or make a near miss | Impact or clear miss |
| Recovering | Regain position and energy | Pass the target, arc away, slow, or dive | Threat visibly leaves the attack lane |
| Disengaging | End contact | Return to habitat or search elsewhere | Sound and visual pressure recede |
| Cooldown | Prevent immediate repetition | No new attack until the lock expires | Quiet period |

### 4.1 Transition evidence

A transition should require a combination of evidence and time, not a single random roll.

Suggested starting thresholds on a 0–1 belief scale:

| Transition | Suggested minimum belief | Additional gate |
|---|---:|---|
| Detecting → Investigating | 0.30 | Cue is inside hearing / sensing limits |
| Investigating → Warning | 0.50 | Threat is near enough for a warning to be meaningful |
| Warning → Stalking | 0.58 | Target evidence persists or disturbance increases |
| Stalking → Pursuing | 0.78 | Valid route, range, and commitment conditions |
| Pursuing → AttackTelegraph | 0.85 | Attack lane, cooldown, and safety checks pass |

These are initial game-feel thresholds, not biological constants. Use hysteresis: the threshold to leave a state should generally be lower than the threshold needed to enter it. Require minimum state dwell time to prevent frame-to-frame state oscillation.

## 5. Perception, belief, and memory

### 5.1 Separate the senses

Each creature has independently tunable channels:

- **Vision:** field of view, range, contrast, movement exposure, fog / turbidity transmission, and occlusion.
- **Hearing:** sound category, band sensitivity, received level, masking, propagation conditions, and source age.
- **Wake / vibration:** nearby hull movement, collision, propeller turbulence, and large water displacement.
- **Light:** lantern or instrument use, only for species that plausibly respond to it.
- **Disturbance:** repeated fishing, repeated Rig deployment, damaged buoys, or disruption within a territory.

The elevated 2.5D player camera is not the creature's sensory cone. The camera determines what the player can see; creature perception is evaluated from its own position and orientation. Preserve the existing project rule separating camera FOV, player awareness, and threat FOV.

### 5.2 Belief confidence

Store a belief rather than a binary seen/not-seen flag:

`E(t+Delta t) = E(t)e^{-lambdaDelta t} + sum_i w_i s_i`

`C = 1 - e^{-E}`

Where:

- `E` is accumulated evidence.
- `lambda` is evidence decay per second.
- `w_i` is the authored reliability weight for a cue type.
- `s_i` is the normalized strength of the new cue (0–1).
- `C` is belief confidence (0–1).

Example: a strong Rig pulse may add more evidence than a faint distant engine sound. A second cue from a different channel can increase confidence. Repeated identical pings should have diminishing returns so players cannot trivially force a guaranteed encounter.

Store at least:

- Last seen position and simulation time.
- Last heard position, category, and time.
- Estimated target velocity and its confidence.
- Current belief confidence and evidence age.
- Last known route / direction of travel.
- Last successful visual or acoustic confirmation.
- Disturbance level associated with the target region.

When visual contact is lost, confidence decays and the creature searches the last-known position or a plausible escape corridor. It must not snap to the player's new position.

### 5.3 Sound-driven investigation

The Sounding Rig, engine, horn, collision, hull impacts, and boat wake should emit typed `SoundEvent` or disturbance events. Each event includes stable source ID, position, category, frequency band, loudness, timestamp, duration, and propagation profile.

A listener calculates received strength using the acoustic model in the existing threat-perception document. It then records the event only if the received value exceeds its species-specific threshold. The creature investigates that source location. The audio bus must not pass a live player transform to the threat brain.

## 6. Utility-based action selection

The HFSM determines which actions are legal. A utility evaluator ranks those legal actions.

Example action set:

| Action | Purpose |
|---|---|
| Listen | Stop or slow and gather more evidence |
| InvestigateSource | Approach last sound / disturbance origin |
| Circle | Observe without committing |
| Shadow | Follow outside immediate attack range |
| ProbeRoute | Test a likely escape corridor |
| Intercept | Move toward a predicted future position |
| Pursue | Close distance while target confidence is strong |
| Ambush | Move toward an authored choke point or route boundary |
| Attack | Execute one telegraphed attack |
| Withdraw | Break contact when risk is too high |

A simple tunable score:

`U(a) = sum_j alpha_{a,j} f_j - sum_k eta_{a,k} g_k`

Where `f` values are positive factors (target confidence, motive, range suitability, territory relevance) and `g` values are costs (hull danger, obstacle risk, energy cost, low confidence, cooldown). Weights are authored per creature.

Do not choose a new action every frame. Re-evaluate on a bounded decision tick (for example 2–4 times per second), use a minimum action lock, and add a switch margin so an action must be meaningfully better before replacing the current one. This prevents twitchy behaviour.

## 7. Stalking, testing, and pursuit movement

### 7.1 Predictive pursuit

Use a short prediction horizon rather than chasing the target's current position:

`predictedPosition = targetPosition + targetVelocity × leadTime`

`leadTime = clamp(distance / max(creatureSpeed, minimumSpeed), 0, maximumLeadTime)`

Start with a lead horizon of roughly 1–2 seconds and tune per creature. Smooth velocity estimates; ignore implausible spikes caused by teleportation, respawn, or physics discontinuity. Prediction should be imperfect when the target estimate is uncertain.

### 7.2 Steering

Combine bounded steering forces:

`desired = seek(predictedTarget) + avoid(obstacles) + currentCompensation + laneBias`

Clamp the final desired acceleration and angular velocity to the creature's authored limits. Use water-volume constraints and authored avoidance probes for rocks, shorelines, and shallow exclusion regions. Do not use humanoid NavMesh assumptions for a free-swimming creature unless it is moving on actual traversable geometry.

### 7.3 Circling and “playing with prey”

Treat play-like testing as a fictional behaviour personality, not as a universal scientific claim about predators. Possible actions:

- Observe from outside the boat's effective response range.
- Circle once to test orientation and speed.
- Approach the last sound source, then pause.
- Cross the likely escape lane without instantly attacking.
- Withdraw after a strong defensive response.
- Repeat a tactic only when the creature has a reason to believe it may work.

Use sparingly. Repeated fake charges become predictable and can feel like the game is wasting the player's time. A test should reveal information, alter position, or create a meaningful decision.

### 7.4 Destructive tendency

A destructive creature may target equipment or infrastructure because of territory, attraction, obstruction, or an authored escalation state. It must not damage arbitrary player property simply because a random timer fired.

Destructive targets should be selected by:
1. Species capability and motive.
2. Target availability and distance.
3. Current aggression / disturbance.
4. Whether the action has a warning and fair counterplay.
5. Whether the target is protected by a safe-zone or quest-critical rule.

Damage should be bounded and repairable. Do not permanently delete unique progression items or destroy quest-critical equipment without a clearly communicated, recoverable design.

## 8. Coordinated hunting

Do not build full multi-agent reinforcement learning for the first implementation. Use a small, deterministic role-assignment layer with limited shared information. Advanced learning systems are expensive to author, debug, balance, and validate for a solo-first project.

### 8.1 Shared information

Group members may share:

- Alert state (unaware / investigating / target suspected / target confirmed).
- Last-known target region or bearing.
- Confidence bucket (low / medium / high).
- Assigned role and expiry time.
- A short communication event (call, pulse, wake, or movement cue).

They should not share exact live coordinates unless direct line of sight or a plausible communication mechanism provides them. Allow messages to be delayed, lost, or stale.

### 8.2 Roles

| Role | Behaviour | Fairness constraint |
|---|---|---|
| Searcher | Investigates last-known evidence | Does not know live player location |
| Pressure | Closes from one side and forces route choice | Leaves at least one viable escape lane |
| Flanker | Moves toward a predicted route | Must respect travel time and obstacles |
| Ambusher | Occupies an authored choke point | Telegraphs presence before attack |
| Disruptor | Attacks a buoy, light, or equipment | Never silently removes all player options |
| Observer | Maintains distance and updates group confidence | Does not attack unless reassigned |

Role assignment can be greedy: select the highest-value available role for each creature, with costs for duplicate roles, distance, poor habitat fit, or stale target information. Reassign only on state changes, role expiry, or a major new observation.

### 8.3 Group limits

- Prototype with two creatures maximum in one hostile encounter.
- A third may observe or create environmental pressure, but should not add another unavoidable damage lane.
- Keep collision avoidance between creatures separate from target pursuit.
- If communication is interrupted, each creature continues from its own last-known belief and may diverge.
- If the group loses evidence, it should search, split briefly, or withdraw rather than converge magically.

## 9. Creature personality and authoring data

Represent personality as normalized authored parameters (0–1), not dozens of hard-coded branches.

| Parameter | Low value | High value |
|---|---|---|
| Curiosity | Ignores uncertain cues | Investigates ambiguous cues |
| Aggression | Withdraws readily | Commits to pursuit more readily |
| Caution | Accepts risk | Breaks off when risk rises |
| Territoriality | Roams broadly | Defends a bounded region |
| Sociality | Operates alone | Shares alerts and roles |
| Persistence | Gives up quickly | Searches longer after losing contact |
| Destructiveness | Avoids equipment | Targets lights / buoys / hull when motivated |
| Deception / testing | Direct approach | More circling, probing, and feints |

Use species-level defaults plus instance variation derived from a saved seed. Do not randomize core behavioural identity every encounter; that undermines learnability.

The Sleeper should be a solitary, patient, sound-sensitive ambush predator. It investigates noisy events, tests the player's route, uses a limited pursuit burst, and commits only when its belief and attack geometry are strong. It is the first full hostile AI prototype. Other threats should reuse the architecture but not inherit its behaviour profile.

## 10. Attack contract and fairness

Every damaging attack follows the contract:

1. **Tell:** directional sound, wake, silhouette, instrument disturbance, or hull vibration.
2. **Commit:** creature enters a visible / audible attack lane.
3. **Resolve:** one bounded damage event.
4. **Recover:** creature passes, turns, slows, or withdraws.
5. **Reassess:** next attack requires fresh eligibility and cooldown checks.

Existing Sleeper prototype values remain the current starting point: 2.2–3.0 second telegraph; 12–18% starter hull damage; at least 6 seconds of recovery; 18–24 second attack cooldown; no strike in safe zones or invalid retreat spaces. Do not silently change these values in tuning without updating the creature and attack specifications.

A coordinated encounter must preserve a readable escape decision. Do not have multiple attacks resolve in the same reaction window. Suppress attacks when the boat is already in a guaranteed lethal or control-locked state.

## 11. Unity/C# implementation guidance

Suggested runtime responsibilities:

- `ThreatBrain`: HFSM and guarded state transitions.
- `ThreatUtilityEvaluator`: ranked legal action selection.
- `ThreatPerception`: visual, acoustic, wake, and light sensing.
- `ThreatBeliefMemory`: confidence, last-known target state, decay.
- `ThreatMotor`: steering, turn limits, obstacle and current handling.
- `ThreatGroupCoordinator`: role leases and limited message exchange.
- `ThreatAttackController`: telegraph, impact, recovery, cooldown.
- `ThreatDefinition`: ScriptableObject authoring data.
- `ThreatRuntimeState`: per-instance mutable state.
- `ThreatDebugOverlay`: optional gizmos and state / confidence display.

Illustrative decision loop:

```csharp
void EvaluateThreat(float deltaTime)
{
    perception.SampleIfDue();
    memory.DecayEvidence(deltaTime);
    memory.ApplyNewEvidence(perception.DrainEvidence());

    if (!director.IsEncounterStillValid(runtime, worldSnapshot))
    {
        brain.RequestDisengage();
        return;
    }

    brain.EvaluateGuardedTransitions(memory, worldSnapshot);

    IReadOnlyList<ThreatAction> legal =
        brain.GetLegalActions(worldSnapshot);

    ThreatAction best =
        utilityEvaluator.SelectWithSwitchMargin(
            legal, runtime, memory, worldSnapshot);

    if (runtime.ActionLockExpired)
        brain.TrySetAction(best);

    motor.SetDesiredMovement(
        brain.CurrentAction,
        memory.TargetEstimate,
        worldSnapshot);
}
```

This is architectural pseudocode and must be adapted, compiled, profiled, and tested in the actual Unity project. Keep physics movement in the appropriate physics step; run expensive sensing and decision logic at configured cadences. Use stable IDs and seeded randomness for authored event variation and deterministic regression testing.

## 12. Performance and debugging

Recommended initial cadence:

| System | Initial cadence |
|---|---:|
| Nearby threat perception | 4 Hz |
| Distant active-region perception | 1 Hz |
| Utility / state decision | 2–4 Hz |
| Group role reassignment | On event or every 0.5–1 s |
| Steering / physics | Fixed physics step |
| Cosmetic effects | Presentation frame |

Add a debug view that displays state, legal actions, utility scores, belief confidence, last-known point, selected target, group role, next cooldown, and current speed/turn rate. Add a per-encounter event log with cue timestamps and transition reasons. This will make “unfair AI” reports reproducible.

Use region activation and distance-based update throttling. Begin with a small number of active threats and profile before introducing jobs, custom acoustic propagation, or machine learning.

## 13. Validation and acceptance tests

### Perception and memory
- A threat can investigate an audible Sounding Rig pulse outside visual FOV.
- Fog and occlusion reduce visual detection without disabling sound.
- Losing line of sight causes memory confidence to decay.
- The threat searches the last-known location rather than tracking the live player transform.
- Repeated identical sounds have diminishing returns.

### Decision quality
- A creature can investigate and disengage without attacking.
- State changes obey dwell time and hysteresis.
- Utility selection does not oscillate every decision tick.
- High caution increases break-off likelihood; high persistence extends search time.
- Personality changes behaviour without breaking hard safety rules.

### Pursuit and coordination
- The creature respects acceleration, turn rate, depth, terrain, and safe-zone limits.
- A flanker moves toward a predicted route using stale information, not omniscience.
- Group members avoid each other and do not all select the same role.
- At least one viable retreat lane remains during coordinated pressure.
- Lost communication causes believable divergence, not a shared perfect target update.

### Attacks and destructive actions
- Every attack has a valid tell before damage.
- Cooldowns prevent immediate attack chains.
- No attack resolves in a safe zone or while the player is in a protected interaction.
- Equipment damage is species-specific, warned, bounded, and recoverable.
- Non-hostile quest phenomena cannot call creature damage APIs.

### Replay and performance
- A saved seed and event history reproduce the same authored event decisions.
- Save/load preserves state, cooldown, belief data, and group role leases where needed.
- AI cadence and active-creature count meet the project's measured frame budget.
- Every state transition records a reason usable in a bug report.

## 14. Implementation order

1. Build one dummy threat using the existing HFSM.
2. Add typed SoundEvents and last-known-position memory.
3. Add evidence decay and threshold hysteresis.
4. Add steering, pursuit prediction, and obstacle avoidance.
5. Add stalking actions (circle, shadow, probe) with bounded action locks.
6. Add The Sleeper's telegraphed strike and recovery.
7. Add personality data and a debug overlay.
8. Prototype a two-creature encounter with role leases and limited messages.
9. Add destructive equipment targeting only after base attacks are fair.
10. Expand to other species only after The Sleeper passes acceptance tests.

## 15. References

1. Craig W. Reynolds, [Steering Behaviors for Autonomous Characters](https://www.red3d.com/cwr/papers/1999/gdc99steer.html), Game Developers Conference, 1999. Foundational steering behaviours, pursuit/evasion, obstacle avoidance, and group motion.
2. Eric Raboin, Ugur Kuter, Dana Nau, and S. Gupta, [Adversarial Planning for Multi-Agent Pursuit-Evasion Games in Partially Observable Euclidean Space](https://doi.org/10.1609/aiide.v8i3.12545), AIIDE, 2012. Limited observation and communication in continuous spaces.
3. [Pursuit-evasion games of multiple cooperative pursuers and an evader: A biological-inspired perspective](https://doi.org/10.1016/j.cnsns.2022.106386), Communications in Nonlinear Science and Numerical Simulation, 2022. Cooperative pursuit, communication constraints, and collision avoidance.
4. Sihan Xu and Zhaohui Dang, [Emergent behaviors in multiagent pursuit evasion games within a bounded 2D grid world](https://doi.org/10.1038/s41598-025-15057-x), Scientific Reports, 2025. A bounded grid-world study describing coordinated actions such as flank, engage, ambush, drive, chase, and intercept. Treat its results as context-specific, not as a ready-made game AI.
5. Unity, [Physics.Raycast scripting reference](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/physics/raycast). Official reference for line-of-sight / obstacle queries used by the existing Unity perception specification.

## 16. Source-of-truth notes

This document extends, but does not replace:

- [Creature AI and Threat Behaviour](CREATURE-AI.md) — common hostile states, perception, The Sleeper values, attack contract.
- [Threat Director](THREAT-DIRECTOR.md) — encounter eligibility and scheduling.
- [Entity Perception and Encounter Mathematics](../07_GAME_RULES_AND_BALANCING/ENTITY-PERCEPTION-AND-ENCOUNTER-MATH.md) — probability and perception math.
- [Threat Audio Logic](../10_AUDIO_AND_SOUND_DESIGN/THREAT-AUDIO-LOGIC.md) — audio presentation and directional cues.
- [Unity AI Implementation Blueprint](../08_TECHNICAL_DESIGN/AI-IMPLEMENTATION-BLUEPRINT.md) — component boundaries, event handling, save state, and build order.

When these documents conflict, update the relevant authoritative system specification and record the decision. The numerical values in this document are prototype tuning suggestions unless explicitly marked as existing project values.


## 17. Life-cycle behaviour integration

The hunt and encounter logic described above is only one part of a creature's life. Ordinary creatures also need feeding, rest, recovery, migration, habitat use, and reproduction where appropriate. These must not be represented as extra hostile threat states.

Use three connected but independent layers:

- **Life state:** active, resting, sleeping, hungry, feeding, recovering, migrating, sheltering, courting, or spawning.
- **Tactical action:** wander, patrol, forage, investigate, stalk prey, ambush, feed, flee, or return to habitat.
- **Player encounter state:** unaware, curious, warning, stalking the boat, pursuing, attacking, disengaging, or anomalous.

A predator can be hungry and resting, then wake to forage, hunt ordinary fish, feed, and return to shelter without ever detecting the player. The Sleeper's player-directed hunt remains a separate hostile encounter controlled by its threat profile.

For full behaviour rules, needs, schedules, food-web interactions, rest and healing, reproduction, migration, and supernatural exceptions, see [Creature Life Cycles and Ecological Behaviour](CREATURE-LIFE-CYCLES-AND-ECOLOGY.md).

The life-cycle specification also defines **unnatural behaviour**: a deliberate, constrained violation of an established baseline (for example, identical calls repeating at impossible tide intervals). Anomaly controllers may override only explicitly authored biological rules; they must not become a general excuse for arbitrary teleportation, invisible damage, or untelegraphed attacks.
