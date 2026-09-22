# Getting Started

> The toolkit is currently a pre-release candidate and should be tested before production use.

## Install from Git

In Unity:
1. Open **Window > Package Manager**.
2. Choose **+ > Add package from git URL...**.
3. Enter `https://github.com/Ruffy-Git/Unity-Terrain-Path-Toolkit.git`.

For local development, clone the repository and choose **Add package from disk...**, then select `package.json`.

## Create a path

1. Add an empty GameObject to the scene.
2. Add the `TerrainPath` component.
3. Assign a Unity Terrain if desired.
4. Configure width, sample spacing, vertical offset, and material.
5. Move control-point handles in Scene view.
6. Use **Add Point** to extend the path.
7. Click **Generate / Regenerate Path**.

## Manual-edit protection

Every generated mesh receives a geometry fingerprint stored by the authoring component.

If the mesh later differs from that fingerprint, normal regeneration is blocked. You then have two intentional choices:

- **Adopt Current Mesh as Baseline** — keep the current mesh and mark it as the accepted state.
- **Replace Manual Edits and Regenerate** — discard the changed output and rebuild it from the editable path source.

This prevents regeneration from silently overwriting detected manual geometry edits.

## Terrain behavior

When **Conform To Terrain** is enabled and a Terrain is assigned, sampled centerline points use Unity's terrain height. **Vertical Offset** can lift the generated surface slightly to reduce z-fighting.

## Current limitations

The current path is polyline-based rather than a full Bezier spline. Junctions, per-point widths, terrain painting, and embankments are future work.
