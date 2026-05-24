using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MinoHMI.UI.Performance
{
    /// <summary>
    /// 单档 URP 质量参数配置。
    /// </summary>
    [CreateAssetMenu(
        fileName = "UrpQualityLevelProfile",
        menuName = "MinoHMI/UI/Performance/URP Quality Level Profile")]
    public class UrpQualityLevelProfile : ScriptableObject
    {
        [SerializeField] private string profileName = "Medium";
        [Tooltip("可选：使用该 URP Asset 整体切换档位（阴影等只读参数需在 Asset 内预配置）")]
        [SerializeField] private UniversalRenderPipelineAsset pipelineAsset;
        [SerializeField, Range(0.5f, 1.5f)] private float renderScale = 1f;
        [SerializeField] private int msaaSampleCount = 1;
        [SerializeField] private float downgradeFpsThreshold = 42f;
        [SerializeField] private float upgradeFpsThreshold = 57f;

        public string ProfileName => profileName;
        public UniversalRenderPipelineAsset PipelineAsset => pipelineAsset;
        public float RenderScale => renderScale;
        public int MsaaSampleCount => msaaSampleCount;
        public float DowngradeFpsThreshold => downgradeFpsThreshold;
        public float UpgradeFpsThreshold => upgradeFpsThreshold;
    }
}
