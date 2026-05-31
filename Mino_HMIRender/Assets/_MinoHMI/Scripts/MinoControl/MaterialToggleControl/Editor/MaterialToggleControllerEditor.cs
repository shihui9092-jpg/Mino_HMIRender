#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MinoHMI.MY26HMI.MaterialToggleControl
{
    /// <summary>
    /// 材质切换控制器 Inspector：为每个开关参数名生成独立切换按钮。
    /// </summary>
    [CustomEditor(typeof(MaterialToggleController))]
    public class MaterialToggleControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty sharedSwitchParameterNamesProperty;
        private SerializedProperty materialToggleSlotsProperty;
        private string resultMessage = string.Empty;
        private MessageType resultMessageType = MessageType.None;
        private GUIStyle leftAlignedButtonStyle;
        private readonly List<Material> configuredMaterialsScratch = new List<Material>();

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
            sharedSwitchParameterNamesProperty = serializedObject.FindProperty("sharedSwitchParameterNames");
            materialToggleSlotsProperty = serializedObject.FindProperty("materialToggleSlots");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "统一开关参数：点击 + 添加 Shader 开关属性名，每个属性名对应下方一个独立按钮。\n" +
                "材质球凹槽：配置需要被批量驱动的材质球。\n" +
                "运行时会先将全部开关设为关闭；点击按钮开，再次点击关。",
                MessageType.Info);

            if (sharedSwitchParameterNamesProperty != null)
            {
                EditorGUILayout.PropertyField(
                    sharedSwitchParameterNamesProperty,
                    new GUIContent("统一开关参数"),
                    true);
            }

            if (materialToggleSlotsProperty != null)
            {
                EditorGUILayout.PropertyField(
                    materialToggleSlotsProperty,
                    new GUIContent("材质球凹槽"),
                    true);
            }

            serializedObject.ApplyModifiedProperties();

            MaterialToggleController controller = (MaterialToggleController)target;
            DrawSwitchParameterButtons(controller);

            if (!string.IsNullOrWhiteSpace(resultMessage))
            {
                EditorGUILayout.HelpBox(resultMessage, resultMessageType);
            }
        }

        private void DrawSwitchParameterButtons(MaterialToggleController controller)
        {
            int parameterCount = controller.SharedSwitchParameterNameCount;
            if (parameterCount <= 0)
            {
                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("开关参数控制", EditorStyles.boldLabel);

            for (int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
            {
                controller.TryResolveSwitchParameterButtonState(
                    parameterIndex,
                    out string buttonLabel,
                    out bool canToggle,
                    out string disableReason);

                using (new EditorGUI.DisabledScope(!canToggle))
                {
                    if (GUILayout.Button(buttonLabel, LeftAlignedButtonStyle, GUILayout.Height(24f)))
                    {
                        ToggleSwitchParameterWithUndo(controller, parameterIndex);
                    }
                }

                if (!canToggle && !string.IsNullOrWhiteSpace(disableReason))
                {
                    EditorGUILayout.HelpBox(disableReason, MessageType.None);
                }
            }

            EditorGUILayout.Space(4f);
        }

        private void ToggleSwitchParameterWithUndo(MaterialToggleController controller, int parameterIndex)
        {
            if (controller.TryCollectConfiguredMaterials(configuredMaterialsScratch)
                && configuredMaterialsScratch.Count > 0)
            {
                Undo.RecordObjects(
                    configuredMaterialsScratch.ToArray(),
                    "切换材质开关参数");
            }

            if (controller.TryToggleSharedSwitchParameter(parameterIndex, out string errorMessage))
            {
                for (int index = 0; index < configuredMaterialsScratch.Count; index++)
                {
                    EditorUtility.SetDirty(configuredMaterialsScratch[index]);
                }

                resultMessageType = MessageType.Info;
                resultMessage = controller.IsSharedSwitchParameterEnabled(parameterIndex)
                    ? "开关已打开。"
                    : "开关已关闭。";
            }
            else
            {
                resultMessageType = MessageType.Warning;
                resultMessage = errorMessage;
            }

            Repaint();
        }
    }
}
#endif
