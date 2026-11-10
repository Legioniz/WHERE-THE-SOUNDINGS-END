# System Mathematics

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Probability
For mutually exclusive outcomes with non-negative weights: P(i) = w(i) / sum(eligible weights). Weights are relative likelihoods; filter ineligible outcomes first.

For independent per-attempt chance p over n attempts: P(at least one) = 1 - (1-p)^n. A 2% chance repeated 50 times yields about 63.6% chance of one or more successes.

For a constant event hazard lambda per second over interval dt: P(event) = 1 - exp(-lambda*dt). This Poisson approximation is useful for ambient event scheduling; add cooldowns and eligibility.

Pity proposal: p(n) = min(p_max, p_base + n*increment), reset after success. This changes the distribution and should be used only where prolonged failure harms progression.

## Ecology
A regional aggregate stock approximation: N(next) = clamp(N + r*N*(1-N/K)*dt - catch - mortality, 0, K). N is stock units; r is recovery rate; K is regional capacity. This logistic model is not an age-structured fisheries assessment.

CPUE = catch quantity / standardized fishing effort. Real CPUE depends on catchability, gear, habitat, targeting, environment, and standardization; do not present it as perfect abundance.

## Catch
A prototype catch chance can combine base chance with bounded habitat, gear, bait, time/weather, and availability factors. Keep each factor interpretable, clamp final result to [0,1], and test distributions.

## Acoustic inference
Simplified echo distance = propagation speed * round-trip time / 2. Bayes: P(H|S) = P(S|H)*P(H)/P(S). Use these as conceptual models; tune in-game behavior for clarity rather than pretending to provide scientific sonar output.

## Fog
Visibility factor = exp(-k*distance) is a design attenuation curve, not a complete atmospheric scattering model. Test landmark readability and collision fairness.

## Economy
Expected trip profit = expected sale/contract/discovery value minus fuel, bait, repairs, fees, and expected loss. Expected binary loss = probability of loss * cost of loss.

## AI
Use finite-state machines for discrete modes, behavior trees for hierarchical tasks, and utility scores where multiple eligible actions need ranking. A threat director maintains bounded tension, cooldowns, eligibility, and event history.

## Balancing evidence
Distinguish established relations (probability, expected value, Poisson hazard, logistic growth, Bayes, echo time-of-flight), engineering approximations (regional stock units, fog attenuation, catch multipliers, tension budget), and designer values (actual chances, rates, weights, cooldowns, capacities).
