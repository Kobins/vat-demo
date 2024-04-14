using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoundData")]
public class VATData : ScriptableObject
{
    public Bounds bounds;
    public List<VATClipData> clips = new();


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
public class VATClipData
{
    public string name;
    public float start;
    public float end;
    public float frameRate;

    public float StartFrame => start * frameRate;
    public float EndFrame => end * frameRate;
    public float FrameCount => (end - start) * frameRate;
}