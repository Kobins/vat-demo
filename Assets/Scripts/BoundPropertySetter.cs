using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class BoundPropertySetter : MonoBehaviour
{
    private MeshFilter meshFilter;
    private new MeshRenderer renderer;
    
    private static readonly int VatBoundsMin = Shader.PropertyToID("_VAT_Bounds_Min");
    private static readonly int VatBoundsMax = Shader.PropertyToID("_VAT_Bounds_Max");
    private static readonly int VatVertices = Shader.PropertyToID("_VAT_Vertices");

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }
    
    private void Update()
    {
        if(!renderer) return;
        foreach (var mat in renderer.materials)
        {
            var bounds = renderer.localBounds;
            mat.SetVector(VatBoundsMin, bounds.min);
            mat.SetVector(VatBoundsMax, bounds.max);
        }
    }

    private List<Color> pixels = new List<Color>();
    public int y = 0;

    private void OnValidate()
    {
        CalculateVertex();
    }

    private List<Vector3> calculatedVertices = new();
    private List<Color> colors = new();

    [ContextMenu("Clear Calculated Vertices")]
    private void ClearCalculatedVertices()
    {
        calculatedVertices.Clear();
        colors.Clear();
    }
    
    [ContextMenu("Calculate Vertex")]
    private void CalculateVertex()
    {
        if (!meshFilter)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
        
        int count = meshFilter.sharedMesh.vertexCount;
        var bounds = renderer.localBounds;
        var min = bounds.min;
        var max = bounds.max;

        ClearCalculatedVertices();
        foreach (var mat in renderer.materials)
        {
            var texture = mat.GetTexture(VatVertices);
            if (texture is Texture2D t)
            {
                var pixels = t.GetPixels();
                for (int x = 0; x < count; x++)
                {
                    var color = pixels[y * count + x];
                    var v = new Vector3(
                        Mathf.Lerp(min.x, max.x, color.r),
                        Mathf.Lerp(min.y, max.y, color.g),
                        Mathf.Lerp(min.z, max.z, color.b)
                    );
                    calculatedVertices.Add(v);
                    colors.Add(color);
                }
                break;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        var localBounds = renderer.localBounds;
        var localBoundsSize = localBounds.size;
        Gizmos.DrawWireCube(localBounds.center, localBoundsSize);

        var gridColor = Color.white;
        gridColor.a = 0.1f;
        Gizmos.color = gridColor;
        var xOffset = Vector3.forward * localBoundsSize.z;
        for (int x = 0; x < 255; x++)
        {
            var offset = Vector3.right * (localBoundsSize.x * x / 255f);
            Gizmos.DrawLine(localBounds.min + offset, localBounds.min + xOffset + offset);
        }

        if (calculatedVertices.Count > 0)
        {
            var sharedMesh = meshFilter.sharedMesh;
            var triangles = sharedMesh.triangles;
            var originalVertices = sharedMesh.vertices;
            var triCount = triangles.Length / 3;
            var yellow = Color.yellow;
            yellow.a = 0.1f;
            var red = Color.red;
            red.a = 0.1f;
            for (int i = 0; i < triCount; i++)
            {
                var v0 = calculatedVertices[triangles[i * 3 + 0]];
                var v1 = calculatedVertices[triangles[i * 3 + 1]];
                var v2 = calculatedVertices[triangles[i * 3 + 2]];
                var v0Color = colors[triangles[i * 3 + 0]];
                var v1Color = colors[triangles[i * 3 + 1]];
                var v2Color = colors[triangles[i * 3 + 2]];
                var v0Origin = originalVertices[triangles[i * 3 + 0]];
                var v1Origin = originalVertices[triangles[i * 3 + 1]];
                var v2Origin = originalVertices[triangles[i * 3 + 2]];
                Gizmos.color = v0Color.MaskG();
                Gizmos.DrawLine(v0, v1);
                Gizmos.DrawLine(v0Origin, v1Origin);
                Gizmos.color = v1Color.MaskG();
                Gizmos.DrawLine(v1, v2);
                Gizmos.DrawLine(v1Origin, v2Origin);
                Gizmos.color = v2Color.MaskG();
                Gizmos.DrawLine(v2, v0);
                Gizmos.DrawLine(v2Origin, v0Origin);
                
                
                Gizmos.color = yellow;
                
                // Gizmos.color = red;
                // Gizmos.DrawLine(v0Origin, v0);
                // Gizmos.DrawLine(v1Origin, v1);
                // Gizmos.DrawLine(v2Origin, v2);
                
            }
        }
        
        
    }
}