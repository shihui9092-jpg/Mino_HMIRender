using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MinoHMI.MY26HMI.MaterialControl
{
    /// <summary>
    /// 材质参数应用与 Shader 一致性校验工具。
    /// 规则：不同类别可使用不同 Shader；同一类别内本体与所有变体须为相同 Shader。
    /// </summary>
    public static class MaterialVariantApplyUtility
    {
        private static readonly List<Material> categoryMaterialsScratch = new List<Material>();

        /// <summary>
        /// 将变体材质球的属性参数复制到本体材质球（要求两者 Shader 相同）。
        /// </summary>
        public static bool TryApplyVariantPropertiesToBody(
            Material bodyMaterial,
            Material variantMaterial,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            if (bodyMaterial == null)
            {
                errorMessage = "本体材质球未配置。";
                return false;
            }

            if (variantMaterial == null)
            {
                errorMessage = "变体材质球未配置。";
                return false;
            }

            if (bodyMaterial.shader != variantMaterial.shader)
            {
                errorMessage =
                    $"Shader 不一致：本体「{bodyMaterial.shader.name}」，变体「{variantMaterial.shader.name}」。";
                return false;
            }

            bodyMaterial.CopyPropertiesFromMaterial(variantMaterial);
            return true;
        }

        /// <summary>
        /// 校验单个材质种类组内本体与变体是否使用相同 Shader。
        /// </summary>
        public static bool ValidateCategoryShaderConsistency(
            MaterialCategoryEntry entry,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (entry == null)
            {
                errorMessage = "材质种类条目为空。";
                return false;
            }

            CollectCategoryMaterials(entry, categoryMaterialsScratch);
            if (categoryMaterialsScratch.Count <= 1)
            {
                return true;
            }

            if (!ValidateMaterialsSameShader(categoryMaterialsScratch, out string innerError))
            {
                errorMessage = $"[{entry.ResolveCategoryDisplayName()}] {innerError}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 校验目录内每个种类组内部的 Shader 一致性（组与组之间允许不同 Shader）。
        /// </summary>
        public static bool ValidateCatalogShaderConsistency(
            MaterialCategoryEntry[] entries,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (entries == null || entries.Length == 0)
            {
                return true;
            }

            StringBuilder mismatchBuilder = new StringBuilder();
            bool allCategoriesValid = true;

            for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
            {
                MaterialCategoryEntry entry = entries[entryIndex];
                if (entry == null)
                {
                    continue;
                }

                if (ValidateCategoryShaderConsistency(entry, out string categoryError))
                {
                    continue;
                }

                allCategoriesValid = false;
                mismatchBuilder.AppendLine(categoryError);
            }

            if (!allCategoriesValid)
            {
                errorMessage =
                    "以下种类组内 Shader 不一致（同一类别须相同，不同类别可使用不同 Shader）：\n" +
                    mismatchBuilder;
            }

            return allCategoriesValid;
        }

        private static void CollectCategoryMaterials(
            MaterialCategoryEntry entry,
            List<Material> destination)
        {
            destination.Clear();
            if (entry == null)
            {
                return;
            }

            if (entry.bodyMaterial != null)
            {
                destination.Add(entry.bodyMaterial);
            }

            if (entry.variantMaterialSlots == null)
            {
                return;
            }

            for (int index = 0; index < entry.variantMaterialSlots.Length; index++)
            {
                MaterialVariantSlot slot = entry.variantMaterialSlots[index];
                if (slot != null && slot.HasMaterial)
                {
                    destination.Add(slot.variantMaterial);
                }
            }
        }

        private static bool ValidateMaterialsSameShader(List<Material> materials, out string errorMessage)
        {
            errorMessage = string.Empty;
            Shader referenceShader = null;
            string referenceMaterialName = string.Empty;
            StringBuilder mismatchBuilder = new StringBuilder();

            for (int index = 0; index < materials.Count; index++)
            {
                Material material = materials[index];
                if (material == null)
                {
                    continue;
                }

                if (referenceShader == null)
                {
                    referenceShader = material.shader;
                    referenceMaterialName = material.name;
                    continue;
                }

                if (material.shader == referenceShader)
                {
                    continue;
                }

                mismatchBuilder.AppendLine(
                    $"「{material.name}」使用 {material.shader.name}，基准「{referenceMaterialName}」使用 {referenceShader.name}");
            }

            if (mismatchBuilder.Length > 0)
            {
                errorMessage = "同一类别内的材质球必须使用相同 Shader：\n" + mismatchBuilder;
                return false;
            }

            return true;
        }
    }
}
