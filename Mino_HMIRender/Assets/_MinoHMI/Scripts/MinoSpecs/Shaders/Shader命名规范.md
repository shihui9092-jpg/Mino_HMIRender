# MinoHMI Shader 命名规范

> **已合并至上级文档**：[`../NAMING_CONVENTION.md`](../NAMING_CONVENTION.md)  
> 本文档保留 Shader 章节索引，完整 C# / Shader / HLSL 规范请以主文档为准。

---

## 快速跳转（主文档章节）

| 章节 | 内容 |
|------|------|
| §3 | Shader / ShaderLab 命名 |
| §4 | HLSL 命名 |
| §5 | C# ↔ Shader 映射 |
| §7 | 禁止事项 |
| §8 | 检查清单 |

---

## Shader 核心规则（摘要）

1. Property 标识符：`_PascalCase`（与 CBUFFER、PropertyToID 一致）
2. `[Header(...)]`：英文，如 `[Header(Base)]`
3. 引号显示名：模板 Shader 可用中文，工程模块用英文
4. HLSL 标识符：全部英文；`Attributes` / `Varyings`；`OS/WS/CS` 后缀
5. 局部变量：camelCase（`baseMap`、`color`）

详细说明、示例与踩坑记录见 [`NAMING_CONVENTION.md`](../NAMING_CONVENTION.md)。  
**Properties 块专项规范**见 [`SHADER_PROPERTIES_NAMING_CONVENTION.md`](SHADER_PROPERTIES_NAMING_CONVENTION.md)。

