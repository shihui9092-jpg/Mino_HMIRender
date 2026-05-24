# 3D HMI UI 布局规范（2880×1620 · 方案 B · 保留 CarPaintColorDock）

> 设计稿坐标系：**屏幕左上角为 (0,0)**，`X` 向右、`Y` 向下。  
> 基于现有 `HMI_UIBootstrap` 框架扩展；**本文档仅作实施对照，不自动改工程**。  
> 接入时请使用 **Prefab 变体**（如 `HMI_UIBootstrap_2880.prefab`），避免菜单 **MinoHMI → UI → Create UIBootstrap Prefab** 覆盖原版。

---

## 一、全局分区（定稿尺寸）

| 区域 | L | T | W | H | 说明 |
|------|---|---|---|---|------|
| `TopStatusBar` | 0 | 0 | 2880 | **80** | 顶栏常驻 |
| `LeftNavDock` | 0 | **80** | **360** | **1360** | 主界面可见；**Settings 时被 Page_Settings 盖住** |
| `ContentSafeArea`（Home） | **360** | **80** | **2520** | **1360** | 3D 车模主视野 |
| `Page_Settings`（方案 B） | **0** | **80** | **2880** | **1360** | 全宽，覆盖左栏 |
| `CarPaintColorDock` | 0 | **1440** | 2880 | **180** | 底栏常驻（1620−180=1440） |

校验：80 + 1360 + 180 = **1620**。

```
Y=0    ┌──────────────────── TopStatusBar 80 ────────────────────┐
Y=80   ├──────┬──────────────────────────────────────────────────┤
       │ Left │  Home: Content 2520×1360                          │
       │ 360  │  Settings: 全宽 2880×1360（盖住左栏）                 │
Y=1440 ├──────┴──────────────────────────────────────────────────┤
Y=1620 └──────────────── CarPaintColorDock 180 ─────────────────────┘
```

---

## 二、Canvas / Scaler

| 组件 | 参数 |
|------|------|
| `CanvasScaler.uiScaleMode` | Scale With Screen Size |
| `referenceResolution` | **(2880, 1620)** |
| `matchWidthOrHeight` | **0.5** |
| `Canvas.renderMode` | Screen Space Overlay |

---

## 三、节点树（方案 B 定稿）

```
Canvas
├── Layer_Base
│   ├── TopStatusBar                    [L0 T0 W2880 H80]
│   ├── LeftNavDock                     [L0 T80 W360 H1360]
│   │   ├── NavBackground
│   │   └── NavButtonList
│   │       ├── Btn_Home
│   │       ├── Btn_Settings
│   │       ├── Btn_Camera
│   │       ├── Btn_Light               （可选）
│   │       └── Btn_VehicleInfo         （可选）
│   ├── ContentSafeArea                 [L360 T80 W2520 H1360]
│   │   └── Page_Home                   （UIPageBase, PageId=Home，透明不挡 3D）
│   ├── Page_Settings                   [L0 T80 W2880 H1360] ★方案B，与上三项平级
│   │   ├── SettingsBackground
│   │   ├── SettingsHeader
│   │   │   ├── Btn_Back
│   │   │   └── Txt_Title
│   │   └── SettingsScrollView
│   └── CarPaintColorDock               [L0 T1440 W2880 H180]
│       ├── DockBackground
│       └── SwatchLayout
│           ├── Swatch_White … Swatch_Yellow（6）
├── Layer_Popup
└── Layer_System
```

**Hierarchy 建议（自下而上）：** `ContentSafeArea` → `LeftNavDock` → `Page_Settings` → `CarPaintColorDock` → `TopStatusBar`。

### 与框架的对应关系

| 节点 | UIPageLayer | UIPageBase | 导航 / 交互 |
|------|-------------|------------|-------------|
| `TopStatusBar` | — | 否 | 常驻 |
| `LeftNavDock` | — | 否 | 按钮 → Command |
| `CarPaintColorDock` | — | 否 | `HMI.CarPaint.Preset0~5` |
| `Page_Home` | Base | 是 | `Push(Home)` |
| `Page_Settings` | Base | 是 | `Push(Settings)` |
| `Layer_Popup` | Popup | — | 自管显隐 |

---

## 四、容器 RectTransform 总表

父级均为 `Layer_Base` 直接子节点（全屏拉伸时 offset 写法）。

### 4.1 TopStatusBar

| 属性 | 值 |
|------|-----|
| anchorMin / anchorMax | (0, **1**) ~ (1, **1**) |
| pivot | (0.5, **1**) |
| anchoredPosition | (0, **0**) |
| sizeDelta | (0, **80**) |
| **屏幕 L/T/W/H** | **0 / 0 / 2880 / 80** |

### 4.2 LeftNavDock

| 属性 | 值 |
|------|-----|
| anchorMin / anchorMax | (**0**, 0) ~ (**0**, 1) |
| pivot | (**0**, 0.5) |
| anchoredPosition | (0, 0) |
| offsetMin | (**0**, **180**) |
| offsetMax | (**360**, **80**) |
| **屏幕 L/T/W/H** | **0 / 80 / 360 / 1360** |

### 4.3 ContentSafeArea（Home）

| 属性 | 值 |
|------|-----|
| anchorMin / anchorMax | (0, 0) ~ (1, 1) |
| offsetMin | (**360**, **180**) |
| offsetMax | (**0**, **-80**) |
| **屏幕 L/T/W/H** | **360 / 80 / 2520 / 1360** |

### 4.4 Page_Home

| 属性 | 值 |
|------|-----|
| 父级 | `ContentSafeArea`，撑满 |
| offsetMin / offsetMax | (0, 0) |
| **屏幕 L/T/W/H** | **360 / 80 / 2520 / 1360** |
| 背景 | 全透明，`Raycast Target = false` |

### 4.5 Page_Settings（方案 B）

| 属性 | 值 |
|------|-----|
| anchorMin / anchorMax | (0, 0) ~ (1, 1) |
| offsetMin | (**0**, **180**) |
| offsetMax | (**0**, **-80**) |
| **屏幕 L/T/W/H** | **0 / 80 / 2880 / 1360** |
| 背景 | 建议 `#141414`，alpha **0.94**，`Raycast Target = true` |

### 4.6 CarPaintColorDock

| 属性 | 值 |
|------|-----|
| anchorMin / anchorMax | (0, **0**) ~ (1, **0**) |
| pivot | (0.5, **0**) |
| anchoredPosition | (0, **0**) |
| sizeDelta | (0, **180**) |
| **屏幕 L/T/W/H** | **0 / 1440 / 2880 / 180** |

`SwatchLayout`：**HorizontalLayoutGroup** — Padding **32,32,28,28**，Spacing **24**，Child Alignment **Middle Center**。

---

## 五、顶栏控件精确表

父级：`TopStatusBar`（2880×80）。

| 节点 | anchorMin | anchorMax | pivot | anchoredPosition | sizeDelta | 屏幕 L | T | W | H |
|------|-----------|-----------|-------|------------------|-----------|--------|---|---|---|
| Txt_Time | (0, 0.5) | (0, 0.5) | (0, 0.5) | (**48**, 0) | (200, 48) | 48 | 16 | 200 | 48 |
| Txt_Title | (0.5, 0.5) | (0.5, 0.5) | (0.5, 0.5) | (0, 0) | (480, 48) | 1200 | 16 | 480 | 48 |
| Txt_Fps | (1, 0.5) | (1, 0.5) | (1, 0.5) | (**-48**, 0) | (160, 48) | 2672 | 16 | 160 | 48 |

---

## 六、左侧菜单按钮精确表

父级：`NavButtonList`（360×1360）。**距顶 48**，按钮 **320×88**，**间距 16**，水平居中（L=20）。

| 节点 | 序号 | 屏幕 L | T | W | H | 中心 (Cx, Cy) |
|------|------|--------|---|---|---|---------------|
| Btn_Home | 0 | 20 | **128** | 320 | 88 | (180, 172) |
| Btn_Settings | 1 | 20 | **232** | 320 | 88 | (180, 276) |
| Btn_Camera | 2 | 20 | **336** | 320 | 88 | (180, 380) |
| Btn_Light | 3 | 20 | **440** | 320 | 88 | (180, 484) |
| Btn_VehicleInfo | 4 | 20 | **544** | 320 | 88 | (180, 588) |

**公式：** `T(i) = 80 + 48 + i × 104 = 128 + i × 104`；`L = 20`。

**Unity（父级 NavButtonList，顶对齐）：**

| 节点 | anchorMin | anchorMax | pivot | anchoredPosition | sizeDelta |
|------|-----------|-----------|-------|------------------|-----------|
| Btn_Home | (0.5, 1) | (0.5, 1) | (0.5, 1) | (0, **-92**) | (320, 88) |
| Btn_Settings | (0.5, 1) | (0.5, 1) | (0.5, 1) | (0, **-196**) | (320, 88) |
| Btn_Camera | (0.5, 1) | (0.5, 1) | (0.5, 1) | (0, **-300**) | (320, 88) |
| Btn_Light | (0.5, 1) | (0.5, 1) | (0.5, 1) | (0, **-404**) | (320, 88) |
| Btn_VehicleInfo | (0.5, 1) | (0.5, 1) | (0.5, 1) | (0, **-508**) | (320, 88) |

**建议 commandId：**

| 按钮 | commandId |
|------|-----------|
| Btn_Home | `HMI.Page.OpenHome` |
| Btn_Settings | `HMI.Page.OpenSettings` |
| Btn_Camera | `HMI.Camera.Preset0` 或自定义 |
| Btn_Light / Btn_VehicleInfo | 预留 |

---

## 七、底部 6 色块精确表（2880 宽）

### 7.1 布局参数

| 项 | 值 |
|----|-----|
| Dock 屏幕区域 | L=0, T=**1440**, W=**2880**, H=**180** |
| Padding | 左 32，右 32，上 28，下 28 |
| 色块 | **112 × 112**，间距 **24** |
| 6 块总宽 | 6×112 + 5×24 = **792** |
| 内容区宽 | 2880 − 64 = **2816** |
| 左侧留白 | (2816 − 792) / 2 = **1012** |
| **第 1 块左缘 X（屏幕）** | **32 + 1012 = 1044** |

### 7.2 色块屏幕坐标

| 节点 | 索引 | 左缘 X | 中心 Cx | 右缘 X | T | W | H | 代表色 |
|------|------|--------|---------|--------|---|---|---|---|
| Swatch_White | 0 | 1044 | **1100** | 1156 | 1468 | 112 | 112 | #FFFFFF |
| Swatch_Black | 1 | 1180 | **1236** | 1292 | 1468 | 112 | 112 | #000000 |
| Swatch_Red | 2 | 1316 | **1372** | 1428 | 1468 | 112 | 112 | #FF0A00 |
| Swatch_Blue | 3 | 1452 | **1508** | 1564 | 1468 | 112 | 112 | #00D1FF |
| Swatch_Gray | 4 | 1588 | **1644** | 1700 | 1468 | 112 | 112 | #6C6C6C |
| Swatch_Yellow | 5 | 1724 | **1780** | 1836 | 1468 | 112 | 112 | #FFB800 |

**公式：**

- `左缘X(i) = 1044 + i × 136`
- `中心Cx(i) = 左缘X(i) + 56`
- `T = 1440 + 28 = 1468`

**commandId（与 CarRoot 预设索引一致）：**

| 节点 | commandId |
|------|-----------|
| Swatch_White | `HMI.CarPaint.Preset0` |
| Swatch_Black | `HMI.CarPaint.Preset1` |
| Swatch_Red | `HMI.CarPaint.Preset2` |
| Swatch_Blue | `HMI.CarPaint.Preset3` |
| Swatch_Gray | `HMI.CarPaint.Preset4` |
| Swatch_Yellow | `HMI.CarPaint.Preset5` |

### 7.3 色块手写锚点（父级 Dock，左下锚点，不用 LayoutGroup 时）

| 节点 | anchorMin | anchorMax | pivot | anchoredPosition | sizeDelta |
|------|-----------|-----------|-------|------------------|-----------|
| Swatch_White | (0, 0) | (0, 0) | (0, 0) | (1044, 28) | (112, 112) |
| Swatch_Black | (0, 0) | (0, 0) | (0, 0) | (1180, 28) | (112, 112) |
| Swatch_Red | (0, 0) | (0, 0) | (0, 0) | (1316, 28) | (112, 112) |
| Swatch_Blue | (0, 0) | (0, 0) | (0, 0) | (1452, 28) | (112, 112) |
| Swatch_Gray | (0, 0) | (0, 0) | (0, 0) | (1588, 28) | (112, 112) |
| Swatch_Yellow | (0, 0) | (0, 0) | (0, 0) | (1724, 28) | (112, 112) |

**色块中心 X 速查：** 1100 · 1236 · 1372 · 1508 · 1644 · 1780

---

## 八、设置页内部控件表

父级：`Page_Settings`（2880×1360）。

### 8.1 子容器

| 节点 | 屏幕 L | T | W | H | Unity（父 Settings 全拉伸） |
|------|--------|---|---|---|------------------------------|
| SettingsHeader | 0 | 80 | 2880 | 120 | offsetMax.y=0, offsetMin.y=**-120** |
| SettingsScrollView | 0 | 200 | 2880 | 1240 | offsetMin.y=0, offsetMax.y=**-120** |

### 8.2 Header 内控件

| 节点 | 屏幕 L | T | W | H | sizeDelta |
|------|--------|---|---|---|-----------|
| Btn_Back | 48 | 96 | 240 | 88 | (240, 88) |
| Txt_Title | 1200 | 104 | 480 | 72 | (480, 72) |

- 标题文案建议：「车辆设置」
- `Btn_Back` → commandId：`HMI.Page.OpenHome`

### 8.3 设置项行模板（ScrollView 内容）

左右边距 **96**，行高 **96**，行间距 **16**。

| 节点 | 屏幕 L | T（第 n 行） | W | H |
|------|--------|--------------|---|---|
| SettingRow_Template | 96 | **216 + (n−1)×112** | 2688 | 96 |

行内：`Txt_Label` 距行左 24，宽约 400；`Toggle_Value` 靠右，约 80×48。

---

## 九、页面显隐（方案 B）

| 状态 | 可见 |
|------|------|
| **Home** | TopStatusBar、LeftNavDock、ContentSafeArea+Page_Home、CarPaintColorDock；隐藏 Page_Settings |
| **Settings** | TopStatusBar、Page_Settings、CarPaintColorDock；隐藏 Page_Home；左栏被 Settings 遮住 |

`Push(Settings)` 仅隐藏同层 Page_Home；**不隐藏** Dock / TopBar（与 Page 平级）。

---

## 十、Raycast 与 3D 车模交互

| 区域 | Raycast | 相机拖拽 |
|------|---------|----------|
| TopStatusBar | 开 | 禁用 |
| LeftNav / Settings | 开 | 禁用 |
| Page_Home 透明区 | **关** | **允许**（中央 2520×1360） |
| CarPaintColorDock | 开 | 禁用 |
| Settings 全屏底图 | 开 | 禁用 |

配合 `UIInteractionArbiter` 使用。

---

## 十一、命令链路（按钮接线）

```
Button → UIButtonCommandBinder（commandId）
       → UICommandCenter
       → UICommandBridge
       → XxxUseCase
       → CarPaintSwitcher / MinoCameraController / UIPageController
```

车漆色块仅使用 `HMI.CarPaint.Preset0` ~ `Preset5`，无需 `Push(Page_CarPaint)`。

---

## 十二、代码侧需配合项（实施时）

1. `UIPageId` 增加 `Settings`（若尚未添加）。
2. `HmiUiCommandIds` 增加 `HMI.Page.OpenSettings`。
3. `UICommandBridge` 增加 Settings 页导航路由 + `UiPageNavigationUseCase`。
4. `CarRoot` 上 `CarPaintSwitcher` 预设顺序与色块索引 0~5 一致（菜单：**MinoHMI → CarPaint → Setup CarRoot Presets**）。
5. 场景仅 **1** 个 `EventSystem`（Bootstrap 或场景二选一）。
6. URP：**MSAA 关闭**，与 TAA 共存；性能档位主要调 `renderScale`。

---

## 十三、实施核对清单

- [ ] CanvasScaler = 2880×1620
- [ ] 使用 Prefab 变体，避免重建覆盖
- [ ] Page_Settings 在 Layer_Base，offset 见 §4.5
- [ ] 6 色块中心 X：1100, 1236, 1372, 1508, 1644, 1780
- [ ] 左栏按钮 T：128, 232, 336, 440, 544
- [ ] 单 EventSystem
- [ ] Play：Home 可拖车模、色条变色、进设置全屏、返回 Home

---

## 十四、相关文档与资源

| 路径 | 说明 |
|------|------|
| `Assets/_MinoHMI/Prefabs/UI/HMI_UIBootstrap.prefab` | UI 框架预制体模板 |
| `Assets/_MinoHMI/Prefabs/UI/HMI_UIBootstrap_使用说明.md` | Bootstrap 使用说明 |
| `Assets/_MinoHMI/URP_3D_HMI_UI_Framework.md` | 框架分层与性能策略 |
| `Assets/_MinoHMI/Scripts/UI/Core/UIPageId.cs` | 页面枚举 |
| `Assets/_MinoHMI/Scripts/UI/Application/HmiUiCommandIds.cs` | 命令 ID |

---

*文档版本：方案 B + TopStatusBar 80px + 2880×1620，与 CarPaintColorDock 常驻色条对齐。*
