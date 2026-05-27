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

// 屏幕空间遮罩：在 [底部, 顶部] 区间内显示反射，区间外按遮罩柔化渐隐
// screenMaskRange: x=底部, y=顶部, w=强度(0=关闭, 1=完全生效)
half EvaluatePlanarReflectionScreenMask(float4 positionCS, float4 screenMaskRange, float maskSoftness)
{
    // 遮罩强度
    float maskStrength = saturate(screenMaskRange.w);
    if (maskStrength <= 0.0001)
    {
        return 1.0;
    }

    // 归一化屏幕 Y
    float screenHeight = max(_ScreenParams.y, 1.0);
    float normalizedScreenY = saturate(positionCS.y / screenHeight);

    float maskBottom = saturate(screenMaskRange.x);
    float maskTop = saturate(max(screenMaskRange.y, maskBottom + 0.0001));
    float edgeSoftness = max(maskSoftness, 0.0001);

    float lowerMask = smoothstep(maskBottom - edgeSoftness, maskBottom + edgeSoftness, normalizedScreenY);
    float upperMask = 1.0 - smoothstep(maskTop - edgeSoftness, maskTop + edgeSoftness, normalizedScreenY);
    half rangeMask = lowerMask * upperMask;

    return lerp(1.0, rangeMask, maskStrength);
}

#endif
