# Unity Terrain Path Toolkit

An open-source Unity editor toolkit for building **editable roads, sidewalks, driveways, garden paths, trails, and other terrain-conforming surfaces**.

The toolkit is built around non-destructive authoring: procedural regeneration should never silently destroy intentional artist edits.

> **Status: v0.1 pre-release candidate.** Core authoring is implemented, but it still needs validation inside supported Unity Editor versions before a stable release.

## Current features

- Editable Scene-view path control points
- Adjustable width and sample spacing
- Optional projection onto a Unity Terrain
- Configurable vertical offset
- Basic strip-mesh generation with UVs and normals
- Optional path material
- Generated-mesh fingerprinting
- Detection of manual/external mesh changes
- Safe regeneration blocking when changes are detected
- **Adopt Current Mesh as Baseline** workflow
- Explicit **Replace Manual Edits and Regenerate** action
- Unity Undo support for core authoring operations
- EditMode tests for mesh generation and change detection

## Why this exists

Procedural environment tools are fast until an artist needs to fix the result manually. A corner may need reshaping, a driveway may need adjustment, or terrain may change after generation. Many regeneration workflows treat the generated mesh as disposable and overwrite those changes.

Terrain Path Toolkit separates **editable source data** from **generated output** and records a fingerprint of generated geometry. If that geometry changes outside the authoring workflow, normal regeneration stops and asks the user to choose what should happen.

## Install from Git

In Unity, open **Window > Package Manager**, choose **+ > Add package from git URL...**, and enter:

`https://github.com/Ruffy-Git/Unity-Terrain-Path-Toolkit.git`

You can also clone the repository and use **Add package from disk...** with `package.json`.

## Quick start

1. Create an empty GameObject.
2. Add the **Terrain Path** component.
3. Assign a Terrain if the path should follow terrain.
4. Set width, sample spacing, vertical offset, and optionally a material.
5. Move the path handles in Scene view and use **Add Point** as needed.
6. Select **Generate / Regenerate Path**.
7. If you intentionally edit the generated mesh, select **Adopt Current Mesh as Baseline** before later regeneration.
8. To intentionally discard manual edits, use **Replace Manual Edits and Regenerate**.

## Safety behavior

When the generated mesh no longer matches its recorded fingerprint, ordinary regeneration is blocked. The tool does not silently overwrite the detected change.

## Documentation

- [Getting Started](Documentation~/getting-started.md)
- [Architecture](Documentation~/architecture.md)
- [Roadmap](ROADMAP.md)
- [Contributing](CONTRIBUTING.md)

## Compatibility

The package manifest targets Unity 2022.3 or newer. Editor validation across Unity versions is still in progress.

## License

MIT. See [LICENSE](LICENSE).

## Disclaimer

This is an independent open-source project and is not affiliated with or endorsed by Unity Technologies.
