using MinoHMI.UI.Application;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MinoHMI.MY26HMI.MaterialControl
{
    /// <summary>
    /// 材质变体切换用例，供 UI Button + UICommandBridge 调用。
    /// </summary>
    [DisallowMultipleComponent]
    [MovedFrom("MinoHMI.MY26HMI.TimeAndWeather.TimeWeatherVariantApplyUseCase")]
    public class MaterialVariantApplyUseCase : MonoBehaviour, IUiCommandExecutor
    {
        [SerializeField]
        private MaterialVariantCatalog materialCatalog;

        [SerializeField]
        [Tooltip("变体索引（0 对应 Element 1 / 变体 1）")]
        private int variantIndex;

        [SerializeField]
        private bool useSmoothTransition = true;

        public int VariantIndex => variantIndex;

        public void Bind(MaterialVariantCatalog catalog)
        {
            materialCatalog = catalog;
        }

        public void ExecuteCommand()
        {
            if (materialCatalog == null)
            {
                Debug.LogWarning($"[{nameof(MaterialVariantApplyUseCase)}] 未绑定 {nameof(MaterialVariantCatalog)}。", this);
                return;
            }

            if (!materialCatalog.TryApplyVariantToAllCategories(variantIndex, useSmoothTransition, out string resultMessage))
            {
                Debug.LogWarning($"[{nameof(MaterialVariantApplyUseCase)}] {resultMessage}", this);
                return;
            }

            if (!string.IsNullOrWhiteSpace(resultMessage))
            {
                Debug.Log($"[{nameof(MaterialVariantApplyUseCase)}] {resultMessage}", this);
            }
        }
    }
}
