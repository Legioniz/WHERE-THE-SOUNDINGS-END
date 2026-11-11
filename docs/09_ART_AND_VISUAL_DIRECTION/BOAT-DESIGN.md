# Boat Design

> Project: WHERE THE SOUNDINGS END
> Status: Procedural visual model implemented; Unity scene review pending
> Owner: Project lead
> Version: 1.0
> Last reviewed: 2026-11-12

## Design intent

The starter vessel is a compact, aging coastal workboat used for fishing, salvage and hydrographic sounding. It should feel practical and slightly overworked: broad enough to look stable, narrow enough to feel vulnerable in open water, and visually distinct from a generic speedboat.

The model uses a restrained maritime palette and readable 2.5D silhouette. It is deliberately not a luxury yacht, military patrol craft or modern sport-fishing boat.

## Silhouette and proportions

- Overall visual length: approximately 5.55 m in local game units.
- Maximum beam: approximately 2.08 m.
- Bow: narrow, lifted and tapered, with a visible foredeck and anchor roller.
- Stern: squared transom with an exposed outboard motor and open working deck.
- Hull: deep V/chine-inspired lower body, fuller amidships, narrowing into the bow.
- Wheelhouse: compact, pale weathered cabin around the forward-middle section, with dark glass and a low overhanging roof.
- Working deck: open aft area with two small tubs/crates, a simple winch, rope and low protective rails.
- Fenders, rub rails, cleats, antenna and navigation lights provide scale and believable working detail without cluttering the silhouette.

The visual model is oriented with local +Z forward, +X starboard and -X port. Do not rotate the visual child independently of the physics root.

## Material language

| Part | Material direction |
|---|---|
| Main hull | Deep, desaturated teal-charcoal paint |
| Deck | Salt-worn brown timber |
| Wheelhouse | Faded pale sage/grey paint |
| Roof and engine | Charcoal and oxidized dark metal |
| Windows | Deep teal tinted glass, not bright blue |
| Rub rails and fittings | Dull oxidized metal |
| Safety details | Small faded ivory and signal-red marks |
| Rope and fenders | Natural tan rope and near-black rubber |
| Lamps | Warm amber, used sparingly |

The overall model should remain legible in dawn fog, overcast daylight and deep-ocean low light. Avoid high-gloss surfaces, saturated trim, chrome, excessive decals and decorative light strips.

## Geometry and implementation

Assets/_Project/Scripts/Editor/BoatVisualModelBuilder.cs generates reusable hull, deck and wheelhouse meshes in Assets/_Project/Art/Meshes/. Materials are maintained in Assets/_Project/Art/Materials/.

The same visual builder is used by:
- GreyhavenEnvironmentBuilder for the player workboat;
- DeepOceanSurveySandboxBuilder for the Outer Shoals and Hollow Trench survey craft.

The builder creates a visual-only child hierarchy named Boat_Visual_Model. Generated primitive details have their colliders removed. Rebuilding the generated scenes refreshes the shared meshes and materials without introducing physics objects into the visual tree.

## Physics boundary

The visual model must never become the physics authority.

- The Rigidbody remains on Player_Boat_Blockout / Player_Boat_Survey_Prototype.
- The existing root BoxCollider remains the simplified waterline collision proxy.
- BoatHydrodynamics continues to sample its four configured buoyancy offsets.
- BoatPrototypeController continues to apply movement through Rigidbody forces and torque.
- Cabin, rails, deck fittings, crates, rope, fenders and outboard visuals must not add colliders.
- Do not change mass, center of mass, target draft, buoyancy sample positions or collision response solely to accommodate decorative geometry.

The rendered hull is intentionally more tapered than its simple BoxCollider proxy. It is a first art pass, not a replacement for a future authored compound collision hull.

## Review and acceptance

Before calling the boat production-ready, review the generated scenes in Unity:

1. Inspect the model from bow, stern, port, starboard and the current 2.5D gameplay camera.
2. Confirm the bow reads as pointed and lifted, the stern has a clear work deck, and the wheelhouse does not obscure the vessel silhouette.
3. Confirm no visual part clips through the sea at the normal draft or during representative wave heave/pitch/roll.
4. Confirm the engine, propeller, fenders and deck fittings remain visually attached during Rigidbody motion.
5. Confirm no child visual object has a Collider or Rigidbody.
6. Confirm the single root collision proxy still owns pier, terrain and wreck contacts.
7. Review the boat in Greyhaven, Outer Shoals and Hollow Trench under each scene's lighting and fog.
8. Tune geometry and materials from actual Unity screenshots rather than adding unsupported micro-detail.

The current Edit Mode geometry checks and repository syntax validation do not replace the in-Editor visual and Play Mode review.
