Shader "Hidden/AstralElites/Bloom"
{
    HLSLINCLUDE
    #pragma target 3.5
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D(_BlitTexture);

    // x=1/srcWidth  y=1/srcHeight  z=srcWidth  w=srcHeight
    float4 _BloomTexelSize;

    // x=threshold  y=intensity  z=scatter  w=knee
    float4 _BloomParams;
    float4 _BloomTint;

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv         : TEXCOORD0;
    };

    Varyings Vert(uint vertexID : SV_VertexID)
    {
        Varyings output;
        output.positionCS = GetFullScreenTriangleVertexPosition(vertexID);
        output.uv         = GetFullScreenTriangleTexCoord(vertexID);
        return output;
    }

    float3 QuadraticThreshold(float3 color)
    {
        float threshold  = _BloomParams.x;
        float knee       = max(_BloomParams.w, 1e-4);
        float brightness = max(color.r, max(color.g, color.b));
        float rq = clamp(brightness - threshold + knee, 0.0, 2.0 * knee);
        rq = (0.25 / knee) * rq * rq;
        return color * max(rq, brightness - threshold) / max(brightness, 1e-4);
    }

    // Pass 0 — Prefilter
    float4 FragPrefilter(Varyings input) : SV_Target
    {
        float3 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv).rgb;
        return float4(QuadraticThreshold(color), 1.0);
    }

    // Pass 1 — Downsample: 4-tap box filter
    float4 FragDownsample(Varyings input) : SV_Target
    {
        float2 d = _BloomTexelSize.xy;
        float4 s;
        s  = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2(-1.0, -1.0) * d);
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2( 1.0, -1.0) * d);
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2(-1.0,  1.0) * d);
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2( 1.0,  1.0) * d);
        return s * 0.25;
    }

    // Pass 2 — Upsample: 9-tap tent filter; scatter widens sample radius
    float4 FragUpsample(Varyings input) : SV_Target
    {
        float2 d = _BloomTexelSize.xy * (1.0 + _BloomParams.z);
        float4 s;
        s  = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2(-1.0,  1.0) * d);
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2( 0.0,  1.0) * d) * 2.0;
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2( 1.0,  1.0) * d);
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2(-1.0,  0.0) * d) * 2.0;
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2( 0.0,  0.0) * d) * 4.0;
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2( 1.0,  0.0) * d) * 2.0;
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2(-1.0, -1.0) * d);
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2( 0.0, -1.0) * d) * 2.0;
        s += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv + float2( 1.0, -1.0) * d);
        return s / 16.0;
    }

    // Pass 3 — Composite: apply tint + intensity; Blend One One adds onto screen
    float4 FragComposite(Varyings input) : SV_Target
    {
        float3 bloom = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.uv).rgb;
        return float4(bloom * _BloomTint.rgb * _BloomParams.y, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "Prefilter"
            Blend Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragPrefilter
            ENDHLSL
        }

        Pass
        {
            Name "Downsample"
            Blend Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDownsample
            ENDHLSL
        }

        Pass
        {
            Name "Upsample"
            Blend Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragUpsample
            ENDHLSL
        }

        Pass
        {
            Name "Composite"
            Blend One One
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragComposite
            ENDHLSL
        }
    }
}
