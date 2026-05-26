Shader "MinoHMI/PlanarReflectionTransparentOnly"
{
    Properties
    {
        [Header(Reflection)]
        _ReflectionTint("Reflection Tint", Color) = (1, 1, 1, 1)
        _ReflectionIntensity("Reflection Intensity", Range(0, 1)) = 1.0
        _AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "PlanarReflectionCommon.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_PlanarReflectionTexture);
            SAMPLER(sampler_PlanarReflectionTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _ReflectionTint;
                float _ReflectionIntensity;
                float _AlphaCutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 reflectionColor = half4(0, 0, 0, 0);
                if (IsPlanarReflectionUVValid(input.positionWS))
                {
                    float2 reflectionUV = GetPlanarReflectionUV(input.positionWS);
                    reflectionColor = SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, reflectionUV);
                }

                reflectionColor *= _ReflectionTint;
                reflectionColor.rgb *= _ReflectionIntensity;
                reflectionColor.a *= _ReflectionIntensity;

                // 除反射物体像素外全部透明
                clip(reflectionColor.a - _AlphaCutoff);
                return reflectionColor;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
