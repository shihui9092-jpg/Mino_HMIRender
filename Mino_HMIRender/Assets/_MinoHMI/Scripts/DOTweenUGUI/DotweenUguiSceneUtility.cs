using UnityEngine;

namespace MinoHMI.DOTweenUGUI
{
    /// <summary>
    /// 判断 Transform 是否属于已加载场景实例（非 Project 中的 Persistent 预制体资源）。
    /// </summary>
    internal static class DotweenUguiSceneUtility
    {
        public static bool IsSceneInstance(Component component)
        {
            return component != null && component.gameObject.scene.IsValid();
        }

        public static bool CanInstantiateUnder(Transform parent)
        {
            return IsSceneInstance(parent);
        }

        public static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
