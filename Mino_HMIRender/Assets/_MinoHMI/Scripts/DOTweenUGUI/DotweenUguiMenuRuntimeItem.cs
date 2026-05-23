using UnityEngine;

namespace MinoHMI.DOTweenUGUI
{
    /// <summary>
    /// 标记由菜单系统在运行时动态生成的按钮；清理时仅删除带此组件的对象。
    /// </summary>
    public class DotweenUguiMenuRuntimeItem : MonoBehaviour
    {
        internal const string MainButtonNamePrefix = "MainButton_";
        internal const string SubButtonNamePrefix = "SubButton_";

        /// <summary>
        /// 标记为运行时按钮。
        /// </summary>
        public static void Mark(GameObject targetObject)
        {
            if (targetObject == null)
            {
                return;
            }

            if (targetObject.GetComponent<DotweenUguiMenuRuntimeItem>() == null)
            {
                targetObject.AddComponent<DotweenUguiMenuRuntimeItem>();
            }
        }

        /// <summary>
        /// 删除指定容器下所有运行时按钮。
        /// </summary>
        public static void PurgeUnder(Transform container)
        {
            if (container == null)
            {
                return;
            }

            DotweenUguiMenuRuntimeItem[] runtimeItems = container.GetComponentsInChildren<DotweenUguiMenuRuntimeItem>(true);
            for (int i = runtimeItems.Length - 1; i >= 0; i--)
            {
                DotweenUguiMenuRuntimeItem runtimeItem = runtimeItems[i];
                if (runtimeItem == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(runtimeItem.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(runtimeItem.gameObject);
                }
            }
        }
    }
}
