# MinoHMI C# 代码命名规范

> 适用范围：`MinoRenderer/` 及子模块（含 `PlanarReflection/`）Runtime / Editor / Examples 脚本。  
> 与 [`../Shaders/Templates/SHADER_PROPERTIES_NAMING_CONVENTION.md`](../Shaders/Templates/SHADER_PROPERTIES_NAMING_CONVENTION.md) 配套：Shader 引号内中文，C# Inspector 中文，**标识符全英文**。

---

## 1. 总则

**编译器识别的名字用英文；给人看的字符串用中文。**

| 层级 | 语言 | 风格 | 可否修改 |
|------|------|------|----------|
| 命名空间 / 类 / 结构体 / 枚举 | 英文 | `PascalCase` | 谨慎 |
| 枚举成员 | 英文 | `PascalCase` | 谨慎（switch 引用） |
| public 序列化字段 | 英文 | `camelCase` | **不可随意改**（Prefab 序列化） |
| private 字段 / 局部变量 | 英文 | `camelCase` | 随时 |
| 方法 / 参数 | 英文 | `PascalCase` / `camelCase` | 随时 |
| `PropertyToID("...")` | 英文 | 与 Shader `_Property` 一致 | 仅随 Shader 同步改 |
| `[Header]` / `[Tooltip]` | **中文** | 引号字符串 | 随时 |
| `/// summary` / `//` 注释 | **中文** | — | 随时 |
| `Debug.Log` / OnGUI / Editor UI | **中文** | — | 随时 |
| `[CreateAssetMenu] menuName` | **中文路径** | 如 `MinoHMI/渲染/...` | 随时 |
| `[MenuItem]` 路径 | **中文** | 如 `MinoHMI/工具/...` | 随时 |

---

## 2. 命名空间

```csharp
namespace MinoHMI.Rendering { }
namespace MinoHMI.Rendering.Examples { }
namespace MinoHMI.Rendering.Editor { }
```

---

## 3. 类型命名

| 类型 | 规则 | 示例 |
|------|------|------|
| MonoBehaviour | 功能 + 角色 | `PlanarReflectionCamera` |
| ScriptableObject | 功能 + Settings | `PlanarReflectionSettings` |
| EditorWindow | 功能 + Wizard/Editor | `PlanarReflectionSetupWizard` |
| struct 配置 | 功能 + Settings | `ReflectionQualitySettings` |
| enum | 领域名词 | `ReflectionQuality`、`PerformanceLevel` |

---

## 4. 字段与常量

```csharp
// 公开序列化
public float reflectionIntensity = 0.5f;

// 私有
private Camera reflectionCamera;
private bool materialPropertiesDirty;

// 缓存（脏标记优化）
private float cachedReflectionIntensity;

// PropertyToID
private static readonly int ReflectionIntensityID = Shader.PropertyToID("_ReflectionIntensity");

// 常量
private const int MinReflectionTextureSize = 4;
private const float AutoOptimizeCooldownSeconds = 2f;
```

| 规则 | 说明 |
|------|------|
| 布尔 | `enable`、`is`、`has`、`Dirty` 后缀 | `enableReflection`、`materialPropertiesDirty` |
| 缓存 | `cached` + 语义 | `cachedFadeStart` |
| **禁止** | 中文 private / public 字段名 | ❌ `平面渲染器` |

---

## 5. 方法命名

| 规则 | 示例 |
|------|------|
| 公开方法 | `ApplyQualitySettings`、`SetQualityPreset` |
| 私有更新 | `UpdateMaterialProperties` |
| 布尔查询 | `IsBaseRenderCamera`、`HasReflectionParametersChanged` |
| Try 模式 | `TryGetReflectionTextureSize` |
| 标记副作用 | `MarkMaterialPropertiesDirty` |

Unity 生命周期方法保持 Unity 约定：`Awake`、`Update`、`OnValidate`。

---

## 6. Inspector 中文（与 Shader 对齐）

组件参数与 Shader Properties 显示名语义一致：

| Shader 显示名 | C# Tooltip 示例 |
|---------------|-----------------|
| 反射强度 | `[Tooltip("反射强度")]` |
| 反射模糊 | `[Tooltip("反射模糊")]` |
| 反射色调 | `[Tooltip("反射色调")]` |

```csharp
[Header("反射")]
[Tooltip("反射强度")]
[Range(0, 1)]
public float reflectionIntensity = 0.5f;
```

---

## 7. 日志与 UI 文案

```csharp
Debug.Log("[PlanarReflectionManager] 已初始化");
Debug.LogError("[PlanarReflectionManager] 缺少反射设置配置!");
GUI.Label(rect, "反射状态: 启用", labelStyle);
EditorGUILayout.LabelField("设置反射相机", EditorStyles.boldLabel);
```

---

## 8. 与 Shader 联动

| C# | Shader | 关系 |
|----|--------|------|
| `reflectionIntensity` | `_ReflectionIntensity` | 语义对应，命名独立 |
| `ReflectionIntensityID` | `"_ReflectionIntensity"` | **字符串必须完全一致** |
| `fadeStart` / `fadeEnd` | `_ReflectionFadeParams` | 脚本打包 Vector4 |

---

## 9. 禁止事项

1. 中文类名、方法名、字段名（含 private）
2. 修改 public 序列化字段名不迁移 Prefab
3. `PropertyToID` 与 Shader Property 不一致
4. 仅改 Tooltip 不同步文档

---

## 10. 新建脚本检查清单

- [ ] 命名空间 `MinoHMI.Rendering`（或 `.Examples` / `.Editor`）
- [ ] 类型名 PascalCase 英文
- [ ] public 字段 camelCase 英文
- [ ] private 字段 camelCase 英文
- [ ] `[Header]` / `[Tooltip]` 中文
- [ ] 注释中文
- [ ] PropertyToID 与 Shader 同步
- [ ] Log / Editor 文案中文

---

## 11. 参考文件

| 文件 | 说明 |
|------|------|
| `MinoHMIComponentTemplate.cs` | **复制起点**：MonoBehaviour 全结构模板 |
| `MinoHMISettingsTemplate.cs` | **复制起点**：ScriptableObject 配置模板 |
| `PlanarReflectionPlane.cs` | 生产环境参考 |
| `PlanarReflectionSetupWizard.cs` | Editor 中文 UI 参考 |
| `../MinoRenderer/Shaders/Templates/SHADER_PROPERTIES_NAMING_CONVENTION.md` | Shader Properties 规范 |

---

## 12. 模板使用方式

1. 复制 `MinoHMIComponentTemplate.cs` 到目标模块目录
2. 重命名类名、文件名、命名空间（如 `MinoHMI.Rendering`）
3. 删除 `[DisallowMultipleComponent]` 等不需要的特性
4. 按 Shader 实际 Property 更新 `PropertyToID` 常量
5. 对照 §10 检查清单逐项确认

---

*与 MinoHMI URP 14 工程同步。*
