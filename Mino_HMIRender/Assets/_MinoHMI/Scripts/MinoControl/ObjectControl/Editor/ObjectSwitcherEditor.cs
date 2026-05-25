#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MinoHMI.MY26HMI.ObjectControl
{
    /// <summary>
    /// 对象切换组件 Inspector：提供按绑定项或当前激活场景切换对象显示/隐藏的按钮。
    /// </summary>
    [CustomEditor(typeof(ObjectSwitcher))]
    public class ObjectSwitcherEditor : UnityEditor.Editor
    {
        private SerializedProperty objectSlotsProperty;
        private string resultMessage = string.Empty;
        private MessageType resultMessageType = MessageType.None;

        private void OnEnable()
        {
            objectSlotsProperty = serializedObject.FindProperty("objectSlots");
            if (objectSlotsProperty == null)
            {
                objectSlotsProperty = serializedObject.FindProperty("objectSceneSlots");
            }

            if (objectSlotsProperty == null)
            {
                objectSlotsProperty = serializedObject.FindProperty("sceneSwitchSlots");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "凹槽请拖入场景中的 GameObject 或组件。点击按钮可显示对应绑定对象，并隐藏列表中其他对象。",
                MessageType.Info);

            EditorGUILayout.PropertyField(objectSlotsProperty, true);

            EditorGUILayout.Space(8f);
            DrawVisibilityButtons();

            if (!string.IsNullOrWhiteSpace(resultMessage))
            {
                EditorGUILayout.HelpBox(resultMessage, resultMessageType);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawVisibilityButtons()
        {
            ObjectSwitcher objectSwitcher = (ObjectSwitcher)target;
            Scene activeScene = SceneManager.GetActiveScene();
            string activeSceneLabel = activeScene.IsValid() ? activeScene.name : "（无有效场景）";

            EditorGUILayout.LabelField("对象显示控制", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("当前激活场景", activeSceneLabel);

            using (new EditorGUI.DisabledScope(objectSwitcher.ObjectSlotCount <= 0))
            {
                if (GUILayout.Button("按当前激活场景显示 / 隐藏其他", GUILayout.Height(26f)))
                {
                    ApplyVisibilityByActiveSceneWithUndo(objectSwitcher);
                }
            }

            EditorGUILayout.Space(4f);

            if (objectSlotsProperty == null || !objectSlotsProperty.isArray)
            {
                return;
            }

            for (int index = 0; index < objectSlotsProperty.arraySize; index++)
            {
                SerializedProperty slotProperty = objectSlotsProperty.GetArrayElementAtIndex(index);
                SerializedProperty sceneNameProperty = slotProperty.FindPropertyRelative("targetSceneName");
                string sceneName = sceneNameProperty != null ? sceneNameProperty.stringValue : string.Empty;
                string buttonLabel = string.IsNullOrWhiteSpace(sceneName)
                    ? $"显示绑定项 {index + 1}"
                    : $"显示「{sceneName}」并隐藏其他";

                if (GUILayout.Button(buttonLabel, GUILayout.Height(22f)))
                {
                    ApplyVisibilityAtIndexWithUndo(objectSwitcher, index);
                }
            }
        }

        private void ApplyVisibilityByActiveSceneWithUndo(ObjectSwitcher objectSwitcher)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            int activeIndex = objectSwitcher.FindSlotIndexBySceneName(activeScene.name);
            if (activeIndex < 0)
            {
                resultMessageType = MessageType.Warning;
                resultMessage = $"未在绑定列表中找到场景「{activeScene.name}」。";
                return;
            }

            ApplyVisibilityAtIndexWithUndo(objectSwitcher, activeIndex);
        }

        private void ApplyVisibilityAtIndexWithUndo(ObjectSwitcher objectSwitcher, int activeIndex)
        {
            List<Object> undoTargets = CollectBoundObjectsForUndo(objectSwitcher);
            if (undoTargets.Count > 0)
            {
                Undo.RecordObjects(undoTargets.ToArray(), "对象显示切换");
            }

            objectSwitcher.ApplyVisibilityAtIndex(activeIndex);

            if (!Application.isPlaying && objectSwitcher.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(objectSwitcher.gameObject.scene);
            }

            EditorUtility.SetDirty(objectSwitcher);

            if (TryGetObjectSlot(objectSwitcher, activeIndex, out ObjectSlot activeSlot))
            {
                string sceneLabel = activeSlot.HasTargetScene ? activeSlot.targetSceneName : $"索引 {activeIndex + 1}";
                resultMessageType = MessageType.Info;
                resultMessage = $"已显示「{sceneLabel}」对应对象，并隐藏其他绑定对象。";
            }
        }

        private static List<Object> CollectBoundObjectsForUndo(ObjectSwitcher objectSwitcher)
        {
            List<Object> undoTargets = new List<Object>();
            for (int index = 0; index < objectSwitcher.ObjectSlotCount; index++)
            {
                if (!TryGetObjectSlot(objectSwitcher, index, out ObjectSlot slot))
                {
                    continue;
                }

                if (!slot.TryGetBoundGameObject(out GameObject boundObject))
                {
                    continue;
                }

                if (!undoTargets.Contains(boundObject))
                {
                    undoTargets.Add(boundObject);
                }
            }

            return undoTargets;
        }

        private static bool TryGetObjectSlot(ObjectSwitcher objectSwitcher, int index, out ObjectSlot slot)
        {
            return objectSwitcher.TryGetObjectSlot(index, out slot);
        }
    }
}
#endif
