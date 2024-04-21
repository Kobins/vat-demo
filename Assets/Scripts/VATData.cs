using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoundData")]
public class VATData : ScriptableObject
{
    public Bounds bounds;
    public List<VATClipData> clips = new();

    private void OnValidate()
    {
        clipByName = null;
        for (var index = 0; index < clips.Count; index++)
        {
            var clip = clips[index];
            clip.index = index;
        }
    }
    private Dictionary<string, VATClipData> clipByName = null;
    public Dictionary<string, VATClipData> ClipByName
    {
        get
        {
            if (clipByName == null)
            {
                clipByName = new Dictionary<string, VATClipData>(clips.Count);
                foreach (var clip in clips)
                {
                    var clipName = clip.name+"_vat";
                    if (!clipByName.TryAdd(clipName, clip))
                    {
                        Debug.LogWarning($"{clipName} duplicate !!!", this);   
                    }

                    Debug.Log($"bind {clipName} into clip {clip}", clip.clipAsset);
                }
            }
            return clipByName;
        }
    }
}

[Serializable]
public class VATClipData
{
    public int index;
    public string name;
    public float start;
    public float end;
    public float frameRate;
    public AnimationClip clipAsset;

    public float StartFrame => start * frameRate;
    public float EndFrame => end * frameRate;
    public float FrameCount => (end - start) * frameRate;
}