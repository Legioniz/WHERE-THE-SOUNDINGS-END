# Catchable Seafood and Market Species Catalog

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed production roster — final ecology and legal review required  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## 1. Purpose

This catalog defines the fish and marine creatures the player may catch, land, process, and sell. It uses the project's provisional warm-water volcanic archipelago and draws on real Western Central Pacific and Philippine fisheries.

**Expanded proposed roster:** 100 fish, 47 invertebrates, 3 broad seaweed harvest categories plus 5 specific seaweed records, and a separate protected / release-only wildlife class. This is a game content proposal, not a claim that every species is abundant, sustainable, or legally harvestable in every real-world location. Exact climate, species distribution, and fictional harvest rules remain provisional.

Philippine marine capture fisheries include pelagic resources such as tuna, sardines, scads, mackerels, and anchovies, and demersal resources such as slipmouth, threadfin bream, squid, shrimp, and blue crab. SEAFDEC's Philippines profile, using BFAR production data for 2023, lists these among the leading marine capture commodities. NFRDI also identifies commercially relevant reef fish families including snappers, groupers, trevallies, emperors, goatfishes, rabbitfishes, and threadfin breams.

References:
- SEAFDEC, [Fisheries Country Profile: Philippines](https://www.seafdec.org/fisheries-country-profile-philippines/)
- BFAR / NFRDI, [Common and Local Names of Marine Fishes of the Philippines](https://repository.nfrdi.da.gov.ph/items/0bc0e276-842e-470a-911d-182f97baf0a3)
- NFRDI, [Commercial and Reef Fishes in the West Philippine Sea](https://nfrdi.da.gov.ph/commercial-and-reef-fishes-in-the-west-philippine-sea/)
- FAO, [Western Central Pacific Vol. 2: Cephalopods, crustaceans, holothurians and sharks](https://www.fao.org/4/w7192e/w7192e00.htm)
- FAO, [Western Central Pacific Vol. 4: Bony fishes](https://www.fao.org/4/x2400e/x2400e00.htm)

## 2. Catalog conventions

### Region codes

| Code | Region |
|---|---|
| GH | Greyhaven: sheltered bay, estuary, river mouth |
| SH | The Shallows: seagrass, reef flats, lagoons, sand channels |
| BR | Blackrock Reach: basalt coast, reef edge, surge channels |
| DP | The Drowned Parish: flooded river valley and brackish ruins |
| OS | The Outer Shoals: offshore ridges, pelagic grounds, seamount edges |
| HT | The Hollow Trench: deep slope and canyon margins; abyssal / hadal floor is generally not a routine catch zone |

A regional assignment means the species may occur there under suitable habitat and environmental conditions, not that it is endemic.

### Rarity

- **Common:** Regularly available when habitat and conditions are suitable.
- **Uncommon:** Depends more on location, tide, time, season, or gear.
- **Scarce:** Requires targeted fishing or a specific habitat.
- **Rare:** Limited high-value encounter; not a dependable farming target.
- **Seasonal:** Availability changes with migration, spawning, or environmental windows.

### Trade-value index

These are balancing multipliers, not real-world prices. Final currency values will be defined in the economy specification.

| Tier | Name | Relative base value | Purpose |
|---|---|---:|---|
| T1 | Bulk | 1.0× | Frequent small catches and bulk lots |
| T2 | Standard | 1.4× | Regular individual fish and seafood |
| T3 | Premium | 2.0× | Higher-demand or targeted catches |
| T4 | Luxury | 3.0× | Difficult, fragile, or high-demand catch |
| T5 | Trophy / Contract | 4.5× | Large specimen or special buyer request |

### Catch methods

- **Rod / bait:** Bottom, float, or mid-water bait fishing.
- **Rod / lure:** Casting or jigging with artificial lures.
- **Handline:** Compact baited line, including deep drop-line variants.
- **Trolling:** Moving boat with trailing lure or bait.
- **Selective net:** School or small-pelagic harvest; requires appropriate equipment and a local permission / quota system.
- **Squid jig:** Vertical or drifting light-assisted jigging.
- **Trap / pot:** Selective crab, lobster, and octopus pots.
- **Hand collection:** Low-tide collection from designated shellfish or urchin beds.
- **Diving / hand capture:** Optional later-game system, not required for the first prototype.

Nets, pots, specialized jigs, and deep drop lines are equipment unlocks. Avoid unrestricted bottom trawling as a default player tool; it would damage the habitats that make the regions distinct.

---

## 3. Fish species — 100 entries

Scientific names are used for stable production identification. Common and local names may vary between ports and languages.

### 3.1 Estuary, harbor, and inshore fish

| ID | Fish | Scientific name | Region / habitat | Method | Rarity | Trade |
|---|---|---|---|---|---|---|
| FSH-001 | Milkfish | *Chanos chanos* | GH, DP; coastal and brackish water | Rod / bait, selective net | Common | T2 |
| FSH-002 | Flathead grey mullet | *Mugil cephalus* | GH, DP; estuary, harbor edge | Rod / bait, selective net | Common | T2 |
| FSH-003 | Asian seabass / barramundi | *Lates calcarifer* | GH, DP; estuary mouth and channels | Rod / bait, rod / lure | Uncommon | T3 |
| FSH-004 | Fourfinger threadfin | *Eleutheronema tetradactylum* | GH, DP; sandy estuary channels | Rod / bait, handline | Uncommon | T3 |
| FSH-005 | Silver sillago | *Sillago sihama* | GH, SH, DP; sand flats and channels | Rod / bait | Common | T2 |
| FSH-006 | Whipfin silverbiddy | *Gerres filamentosus* | GH, SH, DP; brackish shallows | Rod / bait, selective net | Common | T1 |
| FSH-007 | Splendid ponyfish / slipmouth | *Leiognathus splendens* | GH, DP; muddy and sandy bottom | Selective net, light handline | Common | T1 |
| FSH-008 | Spotted scat | *Scatophagus argus* | GH, DP; brackish creeks and pilings | Rod / bait, selective net | Uncommon | T2 |
| FSH-009 | Indian anchovy | *Stolephorus indicus* | GH, SH, OS; coastal pelagic schools | Selective net, light handline | Common / seasonal | T1 |
| FSH-010 | Commerson's anchovy | *Stolephorus commersonnii* | GH, SH; coastal pelagic schools | Selective net, light handline | Common / seasonal | T1 |
| FSH-011 | Goldstripe sardinella | *Sardinella gibbosa* | GH, SH, OS; coastal schools | Selective net, light handline | Common | T1 |
| FSH-012 | Bali sardinella | *Sardinella lemuru* | GH, SH, OS; coastal pelagic schools | Selective net, light handline | Common / seasonal | T1 |
| FSH-013 | Fringescale sardinella | *Sardinella fimbriata* | GH, SH; coastal schools | Selective net, light handline | Uncommon / seasonal | T1 |
| FSH-014 | Tigertooth croaker | *Otolithes ruber* | GH, DP, SH; soft-bottom coastal water | Rod / bait, handline | Uncommon | T2 |
| FSH-015 | Brushtooth lizardfish | *Saurida undosquamis* | SH, BR, DP; sandy or muddy bottom | Rod / bait, handline | Common | T2 |
| FSH-016 | Indian halibut | *Psettodes erumei* | SH, DP, OS; sandy shelf bottom | Bottom rod / bait, handline | Uncommon | T3 |

### 3.2 Reef, rocky shelf, and coastal demersal fish

| ID | Fish | Scientific name | Region / habitat | Method | Rarity | Trade |
|---|---|---|---|---|---|---|
| FSH-017 | White-spotted spinefoot / rabbitfish | *Siganus canaliculatus* | SH, GH; seagrass and reef flat | Rod / bait, selective net | Common | T2 |
| FSH-018 | Little spinefoot | *Siganus spinus* | SH, BR; reef edge and vegetation | Rod / bait, selective net | Uncommon | T2 |
| FSH-019 | Ornate threadfin bream | *Nemipterus hexodon* | SH, DP, OS; sandy shelf and reef margin | Bottom rod / bait, handline | Common | T2 |
| FSH-020 | Japanese threadfin bream | *Nemipterus japonicus* | SH, DP, OS; sandy and muddy shelf | Bottom rod / bait, handline | Common | T2 |
| FSH-021 | Goldband goatfish | *Upeneus moluccensis* | SH, BR; sand beside reef | Bottom rod / bait | Uncommon | T2 |
| FSH-022 | Whitesaddle goatfish | *Parupeneus ciliatus* | SH, BR; reef and rubble bottom | Rod / bait | Uncommon | T3 |
| FSH-023 | Mangrove red snapper | *Lutjanus argentimaculatus* | GH, DP, SH; roots, channels, reef edge | Rod / bait, handline | Uncommon | T4 |
| FSH-024 | Russell's snapper | *Lutjanus russellii* | SH, BR, DP; reef edge and coastal shelf | Rod / bait, handline | Uncommon | T3 |
| FSH-025 | John's snapper | *Lutjanus johnii* | GH, SH, DP; estuary edge and coastal reefs | Rod / bait, handline | Uncommon | T3 |
| FSH-026 | Thumbprint emperor | *Lethrinus harak* | SH, BR; sand and seagrass near reef | Rod / bait | Common | T2 |
| FSH-027 | Pinkear emperor | *Lethrinus lentjan* | SH, BR, OS; reef and sandy shelf | Rod / bait, handline | Uncommon | T3 |
| FSH-028 | Spangled emperor | *Lethrinus nebulosus* | SH, BR, OS; reef and offshore shelf | Rod / bait, handline | Scarce | T4 |
| FSH-029 | Orange-spotted grouper | *Epinephelus coioides* | GH, SH, DP; estuary structure and reef edge | Rod / bait, handline | Uncommon | T4 |
| FSH-030 | Honeycomb grouper | *Epinephelus merra* | SH, BR; shallow coral and rocky reef | Rod / bait | Uncommon | T3 |
| FSH-031 | Leopard coral grouper | *Plectropomus leopardus* | SH, BR, OS; outer reef and reef slope | Rod / bait, rod / lure | Scarce | T5 |
| FSH-032 | Giant trevally | *Caranx ignobilis* | SH, BR, OS; channels, reef edge, open coast | Rod / lure, trolling | Scarce | T4 |
| FSH-033 | Bluefin trevally | *Caranx melampygus* | SH, BR, OS; reef edges and current lines | Rod / lure, rod / bait | Uncommon | T4 |
| FSH-034 | Golden trevally | *Gnathanodon speciosus* | SH, OS; sandy reef margins and offshore schools | Rod / bait, rod / lure | Uncommon | T3 |
| FSH-035 | Java parrotfish | *Scarus javanicus* | SH, BR; coral reef grazer | Rod / bait, limited selective capture | Scarce / quota-limited | T2 |

Parrotfish are a deliberately constrained harvest. Their grazing can influence reef algal balance and substrate condition; fishing pressure should affect the reef's ecological state. They must not become a limitless low-cost farming loop.

### 3.3 Shelf, channel, and offshore pelagic fish

| ID | Fish | Scientific name | Region / habitat | Method | Rarity | Trade |
|---|---|---|---|---|---|---|
| FSH-036 | Shortfin scad | *Decapterus macrosoma* | GH, SH, OS; coastal and offshore schools | Selective net, handline | Common | T1 |
| FSH-037 | Japanese scad | *Decapterus maruadsi* | SH, OS; pelagic schools | Selective net, handline | Common | T1 |
| FSH-038 | Yellowstripe scad | *Selaroides leptolepis* | GH, SH, OS; coastal pelagic schools | Selective net, rod / bait | Common | T1 |
| FSH-039 | Bigeye scad | *Selar crumenophthalmus* | SH, BR, OS; reef-associated and offshore schools | Selective net, handline | Common | T2 |
| FSH-040 | Indian mackerel | *Rastrelliger kanagurta* | GH, SH, OS; coastal pelagic schools | Selective net, trolling | Common / seasonal | T2 |
| FSH-041 | Short-bodied mackerel | *Rastrelliger brachysoma* | GH, SH, OS; coastal schools | Selective net, handline | Uncommon / seasonal | T2 |
| FSH-042 | Narrow-barred Spanish mackerel | *Scomberomorus commerson* | BR, OS; coastal channels and offshore | Trolling, rod / lure | Uncommon | T4 |
| FSH-043 | Talang queenfish | *Scomberoides commersonnianus* | BR, OS; surf edge and open water | Rod / lure, trolling | Uncommon | T3 |
| FSH-044 | Great barracuda | *Sphyraena barracuda* | SH, BR, OS; reef edge and open coast | Rod / lure, trolling | Scarce | T3 |
| FSH-045 | Frigate tuna | *Auxis thazard* | OS; warm offshore surface schools | Trolling, handline | Uncommon / seasonal | T3 |
| FSH-046 | Kawakawa | *Euthynnus affinis* | SH, BR, OS; coastal pelagic predator | Trolling, rod / lure | Uncommon | T3 |
| FSH-047 | Skipjack tuna | *Katsuwonus pelamis* | OS; open-ocean surface schools | Trolling, handline | Seasonal | T4 |
| FSH-048 | Yellowfin tuna | *Thunnus albacares* | OS; offshore pelagic grounds | Trolling, heavy handline | Scarce | T5 |
| FSH-049 | Bigeye tuna | *Thunnus obesus* | OS, upper HT water column; deeper pelagic water | Deep trolling, deep handline | Rare | T5 |
| FSH-050 | Dolphinfish / mahi-mahi | *Coryphaena hippurus* | OS; surface fronts and floating structure | Trolling, rod / lure | Uncommon / seasonal | T4 |
| FSH-051 | Wahoo | *Acanthocybium solandri* | OS; open-water current edges | High-speed trolling | Rare | T5 |
| FSH-052 | Rainbow runner | *Elagatis bipinnulata* | BR, OS; current lines and offshore structures | Trolling, rod / lure | Uncommon | T3 |
| FSH-053 | Black pomfret | *Parastromateus niger* | GH, SH, OS; coastal and shelf water | Handline, selective net | Uncommon | T3 |
| FSH-054 | Silver pomfret | *Pampus argenteus* | GH, DP, OS; coastal and shelf water | Handline, selective net | Scarce | T4 |
| FSH-055 | Largehead hairtail | *Trichiurus lepturus* | GH, DP, OS; midwater and coastal shelf | Handline, night fishing | Uncommon / seasonal | T3 |
| FSH-056 | Flying fish group | *Cheilopogon* spp. | SH, OS; surface pelagic schools | Selective net, light handline | Seasonal | T2 |

**Deep-water boundary:** Not every deep-sea animal is a commercial catch. Deepwater species may be discovered, photographed, or sampled by quest, but a marketable deep-water fish needs a credible capture method, handling rule, and habitat population. Ordinary gear must not harvest unlimited abyssal stock from the surface.

---

## 4. Catchable invertebrates — 47 entries

| ID | Creature | Scientific name | Region / habitat | Method | Rarity | Trade |
|---|---|---|---|---|---|---|
| INV-001 | Small shrimp / alamang group | *Acetes* spp. | GH, DP; estuary and coastal swarms | Fine-mesh selective net | Common / seasonal | T1 |
| INV-002 | Giant tiger prawn | *Penaeus monodon* | GH, DP, SH; estuary and soft-bottom grounds | Baited trap, selective net | Uncommon | T4 |
| INV-003 | Banana prawn | *Fenneropenaeus merguiensis* | GH, DP, SH; estuary and muddy bottom | Baited trap, selective net | Common / seasonal | T3 |
| INV-004 | Greasyback shrimp | *Metapenaeus ensis* | GH, DP, SH; muddy and sandy bottom | Selective net, baited trap | Common | T2 |
| INV-005 | Blue swimming crab | *Portunus pelagicus* species complex | GH, SH, DP; seagrass edge and soft bottom | Crab pot, baited trap | Common | T3 |
| INV-006 | Giant mud crab | *Scylla serrata* | GH, DP; mangrove creeks and estuary mud | Crab pot, baited trap | Uncommon | T4 |
| INV-007 | Spanner crab | *Ranina ranina* | BR, OS; sandy offshore shelf | Specialized baited trap | Scarce | T4 |
| INV-008 | Ornate spiny lobster | *Panulirus ornatus* | BR, OS; reef slope and rocky shelter | Lobster pot, permitted hand capture | Scarce | T5 |
| INV-009 | Painted spiny lobster | *Panulirus versicolor* | SH, BR; reef crevices | Lobster pot, permitted hand capture | Uncommon | T4 |
| INV-010 | Slipper lobster | *Thenus orientalis* | SH, OS; sandy and muddy shelf | Bottom trap, selective gear | Uncommon | T3 |
| INV-011 | Bigfin reef squid | *Sepioteuthis lessoniana* | SH, BR; shallow reef and seagrass edge | Squid jig | Common / seasonal | T3 |
| INV-012 | Indian squid | *Uroteuthis duvaucelii* | SH, DP, OS; coastal and shelf water | Squid jig, light-assisted jigging | Common / seasonal | T3 |
| INV-013 | Pharaoh cuttlefish | *Sepia pharaonis* | SH, BR, DP; sand near reef and seagrass | Squid jig, baited trap | Uncommon | T4 |
| INV-014 | Day octopus | *Octopus cyanea* | SH, BR; reef flats, rubble, and rock dens | Selective octopus pot, permitted hand capture | Uncommon | T4 |
| INV-015 | Asian green mussel | *Perna viridis* | GH, DP; sheltered coastal structures | Hand collection at approved beds | Common | T2 |
| INV-016 | Tropical cupped oyster | *Crassostrea* / *Magallana* spp. | GH, DP; estuary and sheltered hard substrate | Hand collection at approved beds | Common | T2 |
| INV-017 | Blood cockle | *Tegillarca granosa* | GH, DP; muddy intertidal flats | Hand collection, shallow sediment rake | Uncommon | T3 |
| INV-018 | Undulated surf clam | *Paphia undulata* | GH, DP, SH; sand and mud flats | Hand collection, shallow sediment rake | Common | T2 |
| INV-019 | Collector sea urchin | *Tripneustes gratilla* | SH, BR; seagrass and reef edge | Hand collection with protection rules | Uncommon | T3 |
| INV-020 | Mantis shrimp / spearer group | *Harpiosquilla* spp. | GH, DP, SH; soft-substrate burrows | Specialized burrow trap | Uncommon | T3 |
| INV-021 | Giant river prawn | *Macrobrachium rosenbergii* | GH upper river and freshwater reaches | Baited trap, handline | Uncommon | T3 |

### Handling and quality

- Shrimp and prawns lose quality quickly without chilling or proper storage.
- Live crabs and lobsters receive a better price when landed intact and kept alive.
- Squid and cuttlefish quality falls with poor handling, heat, or long delays before chilling.
- Octopus may escape poorly designed traps and hide in crevices.
- Mussels, oysters, cockles, and clams require clean approved beds. Contaminated-water closures make a harvest unsellable.
- Sea urchin is a specialty roe product, not a generic whole-animal sale. Only mature, approved individuals produce marketable roe.
- Shellfish beds are specific locations with recovery limits; do not let players harvest every wall, reef, or wreck.
- Mantis shrimp can damage traps and needs species-appropriate handling.

## 5. Optional marine plant harvests

These are not animals, but they can broaden a coastal economy through managed cultivation.

| ID | Harvest | Scientific group | Habitat | Method | Trade role |
|---|---|---|---|---|---|
| ALG-001 | Cottonii / carrageenan seaweed | *Kappaphycus* spp. | Managed shallow-water farm plots | Harvest cultivation lines | Bulk trade / farm contract |
| ALG-002 | Spinosum seaweed | *Eucheuma* spp. | Managed shallow-water farm plots | Harvest cultivation lines | Bulk trade / farm contract |
| ALG-003 | Gracilaria seaweed | *Gracilaria* spp. | Suitable sheltered estuary or coastal plots | Cut-and-regrow harvest | Food / processing trade |

Use cultivation or designated managed plots for repeatable seaweed income. Do not let players strip natural reef habitat for unlimited resources.

---

## 6. Protected, release-only, and non-commercial wildlife

The game needs valuable encounters that cannot be turned into ordinary sale items. These reinforce conservation, identification gameplay, and the distinction between rare wildlife and market goods.

**Project rule:** These are not normal catch-and-sell targets. If accidentally hooked, entangled, or disturbed, the intended response is to release, disentangle, withdraw, or report. No fishmonger sale is allowed. This is a conservative game-design rule, not a complete statement of every domestic law.

| Wildlife class | Examples | Game treatment |
|---|---|---|
| Marine turtles | Green, hawksbill, loggerhead, leatherback, olive ridley | Observe, protect, or release from accidental interaction; never sell |
| Marine mammals | Dolphins, whales, porpoises | Observation, route encounter, rescue / report quest; never sell |
| Dugong | Dugong | Protected wildlife encounter; never sell |
| Whale shark | Whale shark | Observe and photograph / survey; never sell |
| Manta and devil rays | *Mobula* and *Manta* species | Observe, release, or research record; never sell |
| Sawfish | All sawfish species | Protected encounter; release / report |
| CITES-listed sharks | Listed requiem sharks, hammerheads, threshers, and other listed species | Release / report; no general shark commodity |
| Giant clams | *Tridacna* and *Hippopus* species | Observe / survey only |
| Corals | Stony corals, black corals, and other protected coral taxa | Never harvest as sale goods |
| Seahorses | *Hippocampus* species | Observe / release; never sell |
| Chambered nautilus | *Nautilus* species | Observe / research record; never sell |
| Queen conch and protected gastropods | Applicable listed conch and gastropod species | Exclude from standard market; verify status before any use |
| Protected sea cucumbers | Listed *Holothuria* and *Thelenota* species | Research observation only |
| Nursery / spawning individuals | Animals at authored breeding and nursery sites | Closed-window protection; leave site undisturbed |

CITES listings are trade controls; do not simplify them into a claim that every member of a broad animal group has identical protection under every domestic law. For this game, keep these examples out of the sellable roster.

References:
- CITES, [The CITES species](https://cites.org/eng/disc/species.php)
- NOAA Fisheries, [Marine species in CITES Appendices](https://www.fisheries.noaa.gov/national/international-affairs/convention-international-trade-endangered-species-wild-fauna-and)
- Philippine NFRDI, [National Red List assessment for sharks and rays](https://nfrdi.da.gov.ph/2024/11/18/parlc-leads-natl-red-list-assessment-for-sharks-and-rays/)
- BFAR / DTI, [List of prohibited and regulated fishery products](https://trustmark.dti.gov.ph/LIST-OF-PROHIBITED-AND-REGULATED-PRODUCTS)

Protected encounters can still reward non-extractive actions: photographs, research logs, successful release, habitat surveys, NPC trust, or conservation contracts.

---

## 7. Anomalous catches and research specimens

Anomalous entities are not ordinary edible fish. The player may recover a clue, trace, shed fragment, or quest sample only when its encounter specification permits it.

| Source | Recoverable item | Sale rule |
|---|---|---|
| The Glassback | Reflective plate fragment | Research contract only; never food |
| The Cliffblind | Basalt-like cartilage / mineralized tissue sample | Research contract only |
| The Bell Eel | Resonant tissue trace or acoustic residue | Quest evidence; no ordinary market |
| The Countercurrent | Unusual scale or membrane fragment after a defined event | Research contract only |
| The Marrow Choir | Resonant bone sample | Research contract only; do not harvest the food-fall site |
| The Borrowed Light | Bioluminescent residue in a sealed sample | Research contract only |
| The Sleeper | Quest-specific trace or recovered material | Story-controlled reward, not repeatable farming |

Anomalous specimens are rare, quest-gated, and non-renewable unless a story rule explicitly permits otherwise. Their value is narrative, scientific, or contractual—not ordinary seafood income. A player cannot farm hostile encounters for unlimited money.

---

## 8. Market and catch value rules

Suggested sale model:

**Sale Value = Base Species Value × Landed Quantity × Quality Multiplier × Freshness Multiplier × Local Demand Modifier × Legal Eligibility**

Initial balancing ranges:

| Factor | Suggested range | Notes |
|---|---:|---|
| Quality | 0.60–1.25 | Size grade, clean landing, no major damage |
| Freshness | 0.35–1.00 | Time and storage since catch |
| Local demand | 0.75–1.35 | Buyer stock and current orders |
| Legal eligibility | 0 or 1 | Protected, undersized, or closed-season catch cannot enter ordinary sale |

These are proposed tuning ranges, not final economy values. Apply quality and freshness to the landed product rather than arbitrary dice rolls. Buyers should explain why a catch receives a lower grade.

### Product forms

- Whole fresh fish: quick sale, low preparation cost.
- Chilled fish: better freshness retention, requires ice / cold storage.
- Fillets: lower landed weight but higher value per usable unit; processing time required.
- Whole live crab / lobster: premium if intact and kept alive.
- Chilled shrimp / squid: value depends strongly on handling.
- Dried / salted small fish: bulk preservation, less freshness pressure.
- Roe / specialty cuts: only for eligible species and buyers; never from protected breeding individuals.

Processing must not create free value. It consumes time, storage, packaging, and sometimes reduces usable weight.

### Buyers

| Buyer | Preferred goods | Behavior |
|---|---|---|
| Greyhaven Fishmonger | Common fish, mullet, milkfish, squid, shrimp, crab | Reliable everyday buyer, moderate demand |
| Harbor Restaurant | Snapper, emperor, grouper, squid, octopus, lobster | Pays for fresh, correctly sized premium catch |
| Smokehouse / Preserver | Sardines, anchovies, scads, mackerels, small fish | Buys bulk lots |
| Offshore Dealer | Tuna, wahoo, mahi-mahi, Spanish mackerel | High-value pelagic trade, variable demand |
| Shellfish Buyer | Mussels, oysters, clams, cockles | Requires clean-bed provenance and proper handling |
| Research Station | Non-protected survey samples and anomaly evidence | Pays through contracts, not commodity pricing |
| Conservation Office | Protected wildlife records and successful releases | Rewards documentation and safe release; never buys the animal |

Market demand can change with NPC orders, menus, local events, supply shortages after storms, seasonal migration, buyer inventory, and catch freshness. Price changes must be visible through buyer requests, notices, or dialogue, not arbitrary hidden swings.

---

## 9. Catch eligibility and ecological integration

A catch attempt is eligible only when:
- The species inhabits the current region and depth band.
- Habitat suitability and environmental conditions are valid.
- Selected gear is compatible.
- Tide, time, weather, and season allow its activity.
- Regional stock is available.
- Size, season, quota, and protected-status rules permit retention.

Catch outcome combines local stock, gear compatibility, bait / lure, depth, habitat, time, tide, weather, migration, player technique, size distribution, and disturbance. Do not make one rarity roll the entire fishing system.

Connect every catch to the existing stock model:
- Successful catch removes regional stock.
- Migration transfers stock; it does not duplicate it.
- Spawning adds recruitment only on valid scheduled windows.
- Wildlife feeding removes prey stock.
- Habitat quality influences carrying capacity and recovery.
- Repeated fishing in one patch lowers local catch-per-unit-effort.
- Schools may move to adjacent suitable habitat.
- Nursery and spawning sites can have seasonal closures.
- Selective gear reduces non-target catch.
- Bycatch can be released with a survival chance based on handling and species.
- Ordinary species must not be permanently erased by one routine trip.

See [Fish Population and Food-Web Model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md) for the authoritative stock mathematics.

---

## 10. Prototype selection

Do not implement all 77 animal entries at once. The initial fishing prototype should prove different habitats, catch methods, value grades, and ecological effects.

### MVP fish (8)
1. Milkfish — Greyhaven routine catch.
2. Flathead grey mullet — tidal channel catch.
3. Indian anchovy — small schooling bulk catch.
4. Indian mackerel — pelagic school catch.
5. Silver sillago — shallow sand-bottom catch.
6. Ornate threadfin bream — bottom fishing.
7. Rabbitfish — seagrass / reef grazer with habitat feedback.
8. Mangrove red snapper — premium structure-associated catch.

### MVP invertebrates (3)
1. Blue swimming crab — crab pot.
2. Giant tiger prawn — trap / selective net.
3. Bigfin reef squid — squid jig.

### MVP non-sale wildlife (2)
1. Green turtle — observation / accidental interaction and release.
2. A locally validated reef predator — distant ecological presence, not a commodity.

This MVP proves that fishing is not merely changing a random fish icon: the player selects a location, method, timing, gear, and storage approach.

---

## 11. Data record template

Every implemented species definition should have:

- speciesId
- displayName
- scientificName
- category (fish, crustacean, cephalopod, bivalve, other)
- regions[]
- habitatTags[]
- minDepthM, maxDepthM
- temperatureRangeC where gameplay uses it
- currentPreference
- activePeriods[]
- seasonalWindow
- migrationRoutes[]
- dietTags[]
- ecologicalRole
- rarityClass
- tradeTier
- saleUnit
- compatibleGear[]
- baitTags[] and lureTags[]
- sizeDistribution
- stockId
- minimumRetainSize / retentionRule where used
- handlingClass
- freshnessDecayProfile
- protectedStatus
- notesAndReferences[]

Separate the authored species definition from runtime stock, individual catch instance, and buyer price. Do not store current population or market demand inside the shared species definition.

## 12. Acceptance criteria

- Every ordinary sale species has a valid habitat, catch method, and buyer.
- Scientific names identify species; display names may vary by port.
- The proposed sellable roster contains 56 fish and 21 invertebrates.
- Protected / release-only wildlife cannot be sold through ordinary markets.
- Anomalous specimens use quest / research contracts and cannot be farmed endlessly.
- Gear incompatibility blocks or sharply lowers catch eligibility.
- Stock depletion changes local availability but does not permanently delete ordinary species.
- Freshness and product form matter without making inventory management tedious.
- Catch feedback explains wrong depth, wrong gear, inactive species, empty stock, protected status, and closed habitat.
- The MVP uses data-driven fishing definitions and stock cohorts, not 77 unique AI agents.

## 13. Research references

### Philippine species and commercial fisheries
- SEAFDEC, [Fisheries Country Profile: Philippines](https://www.seafdec.org/fisheries-country-profile-philippines/) — marine capture species and 2023 production figures reported using BFAR data.
- BFAR / NFRDI, [Common and Local Names of Marine Fishes of the Philippines](https://repository.nfrdi.da.gov.ph/items/0bc0e276-842e-470a-911d-182f97baf0a3) — local and scientific nomenclature.
- NFRDI, [Commercial and Reef Fishes in the West Philippine Sea](https://nfrdi.da.gov.ph/commercial-and-reef-fishes-in-the-west-philippine-sea/) — economically relevant reef and finfish families.
- BFAR, [Fresh, chilled, and frozen fishery products](https://www.bfar.da.gov.ph/wp-content/uploads/2023/08/AUGUST-CY2023-LIST-OF-FRESH-CHILLED-FROZEN-PRODUCTS-FISHERY-ESTABLISHMENT.pdf) — examples of traded squid, octopus, cuttlefish, shrimp, crab, lobster, grouper, hairtail, and clam products.
- Philippine Statistics Authority, [Fisheries Situationer](https://psa.gov.ph/statistics/fisheries-situationer) — current fisheries production reporting.

### Species identification and biology
- FAO, [Western Central Pacific Vol. 2: Cephalopods, crustaceans, holothurians and sharks](https://www.fao.org/4/w7192e/w7192e00.htm).
- FAO, [Western Central Pacific Vol. 4: Bony fishes](https://www.fao.org/4/x2400e/x2400e00.htm).
- DENR, [Philippine Coastal Management Guidebook Series No. 6](https://faspselib.denr.gov.ph/uploads/materials/f14e213d-42b2-4073-9511-4e4e86da5615/4ea4b90a-2cfd-4ef9-a255-c7b9030a6720.pdf).

### Protected species and trade controls
- CITES, [The CITES species](https://cites.org/eng/disc/species.php).
- NOAA Fisheries, [Marine species in CITES Appendices](https://www.fisheries.noaa.gov/national/international-affairs/convention-international-trade-endangered-species-wild-fauna-and).
- Philippine NFRDI, [National Red List assessment for sharks and rays](https://nfrdi.da.gov.ph/2024/11/18/parlc-leads-natl-red-list-assessment-for-sharks-and-rays/).

## 14. Related project documents

- [Fish Species Index](FISH-SPECIES.md)
- [Creature Bible](CREATURE-BIBLE.md)
- [Regional Ecology and Creature Distribution](../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md)
- [Fish Population and Food-Web Model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md)
- [Economy Mathematics](../07_GAME_RULES_AND_BALANCING/ECONOMY-MATHEMATICS.md)
- [Creature Life Cycles and Ecological Behaviour](../12_AI_AND_SIMULATION/CREATURE-LIFE-CYCLES-AND-ECOLOGY.md)


### 3.4 Additional reef, coastal, shelf-edge, and freshwater fish (FSH-057–FSH-100)

This expansion adds commercially plausible fish from reef slopes, current corridors, exposed coast, soft-bottom habitat, upper deep slopes, and the freshwater reaches feeding Greyhaven's estuary. The deep-bottom species are catchable only on the upper slope / deep-drop grounds described below, not from the hadal floor.

| ID | Fish | Scientific name | Region / habitat | Method | Rarity | Trade |
|---|---|---|---|---|---|---|
| FSH-057 | Redbelly yellowtail fusilier | *Caesio cuning* | SH, BR, OS; reef slopes and current edges | Rod / bait, selective net | Common / seasonal | T2 |
| FSH-058 | Blue-and-gold fusilier | *Caesio caerulaurea* | SH, BR, OS; reef edge and midwater schools | Rod / bait, selective net | Uncommon | T2 |
| FSH-059 | Paddletail snapper | *Lutjanus gibbus* | SH, BR, OS; reef slopes and drop-offs | Rod / bait, handline | Uncommon | T3 |
| FSH-060 | Blackspot snapper | *Lutjanus fulviflamma* | GH, DP, SH; estuary edge, lagoon and coastal reef | Rod / bait | Common / uncommon | T2 |
| FSH-061 | Red emperor | *Lutjanus sebae* | SH, BR, OS; reef slope and shelf edge | Bottom rod / bait, handline | Scarce | T4 |
| FSH-062 | Malabar grouper | *Epinephelus malabaricus* | GH, DP, SH; estuary structures, channels and coastal reef | Rod / bait, handline | Uncommon | T4 |
| FSH-063 | Brown-marbled grouper | *Epinephelus fuscoguttatus* | SH, BR, OS; outer reef and reef slope | Rod / bait, handline | Scarce | T5 |
| FSH-064 | Wire-netting cod | *Epinephelus chlorostigma* | BR, OS, HT upper slope; rocky and deep shelf structure | Deep handline, bottom rod / bait | Scarce | T4 |
| FSH-065 | Lunar-tail grouper | *Variola louti* | SH, BR, OS; clear outer reef and rocky drop-off | Rod / bait, rod / lure | Scarce | T4 |
| FSH-066 | Painted sweetlips | *Diagramma pictum* | SH, BR, DP; reef, rubble and sheltered coastal structures | Rod / bait | Uncommon | T3 |
| FSH-067 | Blackspotted rubberlip | *Plectorhinchus gibbosus* | SH, BR; sandy reef margins and rubble | Rod / bait | Uncommon | T2 |
| FSH-068 | Iodine bream | *Gymnocranius euanus* | OS, HT upper slope; deep shelf and hardground | Deep handline | Rare | T4 |
| FSH-069 | Large-eyed sea bream | *Wattsia mossambica* | OS, HT upper slope; deep shelf and slope | Deep handline | Rare | T4 |
| FSH-070 | Orange-spotted spinefoot | *Siganus guttatus* | GH, DP, SH; estuary edge, seagrass and reef flat | Rod / bait, selective net | Common | T2 |
| FSH-071 | Blue-spotted cornetfish | *Fistularia commersonii* | SH, BR, OS; reef edge and sandy channels | Rod / lure, light handline | Uncommon | T2 |
| FSH-072 | Hound needlefish | *Tylosurus crocodilus* | GH, SH, OS; surface waters, channels and reef margins | Rod / lure, surface bait | Uncommon | T2 |
| FSH-073 | Black-barred halfbeak | *Hemiramphus far* | GH, SH, OS; surface schools near coast and islands | Light handline, selective net | Common / seasonal | T1 |
| FSH-074 | Bartail flathead | *Platycephalus indicus* | GH, DP, SH; sandy and muddy bottom | Bottom rod / bait | Uncommon | T2 |
| FSH-075 | Large-scale tonguesole | *Paraplagusia blochii* | GH, DP; soft-bottom estuary and coastal flats | Bottom rod / bait, selective net | Uncommon | T2 |
| FSH-076 | Bigeye bream | *Monotaxis grandoculis* | SH, BR, OS; reef slope and sandy reef patches | Rod / bait, handline | Uncommon | T3 |
| FSH-077 | Black jack | *Caranx lugubris* | BR, OS; exposed reefs, current lines and offshore structures | Rod / lure, trolling | Scarce | T4 |
| FSH-078 | Almaco jack | *Seriola rivoliana* | OS, HT upper slope; offshore structures and deep current edges | Trolling, deep handline | Scarce | T4 |
| FSH-079 | Dogtooth tuna | *Gymnosarda unicolor* | BR, OS; steep reef edges and offshore pinnacles | Heavy rod / lure, trolling | Rare | T5 |
| FSH-080 | Dorab wolf-herring | *Chirocentrus dorab* | GH, DP, OS; coastal pelagic water and estuary mouths | Rod / bait, trolling, selective net | Uncommon | T3 |
| FSH-081 | Redtail scad | *Decapterus kurroides* | SH, OS; coastal and offshore pelagic schools | Selective net, handline | Uncommon / seasonal | T2 |
| FSH-082 | Picnic seabream | *Acanthopagrus berda* | GH, DP; estuary, brackish channels and coastal flats | Rod / bait | Uncommon | T3 |
| FSH-083 | Goldlined seabream | *Rhabdosargus sarba* | GH, SH, DP; estuary mouth, reef flat and sandy coast | Rod / bait | Uncommon | T3 |
| FSH-084 | Yellowstripe goatfish | *Mulloidichthys flavolineatus* | SH, BR; sand patches beside reef | Bottom rod / bait | Common / uncommon | T2 |
| FSH-085 | Dash-and-dot goatfish | *Parupeneus barberinus* | SH, BR; reef rubble and sandy channels | Bottom rod / bait | Uncommon | T3 |
| FSH-086 | Goldsaddle goatfish | *Parupeneus cyclostomus* | SH, BR, OS; reef slope and outer reef | Bottom rod / bait | Scarce | T4 |
| FSH-087 | Ruby snapper | *Etelis carbunculus* | OS, HT upper slope; deep slope, typically around 200–400 m in regional deep-water fisheries | Deep drop-line / deep handline | Rare | T5 |
| FSH-088 | Longtail red snapper | *Etelis coruscans* | OS, HT upper slope; deep slope, typically around 200–400 m in regional deep-water fisheries | Deep drop-line / deep handline | Rare | T5 |
| FSH-089 | Small-tooth jobfish | *Aphareus rutilans* | OS, HT upper slope; deep reef and slope | Deep drop-line / deep handline | Scarce | T4 |
| FSH-090 | Green jobfish | *Aprion virescens* | BR, OS; reef drop-offs and upper slope | Deep handline, rod / bait | Scarce | T4 |
| FSH-091 | Rosy jobfish | *Pristipomoides filamentosus* | OS, HT upper slope; deep shelf and slope, commonly 50–300 m in regional fisheries | Deep drop-line / deep handline | Rare | T5 |
| FSH-092 | Purple-cheek jobfish | *Pristipomoides multidens* | OS, HT upper slope; deep shelf and slope, commonly 50–300 m | Deep drop-line / deep handline | Rare | T5 |
| FSH-093 | Banded flower snapper | *Pristipomoides zonatus* | OS, HT upper slope; deep shelf and seamount flanks | Deep drop-line / deep handline | Rare | T5 |
| FSH-094 | Yellow-finned jobfish | *Pristipomoides flavipinnis* | OS, HT upper slope; deep shelf and slope | Deep drop-line / deep handline | Scarce | T4 |
| FSH-095 | Kalikali / pink snapper | *Pristipomoides sieboldii* | OS, HT upper slope; deep reef and shelf break | Deep drop-line / deep handline | Rare | T5 |
| FSH-096 | Flower snapper | *Pristipomoides amoenus* | OS, HT upper slope; deep hardground and slope | Deep drop-line / deep handline | Rare | T5 |
| FSH-097 | Asian swamp eel | *Monopterus albus* | GH upper river, marsh and slow freshwater channels | Baited trap, handline | Uncommon | T3 |
| FSH-098 | Striped snakehead | *Channa striata* | GH upper river, floodplain pools and vegetated freshwater | Rod / bait, trap | Uncommon | T3 |
| FSH-099 | Climbing perch | *Anabas testudineus* | GH upper river, marsh and low-oxygen freshwater pools | Rod / bait, trap | Common | T2 |
| FSH-100 | Philippine broadhead catfish | *Clarias macrocephalus* | GH upper river, marsh and muddy freshwater channels | Rod / bait, trap | Uncommon | T3 |

**Deep-water catch boundary:** The Hollow Trench's hadal floor is not a routine fishery. The deep-drop species above are based on upper-slope regional fisheries at roughly 50–400 m, with individual species-specific ranges. The player needs purpose-built deep-line equipment, correct line capacity, suitable weather, and a mapped slope station. These species are not caught from a shallow boat by ordinary rod equipment. Regional deep-bottom fisheries commonly target ruby snappers and jobfishes, with *Etelis carbunculus* and *E. coruscans* often reported around 200–400 m and several *Pristipomoides* species around 50–300 m. See the SPC deep-bottom fishing guide and the regional deep-snapper study in the references below.

The freshwater fish are restricted to the upper river and connected floodplain habitats of Greyhaven. They do not appear in saltwater reef or offshore tables.

---

### 4.2 Additional crabs, crustaceans, mollusks, and other seafood (INV-022–INV-047)

| ID | Creature / seafood | Scientific name | Region / habitat | Method | Rarity | Trade |
|---|---|---|---|---|---|---|
| INV-022 | Orange mud crab | *Scylla olivacea* | GH, DP; mangrove creeks and muddy estuary | Crab pot, baited trap | Uncommon | T4 |
| INV-023 | Purple mud crab | *Scylla tranquebarica* | GH, DP; larger estuary channels and mangrove mud | Crab pot, baited trap | Scarce | T4 |
| INV-024 | Crucifix crab | *Charybdis feriata* | GH, SH, DP; coastal shelf and sandy / muddy bottom | Crab pot, baited trap | Uncommon | T3 |
| INV-025 | Three-spot swimming crab | *Portunus sanguinolentus* | GH, SH, DP; sandy shelf and nearshore bottom | Crab pot, baited trap | Uncommon | T3 |
| INV-026 | Long-eyed swimming crab | *Podophthalmus vigil* | GH, DP, SH; sandy and muddy coastal bottom | Crab pot, baited trap | Scarce / bycatch | T3 |
| INV-027 | Crenate swimming crab | *Thalamita crenata* | SH, DP; reef rubble, mangrove edge and shallow bottom | Small crab pot, hand collection where permitted | Uncommon / bycatch | T2 |
| INV-028 | Common banded mantis shrimp | *Lysiosquillina maculata* | GH, DP, SH; long burrows in sand and mud | Specialized burrow trap | Scarce | T4 |
| INV-029 | Sculptured mitten lobster | *Parribacus antarcticus* | SH, BR; reef rubble and rocky crevices | Lobster pot, permitted hand capture | Scarce | T4 |
| INV-030 | Longleg spiny lobster | *Panulirus longipes* | BR, SH, OS; rocky reef and reef slope shelter | Lobster pot, permitted hand capture | Scarce | T4 |
| INV-031 | Donkey's ear abalone | *Haliotis asinina* | BR, SH; wave-washed rocky reef with algal food | Licensed farm / managed harvest, permitted hand collection | Scarce | T5 |
| INV-032 | Asian hard clam | *Meretrix meretrix* | GH, DP; sandy and muddy intertidal flats | Hand collection, shallow sediment rake | Uncommon | T3 |
| INV-033 | Razor clams | *Solen* spp. | GH, DP, SH; fine sand and mud flats | Hand collection, sediment pump in designated beds | Seasonal / uncommon | T3 |
| INV-034 | Pen shell | *Atrina pectinata* | GH, DP, SH; soft sediment with stable seagrass or muddy bottom | Hand collection / careful small-scale harvest | Scarce | T3 |
| INV-035 | Dog conch | *Laevistrombus canarium* | GH, SH, DP; seagrass, sand and mud flats | Hand collection at designated beds | Uncommon | T3 |
| INV-036 | Spider conch | *Lambis lambis* | SH, BR; reef flat and seagrass edge | Hand collection at designated beds | Scarce | T4 |
| INV-037 | Sandfish sea cucumber | *Holothuria scabra* | GH, SH, DP; clean seagrass and sand-mud flats | Licensed, size-controlled collection; processing contract | Very scarce / controlled | T5 contract |
| INV-038 | Warty sea cucumber | *Stichopus horrens* | SH, DP; reef, rubble and seagrass edge | Licensed, size-controlled collection; processing contract | Scarce / controlled | T4 contract |
| INV-039 | Deepwater redfish sea cucumber | *Actinopyga echinites* | SH, BR; reef and seagrass-associated habitat | Licensed, size-controlled collection; processing contract | Scarce / controlled | T4 contract |
| INV-040 | Lollyfish sea cucumber | *Holothuria atra* | SH, BR; reef flat and seagrass | Licensed, size-controlled collection; processing contract | Uncommon / controlled | T3 contract |
| INV-041 | Leopardfish sea cucumber | *Bohadschia argus* | SH, BR; coral rubble and reef flat | Licensed, size-controlled collection; processing contract | Scarce / controlled | T4 contract |
| INV-042 | Edible jellyfish | *Rhopilema hispidum* | GH, SH, OS; seasonal coastal and offshore bloom waters | Dedicated non-damaging net / bloom harvest | Seasonal | T3 processed |
| INV-043 | Edible squid / lokos | *Photololigo edulis* | SH, DP, OS; coastal shelf and sand-mud bottom | Squid jig, light-assisted jigging | Common / seasonal | T3 |
| INV-044 | Spineless cuttlefish | *Sepia lycidas* | SH, DP, OS; sandy and muddy shelf | Cuttlefish jig, baited trap | Uncommon | T4 |
| INV-045 | Western king prawn | *Penaeus latisulcatus* | DP, SH, OS; sandy and muddy shelf | Selective net, baited prawn trap | Uncommon | T3 |
| INV-046 | Palm shrimp | *Metapenaeus palmensis* | GH, DP, SH; estuary and soft-bottom coastal ground | Selective net, baited trap | Uncommon | T2 |
| INV-047 | Asian moon scallop | *Amusium pleuronectes* | DP, SH, OS; sandy shelf and offshore soft bottom | Selective scallop dredge / small-scale pot where designed | Scarce | T4 |

**Crab and bycatch note:** The Philippine Journal of Fisheries' Western Visayan Sea blue-swimming-crab assessment records *Charybdis feriata*, *Portunus sanguinolentus*, *Podophthalmus vigil*, and *Thalamita crenata* in crab-pot catches, alongside *Portunus pelagicus*. Use these as plausible mixed crab-ground catches, not as proof that every region has equal abundance. Gear selectivity, undersized releases, and bycatch survival should be modeled.

**Sea cucumber restriction:** Sea cucumbers must be a controlled specialist activity, never an unlimited common harvest. BFAR Administrative Circular 248 (2013) set permit / transport requirements and a minimum dried-size rule for commercial collection and trade. The real-world rule is a research reference, not a complete legal interpretation; check current regulations before final release. In game, use a licensed buyer, size and maturity checks, closed grounds, slow stock recovery, and a drying / processing stage. The player cannot sell freshly collected sea cucumbers through the ordinary fishmonger.

**Jellyfish safety:** *Rhopilema hispidum* has been reported as an edible and commercially important species in Palawan. A jellyfish bloom must still be species-identified; never allow unidentified or dangerous jellyfish to become food. The NFRDI-linked Philippine Journal of Fisheries policy brief distinguishes edible species from harmful jellyfish and documents the risks of stings and blooms.

---

### 5.1 Species-level seaweed and edible algae records

The earlier ALG-001–ALG-003 entries remain broad market categories. These specific records allow the player to distinguish edible sea grapes, carrageenan-farm seaweeds, and agar-producing seaweeds without double-counting the broad categories as separate species.

| ID | Harvest | Scientific name | Habitat / production | Method | Trade role |
|---|---|---|---|---|---|
| ALG-004 | Sea grapes / lato | *Caulerpa lentillifera* | GH, SH; sheltered shallow beds or managed plots | Careful hand harvest / farm harvest | Fresh food, premium local market |
| ALG-005 | Spinosum seaweed | *Eucheuma denticulatum* | SH, OS; managed seaweed farm lines | Harvest cultivation lines | Carrageenan raw material |
| ALG-006 | Cottonii seaweed | *Kappaphycus alvarezii* | SH, OS; managed shallow farm plots | Harvest cultivation lines | Carrageenan raw material |
| ALG-007 | Edible red agar seaweed | *Gracilaria edulis* | GH, DP, SH; sheltered estuary and coastal plots | Cut-and-regrow harvest | Agar / food processing |
| ALG-008 | Sea grape / Caulerpa | *Caulerpa racemosa* | GH, SH; suitable sheltered shallow beds | Careful hand harvest | Fresh food / local market |

Do not assume every species in the genus *Caulerpa* is interchangeable in food handling. Farmed plots and identified edible species are safer and more predictable than unrestricted wild gathering. Philippine sources identify *Caulerpa lentillifera* as a cultivated edible seaweed, and Philippine seaweed farming commonly includes *Eucheuma*, *Kappaphycus*, *Gracilaria*, and *Caulerpa* groups.

---

## 15. Expanded geography, depth, and conservation rules

### 15.1 New geography-to-catch links

- **Greyhaven upper river:** Asian swamp eel, striped snakehead, climbing perch, and Philippine broadhead catfish. These are freshwater catches and must not spawn in saltwater harbour zones.
- **Estuary and mangrove margin:** Orange and purple mud crabs, blackspot snapper, picnic seabream, Asian hard clam, razor clams, and selected prawns. Brackish tolerance and tide phase determine availability.
- **Seagrass and shallow reef:** Fusiliers, rabbitfish, goatfish, reef sweetlips, dog conch, spider conch, sea cucumbers, and sea urchins. Grazer harvest should reduce local grazing pressure and can affect algae / habitat quality.
- **Exposed basalt reef and outer reef slope:** Groupers, lunar-tail grouper, dogtooth tuna, cornetfish, and spiny lobsters. Shelter and surge conditions determine animal activity.
- **Soft-bottom shelf:** Flathead, tonguesole, breams, squid, cuttlefish, scallops, and prawns. Fishing is by selective bottom gear; no unrestricted seabed trawling.
- **Offshore current fronts:** Fusiliers, jacks, dogtooth tuna, pelagic schools, and migratory predators. Surface signs and bird activity may reveal feeding aggregations.
- **Upper deep slope:** Ruby snappers, longtail red snapper, jobfishes, deeper groupers and deep breams. These require a deep-drop line, suitable vessel capability, and a mapped slope station.
- **Abyssal / hadal floor:** Not part of the ordinary commercial fishery. Deep-sea inhabitants can be encountered, photographed, or sampled by story or research missions; do not turn the trench floor into a normal fishing pond.

### 15.2 Gear progression for the expanded catalog

| Gear unlock | Catch group | Main constraint |
|---|---|---|
| Basic rod and bait | Estuary fish, small coastal fish | Correct depth and bait |
| Light spinning rod | Needlefish, cornetfish, trevallies, reef edge fish | Casting lane and lure behavior |
| Bottom rig | Threadfin bream, emperors, snappers, flatfish | Seabed type, sinker weight, snag risk |
| Squid / cuttlefish jig | Squid and cuttlefish | Light, tide, depth, and seasonal activity |
| Crab / prawn pot | Crab, lobster, shrimp, selected octopus | Bait, soak duration, escape risk, bycatch |
| Selective net | Anchovies, sardines, scads, fusiliers, mullet | School size, mesh selectivity, legal local quota |
| Deep-drop handline | Ruby snapper, Etelis snappers, jobfishes, deep breams | 50–400 m upper slope, line capacity, current, long retrieval |
| Managed shellfish harvest kit | Clams, mussels, oysters, selected gastropods | Bed provenance, water quality, legal size and season |
| Seaweed cultivation gear | Caulerpa, Eucheuma, Kappaphycus, Gracilaria | Farm plot, crop cycle, disease and water quality |

### 15.3 Species-specific risk and retention

- Large groupers and reef predators should have size distributions and maturity thresholds. A large adult should not be the most common catch.
- Reef herbivores such as rabbitfish and parrotfish influence the ecosystem. Heavy fishing must reduce grazing activity or shift the local balance.
- Sea cucumber collection is a controlled endgame / specialist activity with permits, closed windows, size checks, slow recovery, and processing. It must not be an easy high-yield exploit.
- Abalone, spider conch and other slow-growing reef mollusks are rare, bed-limited, or farmed. Their local stocks recover slowly.
- Jellyfish harvesting is an event activity during an identified edible-species bloom, not a generic net catch.
- Freshwater Greyhaven fish are available only upstream of the brackish transition. The saltwater estuary may contain euryhaline species but not every freshwater species.
- Species with ciguatera or other food-safety concerns should have region-specific warnings or retention restrictions if that system is implemented. Never make a blanket claim that all reef fish of a given family are safe.

### 15.4 Additions to the no-sale / release-only record

Keep the following outside the ordinary catch-and-sell list:

- Sailfish, marlins, and spearfishes: sport / research encounter with tag-and-release rules unless a specifically reviewed fictional fishery is designed.
- Whale sharks, manta and devil rays, sawfish, turtles, dugongs, dolphins, and whales: observe, rescue, document, or release; never sell.
- Ornamental or ecologically important reef fish such as butterflyfish, angelfish, Moorish idols, and most damselfish: not ordinary food catches.
- Pufferfish, porcupinefish, boxfish, and other potentially toxic species: non-sale hazardous bycatch / release; do not create casual edible products.
- Giant clams and corals: survey and observe only.
- Smooth top shell (*Trochus niloticus*) and green snail (*Turbo marmoratus*): keep excluded from sale pending current legal review. Philippine conservation rules have specifically listed these taxa among threatened mollusks.

These exclusions are a conservative game-design rule. Actual local legal status should be rechecked against the law and regulations in force at the time of release.

## 16. Expanded research references

- SPC, [Deep-bottom fishing techniques for the Pacific Islands](https://coastfish.spc.int/Fishing/Deep_E/Deep_E.pdf) — deep-bottom target species, including Etelis snappers and Pristipomoides jobfishes, and fishing techniques.
- PLOS ONE, [Species distribution models of tropical deep-sea snappers](https://doi.org/10.1371/journal.pone.0127395) — Western Central Pacific deep-water snapper and jobfish depth distributions.
- Philippine Journal of Fisheries, [Blue Swimming Crab Stock Assessment in the Western Visayan Sea](https://www.nfrdi.da.gov.ph/tpjf/vol25/Blue%20Swimming%20Crab%20Stock%20Assessment%20in%20the%20Western%20Visayan%20Sea.pdf) — crab fishery, pot catch, and bycatch taxa.
- BFAR, [Updated list of registered live food-fish exporters](https://www.bfar.da.gov.ph/wp-content/uploads/2024/11/103124_Updated-List-of-Registered-Live-Food-Fish-Exporters.pdf) — export species including orange mud crab, purple mud crab, crucifix crab, banded mantis shrimp, and other market taxa.
- SEAFDEC/AQD, [Farming Abalone](https://www.seafdec.org.ph/technologies/abalone/) — Philippine *Haliotis asinina* culture and market context.
- BFAR Administrative Circular 248 (2013), [Sea cucumber size regulation and trade](https://repository.unesco.gov.ph/items/6e11ea3b-fef0-4bd6-8513-1551dd6d2aa8) — permit, transport and dried-size restrictions in the cited issuance.
- Philippine Journal of Fisheries, [Harmful Jellyfishes are Manageable](https://www.nfrdi.da.gov.ph/tpjf/vol29_2/Harmful%20Jellyfishes%20are%20Manageable.pdf) — jellyfish safety and *Rhopilema hispidum* context.
- BFAR-CALABARZON, [Seaweeds](https://bfar4a.da.gov.ph/seaweeds/) — *Caulerpa lentillifera*, *Eucheuma*, and *Gracilaria* cultivation context.
- NAST Philippines, [Philippine seaweeds and fishery by-products](https://www.nast.dost.gov.ph/index.php/13-news-press-releases/971-philippine-seaweeds-and-fishery-by-products-are-raw-materials-for-locally-made-bioplastics) — Philippine seaweed species and farmed groups.
- BFAR Administrative Order / Fisheries Administrative Order 208 s. 2001, [Conservation of rare, threatened and endangered fishery species](https://elibrary.judiciary.gov.ph/thebookshelf/showdocs/10/44018) — includes protected mollusk taxa such as *Trochus niloticus* and *Turbo marmoratus*.

