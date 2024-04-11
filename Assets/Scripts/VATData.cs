using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoundData")]
public class VATData : ScriptableObject
{
    public Bounds bounds;
    public List<VATAnimationClipData> clips = new();


    private Dictionary<string, VATAnimationClipData> clipByName = null;
    public Dictionary<string, VATAnimationClipData> ClipByName
    {
        get
        {
            if (clipByName == null)
            {
                clipByName = new Dictionary<string, VATAnimationClipData>(clips.Count);
                foreach (var clip in clips)
                {
                    if (!ClipByName.TryAdd(clip.name, clip))
                    {
                        Debug.LogWarning($"{clip.name} duplicate !!!", this);   
                    }
                }
            }
            return clipByName;
        }
    }
}

[Serializable]
public class VATAnimationClipData
{
    public string name;
    public float start;
    public float end;
    public float frameRate;
}