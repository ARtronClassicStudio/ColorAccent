using System;
using UnityEngine.Rendering.Universal;
public class ColorAccentRenderer : ScriptableRendererFeature
{
    private ColorAccentPass pass;
    public override void Create()=> pass = new ColorAccentPass();

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) => renderer.EnqueuePass(pass);

}
