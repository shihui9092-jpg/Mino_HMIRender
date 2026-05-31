using System;
using UnityEngine;

namespace MinoHMI.MY26HMI.MaterialToggleControl
{
    /// <summary>
    /// 材质切换槽位：单个可配置材质球及其显示名称。
    /// </summary>
    [Serializable]
    public class MaterialToggleSlot
    {
        [Tooltip("槽位材质球")]
        public Material material;

        [Tooltip("槽位显示名称（留空时使用材质球资源名）")]
        public string slotDisplayName;

        public bool HasMaterial => material != null;

        public string ResolveSlotDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(slotDisplayName))
            {
                return slotDisplayName.Trim();
            }

            if (material != null)
            {
                return material.name;
            }

            return "未命名材质球";
        }
    }
}
