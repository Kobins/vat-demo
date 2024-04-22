using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class VATController : MonoBehaviour
{

    public Animator animator;
    public VATData data;
    
    private new MeshRenderer renderer;

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

    [SerializeField] private int prevAnimationIndex = -1;
    public int PrevAnimationIndex
    {
        get => prevAnimationIndex;
        set => prevAnimationIndex = value;
    }

    private void ResetFrame()
    {
        if(useAnimator) return;
        frameIndex = Clip?.start * Clip?.frameRate ?? 0;
    }

    public VATClipData Clip => !data || data.clips.Count <= 0 ? null : data.clips[Mathf.Clamp(AnimationIndex, 0, data.clips.Count)];
    public VATClipData PrevClip => !data || data.clips.Count <= 0 || PrevAnimationIndex == -1 ? null : data.clips[Mathf.Clamp(PrevAnimationIndex, 0, data.clips.Count)];
    public float frameIndex = 0;
    public float prevFrameIndex = 0;
    [Range(0f, 1f)]
    public float blendFactor = 0f;

    public bool useAnimator = true;

    public const int FrameCount = 32;
    #region DIRTY PART
    
    // https://forum.unity.com/threads/is-it-possible-to-animate-variables-in-an-array-in-the-animation-window.518707/
    // no unity support on animating array variable
    
    #region Defenition

    public float frames00;
    public float frames01;
    public float frames02;
    public float frames03;
    public float frames04;
    public float frames05;
    public float frames06;
    public float frames07;
    public float frames08;
    public float frames09;
    public float frames10;
    public float frames11;
    public float frames12;
    public float frames13;
    public float frames14;
    public float frames15;
    public float frames16;
    public float frames17;
    public float frames18;
    public float frames19;
    public float frames20;
    public float frames21;
    public float frames22;
    public float frames23;
    public float frames24;
    public float frames25;
    public float frames26;
    public float frames27;
    public float frames28;
    public float frames29;
    public float frames30;
    public float frames31;

    #endregion
    #region GetFrame(int index) Function

    public float GetFrame(int index)
    {
        switch (index)
        {
            case  0: return frames00;
            case  1: return frames01;
            case  2: return frames02;
            case  3: return frames03;
            case  4: return frames04;
            case  5: return frames05;
            case  6: return frames06;
            case  7: return frames07;
            case  8: return frames08;
            case  9: return frames09;
            case 10: return frames10;
            case 11: return frames11;
            case 12: return frames12;
            case 13: return frames13;
            case 14: return frames14;
            case 15: return frames15;
            case 16: return frames16;
            case 17: return frames17;
            case 18: return frames18;
            case 19: return frames19;
            case 20: return frames20;
            case 21: return frames21;
            case 22: return frames22;
            case 23: return frames23;
            case 24: return frames24;
            case 25: return frames25;
            case 26: return frames26;
            case 27: return frames27;
            case 28: return frames28;
            case 29: return frames29;
            case 30: return frames30;
            case 31: return frames31;
        }

        return -1;
    }
    
    #endregion
    #region SetFrame(int index, float value) Function
    public void SetFrame(int index, float value)
    {
        switch (index)
        {
            case  0: frames00 = value; return;
            case  1: frames01 = value; return;
            case  2: frames02 = value; return;
            case  3: frames03 = value; return;
            case  4: frames04 = value; return;
            case  5: frames05 = value; return;
            case  6: frames06 = value; return;
            case  7: frames07 = value; return;
            case  8: frames08 = value; return;
            case  9: frames09 = value; return;
            case 10: frames10 = value; return;
            case 11: frames11 = value; return;
            case 12: frames12 = value; return;
            case 13: frames13 = value; return;
            case 14: frames14 = value; return;
            case 15: frames15 = value; return;
            case 16: frames16 = value; return;
            case 17: frames17 = value; return;
            case 18: frames18 = value; return;
            case 19: frames19 = value; return;
            case 20: frames20 = value; return;
            case 21: frames21 = value; return;
            case 22: frames22 = value; return;
            case 23: frames23 = value; return;
            case 24: frames24 = value; return;
            case 25: frames25 = value; return;
            case 26: frames26 = value; return;
            case 27: frames27 = value; return;
            case 28: frames28 = value; return;
            case 29: frames29 = value; return;
            case 30: frames30 = value; return;
            case 31: frames31 = value; return;
        }
    }
    #endregion

    #endregion

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
            PrevAnimationIndex = -1;
            return;
        }

        if (AnimationIndex < 0)
        {
            AnimationIndex = 0;
        }else if (AnimationIndex >= data.clips.Count)
        {
            AnimationIndex = data.clips.Count - 1;
        }

        if (PrevAnimationIndex < -1)
        {
            PrevAnimationIndex = -1;
        }else if (PrevAnimationIndex >= data.clips.Count)
        {
            PrevAnimationIndex = data.clips.Count - 1;
        }
    }

    [ContextMenu("Initialize Bounds")]
    public void InitializeBounds()
    {
        if (!renderer) renderer = GetComponent<MeshRenderer>();
        var bounds = Bounds;
        foreach (var mat in renderer.sharedMaterials)
        {
            mat.SetVector(VATConst.VatBoundsMin, bounds.min);
            mat.SetVector(VATConst.VatBoundsMax, bounds.max);
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
        var previousClip = PrevClip;
        if (currentClip != null)
        {
            // 애니메이터를 사용할 경우 애니메이터 내에서 설정된 Frame을 기반으로 설정
            if (useAnimator)
            {
                UpdateAnimator(0);
                
                frameIndex = GetFrame(AnimationIndex);
                if(PrevAnimationIndex >= 0)
                    prevFrameIndex = GetFrame(PrevAnimationIndex);
                // Debug.Log($"frameIndex={frameIndex}, prevFrameIndex={prevFrameIndex}");
            }
            // 애니메이터를 사용하지 않으면 설정된 값에 따라 Repeat 플레이
            else
            {
                frameIndex += Time.deltaTime * currentClip.frameRate;
                while (frameIndex >= currentClip.EndFrame)
                {
                    frameIndex -= currentClip.FrameCount;
                }
                if (previousClip != null)
                {
                    prevFrameIndex += Time.deltaTime * previousClip.frameRate;
                    while (prevFrameIndex >= previousClip.EndFrame)
                    {
                        prevFrameIndex -= previousClip.FrameCount;
                    }
                }
            }
            
            var props = new MaterialPropertyBlock();
            props.SetFloat(VATConst.VatFrameIndex, Mathf.Floor(frameIndex));
            props.SetFloat(VATConst.VatPreviousFrameIndex, Mathf.Floor(prevFrameIndex));
            props.SetFloat(VATConst.VatBlendFactor, PrevAnimationIndex == -1 ? 1f : blendFactor);
            renderer.SetPropertyBlock(props);
        }
    }

    private static AnimatorClipInfo MaxWeightClip(AnimatorClipInfo[] clipInfos, out float weightSum)
    {
        ref AnimatorClipInfo maxWeight = ref clipInfos[0];
        weightSum = maxWeight.weight;
        for (var index = 1; index < clipInfos.Length; index++)
        {
            var clipInfo = clipInfos[index];
            weightSum += clipInfo.weight;
            if (clipInfo.weight > maxWeight.weight)
            {
                maxWeight = clipInfo;
            }
        }

        return maxWeight;
    }

    private int GetAnimationIndexByClip(AnimationClip clip)
    {
        return data.ClipByName.TryGetValue(clip.name, out var clipData) ? clipData.index : -1;
    }
        private static string ToString(AnimatorClipInfo[] clipInfo) =>
            string.Join(", ", clipInfo.Select(it => $"{it.clip.name}={(int)(it.weight*100)}%"));
    private void UpdateAnimator(int layerIndex)
    {
        var vat = this;
        var currentClipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex);
        var nextClipInfo = animator.GetNextAnimatorClipInfo(layerIndex);

        // Debug.Log($"[{layerIndex}] UPDATE - normalized: {stateInfo.normalizedTime}, current[{currentClipInfo.Length}]: {ToString(currentClipInfo)} /// next[{nextClipInfo.Length}]: {ToString(nextClipInfo)}");
        // Debug.Log($"[{layerIndex}] UPDATE NT={currentStateInfo.normalizedTime} - current[{currentClipInfo.Length}]: {ToString(currentClipInfo)} /// next[{nextClipInfo.Length}]: {ToString(nextClipInfo)}");

        // 현재 실행중인 애니메이션 클립이 없음: 아무것도 안 함
        if (currentClipInfo.Length <= 0)
        {
            return;
        }

        float blendFactor;
        // 다음 애니메이션이 있음: 애니메이션 블렌딩 필요
        if (nextClipInfo.Length >= 1)
        {
            // 최대 weight 애니메이션만 사용
            var prev = MaxWeightClip(currentClipInfo, out _);
            var current = MaxWeightClip(nextClipInfo, out blendFactor);
            
            vat.PrevAnimationIndex = GetAnimationIndexByClip(prev.clip);
            vat.AnimationIndex = GetAnimationIndexByClip(current.clip);
            vat.blendFactor = blendFactor;
            // Debug.Log($"[{layerIndex}][VAT] next >=1, prev={vat.PrevAnimationIndex}, current={vat.AnimationIndex}, blendFactor={vat.blendFactor}");
            return;
        }
        
        // 현재 애니메이션만 있는데 안에서 blend되고 있을 때 ...
        if (currentClipInfo.Length == 2)
        {
            var prev = currentClipInfo[0];
            var current = currentClipInfo[1];
            
            vat.PrevAnimationIndex = GetAnimationIndexByClip(prev.clip);
            vat.AnimationIndex = GetAnimationIndexByClip(current.clip);
            vat.blendFactor = current.weight;
            // Debug.Log($"[{layerIndex}][VAT] current==2, prev({prev.clip.name})={vat.PrevAnimationIndex}, current({current.clip.name})={vat.AnimationIndex}, blendFactor={vat.blendFactor}");
            return;
        }

        if (currentClipInfo.Length == 1)
        {
            var current = currentClipInfo[0];
            vat.PrevAnimationIndex = -1;
            vat.AnimationIndex = GetAnimationIndexByClip(current.clip);
            vat.blendFactor = 1f;
            // Debug.Log($"[{layerIndex}][VAT] current==1, prev={vat.PrevAnimationIndex}, current={vat.AnimationIndex}, blendFactor={vat.blendFactor}");
            return;
        }
        

        Debug.LogWarning($"VATStateMachineBehaviour layer {layerIndex} has more than three current clips !!!");
    }
}