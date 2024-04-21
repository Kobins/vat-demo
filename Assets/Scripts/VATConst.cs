
using UnityEngine;

public static class VATConst
{

    public static readonly int VatTexture = Shader.PropertyToID("_VAT_Vertices");
    public static readonly int VatBoundsMin = Shader.PropertyToID("_VAT_Bounds_Min");
    public static readonly int VatBoundsMax = Shader.PropertyToID("_VAT_Bounds_Max");
    public static readonly int VatFrameIndex = Shader.PropertyToID("_VAT_Frame_Index");
    public static readonly int VatPreviousFrameIndex = Shader.PropertyToID("_VAT_Prev_Frame_Index");
    public static readonly int VatBlendFactor = Shader.PropertyToID("_VAT_Blend_Factor");
}