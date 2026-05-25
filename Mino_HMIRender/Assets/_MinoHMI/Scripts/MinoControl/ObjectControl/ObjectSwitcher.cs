using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace MinoHMI.MY26HMI.ObjectControl
{
    /// <summary>
    /// 对象切换：场景名与对象凹槽按数组条目一一绑定。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MinoHMI/对象控制/对象切换")]
    [MovedFrom("MinoHMI.MY26HMI.ObjectControl.ObjectSceneSwitcher")]
    public class ObjectSwitcher : MonoBehaviour
    {
        [Header("场景与凹槽绑定列表")]
        [SerializeField]
        [Tooltip("可在 Inspector 中点击 + 添加；每项同时配置目标场景名与对象凹槽")]
        [FormerlySerializedAs("objectSceneSlots")]
        [FormerlySerializedAs("sceneSwitchSlots")]
        private ObjectSlot[] objectSlots = Array.Empty<ObjectSlot>();

        public int ObjectSlotCount => objectSlots?.Length ?? 0;

        public bool TryGetObjectSlot(int index, out ObjectSlot slot)
        {
            if (objectSlots != null && index >= 0 && index < objectSlots.Length)
            {
                slot = objectSlots[index];
                return slot != null;
            }

            slot = null;
            return false;
        }

        public ObjectSlot GetObjectSlot(int index)
        {
            return TryGetObjectSlot(index, out ObjectSlot slot) ? slot : null;
        }

        /// <summary>
        /// 按场景名查找绑定列表索引，未找到返回 -1。
        /// </summary>
        public int FindSlotIndexBySceneName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || objectSlots == null)
            {
                return -1;
            }

            for (int index = 0; index < objectSlots.Length; index++)
            {
                ObjectSlot slot = objectSlots[index];
                if (slot == null || !slot.HasTargetScene)
                {
                    continue;
                }

                if (string.Equals(slot.targetSceneName, sceneName, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// 显示指定索引绑定对象，并隐藏列表中其他绑定对象。
        /// </summary>
        public void ApplyVisibilityAtIndex(int activeIndex)
        {
            if (objectSlots == null || objectSlots.Length == 0)
            {
                Debug.LogWarning($"[{nameof(ObjectSwitcher)}] 绑定列表为空，无法切换显示。", this);
                return;
            }

            if (activeIndex < 0 || activeIndex >= objectSlots.Length)
            {
                Debug.LogWarning($"[{nameof(ObjectSwitcher)}] 索引 {activeIndex} 无效，无法切换显示。", this);
                return;
            }

            for (int index = 0; index < objectSlots.Length; index++)
            {
                ObjectSlot slot = objectSlots[index];
                if (slot == null)
                {
                    continue;
                }

                slot.TrySetGameObjectActive(index == activeIndex);
            }
        }

        /// <summary>
        /// 按当前激活场景名匹配绑定项，显示对应对象并隐藏其他对象。
        /// </summary>
        public void ApplyVisibilityByActiveScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                Debug.LogWarning($"[{nameof(ObjectSwitcher)}] 当前无有效激活场景。", this);
                return;
            }

            int activeIndex = FindSlotIndexBySceneName(activeScene.name);
            if (activeIndex < 0)
            {
                Debug.LogWarning(
                    $"[{nameof(ObjectSwitcher)}] 未在绑定列表中找到场景「{activeScene.name}」，请检查 Target Scene Name。",
                    this);
                return;
            }

            ApplyVisibilityAtIndex(activeIndex);
        }

        /// <summary>
        /// 按绑定列表索引加载对应目标场景。
        /// </summary>
        public void LoadSceneAtIndex(int index)
        {
            if (!TryGetObjectSlot(index, out ObjectSlot slot))
            {
                Debug.LogWarning($"[{nameof(ObjectSwitcher)}] 索引 {index} 无效，无法加载场景。", this);
                return;
            }

            LoadSceneByName(slot.targetSceneName);
        }

        /// <summary>
        /// 按指定名称加载场景。
        /// </summary>
        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning($"[{nameof(ObjectSwitcher)}] 场景名为空，无法加载。", this);
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
