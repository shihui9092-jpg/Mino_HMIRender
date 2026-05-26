#ifndef PLANAR_REFLECTION_COMMON_INCLUDED
#define PLANAR_REFLECTION_COMMON_INCLUDED

// 反射相机 View * Projection，由 PlanarReflectionCamera 每帧设置
float4x4 _PlanarReflectionVP;

float2 GetPlanarReflectionUV(float3 worldPos)
{
    float4 clipPos = mul(_PlanarReflectionVP, float4(worldPos, 1.0));
    float2 uv = clipPos.xy / clipPos.w;
    uv = uv * 0.5 + 0.5;

    return uv;
}

bool IsPlanarReflectionUVValid(float3 worldPos)
{
    float4 clipPos = mul(_PlanarReflectionVP, float4(worldPos, 1.0));
    return clipPos.w > 0.0001;
}

#endif
