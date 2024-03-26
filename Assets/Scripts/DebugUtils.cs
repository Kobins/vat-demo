using UnityEngine;

public static class DebugUtils
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
        
    /// <summary>
    /// Debug.Log 출력 시 색상 입히는 함수입니다.
    /// </summary>
    /// <param name="raw">ToString 호출이 가능한 개체입니다.</param>
    /// <param name="color">대상 색상입니다.</param>
    /// <returns>raw를 ToString한 뒤 color 태그를 입힌 문자열을 반환합니다.</returns>
    public static string Colored(this object raw, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{raw}</color>";
}