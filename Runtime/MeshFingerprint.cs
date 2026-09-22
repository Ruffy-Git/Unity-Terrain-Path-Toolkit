using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace TerrainPathToolkit
{
    public static class MeshFingerprint
    {
        public static string Calculate(Mesh mesh)
        {
            if (mesh == null) return string.Empty;
            var sb = new StringBuilder();
            foreach (var v in mesh.vertices)
                sb.Append(v.x.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                  .Append(v.y.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                  .Append(v.z.ToString("R", CultureInfo.InvariantCulture)).Append(';');
            foreach (var i in mesh.triangles) sb.Append(i).Append(',');
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            var result = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) result.Append(b.ToString("x2"));
            return result.ToString();
        }
    }
}
