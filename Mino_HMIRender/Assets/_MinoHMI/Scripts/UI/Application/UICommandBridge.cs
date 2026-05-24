using System.Collections.Generic;
using UnityEngine;

namespace MinoHMI.UI.Application
{
    /// <summary>
    /// 把 UICommandCenter 的字符串命令映射为强类型执行器。
    /// </summary>
    [DisallowMultipleComponent]
    public class UICommandBridge : MonoBehaviour
    {
        [System.Serializable]
        public class CommandRoute
        {
            public string commandId;
            public MonoBehaviour executorBehaviour;
        }

        [SerializeField] private UICommandCenter commandCenter;
        [SerializeField] private List<CommandRoute> commandRoutes = new List<CommandRoute>();

        private readonly List<string> registeredCommandIds = new List<string>();

        private void Awake()
        {
            if (commandCenter == null)
            {
                commandCenter = FindObjectOfType<UICommandCenter>(true);
            }

            RegisterAllRoutes();
        }

        private void OnDestroy()
        {
            UnregisterAllRoutes();
        }

        public void RegisterAllRoutes()
        {
            if (commandCenter == null)
            {
                return;
            }

            UnregisterAllRoutes();

            for (int index = 0; index < commandRoutes.Count; index++)
            {
                CommandRoute route = commandRoutes[index];
                if (route == null || string.IsNullOrWhiteSpace(route.commandId))
                {
                    continue;
                }

                if (!(route.executorBehaviour is IUiCommandExecutor commandExecutor))
                {
                    continue;
                }

                bool registerSucceeded = commandCenter.RegisterRuntimeCommand(
                    route.commandId,
                    commandExecutor.ExecuteCommand);
                if (registerSucceeded)
                {
                    registeredCommandIds.Add(route.commandId);
                }
            }
        }

        private void UnregisterAllRoutes()
        {
            if (commandCenter == null)
            {
                registeredCommandIds.Clear();
                return;
            }

            for (int index = 0; index < registeredCommandIds.Count; index++)
            {
                commandCenter.UnregisterRuntimeCommand(registeredCommandIds[index]);
            }

            registeredCommandIds.Clear();
        }
    }
}
