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
        [Tooltip("是否显示性能信息")]
        public bool showPerformanceInfo = true;
        
        [Tooltip("目标帧率")]
        public int targetFrameRate = 60;
        
        [Tooltip("自动优化")]
        public bool autoOptimize = true;

        [Header("引用")]
        public PlanarReflectionManager reflectionManager;

        // 性能数据
        private float deltaTime = 0.0f;
        private int frameCount = 0;
        private float fps = 0.0f;
        private float updateInterval = 0.5f;
        private float accumulatedTime = 0.0f;

        private GUIStyle labelStyle;
        private bool initialized = false;

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
            deltaTime += Time.unscaledDeltaTime;
            accumulatedTime += Time.unscaledDeltaTime;

            if (accumulatedTime >= updateInterval)
            {
                fps = frameCount / accumulatedTime;
                frameCount = 0;
                accumulatedTime = 0.0f;
            }
        }

        private void AutoOptimizeQuality()
        {
            if (reflectionManager == null || !reflectionManager.enableReflection)
                return;

            // 根据帧率自动调整质量
            if (fps < targetFrameRate * 0.8f) // 低于目标80%
            {
                // 降低质量
                var currentQuality = reflectionManager.settings?.currentQuality ?? ReflectionQuality.Medium;
                
                if (currentQuality == ReflectionQuality.Ultra)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.High);
                    Debug.Log("[性能优化] 降低反射质量: Ultra -> High");
                }
                else if (currentQuality == ReflectionQuality.High)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.Medium);
                    Debug.Log("[性能优化] 降低反射质量: High -> Medium");
                }
                else if (currentQuality == ReflectionQuality.Medium)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.Low);
                    Debug.Log("[性能优化] 降低反射质量: Medium -> Low");
                }
            }
            else if (fps > targetFrameRate * 1.2f) // 高于目标120%
            {
                // 提升质量
                var currentQuality = reflectionManager.settings?.currentQuality ?? ReflectionQuality.Medium;
                
                if (currentQuality == ReflectionQuality.Low)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.Medium);
                    Debug.Log("[性能优化] 提升反射质量: Low -> Medium");
                }
                else if (currentQuality == ReflectionQuality.Medium)
                {
                    reflectionManager.ApplyQualitySettings(ReflectionQuality.High);
                    Debug.Log("[性能优化] 提升反射质量: Medium -> High");
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

            // 背景
            GUI.Box(new Rect(x - 5, y - 5, width + 10, lineHeight * 8 + 10), "");

            // 标题
            GUI.Label(new Rect(x, y, width, lineHeight), "=== 反射性能监控 ===", labelStyle);
            y += lineHeight;

            // FPS
            Color fpsColor = fps >= targetFrameRate ? Color.green : (fps >= targetFrameRate * 0.8f ? Color.yellow : Color.red);
            GUI.contentColor = fpsColor;
            GUI.Label(new Rect(x, y, width, lineHeight), $"FPS: {fps:F1} (目标: {targetFrameRate})", labelStyle);
            GUI.contentColor = Color.white;
            y += lineHeight;

            // 帧时间
            float ms = deltaTime * 1000.0f;
            GUI.Label(new Rect(x, y, width, lineHeight), $"帧时间: {ms:F2} ms", labelStyle);
            y += lineHeight;

            // 反射状态
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

            // 自动优化状态
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
                currentFPS = fps,
                targetFPS = targetFrameRate,
                frameTime = deltaTime * 1000.0f,
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
            float targetRatio = fps / targetFrameRate;

            if (targetRatio >= 1.0f)
                return PerformanceLevel.Excellent;
            else if (targetRatio >= 0.9f)
                return PerformanceLevel.Good;
            else if (targetRatio >= 0.7f)
                return PerformanceLevel.Acceptable;
            else
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
                   $"帧时间: {frameTime:F2}ms\n" +
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
        Poor,       // 差
        Acceptable, // 可接受
        Good,       // 良好
        Excellent   // 优秀
    }
}
