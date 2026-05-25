using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MinoHMI.MY26HMI
{
    /// <summary>
    /// MY26 HMI 场景切换：场景名与对象凹槽按数组条目一一绑定。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MinoHMI/MY26/场景切换")]
    public class HmiSceneSwitcher : MonoBehaviour
    {
        [Header("场景与凹槽绑定列表")]
        [SerializeField]
        [Tooltip("可在 Inspector 中点击 + 添加；每项同时配置目标场景名与对象凹槽")]
        private HmiSceneObjectSlot[] sceneSwitchSlots = Array.Empty<HmiSceneObjectSlot>();

        public int SceneSwitchSlotCount => sceneSwitchSlots?.Length ?? 0;

        public bool TryGetSceneSwitchSlot(int index, out HmiSceneObjectSlot slot)
        {
            if (sceneSwitchSlots != null && index >= 0 && index < sceneSwitchSlots.Length)
            {
                slot = sceneSwitchSlots[index];
                return slot != null;
            }

            slot = null;
            return false;
        }

        public HmiSceneObjectSlot GetSceneSwitchSlot(int index)
        {
            return TryGetSceneSwitchSlot(index, out HmiSceneObjectSlot slot) ? slot : null;
        }

        /// <summary>
        /// 按场景名查找绑定列表索引，未找到返回 -1。
        /// </summary>
        public int FindSlotIndexBySceneName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || sceneSwitchSlots == null)
            {
                return -1;
            }

            for (int index = 0; index < sceneSwitchSlots.Length; index++)
            {
                HmiSceneObjectSlot slot = sceneSwitchSlots[index];
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
            if (sceneSwitchSlots == null || sceneSwitchSlots.Length == 0)
            {
                Debug.LogWarning($"[{nameof(HmiSceneSwitcher)}] 绑定列表为空，无法切换显示。", this);
                return;
            }

            if (activeIndex < 0 || activeIndex >= sceneSwitchSlots.Length)
            {
                Debug.LogWarning($"[{nameof(HmiSceneSwitcher)}] 索引 {activeIndex} 无效，无法切换显示。", this);
                return;
            }

            for (int index = 0; index < sceneSwitchSlots.Length; index++)
            {
                HmiSceneObjectSlot slot = sceneSwitchSlots[index];
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
                Debug.LogWarning($"[{nameof(HmiSceneSwitcher)}] 当前无有效激活场景。", this);
                return;
            }

            int activeIndex = FindSlotIndexBySceneName(activeScene.name);
            if (activeIndex < 0)
            {
                Debug.LogWarning(
                    $"[{nameof(HmiSceneSwitcher)}] 未在绑定列表中找到场景「{activeScene.name}」，请检查 Target Scene Name。",
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
            if (!TryGetSceneSwitchSlot(index, out HmiSceneObjectSlot slot))
            {
                Debug.LogWarning($"[{nameof(HmiSceneSwitcher)}] 索引 {index} 无效，无法加载场景。", this);
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
                Debug.LogWarning($"[{nameof(HmiSceneSwitcher)}] 场景名为空，无法加载。", this);
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
