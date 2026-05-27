Shader "MinoHMI/Unlit"
{
    Properties
    {
        [Header((_Base))]
        [Space(5)]
        [MainTexture] _BaseMap("主贴图", 2D) = "white" {}
        [MainColor] _BaseColor("颜色", Color) = (1, 1, 1, 1)
        [Header((_Rendering))]
        [Space(5)]
        [Toggle(_ALPHATEST_ON)] _AlphaClip("裁剪开关", Float) = 0
        _Cutoff("裁剪值", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("背面剔除", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            // 按需启用：Alpha 裁剪
            #pragma shader_feature_local _ALPHATEST_ON

            // 按需启用：GPU Instancing / Fog
            // #pragma multi_compile_instancing
            // #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 color = baseMap * _BaseColor;

                #ifdef _ALPHATEST_ON
                    clip(color.a - _Cutoff);
                #endif

                return color;
            }
            ENDHLSL
        }
    }

    // -------------------------------------------------------------------------
    // 透明版本示例：复制 SubShader 并修改 Tags / Blend / ZWrite
    //
    // Tags { "RenderType" = "Transparent" "Queue" = "Transparent" ... }
    // Blend SrcAlpha OneMinusSrcAlpha
    // ZWrite Off
    // 移除 _ALPHATEST_ON，直接输出 alpha
    // -------------------------------------------------------------------------

    FallBack "Hidden/Universal/FallbackError"
}
