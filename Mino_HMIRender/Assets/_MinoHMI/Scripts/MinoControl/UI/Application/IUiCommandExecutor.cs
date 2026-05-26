namespace MinoHMI.UI.Application
{
    /// <summary>
    /// UI 命令执行器：供 UICommandBridge 或 UnityEvent 调用的用例接口。
    /// </summary>
    public interface IUiCommandExecutor
    {
        void ExecuteCommand();
    }
}
