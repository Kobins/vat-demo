using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PerformanceTester : MonoBehaviour
{
    public GameObject target;
    public int count = 500;

    private int width;
    [ContextMenu("Start")]
    private void Start()
    {
        width = (int)Mathf.Sqrt(count);
        float halfWidth = width * 0.5f;
        for (int i = 0; i < count; i++)
        {
            var position = new Vector3(i % width - halfWidth, 0, i / width - halfWidth);
            var obj = Instantiate(target, position, Quaternion.identity, transform);
            obj.name = $"{target.name}_instance_{i:000}";

            var vat = obj.GetComponentInChildren<VATController>();
            if (vat)
            {
                vat.AnimationIndex = Random.Range(0, vat.Data.clips.Count);
            }
        }
    }

    [ContextMenu("Clear")]
    private void Clear()
    {
        var selfTransform = transform;
        foreach (var t in GetComponentsInChildren<Transform>())
        {
            if(t == selfTransform) continue;
            Destroy(t.gameObject);
        }
    }
    
    
    public Transform rotateTransform;
    public float rotateSpeed = 360f;
    private void Update()
    {
        if(!rotateTransform) return;
        rotateTransform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}