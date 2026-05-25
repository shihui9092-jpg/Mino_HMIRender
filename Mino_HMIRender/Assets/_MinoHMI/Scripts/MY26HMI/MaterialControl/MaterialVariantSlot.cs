using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MinoHMI.MY26HMI.MaterialControl
{
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

        public bool HasMaterial => variantMaterial != null;
    }
}
