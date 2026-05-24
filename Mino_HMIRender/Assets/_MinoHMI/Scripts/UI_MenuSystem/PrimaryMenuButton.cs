using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MinoHMI.UI_MenuSystem
{
    /// <summary>
    /// 主菜单文字显示来源。
    /// </summary>
    public enum PrimaryLabelDisplayTarget
    {
        PrimaryLabel = 0,
        PrimaryTmpLabel = 1
    }

    /// <summary>
    /// 主菜单项：挂在每个主菜单项根节点上（Primary_Menu 下的单个条目）；Button 在子节点 Btn_Primary。
    /// </summary>
    public class PrimaryMenuButton : MonoBehaviour
    {
        [Header("结构引用")]
        [SerializeField] private RectTransform primaryMenuContainer;
        [SerializeField] private Button primaryButton;
        [Tooltip("可选：图标 Image，节点名 Img_PrimaryIcon")]
        [SerializeField] private Image primaryIcon;
        [Tooltip("可选：Legacy Text，节点名 Txt_PrimaryLabel")]
        [SerializeField] private Text primaryLabel;
        [Tooltip("可选：TextMeshPro，节点名 TxtMeshPro_PrimaryLabel")]
        [SerializeField] private TMP_Text primaryTmpLabel;

        [Header("显示内容")]
        [Tooltip("选择使用 Legacy Text 还是 TextMeshPro 显示文字")]
        [SerializeField] private PrimaryLabelDisplayTarget labelDisplayTarget = PrimaryLabelDisplayTarget.PrimaryLabel;
        [SerializeField] private string labelText = "主菜单";
        [Tooltip("Txt_PrimaryLabel（Legacy Text）文字颜色")]
        [SerializeField] private Color primaryLabelColor = Color.white;
        [Tooltip("TxtMeshPro_PrimaryLabel（TextMeshPro）文字颜色")]
        [SerializeField] private Color primaryTmpLabelColor = Color.white;
        [SerializeField] private Sprite iconSprite;
        [SerializeField] private bool showLabel = true;
        [SerializeField] private bool showIcon = true;

        [Header("子菜单配置")]
        [SerializeField] private PrimaryMenuProfile menuProfile = new PrimaryMenuProfile();

        [Header("交互")]
        [SerializeField] private bool interactable = true;
        [SerializeField] private UnityEvent onPrimaryClicked;

        private MenuSystemController menuSystem;

        public RectTransform PrimaryMenuContainer => primaryMenuContainer;
        public Button PrimaryButton => primaryButton;
        public Text PrimaryLabel => primaryLabel;
        public TMP_Text PrimaryTmpLabel => primaryTmpLabel;
        public Image PrimaryIcon => primaryIcon;
        public string LabelText => labelText;
        public Color PrimaryLabelColor => primaryLabelColor;
        public Color PrimaryTmpLabelColor => primaryTmpLabelColor;
        public PrimaryLabelDisplayTarget LabelDisplayTarget => labelDisplayTarget;
        public PrimaryMenuProfile MenuProfile => menuProfile;
        public Sprite IconSprite => iconSprite;
        public UnityEvent PrimaryClicked => onPrimaryClicked;

        private void Reset()
        {
            ResolveReferences(includeInactive: true);
            EnsureButtonTargetGraphic();
            ApplyDisplay();
        }

        private void Awake()
        {
            ResolveMenuSystem();
            ResolveReferences(includeInactive: true);
            EnsureButtonTargetGraphic();
            ApplyDisplay();
            ApplyInteractable();
            EnsureMenuSystem();
        }

        public void BindMenuSystem(MenuSystemController controller)
        {
            menuSystem = controller;
        }

        private void ResolveMenuSystem()
        {
            if (menuSystem == null)
            {
                menuSystem = GetComponentInParent<MenuSystemController>();
            }
        }

        private void OnEnable()
        {
            ResolveReferences(includeInactive: true);

            if (primaryButton != null)
            {
                primaryButton.onClick.RemoveListener(HandlePrimaryClicked);
                primaryButton.onClick.AddListener(HandlePrimaryClicked);
            }
        }

        private void OnDisable()
        {
            if (primaryButton != null)
            {
                primaryButton.onClick.RemoveListener(HandlePrimaryClicked);
            }
        }

        /// <summary>
        /// 从当前主菜单项子树按标准命名自动绑定引用（不会跨条目查找 sibling）。
        /// </summary>
        public bool ResolveReferences(bool includeInactive = true)
        {
            ResolvePrimaryMenuContainer();
            SanitizeCrossItemReferences();

            Transform searchRoot = transform;

            if (primaryButton == null)
            {
                primaryButton = GetComponent<Button>();
            }

            if (primaryButton == null)
            {
                primaryButton = FindNamedComponent<Button>(searchRoot, PrimaryMenuNodeNames.PrimaryButton, includeInactive);
            }

            if (primaryLabel == null)
            {
                primaryLabel = FindNamedComponent<Text>(searchRoot, PrimaryMenuNodeNames.PrimaryLabel, includeInactive);
            }

            if (primaryTmpLabel == null)
            {
                primaryTmpLabel = FindNamedComponent<TMP_Text>(searchRoot, PrimaryMenuNodeNames.PrimaryTmpLabel, includeInactive);
            }

            if (primaryIcon == null)
            {
                primaryIcon = FindNamedComponent<Image>(searchRoot, PrimaryMenuNodeNames.PrimaryIcon, includeInactive);
            }

            // 避免误绑 Btn_Primary 上的 Image
            if (primaryIcon != null &&
                primaryButton != null &&
                primaryIcon.gameObject == primaryButton.gameObject)
            {
                primaryIcon = null;
            }

            EnsureButtonTargetGraphic();
            return primaryButton != null;
        }

        /// <summary>
        /// 清除绑定到其他主菜单条目的陈旧引用。
        /// </summary>
        private void SanitizeCrossItemReferences()
        {
            if (primaryButton != null && !IsUnderSelf(primaryButton.transform))
            {
                primaryButton.onClick.RemoveListener(HandlePrimaryClicked);
                primaryButton = null;
            }

            if (primaryLabel != null && !IsUnderSelf(primaryLabel.transform))
            {
                primaryLabel = null;
            }

            if (primaryTmpLabel != null && !IsUnderSelf(primaryTmpLabel.transform))
            {
                primaryTmpLabel = null;
            }

            if (primaryIcon != null && !IsUnderSelf(primaryIcon.transform))
            {
                primaryIcon = null;
            }
        }

        private bool IsUnderSelf(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            return target == transform || target.IsChildOf(transform);
        }

        private void ResolvePrimaryMenuContainer()
        {
            if (primaryMenuContainer != null)
            {
                return;
            }

            primaryMenuContainer = transform as RectTransform;
        }

        /// <summary>
        /// 将 labelText / iconSprite 应用到可选文字与图标节点。
        /// </summary>
        public void ApplyDisplay()
        {
            bool shouldShowLabel = showLabel;
            string displayText = labelText ?? string.Empty;

            ApplyLabelDisplay(displayText, shouldShowLabel);

            if (primaryIcon != null)
            {
                primaryIcon.sprite = MenuSystemSpriteUtility.ResolveOrWhite(iconSprite);
                primaryIcon.enabled = showIcon;
                primaryIcon.gameObject.SetActive(showIcon);
            }
        }

        public void SetLabelDisplayTarget(PrimaryLabelDisplayTarget target, bool refreshDisplay = true)
        {
            labelDisplayTarget = target;
            if (refreshDisplay)
            {
                ApplyDisplay();
            }
        }

        private void ApplyLabelDisplay(string displayText, bool shouldShowLabel)
        {
            bool useLegacyText = labelDisplayTarget == PrimaryLabelDisplayTarget.PrimaryLabel;

            if (primaryLabel != null)
            {
                primaryLabel.text = displayText;
                primaryLabel.color = primaryLabelColor;
                primaryLabel.gameObject.SetActive(useLegacyText && shouldShowLabel);
            }

            if (primaryTmpLabel != null)
            {
                primaryTmpLabel.text = displayText;
                primaryTmpLabel.color = primaryTmpLabelColor;
                primaryTmpLabel.gameObject.SetActive(!useLegacyText && shouldShowLabel);
            }
        }

        public void SetPrimaryLabelColor(Color color, bool refreshDisplay = true)
        {
            primaryLabelColor = color;
            if (refreshDisplay)
            {
                ApplyDisplay();
            }
        }

        public void SetPrimaryTmpLabelColor(Color color, bool refreshDisplay = true)
        {
            primaryTmpLabelColor = color;
            if (refreshDisplay)
            {
                ApplyDisplay();
            }
        }

        public void SetLabelText(string text, bool refreshDisplay = true)
        {
            labelText = text ?? string.Empty;
            if (refreshDisplay)
            {
                ApplyDisplay();
            }
        }

        public void SetIconSprite(Sprite sprite, bool refreshDisplay = true)
        {
            iconSprite = sprite;
            if (refreshDisplay)
            {
                ApplyDisplay();
            }
        }

        public void SetInteractable(bool enabled)
        {
            interactable = enabled;
            ApplyInteractable();
        }

        public void SetShowLabel(bool visible)
        {
            showLabel = visible;
            ApplyDisplay();
        }

        public void SetShowIcon(bool visible)
        {
            showIcon = visible;
            ApplyDisplay();
        }

        private void HandlePrimaryClicked()
        {
            onPrimaryClicked?.Invoke();
            EnsureMenuSystem();
            menuSystem?.OnPrimaryMenuClicked(this);
        }

        /// <summary>
        /// 确保已绑定 MenuSystemController；场景未挂载时在 UI_MenuSystem 上自动补全。
        /// </summary>
        private void EnsureMenuSystem()
        {
            ResolveMenuSystem();
            if (menuSystem != null)
            {
                return;
            }

            Transform cursor = transform.parent;
            while (cursor != null)
            {
                if (string.Equals(cursor.name, PrimaryMenuNodeNames.MenuSystemRoot, StringComparison.Ordinal))
                {
                    menuSystem = cursor.GetComponent<MenuSystemController>();
                    if (menuSystem == null)
                    {
                        menuSystem = cursor.gameObject.AddComponent<MenuSystemController>();
                    }

                    menuSystem.RegisterPrimaryMenuButton(this);
                    return;
                }

                cursor = cursor.parent;
            }
        }

        private void ApplyInteractable()
        {
            if (primaryButton != null)
            {
                primaryButton.interactable = interactable;
            }
        }

        private void EnsureButtonTargetGraphic()
        {
            if (primaryButton == null)
            {
                return;
            }

            if (primaryButton.targetGraphic != null)
            {
                return;
            }

            Image buttonImage = primaryButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (buttonImage.sprite == null)
                {
                    buttonImage.sprite = MenuSystemSpriteUtility.GetWhiteSprite();
                }

                primaryButton.targetGraphic = buttonImage;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences(includeInactive: true);
            EnsureButtonTargetGraphic();
            ApplyDisplay();

            if (Application.isPlaying)
            {
                ResolveMenuSystem();
                menuSystem?.RefreshSubMenuIfOwnedBy(this);
            }
        }
#endif

        private static T FindNamedComponent<T>(Transform root, string nodeName, bool includeInactive)
            where T : Component
        {
            if (root == null)
            {
                return null;
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (!string.Equals(candidate.name, nodeName, StringComparison.Ordinal))
                {
                    continue;
                }

                T component = candidate.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
    }
}
