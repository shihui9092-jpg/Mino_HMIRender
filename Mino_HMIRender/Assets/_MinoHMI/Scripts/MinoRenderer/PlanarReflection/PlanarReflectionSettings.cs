using UnityEngine;

namespace MinoHMI.Rendering
{
    /// <summary>
    /// 平面反射全局设置
    /// ScriptableObject配置文件
    /// </summary>
    [CreateAssetMenu(fileName = "PlanarReflectionSettings", menuName = "MinoHMI/Rendering/Planar Reflection Settings")]
    public class PlanarReflectionSettings : ScriptableObject
    {
        [Header("质量预设")]
        [Tooltip("当前质量等级")]
        public ReflectionQuality currentQuality = ReflectionQuality.Medium;

        [Header("低质量设置")]
        public ReflectionQualitySettings lowQuality = new ReflectionQualitySettings
        {
            resolution = new Vector2Int(256, 256),
            resolutionScale = 0.5f,
            useHDR = false,
            maxDistance = 30f,
            updateRate = 2
        };

        [Header("中等质量设置")]
        public ReflectionQualitySettings mediumQuality = new ReflectionQualitySettings
        {
            resolution = new Vector2Int(512, 512),
            resolutionScale = 0.75f,
            useHDR = false,
            maxDistance = 50f,
            updateRate = 1
        };

        [Header("高质量设置")]
        public ReflectionQualitySettings highQuality = new ReflectionQualitySettings
        {
            resolution = new Vector2Int(1024, 1024),
            resolutionScale = 1.0f,
            useHDR = true,
            maxDistance = 80f,
            updateRate = 1
        };

        [Header("超高质量设置")]
        public ReflectionQualitySettings ultraQuality = new ReflectionQualitySettings
        {
            resolution = new Vector2Int(2048, 2048),
            resolutionScale = 1.0f,
            useHDR = true,
            maxDistance = 100f,
            updateRate = 1
        };

        [Header("性能优化")]
        [Tooltip("是否启用自动LOD")]
        public bool enableAutoLOD = true;
        
        [Tooltip("LOD距离阈值")]
        public float[] lodDistances = new float[] { 20f, 50f, 100f };
        
        [Tooltip("是否在非焦点时降低更新率")]
        public bool reduceFPSWhenUnfocused = true;

        /// <summary>
        /// 获取当前质量设置
        /// </summary>
        public ReflectionQualitySettings GetCurrentQualitySettings()
        {
            switch (currentQuality)
            {
                case ReflectionQuality.Low:
                    return lowQuality;
                case ReflectionQuality.Medium:
                    return mediumQuality;
                case ReflectionQuality.High:
                    return highQuality;
                case ReflectionQuality.Ultra:
                    return ultraQuality;
                default:
                    return mediumQuality;
            }
        }

        /// <summary>
        /// 根据设备性能自动选择质量等级
        /// </summary>
        public void AutoDetectQuality()
        {
            // 根据系统内存判断
            int systemMemoryMB = SystemInfo.systemMemorySize;
            int graphicsMemoryMB = SystemInfo.graphicsMemorySize;

            if (systemMemoryMB < 4096 || graphicsMemoryMB < 2048)
            {
                currentQuality = ReflectionQuality.Low;
            }
            else if (systemMemoryMB < 8192 || graphicsMemoryMB < 4096)
            {
                currentQuality = ReflectionQuality.Medium;
            }
            else if (systemMemoryMB < 16384 || graphicsMemoryMB < 6144)
            {
                currentQuality = ReflectionQuality.High;
            }
            else
            {
                currentQuality = ReflectionQuality.Ultra;
            }

            Debug.Log($"[PlanarReflection] Auto-detected quality: {currentQuality} " +
                      $"(RAM: {systemMemoryMB}MB, VRAM: {graphicsMemoryMB}MB)");
        }
    }

    /// <summary>
    /// 反射质量设置结构
    /// </summary>
    [System.Serializable]
    public struct ReflectionQualitySettings
    {
        [Tooltip("反射纹理分辨率（宽高至少为 1）")]
        public Vector2Int resolution;
        
        [Tooltip("分辨率缩放")]
        [Range(0.25f, 1.0f)]
        public float resolutionScale;
        
        [Tooltip("是否使用HDR")]
        public bool useHDR;
        
        [Tooltip("最大反射距离")]
        public float maxDistance;
        
        [Tooltip("更新频率(每N帧更新一次,1=每帧)")]
        [Range(1, 10)]
        public int updateRate;
    }
}
