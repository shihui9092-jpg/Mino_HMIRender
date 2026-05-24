using MinoHMI.UI.Core;
using UnityEngine;

namespace MinoHMI.UI.Application
{
    /// <summary>
    /// 页面导航用例，供 UICommandBridge 调用。
    /// </summary>
    [DisallowMultipleComponent]
    public class UiPageNavigationUseCase : MonoBehaviour, IUiCommandExecutor
    {
        [SerializeField] private UIPageController pageController;
        [SerializeField] private UIPageId targetPageId = UIPageId.Home;

        public void Bind(UIPageController controller)
        {
            pageController = controller;
        }

        public void ExecuteCommand()
        {
            if (pageController == null)
            {
                return;
            }

            pageController.Push(targetPageId);
        }
    }
}
