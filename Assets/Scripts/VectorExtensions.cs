using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 Normalize(this in Bounds bounds, in Vector3 target)
    {
        var min = bounds.min;
        var max = bounds.max;

        return new Vector3(
            Mathf.InverseLerp(min.x, max.x, target.x),
            Mathf.InverseLerp(min.y, max.y, target.y),
            Mathf.InverseLerp(min.z, max.z, target.z)
        );
    }

    public static Color MaskR(this Color c)
    {
        c.g = 0f;
        c.b = 0f;
        return c;
    }
    public static Color MaskG(this Color c)
    {
        c.r = 0f;
        c.b = 0f;
        return c;
    }
    public static Color MaskB(this Color c)
    {
        c.r = 0f;
        c.g = 0f;
        return c;
    }
}