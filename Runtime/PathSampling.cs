using System.Collections.Generic;
using UnityEngine;

namespace TerrainPathToolkit
{
    public static class PathSampling
    {
        public static List<Vector3> Sample(TerrainPath path)
        {
            return path != null && path.SmoothPath ? SampleCatmullRom(path) : SamplePolyline(path);
        }

        public static List<Vector3> SamplePolyline(TerrainPath path)
        {
            var result = new List<Vector3>();
            if (path == null || path.ControlPoints.Count < 2) return result;
            var spacing = Mathf.Max(0.05f, path.SampleSpacing);
            for (var segment = 0; segment < path.ControlPoints.Count - 1; segment++)
            {
                var a = path.GetWorldPoint(segment);
                var b = path.GetWorldPoint(segment + 1);
                var steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(a, b) / spacing));
                for (var i = 0; i < steps; i++)
                {
                    if (segment > 0 && i == 0) continue;
                    result.Add(Vector3.Lerp(a, b, i / (float)steps));
                }
            }
            result.Add(path.GetWorldPoint(path.ControlPoints.Count - 1));
            return result;
        }

        public static List<Vector3> SampleCatmullRom(TerrainPath path)
        {
            var result = new List<Vector3>();
            if (path == null || path.ControlPoints.Count < 2) return result;
            var spacing = Mathf.Max(0.05f, path.SampleSpacing);

            for (var segment = 0; segment < path.ControlPoints.Count - 1; segment++)
            {
                var p0 = path.GetWorldPoint(Mathf.Max(0, segment - 1));
                var p1 = path.GetWorldPoint(segment);
                var p2 = path.GetWorldPoint(segment + 1);
                var p3 = path.GetWorldPoint(Mathf.Min(path.ControlPoints.Count - 1, segment + 2));
                var estimate = Vector3.Distance(p1, p2);
                var steps = Mathf.Max(4, Mathf.CeilToInt(estimate / spacing));

                for (var i = 0; i < steps; i++)
                {
                    if (segment > 0 && i == 0) continue;
                    result.Add(CatmullRom(p0, p1, p2, p3, i / (float)steps));
                }
            }

            result.Add(path.GetWorldPoint(path.ControlPoints.Count - 1));
            return result;
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return 0.5f * ((2f * p1) + (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        public static void ProjectToTerrain(List<Vector3> points, Terrain terrain, float verticalOffset = 0.02f)
        {
            if (points == null || terrain == null) return;
            var origin = terrain.transform.position;
            var data = terrain.terrainData;
            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];
                var x = p.x - origin.x;
                var z = p.z - origin.z;
                if (x < 0f || z < 0f || x > data.size.x || z > data.size.z) continue;
                p.y = terrain.SampleHeight(p) + origin.y + verticalOffset;
                points[i] = p;
            }
        }
    }
}
