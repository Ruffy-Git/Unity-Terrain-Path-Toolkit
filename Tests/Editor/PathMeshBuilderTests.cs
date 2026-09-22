using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TerrainPathToolkit.Tests
{
    public class PathMeshBuilderTests
    {
        [Test]
        public void BuildStrip_TwoPoints_CreatesQuad()
        {
            var go = new GameObject("Path Test");
            try
            {
                var mesh = PathMeshBuilder.BuildStrip(new List<Vector3> { Vector3.zero, Vector3.forward * 5f }, 2f, go.transform);
                Assert.AreEqual(4, mesh.vertexCount);
                Assert.AreEqual(6, mesh.triangles.Length);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Fingerprint_ChangesWhenVertexChanges()
        {
            var mesh = new Mesh { vertices = new[] { Vector3.zero, Vector3.right, Vector3.forward }, triangles = new[] { 0, 1, 2 } };
            var before = MeshFingerprint.Calculate(mesh);
            var vertices = mesh.vertices;
            vertices[0] = Vector3.up;
            mesh.vertices = vertices;
            var after = MeshFingerprint.Calculate(mesh);
            Assert.AreNotEqual(before, after);
        }
    }
}
