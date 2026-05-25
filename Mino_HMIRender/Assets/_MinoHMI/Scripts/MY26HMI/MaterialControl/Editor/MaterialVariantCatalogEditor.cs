#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MinoHMI.MY26HMI.MaterialControl
{
    /// <summary>
    /// 材质类别目录 Inspector：全局变体按钮统一控制所有种类组的参数赋值。
    /// </summary>
    [CustomEditor(typeof(MaterialVariantCatalog))]
    public class MaterialVariantCatalogEditor : UnityEditor.Editor
    {
        private sealed class InspectorCategoryRow
        {
            public MaterialCategoryEntry Entry;
            public bool ShaderConsistent;
            public string ShaderError;
        }

        private SerializedProperty materialCategoryEntriesProperty;
        private string resultMessage = string.Empty;
        private MessageType resultMessageType = MessageType.None;
        private GUIStyle leftAlignedButtonStyle;
        private readonly List<InspectorCategoryRow> inspectorCategoryRows = new List<InspectorCategoryRow>();
        private readonly HashSet<Material> bodyMaterialSetScratch = new HashSet<Material>();
        private readonly List<Material> bodyMaterialsScratch = new List<Material>();

        private GUIStyle LeftAlignedButtonStyle
        {
            get
            {
                if (leftAlignedButtonStyle == null)
                {
                    leftAlignedButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleLeft
                    };
                    leftAlignedButtonStyle.padding = new RectOffset(8, 8, 4, 4);
                }

                return leftAlignedButtonStyle;
            }
        }

        private void OnEnable()
        {
            materialCategoryEntriesProperty = serializedObject.FindProperty("materialCategoryEntries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MaterialVariantCatalog catalog = (MaterialVariantCatalog)target;

            EditorGUILayout.HelpBox(
                "本体材质球凹槽：点击 + 添加本体材质球，每添加一项即新增一个材质种类组。\n" +
                "材质球命名：作为材质种类组标题显示。\n" +
                "材质球列表：各类别下配置变体材质球；下方「全局变体参数应用」一键赋予所有种类组本体。\n" +
                "不同种类组可使用不同 Shader；同一种类组内本体与变体须为相同 Shader。\n" +
                "每个变体槽位可单独设置「平滑过渡时间（秒）」，0 表示立即应用。",
                MessageType.Info);

            DrawBodyMaterialCategoryEntries();

            serializedObject.ApplyModifiedProperties();
            catalog = (MaterialVariantCatalog)target;

            DrawCategoryShaderWarnings(catalog);
            DrawGlobalVariantApplyButtons(catalog);

            if (!string.IsNullOrWhiteSpace(resultMessage))
            {
                EditorGUILayout.HelpBox(resultMessage, resultMessageType);
            }
        }

        private void DrawCategoryShaderWarnings(MaterialVariantCatalog catalog)
        {
            if (!catalog.ValidateCatalogShaderConsistency(out string errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Warning);
            }
        }

        private void RefreshInspectorCategoryRows(MaterialVariantCatalog catalog)
        {
            inspectorCategoryRows.Clear();
            for (int entryIndex = 0; entryIndex < catalog.MaterialCategoryEntryCount; entryIndex++)
            {
                if (!catalog.TryGetCategoryEntry(entryIndex, out MaterialCategoryEntry entry))
                {
                    continue;
                }

                bool shaderConsistent = MaterialVariantApplyUtility.ValidateCategoryShaderConsistency(
                    entry,
                    out string shaderError);
                inspectorCategoryRows.Add(new InspectorCategoryRow
                {
                    Entry = entry,
                    ShaderConsistent = shaderConsistent,
                    ShaderError = shaderError
                });
            }
        }

        private void DrawGlobalVariantApplyButtons(MaterialVariantCatalog catalog)
        {
            int variantSlotCount = catalog.GetGlobalVariantSlotCount();
            if (variantSlotCount <= 0)
            {
                return;
            }

            RefreshInspectorCategoryRows(catalog);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("全局变体参数应用", EditorStyles.boldLabel);

            for (int variantIndex = 0; variantIndex < variantSlotCount; variantIndex++)
            {
                ResolveGlobalVariantButtonState(
                    variantIndex,
                    out string buttonLabel,
                    out bool canApply,
                    out string disableReason);

                using (new EditorGUI.DisabledScope(!canApply))
                {
                    if (GUILayout.Button(buttonLabel, LeftAlignedButtonStyle, GUILayout.Height(24f)))
                    {
                        ApplyGlobalVariantWithUndo(catalog, variantIndex);
                    }
                }

                if (!canApply && !string.IsNullOrWhiteSpace(disableReason))
                {
                    EditorGUILayout.HelpBox(disableReason, MessageType.None);
                }
            }

            EditorGUILayout.Space(4f);
        }

        private void ResolveGlobalVariantButtonState(
            int variantIndex,
            out string buttonLabel,
            out bool canApply,
            out string disableReason)
        {
            buttonLabel = $"赋予全部本体：变体 {variantIndex + 1}";
            canApply = false;
            disableReason = string.Empty;

            StringBuilder reasonBuilder = new StringBuilder();
            bool hasAnyVariantSlot = false;
            bool hasBodyMaterial = false;
            bool buttonLabelResolved = false;

            for (int rowIndex = 0; rowIndex < inspectorCategoryRows.Count; rowIndex++)
            {
                InspectorCategoryRow row = inspectorCategoryRows[rowIndex];
                MaterialCategoryEntry entry = row.Entry;
                if (entry == null)
                {
                    continue;
                }

                if (!row.ShaderConsistent)
                {
                    reasonBuilder.AppendLine($"按钮禁用：{row.ShaderError}");
                    continue;
                }

                if (entry.TryGetVariantMaterial(variantIndex, out Material variantMaterial))
                {
                    hasAnyVariantSlot = true;
                    if (!buttonLabelResolved)
                    {
                        buttonLabel = $"赋予全部本体：变体 {variantIndex + 1}（{variantMaterial.name}）";
                        buttonLabelResolved = true;
                    }

                    if (entry.bodyMaterial != null && entry.bodyMaterial.shader == variantMaterial.shader)
                    {
                        canApply = true;
                    }
                }

                if (entry.bodyMaterial != null)
                {
                    hasBodyMaterial = true;
                }

                if (!entry.TryGetVariantMaterial(variantIndex, out variantMaterial))
                {
                    continue;
                }

                if (entry.bodyMaterial == null)
                {
                    reasonBuilder.AppendLine(
                        $"[{entry.ResolveCategoryDisplayName()}] 未配置本体材质球。");
                    continue;
                }

                if (entry.bodyMaterial.shader != variantMaterial.shader)
                {
                    reasonBuilder.AppendLine(
                        $"[{entry.ResolveCategoryDisplayName()}] 本体与变体 {variantIndex + 1} 的 Shader 不一致。");
                }
            }

            if (!hasAnyVariantSlot)
            {
                disableReason = $"按钮禁用：变体 {variantIndex + 1} 在所有种类组中均未配置材质球。";
                canApply = false;
                return;
            }

            if (!hasBodyMaterial)
            {
                disableReason = "按钮禁用：所有种类组均未配置本体材质球。";
                canApply = false;
                return;
            }

            if (!canApply)
            {
                if (reasonBuilder.Length > 0)
                {
                    disableReason = "按钮禁用：\n" + reasonBuilder;
                }
                else
                {
                    disableReason = "按钮禁用：没有可应用的种类组，请检查本体与变体配置。";
                }
            }
        }

        private void ApplyGlobalVariantWithUndo(MaterialVariantCatalog catalog, int variantIndex)
        {
            CollectAllBodyMaterials(catalog);
            if (bodyMaterialsScratch.Count > 0)
            {
                Undo.RecordObjects(bodyMaterialsScratch.ToArray(), "全局应用变体材质参数到本体");
            }

            if (catalog.TryApplyVariantToAllCategories(variantIndex, out string result))
            {
                for (int index = 0; index < bodyMaterialsScratch.Count; index++)
                {
                    EditorUtility.SetDirty(bodyMaterialsScratch[index]);
                }

                resultMessageType = MessageType.Info;
                resultMessage = result;
            }
            else
            {
                resultMessageType = MessageType.Warning;
                resultMessage = result;
            }
        }

        private void CollectAllBodyMaterials(MaterialVariantCatalog catalog)
        {
            bodyMaterialsScratch.Clear();
            bodyMaterialSetScratch.Clear();
            for (int entryIndex = 0; entryIndex < catalog.MaterialCategoryEntryCount; entryIndex++)
            {
                if (!catalog.TryGetCategoryEntry(entryIndex, out MaterialCategoryEntry entry)
                    || entry.bodyMaterial == null)
                {
                    continue;
                }

                if (bodyMaterialSetScratch.Add(entry.bodyMaterial))
                {
                    bodyMaterialsScratch.Add(entry.bodyMaterial);
                }
            }
        }

        private void DrawBodyMaterialCategoryEntries()
        {
            if (materialCategoryEntriesProperty == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("本体材质球凹槽", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(28f), GUILayout.Height(20f)))
            {
                int newIndex = materialCategoryEntriesProperty.arraySize;
                materialCategoryEntriesProperty.InsertArrayElementAtIndex(newIndex);
                ResetCategoryEntry(materialCategoryEntriesProperty.GetArrayElementAtIndex(newIndex), newIndex);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4f);

            if (materialCategoryEntriesProperty.arraySize <= 0)
            {
                EditorGUILayout.HelpBox("请点击 + 添加本体材质球以创建材质种类组。", MessageType.None);
                return;
            }

            int removeIndex = -1;
            for (int entryIndex = 0; entryIndex < materialCategoryEntriesProperty.arraySize; entryIndex++)
            {
                SerializedProperty entryProperty = materialCategoryEntriesProperty.GetArrayElementAtIndex(entryIndex);
                SerializedProperty bodyMaterialProperty = entryProperty.FindPropertyRelative("bodyMaterial");
                SerializedProperty categoryNameProperty = entryProperty.FindPropertyRelative("categoryDisplayName");
                SerializedProperty variantMaterialSlotsProperty = entryProperty.FindPropertyRelative("variantMaterialSlots");

                string groupTitle = ResolveGroupTitle(categoryNameProperty, bodyMaterialProperty, entryIndex);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"材质种类组：{groupTitle}", EditorStyles.boldLabel);
                if (GUILayout.Button("-", GUILayout.Width(28f), GUILayout.Height(18f)))
                {
                    removeIndex = entryIndex;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                if (bodyMaterialProperty != null)
                {
                    EditorGUILayout.PropertyField(bodyMaterialProperty, new GUIContent("本体材质球"));
                }

                if (categoryNameProperty != null)
                {
                    EditorGUILayout.PropertyField(categoryNameProperty, new GUIContent("材质球命名"));
                }

                if (EditorGUI.EndChangeCheck())
                {
                    TryAutoFillCategoryNameFromBodyMaterial(categoryNameProperty, bodyMaterialProperty);
                }

                if (variantMaterialSlotsProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        variantMaterialSlotsProperty,
                        new GUIContent("变体槽位列表（材质球 + 过渡时间）"),
                        true);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4f);
            }

            if (removeIndex >= 0)
            {
                materialCategoryEntriesProperty.DeleteArrayElementAtIndex(removeIndex);
            }
        }

        private static string ResolveGroupTitle(
            SerializedProperty categoryNameProperty,
            SerializedProperty bodyMaterialProperty,
            int entryIndex)
        {
            if (categoryNameProperty != null && !string.IsNullOrWhiteSpace(categoryNameProperty.stringValue))
            {
                return categoryNameProperty.stringValue.Trim();
            }

            if (bodyMaterialProperty != null && bodyMaterialProperty.objectReferenceValue is Material bodyMaterial)
            {
                return bodyMaterial.name;
            }

            return $"材质球{entryIndex + 1}";
        }

        private static void TryAutoFillCategoryNameFromBodyMaterial(
            SerializedProperty categoryNameProperty,
            SerializedProperty bodyMaterialProperty)
        {
            if (categoryNameProperty == null || bodyMaterialProperty == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(categoryNameProperty.stringValue))
            {
                return;
            }

            if (bodyMaterialProperty.objectReferenceValue is Material bodyMaterial)
            {
                categoryNameProperty.stringValue = bodyMaterial.name;
            }
        }

        private static void ResetCategoryEntry(SerializedProperty entryProperty, int entryIndex)
        {
            SerializedProperty bodyMaterialProperty = entryProperty.FindPropertyRelative("bodyMaterial");
            SerializedProperty categoryNameProperty = entryProperty.FindPropertyRelative("categoryDisplayName");
            SerializedProperty variantMaterialSlotsProperty = entryProperty.FindPropertyRelative("variantMaterialSlots");

            if (bodyMaterialProperty != null)
            {
                bodyMaterialProperty.objectReferenceValue = null;
            }

            if (categoryNameProperty != null)
            {
                categoryNameProperty.stringValue = $"材质球{entryIndex + 1}";
            }

            if (variantMaterialSlotsProperty != null)
            {
                variantMaterialSlotsProperty.arraySize = 0;
            }
        }
    }
}
#endif
