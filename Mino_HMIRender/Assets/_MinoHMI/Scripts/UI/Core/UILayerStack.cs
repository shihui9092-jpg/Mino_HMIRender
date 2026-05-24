using System.Collections.Generic;
using UnityEngine;

namespace MinoHMI.UI.Core
{
    /// <summary>
    /// 管理各 UI 层的挂载节点和页面栈。
    /// </summary>
    [DisallowMultipleComponent]
    public class UILayerStack : MonoBehaviour
    {
        [System.Serializable]
        public class LayerRoot
        {
            public UIPageLayer layer;
            public RectTransform root;
        }

        [SerializeField] private List<LayerRoot> layerRoots = new List<LayerRoot>();

        private readonly Dictionary<UIPageLayer, RectTransform> layerRootMap = new Dictionary<UIPageLayer, RectTransform>();
        private readonly Dictionary<UIPageLayer, Stack<UIPageBase>> layerPageStacks = new Dictionary<UIPageLayer, Stack<UIPageBase>>();

        private void Awake()
        {
            RebuildLayerRootMap();
        }

        public void RebuildLayerRootMap()
        {
            layerRootMap.Clear();

            for (int index = 0; index < layerRoots.Count; index++)
            {
                LayerRoot layerRoot = layerRoots[index];
                if (layerRoot == null || layerRoot.root == null)
                {
                    continue;
                }

                layerRootMap[layerRoot.layer] = layerRoot.root;
            }
        }

        public RectTransform GetLayerRoot(UIPageLayer layer)
        {
            if (layerRootMap.TryGetValue(layer, out RectTransform root) && root != null)
            {
                return root;
            }

            return transform as RectTransform;
        }

        public void Push(UIPageBase page)
        {
            if (page == null)
            {
                return;
            }

            Stack<UIPageBase> stack = GetLayerStack(page.Layer);
            stack.Push(page);
        }

        public UIPageBase Pop(UIPageLayer layer)
        {
            Stack<UIPageBase> stack = GetLayerStack(layer);
            if (stack.Count <= 0)
            {
                return null;
            }

            return stack.Pop();
        }

        public UIPageBase Peek(UIPageLayer layer)
        {
            Stack<UIPageBase> stack = GetLayerStack(layer);
            if (stack.Count <= 0)
            {
                return null;
            }

            return stack.Peek();
        }

        public void Clear(UIPageLayer layer)
        {
            GetLayerStack(layer).Clear();
        }

        private Stack<UIPageBase> GetLayerStack(UIPageLayer layer)
        {
            if (!layerPageStacks.TryGetValue(layer, out Stack<UIPageBase> pageStack))
            {
                pageStack = new Stack<UIPageBase>();
                layerPageStacks.Add(layer, pageStack);
            }

            return pageStack;
        }
    }
}
