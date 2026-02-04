#if UNITY_EDITOR && UNITY_WEBGL
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderStripper : IPreprocessShaders
{
    private const string AssetPath = "Assets/AstralElites/Gameplay/Effects/All Shaders.shadervariants";
    private const string InternalGuiShader = "Hidden/Internal-GUITexture";

    private ShaderVariantCollection _collection;

    public int callbackOrder => 0;

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (shader.name == InternalGuiShader)
        {
            return;
        }

        if (_collection == null)
        {
            _collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(AssetPath);

            if (_collection == null)
            {
                Debug.LogError($"[ShaderStripper] Failed to find ShaderVariantCollection at {AssetPath}. Build may fail or be incorrect.");
                return;
            }
        }

        for (int i = data.Count - 1; i >= 0; i--)
        {
            var variant = new ShaderVariantCollection.ShaderVariant
            {
                shader = shader,
                passType = snippet.passType,
                keywords = GetKeywordArray(data[i].shaderKeywordSet)
            };

            if (!_collection.Contains(variant))
            {
                data.RemoveAt(i);
            }
        }
    }

    private string[] GetKeywordArray(ShaderKeywordSet keywordSet)
    {
        var keywords = keywordSet.GetShaderKeywords();
        string[] keywordNames = new string[keywords.Length];

        for (int i = 0; i < keywords.Length; i++)
        {
            keywordNames[i] = keywords[i].name;
        }

        return keywordNames;
    }
}
#endif
