using System.Collections.Generic;
using UnityEngine;

namespace TerrainPathToolkit
{
    public static class PathMeshBuilder
    {
        public static Mesh BuildStrip(IReadOnlyList<Vector3> worldPoints, float width, Transform outputSpace)
        {
            var mesh = new Mesh { name = "Terrain Path Mesh" };
            if (worldPoints == null || worldPoints.Count < 2 || outputSpace == null) return mesh;

            var vertices = new Vector3[worldPoints.Count * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[(worldPoints.Count - 1) * 6];
            var distance = 0f;
            var lastSide = Vector3.right;

            for (var i = 0; i < worldPoints.Count; i++)
            {
                var tangent = CalculateTangent(worldPoints, i);
                var side = Vector3.Cross(Vector3.up, tangent);
                if (side.sqrMagnitude < 0.0001f) side = lastSide;
                else side.Normalize();
                lastSide = side;

                var halfWidth = width * 0.5f;
                vertices[i * 2] = outputSpace.InverseTransformPoint(worldPoints[i] - side * halfWidth);
                vertices[i * 2 + 1] = outputSpace.InverseTransformPoint(worldPoints[i] + side * halfWidth);

                if (i > 0) distance += Vector3.Distance(worldPoints[i - 1], worldPoints[i]);
                uvs[i * 2] = new Vector2(0f, distance);
                uvs[i * 2 + 1] = new Vector2(1f, distance);

                if (i >= worldPoints.Count - 1) continue;
                var t = i * 6;
                var v = i * 2;
                triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 CalculateTangent(IReadOnlyList<Vector3> points, int index)
        {
            if (index == 0) return (points[1] - points[0]).normalized;
            if (index == points.Count - 1) return (points[index] - points[index - 1]).normalized;
            var incoming = (points[index] - points[index - 1]).normalized;
            var outgoing = (points[index + 1] - points[index]).normalized;
            var tangent = incoming + outgoing;
            return tangent.sqrMagnitude > 0.0001f ? tangent.normalized : outgoing;
        }
    }
}
