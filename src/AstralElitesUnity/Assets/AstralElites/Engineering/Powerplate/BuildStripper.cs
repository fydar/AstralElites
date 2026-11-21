#if UNITY_EDITOR && UNITY_WEBGL
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BuildStripper : IPostprocessBuildWithReport, IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        ProcessData();
    }

    public void OnPostprocessBuild(BuildReport report)
    {
    }

    private void ProcessData()
    {
        if (UniversalRenderPipeline.asset == null)
        {
            return;
        }

        var rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(UniversalRenderPipeline.asset);

        for (int i = 0; i < rendererDataList.Length; i++)
        {
            var rendererData = (Renderer2DData)rendererDataList[i];
            if (rendererData != null)
            {
                var postProcessingData = (PostProcessData)typeof(Renderer2DData).GetField("m_PostProcessData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(rendererData);

                if (postProcessingData == null)
                {
                    return;
                }
                postProcessingData.textures.blueNoise16LTex = Array.Empty<Texture2D>();
#if !UNITY_6000_6_OR_NEWER
                postProcessingData.textures.filmGrainTex = Array.Empty<Texture2D>();
#endif
                postProcessingData.textures.smaaSearchTex = null;
                postProcessingData.textures.smaaAreaTex = null;

                EditorUtility.SetDirty(postProcessingData);
            }
        }

        StripResourceType("UnityEngine.Rendering.Universal.UniversalRenderPipelineFilmGrainResources", "m_Textures");
        StripResourceType("UnityEngine.Rendering.UnifiedRayTracing.RayTracingRenderPipelineResources",
            "m_GeometryPoolKernels", "m_CopyBuffer", "m_CopyPositions", "m_BitHistogram",
            "m_BlockReducePart", "m_BlockScan", "m_BuildHlbvh", "m_RestructureBvh", "m_Scatter");
        StripResourceType("UnityEngine.PathTracing.Core.WorldRenderPipelineResources",
            "_blitCubemap", "_blitGrayScaleCookie", "_setAlphaChannelShader",
            "_environmentImportanceSamplingBuild", "_skyBoxMesh", "_sixFaceSkyBoxMesh",
            "_buildLightGridShader", "_solidColorShader");
        StripResourceType("UnityEngine.Rendering.GPUResidentDrawerResources",
            "m_InstanceDataBufferCopyKernels", "m_InstanceDataBufferUploadKernels",
            "m_TransformUpdaterKernels", "m_WindDataUpdaterKernels",
            "m_OccluderDepthPyramidKernels", "m_InstanceOcclusionCullingKernels",
            "m_OcclusionCullingDebugKernels", "m_DebugOcclusionTestPS", "m_DebugOccluderPS");
        StripResourceType("UnityEngine.Rendering.STP+RuntimeResources",
            "m_setupCS", "m_preTaaCS", "m_taaCS");
        StripResourceType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusionBlueNoiseResources",
            "m_BlueNoise256Textures");
        StripResourceType("UnityEngine.Rendering.Universal.ScreenSpaceReflectionPersistentResources",
            "m_Shader", "m_BlitShader");
        StripResourceType("UnityEngine.Rendering.VrsRenderPipelineRuntimeResources",
            "m_TextureComputeShader", "m_VisualizationShader");
        StripResourceType("UnityEngine.Rendering.Universal.UniversalRenderPipelineRuntimeXRResources",
            "m_xrOcclusionMeshPS", "m_xrMirrorViewPS", "m_xrMotionVector");

        foreach (var texture in Resources.FindObjectsOfTypeAll(typeof(Texture)))
        {
            if (texture.name == "Splash Screen Unity Logo")
            {
                texture.hideFlags |= HideFlags.DontSaveInBuild;
                ClearOutTexture(texture);
            }
        }
    }

    private static void StripResourceType(string typeName, params string[] fieldNames)
    {
        Type resourceType = null;
        foreach (var assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies())
        {
            resourceType = assembly.GetType(typeName);
            if (resourceType != null)
            {
                break;
            }
        }

        if (resourceType == null)
        {
            Debug.LogWarning($"[BuildStripper] Type not found: {typeName}");
            return;
        }

        var instances = Resources.FindObjectsOfTypeAll(resourceType);
        if (instances.Length == 0)
        {
            return;
        }

        foreach (var instance in instances)
        {
            foreach (string fieldName in fieldNames)
            {
                var field = resourceType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null)
                {
                    Debug.LogWarning($"[BuildStripper] Field not found: {typeName}.{fieldName}");
                    continue;
                }

                if (field.FieldType.IsArray)
                {
                    field.SetValue(instance, Array.CreateInstance(field.FieldType.GetElementType(), 0));
                }
                else
                {
                    field.SetValue(instance, null);
                }
            }

            EditorUtility.SetDirty(instance);
            Debug.Log($"[BuildStripper] Stripped {typeName} from {instance.name}");
        }
    }

    private static void ClearOutTexture(UnityEngine.Object texture)
    {
        if (texture == null)
        {
            return;
        }
        try
        {
            if (texture is Texture2D texture2D)
            {
                if (texture2D.isReadable)
                {
                    _ = texture2D.Reinitialize(2, 2);
                    texture2D.SetPixels32(new Color32[]
                    {
                        Color.clear,
                        Color.clear,
                        Color.clear,
                        Color.clear
                    });
                    texture2D.Apply();
                }
            }

            EditorUtility.SetDirty(texture);
        }
        catch (Exception exception)
        {
            Debug.Log(exception);
        }
    }
}
#endif
