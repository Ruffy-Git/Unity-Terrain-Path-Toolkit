using System.Collections.Generic;
using UnityEngine;

namespace TerrainPathToolkit
{
    public static class PathSampling
    {
        public static List<Vector3> SamplePolyline(TerrainPath path)
        {
            var result = new List<Vector3>();
            if (path == null || path.ControlPoints.Count < 2) return result;

            var spacing = Mathf.Max(0.05f, path.SampleSpacing);

            for (var segment = 0; segment < path.ControlPoints.Count - 1; segment++)
            {
                var a = path.GetWorldPoint(segment);
                var b = path.GetWorldPoint(segment + 1);
                var length = Vector3.Distance(a, b);
                var steps = Mathf.Max(1, Mathf.CeilToInt(length / spacing));

                for (var i = 0; i < steps; i++)
                {
                    if (segment > 0 && i == 0) continue;
                    result.Add(Vector3.Lerp(a, b, i / (float)steps));
                }
            }

            result.Add(path.GetWorldPoint(path.ControlPoints.Count - 1));
            return result;
        }

        public static void ProjectToTerrain(List<Vector3> points, Terrain terrain, float verticalOffset = 0.02f)
        {
            if (points == null || terrain == null) return;

            var origin = terrain.transform.position;
            var data = terrain.terrainData;

            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];
                var localX = p.x - origin.x;
                var localZ = p.z - origin.z;

                if (localX < 0f || localZ < 0f || localX > data.size.x || localZ > data.size.z)
                    continue;

                p.y = terrain.SampleHeight(p) + origin.y + verticalOffset;
                points[i] = p;
            }
        }
    }
}
