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

    [Header("Texture Settings")] public GraphicsFormat format = GraphicsFormat.R8G8B8_UNorm;
    public TextureCreationFlags flags = TextureCreationFlags.None;

    private void OnValidate()
    {
        if (!SystemInfo.IsFormatSupported(format, FormatUsage.SetPixels))
        {
            Debug.LogError(
                $"{format} is not supported on this platform, suggests: {SystemInfo.GetCompatibleFormat(format, FormatUsage.SetPixels)}");
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
        Debug.Log(
            $"exporting {clips.Length} clips from {animator.name}: [{string.Join(", ", clips.Select(it => it.name))}]");

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

        var importer = (TextureImporter)AssetImporter.GetAtPath($"Assets/Vertex Animation Textures/{fileName}");
        Debug.Log($"Exported to {path}", importer);
        importer.npotScale = TextureImporterNPOTScale.None; // 안하면 개박살남
        importer.mipmapEnabled = false; // 굳이 할 이유가 없음
        importer.sRGBTexture = false;
    }

    private static int count = 0;
    private static Color EncodePositionToRGB(in Vector3 positionOS)
    {
        const uint bit11 = 1 << 11;
        const uint bit10 = 1 << 10;
        uint rawX = (uint)(positionOS.x * bit11) << (11 + 10);
        uint rawY = (uint)(positionOS.y * bit10) << (11 +  0);
        uint rawZ = (uint)(positionOS.z * bit11) << ( 0 +  0);
        uint raw = (uint)(rawX + rawY + rawZ);
        if (count++ % 100 == 0)
        {
            Debug.Log($"{positionOS} => {ToBinary(raw)} ({rawX}, {rawY}, {rawZ}) / ({rawX >> 21}/2048, {rawY >> 11}/1024, {rawZ}/2048) => {raw}");
        }
        const uint maskR = 0xFF000000;
        const uint maskG = 0x00FF0000;
        const uint maskB = 0x0000FF00;
        const uint maskA = 0x000000FF;

        return new Color32(
            (byte)((raw & maskR) >> 24),
            (byte)((raw & maskG) >> 16),
            (byte)((raw & maskB) >> 8),
            (byte)(raw & maskA)
        );

        static string ToBinary(uint int32)
        {
            var sb = new StringBuilder(32);
            for (int i = 31; i >= 0; --i)
            {
                sb.Append((int32 & (1 << i)) > 0 ? 1 : 0);
            }

            return sb.ToString();
        }
    }
}