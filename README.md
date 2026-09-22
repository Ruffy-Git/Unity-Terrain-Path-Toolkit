# Unity Terrain Path Toolkit

An open-source Unity editor toolkit for building **editable spline-based roads, sidewalks, driveways, garden paths, and other terrain-conforming surfaces**.

The project focuses on a common environment-authoring problem: generated geometry is useful until an artist manually fixes it and regeneration destroys the work. Unity Terrain Path Toolkit is being designed around **non-destructive authoring**, editable source data, and explicit adoption of manual mesh edits.

> **Project status:** early development / pre-alpha. The public API and file formats may change.

## Goals

- Author paths from editable spline/control-point data.
- Conform generated paths to uneven Unity Terrain.
- Generate reusable path meshes in the editor.
- Keep source data editable after generation.
- Detect when generated meshes have been manually changed.
- Allow artists to adopt intentional mesh edits as a new baseline instead of silently overwriting them.
- Support branching path networks for driveways, sidewalks, trails, and landscaped environments.
- Keep the core package independent from any specific game project.

## Why this project exists

Environment tools often make procedural generation easy but iteration difficult. A generated road may need a hand-adjusted corner, a path may need to split around landscaping, or terrain may change after the first pass. This toolkit explores a workflow where procedural generation and manual art direction can coexist.

## Planned workflow

1. Create a Path Authoring component.
2. Add and move control points in the Scene view.
3. Preview a terrain-conforming centerline.
4. Generate/update the path mesh.
5. Continue editing the source path at any time.
6. If the output mesh is edited externally, detect the change.
7. Explicitly adopt that mesh as the manual baseline or regenerate from source.

## Installation

The package is not yet published as a stable release. During development, clone this repository and add the package locally through Unity Package Manager.

When the first usable package release is available, Git URL installation instructions will be added here.

## Repository layout

```text
Runtime/          Runtime components and path data
Editor/           Unity editor authoring and generation tools
Tests/            EditMode tests
Documentation~/   Design and usage documentation
Samples~/         Sample content (planned)
package.json      Unity Package Manager manifest
```

## Roadmap

See [ROADMAP.md](ROADMAP.md).

## Contributing

Issues, bug reports, documentation improvements, test cases, and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

MIT. See [LICENSE](LICENSE).

## Disclaimer

This is an independent open-source Unity tool and is not affiliated with or endorsed by Unity Technologies.
