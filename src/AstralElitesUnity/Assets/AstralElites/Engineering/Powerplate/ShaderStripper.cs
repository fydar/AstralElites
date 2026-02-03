#if UNITY_EDITOR && UNITY_WEBGL
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

public class ShaderStripper : IPreprocessShaders
{
    public int callbackOrder => 0;

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (shader.name == "Hidden/Light2D")
        {
            data.Clear();
            return;
        }
    }
}
#endif
