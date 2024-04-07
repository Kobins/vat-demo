using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 Normalize(this in Bounds bounds, in Vector3 target)
    {
        var min = bounds.min;
        var max = bounds.max;

        return new Vector3(
            InverseLerp(min.x, max.x, target.x, nearOne11bit),
            InverseLerp(min.y, max.y, target.y, nearOne10bit),
            InverseLerp(min.z, max.z, target.z, nearOne11bit)
        );
    }

    private const float nearOne10bit = 1023f / 1024f;
    private const float nearOne11bit = 2047f / 2048f;
    public static float InverseLerp(in float start, in float end, in float value, in float nearOne)
    {
        if (start == end)
        {
            return 0.0f;
        }

        float result = (value - start) / (end - start);
        if (result < 0f)
        {
            result = 0f;
        }else if (result >= 1f)
        {
            result = nearOne;
        }
        return result;
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