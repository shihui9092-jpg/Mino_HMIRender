using UnityEngine;

namespace MinoHMI.Rendering
{
    /// <summary>
    /// 平面反射性能监控器
    /// 监控反射系统的性能指标并提供优化建议
    /// </summary>
    public class PlanarReflectionPerformanceMonitor : MonoBehaviour
    {
        [Header("监控设置")]
        [Tooltip("显示性能叠加层")]
        public bool showPerformanceInfo = true;
        
        [Tooltip("目标帧率")]
        public int targetFrameRate = 60;
        
        [Tooltip("自动调整反射质量")]
        public bool autoOptimize = true;

        [Header("引用")]
        public PlanarReflectionManager reflectionManager;

        private int frameCount;
        private float currentFps;
        private float averageFrameTimeMs;
        private float statsIntervalSeconds = 0.5f;
        private float accumulatedStatsTime;
        private float lastAutoOptimizeTime = -999f;

        private const float AutoOptimizeCooldownSeconds = 2f;

        private GUIStyle labelStyle;
        private bool initialized;

        private void Start()
        {
            if (reflectionManager == null)
            {
                reflectionManager = FindObjectOfType<PlanarReflectionManager>();
            }

            InitializeStyle();
        }

        private void Update()
        {
            UpdateFPS();

            if (autoOptimize)
            {
                AutoOptimizeQuality();
            }
        }

        private void UpdateFPS()
        {
            frameCount++;
            accumulatedStatsTime += Time.unscaledDeltaTime;

            if (accumulatedStatsTime >= statsIntervalSeconds)
            {
                currentFps = frameCount / accumulatedStatsTime;
                averageFrameTimeMs = (accumulatedStatsTime / Mathf.Max(1, frameCount)) * 1000f;
                frameCount = 0;
                accumulatedStatsTime = 0f;
            }
        }

        private void AutoOptimizeQuality()
        {
            if (reflectionManager == null || !reflectionManager.enableReflection)
                return;

            if (Time.unscaledTime - lastAutoOptimizeTime < AutoOptimizeCooldownSeconds)
                return;

            if (currentFps < targetFrameRate * 0.8f)
            {
                var currentQuality = reflectionManager.settings?.currentQuality ?? ReflectionQuality.Medium;
                
                if (currentQuality == ReflectionQuality.Ultra)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.High);
                    lastAutoOptimizeTime = Time.unscaledTime;
                    Debug.Log("[PlanarReflection] 自动降质: Ultra -> High");
                }
                else if (currentQuality == ReflectionQuality.High)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.Medium);
                    lastAutoOptimizeTime = Time.unscaledTime;
                    Debug.Log("[PlanarReflection] 自动降质: High -> Medium");
                }
                else if (currentQuality == ReflectionQuality.Medium)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.Low);
                    lastAutoOptimizeTime = Time.unscaledTime;
                    Debug.Log("[PlanarReflection] 自动降质: Medium -> Low");
                }
            }
            else if (currentFps > targetFrameRate * 1.2f)
            {
                var currentQuality = reflectionManager.settings?.currentQuality ?? ReflectionQuality.Medium;
                
                if (currentQuality == ReflectionQuality.Low)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.Medium);
                    lastAutoOptimizeTime = Time.unscaledTime;
                    Debug.Log("[PlanarReflection] 自动升质: Low -> Medium");
                }
                else if (currentQuality == ReflectionQuality.Medium)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.High);
                    lastAutoOptimizeTime = Time.unscaledTime;
                    Debug.Log("[PlanarReflection] 自动升质: Medium -> High");
                }
            }
        }

        private void OnGUI()
        {
            if (!showPerformanceInfo)
                return;

            if (!initialized)
                InitializeStyle();

            float x = 10;
            float y = 10;
            float width = 300;
            float lineHeight = 20;

            GUI.Box(new Rect(x - 5, y - 5, width + 10, lineHeight * 8 + 10), "");
            GUI.Label(new Rect(x, y, width, lineHeight), "=== 平面反射性能监控 ===", labelStyle);
            y += lineHeight;

            Color fpsColor = currentFps >= targetFrameRate ? Color.green : (currentFps >= targetFrameRate * 0.8f ? Color.yellow : Color.red);
            GUI.contentColor = fpsColor;
            GUI.Label(new Rect(x, y, width, lineHeight), $"FPS: {currentFps:F1} (目标: {targetFrameRate})", labelStyle);
            GUI.contentColor = Color.white;
            y += lineHeight;

            GUI.Label(new Rect(x, y, width, lineHeight), $"帧时间: {averageFrameTimeMs:F2} ms", labelStyle);
            y += lineHeight;

            if (reflectionManager != null)
            {
                bool enabled = reflectionManager.enableReflection;
                GUI.contentColor = enabled ? Color.green : Color.gray;
                GUI.Label(new Rect(x, y, width, lineHeight), $"反射状态: {(enabled ? "启用" : "禁用")}", labelStyle);
                GUI.contentColor = Color.white;
                y += lineHeight;

                if (reflectionManager.settings != null)
                {
                    var quality = reflectionManager.settings.currentQuality;
                    GUI.Label(new Rect(x, y, width, lineHeight), $"质量等级: {quality}", labelStyle);
                    y += lineHeight;

                    var settings = reflectionManager.settings.GetCurrentQualitySettings();
                    GUI.Label(new Rect(x, y, width, lineHeight),
                        $"分辨率: {settings.resolution.x}x{settings.resolution.y} ({settings.resolutionScale:F2}x)",
                        labelStyle);
                    y += lineHeight;

                    GUI.Label(new Rect(x, y, width, lineHeight),
                        $"HDR: {(settings.useHDR ? "开启" : "关闭")} | 更新率: 1/{settings.updateRate}",
                        labelStyle);
                    y += lineHeight;
                }
            }
            else
            {
                GUI.contentColor = Color.red;
                GUI.Label(new Rect(x, y, width, lineHeight), "未找到反射管理器", labelStyle);
                GUI.contentColor = Color.white;
                y += lineHeight;
            }

            GUI.contentColor = autoOptimize ? Color.cyan : Color.gray;
            GUI.Label(new Rect(x, y, width, lineHeight), $"自动优化: {(autoOptimize ? "启用" : "禁用")}", labelStyle);
            GUI.contentColor = Color.white;
        }

        private void InitializeStyle()
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft
            };
            initialized = true;
        }

        /// <summary>
        /// 获取性能报告
        /// </summary>
        public PerformanceReport GetPerformanceReport()
        {
            PerformanceReport report = new PerformanceReport
            {
                currentFPS = currentFps,
                targetFPS = targetFrameRate,
                frameTime = averageFrameTimeMs,
                reflectionEnabled = reflectionManager?.enableReflection ?? false
            };

            if (reflectionManager != null && reflectionManager.settings != null)
            {
                report.currentQuality = reflectionManager.settings.currentQuality;
                var settings = reflectionManager.settings.GetCurrentQualitySettings();
                report.resolutionWidth = settings.resolution.x;
                report.resolutionHeight = settings.resolution.y;
                report.resolutionScale = settings.resolutionScale;
                report.useHDR = settings.useHDR;
                report.updateRate = settings.updateRate;
            }

            return report;
        }

        /// <summary>
        /// 性能评估
        /// </summary>
        public PerformanceLevel EvaluatePerformance()
        {
            float targetRatio = currentFps / targetFrameRate;

            if (targetRatio >= 1.0f)
                return PerformanceLevel.Excellent;
            if (targetRatio >= 0.9f)
                return PerformanceLevel.Good;
            if (targetRatio >= 0.7f)
                return PerformanceLevel.Acceptable;
            return PerformanceLevel.Poor;
        }
    }

    /// <summary>
    /// 性能报告结构
    /// </summary>
    [System.Serializable]
    public struct PerformanceReport
    {
        public float currentFPS;
        public int targetFPS;
        public float frameTime;
        public bool reflectionEnabled;
        public ReflectionQuality currentQuality;
        public int resolutionWidth;
        public int resolutionHeight;
        public float resolutionScale;
        public bool useHDR;
        public int updateRate;

        public override string ToString()
        {
            return $"性能报告:\n" +
                   $"FPS: {currentFPS:F1} / {targetFPS}\n" +
                   $"帧时间: {frameTime:F2} ms\n" +
                   $"反射: {(reflectionEnabled ? "启用" : "禁用")}\n" +
                   $"质量: {currentQuality}\n" +
                   $"分辨率: {resolutionWidth}x{resolutionHeight} ({resolutionScale:F2}x)\n" +
                   $"HDR: {(useHDR ? "是" : "否")}\n" +
                   $"更新率: 1/{updateRate}";
        }
    }

    /// <summary>
    /// 性能等级
    /// </summary>
    public enum PerformanceLevel
    {
        Poor,
        Acceptable,
        Good,
        Excellent
    }
}
