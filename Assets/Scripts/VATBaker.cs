using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class VATBaker : MonoBehaviour
{
    [Header("Objects")]
    // 목표 오브젝트
    public Animator animator;

    // 목표 Skinned Mesh Renderer
    public new SkinnedMeshRenderer renderer;

    [Header("Texture Settings")] 
    public GraphicsFormat format = GraphicsFormat.R8G8B8_UNorm;
    public TextureCreationFlags flags = TextureCreationFlags.None;

    [Header("Bake Settings")] 
    public Shader targetVATShader;

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
        var identifier = originalMesh.name;
        var vertices = originalMesh.vertices;
        Debug.Log($"Animator Object: {obj.name}", obj);
        Debug.Log($"Renderer Object: {renderer.name}", renderer);
        Debug.Log($"Target Mesh: {originalMesh} ({vertices.Length} vertices)", originalMesh);

        var controller = animator.runtimeAnimatorController;
        var clips = controller.animationClips;
        Debug.Log(
            $"exporting {clips.Length} clips from {animator.name}: [{string.Join(", ", clips.Select(it => it.name))}]");

        var data = ScriptableObject.CreateInstance<VATData>();
        data.bounds = originalMesh.bounds;

        var directory = Application.dataPath + $"/Vertex Animation Textures/{identifier}/";
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        if (!Directory.Exists(directory+"Clips/")) Directory.CreateDirectory(directory+"Clips/");
        var assetDirectory = $"Assets/Vertex Animation Textures/{identifier}/";

        AssetDatabase.CreateAsset(data, $"{assetDirectory}{originalMesh.name}_vatdata.asset");
        
        var relativePath = renderer.name;
        {
            Transform parent = renderer.transform.parent;
            Transform target = obj.transform;
            while (parent != target)
            {
                relativePath = $"{parent.name}.{relativePath}";
                parent = parent.parent;
            }
        }
        Debug.Log($"Relative path: {relativePath}");
        int height = 0;
        float time = 0f;
        var overrideList = new List<KeyValuePair<AnimationClip, AnimationClip>>(clips.Length);
        for (var index = 0; index < clips.Length; index++)
        {
            var clip = clips[index];
            CalculateBounds(clip, ref data.bounds);
            var frames = (int) (clip.length * clip.frameRate);
            var clipData = new VATClipData
            {
                index = index,
                name = clip.name,
                frameRate = clip.frameRate,
                start = time,
                end = time + clip.length,
            };

            var vatClip = new AnimationClip();
            var curve = AnimationCurve.Linear(0f, clipData.StartFrame, clip.length, clipData.EndFrame);
            vatClip.SetCurve(relativePath, typeof(VATController), $"frames{index:00}", curve);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            AnimationUtility.SetAnimationClipSettings(vatClip, settings);
            AssetDatabase.CreateAsset(vatClip, $"{assetDirectory}Clips/{clipData.name}_vat.anim");
            clipData.clipAsset = vatClip;

            data.clips.Add(clipData);
            time += clip.length;
            height += frames;
            
            overrideList.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, vatClip));
        }

        var vatOverrideController = new AnimatorOverrideController(controller);
        vatOverrideController.ApplyOverrides(overrideList);
        AssetDatabase.CreateAsset(vatOverrideController, $"{assetDirectory}{originalMesh.name}_override_controller.overrideController");
        

        // 텍스처 생성
        var vatTexture = new Texture2D(vertices.Length, height, format, flags);
        Debug.Log($"Texture Size: {vatTexture.width} x {vatTexture.height}");
        
        var bounds = data.bounds;
        var mesh = new Mesh();
        int y = 0;
        for (var i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            var frameRate = clip.frameRate;
            var invFrameRate = 1f / frameRate;
            var frames = (int)(clip.length * frameRate);
            Debug.Log("================================");
            Debug.Log($"Animation Clip: {clip.name} ({clip.length}s, {clip.frameRate} FPS)", clip);
            Debug.Log($"Bounds: {bounds.Colored(Color.yellow)}");
            for (int frame = 0; frame < frames; frame++)
            {
                float t = frame * invFrameRate;
                clip.SampleAnimation(obj, t); // 오브젝트에 특정 시간대 애니메이션 적용
                renderer.BakeMesh(mesh); // mesh에 현재 메시 상태 저장 
                var tempVertices = mesh.vertices;
                for (int x = 0; x < mesh.vertexCount; x++)
                {
                    // [0, 1]로 정규화
                    var rawPosition = tempVertices[x];
                    var normalizedPosition = bounds.Normalize(rawPosition);
                    var color = EncodePositionToRGB(normalizedPosition);
                    vatTexture.SetPixel(x, y, color);
                }
                ++y;
            }
        }

        // 텍스처 파일 저장
        vatTexture.Apply();
        var bytes = vatTexture.EncodeToPNG();
        var fileName = $"{identifier}_texture.png";
        var path = directory + fileName;
        File.WriteAllBytes(directory + fileName, bytes);
        AssetDatabase.Refresh();

        var importer = (TextureImporter)AssetImporter.GetAtPath($"{assetDirectory}{fileName}");
        Debug.Log($"Exported to {path}", importer);
        importer.npotScale = TextureImporterNPOTScale.None; // 안하면 개박살남
        importer.mipmapEnabled = false; // 굳이 할 이유가 없음
        importer.sRGBTexture = false;
        importer.filterMode = FilterMode.Point;
        var platformTextureSettings = importer.GetDefaultPlatformTextureSettings();
        platformTextureSettings.format = TextureImporterFormat.RGBA32;
        importer.SetPlatformTextureSettings(platformTextureSettings);
        importer.SaveAndReimport();

        // 오브젝트 복제
        var vatObj = Instantiate(obj);
        vatObj.name = obj.name + "_VAT";
        var vatAnimator = vatObj.GetComponentInChildren<Animator>();
        vatAnimator.runtimeAnimatorController = vatOverrideController;
        var vatRendererTransform = vatObj.transform.Find(renderer.name); // SMR 이름과 같은 자식 찾기
        // Material에서 메인 텍스처만 따오고 SMR 제거
        var vatRendererSkinned = vatRendererTransform.GetComponent<SkinnedMeshRenderer>();
        var textures = vatRendererSkinned.sharedMaterials.Select(it => it.mainTexture);
        DestroyImmediate(vatRendererSkinned);
        // Static Mesh & Renderer 추가
        var vatRendererMeshFilter = vatRendererTransform.AddComponent<MeshFilter>();
        vatRendererMeshFilter.sharedMesh = originalMesh;
        var vatRendererStatic = vatRendererTransform.AddComponent<MeshRenderer>();
        var vatTextureAsset = AssetDatabase.LoadAssetAtPath<Texture>($"{assetDirectory}{fileName}");
        vatRendererStatic.SetSharedMaterials(textures.Select(originalTexture =>
        {
            var mat = new Material(targetVATShader);
            mat.mainTexture = originalTexture;
            mat.SetTexture(VATConst.VatTexture, vatTextureAsset);
            mat.SetVector(VATConst.VatBoundsMin, bounds.min);
            mat.SetVector(VATConst.VatBoundsMax, bounds.max);
            AssetDatabase.CreateAsset(mat, $"{assetDirectory}{identifier}_{originalTexture.name}.mat");
            return mat;
        }).ToList());

        var vatController = vatRendererTransform.AddComponent<VATController>();
        vatController.data = data;
        vatController.AnimationIndex = 0;
        vatController.PrevAnimationIndex = 0;
        vatController.blendFactor = 1f;
        vatController.animator = vatAnimator;

    }

    private void CalculateBounds(AnimationClip clip, ref Bounds bounds)
    {
        var obj = animator.gameObject;
        var timeUnit = 1f / clip.frameRate;
        var mesh = new Mesh();
        for (float t = 0; t < clip.length; t += timeUnit)
        {
            clip.SampleAnimation(obj, t); // 오브젝트에 특정 시간대 애니메이션 적용
            renderer.BakeMesh(mesh); // mesh에 현재 메시 상태 저장 
            bounds.Encapsulate(mesh.bounds);
        }
    }

    private void Export(AnimationClip clip, in Bounds bounds)
    {
    }

    // private static int count = 0;
    private static Color EncodePositionToRGB(in Vector3 positionOS)
    {
        const uint bit11 = 1 << 11;
        const uint bit10 = 1 << 10;
        uint rawX = (uint)(positionOS.x * bit11) << (11 + 10);
        uint rawY = (uint)(positionOS.y * bit10) << (11 +  0);
        uint rawZ = (uint)(positionOS.z * bit11) << ( 0 +  0);
        uint raw = (uint)(rawX + rawY + rawZ);
        // if (count % 100 == 0)
        // {
            // Debug.Log($"{positionOS} => {ToBinary(raw)} ({rawX}, {rawY}, {rawZ}) / ({rawX >> 21}/2048, {rawY >> 11}/1024, {rawZ}/2048) => {raw}");
            
        // }
        const uint maskR = 0xFF000000;
        const uint maskG = 0x00FF0000;
        const uint maskB = 0x0000FF00;
        const uint maskA = 0x000000FF;

        Color color = new Color32(
            (byte)((raw & maskR) >> 24),
            (byte)((raw & maskG) >> 16),
            (byte)((raw & maskB) >> 8),
            (byte)(raw & maskA)
        );
        // if (count % 100 == 0)
        // {
        //     var rawColorPosition = color;
        //     uint rawColor =
        //         ((uint)(rawColorPosition.r * 255) << 24)
        //         +((uint)(rawColorPosition.g * 255) << 16)
        //         +((uint)(rawColorPosition.b * 255) <<  8)
        //         +((uint)(rawColorPosition.a * 255) <<  0);
        //     Color colorPosition = new Color
        //     {
        //         r = ((rawColor & 0xFFE00000) >> 21) / 2048.0f, // 0b11111111111000000000000000000000
        //         g = ((rawColor & 0x001FF800) >> 11) / 1024.0f, // 0b00000000000111111111100000000000
        //         b = ((rawColor & 0x000007FF) >>  0) / 2048.0f, // 0b00000000000000000000011111111111
        //         a = 1f,
        //     };
        //     Vector3 position = new Vector3Int(
        //         (int) (colorPosition.r * 2048), 
        //         (int) (colorPosition.g * 1024), 
        //         (int) (colorPosition.b * 2048)
        //     );
        //     // Debug.Log($"{positionOS} => color: {color}, rawColor: {ToBinary(rawColor)}, colorPosition: {colorPosition}, position: {position}");
        // }

        // ++count;
        return color;

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