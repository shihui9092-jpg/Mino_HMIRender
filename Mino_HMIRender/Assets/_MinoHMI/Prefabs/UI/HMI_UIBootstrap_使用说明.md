# HMI_UIBootstrap 预制体使用说明

## 快速接入

1. 将车模场景中的 `CarRoot`（需挂 `CarPaintSwitcher`）与 `MinoCameraController` 准备好。
2. 将 `Assets/_MinoHMI/Prefabs/UI/HMI_UIBootstrap.prefab` 拖入场景。
3. 菜单执行 **MinoHMI → CarPaint → Setup CarRoot Presets**（首次配置车模 6 色预设）。
4. 进入 Play Mode，点击底部 **CarPaintColorDock** 色块验证车漆切换。

## 预制体包含内容

- `UIRoot` / `UIPageController` / `UILayerStack`（Base/Popup/System）
- `UICommandCenter` + `UICommandBridge`（命令路由）
- `HmiUiBootstrapBinder`（自动绑定场景中的车漆与相机）
- `UIInteractionArbiter`（UI 操作时禁用车模相机拖拽）
- `UiPerformanceGovernor` + `UiFrameBudgetWatcher`（8295 性能治理）
- 示例页面：`Page_Home`、`Page_CarPaint`
- **CarPaintColorDock**：底部常驻 6 色色条（与 Page 平级，换页不消失）

## 车漆色条命令 ID（索引与 CarRoot 预设一致）

| 索引 | 颜色 | CommandId |
|------|------|-----------|
| 0 | 珍珠白 | `HMI.CarPaint.Preset0` |
| 1 | 曜石黑 | `HMI.CarPaint.Preset1` |
| 2 | 烈焰红 | `HMI.CarPaint.Preset2` |
| 3 | 晴空蓝 | `HMI.CarPaint.Preset3` |
| 4 | 金属灰 | `HMI.CarPaint.Preset4` |
| 5 | 竞速黄 | `HMI.CarPaint.Preset5` |

## 重新生成预制体

菜单：**MinoHMI → UI → Create UIBootstrap Prefab**

会同步更新：

- `Assets/_MinoHMI/Prefabs/UI/HMI_UIBootstrap.prefab`
- `Assets/_MinoHMI/Settings/UI/Performance/UrpQuality_*.asset`
