using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class TextureExporter : MonoBehaviour
{
    [Header("Objects")]
    // 목표 오브젝트
    public Animator animator;
    // 목표 Skinned Mesh Renderer
    public new SkinnedMeshRenderer renderer;

    [Header("Texture Settings")] 
    public GraphicsFormat format = GraphicsFormat.R8G8B8_UNorm;
    public TextureCreationFlags flags = TextureCreationFlags.None;

    private void OnValidate()
    {
        if (!SystemInfo.IsFormatSupported(format, FormatUsage.SetPixels))
        {
            Debug.LogError($"{format} is not supported on this platform, suggests: {SystemInfo.GetCompatibleFormat(format, FormatUsage.SetPixels)}");
        }
    }

    [ContextMenu("Export All")]
    private void ExportAll()
    {
        var obj = animator.gameObject;
        var originalMesh = renderer.sharedMesh; // 원본 메시
        var vertices = originalMesh.vertices;
        Debug.Log($"Animator Object: {obj.name}", obj);
        Debug.Log($"Renderer Object: {renderer.name}", renderer);
        Debug.Log($"Target Mesh: {originalMesh} ({vertices.Length} vertices)", originalMesh);
        
        var clips = animator.runtimeAnimatorController.animationClips;
        Debug.Log($"exporting {clips.Length} clips from {animator.name}: [{string.Join(", ", clips.Select(it => it.name))}]");
        
        foreach (var clip in clips)
        {
            Debug.Log("================================");
            Export(clip);
        }
    }
    
    private void Export(AnimationClip clip)
    {
        var obj = animator.gameObject;
        var originalMesh = renderer.sharedMesh; // 원본 메시
        var vertices = originalMesh.vertices;
        var timeUnit = 1f / clip.frameRate;
        var frames = (int)(clip.length * clip.frameRate);
        Debug.Log($"Animation Clip: {clip.name} ({clip.length}s, {clip.frameRate} FPS)", clip);

        // 텍스처 생성
        var texture = new Texture2D(vertices.Length, frames, format, flags);
        Debug.Log($"Texture Size: {texture.width} x {texture.height}");

        var bounds = originalMesh.bounds;
        var mesh = new Mesh();
        int y = 0;
        Debug.Log($"Bounds: {bounds.Colored(Color.yellow)}");
        var sb = new StringBuilder(mesh.vertexCount);
        for (float t = 0; t < clip.length; t += timeUnit)
        {
            clip.SampleAnimation(obj, t); // 오브젝트에 특정 시간대 애니메이션 적용
            renderer.BakeMesh(mesh); // mesh에 현재 메시 상태 저장 
            var tempVertices = mesh.vertices;
            sb.Clear();
            sb.AppendLine($"at {t:F2}s ...");
            for (int x = 0; x < mesh.vertexCount; x++)
            {
                // [0, 1]로 정규화
                var rawPosition = tempVertices[x];
                var normalizedPosition = bounds.Normalize(rawPosition);
                var color = EncodePositionToRGB(normalizedPosition);
                sb.AppendLine($"[{x:0000}] {rawPosition} => {normalizedPosition} => {color.Colored(color)}");
                texture.SetPixel(x, y, color);
            }
            Debug.Log(sb.ToString());

            ++y;
        }
        
        // 텍스처 파일 저장
        texture.Apply();
        var bytes = texture.EncodeToPNG();
        var directory = Application.dataPath + "/Vertex Animation Textures/";
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        var fileName = $"{obj.name}-{clip.name}.png";
        var path = directory + fileName;
        File.WriteAllBytes(directory + fileName, bytes);
        AssetDatabase.Refresh();
        var asset = AssetDatabase.LoadAssetAtPath($"Assets/Vertex Animation Textures/{fileName}", typeof(Texture2D));
        Debug.Log($"Exported to {path}", asset);
    }
    
    private static Color EncodePositionToRGB(in Vector3 positionOS) 
        => new(positionOS.x, positionOS.y, positionOS.z);
}
