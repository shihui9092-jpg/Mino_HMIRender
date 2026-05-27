using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MinoHMI.MY26HMI.MaterialControl
{
    /// <summary>
    /// 材质种类条目：本体材质球 + 材质球命名 + 变体槽位列表（每套变体含独立过渡时间）。
    /// </summary>
    [Serializable]
    [MovedFrom("MinoHMI.MY26HMI.TimeAndWeather.TimeWeatherMaterialCategoryEntry")]
    public class MaterialCategoryEntry
    {
        [Tooltip("本体材质球，每添加一项即新增一个材质种类组")]
        public Material bodyMaterial;

        [Tooltip("材质球命名，材质种类组标题以此显示（留空时使用本体材质球名称）")]
        public string categoryDisplayName;

        [Tooltip("变体槽位列表，每项包含材质球与该变体的平滑过渡时间")]
        public MaterialVariantSlot[] variantMaterialSlots = Array.Empty<MaterialVariantSlot>();

        public bool HasBodyMaterial => bodyMaterial != null;

        public int VariantMaterialCount => variantMaterialSlots?.Length ?? 0;

        /// <summary>
        /// 已赋值变体的最大索引 + 1（忽略尾部空槽位，用于全局按钮数量）。
        /// </summary>
        public int GetAssignedVariantSlotCount()
        {
            if (variantMaterialSlots == null || variantMaterialSlots.Length == 0)
            {
                return 0;
            }

            int maxAssignedIndex = -1;
            for (int index = 0; index < variantMaterialSlots.Length; index++)
            {
                if (variantMaterialSlots[index] != null && variantMaterialSlots[index].HasMaterial)
                {
                    maxAssignedIndex = index;
                }
            }

            return maxAssignedIndex + 1;
        }

        public string ResolveCategoryDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(categoryDisplayName))
            {
                return categoryDisplayName.Trim();
            }

            if (bodyMaterial != null)
            {
                return bodyMaterial.name;
            }

            return "未命名材质球";
        }

        public void SyncCategoryDisplayNameFromBodyMaterial()
        {
            if (bodyMaterial == null)
            {
                return;
            }

            categoryDisplayName = bodyMaterial.name;
        }

        public bool TryGetVariantSlot(int variantIndex, out MaterialVariantSlot variantSlot)
        {
            if (variantMaterialSlots != null
                && variantIndex >= 0
                && variantIndex < variantMaterialSlots.Length
                && variantMaterialSlots[variantIndex] != null)
            {
                variantSlot = variantMaterialSlots[variantIndex];
                return true;
            }

            variantSlot = null;
            return false;
        }

        public bool TryGetVariantMaterial(int variantIndex, out Material material)
        {
            if (TryGetVariantSlot(variantIndex, out MaterialVariantSlot variantSlot)
                && variantSlot.HasMaterial)
            {
                material = variantSlot.variantMaterial;
                return true;
            }

            material = null;
            return false;
        }

        public bool TryGetVariantTransitionDuration(int variantIndex, out float transitionDuration)
        {
            if (TryGetVariantSlot(variantIndex, out MaterialVariantSlot variantSlot))
            {
                transitionDuration = Mathf.Max(0f, variantSlot.transitionDuration);
                return true;
            }

            transitionDuration = 0f;
            return false;
        }

        public bool TryGetVariantTransitionOptions(
            int variantIndex,
            out MaterialTransitionBlendMode blendMode,
            out MaterialDiscretePropertySwitchTiming discretePropertySwitchTiming)
        {
            if (TryGetVariantSlot(variantIndex, out MaterialVariantSlot variantSlot))
            {
                blendMode = variantSlot.blendMode;
                discretePropertySwitchTiming = variantSlot.discretePropertySwitchTiming;
                return true;
            }

            blendMode = MaterialTransitionBlendMode.SmoothStep;
            discretePropertySwitchTiming = MaterialDiscretePropertySwitchTiming.AtStart;
            return false;
        }

        public bool TryApplyVariantPropertiesToBody(int variantIndex, out string errorMessage)
        {
            if (!TryGetVariantMaterial(variantIndex, out Material variantMaterial))
            {
                errorMessage = $"变体索引 {variantIndex} 无效或未配置材质球。";
                return false;
            }

            return MaterialVariantApplyUtility.TryApplyVariantPropertiesToBody(
                bodyMaterial,
                variantMaterial,
                out errorMessage);
        }
    }
}
