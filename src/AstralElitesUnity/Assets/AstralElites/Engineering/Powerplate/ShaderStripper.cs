#if UNITY_EDITOR && UNITY_WEBGL
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderStripper : IPreprocessShaders, IPreprocessComputeShaders
{
    private const string assetPath = "Assets/AstralElites/Gameplay/Effects/PrewarmShaders.graphicsstate";

    private GraphicsStateCollection _collection;

    public int callbackOrder => 0;

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (_collection == null)
        {
            _collection = AssetDatabase.LoadAssetAtPath<GraphicsStateCollection>(assetPath);
        }

        if (_collection == null)
        {
            return;
        }

        for (int i = data.Count - 1; i >= 0; i--)
        {
            var keywords = GetKeywordArray(shader, data[i].shaderKeywordSet);

            if (!_collection.ContainsVariant(shader, snippet.pass, keywords))
            {
                data.RemoveAt(i);
                Debug.Log($"[Stripper] Removed variant: {shader.name} - {snippet.passType} - {string.Join(", ", keywords)}");
            }
        }
    }

    public void OnProcessComputeShader(ComputeShader shader, string kernelName, IList<ShaderCompilerData> data)
    {
        shader.hideFlags |= HideFlags.DontSaveInBuild;
        data.Clear();
        Debug.Log($"[Stripper] Compute Shader {shader.name} fully stripped.");
    }

    private LocalKeyword[] GetKeywordArray(Shader shader, ShaderKeywordSet keywordSet)
    {
        var keywords = keywordSet.GetShaderKeywords();
        var keywordNames = new LocalKeyword[keywords.Length];

        for (int i = 0; i < keywords.Length; i++)
        {
            keywordNames[i] = new LocalKeyword(shader, keywords[i].name);
        }

        return keywordNames;
    }
}
#endif
