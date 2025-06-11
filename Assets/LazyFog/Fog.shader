Shader "LemonSpawn/LazyFogURP"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Scale ("Scale", Range(0,5)) = 1
        _Intensity ("Intensity", Range(0,1)) = 0.5
        _Alpha ("Alpha", Range(0,2.5)) = 0.75
        _AlphaSub ("AlphaSub", Range(0,1)) = 0.0
        _Pow ("Pow", Range(0,4)) = 1.0
        _FadeRadius ("Fade Radius", Range(0.2, 0.6)) = 0.45
    }

    SubShader
    {
        Tags { "Queue"="Transparent+101" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Scale;
            float _Intensity;
            float _Alpha;
            float _AlphaSub;
            float _Pow;
            float _FadeRadius;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex) * _Scale;
                OUT.worldPos = worldPos;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 camPos = _WorldSpaceCameraPos;
                float3 viewDir = normalize(camPos - IN.worldPos);

                float4 texCol = tex2D(_MainTex, IN.uv);
                float xx = texCol.r * _Intensity;
                xx = pow(xx, _Pow);

                float4 col = float4(xx * _Color.rgb, texCol.r);

                // Smooth radial fade based on UV distance from center
                float2 centerUV = IN.uv - float2(0.5, 0.5);
                float edgeFade = smoothstep(0.5, _FadeRadius, length(centerUV));
                col.a *= edgeFade;

                col.a *= _Alpha;
                col.a -= _AlphaSub;
                return col;
            }

            ENDHLSL
        }
    }
}
