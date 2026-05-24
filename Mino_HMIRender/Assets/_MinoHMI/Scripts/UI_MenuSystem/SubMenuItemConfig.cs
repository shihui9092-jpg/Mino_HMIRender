using System;
using UnityEngine;

namespace MinoHMI.UI_MenuSystem
{
    /// <summary>
    /// 子菜单弹窗定位方式。
    /// </summary>
    public enum SubMenuPopupAnchorMode
    {
        /// <summary>相对触发的主菜单项 Rect + 偏移。</summary>
        RelativeToPrimaryItem = 0,

        /// <summary>使用 Profile 中的固定 anchoredPosition。</summary>
        FixedAnchoredPosition = 1
    }

    /// <summary>
    /// 单个子按钮槽位配置（与 Btn_Sub_X 一一对应）。
    /// </summary>
    [Serializable]
    public class SubMenuItemConfig
    {
        [Tooltip("是否显示该槽位")]
        public bool enabled = true;

        [Tooltip("子按钮逻辑 ID，用于事件回调")]
        public string buttonId = "Sub_0";

        [Tooltip("按钮显示文字")]
        public string labelText = "子菜单";

        [Tooltip("Legacy / TMP 文字来源")]
        public PrimaryLabelDisplayTarget labelDisplayTarget = PrimaryLabelDisplayTarget.PrimaryLabel;

        [Tooltip("Legacy Text 颜色")]
        public Color labelColor = Color.white;

        [Tooltip("TextMeshPro 颜色")]
        public Color tmpLabelColor = Color.white;

        [Tooltip("按钮图标")]
        public Sprite iconSprite;

        [Tooltip("按钮背景图（可选，覆盖 Btn_Sub 上 Image.sprite）")]
        public Sprite buttonBackgroundSprite;

        public bool showLabel = true;
        public bool showIcon = true;

        [Tooltip("按钮尺寸；为零则保持场景预设")]
        public Vector2 buttonSize = Vector2.zero;
    }

    /// <summary>
    /// 单个主菜单项对应的子菜单弹窗方案。
    /// </summary>
    [Serializable]
    public class PrimaryMenuProfile
    {
        [Tooltip("弹窗定位方式：默认固定坐标，不跟随主菜单按钮")]
        public SubMenuPopupAnchorMode anchorMode = SubMenuPopupAnchorMode.FixedAnchoredPosition;

        [Tooltip("FixedAnchoredPosition 模式：子菜单弹窗在父节点下的自定义 anchoredPosition")]
        public Vector2 popupAnchoredPosition = new Vector2(330f, 0f);

        [Tooltip("仅 RelativeToPrimaryItem 模式生效：相对主菜单项的偏移")]
        public Vector2 popupOffset = new Vector2(0f, -120f);

        [Tooltip("弹窗背景图（可选，未指定时使用纯白占位图）")]
        public Sprite popupBackground;

        [Tooltip("是否显示弹窗背景")]
        public bool showPopupBackground = true;

        [Tooltip("背景相对当前可见子按钮区域的边距（宽、高各方向扩展）")]
        public Vector2 popupBackgroundPadding = new Vector2(8f, 8f);

        [Tooltip("背景位置微调（X=左、Y=下、Z=右、W=上；正值向该方向平移，仅影响背景图）")]
        public Vector4 popupBackgroundPositionOffset = Vector4.zero;

        [Tooltip("是否显示箭头")]
        public bool showArrow = true;

        [Tooltip("箭头相对弹窗根的偏移")]
        public Vector2 arrowOffset = new Vector2(-20f, 40f);

        [Tooltip("与 Btn_Sub_0、Btn_Sub_1... 一一对应")]
        public SubMenuItemConfig[] subItems = Array.Empty<SubMenuItemConfig>();
    }
}
