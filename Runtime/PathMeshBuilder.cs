using System.Collections.Generic;
using UnityEngine;

namespace TerrainPathToolkit
{
    public static class PathMeshBuilder
    {
        public static Mesh BuildStrip(IReadOnlyList<Vector3> worldPoints, float width, Transform outputSpace)
            => BuildStrip(worldPoints, width, outputSpace, null, 0f, 1);

        public static Mesh BuildStrip(IReadOnlyList<Vector3> worldPoints, float width, Transform outputSpace,
            Terrain terrain, float verticalOffset, int widthSubdivisions)
        {
            var mesh = new Mesh { name = "Terrain Path Mesh" };
            if (worldPoints == null || worldPoints.Count < 2 || outputSpace == null) return mesh;

            widthSubdivisions = Mathf.Clamp(widthSubdivisions, 1, 32);
            var columns = widthSubdivisions + 1;
            var vertices = new Vector3[worldPoints.Count * columns];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[(worldPoints.Count - 1) * widthSubdivisions * 6];
            var distance = 0f;
            var lastSide = Vector3.right;

            for (var i = 0; i < worldPoints.Count; i++)
            {
                var tangent = CalculateTangent(worldPoints, i);
                tangent.y = 0f;
                if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.forward;
                tangent.Normalize();

                var side = Vector3.Cross(Vector3.up, tangent);
                if (side.sqrMagnitude < 0.0001f) side = lastSide;
                else side.Normalize();
                lastSide = side;

                if (i > 0) distance += Vector3.Distance(worldPoints[i - 1], worldPoints[i]);

                for (var column = 0; column < columns; column++)
                {
                    var across = column / (float)widthSubdivisions;
                    var lateral = Mathf.Lerp(-width * 0.5f, width * 0.5f, across);
                    var vertex = worldPoints[i] + side * lateral;
                    if (terrain != null) vertex = ProjectVertexToTerrain(vertex, terrain, verticalOffset);

                    var index = i * columns + column;
                    vertices[index] = outputSpace.InverseTransformPoint(vertex);
                    uvs[index] = new Vector2(across, distance);
                }

                if (i >= worldPoints.Count - 1) continue;
                for (var column = 0; column < widthSubdivisions; column++)
                {
                    var quad = (i * widthSubdivisions + column) * 6;
                    var current = i * columns + column;
                    var next = current + columns;
                    triangles[quad] = current;
                    triangles[quad + 1] = next;
                    triangles[quad + 2] = current + 1;
                    triangles[quad + 3] = current + 1;
                    triangles[quad + 4] = next;
                    triangles[quad + 5] = next + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 ProjectVertexToTerrain(Vector3 point, Terrain terrain, float verticalOffset)
        {
            var origin = terrain.transform.position;
            var data = terrain.terrainData;
            var localX = point.x - origin.x;
            var localZ = point.z - origin.z;
            if (localX < 0f || localZ < 0f || localX > data.size.x || localZ > data.size.z) return point;
            point.y = terrain.SampleHeight(point) + origin.y + verticalOffset;
            return point;
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
