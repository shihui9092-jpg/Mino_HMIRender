using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MinoHMI.MY26HMI.MaterialControl
{
    /// <summary>
    /// 平滑过渡插值方式。
    /// </summary>
    public enum MaterialTransitionBlendMode
    {
        Linear = 0,
        SmoothStep = 1
    }

    /// <summary>
    /// 离散属性（贴图/关键词/整型）切换时机。
    /// </summary>
    public enum MaterialDiscretePropertySwitchTiming
    {
        AtStart = 0,
        AtEnd = 1
    }

    /// <summary>
    /// 单套变体材质球配置：材质引用 + 该变体专属的平滑过渡时间。
    /// </summary>
    [Serializable]
    [MovedFrom("MinoHMI.MY26HMI.TimeAndWeather.TimeWeatherVariantMaterialSlot")]
    public class MaterialVariantSlot
    {
        [Tooltip("变体材质球")]
        public Material variantMaterial;

        [Tooltip("该变体应用到本体时的平滑过渡时间（秒），0 表示立即应用")]
        [Min(0f)]
        public float transitionDuration = 0.5f;

        [Tooltip("平滑过渡插值方式：Linear 线性；SmoothStep 更柔和，能减少突变闪烁感")]
        public MaterialTransitionBlendMode blendMode = MaterialTransitionBlendMode.SmoothStep;

        [Tooltip("贴图/关键词/整型等离散属性切换时机：AtStart 立即切换；AtEnd 结束时切换")]
        public MaterialDiscretePropertySwitchTiming discretePropertySwitchTiming = MaterialDiscretePropertySwitchTiming.AtStart;

        public bool HasMaterial => variantMaterial != null;
    }
}
