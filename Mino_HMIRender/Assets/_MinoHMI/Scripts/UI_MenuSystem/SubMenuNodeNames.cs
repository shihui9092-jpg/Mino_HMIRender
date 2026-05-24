using System;
using System.Text.RegularExpressions;

namespace MinoHMI.UI_MenuSystem
{
    /// <summary>
    /// UI_MenuSystem 子菜单标准节点命名。
    /// </summary>
    public static class SubMenuNodeNames
    {
        public const string SubMenuPopup = "SubMenu_Popup";
        public const string SubMenuBackground = "Img_SubMenuBg";
        public const string SubMenuList = "SubMenu_List";
        public const string SubMenuArrow = "Img_Arrow";
        public const string SubMenuArrowAlias = "Img_SubMenuArrow";
        public const string SubButtonPrefix = "Btn_Sub_";

        public const string SubLabel = "Txt_SubLabel";
        public const string SubTmpLabel = "TxtMeshPro_SubLabel";
        public const string SubTmpLabelAlias = "Txt_SubTmpLabel";
        public const string SubIcon = "Img_SubIcon";

        private static readonly Regex SubButtonIndexPattern = new Regex(
            @"^Btn[_\s-]*Sub[_\s-]*(\d+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// 从子按钮节点名解析索引，兼容 Btn_Sub_0、Btn_Sub 0 等写法。
        /// </summary>
        public static bool TryParseSubButtonIndex(string name, out int index)
        {
            index = -1;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            string normalizedName = MenuSystemHierarchyUtility.NormalizeNodeName(name);
            Match match = SubButtonIndexPattern.Match(normalizedName);
            if (!match.Success)
            {
                return false;
            }

            return int.TryParse(match.Groups[1].Value, out index);
        }
    }
}
