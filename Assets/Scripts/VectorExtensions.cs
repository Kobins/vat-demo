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
}