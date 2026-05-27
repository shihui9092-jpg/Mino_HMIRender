Shader "MinoHMI/PlanarReflectionPlane"
{
    Properties
    {
        [Header((_Reflection))]
        [Space(5)]
        _ReflectionTint("反射色调", Color) = (1, 1, 1, 1)
        _ReflectionIntensity("反射强度", Range(0, 1)) = 0.5
        _ReflectionBlur("反射模糊", Range(0, 1)) = 0.0
        _ReflectionFadeParams("淡出参数(起始,结束,斜率,预留)", Vector) = (20, 50, 0.033, 0)

        [Header((_Surface))]
        [Space(5)]
        _FresnelPower("菲涅尔强度", Range(0, 5)) = 2.0
        _Roughness("粗糙度", Range(0, 1)) = 0.1

        [Header((_Mask))]
        [Space(5)]
        _ScreenMaskRange("屏幕遮罩(底部,顶部,预留,强度)", Vector) = (0, 0.65, 0, 1)
        _ScreenMaskSoftness("屏幕遮罩柔化", Range(0, 0.5)) = 0.05

        [Header((_Alpha))]
        [Space(5)]
        _AlphaCutoff("透明裁剪", Range(0, 1)) = 0.01
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
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 viewDirWS  : TEXCOORD2;
                float2 uv         : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_PlanarReflectionTexture);
            SAMPLER(sampler_PlanarReflectionTexture);
            float4 _PlanarReflectionTexture_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _ReflectionTint;
                float4 _ReflectionFadeParams;
                float4 _ScreenMaskRange;
                float _ScreenMaskSoftness;
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
                output.uv = input.uv;

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
                half screenMask = EvaluatePlanarReflectionScreenMask(
                    input.positionCS,
                    _ScreenMaskRange,
                    _ScreenMaskSoftness);

                reflectionColor.rgb *= finalIntensity * screenMask;
                reflectionColor.a *= finalIntensity * screenMask;

                half bottomUpHeightFactor = 1.0 - saturate(input.uv.y);
                clip(reflectionColor.a - _AlphaCutoff * bottomUpHeightFactor);
                return reflectionColor;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
