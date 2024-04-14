using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class VATController : MonoBehaviour
{

    [SerializeField] private VATData data;
    public VATData Data => data;
    
    private new MeshRenderer renderer;

    private static readonly int VatBoundsMin = Shader.PropertyToID("_VAT_Bounds_Min");
    private static readonly int VatBoundsMax = Shader.PropertyToID("_VAT_Bounds_Max");
    private static readonly int VatFrameIndex = Shader.PropertyToID("_VAT_Frame_Index");

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    private Bounds Bounds => data ? data.bounds : renderer.localBounds;

    [SerializeField] private int animationIndex = 0;
    public int AnimationIndex
    {
        get => animationIndex;
        set
        {
            if (animationIndex != value)
            {
                animationIndex = value;
                ResetFrame();
            }
        }
    }

    private void ResetFrame()
    {
        frameIndex = Clip?.start * Clip?.frameRate ?? 0;
    }

    public VATClipData Clip => !data || data.clips.Count <= 0 ? null : data.clips[AnimationIndex % data.clips.Count];
    public float frameIndex = 0;

    private void OnEnable()
    {
        Debug.Log($"OnEnable {name}");
        InitializeBounds();
    }

    private void OnValidate()
    {
        InitializeBounds();
        if (!data || data.clips.Count <= 0)
        {
            AnimationIndex = -1;
            return;
        }

        if (AnimationIndex < 0)
        {
            AnimationIndex = 0;
        }else if (AnimationIndex >= data.clips.Count)
        {
            AnimationIndex = data.clips.Count - 1;
        }
    }

    [ContextMenu("Initialize Bounds")]
    public void InitializeBounds()
    {
        if (!renderer) renderer = GetComponent<MeshRenderer>();
        var bounds = Bounds;
        foreach (var mat in renderer.sharedMaterials)
        {
            mat.SetVector(VatBoundsMin, bounds.min);
            mat.SetVector(VatBoundsMax, bounds.max);
        }
    }

    private void Update()
    {
        if (!renderer)
        {
            renderer = GetComponent<MeshRenderer>();
            return;
        }

        var currentClip = Clip;
        if (currentClip != null)
        {
            frameIndex += Time.deltaTime * currentClip.frameRate;
            while (frameIndex >= currentClip.EndFrame)
            {
                frameIndex -= currentClip.FrameCount;
            }
            
            var props = new MaterialPropertyBlock();
            props.SetFloat(VatFrameIndex, Mathf.Floor(frameIndex));
            renderer.SetPropertyBlock(props);
        } 
    }
}