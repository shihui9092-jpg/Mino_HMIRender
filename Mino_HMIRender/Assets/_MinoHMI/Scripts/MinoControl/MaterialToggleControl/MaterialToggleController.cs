using System;
using System.Collections.Generic;
using UnityEngine;

namespace MinoHMI.MY26HMI.MaterialToggleControl
{
    /// <summary>
    /// 材质切换控制器：通过材质球凹槽管理多套可切换材质，并用统一开关参数名批量驱动相同 Shader 属性。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MinoHMI/材质切换控制/材质切换控制器")]
    public class MaterialToggleController : MonoBehaviour
    {
        [Header("统一开关参数")]
        [SerializeField]
        [Tooltip("可在 Inspector 中点击 + 添加；各槽位材质球中共用的 Shader 开关属性名（如 _EnableFeature）")]
        private string[] sharedSwitchParameterNames = Array.Empty<string>();

        [Header("材质球凹槽")]
        [SerializeField]
        [Tooltip("可在 Inspector 中点击 + 添加；每项配置一个可切换的材质球")]
        private MaterialToggleSlot[] materialToggleSlots = Array.Empty<MaterialToggleSlot>();

        private bool[] sharedSwitchParameterStates = Array.Empty<bool>();

        private void Awake()
        {
            EnsureSharedSwitchParameterStateCapacity();
            ResetAllSharedSwitchParametersToOff(applyToMaterials: true);
        }

        private void OnValidate()
        {
            EnsureSharedSwitchParameterStateCapacity();
        }

        public bool HasSharedSwitchParameterNames => GetAssignedSharedSwitchParameterNameCount() > 0;

        public int MaterialToggleSlotCount => materialToggleSlots?.Length ?? 0;

        public int SharedSwitchParameterNameCount => sharedSwitchParameterNames?.Length ?? 0;

        /// <summary>
        /// 已填写内容的开关参数名数量（忽略尾部空项）。
        /// </summary>
        public int GetAssignedSharedSwitchParameterNameCount()
        {
            if (sharedSwitchParameterNames == null || sharedSwitchParameterNames.Length == 0)
            {
                return 0;
            }

            int assignedCount = 0;
            for (int index = 0; index < sharedSwitchParameterNames.Length; index++)
            {
                if (!string.IsNullOrWhiteSpace(sharedSwitchParameterNames[index]))
                {
                    assignedCount++;
                }
            }

            return assignedCount;
        }

        public bool TryGetSharedSwitchParameterName(int parameterIndex, out string parameterName)
        {
            if (sharedSwitchParameterNames != null
                && parameterIndex >= 0
                && parameterIndex < sharedSwitchParameterNames.Length
                && !string.IsNullOrWhiteSpace(sharedSwitchParameterNames[parameterIndex]))
            {
                parameterName = sharedSwitchParameterNames[parameterIndex].Trim();
                return true;
            }

            parameterName = string.Empty;
            return false;
        }

        public bool IsSharedSwitchParameterEnabled(int parameterIndex)
        {
            EnsureSharedSwitchParameterStateCapacity();
            return parameterIndex >= 0
                && parameterIndex < sharedSwitchParameterStates.Length
                && sharedSwitchParameterStates[parameterIndex];
        }

        /// <summary>
        /// 切换指定索引的开关参数：关 → 开，开 → 关。
        /// </summary>
        public bool TryToggleSharedSwitchParameter(int parameterIndex, out string errorMessage)
        {
            EnsureSharedSwitchParameterStateCapacity();
            if (parameterIndex < 0 || parameterIndex >= sharedSwitchParameterStates.Length)
            {
                errorMessage = $"开关参数索引 {parameterIndex} 无效。";
                return false;
            }

            bool targetEnabled = !sharedSwitchParameterStates[parameterIndex];
            if (!TrySetSharedSwitchParameter(parameterIndex, targetEnabled, out errorMessage))
            {
                return false;
            }

            sharedSwitchParameterStates[parameterIndex] = targetEnabled;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// 运行时将全部开关参数重置为关闭，并可选同步写入材质。
        /// </summary>
        public void ResetAllSharedSwitchParametersToOff(bool applyToMaterials)
        {
            EnsureSharedSwitchParameterStateCapacity();
            for (int index = 0; index < sharedSwitchParameterStates.Length; index++)
            {
                sharedSwitchParameterStates[index] = false;
            }

            if (!applyToMaterials)
            {
                return;
            }

            for (int index = 0; index < SharedSwitchParameterNameCount; index++)
            {
                if (!TryGetSharedSwitchParameterName(index, out string parameterName))
                {
                    continue;
                }

                TryApplySwitchParameterToAllMaterials(parameterName, false, out _);
            }
        }

        public bool TryResolveSwitchParameterButtonState(
            int parameterIndex,
            out string buttonLabel,
            out bool canToggle,
            out string disableReason)
        {
            buttonLabel = $"开关 {parameterIndex + 1}";
            canToggle = false;
            disableReason = string.Empty;

            if (!TryGetSharedSwitchParameterName(parameterIndex, out string parameterName))
            {
                disableReason = $"开关参数索引 {parameterIndex + 1} 未配置属性名。";
                return false;
            }

            string stateText = IsSharedSwitchParameterEnabled(parameterIndex) ? "开" : "关";
            buttonLabel = $"开关：{parameterName}（{stateText}）";

            if (materialToggleSlots == null || materialToggleSlots.Length == 0)
            {
                disableReason = "材质球凹槽为空，无法切换开关。";
                return false;
            }

            int propertyId = Shader.PropertyToID(parameterName);
            bool hasAnyMaterial = false;

            for (int slotIndex = 0; slotIndex < materialToggleSlots.Length; slotIndex++)
            {
                MaterialToggleSlot slot = materialToggleSlots[slotIndex];
                if (slot == null || !slot.HasMaterial)
                {
                    continue;
                }

                hasAnyMaterial = true;
                if (!slot.material.HasProperty(propertyId))
                {
                    disableReason =
                        $"材质「{slot.ResolveSlotDisplayName()}」不存在开关属性「{parameterName}」。";
                    return false;
                }
            }

            if (!hasAnyMaterial)
            {
                disableReason = "材质球凹槽中未配置有效材质。";
                return false;
            }

            canToggle = true;
            return true;
        }

        public bool TryCollectConfiguredMaterials(List<Material> outputMaterials)
        {
            outputMaterials?.Clear();

            if (outputMaterials == null || materialToggleSlots == null || materialToggleSlots.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < materialToggleSlots.Length; index++)
            {
                MaterialToggleSlot slot = materialToggleSlots[index];
                if (slot == null || !slot.HasMaterial)
                {
                    continue;
                }

                if (!outputMaterials.Contains(slot.material))
                {
                    outputMaterials.Add(slot.material);
                }
            }

            return outputMaterials.Count > 0;
        }

        /// <summary>
        /// 对全部凹槽材质球写入指定索引的开关参数（Float 属性，0 关 / 1 开）。
        /// </summary>
        public bool TrySetSharedSwitchParameter(int parameterIndex, bool enabled, out string errorMessage)
        {
            if (!TryGetSharedSwitchParameterName(parameterIndex, out string parameterName))
            {
                errorMessage = $"开关参数索引 {parameterIndex} 无效或未配置。";
                return false;
            }

            if (!TryApplySwitchParameterToAllMaterials(parameterName, enabled, out errorMessage))
            {
                return false;
            }

            EnsureSharedSwitchParameterStateCapacity();
            if (parameterIndex >= 0 && parameterIndex < sharedSwitchParameterStates.Length)
            {
                sharedSwitchParameterStates[parameterIndex] = enabled;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// 对全部凹槽材质球写入所有已配置的开关参数（Float 属性，0 关 / 1 开）。
        /// </summary>
        public bool TrySetAllSharedSwitchParameters(bool enabled, out string errorMessage)
        {
            if (sharedSwitchParameterNames == null || sharedSwitchParameterNames.Length == 0)
            {
                errorMessage = "未配置统一开关参数名。";
                return false;
            }

            bool hasAppliedAny = false;
            EnsureSharedSwitchParameterStateCapacity();
            for (int index = 0; index < sharedSwitchParameterNames.Length; index++)
            {
                if (!TryGetSharedSwitchParameterName(index, out string parameterName))
                {
                    continue;
                }

                if (!TryApplySwitchParameterToAllMaterials(parameterName, enabled, out errorMessage))
                {
                    return false;
                }

                if (index < sharedSwitchParameterStates.Length)
                {
                    sharedSwitchParameterStates[index] = enabled;
                }

                hasAppliedAny = true;
            }

            if (!hasAppliedAny)
            {
                errorMessage = "未配置有效的统一开关参数名。";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public bool TryGetMaterialToggleSlot(int index, out MaterialToggleSlot slot)
        {
            if (materialToggleSlots != null && index >= 0 && index < materialToggleSlots.Length)
            {
                slot = materialToggleSlots[index];
                return slot != null;
            }

            slot = null;
            return false;
        }

        public MaterialToggleSlot GetMaterialToggleSlot(int index)
        {
            return TryGetMaterialToggleSlot(index, out MaterialToggleSlot slot) ? slot : null;
        }

        private void EnsureSharedSwitchParameterStateCapacity()
        {
            int targetLength = sharedSwitchParameterNames?.Length ?? 0;
            if (sharedSwitchParameterStates != null && sharedSwitchParameterStates.Length == targetLength)
            {
                return;
            }

            bool[] resizedStates = targetLength > 0 ? new bool[targetLength] : Array.Empty<bool>();
            if (sharedSwitchParameterStates != null)
            {
                int copyLength = Math.Min(sharedSwitchParameterStates.Length, resizedStates.Length);
                for (int index = 0; index < copyLength; index++)
                {
                    resizedStates[index] = sharedSwitchParameterStates[index];
                }
            }

            sharedSwitchParameterStates = resizedStates;
        }

        private bool TryApplySwitchParameterToAllMaterials(
            string parameterName,
            bool enabled,
            out string errorMessage)
        {
            if (materialToggleSlots == null || materialToggleSlots.Length == 0)
            {
                errorMessage = "材质球凹槽为空，无法写入开关参数。";
                return false;
            }

            int propertyId = Shader.PropertyToID(parameterName);
            float switchValue = enabled ? 1f : 0f;
            int appliedCount = 0;

            for (int index = 0; index < materialToggleSlots.Length; index++)
            {
                MaterialToggleSlot slot = materialToggleSlots[index];
                if (slot == null || !slot.HasMaterial)
                {
                    continue;
                }

                Material material = slot.material;
                if (!material.HasProperty(propertyId))
                {
                    errorMessage =
                        $"材质「{slot.ResolveSlotDisplayName()}」不存在开关属性「{parameterName}」。";
                    return false;
                }

                material.SetFloat(propertyId, switchValue);
                appliedCount++;
            }

            if (appliedCount == 0)
            {
                errorMessage = "材质球凹槽中未找到可写入的有效材质。";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
