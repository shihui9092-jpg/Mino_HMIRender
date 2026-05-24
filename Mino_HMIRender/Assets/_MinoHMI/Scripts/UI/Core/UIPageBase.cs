using UnityEngine;

namespace MinoHMI.UI.Core
{
    /// <summary>
    /// 页面基类：统一显隐和生命周期回调。
    /// </summary>
    [DisallowMultipleComponent]
    public class UIPageBase : MonoBehaviour, IUIPage
    {
        [SerializeField] private UIPageId pageId = UIPageId.None;
        [SerializeField] private UIPageLayer layer = UIPageLayer.Base;
        [SerializeField] private GameObject root;

        public UIPageId PageId => pageId;
        public UIPageLayer Layer => layer;
        public bool IsVisible => root != null && root.activeSelf;

        protected virtual void Reset()
        {
            root = gameObject;
        }

        protected virtual void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }
        }

        public virtual void OnPageEnter()
        {
        }

        public virtual void OnPageExit()
        {
        }

        public virtual void SetVisible(bool visible)
        {
            if (root == null)
            {
                return;
            }

            if (root.activeSelf != visible)
            {
                root.SetActive(visible);
            }
        }
    }
}
