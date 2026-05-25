Shader "Hidden/KawaseBlur"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BlurOffset("Blur Offset", Float) = 1.0
    }

        SubShader
        {
            Cull Off ZWrite Off ZTest Always

            Pass
            {
                Name "KawaseBlur"

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
                    float4 positionCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float _BlurOffset;
                float2 _MainTex_TexelSize;

                Varyings vert(Attributes input)
                {
                    Varyings output;
                    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                    output.uv = input.uv;
                    return output;
                }

                half4 frag(Varyings input) : SV_Target
                {
                    float2 texelSize = _MainTex_TexelSize;
                    float2 uv = input.uv;

                    half4 color = 0;
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texelSize.x, -texelSize.y) * _BlurOffset);
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texelSize.x, -texelSize.y) * _BlurOffset);
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texelSize.x,  texelSize.y) * _BlurOffset);
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texelSize.x,  texelSize.y) * _BlurOffset);
                    return color * 0.25;
                }
                ENDHLSL
            }
        }
}