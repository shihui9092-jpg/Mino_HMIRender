using System.Collections.Generic;
using UnityEngine;

namespace MinoHMI.UI.Core
{
    /// <summary>
    /// 页面控制器：提供 Push/Replace/Back 导航能力。
    /// </summary>
    [DisallowMultipleComponent]
    public class UIPageController : MonoBehaviour
    {
        [SerializeField] private UILayerStack layerStack;
        [SerializeField] private List<UIPageBase> registeredPages = new List<UIPageBase>();
        [SerializeField] private UIPageId startupPageId = UIPageId.Home;

        private readonly Dictionary<UIPageId, UIPageBase> pageRegistry = new Dictionary<UIPageId, UIPageBase>();

        private void Awake()
        {
            if (layerStack == null)
            {
                layerStack = GetComponentInChildren<UILayerStack>(true);
            }

            if (layerStack == null)
            {
                enabled = false;
                return;
            }

            RebuildRegistry();
            HideAllPages();
            if (startupPageId != UIPageId.None)
            {
                Push(startupPageId);
            }
        }

        public void RebuildRegistry()
        {
            pageRegistry.Clear();
            for (int index = 0; index < registeredPages.Count; index++)
            {
                UIPageBase page = registeredPages[index];
                if (page == null || page.PageId == UIPageId.None)
                {
                    continue;
                }

                pageRegistry[page.PageId] = page;
            }
        }

        public bool Push(UIPageId pageId)
        {
            if (!TryGetPage(pageId, out UIPageBase page))
            {
                return false;
            }

            UIPageBase previousPage = layerStack.Peek(page.Layer);
            if (previousPage != null)
            {
                previousPage.OnPageExit();
                previousPage.SetVisible(false);
            }

            AttachToLayer(page);
            page.SetVisible(true);
            page.OnPageEnter();
            layerStack.Push(page);
            return true;
        }

        public bool Replace(UIPageId pageId)
        {
            if (!TryGetPage(pageId, out UIPageBase nextPage))
            {
                return false;
            }

            UIPageBase currentPage = layerStack.Pop(nextPage.Layer);
            if (currentPage != null)
            {
                currentPage.OnPageExit();
                currentPage.SetVisible(false);
            }

            AttachToLayer(nextPage);
            nextPage.SetVisible(true);
            nextPage.OnPageEnter();
            layerStack.Push(nextPage);
            return true;
        }

        public bool Back(UIPageLayer layer)
        {
            UIPageBase currentPage = layerStack.Pop(layer);
            if (currentPage == null)
            {
                return false;
            }

            currentPage.OnPageExit();
            currentPage.SetVisible(false);

            UIPageBase previousPage = layerStack.Peek(layer);
            if (previousPage != null)
            {
                previousPage.SetVisible(true);
                previousPage.OnPageEnter();
            }

            return true;
        }

        public void HideAllPages()
        {
            for (int index = 0; index < registeredPages.Count; index++)
            {
                UIPageBase page = registeredPages[index];
                if (page == null)
                {
                    continue;
                }

                page.SetVisible(false);
            }

            layerStack.Clear(UIPageLayer.Base);
            layerStack.Clear(UIPageLayer.Popup);
            layerStack.Clear(UIPageLayer.System);
        }

        private bool TryGetPage(UIPageId pageId, out UIPageBase page)
        {
            return pageRegistry.TryGetValue(pageId, out page) && page != null;
        }

        private void AttachToLayer(UIPageBase page)
        {
            RectTransform layerRoot = layerStack.GetLayerRoot(page.Layer);
            if (layerRoot == null)
            {
                return;
            }

            page.transform.SetParent(layerRoot, false);
        }
    }
}
