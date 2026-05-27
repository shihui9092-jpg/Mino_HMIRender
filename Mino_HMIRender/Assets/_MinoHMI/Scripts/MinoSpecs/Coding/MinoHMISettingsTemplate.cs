using UnityEngine;

namespace MinoHMI.Templates
{
    /// <summary>
    /// MinoHMI ScriptableObject 配置模板
    /// 配合 MinoHMIComponentTemplate 使用，复制后按模块重命名。
    /// </summary>
    [CreateAssetMenu(fileName = "MinoHMISettingsTemplate", menuName = "MinoHMI/模板/组件配置模板")]
    public class MinoHMISettingsTemplate : ScriptableObject
    {
        [Header("默认参数")]
        [Tooltip("默认效果强度")]
        [Range(0f, 1f)]
        public float defaultIntensity = 0.5f;

        [Tooltip("默认更新间隔(帧)")]
        [Range(1, 10)]
        public int defaultUpdateInterval = 1;

        [Header("质量预设")]
        public TemplateQualitySettings lowQuality = TemplateQualitySettings.CreateLow();
        public TemplateQualitySettings mediumQuality = TemplateQualitySettings.CreateMedium();
        public TemplateQualitySettings highQuality = TemplateQualitySettings.CreateHigh();

        /// <summary>
        /// 按质量等级获取配置
        /// </summary>
        public TemplateQualitySettings GetQualitySettings(TemplateQuality quality)
        {
            switch (quality)
            {
                case TemplateQuality.Low:
                    return lowQuality;
                case TemplateQuality.Medium:
                    return mediumQuality;
                case TemplateQuality.High:
                    return highQuality;
                default:
                    return mediumQuality;
            }
        }
    }

    /// <summary>
    /// 模板质量参数结构
    /// </summary>
    [System.Serializable]
    public struct TemplateQualitySettings
    {
        [Tooltip("效果强度")]
        [Range(0f, 1f)]
        public float intensity;

        [Tooltip("更新间隔(帧)")]
        [Range(1, 10)]
        public int updateInterval;

        public static TemplateQualitySettings CreateLow()
        {
            return new TemplateQualitySettings { intensity = 0.3f, updateInterval = 3 };
        }

        public static TemplateQualitySettings CreateMedium()
        {
            return new TemplateQualitySettings { intensity = 0.5f, updateInterval = 2 };
        }

        public static TemplateQualitySettings CreateHigh()
        {
            return new TemplateQualitySettings { intensity = 0.8f, updateInterval = 1 };
        }
    }
}
