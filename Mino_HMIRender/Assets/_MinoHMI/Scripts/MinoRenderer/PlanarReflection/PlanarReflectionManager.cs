using UnityEngine;

namespace MinoHMI.Rendering
{
    /// <summary>
    /// 平面反射管理器
    /// 统一管理场景中的所有反射组件
    /// </summary>
    public class PlanarReflectionManager : MonoBehaviour
    {
        [Header("引用")]
        [Tooltip("平面反射设置资源")]
        public PlanarReflectionSettings settings;
        
        [Tooltip("平面反射相机组件")]
        public PlanarReflectionCamera reflectionCamera;
        
        [Tooltip("平面反射地面组件列表")]
        public PlanarReflectionPlane[] reflectionPlanes;

        [Header("运行时控制")]
        [Tooltip("启用平面反射")]
        public bool enableReflection = true;
        
        [Tooltip("运行时反射质量等级")]
        public ReflectionQuality runtimeQuality = ReflectionQuality.Medium;

        private ReflectionQualitySettings currentSettings;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化反射系统
        /// </summary>
        public void Initialize()
        {
            if (settings == null)
            {
                Debug.LogError("[PlanarReflectionManager] 缺少反射设置配置!");
                return;
            }

            // 自动检测质量
            if (Application.isPlaying)
            {
                settings.AutoDetectQuality();
                runtimeQuality = settings.currentQuality;
            }

            // 应用质量设置
            ApplyQualitySettings(runtimeQuality);

            // 查找所有反射平面
            if (reflectionPlanes == null || reflectionPlanes.Length == 0)
            {
                reflectionPlanes = FindObjectsOfType<PlanarReflectionPlane>();
            }

            Debug.Log($"[PlanarReflectionManager] 已初始化,质量等级: {runtimeQuality}, 反射平面数量: {reflectionPlanes.Length}");
        }

        private void Update()
        {
            if (reflectionCamera == null)
                return;

            reflectionCamera.enableReflection = enableReflection;
        }

        /// <summary>
        /// 应用质量设置
        /// </summary>
        public void ApplyQualitySettings(ReflectionQuality quality)
        {
            if (settings == null)
                return;

            settings.currentQuality = quality;
            runtimeQuality = quality;
            currentSettings = settings.GetCurrentQualitySettings();

            // 应用到反射相机
            if (reflectionCamera != null)
            {
                reflectionCamera.reflectionResolution = currentSettings.resolution;
                reflectionCamera.resolutionScale = currentSettings.resolutionScale;
                reflectionCamera.useHDR = currentSettings.useHDR;
                reflectionCamera.maxReflectionDistance = currentSettings.maxDistance;
                reflectionCamera.reflectionUpdateRate = Mathf.Clamp(currentSettings.updateRate, 1, 10);
            }

            // 应用到反射平面
            if (reflectionPlanes != null)
            {
                foreach (var plane in reflectionPlanes)
                {
                    if (plane != null)
                    {
                        plane.SetQualityPreset(quality);
                    }
                }
            }

            Debug.Log($"[PlanarReflectionManager] 已应用质量设置: {quality}");
        }

        /// <summary>
        /// 切换反射开关
        /// </summary>
        public void ToggleReflection(bool enable)
        {
            enableReflection = enable;
            
            if (reflectionCamera != null)
            {
                reflectionCamera.enableReflection = enable;
            }

            Debug.Log($"[PlanarReflectionManager] 反射{(enable ? "已启用" : "已禁用")}");
        }

        /// <summary>
        /// 设置反射强度
        /// </summary>
        public void SetReflectionIntensity(float intensity)
        {
            intensity = Mathf.Clamp01(intensity);

            if (reflectionPlanes != null)
            {
                foreach (var plane in reflectionPlanes)
                {
                    if (plane != null)
                    {
                        plane.reflectionIntensity = intensity;
                    }
                }
            }
        }

        /// <summary>
        /// 设置反射淡出距离
        /// </summary>
        public void SetFadeDistance(float start, float end)
        {
            if (reflectionPlanes != null)
            {
                foreach (var plane in reflectionPlanes)
                {
                    if (plane != null)
                    {
                        plane.fadeStart = start;
                        plane.fadeEnd = end;
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 编辑器下实时应用设置
            if (Application.isPlaying && runtimeQuality != settings?.currentQuality)
            {
                ApplyQualitySettings(runtimeQuality);
            }
        }
#endif
    }
}
