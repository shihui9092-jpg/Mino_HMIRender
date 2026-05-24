# HMI_UIBootstrap 预制体使用说明

## 快速接入

1. 将车模场景中的 `CarRoot`（需挂 `CarPaintSwitcher`）与 `MinoCameraController` 准备好。
2. 将 `Assets/_MinoHMI/Prefabs/UI/HMI_UIBootstrap.prefab` 拖入场景。
3. 进入 Play Mode，点击底部 **DemoCommandBar** 按钮验证整条链路。

## 预制体包含内容

- `UIRoot` / `UIPageController` / `UILayerStack`（Base/Popup/System）
- `UICommandCenter` + `UICommandBridge`（命令路由）
- `HmiUiBootstrapBinder`（自动绑定场景中的车漆与相机）
- `UIInteractionArbiter`（UI 操作时禁用车模相机拖拽）
- `UiPerformanceGovernor` + `UiFrameBudgetWatcher`（8295 性能治理）
- 示例页面：`Page_Home`、`Page_CarPaint`
- 示例命令按钮（底部常驻条）

## 重新生成预制体

菜单：**MinoHMI → UI → Create UIBootstrap Prefab**
