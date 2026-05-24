using UnityEngine;

namespace MinoHMI.UI.Core
{
    /// <summary>
    /// UI 框架入口，统一暴露页面控制器。
    /// </summary>
    [DisallowMultipleComponent]
    public class UIRoot : MonoBehaviour
    {
        [SerializeField] private UIPageController pageController;

        public UIPageController PageController => pageController;

        private void Awake()
        {
            if (pageController == null)
            {
                pageController = GetComponentInChildren<UIPageController>(true);
            }
        }
    }
}
