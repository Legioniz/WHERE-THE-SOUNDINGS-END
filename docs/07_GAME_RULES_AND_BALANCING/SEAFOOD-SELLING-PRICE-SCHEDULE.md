# Seafood Selling Price Schedule

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed balancing baseline — playtest validation required  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10  
> Currency: US dollar (USD / $) — in-game dollars

## 1. Purpose and pricing philosophy

This document is the single source of truth for ordinary seafood dockside prices, specimen payouts, and first-discovery research rewards. It covers every sale-eligible record in the catch catalogue: **100 fish, 47 invertebrates, and 8 algae / seaweed records**.

Prices are **fictional game-economy values denominated in US dollars ($)**, not a claim about current legal, wholesale, farm-gate, or retail prices for a real species. Philippine market data is used only as a scale reference. The player is a small-boat fisher selling to local buyers, so the base values are designed as dockside payouts, generally below retail benchmarks.

**Fixed conversion anchor:** 1 USD = PHP 63.06, using the BSP reference rate dated November 9, 2026. This is a one-time design conversion anchor, not a live exchange-rate mechanic. Convert each legacy PHP base rate to USD and round the displayed unit rate to two decimal places first; use that displayed USD rate for all subsequent calculations. All in-game prices remain stable in USD unless the game economy is deliberately rebalanced. Source: [BSP USD/PHP reference rate reported for November 9, 2026](https://philnews.ph/2026/11/10/usd-to-php-exchange-rate-today-tuesday-november-10-2026/).

### Market reference snapshot

The following public Philippine price references were checked on 2026-11-10 and converted to USD equivalents using the fixed 63.06 PHP/USD design anchor:

| Reference | Reported price | How it is used |
|---|---:|---|
| DA eAgri / Metro Manila retail, Nov 9 2026 — milkfish | $4.28/kg | Retail anchor for a familiar staple fish |
| DA eAgri / Metro Manila retail, Nov 9 2026 — round scad | $4.81/kg | Retail anchor for a small pelagic fish |
| DA eAgri / Metro Manila retail, Nov 9 2026 — tilapia | $2.49/kg | Lower-priced everyday fish reference |
| Infanta Commercial Center, Nov 9 2026 — bangus | $4.76–320/kg | Local municipal retail cross-check |
| Infanta Commercial Center, Nov 9 2026 — galunggong | $2.85/kg | Local municipal price variation cross-check |
| Infanta Commercial Center, Nov 9 2026 — large shrimp | $7.93–520/kg | Shrimp premium reference |
| Infanta Commercial Center, Nov 9 2026 — maya-maya | $8.72/kg | Premium reef-fish reference |

Sources:
- Department of Agriculture, [eAgri retail price monitoring](https://eagri.e.gov.ph/prices) — Metro Manila snapshot dated November 9, 2026.
- Municipality of Infanta, Quezon, [Infanta Commercial Center official price monitoring](https://infanta.gov.ph/icc) — dated November 9, 2026.
- Department of Agriculture, [Price Monitoring portal](https://www.da.gov.ph/price-monitoring/).
- BFAR, [Fish and Fishery Product Price Monitoring Service](https://www.bfar.da.gov.ph/market-development-services/fish-and-fishery-product-price-monitoring-service/).

Prices differ by location, grade, season, supply chain, and whether a figure is retail, wholesale, or farm-gate. Do not copy the references directly into the game or describe them as player earnings.

## 2. Base-price rules

1. **Fish:** listed price is the default buyer payout per kilogram of whole, landed fish.
2. **Most invertebrates:** listed price is per kilogram of landed whole product, unless the row explicitly states another unit.
3. **Sea urchin:** paid per mature, approved individual because marketable roe yield is not equal to whole-animal weight.
4. **Sea cucumber:** listed value is per kilogram of fresh whole animal and can only be sold through a licensed, quota-controlled contract.
5. **Jellyfish:** paid per kilogram after the required washing and draining step.
6. **Algae / seaweed:** paid per kilogram of wet harvested biomass. Farm plots and managed beds only; this is a raw farm-gate value.
7. A base price assumes legal size, approved location, ordinary buyer acceptance, and standard sale condition.
8. Protected wildlife, closed-bed harvests, undersized individuals, prohibited gear catches, and quarantined specimens have **no ordinary market price**. A price appearing for a species never overrides a closure or legal eligibility rule.
9. Do not add a second rarity multiplier. The species-specific price already reflects its expected market position; rarity affects encounter frequency, not a hidden extra payout.

## 3. Fish — base dockside payout

All values are USD per kilogram of whole landed fish before the destination-island market factor. Each species has a market class; the class and island market determine the final dockside rate. A rare species does not guarantee a large specimen or a high total payout; its individual weight still matters.

| ID | Species | Trade tier | Market class | Base dockside price |
|---|---|---:|---:|
| FSH-001 | Milkfish | T2 | INSHORE | $2.30/kg |
| FSH-002 | Flathead grey mullet | T2 | INSHORE | $1.98/kg |
| FSH-003 | Asian seabass / barramundi | T3 | INSHORE | $3.49/kg |
| FSH-004 | Fourfinger threadfin | T3 | INSHORE | $3.81/kg |
| FSH-005 | Silver sillago | T2 | INSHORE | $2.14/kg |
| FSH-006 | Whipfin silverbiddy | T1 | INSHORE | $1.03/kg |
| FSH-007 | Splendid ponyfish / slipmouth | T1 | INSHORE | $0.95/kg |
| FSH-008 | Spotted scat | T2 | INSHORE | $1.51/kg |
| FSH-009 | Indian anchovy | T1 | INSHORE | $1.27/kg |
| FSH-010 | Commerson's anchovy | T1 | INSHORE | $1.19/kg |
| FSH-011 | Goldstripe sardinella | T1 | INSHORE | $1.19/kg |
| FSH-012 | Bali sardinella | T1 | INSHORE | $1.35/kg |
| FSH-013 | Fringescale sardinella | T1 | INSHORE | $1.19/kg |
| FSH-014 | Tigertooth croaker | T2 | INSHORE | $2.14/kg |
| FSH-015 | Brushtooth lizardfish | T2 | INSHORE | $1.74/kg |
| FSH-016 | Indian halibut | T3 | REEF | $2.85/kg |
| FSH-017 | White-spotted spinefoot / rabbitfish | T2 | REEF | $1.98/kg |
| FSH-018 | Little spinefoot | T2 | REEF | $1.67/kg |
| FSH-019 | Ornate threadfin bream | T2 | REEF | $2.30/kg |
| FSH-020 | Japanese threadfin bream | T2 | REEF | $2.14/kg |
| FSH-021 | Goldband goatfish | T2 | REEF | $1.98/kg |
| FSH-022 | Whitesaddle goatfish | T3 | REEF | $2.93/kg |
| FSH-023 | Mangrove red snapper | T4 | REEF | $4.60/kg |
| FSH-024 | Russell's snapper | T3 | REEF | $3.33/kg |
| FSH-025 | John's snapper | T3 | REEF | $3.25/kg |
| FSH-026 | Thumbprint emperor | T2 | REEF | $2.14/kg |
| FSH-027 | Pinkear emperor | T3 | REEF | $2.93/kg |
| FSH-028 | Spangled emperor | T4 | REEF | $4.12/kg |
| FSH-029 | Orange-spotted grouper | T4 | REEF | $4.44/kg |
| FSH-030 | Honeycomb grouper | T3 | REEF | $2.85/kg |
| FSH-031 | Leopard coral grouper | T5 | REEF | $5.71/kg |
| FSH-032 | Giant trevally | T4 | REEF | $3.49/kg |
| FSH-033 | Bluefin trevally | T4 | REEF | $3.65/kg |
| FSH-034 | Golden trevally | T3 | REEF | $2.70/kg |
| FSH-035 | Java parrotfish | T2 | REEF | $1.59/kg |
| FSH-036 | Shortfin scad | T1 | PELAGIC | $1.19/kg |
| FSH-037 | Japanese scad | T1 | PELAGIC | $1.27/kg |
| FSH-038 | Yellowstripe scad | T1 | PELAGIC | $1.35/kg |
| FSH-039 | Bigeye scad | T2 | PELAGIC | $1.67/kg |
| FSH-040 | Indian mackerel | T2 | PELAGIC | $2.22/kg |
| FSH-041 | Short-bodied mackerel | T2 | PELAGIC | $1.90/kg |
| FSH-042 | Narrow-barred Spanish mackerel | T4 | PELAGIC | $4.76/kg |
| FSH-043 | Talang queenfish | T3 | PELAGIC | $3.09/kg |
| FSH-044 | Great barracuda | T3 | PELAGIC | $2.85/kg |
| FSH-045 | Frigate tuna | T3 | PELAGIC | $2.22/kg |
| FSH-046 | Kawakawa | T3 | PELAGIC | $2.54/kg |
| FSH-047 | Skipjack tuna | T4 | PELAGIC | $3.33/kg |
| FSH-048 | Yellowfin tuna | T5 | PELAGIC | $5.23/kg |
| FSH-049 | Bigeye tuna | T5 | PELAGIC | $5.71/kg |
| FSH-050 | Dolphinfish / mahi-mahi | T4 | PELAGIC | $3.49/kg |
| FSH-051 | Wahoo | T5 | PELAGIC | $5.87/kg |
| FSH-052 | Rainbow runner | T3 | PELAGIC | $2.78/kg |
| FSH-053 | Black pomfret | T3 | PELAGIC | $3.01/kg |
| FSH-054 | Silver pomfret | T4 | PELAGIC | $4.12/kg |
| FSH-055 | Largehead hairtail | T3 | PELAGIC | $2.46/kg |
| FSH-056 | Flying fish group | T2 | PELAGIC | $1.35/kg |
| FSH-057 | Redbelly yellowtail fusilier | T2 | REEF | $1.51/kg |
| FSH-058 | Blue-and-gold fusilier | T2 | REEF | $1.74/kg |
| FSH-059 | Paddletail snapper | T3 | REEF | $3.49/kg |
| FSH-060 | Blackspot snapper | T2 | REEF | $2.30/kg |
| FSH-061 | Red emperor | T4 | REEF | $4.92/kg |
| FSH-062 | Malabar grouper | T4 | REEF | $4.76/kg |
| FSH-063 | Brown-marbled grouper | T5 | REEF | $6.18/kg |
| FSH-064 | Wire-netting cod | T4 | REEF | $4.44/kg |
| FSH-065 | Lunar-tail grouper | T4 | REEF | $4.12/kg |
| FSH-066 | Painted sweetlips | T3 | REEF | $2.54/kg |
| FSH-067 | Blackspotted rubberlip | T2 | REEF | $1.82/kg |
| FSH-068 | Iodine bream | T4 | REEF | $4.60/kg |
| FSH-069 | Large-eyed sea bream | T4 | REEF | $4.28/kg |
| FSH-070 | Orange-spotted spinefoot | T2 | REEF | $2.06/kg |
| FSH-071 | Blue-spotted cornetfish | T2 | PELAGIC | $1.35/kg |
| FSH-072 | Hound needlefish | T2 | PELAGIC | $1.27/kg |
| FSH-073 | Black-barred halfbeak | T1 | PELAGIC | $1.03/kg |
| FSH-074 | Bartail flathead | T2 | INSHORE | $2.62/kg |
| FSH-075 | Large-scale tonguesole | T2 | INSHORE | $1.82/kg |
| FSH-076 | Bigeye bream | T3 | REEF | $2.93/kg |
| FSH-077 | Black jack | T4 | PELAGIC | $3.96/kg |
| FSH-078 | Almaco jack | T4 | PELAGIC | $4.28/kg |
| FSH-079 | Dogtooth tuna | T5 | PELAGIC | $6.66/kg |
| FSH-080 | Dorab wolf-herring | T3 | PELAGIC | $2.30/kg |
| FSH-081 | Redtail scad | T2 | PELAGIC | $1.43/kg |
| FSH-082 | Picnic seabream | T3 | INSHORE | $2.38/kg |
| FSH-083 | Goldlined seabream | T3 | INSHORE | $2.78/kg |
| FSH-084 | Yellowstripe goatfish | T2 | REEF | $1.98/kg |
| FSH-085 | Dash-and-dot goatfish | T3 | REEF | $2.78/kg |
| FSH-086 | Goldsaddle goatfish | T4 | REEF | $3.96/kg |
| FSH-087 | Ruby snapper | T5 | DEEPWATER | $7.14/kg |
| FSH-088 | Longtail red snapper | T5 | DEEPWATER | $7.45/kg |
| FSH-089 | Small-tooth jobfish | T4 | DEEPWATER | $4.12/kg |
| FSH-090 | Green jobfish | T4 | DEEPWATER | $3.89/kg |
| FSH-091 | Rosy jobfish | T5 | DEEPWATER | $6.18/kg |
| FSH-092 | Purple-cheek jobfish | T5 | DEEPWATER | $5.87/kg |
| FSH-093 | Banded flower snapper | T5 | DEEPWATER | $7.29/kg |
| FSH-094 | Yellow-finned jobfish | T4 | DEEPWATER | $4.44/kg |
| FSH-095 | Kalikali / pink snapper | T5 | DEEPWATER | $5.71/kg |
| FSH-096 | Flower snapper | T5 | DEEPWATER | $6.03/kg |
| FSH-097 | Asian swamp eel | T3 | INSHORE | $2.06/kg |
| FSH-098 | Striped snakehead | T3 | INSHORE | $2.38/kg |
| FSH-099 | Climbing perch | T2 | INSHORE | $1.35/kg |
| FSH-100 | Philippine broadhead catfish | T3 | INSHORE | $1.82/kg |

## 4. Invertebrates — base dockside payout

Values are USD per kilogram except where the unit is written explicitly. All invertebrates use the INVERTEBRATE market class. Contract-only species remain locked behind size limits, permits / quotas, and approved collection zones.

| ID | Species | Trade tier | Market class | Base dockside price |
|---|---|---:|---:|
| INV-001 | Small shrimp / alamang group | T1 | INVERTEBRATE | $1.43/kg |
| INV-002 | Giant tiger prawn | T4 | INVERTEBRATE | $6.66/kg |
| INV-003 | Banana prawn | T3 | INVERTEBRATE | $3.96/kg |
| INV-004 | Greasyback shrimp | T2 | INVERTEBRATE | $2.38/kg |
| INV-005 | Blue swimming crab | T3 | INVERTEBRATE | $3.65/kg |
| INV-006 | Giant mud crab | T4 | INVERTEBRATE | $5.71/kg |
| INV-007 | Spanner crab | T4 | INVERTEBRATE | $5.39/kg |
| INV-008 | Ornate spiny lobster | T5 | INVERTEBRATE | $11.10/kg |
| INV-009 | Painted spiny lobster | T4 | INVERTEBRATE | $8.25/kg |
| INV-010 | Slipper lobster | T3 | INVERTEBRATE | $5.23/kg |
| INV-011 | Bigfin reef squid | T3 | INVERTEBRATE | $3.17/kg |
| INV-012 | Indian squid | T3 | INVERTEBRATE | $2.78/kg |
| INV-013 | Pharaoh cuttlefish | T4 | INVERTEBRATE | $4.12/kg |
| INV-014 | Day octopus | T4 | INVERTEBRATE | $4.44/kg |
| INV-015 | Asian green mussel | T2 | INVERTEBRATE | $1.27/kg |
| INV-016 | Tropical cupped oyster | T2 | INVERTEBRATE | $1.59/kg |
| INV-017 | Blood cockle | T3 | INVERTEBRATE | $2.06/kg |
| INV-018 | Undulated surf clam | T2 | INVERTEBRATE | $1.43/kg |
| INV-019 | Collector sea urchin | T3 | INVERTEBRATE | $0.71 / mature approved individual |
| INV-020 | Mantis shrimp / spearer group | T3 | INVERTEBRATE | $3.81/kg |
| INV-021 | Giant river prawn | T3 | INVERTEBRATE | $5.71/kg |
| INV-022 | Orange mud crab | T4 | INVERTEBRATE | $4.76/kg |
| INV-023 | Purple mud crab | T4 | INVERTEBRATE | $5.55/kg |
| INV-024 | Crucifix crab | T3 | INVERTEBRATE | $3.81/kg |
| INV-025 | Three-spot swimming crab | T3 | INVERTEBRATE | $3.33/kg |
| INV-026 | Long-eyed swimming crab | T3 | INVERTEBRATE | $2.78/kg |
| INV-027 | Crenate swimming crab | T2 | INVERTEBRATE | $1.98/kg |
| INV-028 | Common banded mantis shrimp | T4 | INVERTEBRATE | $4.76/kg |
| INV-029 | Sculptured mitten lobster | T4 | INVERTEBRATE | $7.14/kg |
| INV-030 | Longleg spiny lobster | T4 | INVERTEBRATE | $8.56/kg |
| INV-031 | Donkey's ear abalone | T5 | INVERTEBRATE | $11.10/kg landed whole weight; managed harvest only |
| INV-032 | Asian hard clam | T3 | INVERTEBRATE | $2.06/kg |
| INV-033 | Razor clams | T3 | INVERTEBRATE | $2.22/kg |
| INV-034 | Pen shell | T3 | INVERTEBRATE | $2.22/kg |
| INV-035 | Dog conch | T3 | INVERTEBRATE | $1.90/kg |
| INV-036 | Spider conch | T4 | INVERTEBRATE | $3.01/kg |
| INV-037 | Sandfish sea cucumber | T5 contract | INVERTEBRATE | $1.51/kg fresh whole weight; licensed contract only |
| INV-038 | Warty sea cucumber | T4 contract | INVERTEBRATE | $1.19/kg fresh whole weight; licensed contract only |
| INV-039 | Deepwater redfish sea cucumber | T4 contract | INVERTEBRATE | $1.35/kg fresh whole weight; licensed contract only |
| INV-040 | Lollyfish sea cucumber | T3 contract | INVERTEBRATE | $0.95/kg fresh whole weight; licensed contract only |
| INV-041 | Leopardfish sea cucumber | T4 contract | INVERTEBRATE | $1.27/kg fresh whole weight; licensed contract only |
| INV-042 | Edible jellyfish | T3 processed | INVERTEBRATE | $1.27/kg washed and drained weight |
| INV-043 | Edible squid / lokos | T3 | INVERTEBRATE | $3.01/kg |
| INV-044 | Spineless cuttlefish | T4 | INVERTEBRATE | $3.73/kg |
| INV-045 | Western king prawn | T3 | INVERTEBRATE | $3.89/kg |
| INV-046 | Palm shrimp | T2 | INVERTEBRATE | $1.98/kg |
| INV-047 | Asian moon scallop | T4 | INVERTEBRATE | $3.49/kg |

## 5. Algae and seaweed — raw farm-gate payout

Values are USD per kilogram of wet biomass. All algae records use the SEAWEED market class. Do not use dry-weight prices for fresh harvests.

| ID | Harvest | Trade role | Market class | Base farm-gate price |
|---|---|---|---:|
| ALG-001 | Cottonii / carrageenan seaweed | Bulk trade / farm contract | SEAWEED | $0.29/kg wet harvest |
| ALG-002 | Spinosum seaweed | Bulk trade / farm contract | SEAWEED | $0.32/kg wet harvest |
| ALG-003 | Gracilaria seaweed | Food / processing trade | SEAWEED | $0.38/kg wet harvest |
| ALG-004 | Sea grapes / lato | Fresh food, premium local market | SEAWEED | $2.22/kg wet harvest |
| ALG-005 | Spinosum seaweed | Carrageenan raw material | SEAWEED | $0.32/kg wet harvest |
| ALG-006 | Cottonii seaweed | Carrageenan raw material | SEAWEED | $0.29/kg wet harvest |
| ALG-007 | Edible red agar seaweed | Agar / food processing | SEAWEED | $0.40/kg wet harvest |
| ALG-008 | Sea grape / Caulerpa | Fresh food / local market | SEAWEED | $1.98/kg wet harvest |

## 6. Aberrant specimens — biological variants

Aberrant specimens inherit the base species price. Their appearance does **not** create a universal mutant bonus. The first verified research record can pay a separate finite discovery reward.

| ID | Specimen | Base species price | Ordinary sale rule | First verified research reward |
|---|---|---:|---|---:|
| ABR-001 | Chalkskin Mullet | FSH-002: $1.98/kg | After basic inspection: 100% of base price if food-grade | $5.55 |
| ABR-002 | Bentspine Flathead | FSH-074: $2.62/kg | If mild and cleared: 70% of base price ($1.84/kg rounded); severe deformity is research-only | $8.72 |
| ABR-003 | Crookedjaw Croaker | FSH-014: $2.14/kg | If inspected and cleared: 75% of base price ($1.60/kg rounded); otherwise research-only | $7.14 |
| ABR-004 | Knotfin Rabbitfish | FSH-017: $1.98/kg | Healed fin injury, inspected: 85% of base price ($1.68/kg rounded) | $4.76 |
| ABR-005 | Cloud-eye Snapper | FSH-024 or FSH-025: $3.33 or $3.25/kg | If explicitly cleared: 70% of the selected base price ($2.33 or $2.28/kg); otherwise research-only | $7.93 |
| ABR-006 | Callusback Emperor | FSH-026 or FSH-027: $2.14 or $2.93/kg | If food-grade after inspection: 100% of base price; no automatic specimen premium | $10.31 |
| ABR-007 | Parasite-marked Scad | FSH-036 or FSH-038: $1.19 or $1.35/kg | Quarantine / research-only; no food sale while parasite or lesion concern remains | $9.51 |
| ABR-008 | Split-tail Sillago | FSH-005: $2.14/kg | Healed tail injury, inspected: 80% of base price ($1.71/kg) | $4.76 |

**Reward limit:** the discovery reward is paid once per specimen ID per save, after the journal reaches Reviewed and the evidence is submitted. Duplicate individuals do not repeat it. A food sale and a first-record reward may both occur only when the specimen has been cleared for food and the player has not already claimed the research record.

## 7. Unnatural specimens — finite research contracts, not seafood sales

Unnatural specimens cannot be sold by weight, placed in ordinary restaurant stock, or processed into food. The figures below are one-time research-contract payouts for the first complete, verified record submitted for each ID.

| ID | Specimen | First verified contract payout | Ordinary market |
|---|---|---:|---|
| UNC-001 | The Tideglass Mullet | $19.03 | Not accepted |
| UNC-002 | The Bellbone Croaker | $28.54 | Not accepted |
| UNC-003 | The Backstep Eel | $34.89 | Not accepted |
| UNC-004 | The Seamfin Shoal | $39.64 | Not accepted |
| UNC-005 | The Lanternless Lure | $44.40 | Not accepted |
| UNC-006 | The Saltless Eel | $38.06 | Not accepted |
| UNC-007 | The Hollowjaw Grenadier | $57.09 | Not accepted |
| UNC-008 | The Unmoored Fry | $23.79 | Not accepted |

The payout is for evidence and research delivery, not for the specimen's mass. These fixed contract rewards are not multiplied by the island market factor; the reward is paid by the assigned research contract location. UNC-008 may be released and documented; capturing it is not required for the reward. Each reward is claimed once per save. Repeated exposure, duplicate samples, or reloading an already recorded event cannot generate more money.

## 8. Sale calculation

For a normal eligible sale:

`SaleValueUSD = roundToCent(SaleQuantity × BasePriceUSD × IslandMarketFactor × Quality × Freshness × Demand × LiveCondition)`

For a per-individual item, `SaleQuantity` is the eligible individual count and `BasePriceUSD` is the listed per-individual rate. Use the two-decimal rate shown in the table as the runtime value, then round the completed sale line to the nearest cent (midpoints round away from zero). `IslandMarketFactor` comes from the destination dock and the item's market class. `LiveCondition` is 1.00 by default; use 1.10–1.15 only for an eligible live crab / lobster handed over alive and intact to a buyer that explicitly accepts live stock. Calculate per species / buyer line, then sum the transaction. Apply all factors once only.

| Factor | Working value | Rule |
|---|---:|---|
| Standard quality | 1.00 | Clean landing, expected size and no significant damage |
| Damaged / low grade | 0.60–0.80 | Bruising, torn tissue, poor handling |
| Good grade | 1.10 | Above-standard size or careful landing |
| Premium grade | 1.20 | Exceptional condition or buyer-requested grade |
| Freshness: fresh | 1.00 | Sold promptly or held correctly |
| Freshness: chilled | 0.95–1.00 | Proper ice / cold storage; freshness decays slowly |
| Freshness: aging | 0.70–0.85 | Delayed sale with acceptable quality remaining |
| Freshness: poor | 0.35–0.60 | Warm, long delay, or visibly deteriorated |
| Island market | 0.50–1.40 | Destination island's multiplier for the species' market class; see regional matrix below |
| Local demand | 0.75–1.35 | Buyer stock, menu, event, weather disruption or posted order |
| Live condition | 1.00–1.15 | Only for eligible live, intact crab / lobster; otherwise 1.00 |
| Legal eligibility | 0 or 1 | Ineligible catch is rejected; do not apply a fractional legal multiplier |

Quality, freshness, and demand should be visible to the player. Buyers must explain a discount or refusal. Do not roll hidden price penalties after the catch lands.

### Worked examples

| Catch | Calculation | Base result |
|---|---|---:|
| 2.0 kg Milkfish | 2.0 × $2.30 × 1.00 × 1.00 × 1.00 | $4.60 |
| 3.0 kg Mangrove red snapper | 3.0 × $4.60 × 1.00 × 1.00 × 1.00 | $13.80 |
| 1.0 kg Giant tiger prawn | 1.0 × $6.66 × 1.00 × 1.00 × 1.00 | $6.66 |
| 4.0 kg Yellowfin tuna | 4.0 × $5.23 × 1.00 × 0.95 × 1.00 | $19.89 |
| 5 mature approved sea urchins | 5 × $0.71 | $3.55 |



## 9. Island market pricing by destination

For pricing purposes, the six established world regions act as six distinct island-market / port economies. These are market hubs mapped to existing geography; they do not lock new canonical island names. Apply the market where the player physically sells the catch, not the region where it was caught.

| Destination market | INSHORE | REEF | PELAGIC | DEEPWATER | INVERTEBRATE | SEAWEED | Market character |
|---|---:|---:|---:|---:|---:|---:|---|
| Greyhaven | 1.08 | 0.92 | 0.90 | 0.70 | 1.10 | 1.05 | Largest dependable home market; reliable demand for staples, estuary fish, and shellfish |
| The Shallows | 0.95 | 1.20 | 0.95 | 0.75 | 1.15 | 1.15 | Restaurants, divers, and visitors pay more for reef catch, live shellfish, and fresh coastal produce |
| Blackrock Reach | 0.90 | 1.10 | 1.18 | 1.00 | 0.95 | 0.85 | Workboat and processing port values durable reef fish and offshore pelagic supply |
| The Drowned Parish | 1.20 | 0.85 | 0.75 | 0.55 | 1.15 | 1.10 | Isolated settlement values brackish, freshwater, shellfish, and reliable local provisioning |
| The Outer Shoals | 0.75 | 0.90 | 1.25 | 1.15 | 0.85 | 0.75 | Offshore fleet and cold-storage buyers value tuna, migratory pelagics, and deep-slope supply |
| The Hollow Trench | 0.55 | 0.75 | 1.10 | 1.40 | 0.70 | 0.50 | Expedition and research port has specialist demand for deepwater catch, but little everyday seafood demand |

The market classes are assigned in the species tables:
- **INSHORE:** estuary, harbor, brackish, freshwater, and nearshore demersal fish.
- **REEF:** reef-associated, rocky-shelf, and reef-edge fish.
- **PELAGIC:** schooling surface fish, migratory open-water fish, tuna, mackerel, jacks, and similar offshore species.
- **DEEPWATER:** upper-slope and deep-slope fish suitable for the defined deep-line gear.
- **INVERTEBRATE:** crustaceans, mollusks, squid, octopus, echinoderms, jellyfish, and other catchable invertebrates.
- **SEAWEED:** cultivated or permitted algae and seaweed harvests.

The multipliers express local supply, buyer specialization, storage infrastructure, and settlement demand. A high multiplier does not mean unlimited buyer capacity. Remote ports may pay more for a scarce class while having low total intake.

### Example destination rates

These are base dockside rates after the island factor, before catch quality, freshness, live condition, or temporary demand modifiers.

| Catch | Market class | Base USD rate | Greyhaven | Shallows | Blackrock | Drowned Parish | Outer Shoals | Hollow Trench |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| Milkfish | INSHORE | $2.30/kg | $2.48 | $2.19 | $2.07 | $2.76 | $1.73 | $1.27 |
| Mangrove red snapper | REEF | $4.60/kg | $4.23 | $5.52 | $5.06 | $3.91 | $4.14 | $3.45 |
| Yellowfin tuna | PELAGIC | $5.23/kg | $4.71 | $4.97 | $6.17 | $3.92 | $6.54 | $5.75 |
| Ruby snapper | DEEPWATER | $7.14/kg | $5.00 | $5.36 | $7.14 | $3.93 | $8.21 | $10.00 |
| Giant tiger prawn | INVERTEBRATE | $6.66/kg | $7.33 | $7.66 | $6.33 | $7.66 | $5.66 | $4.66 |
| Sea grapes / lato | SEAWEED | $2.22/kg wet | $2.33 | $2.55 | $1.89 | $2.44 | $1.67 | $1.11 |

A player may compare prices on a market board before travelling. Travel time, fuel, ice, storage loss, and boat risk must be considered before chasing a distant premium. Selling is permitted only at an actual buyer or contract terminal in that destination market. The game must never apply the destination multiplier merely because the catch originated there.

## 10. Buyer behavior and market modifiers

| Buyer | Preferred stock | Pricing behavior |
|---|---|---|
| Greyhaven Fishmonger | Staples, inshore fish, squid, shrimp, common crab | Stable baseline buyer; limited daily intake |
| Harbor Restaurant | Snapper, emperor, grouper, fresh squid, octopus, live lobster | Pays good / premium grade and may post size-specific orders |
| Smokehouse / Preserver | Anchovy, sardine, scad, mackerel and small pelagics | Bulk demand; accepts lower grade while fresh enough |
| Offshore Dealer | Tuna, wahoo, mahi-mahi, Spanish mackerel, jobfish | Higher-value specialist buyer; limited stock and travel / timing requirements |
| Shellfish Buyer | Mussels, oysters, clams, cockles, approved urchin roe | Requires clean-bed provenance, legal size and safe handling |
| Seaweed Cooperative | Cottonii, spinosum, Gracilaria and managed sea grapes | Farm-gate contracts, wet-weight grading and plot limits |
| Licensed Marine-products Buyer | Approved sea cucumber or restricted harvests | Contract / quota only; never a general walk-up buyer |
| Research Station | First-time aberrant or unnatural records, eligible samples | Finite discovery contracts; no repeat commodity farming |
| Conservation Office | Protected wildlife sightings and safe releases | Pays for valid documentation / rescue work, never buys the animal |

Demand should change only through visible conditions such as a posted order, buyer stock, a festival, storm shortage, or seasonal request. Clamp the demand factor to 0.75–1.35. Do not stack several discounts for the same cause.

## 11. Anti-exploit and economy guardrails

- No buyer has infinite daily capacity for a single species. Use a visible daily intake or contract quota.
- Selling into the same buyer reduces demand only when the buyer's stock / order visibly fills.
- Rare encounter frequency does not also multiply price; the base price already sets species value.
- Research rewards are unique-ID, first-record payouts, not per-catch payouts.
- Protected wildlife, undersized seafood, closed-season catches, prohibited gear catches, polluted shellfish-bed harvests, and quarantined specimens are rejected.
- A premium buyer order is a temporary, explicit contract. It cannot be refreshed by sleeping, reloading, or changing scenes.
- If a species is farmed or harvested from a managed plot, stock recovery and plot yield still limit volume.
- Large fish must use plausible size / weight distributions. Price is per kilogram, not a flat trophy bonus.
- Use the two-decimal USD unit rate displayed in the schedule, then round each completed buyer line to the nearest cent (midpoints away from zero).

## 12. Initial balance targets

These are playtest targets, not guaranteed player earnings:

| Activity | Target gross return | Intended role |
|---|---:|---|
| Short inshore trip | $3.96–$11.10 | Starter income; low gear and fuel risk |
| Productive coastal trip | $11.10–$28.54 | Regular progression income |
| Targeted reef / crab / squid trip | $14.27–$39.64 | Skill and location reward |
| Offshore pelagic trip | $28.54–$79.29 | Mid-to-late progression, meaningful fuel and risk |
| Deep-slope contract trip | $31.72–$87.22 | Specialized gear and controlled catch volume |
| First verified unnatural research record | $19.03–$57.09 | One-time narrative / research reward, not repeatable income |

Review these after fuel, bait, repairs, equipment prices, boat upgrades, storage, and trip durations exist. Net profit—not gross catch value—is the final balance metric.

## 13. Acceptance checks

- Every FSH-001–FSH-100 code has exactly one base price.
- Every INV-001–INV-047 code has exactly one price and unit.
- Every ALG-001–ALG-008 code has exactly one wet-weight price.
- Protected species have no ordinary commodity price.
- No aberrant / unnatural ID has an unbounded or repeatable research reward.
- A 2 kg standard milkfish pays $4.60 at neutral market factor 1.00 before other modifiers.
- Five approved mature sea urchins pay $3.55 before modifiers.
- Sale calculations apply weight, quality, freshness, and demand once each.
- All buyers explain eligibility, discounts, and rejection.

## 14. Related project documents

- [Catchable seafood and market species catalog](../../04_CHARACTERS_AND_ENTITIES/CATCHABLE-SEAFOOD-CATALOG.md)
- [Mutated and unnatural catchable specimens](../../04_CHARACTERS_AND_ENTITIES/MUTATED-AND-UNNATURAL-CATCHES.md)
- [Economy mathematics](ECONOMY-MATHEMATICS.md)
- [Fish population and food-web model](FISH-POPULATION-MODEL.md)
- [Development roadmap](../13_PRODUCTION_MANAGEMENT/DEVELOPMENT-ROADMAP.md)
