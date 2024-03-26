using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class BoundPropertySetter : MonoBehaviour
{
    private new MeshRenderer renderer;
    
    private static readonly int VatBoundsMin = Shader.PropertyToID("_VAT_Bounds_Min");
    private static readonly int VatBoundsMax = Shader.PropertyToID("_VAT_Bounds_Max");

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
    }
    
    private void Update()
    {
        if(!renderer) return;
        foreach (var mat in renderer.materials)
        {
            var bounds = renderer.bounds;
            mat.SetVector(VatBoundsMin, bounds.min);
            mat.SetVector(VatBoundsMax, bounds.max);
        }
    }
}