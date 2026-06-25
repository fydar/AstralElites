using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class BloomRenderFeature : ScriptableRendererFeature
{
    public static BloomRenderFeature Instance { get; private set; }

    [Serializable]
    public class Settings
    {
        public Shader bloomShader;
        [Range(0f, 1f)] public float threshold = 0.9f;
        [Min(0f)] public float intensity = 3f;
        [Range(0f, 1f)] public float scatter = 0.7f;
        public Color tint = Color.red;
        [Range(2, 8)] public int maxIterations = 4;
    }

    public Settings settings = new();

    [NonSerialized] public float IntensityMultiplier;

    private BloomPass _pass;

    private const float kSleepThreshold = 0.001f;

    public override void Create()
    {
        Instance = this;

        var shader = settings.bloomShader != null
            ? settings.bloomShader
            : Shader.Find("Hidden/AstralElites/Bloom");

        if (shader == null)
        {
            Debug.LogWarning("[BloomRenderFeature] Bloom shader not found. Assign it in the Renderer Feature settings.");
            return;
        }

        _pass = new BloomPass(settings, shader)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
            requiresIntermediateTexture = true,
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_pass == null || IntensityMultiplier <= kSleepThreshold) return;

        _pass.IntensityMultiplier = IntensityMultiplier;
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Cleanup();
        _pass = null;
    }

    private class BloomPass : ScriptableRenderPass
    {
        private const int kMaxIterations  = 8;
        private const int kPrefilterPass  = 0;
        private const int kDownsamplePass = 1;
        private const int kUpsamplePass   = 2;
        private const int kCompositePass  = 3;

        private static readonly int s_BloomTexelSizeID = Shader.PropertyToID("_BloomTexelSize");
        private static readonly int s_BloomParamsID    = Shader.PropertyToID("_BloomParams");
        private static readonly int s_BloomTintID      = Shader.PropertyToID("_BloomTint");

        private readonly Settings _settings;
        private readonly Material _material;

        public float IntensityMultiplier;

        public BloomPass(Settings settings, Shader shader)
        {
            _settings = settings;
            _material = CoreUtils.CreateEngineMaterial(shader);
        }

        public void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        private class PassData
        {
            public Material material;
            public TextureHandle cameraColorCopy;
            public TextureHandle cameraColor;
            public TextureHandle[] mips;
            public int iterations;
            public Vector4 bloomParams;
            public Vector4 bloomTint;
            public Vector4[] texelSizes;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_material == null) return;

            var resources  = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            var cameraColor = resources.activeColorTexture;
            if (!cameraColor.IsValid()) return;

            int screenW    = cameraData.cameraTargetDescriptor.width;
            int screenH    = cameraData.cameraTargetDescriptor.height;
            int iterations = Mathf.Clamp(_settings.maxIterations, 2, kMaxIterations);

            float knee      = _settings.threshold * 0.1f;
            var bloomParams = new Vector4(_settings.threshold, _settings.intensity * IntensityMultiplier, _settings.scatter, knee);
            var bloomTint   = (Vector4)_settings.tint.linear;

            // Mip chain dimensions
            int[] widths  = new int[iterations];
            int[] heights = new int[iterations];
            widths[0]  = Mathf.Max(1, screenW >> 1);
            heights[0] = Mathf.Max(1, screenH >> 1);
            for (int i = 1; i < iterations; i++)
            {
                widths[i]  = Mathf.Max(1, widths[i  - 1] >> 1);
                heights[i] = Mathf.Max(1, heights[i - 1] >> 1);
            }

            var copyDesc = new TextureDesc(screenW, screenH)
            {
                name        = "BloomSourceCopy",
                colorFormat = cameraData.cameraTargetDescriptor.graphicsFormat,
                filterMode  = FilterMode.Bilinear,
                wrapMode    = TextureWrapMode.Clamp,
                msaaSamples = MSAASamples.None,
                clearBuffer = false,
            };
            var cameraColorCopy = renderGraph.CreateTexture(copyDesc);
            renderGraph.AddCopyPass(cameraColor, cameraColorCopy, passName: "Bloom Copy Source");

            var mipFormat = GraphicsFormat.R8G8B8A8_UNorm;

            // Allocate mip chain textures
            var mips = new TextureHandle[iterations];
            for (int i = 0; i < iterations; i++)
            {
                mips[i] = renderGraph.CreateTexture(new TextureDesc(widths[i], heights[i])
                {
                    name        = "BloomMip" + i,
                    colorFormat = mipFormat,
                    filterMode  = FilterMode.Bilinear,
                    wrapMode    = TextureWrapMode.Clamp,
                    clearBuffer = false,
                });
            }

            // Texel sizes per mip level (x=1/w, y=1/h, z=w, w=h)
            var texelSizes = new Vector4[iterations];
            for (int i = 0; i < iterations; i++)
                texelSizes[i] = new Vector4(1f / widths[i], 1f / heights[i], widths[i], heights[i]);

            using var builder = renderGraph.AddUnsafePass<PassData>("Bloom", out var data);

            data.material        = _material;
            data.cameraColorCopy = cameraColorCopy;
            data.cameraColor     = cameraColor;
            data.mips            = mips;
            data.iterations      = iterations;
            data.bloomParams     = bloomParams;
            data.bloomTint       = bloomTint;
            data.texelSizes      = texelSizes;

            builder.UseTexture(cameraColorCopy, AccessFlags.Read);
            builder.UseTexture(cameraColor, AccessFlags.Write);
            for (int i = 0; i < iterations; i++)
                builder.UseTexture(mips[i], AccessFlags.ReadWrite);

            builder.AllowPassCulling(false);

            builder.SetRenderFunc(static (PassData d, UnsafeGraphContext ctx) =>
            {
                CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);

                cmd.SetGlobalVector(s_BloomParamsID, d.bloomParams);
                cmd.SetGlobalVector(s_BloomTintID, d.bloomTint);

                // Prefilter: cameraColorCopy → mips[0]
                cmd.SetGlobalVector(s_BloomTexelSizeID, d.texelSizes[0]);
                ctx.cmd.SetRenderTarget(d.mips[0]);
                Blitter.BlitTexture(cmd, (RenderTargetIdentifier)d.cameraColorCopy, new Vector4(1, 1, 0, 0), d.material, kPrefilterPass);

                // Downsample: mips[i-1] → mips[i]
                for (int i = 1; i < d.iterations; i++)
                {
                    cmd.SetGlobalVector(s_BloomTexelSizeID, d.texelSizes[i - 1]);
                    ctx.cmd.SetRenderTarget(d.mips[i]);
                    Blitter.BlitTexture(cmd, (RenderTargetIdentifier)d.mips[i - 1], new Vector4(1, 1, 0, 0), d.material, kDownsamplePass);
                }

                // Upsample: mips[i] → mips[i-1]
                for (int i = d.iterations - 1; i > 0; i--)
                {
                    cmd.SetGlobalVector(s_BloomTexelSizeID, d.texelSizes[i]);
                    ctx.cmd.SetRenderTarget(d.mips[i - 1]);
                    Blitter.BlitTexture(cmd, (RenderTargetIdentifier)d.mips[i], new Vector4(1, 1, 0, 0), d.material, kUpsamplePass);
                }

                // Composite: additive blend (Blend One One in shader) onto camera color
                cmd.SetGlobalVector(s_BloomTexelSizeID, d.texelSizes[0]);
                ctx.cmd.SetRenderTarget(d.cameraColor, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
                Blitter.BlitTexture(cmd, (RenderTargetIdentifier)d.mips[0], new Vector4(1, 1, 0, 0), d.material, kCompositePass);
            });
        }
    }
}
