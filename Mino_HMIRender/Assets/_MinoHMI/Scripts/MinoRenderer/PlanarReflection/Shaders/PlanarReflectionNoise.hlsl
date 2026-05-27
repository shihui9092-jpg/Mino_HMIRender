#ifndef PLANAR_REFLECTION_NOISE_INCLUDED
#define PLANAR_REFLECTION_NOISE_INCLUDED

// 2D 哈希，输出 0~1
float PlanarReflectionHash21(float2 p)
{
    p = frac(p * float2(443.897, 441.423));
    p += dot(p, p.yx + 19.19);
    return frac(p.x * p.y);
}

// 平滑插值
float PlanarReflectionSmoothStep01(float t)
{
    return t * t * (3.0 - 2.0 * t);
}

// 2D 值噪波，输出 0~1
float PlanarReflectionValueNoise(float2 uv)
{
    float2 cell = floor(uv);
    float2 fracUV = frac(uv);

    float a = PlanarReflectionHash21(cell);
    float b = PlanarReflectionHash21(cell + float2(1.0, 0.0));
    float c = PlanarReflectionHash21(cell + float2(0.0, 1.0));
    float d = PlanarReflectionHash21(cell + float2(1.0, 1.0));

    float2 smoothFrac = float2(
        PlanarReflectionSmoothStep01(fracUV.x),
        PlanarReflectionSmoothStep01(fracUV.y));

    float ab = lerp(a, b, smoothFrac.x);
    float cd = lerp(c, d, smoothFrac.x);
    return lerp(ab, cd, smoothFrac.y);
}

// 分形噪波，输出 0~1
float PlanarReflectionFractalNoise(float2 uv, int octaveCount)
{
    float amplitude = 0.5;
    float frequency = 1.0;
    float sum = 0.0;
    float weight = 0.0;

    for (int octaveIndex = 0; octaveIndex < octaveCount; octaveIndex++)
    {
        sum += PlanarReflectionValueNoise(uv * frequency) * amplitude;
        weight += amplitude;
        frequency *= 2.0;
        amplitude *= 0.5;
    }

    return weight > 0.0001 ? sum / weight : 0.0;
}

// 基于世界 XZ 的零均值扭曲偏移（单位：UV），不依赖反射 UV，避免调参导致反射整体漂移
float2 PlanarReflectionGetWorldAnchoredDistortionOffset(
    float3 positionWS,
    float distortionStrength,
    float distortionSpeed,
    float distortionFrequency,
    float distortionOctaves)
{
    if (distortionStrength <= 0.000001)
    {
        return float2(0.0, 0.0);
    }

    float timePhase = _Time.y * distortionSpeed;
    float2 noiseCoord = positionWS.xz * max(distortionFrequency, 0.01);
    noiseCoord += float2(timePhase * 0.31, timePhase * 0.47);

    int octaveCount = (int)clamp(distortionOctaves, 1.0, 4.0);
    float noiseX = PlanarReflectionFractalNoise(noiseCoord, octaveCount);
    float noiseY = PlanarReflectionFractalNoise(noiseCoord + float2(17.13, 9.71), octaveCount);

    // 映射到 [-1, 1]，均值为 0，仅改变局部波纹形态
    float2 zeroMeanNoise = float2(noiseX, noiseY) * 2.0 - 1.0;
    return zeroMeanNoise * distortionStrength;
}

#endif
