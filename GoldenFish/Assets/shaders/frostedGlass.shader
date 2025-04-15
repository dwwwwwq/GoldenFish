Shader "Unlit/frostedGlass"
{
    Properties
    {
        _Transparency ("Transparency", Range(0,1)) = 0.3 // 控制透明度
        _BlurStrength ("Blur Strength", Range(0,1)) = 0.5 // 控制模糊程度
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 透明混合
            // ZWrite Off // 不写入深度缓冲，避免遮挡问题

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Transparency;
            float _BlurStrength;

            sampler2D _MainTex;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a = lerp(1, _Transparency, _BlurStrength);
                return col;
            }
            ENDCG
        }
    }
}
