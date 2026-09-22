using System.Collections.Generic;
using UnityEngine;

namespace TerrainPathToolkit
{
    public static class PathSampling
    {
        public static List<Vector3> Sample(TerrainPath path)
        {
            if (path == null) return new List<Vector3>();
            var points = path.SmoothPath ? SampleCatmullRom(path) : SamplePolyline(path);
            if (path.ConformToTerrain && path.TargetTerrain != null && path.AdaptiveTerrainSampling)
                points = RefineForTerrain(points, path.TargetTerrain, path.TerrainHeightTolerance, path.MaxAdaptiveDepth);
            return points;
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


        private static List<Vector3> RefineForTerrain(List<Vector3> source, Terrain terrain, float tolerance, int maxDepth)
        {
            if (source == null || source.Count < 2 || terrain == null || maxDepth <= 0) return source;
            var result = new List<Vector3>();
            result.Add(source[0]);
            for (var i = 0; i < source.Count - 1; i++)
                RefineSegment(source[i], source[i + 1], terrain, Mathf.Max(0.01f, tolerance), maxDepth, result);
            return result;
        }

        private static void RefineSegment(Vector3 a, Vector3 b, Terrain terrain, float tolerance, int depth, List<Vector3> output)
        {
            if (depth <= 0)
            {
                output.Add(b);
                return;
            }

            var midpoint = Vector3.Lerp(a, b, 0.5f);
            var terrainA = TerrainHeight(a, terrain);
            var terrainB = TerrainHeight(b, terrain);
            var terrainMid = TerrainHeight(midpoint, terrain);
            var linearMid = (terrainA + terrainB) * 0.5f;
            var error = Mathf.Abs(terrainMid - linearMid);

            if (error <= tolerance)
            {
                output.Add(b);
                return;
            }

            RefineSegment(a, midpoint, terrain, tolerance, depth - 1, output);
            RefineSegment(midpoint, b, terrain, tolerance, depth - 1, output);
        }

        private static float TerrainHeight(Vector3 point, Terrain terrain)
        {
            var origin = terrain.transform.position;
            var data = terrain.terrainData;
            var x = point.x - origin.x;
            var z = point.z - origin.z;
            if (x < 0f || z < 0f || x > data.size.x || z > data.size.z) return point.y;
            return terrain.SampleHeight(point) + origin.y;
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
