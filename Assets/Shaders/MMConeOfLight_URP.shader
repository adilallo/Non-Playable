Shader "MoreMountains/ConeOfLight_URP"
{
    Properties
    {
        _MainTex  ("Diffuse Texture", 2D)  = "white" {}
        _Contrast ("Contrast"       , Float) = 0.5
        _Color    ("Color"          , Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            Name "UnlitTransparent"
            Blend DstColor One
            Cull  Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog

            // ── URP / Core includes ───────────────────────────────────────────────
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4   _MainTex_ST;
            float4   _Color;
            float    _Contrast;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 posHCS : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.posHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv     = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color  = IN.color;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half3 rgb = tex.rgb * _Color.rgb * IN.color.rgb * _Contrast;
                half  a   = tex.a   * _Color.a   * IN.color.a;

                half4 col = half4(rgb, a);

                return col;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
