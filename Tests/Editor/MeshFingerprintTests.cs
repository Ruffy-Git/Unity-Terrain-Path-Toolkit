using NUnit.Framework;
using UnityEngine;

namespace TerrainPathToolkit.Tests
{
    public class MeshFingerprintTests
    {
        [Test]
        public void Calculate_SameGeometry_ReturnsSameFingerprint()
        {
            var a = CreateTriangle(); var b = CreateTriangle();
            Assert.AreEqual(MeshFingerprint.Calculate(a), MeshFingerprint.Calculate(b));
            Object.DestroyImmediate(a); Object.DestroyImmediate(b);
        }

        [Test]
        public void Calculate_VertexChanged_ReturnsDifferentFingerprint()
        {
            var mesh = CreateTriangle();
            var before = MeshFingerprint.Calculate(mesh);
            var vertices = mesh.vertices;
            vertices[0] = new Vector3(0.25f, 0f, 0f);
            mesh.vertices = vertices;
            Assert.AreNotEqual(before, MeshFingerprint.Calculate(mesh));
            Object.DestroyImmediate(mesh);
        }

        private static Mesh CreateTriangle()
        {
            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.forward };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up };
            return mesh;
        }
    }
}
