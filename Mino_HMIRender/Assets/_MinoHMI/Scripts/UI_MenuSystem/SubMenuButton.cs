using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MinoHMI.UI_MenuSystem
{
    /// <summary>
    /// 子菜单按钮：挂在 Btn_Sub_X 上；Button 仅存在于本节点。
    /// </summary>
    public class SubMenuButton : MonoBehaviour
    {
        [Header("结构引用")]
        [SerializeField] private Button subButton;
        [Tooltip("可选：图标 Image，节点名 Img_SubIcon")]
        [SerializeField] private Image subIcon;
        [Tooltip("可选：Legacy Text，节点名 Txt_SubLabel")]
        [SerializeField] private Text subLabel;
        [Tooltip("可选：TextMeshPro，节点名 TxtMeshPro_SubLabel")]
        [SerializeField] private TMP_Text subTmpLabel;
        [SerializeField] private Image buttonBackground;

        private int slotIndex = -1;
        private SubMenuItemConfig currentConfig;

        public event Action<SubMenuButton> SubButtonClicked;

        public int SlotIndex => slotIndex;
        public Button SubButton => subButton;
        public SubMenuItemConfig CurrentConfig => currentConfig;

        private void Reset()
        {
            ResolveReferences(includeInactive: true);
            EnsureButtonTargetGraphic();
        }

        private void Awake()
        {
            ResolveReferences(includeInactive: true);
            EnsureButtonTargetGraphic();
        }

        private void OnEnable()
        {
            if (subButton == null)
            {
                ResolveReferences(includeInactive: true);
            }

            if (subButton != null)
            {
                subButton.onClick.AddListener(HandleSubButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (subButton != null)
            {
                subButton.onClick.RemoveListener(HandleSubButtonClicked);
            }
        }

        private void HandleSubButtonClicked()
        {
            SubButtonClicked?.Invoke(this);
        }

        public void InitializeSlot(int index)
        {
            slotIndex = index;
        }

        /// <summary>
        /// 从 Btn_Sub_X 子树按标准命名自动绑定引用。
        /// </summary>
        public bool ResolveReferences(bool includeInactive = true)
        {
            Transform searchRoot = transform;

            if (subButton == null)
            {
                subButton = GetComponent<Button>();
            }

            if (buttonBackground == null)
            {
                buttonBackground = GetComponent<Image>();
            }

            if (subLabel == null)
            {
                subLabel = MenuSystemHierarchyUtility.FindNamedComponent<Text>(
                    searchRoot, SubMenuNodeNames.SubLabel, includeInactive);
            }

            if (subTmpLabel == null)
            {
                subTmpLabel = MenuSystemHierarchyUtility.FindNamedComponent<TMP_Text>(
                    searchRoot, SubMenuNodeNames.SubTmpLabel, includeInactive);
            }

            if (subTmpLabel == null)
            {
                subTmpLabel = MenuSystemHierarchyUtility.FindNamedComponent<TMP_Text>(
                    searchRoot, SubMenuNodeNames.SubTmpLabelAlias, includeInactive);
            }

            if (subIcon == null)
            {
                subIcon = MenuSystemHierarchyUtility.FindNamedComponent<Image>(
                    searchRoot, SubMenuNodeNames.SubIcon, includeInactive);
            }

            if (subIcon != null &&
                subButton != null &&
                subIcon.gameObject == subButton.gameObject)
            {
                subIcon = null;
            }

            EnsureButtonTargetGraphic();
            return subButton != null;
        }

        /// <summary>
        /// 根据槽位配置刷新显示；config 为 null 或 disabled 时隐藏。
        /// </summary>
        public void ApplyConfig(SubMenuItemConfig config)
        {
            ResolveReferences(includeInactive: true);
            currentConfig = config;

            if (config == null || !config.enabled)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            ApplyButtonSize(config.buttonSize);
            ApplyBackground(config.buttonBackgroundSprite);
            ApplyLabelDisplay(config);
            ApplyIconDisplay(config);
            EnsureButtonTargetGraphic();
        }

        public void HideSlot()
        {
            currentConfig = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 重置为 GridLayout 接管前的尺寸，避免切换 Profile 时布局残留。
        /// </summary>
        public void ResetLayoutForGrid()
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = Vector2.zero;
            }
        }

        private void ApplyButtonSize(Vector2 buttonSize)
        {
            if (buttonSize == Vector2.zero)
            {
                return;
            }

            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = buttonSize;
            }
        }

        private void ApplyBackground(Sprite backgroundSprite)
        {
            if (buttonBackground == null)
            {
                buttonBackground = GetComponent<Image>();
            }

            if (buttonBackground != null)
            {
                buttonBackground.sprite = MenuSystemSpriteUtility.ResolveOrWhite(backgroundSprite);
            }
        }

        private void ApplyLabelDisplay(SubMenuItemConfig config)
        {
            bool shouldShowLabel = config.showLabel;
            string displayText = config.labelText ?? string.Empty;
            bool useLegacyText = config.labelDisplayTarget == PrimaryLabelDisplayTarget.PrimaryLabel;

            if (subLabel != null)
            {
                subLabel.text = displayText;
                subLabel.color = config.labelColor;
                subLabel.gameObject.SetActive(useLegacyText && shouldShowLabel);
            }

            if (subTmpLabel != null)
            {
                subTmpLabel.text = displayText;
                subTmpLabel.color = config.tmpLabelColor;
                subTmpLabel.gameObject.SetActive(!useLegacyText && shouldShowLabel);
            }
        }

        private void ApplyIconDisplay(SubMenuItemConfig config)
        {
            if (subIcon == null)
            {
                return;
            }

            subIcon.sprite = MenuSystemSpriteUtility.ResolveOrWhite(config.iconSprite);
            subIcon.enabled = config.showIcon;
            subIcon.gameObject.SetActive(config.showIcon);
        }

        private void EnsureButtonTargetGraphic()
        {
            if (subButton == null)
            {
                return;
            }

            if (subButton.targetGraphic != null)
            {
                return;
            }

            if (buttonBackground == null)
            {
                buttonBackground = GetComponent<Image>();
            }

            if (buttonBackground != null)
            {
                if (buttonBackground.sprite == null)
                {
                    buttonBackground.sprite = MenuSystemSpriteUtility.GetWhiteSprite();
                }

                subButton.targetGraphic = buttonBackground;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences(includeInactive: true);
            EnsureButtonTargetGraphic();
            if (currentConfig != null)
            {
                ApplyConfig(currentConfig);
            }
        }
#endif
    }
}
