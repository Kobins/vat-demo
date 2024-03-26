using UnityEngine;

public class DebugUtils
{
    public static void DrawWireMesh(Mesh mesh, Matrix4x4 transform, Color color, float duration = 0f)
    {
        var tris = mesh.triangles;
        var trisCount = tris.Length / 3;
        for (int t = 0; t < trisCount; t++)
        {
            var v0 = mesh.vertices[t * 3 + 0];
            var v1 = mesh.vertices[t * 3 + 1];
            var v2 = mesh.vertices[t * 3 + 2];
            var v0WS = transform * v0;
            var v1WS = transform * v1;
            var v2WS = transform * v2;
            Debug.DrawLine(v0WS, v1WS, color, duration);
            Debug.DrawLine(v1WS, v2WS, color, duration);
            Debug.DrawLine(v2WS, v0WS, color, duration);
        }
    }
}