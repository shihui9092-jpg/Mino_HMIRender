using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MinoHMI.MY26HMI.MaterialControl
{
    /// <summary>
    /// 材质变体目录：本体材质球凹槽驱动材质种类组，支持按类别平滑过渡变体参数。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MinoHMI/材质控制/材质变体目录")]
    [MovedFrom("MinoHMI.MY26HMI.TimeAndWeather.TimeAndWeatherMaterialCatalog")]
    public class MaterialVariantCatalog : MonoBehaviour
    {
        private sealed class MaterialTransitionState
        {
            public MaterialPropertySnapshot FromSnapshot;
            public MaterialPropertySnapshot ToSnapshot;
            public readonly MaterialPropertySnapshot BlendedSnapshot = new MaterialPropertySnapshot();
            public float Duration;
            public float Elapsed;
            public MaterialTransitionBlendMode BlendMode;
            public MaterialDiscretePropertySwitchTiming DiscretePropertySwitchTiming;
        }

        [Header("本体材质球凹槽")]
        [SerializeField]
        [Tooltip("点击 + 添加本体材质球，每添加一项即新增一个材质种类组")]
        private MaterialCategoryEntry[] materialCategoryEntries = Array.Empty<MaterialCategoryEntry>();

        private readonly Dictionary<Material, MaterialTransitionState> activeTransitions = new Dictionary<Material, MaterialTransitionState>();
        private readonly List<Material> completedMaterialsScratch = new List<Material>();

#if UNITY_EDITOR
        private static readonly Dictionary<int, MaterialVariantCatalog> EditorTickCatalogs =
            new Dictionary<int, MaterialVariantCatalog>();

        private static readonly List<MaterialVariantCatalog> editorCatalogTickScratch =
            new List<MaterialVariantCatalog>();

        private static bool editorUpdateRegistered;
#endif

        public int MaterialCategoryEntryCount => materialCategoryEntries?.Length ?? 0;

        public bool IsAnyTransitionActive => activeTransitions.Count > 0;

        public bool TryGetCategoryEntry(int entryIndex, out MaterialCategoryEntry entry)
        {
            if (materialCategoryEntries != null && entryIndex >= 0 && entryIndex < materialCategoryEntries.Length)
            {
                entry = materialCategoryEntries[entryIndex];
                return entry != null;
            }

            entry = null;
            return false;
        }

        public MaterialCategoryEntry GetCategoryEntry(int entryIndex)
        {
            return TryGetCategoryEntry(entryIndex, out MaterialCategoryEntry entry) ? entry : null;
        }

        public int GetGlobalVariantSlotCount()
        {
            if (materialCategoryEntries == null || materialCategoryEntries.Length == 0)
            {
                return 0;
            }

            int maxAssignedVariantSlotCount = 0;
            for (int index = 0; index < materialCategoryEntries.Length; index++)
            {
                MaterialCategoryEntry entry = materialCategoryEntries[index];
                if (entry == null)
                {
                    continue;
                }

                maxAssignedVariantSlotCount = Math.Max(
                    maxAssignedVariantSlotCount,
                    entry.GetAssignedVariantSlotCount());
            }

            return maxAssignedVariantSlotCount;
        }

        /// <summary>
        /// 将指定变体索引的参数赋予所有种类组本体（支持按组平滑过渡）。
        /// </summary>
        public bool TryApplyVariantToAllCategories(int variantIndex, out string resultMessage)
        {
            return TryApplyVariantToAllCategories(variantIndex, useSmoothTransition: true, out resultMessage);
        }

        /// <summary>
        /// 将指定变体索引的参数赋予所有种类组本体。
        /// </summary>
        public bool TryApplyVariantToAllCategories(
            int variantIndex,
            bool useSmoothTransition,
            out string resultMessage)
        {
            resultMessage = string.Empty;
            if (materialCategoryEntries == null || materialCategoryEntries.Length == 0)
            {
                resultMessage = "未配置任何材质种类组。";
                return false;
            }

            int startedCount = 0;
            int instantCount = 0;
            int skipCount = 0;
            StringBuilder failureBuilder = new StringBuilder();

            for (int entryIndex = 0; entryIndex < materialCategoryEntries.Length; entryIndex++)
            {
                if (!TryGetCategoryEntry(entryIndex, out MaterialCategoryEntry entry))
                {
                    skipCount++;
                    continue;
                }

                if (!MaterialVariantApplyUtility.ValidateCategoryShaderConsistency(entry, out string categoryShaderError))
                {
                    skipCount++;
                    failureBuilder.AppendLine(categoryShaderError);
                    continue;
                }

                if (!TryBeginApplyVariantToBody(entry, variantIndex, useSmoothTransition, out string entryError))
                {
                    skipCount++;
                    if (!string.IsNullOrWhiteSpace(entryError))
                    {
                        failureBuilder.AppendLine($"[{entry.ResolveCategoryDisplayName()}] {entryError}");
                    }

                    continue;
                }

                if (useSmoothTransition
                    && entry.TryGetVariantTransitionDuration(variantIndex, out float transitionDuration)
                    && transitionDuration > 0f)
                {
                    startedCount++;
                }
                else
                {
                    instantCount++;
                }
            }

            int successCount = startedCount + instantCount;
            bool hasPartialFailure = failureBuilder.Length > 0;
            if (successCount <= 0)
            {
                resultMessage = failureBuilder.Length > 0
                    ? "未能应用到任何种类组：\n" + failureBuilder
                    : $"变体索引 {variantIndex} 在所有种类组中均不可用。";
                return false;
            }

            if (startedCount > 0)
            {
                resultMessage = $"已开始 {startedCount} 个种类组的平滑过渡";
                if (instantCount > 0)
                {
                    resultMessage += $"，{instantCount} 个种类组已立即应用";
                }

                resultMessage += "。";
            }
            else
            {
                resultMessage = $"已将变体索引 {variantIndex} 的参数立即赋予 {instantCount} 个种类组的本体材质球。";
            }

            if (skipCount > 0)
            {
                resultMessage += $"（跳过 {skipCount} 个组）";
            }

            if (hasPartialFailure)
            {
                resultMessage = "部分成功：" + resultMessage + "\n部分失败：\n" + failureBuilder;
            }

            RegisterEditorUpdateIfNeeded();
            // 只要存在成功应用（立即或平滑）即返回 true，避免“部分成功”被误判为失败
            return successCount > 0;
        }

        public bool TryApplyVariantPropertiesToBody(int entryIndex, int variantIndex, out string errorMessage)
        {
            return TryApplyVariantPropertiesToBody(entryIndex, variantIndex, useSmoothTransition: true, out errorMessage);
        }

        public bool TryApplyVariantPropertiesToBody(
            int entryIndex,
            int variantIndex,
            bool useSmoothTransition,
            out string errorMessage)
        {
            if (!TryGetCategoryEntry(entryIndex, out MaterialCategoryEntry entry))
            {
                errorMessage = $"材质种类索引 {entryIndex} 无效。";
                return false;
            }

            return TryBeginApplyVariantToBody(entry, variantIndex, useSmoothTransition, out errorMessage);
        }

        public bool ValidateCatalogShaderConsistency(out string errorMessage)
        {
            return MaterialVariantApplyUtility.ValidateCatalogShaderConsistency(
                materialCategoryEntries,
                out errorMessage);
        }

        public int FindCategoryEntryIndexByDisplayName(string categoryDisplayName)
        {
            if (string.IsNullOrWhiteSpace(categoryDisplayName) || materialCategoryEntries == null)
            {
                return -1;
            }

            for (int index = 0; index < materialCategoryEntries.Length; index++)
            {
                MaterialCategoryEntry entry = materialCategoryEntries[index];
                if (entry == null)
                {
                    continue;
                }

                if (string.Equals(entry.ResolveCategoryDisplayName(), categoryDisplayName, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private bool TryBeginApplyVariantToBody(
            MaterialCategoryEntry entry,
            int variantIndex,
            bool useSmoothTransition,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (entry == null)
            {
                errorMessage = "材质种类条目为空。";
                return false;
            }

            if (!entry.TryGetVariantMaterial(variantIndex, out Material variantMaterial))
            {
                errorMessage = $"变体索引 {variantIndex} 无效或未配置材质球。";
                return false;
            }

            if (entry.bodyMaterial == null)
            {
                errorMessage = "本体材质球未配置。";
                return false;
            }

            if (entry.bodyMaterial.shader != variantMaterial.shader)
            {
                errorMessage =
                    $"Shader 不一致：本体「{entry.bodyMaterial.shader.name}」，变体「{variantMaterial.shader.name}」。";
                return false;
            }

            float duration = 0f;
            if (useSmoothTransition)
            {
                entry.TryGetVariantTransitionDuration(variantIndex, out duration);
            }
            if (duration <= 0f)
            {
                return MaterialVariantApplyUtility.TryApplyVariantPropertiesToBody(
                    entry.bodyMaterial,
                    variantMaterial,
                    out errorMessage);
            }

            entry.TryGetVariantTransitionOptions(
                variantIndex,
                out MaterialTransitionBlendMode blendMode,
                out MaterialDiscretePropertySwitchTiming discretePropertySwitchTiming);

            BeginSmoothTransition(
                entry.bodyMaterial,
                variantMaterial,
                duration,
                blendMode,
                discretePropertySwitchTiming);
            return true;
        }

        private void BeginSmoothTransition(
            Material bodyMaterial,
            Material variantMaterial,
            float duration,
            MaterialTransitionBlendMode blendMode,
            MaterialDiscretePropertySwitchTiming discretePropertySwitchTiming)
        {
            if (bodyMaterial == null || variantMaterial == null || duration <= 0f)
            {
                return;
            }

            MaterialTransitionState transitionState = new MaterialTransitionState
            {
                FromSnapshot = MaterialPropertySnapshot.FromMaterial(bodyMaterial),
                ToSnapshot = MaterialPropertySnapshot.FromMaterial(variantMaterial),
                Duration = duration,
                Elapsed = 0f,
                BlendMode = blendMode,
                DiscretePropertySwitchTiming = discretePropertySwitchTiming
            };

            activeTransitions[bodyMaterial] = transitionState;
            ApplyTransitionSample(bodyMaterial, transitionState, 0f);
            RegisterEditorUpdateIfNeeded();
        }

        private void Update()
        {
            if (activeTransitions.Count > 0)
            {
                TickTransitions(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            UnregisterEditorUpdate();
        }

        private void OnDisable()
        {
            activeTransitions.Clear();
            UnregisterEditorUpdate();
        }

        private void TickTransitions(float deltaTime)
        {
            if (activeTransitions.Count == 0)
            {
                UnregisterEditorUpdate();
                return;
            }

            completedMaterialsScratch.Clear();
            foreach (KeyValuePair<Material, MaterialTransitionState> pair in activeTransitions)
            {
                Material bodyMaterial = pair.Key;
                MaterialTransitionState transitionState = pair.Value;
                if (bodyMaterial == null)
                {
                    completedMaterialsScratch.Add(pair.Key);
                    continue;
                }

                transitionState.Elapsed += Mathf.Max(0f, deltaTime);
                float normalizedTime = transitionState.Elapsed / Mathf.Max(transitionState.Duration, 0.0001f);
                ApplyTransitionSample(bodyMaterial, transitionState, normalizedTime);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(bodyMaterial);
                }
#endif

                if (normalizedTime >= 1f)
                {
                    completedMaterialsScratch.Add(bodyMaterial);
                }
            }

            if (completedMaterialsScratch.Count == 0)
            {
                return;
            }

            for (int index = 0; index < completedMaterialsScratch.Count; index++)
            {
                activeTransitions.Remove(completedMaterialsScratch[index]);
            }

            if (activeTransitions.Count == 0)
            {
                UnregisterEditorUpdate();
            }
        }

        private static void ApplyTransitionSample(
            Material bodyMaterial,
            MaterialTransitionState transitionState,
            float normalizedTime)
        {
            float evaluatedBlend = EvaluateBlend(normalizedTime, transitionState.BlendMode);
            transitionState.BlendedSnapshot.LerpInto(
                transitionState.FromSnapshot,
                transitionState.ToSnapshot,
                evaluatedBlend,
                transitionState.DiscretePropertySwitchTiming);
            transitionState.BlendedSnapshot.ApplyTo(bodyMaterial);
        }

        /// <summary>
        /// 根据配置计算插值曲线，提供可选过渡方式。
        /// </summary>
        private static float EvaluateBlend(float normalizedTime, MaterialTransitionBlendMode blendMode)
        {
            float clampedTime = Mathf.Clamp01(normalizedTime);
            switch (blendMode)
            {
                case MaterialTransitionBlendMode.Linear:
                    return clampedTime;
                case MaterialTransitionBlendMode.SmoothStep:
                default:
                    return clampedTime * clampedTime * (3f - 2f * clampedTime);
            }
        }

        private void RegisterEditorUpdateIfNeeded()
        {
#if UNITY_EDITOR
            if (Application.isPlaying || activeTransitions.Count == 0)
            {
                return;
            }

            int instanceId = GetInstanceID();
            EditorTickCatalogs[instanceId] = this;
            if (editorUpdateRegistered)
            {
                return;
            }

            EditorApplication.update += OnEditorUpdate;
            editorUpdateRegistered = true;
#endif
        }

        private void UnregisterEditorUpdate()
        {
#if UNITY_EDITOR
            int instanceId = GetInstanceID();
            EditorTickCatalogs.Remove(instanceId);
            if (EditorTickCatalogs.Count > 0)
            {
                return;
            }

            if (!editorUpdateRegistered)
            {
                return;
            }

            EditorApplication.update -= OnEditorUpdate;
            editorUpdateRegistered = false;
#endif
        }

#if UNITY_EDITOR
        private static void OnEditorUpdate()
        {
            if (EditorTickCatalogs.Count == 0)
            {
                return;
            }

            float deltaTime = Mathf.Max(0.0001f, Time.deltaTime);
            editorCatalogTickScratch.Clear();
            editorCatalogTickScratch.AddRange(EditorTickCatalogs.Values);
            for (int index = 0; index < editorCatalogTickScratch.Count; index++)
            {
                MaterialVariantCatalog catalog = editorCatalogTickScratch[index];
                if (catalog == null)
                {
                    continue;
                }

                catalog.TickTransitions(deltaTime);
            }
        }

        public void SyncAllCategoryNamesFromBodyMaterials()
        {
            if (materialCategoryEntries == null)
            {
                return;
            }

            for (int index = 0; index < materialCategoryEntries.Length; index++)
            {
                MaterialCategoryEntry entry = materialCategoryEntries[index];
                if (entry == null || entry.bodyMaterial == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.categoryDisplayName))
                {
                    entry.SyncCategoryDisplayNameFromBodyMaterial();
                }
            }
        }
#endif
    }
}
