# Getting Started

> The toolkit is currently pre-alpha.

## Local package installation

1. Clone or download this repository.
2. In Unity, open **Window > Package Manager**.
3. Choose **+ > Add package from disk...**
4. Select this repository's `package.json`.
5. Add an empty GameObject to a scene.
6. Add the `TerrainPath` component.
7. Move its control-point handles in the Scene view.

## Current functionality

The initial code provides editable path control points, polyline sampling, terrain projection utilities, and a basic strip-mesh builder.

The mesh-generation workflow is intentionally not yet wired to a destructive one-click regeneration button. Safe output tracking and baseline adoption are part of the next milestone.

## Design promise

If the toolkit detects that a generated output has been manually modified, future regeneration workflows should warn instead of silently replacing that work.
