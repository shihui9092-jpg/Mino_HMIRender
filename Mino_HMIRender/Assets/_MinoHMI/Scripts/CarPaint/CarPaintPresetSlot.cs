using System;
using UnityEngine;

namespace MinoHMI.CarPaint
{
    /// <summary>
    /// 车漆预设槽：引用一套已调好的材质球作为参数来源（不直接替换 Renderer 材质）。
    /// </summary>
    [Serializable]
    public class CarPaintPresetSlot
    {
        [Tooltip("UI 或日志显示名称")]
        public string displayName = "默认车漆";

        [Tooltip("使用 Mino/Unlit_CarPaint 制作的参考材质球")]
        public Material sourceMaterial;
    }
}
