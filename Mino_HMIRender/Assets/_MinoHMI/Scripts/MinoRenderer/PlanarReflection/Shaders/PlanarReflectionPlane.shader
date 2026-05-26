Shader "MinoHMI/PlanarReflectionPlane"
{
    Properties
    {
        [Header(Reflection)]
        _ReflectionTint("Reflection Tint", Color) = (1, 1, 1, 1)
        _ReflectionIntensity("Reflection Intensity", Range(0, 1)) = 0.5
        _ReflectionBlur("Reflection Blur", Range(0, 1)) = 0.0

        [Header(Fresnel)]
        _FresnelPower("Fresnel Power", Range(0, 5)) = 2.0

        [Header(Surface)]
        _Roughness("Roughness", Range(0, 1)) = 0.1

        [Header(Distance Fade)]
        _ReflectionFadeParams("Fade Params (Start, End, 1/(End-Start), unused)", Vector) = (20, 50, 0.033, 0)

        [Header(Alpha)]
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
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 viewDirWS  : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_PlanarReflectionTexture);
            SAMPLER(sampler_PlanarReflectionTexture);
            float4 _PlanarReflectionTexture_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _ReflectionTint;
                float4 _ReflectionFadeParams;
                float _ReflectionIntensity;
                float _ReflectionBlur;
                float _FresnelPower;
                float _Roughness;
                float _AlphaCutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

                return output;
            }

            half4 SampleReflectionTexture(float2 reflectionUV, float blur)
            {
                half4 color = SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, reflectionUV);

                if (blur <= 0.001)
                {
                    return color;
                }

                // 简易 5 点模糊，模糊强度由 PlanarReflectionPlane.reflectionBlur 控制
                float2 offset = _PlanarReflectionTexture_TexelSize.xy * blur * 4.0;
                half4 blurColor = color * 0.2;
                blurColor += SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, reflectionUV + float2(offset.x, 0)) * 0.2;
                blurColor += SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, reflectionUV - float2(offset.x, 0)) * 0.2;
                blurColor += SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, reflectionUV + float2(0, offset.y)) * 0.2;
                blurColor += SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, reflectionUV - float2(0, offset.y)) * 0.2;
                return blurColor;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 reflectionColor = half4(0, 0, 0, 0);
                if (IsPlanarReflectionUVValid(input.positionWS))
                {
                    float2 reflectionUV = GetPlanarReflectionUV(input.positionWS);
                    reflectionColor = SampleReflectionTexture(reflectionUV, _ReflectionBlur);
                }

                reflectionColor *= _ReflectionTint;

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);

                // 菲涅尔：边缘反射更强
                float fresnel = pow(saturate(1.0 - dot(normalWS, viewDirWS)), _FresnelPower);

                // 距离淡出
                float distanceToCamera = distance(_WorldSpaceCameraPos.xyz, input.positionWS);
                float fadeFactor = saturate((distanceToCamera - _ReflectionFadeParams.x) * _ReflectionFadeParams.z);
                fadeFactor = 1.0 - fadeFactor;

                // 粗糙度：数值越大反射越弱
                float roughnessFactor = 1.0 - (_Roughness * 0.8);

                float finalIntensity = _ReflectionIntensity * fresnel * fadeFactor * roughnessFactor;

                reflectionColor.rgb *= finalIntensity;
                reflectionColor.a *= finalIntensity;

                // 除反射物体像素外全部透明
                clip(reflectionColor.a - _AlphaCutoff);
                return reflectionColor;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
