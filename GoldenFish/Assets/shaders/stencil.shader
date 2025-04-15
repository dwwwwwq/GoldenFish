Shader "Unlit/stencil"
{
    Properties
    {
        [IntRange]_index("sencil index",Range(0,255)) = 0
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"

        }
        

        Pass
        {
            Blend Zero One
            ZWrite Off

            stencil
            {
                Ref[_index]
                Comp Always
                Pass Replace
                Fail Keep
            }
        }
    }
}
