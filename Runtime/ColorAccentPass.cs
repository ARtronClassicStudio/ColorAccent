using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorAccentPass : ScriptableRenderPass
{
    RenderTargetIdentifier source;
    RenderTargetIdentifier destinationA;
    RenderTargetIdentifier destinationB;
    RenderTargetIdentifier latestDest;

    readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
    readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");
    private const string kShaderName = "Hidden/ColorAccent";
    private Material m_Material;
    public ColorAccentPass() => renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume New Post Process Volume is unable to load.");

        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        var renderer = renderingData.cameraData.renderer;
        source = renderer.cameraColorTarget;
        cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
        destinationA = new RenderTargetIdentifier(temporaryRTIdA);
        cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
        destinationB = new RenderTargetIdentifier(temporaryRTIdB);

    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        CommandBuffer cmd = CommandBufferPool.Get("ColorAccentRenderer");
        cmd.Clear();

        var stack = VolumeManager.instance.stack;

        void BlitTo(Material mat, int pass = 0)
        {
            var first = latestDest;
            var last = first == destinationA ? destinationB : destinationA;
            Blit(cmd, first, last, mat, pass);

            latestDest = last;
        }

        latestDest = source;
        var fx = stack.GetComponent<ColorAccent>();


        if (fx.IsActive())
        {

            m_Material.SetColor(Shader.PropertyToID("_AccentColor"), fx.accentColor.value);
            m_Material.SetColor(Shader.PropertyToID("_LogicColor"), fx.logicColor.value);
            m_Material.SetFloat(Shader.PropertyToID("_Threshold"), fx.threshold.value);
            m_Material.SetFloat(Shader.PropertyToID("_Contrast"), fx.contrast.value);
            m_Material.SetFloat(Shader.PropertyToID("_Lerp"), fx.lerp.value);
            m_Material.SetFloat(Shader.PropertyToID("_Invert"), Convert.ToSingle(fx.invert.value));
       

            switch (fx.logic.value)
            {
                case ColorAccent.Logic.Const: m_Material.SetFloat(Shader.PropertyToID("_Logic"), 0); break;
                case ColorAccent.Logic.Color: m_Material.SetFloat(Shader.PropertyToID("_Logic"), 1); break;
                case ColorAccent.Logic.Luminance: m_Material.SetFloat(Shader.PropertyToID("_Logic"), 2); break;
            }
            BlitTo(m_Material);
        }

        Blit(cmd, latestDest, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(temporaryRTIdA);
        cmd.ReleaseTemporaryRT(temporaryRTIdB);
    }

}
