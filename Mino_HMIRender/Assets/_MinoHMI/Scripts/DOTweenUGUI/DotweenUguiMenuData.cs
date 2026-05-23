using System;
using System.Collections.Generic;

namespace MinoHMI.DOTweenUGUI
{
    /// <summary>
    /// 子菜单按钮配置数据。
    /// </summary>
    [Serializable]
    public class DotweenUguiSubButtonData
    {
        public string buttonId = "SubButton";
        public string buttonName = "子按钮";
    }

    /// <summary>
    /// 主菜单配置数据，每个主菜单可配置一组子按钮。
    /// </summary>
    [Serializable]
    public class DotweenUguiMainMenuData
    {
        public string menuId = "MainMenu";
        public string menuName = "主菜单";
        public List<DotweenUguiSubButtonData> subButtons = new List<DotweenUguiSubButtonData>();
    }
}
