using UnityEngine;

namespace MinoHMI.Templates
{
    /// <summary>
    /// MinoHMI 组件脚本模板
    /// 复制本文件并按模块重命名类名与文件名，删除 Template 相关注释块即可迭代开发。
    /// 命名规范见同目录：Scrips命名规范.md
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public class MinoHMIComponentTemplate : MonoBehaviour
    {
        private const string LogTag = "[MinoHMIComponentTemplate]";
        private const int MinUpdateIntervalFrames = 1;
        private const float DefaultIntensity = 0.5f;

        #region Inspector - 引用

        [Header("引用")]
        [Tooltip("可选的全局配置资源")]
        public MinoHMISettingsTemplate settings;

        [Tooltip("目标渲染器(留空则使用自身 Renderer)")]
        public Renderer targetRenderer;

        #endregion

        #region Inspector - 基础参数

        [Header("基础参数")]
        [Tooltip("是否启用本组件逻辑")]
        public bool enableFeature = true;

        [Tooltip("效果强度")]
        [Range(0f, 1f)]
        public float effectIntensity = DefaultIntensity;

        [Tooltip("效果色调")]
        public Color effectTint = Color.white;

        [Tooltip("淡入淡出起始距离")]
        public float fadeStart = 10f;

        [Tooltip("淡入淡出结束距离")]
        public float fadeEnd = 30f;

        #endregion

        #region Inspector - 运行时

        [Header("运行时")]
        [Tooltip("每 N 帧更新一次(1=每帧)")]
        [Range(1, 10)]
        public int updateIntervalFrames = 1;

        [Tooltip("是否在编辑器下同步预览")]
        public bool previewInEditor = true;

        #endregion

        #region 私有字段

        private Renderer cachedRenderer;
        private MaterialPropertyBlock materialPropertyBlock;
        private bool runtimeStateDirty = true;
        private int frameCounter;

        private float cachedEffectIntensity;
        private Color cachedEffectTint;
        private float cachedFadeStart;
        private float cachedFadeEnd;

        // 与 Shader Property 名严格一致（复制后改为你的 Shader 属性名）
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int ExampleFadeParamsID = Shader.PropertyToID("_ExampleFadeParams");

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            cachedRenderer = targetRenderer != null ? targetRenderer : GetComponent<Renderer>();
            materialPropertyBlock = new MaterialPropertyBlock();
            MarkRuntimeStateDirty();
        }

        private void OnEnable()
        {
            frameCounter = 0;
            MarkRuntimeStateDirty();
        }

        private void OnDisable()
        {
            // 退出时清理运行时状态(按需扩展)
        }

        private void Update()
        {
            if (!enableFeature)
                return;

            if (!previewInEditor && !Application.isPlaying)
                return;

            if (!ShouldUpdateThisFrame())
                return;

            if (HasPublicParametersChanged())
            {
                MarkRuntimeStateDirty();
            }

            ApplyRuntimeState();
        }

        private void OnValidate()
        {
            fadeEnd = Mathf.Max(fadeStart + 0.01f, fadeEnd);
            effectIntensity = Mathf.Clamp01(effectIntensity);
            updateIntervalFrames = Mathf.Clamp(updateIntervalFrames, MinUpdateIntervalFrames, 10);
            MarkRuntimeStateDirty();
        }

        #endregion

        #region 公开 API

        /// <summary>
        /// 应用质量预设
        /// </summary>
        public void ApplyQualityPreset(TemplateQuality quality)
        {
            switch (quality)
            {
                case TemplateQuality.Low:
                    effectIntensity = 0.3f;
                    updateIntervalFrames = 3;
                    break;
                case TemplateQuality.Medium:
                    effectIntensity = 0.5f;
                    updateIntervalFrames = 2;
                    break;
                case TemplateQuality.High:
                    effectIntensity = 0.8f;
                    updateIntervalFrames = 1;
                    break;
            }

            MarkRuntimeStateDirty();
            Debug.Log($"{LogTag} 已应用质量预设: {quality}");
        }

        /// <summary>
        /// 设置效果强度
        /// </summary>
        public void SetEffectIntensity(float intensity)
        {
            effectIntensity = Mathf.Clamp01(intensity);
            MarkRuntimeStateDirty();
        }

        /// <summary>
        /// 切换功能开关
        /// </summary>
        public void SetFeatureEnabled(bool enabled)
        {
            enableFeature = enabled;
            Debug.Log($"{LogTag} 功能{(enabled ? "已启用" : "已禁用")}");
        }

        #endregion

        #region 私有逻辑

        private bool ShouldUpdateThisFrame()
        {
            int interval = Mathf.Clamp(updateIntervalFrames, MinUpdateIntervalFrames, 10);
            frameCounter++;

            if (frameCounter < interval)
                return false;

            frameCounter = 0;
            return true;
        }

        private bool HasPublicParametersChanged()
        {
            return effectIntensity != cachedEffectIntensity ||
                   effectTint != cachedEffectTint ||
                   cachedFadeStart != fadeStart ||
                   cachedFadeEnd != fadeEnd;
        }

        private void MarkRuntimeStateDirty()
        {
            runtimeStateDirty = true;
        }

        /// <summary>
        /// 将公开参数写入 MaterialPropertyBlock
        /// </summary>
        private void ApplyRuntimeState()
        {
            if (!runtimeStateDirty)
                return;

            if (!TryGetTargetRenderer(out Renderer renderer))
                return;

            runtimeStateDirty = false;

            renderer.GetPropertyBlock(materialPropertyBlock);

            Color finalColor = effectTint * effectIntensity;
            materialPropertyBlock.SetColor(BaseColorID, finalColor);

            // 示例：Vector 打包参数 (xy: 起止, z: 斜率, w: 预留)
            float fadeRange = Mathf.Max(0.01f, fadeEnd - fadeStart);
            Vector4 fadeParams = new Vector4(fadeStart, fadeEnd, 1f / fadeRange, 0f);
            materialPropertyBlock.SetVector(ExampleFadeParamsID, fadeParams);

            renderer.SetPropertyBlock(materialPropertyBlock);

            cachedEffectIntensity = effectIntensity;
            cachedEffectTint = effectTint;
            cachedFadeStart = fadeStart;
            cachedFadeEnd = fadeEnd;
        }

        /// <summary>
        /// 安全获取目标 Renderer
        /// </summary>
        private bool TryGetTargetRenderer(out Renderer renderer)
        {
            if (cachedRenderer == null)
            {
                cachedRenderer = targetRenderer != null ? targetRenderer : GetComponent<Renderer>();
            }

            renderer = cachedRenderer;
            if (renderer == null)
            {
                Debug.LogWarning($"{LogTag} 未找到 Renderer 组件");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从 Settings 资源读取配置(示例)
        /// </summary>
        private bool TryApplySettingsFromAsset()
        {
            if (settings == null)
            {
                Debug.LogError($"{LogTag} 缺少 Settings 配置资源");
                return false;
            }

            effectIntensity = settings.defaultIntensity;
            updateIntervalFrames = settings.defaultUpdateInterval;
            MarkRuntimeStateDirty();
            return true;
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, fadeStart);

            Gizmos.color = new Color(1f, 1f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, fadeEnd);
        }
#endif
    }

    /// <summary>
    /// 模板用质量等级
    /// </summary>
    public enum TemplateQuality
    {
        Low,
        Medium,
        High
    }
}
