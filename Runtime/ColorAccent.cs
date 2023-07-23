using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable,VolumeComponentMenuForRenderPipeline("Color Accent",typeof(UniversalRenderPipeline))]
public class ColorAccent : VolumeComponent,IPostProcessComponent
{
    public BoolParameter enabled = new(false,false);
    public ColorParameter accentColor = new(Color.red, false);
    public ColorParameter logicColor = new(new(0.5843138f, 0.9921569f, 0.6817982f,1), false);
    public VolumeParameter<Logic> logic = new() { value = Logic.Color };
    public ClampedFloatParameter lerp = new(1,0,1,false);
    public FloatParameter threshold = new(0.3f, false);
    public FloatParameter contrast = new(1, false);
    public BoolParameter invert = new(false, false);
   

    public enum Logic
    {
        Const,
        Color,
        Luminance,
    }

    public bool IsActive() => enabled.value;
    public bool IsTileCompatible() => true;

}