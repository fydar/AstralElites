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

                // foreach (var texture in postProcessingData.textures.blueNoise16LTex)
                // {
                //     ClearOutTexture(texture);
                // }
                // foreach (var texture in postProcessingData.textures.filmGrainTex)
                // {
                //     ClearOutTexture(texture);
                // }
                // ClearOutTexture(postProcessingData.textures.smaaAreaTex);
                // ClearOutTexture(postProcessingData.textures.smaaSearchTex);

                postProcessingData.textures.blueNoise16LTex = Array.Empty<Texture2D>();
                postProcessingData.textures.filmGrainTex = Array.Empty<Texture2D>();
                postProcessingData.textures.smaaSearchTex = null;
                postProcessingData.textures.smaaAreaTex = null;

                EditorUtility.SetDirty(postProcessingData);
            }
        }

        // foreach (string textureGuid in AssetDatabase.FindAssets("t:Texture"))
        // {
        //     string assetPath = AssetDatabase.GUIDToAssetPath(textureGuid);
        //     if (assetPath.Contains("/Runtime/Debugging/"))
        //     {
        //         var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        //         foreach (var asset in assets)
        //         {
        //             asset.hideFlags |= HideFlags.DontSaveInBuild;
        //         }
        //     }
        // }

        foreach (var texture in Resources.FindObjectsOfTypeAll(typeof(Texture)))
        {
            if (texture.name == "Splash Screen Unity Logo")
            {
                texture.hideFlags |= HideFlags.DontSaveInBuild;
                ClearOutTexture(texture);
            }
            // if (texture.name == "FalloffLookupTexture")
            // {
            //     texture.hideFlags |= HideFlags.DontSaveInBuild;
            //     ClearOutTexture(texture);
            // }
            // if (texture.name == "NumbersDisplayTex")
            // {
            //     texture.hideFlags |= HideFlags.DontSaveInBuild;
            //     ClearOutTexture(texture);
            // }
        }

        static void ClearOutTexture(UnityEngine.Object texture)
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
}
#endif
