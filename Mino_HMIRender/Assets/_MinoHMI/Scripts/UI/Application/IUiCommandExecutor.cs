namespace MinoHMI.UI.Application
{
    /// <summary>
    /// UI 命令执行接口，用于桥接字符串命令到强类型逻辑。
    /// </summary>
    public interface IUiCommandExecutor
    {
        void ExecuteCommand();
    }
}
