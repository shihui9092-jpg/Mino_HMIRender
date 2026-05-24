namespace MinoHMI.UI.Core
{
    /// <summary>
    /// 页面生命周期接口，约束页面最小行为。
    /// </summary>
    public interface IUIPage
    {
        UIPageId PageId { get; }
        UIPageLayer Layer { get; }
        bool IsVisible { get; }

        void OnPageEnter();
        void OnPageExit();
        void SetVisible(bool visible);
    }
}
