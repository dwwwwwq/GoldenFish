Shader "Unlit/halfTone"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _CyanDotSize("Cyan Dot Size", Float) = 40
        _MagentaDotSize("Magenta Dot Size", Float) = 40
        _YellowDotSize("Yellow Dot Size", Float) = 40
        _BlackDotSize("Black Dot Size", Float) = 40
        _CyanBias("Cyan Bias", Float) = 0
        _MagentaBias("Magenta Bias", Float) = 0
        _YellowBias("Yellow Bias", Float) = 0
        _BlackBias("Black Bias", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "HalftonePass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float _CyanDotSize, _MagentaDotSize, _YellowDotSize, _BlackDotSize;
            float _CyanBias, _MagentaBias, _YellowBias, _BlackBias;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float halftone(float2 uv, float v, float bias, float dotSize, float curve)
            {
                float halftone = (sin(uv.x * _MainTex_TexelSize.z * dotSize) + sin(uv.y * _MainTex_TexelSize.w * dotSize)) * 0.5;
                float threshold = pow(v + bias, curve);
                return halftone < threshold ? 1.0 : 0.0;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 col = saturate(tex2D(_MainTex, IN.uv).rgb);
                float r = col.r;
                float g = col.g;
                float b = col.b;
                float k = min(1.0 - r, min(1.0 - g, 1.0 - b));
                float invK = 1.0 - k;

                float3 cmy = 0.0;

                if (invK > 0.0)
                {
                    cmy.r = (1.0 - r - k) / invK;
                    cmy.g = (1.0 - g - k) / invK;
                    cmy.b = (1.0 - b - k) / invK;
                }

                float2x2 R;

                // Cyan
                R = float2x2(cos(0.261799), -sin(0.261799), sin(0.261799), cos(0.261799));
                cmy.r = halftone(mul(IN.uv, R), cmy.r, _CyanBias, _CyanDotSize, 1.0);

                // Magenta
                R = float2x2(cos(1.309), -sin(1.309), sin(1.309), cos(1.309));
                cmy.g = halftone(mul(IN.uv, R), cmy.g, _MagentaBias, _MagentaDotSize, 1.0);

                // Yellow
                cmy.b = halftone(IN.uv, cmy.b, _YellowBias, _YellowDotSize, 1.0);

                // Black
                R = float2x2(cos(0.785398), -sin(0.785398), sin(0.785398), cos(0.785398));
                float black = halftone(mul(IN.uv, R), k, _BlackBias, _BlackDotSize, 0.15);

                return float4(saturate(cmy), black);
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/ShaderFallbackError"
}