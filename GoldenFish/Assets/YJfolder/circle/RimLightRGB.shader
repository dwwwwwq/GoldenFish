Shader "Custom/RimLightRGB"
{
    
    Properties
    {
        _MainColor("Main Color", Color) = (0, 0, 0, 1)
        _MainColorTransparent("Main Color Transparent", Range(0.0, 1.0)) = 0.0
        _RimLightColor("Rim Light Color", Color) = (1, 1, 1, 1)
        _RimLightStrength("Rim Light Strength", Range(0.0, 100.0)) = 10.0
        _RimLightBias("Rim Light Bias", Range(-1.0, 1.0)) = 0.0
        _RimLightSmoothstep("Rim Light Smoothstep", Range(0.0, 1.0)) = 1.0
    }
        SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
        }
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        // 添加VR支持
        #pragma multi_compile_instancing
        #pragma instancing_options renderinglayer
        #pragma multi_compile _ _USE_DRAW_PROCEDURAL

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
        // 包含VR相关功能
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _MainColor;
        float _MainColorTransparent;
        float4 _RimLightColor;
        float _RimLightStrength;
        float _RimLightBias;
        float _RimLightSmoothstep;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings vert(Attributes v)
        {
            Varyings o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
            o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            return o;
        }

        half4 frag(Varyings i) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

            float3 normal = normalize(i.normalWS);
            // 修改相机位置获取方式以支持VR
            float3 viewDirection = normalize(GetWorldSpaceViewDir(i.positionWS));

            float NdotV = dot(normal, viewDirection);

            half4 color = half4(_MainColor.rgb, _MainColorTransparent);
            half3 rimLight = _RimLightColor.rgb * _RimLightStrength * smoothstep(0.5 - _RimLightSmoothstep * 0.5, 0.5 + _RimLightSmoothstep * 0.5, 1 - max(NdotV + _RimLightBias * 0.5, 0));
            color += half4(rimLight, smoothstep(0.5 - _RimLightSmoothstep * 0.5, 0.5 + _RimLightSmoothstep * 0.5, 1 - max(NdotV + _RimLightBias * 0.5, 0)));

            return color;
        }

        ENDHLSL
    }
    }
}