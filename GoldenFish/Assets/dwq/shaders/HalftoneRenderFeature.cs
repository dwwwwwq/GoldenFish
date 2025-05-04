using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HalftoneRenderFeature : ScriptableRendererFeature
{
    class HalftoneRenderPass : ScriptableRenderPass
    {
        private Material halftoneMaterial;
        private RenderTargetIdentifier source;
        private RenderTargetHandle temporaryTexture;

        public HalftoneRenderPass(Material material)
        {
            this.halftoneMaterial = material;
            this.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            temporaryTexture.Init("_TemporaryColorTexture");
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Halftone Effect");

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(temporaryTexture.id, opaqueDesc, FilterMode.Bilinear);
            Blit(cmd, source, temporaryTexture.Identifier(), halftoneMaterial, 0);
            Blit(cmd, temporaryTexture.Identifier(), source, halftoneMaterial, 1);
            cmd.ReleaseTemporaryRT(temporaryTexture.id);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public Material halftoneMaterial;
    private HalftoneRenderPass halftonePass;

    public override void Create()
    {
        if (halftoneMaterial == null)
        {
            Debug.LogError("Halftone Material is not assigned.");
            return;
        }
        halftonePass = new HalftoneRenderPass(halftoneMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        halftonePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(halftonePass);
    }
}
