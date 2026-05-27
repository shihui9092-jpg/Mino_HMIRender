using UnityEngine;

namespace MinoHMI.Rendering
{
    /// <summary>
    /// 平面反射平面控制器
    /// 管理反射地面的材质和效果参数
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    [ExecuteAlways]
    public class PlanarReflectionPlane : MonoBehaviour
    {
        [Header("反射")]
        [Tooltip("反射强度")]
        [Range(0, 1)]
        public float reflectionIntensity = 0.5f;

        [Tooltip("反射模糊")]
        [Range(0, 1)]
        public float reflectionBlur = 0.0f;

        [Tooltip("反射色调")]
        public Color reflectionTint = Color.white;

        [Header("淡出设置")]
        [Tooltip("反射淡出起始距离")]
        public float fadeStart = 20f;

        [Tooltip("反射淡出结束距离")]
        public float fadeEnd = 50f;

        [Header("表面细节")]
        [Tooltip("粗糙度")]
        [Range(0, 1)]
        public float roughness = 0.1f;

        [Tooltip("菲涅尔强度")]
        [Range(0, 5)]
        public float fresnelPower = 2.0f;

        private Renderer planeRenderer;
        private MaterialPropertyBlock propertyBlock;
        private bool materialPropertiesDirty = true;

        private float cachedReflectionIntensity;
        private float cachedReflectionBlur;
        private Color cachedReflectionTint;
        private float cachedFadeStart;
        private float cachedFadeEnd;
        private float cachedRoughness;
        private float cachedFresnelPower;

        private static readonly int ReflectionIntensityID = Shader.PropertyToID("_ReflectionIntensity");
        private static readonly int ReflectionBlurID = Shader.PropertyToID("_ReflectionBlur");
        private static readonly int ReflectionTintID = Shader.PropertyToID("_ReflectionTint");
        private static readonly int FadeParamsID = Shader.PropertyToID("_ReflectionFadeParams");
        private static readonly int RoughnessID = Shader.PropertyToID("_Roughness");
        private static readonly int FresnelPowerID = Shader.PropertyToID("_FresnelPower");

        private void Awake()
        {
            planeRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            MarkMaterialPropertiesDirty();
        }

        private void Update()
        {
            if (HasReflectionParametersChanged())
            {
                MarkMaterialPropertiesDirty();
            }

            UpdateMaterialProperties();
        }

        private bool HasReflectionParametersChanged()
        {
            return reflectionIntensity != cachedReflectionIntensity ||
                   reflectionBlur != cachedReflectionBlur ||
                   reflectionTint != cachedReflectionTint ||
                   fadeStart != cachedFadeStart ||
                   fadeEnd != cachedFadeEnd ||
                   roughness != cachedRoughness ||
                   fresnelPower != cachedFresnelPower;
        }

        private void MarkMaterialPropertiesDirty()
        {
            materialPropertiesDirty = true;
        }

        /// <summary>
        /// 更新材质属性
        /// </summary>
        private void UpdateMaterialProperties()
        {
            if (planeRenderer == null || propertyBlock == null)
                return;

            if (!materialPropertiesDirty)
                return;

            materialPropertiesDirty = false;

            planeRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(ReflectionIntensityID, reflectionIntensity);
            propertyBlock.SetFloat(ReflectionBlurID, reflectionBlur);
            propertyBlock.SetColor(ReflectionTintID, reflectionTint);

            // xy: 淡出起止, z: 1/(end-start), w: 预留
            Vector4 fadeParams = new Vector4(
                fadeStart,
                fadeEnd,
                1.0f / Mathf.Max(0.01f, fadeEnd - fadeStart),
                0
            );
            propertyBlock.SetVector(FadeParamsID, fadeParams);

            propertyBlock.SetFloat(RoughnessID, roughness);
            propertyBlock.SetFloat(FresnelPowerID, fresnelPower);

            planeRenderer.SetPropertyBlock(propertyBlock);

            cachedReflectionIntensity = reflectionIntensity;
            cachedReflectionBlur = reflectionBlur;
            cachedReflectionTint = reflectionTint;
            cachedFadeStart = fadeStart;
            cachedFadeEnd = fadeEnd;
            cachedRoughness = roughness;
            cachedFresnelPower = fresnelPower;
        }

        /// <summary>
        /// 设置反射质量预设
        /// </summary>
        public void SetQualityPreset(ReflectionQuality quality)
        {
            switch (quality)
            {
                case ReflectionQuality.Low:
                    reflectionIntensity = 0.3f;
                    reflectionBlur = 0.5f;
                    roughness = 0.3f;
                    break;

                case ReflectionQuality.Medium:
                    reflectionIntensity = 0.5f;
                    reflectionBlur = 0.2f;
                    roughness = 0.15f;
                    break;

                case ReflectionQuality.High:
                    reflectionIntensity = 0.7f;
                    reflectionBlur = 0.0f;
                    roughness = 0.05f;
                    break;

                case ReflectionQuality.Ultra:
                    reflectionIntensity = 1.0f;
                    reflectionBlur = 0.0f;
                    roughness = 0.0f;
                    break;
            }

            MarkMaterialPropertiesDirty();
        }

        private void OnValidate()
        {
            fadeEnd = Mathf.Max(fadeStart + 0.1f, fadeEnd);
            MarkMaterialPropertiesDirty();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(10, 0.01f, 10));

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.up * 2f);

            Gizmos.color = Color.yellow;
            UnityEditor.Handles.color = new Color(1, 1, 0, 0.1f);
            UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, fadeStart);
            UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, fadeEnd);
        }
#endif
    }

    /// <summary>
    /// 反射质量等级
    /// </summary>
    public enum ReflectionQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }
}
