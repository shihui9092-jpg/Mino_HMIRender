using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MinoHMI.UI_MenuSystem
{
    /// <summary>
    /// 子菜单弹窗：Instant 显示/隐藏，刷新预置 Btn_Sub 槽位。
    /// </summary>
    public class SubMenuPopup : MonoBehaviour
    {
        [Header("结构引用")]
        [SerializeField] private RectTransform popupRoot;
        [SerializeField] private Image subMenuBackground;
        [SerializeField] private RectTransform subMenuList;
        [SerializeField] private Image subMenuArrow;

        private readonly List<SubMenuButton> subMenuButtons = new List<SubMenuButton>();
        private PrimaryMenuButton currentPrimaryOwner;
        private bool isOpen;

        public bool IsOpen => isOpen;
        public PrimaryMenuButton CurrentPrimaryOwner => currentPrimaryOwner;

        public event Action<PrimaryMenuButton, SubMenuButton, SubMenuItemConfig, int> SubItemClicked;

        private void Awake()
        {
            ResolveReferences(includeInactive: true);
            CloseImmediate();
        }

        private void Reset()
        {
            ResolveReferences(includeInactive: true);
        }

        /// <summary>
        /// 解析 SubMenu_Popup 结构并收集 Btn_Sub 槽位。
        /// </summary>
        public bool ResolveReferences(bool includeInactive = true)
        {
            if (popupRoot == null)
            {
                popupRoot = transform as RectTransform;
            }

            if (subMenuBackground == null)
            {
                subMenuBackground = MenuSystemHierarchyUtility.FindNamedComponent<Image>(
                    transform, SubMenuNodeNames.SubMenuBackground, includeInactive);
            }

            if (subMenuList == null)
            {
                Transform listTransform = MenuSystemHierarchyUtility.FindNamedTransform(
                    transform, SubMenuNodeNames.SubMenuList, includeInactive);
                subMenuList = listTransform as RectTransform;
            }

            if (subMenuArrow == null)
            {
                subMenuArrow = MenuSystemHierarchyUtility.FindNamedComponent<Image>(
                    transform, SubMenuNodeNames.SubMenuArrow, includeInactive);
            }

            if (subMenuArrow == null)
            {
                subMenuArrow = MenuSystemHierarchyUtility.FindNamedComponent<Image>(
                    transform, SubMenuNodeNames.SubMenuArrowAlias, includeInactive);
            }

            CollectSubMenuButtons(includeInactive);
            return popupRoot != null && subMenuButtons.Count > 0;
        }

        public void Open(PrimaryMenuButton primaryOwner, PrimaryMenuProfile profile)
        {
            if (primaryOwner == null || profile == null)
            {
                return;
            }

            ResolveReferences(includeInactive: true);
            currentPrimaryOwner = primaryOwner;
            SetPopupActive(true);
            ApplyPopupTransform(primaryOwner, profile);
            ResetSubMenuLayoutState();
            ApplySubItemConfigs(profile);
            ForceRefreshLayout();
            ApplyPopupBackground(profile);
            ApplyPopupArrow(profile);
            Canvas.ForceUpdateCanvases();
            isOpen = true;
        }

        /// <summary>
        /// 弹窗已打开时，重新应用当前主菜单的 Profile（Inspector 改 offset 等后立即生效）。
        /// </summary>
        public void RefreshCurrent()
        {
            if (!isOpen || currentPrimaryOwner == null)
            {
                return;
            }

            PrimaryMenuProfile profile = currentPrimaryOwner.MenuProfile;
            if (profile == null)
            {
                return;
            }

            ApplyPopupTransform(currentPrimaryOwner, profile);
            ResetSubMenuLayoutState();
            ApplySubItemConfigs(profile);
            ForceRefreshLayout();
            ApplyPopupBackground(profile);
            ApplyPopupArrow(profile);
            Canvas.ForceUpdateCanvases();
        }

        public void CloseImmediate()
        {
            currentPrimaryOwner = null;
            isOpen = false;
            SetPopupActive(false);
        }

        public bool IsOwnedBy(PrimaryMenuButton primaryOwner)
        {
            return isOpen && currentPrimaryOwner == primaryOwner;
        }

        private void ApplyPopupTransform(PrimaryMenuButton primaryOwner, PrimaryMenuProfile profile)
        {
            if (popupRoot == null || profile == null)
            {
                return;
            }

            if (profile.anchorMode == SubMenuPopupAnchorMode.FixedAnchoredPosition)
            {
                popupRoot.anchoredPosition = profile.popupAnchoredPosition;
                return;
            }

            RectTransform primaryRect = primaryOwner != null ? primaryOwner.transform as RectTransform : null;
            RectTransform popupParent = popupRoot.parent as RectTransform;
            if (primaryRect == null || popupParent == null)
            {
                popupRoot.anchoredPosition = profile.popupOffset;
                return;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, primaryRect.position);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    popupParent, screenPoint, null, out Vector2 localPoint))
            {
                popupRoot.anchoredPosition = localPoint + profile.popupOffset;
            }
            else
            {
                popupRoot.anchoredPosition = primaryRect.anchoredPosition + profile.popupOffset;
            }
        }

        /// <summary>
        /// 切换 Profile 前重置子按钮布局，避免上一次尺寸/位置残留。
        /// </summary>
        private void ResetSubMenuLayoutState()
        {
            for (int i = 0; i < subMenuButtons.Count; i++)
            {
                SubMenuButton subMenuButton = subMenuButtons[i];
                if (subMenuButton == null)
                {
                    continue;
                }

                subMenuButton.ResetLayoutForGrid();
            }
        }

        private void ApplyPopupBackground(PrimaryMenuProfile profile)
        {
            if (subMenuBackground == null)
            {
                return;
            }

            if (!profile.showPopupBackground)
            {
                subMenuBackground.gameObject.SetActive(false);
                return;
            }

            subMenuBackground.sprite = MenuSystemSpriteUtility.ResolveOrWhite(profile.popupBackground);
            subMenuBackground.enabled = true;
            subMenuBackground.gameObject.SetActive(true);
            subMenuBackground.transform.SetAsFirstSibling();
            FitBackgroundToVisibleSubButtons(profile);
        }

        private void ApplyPopupArrow(PrimaryMenuProfile profile)
        {
            if (subMenuArrow == null)
            {
                return;
            }

            subMenuArrow.gameObject.SetActive(profile.showArrow);
            if (!profile.showArrow)
            {
                return;
            }

            if (subMenuArrow.sprite == null)
            {
                subMenuArrow.sprite = MenuSystemSpriteUtility.GetWhiteSprite();
            }

            subMenuArrow.rectTransform.anchoredPosition = profile.arrowOffset;
        }

        /// <summary>
        /// 将背景 RectTransform 适配为当前可见子按钮的包围区域（含 padding 与位置微调）。
        /// </summary>
        private void FitBackgroundToVisibleSubButtons(PrimaryMenuProfile profile)
        {
            if (subMenuBackground == null || popupRoot == null)
            {
                return;
            }

            if (!TryGetVisibleSubButtonsBounds(out Vector2 boundsMin, out Vector2 boundsMax))
            {
                subMenuBackground.gameObject.SetActive(false);
                return;
            }

            Vector2 padding = profile.popupBackgroundPadding;
            boundsMin.x -= padding.x;
            boundsMin.y -= padding.y;
            boundsMax.x += padding.x;
            boundsMax.y += padding.y;

            Vector2 center = (boundsMin + boundsMax) * 0.5f;
            Vector2 size = boundsMax - boundsMin;

            Vector4 positionOffset = profile.popupBackgroundPositionOffset;
            center.x += positionOffset.z - positionOffset.x;
            center.y += positionOffset.w - positionOffset.y;

            RectTransform backgroundRect = subMenuBackground.rectTransform;
            backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.anchoredPosition = center;
            backgroundRect.sizeDelta = size;
        }

        private bool TryGetVisibleSubButtonsBounds(out Vector2 boundsMin, out Vector2 boundsMax)
        {
            boundsMin = Vector2.zero;
            boundsMax = Vector2.zero;
            bool hasBounds = false;

            for (int i = 0; i < subMenuButtons.Count; i++)
            {
                SubMenuButton subMenuButton = subMenuButtons[i];
                if (subMenuButton == null || !subMenuButton.gameObject.activeSelf)
                {
                    continue;
                }

                RectTransform buttonRect = subMenuButton.transform as RectTransform;
                if (buttonRect == null)
                {
                    continue;
                }

                if (!TryGetRectBoundsInLocalSpace(buttonRect, popupRoot, out Vector2 buttonMin, out Vector2 buttonMax))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    boundsMin = buttonMin;
                    boundsMax = buttonMax;
                    hasBounds = true;
                    continue;
                }

                boundsMin = Vector2.Min(boundsMin, buttonMin);
                boundsMax = Vector2.Max(boundsMax, buttonMax);
            }

            return hasBounds;
        }

        private static bool TryGetRectBoundsInLocalSpace(
            RectTransform sourceRect,
            RectTransform referenceRoot,
            out Vector2 boundsMin,
            out Vector2 boundsMax)
        {
            boundsMin = Vector2.zero;
            boundsMax = Vector2.zero;

            if (sourceRect == null || referenceRoot == null)
            {
                return false;
            }

            Vector3[] corners = new Vector3[4];
            sourceRect.GetWorldCorners(corners);

            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 localPoint = referenceRoot.InverseTransformPoint(corners[i]);
                if (i == 0)
                {
                    boundsMin = localPoint;
                    boundsMax = localPoint;
                    continue;
                }

                boundsMin = Vector2.Min(boundsMin, localPoint);
                boundsMax = Vector2.Max(boundsMax, localPoint);
            }

            return true;
        }

        private void ApplySubItemConfigs(PrimaryMenuProfile profile)
        {
            SubMenuItemConfig[] items = profile.subItems ?? Array.Empty<SubMenuItemConfig>();

            if (subMenuButtons.Count == 0)
            {
                string listName = subMenuList != null ? subMenuList.name : "null";
                Debug.LogWarning(
                    $"[SubMenuPopup] 未找到子按钮槽位（list={listName}，已收集=0）。请确认 SubMenu_List 下存在带 SubMenuButton 的 Btn_Sub 节点。",
                    this);
                return;
            }

            if (items.Length == 0)
            {
                Debug.LogWarning(
                    $"[SubMenuPopup] Menu Profile 的 subItems 为空，无法刷新子按钮显示（槽位数={subMenuButtons.Count}）。",
                    this);
            }

            for (int i = 0; i < subMenuButtons.Count; i++)
            {
                SubMenuButton subMenuButton = subMenuButtons[i];
                if (subMenuButton == null)
                {
                    continue;
                }

                subMenuButton.InitializeSlot(i);
                if (i < items.Length)
                {
                    subMenuButton.ApplyConfig(items[i]);
                }
                else
                {
                    subMenuButton.HideSlot();
                }
            }

            if (items.Length > subMenuButtons.Count)
            {
                Debug.LogWarning(
                    $"[SubMenuPopup] Profile 子项数量 ({items.Length}) 超过场景槽位 ({subMenuButtons.Count})，多余项已忽略。",
                    this);
            }
        }

        /// <summary>
        /// 强制刷新 RectTransform 与 Canvas，使 Game 视口立即反映位置/布局变化。
        /// </summary>
        private void ForceRefreshLayout()
        {
            if (subMenuList != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(subMenuList);
            }

            if (popupRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(popupRoot);
            }

            Canvas.ForceUpdateCanvases();
        }

        private void CollectSubMenuButtons(bool includeInactive)
        {
            subMenuButtons.Clear();
            if (subMenuList == null)
            {
                return;
            }

            SubMenuButton[] buttons = CollectOrderedSubButtons(includeInactive);
            for (int i = 0; i < buttons.Length; i++)
            {
                SubMenuButton button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                button.InitializeSlot(i);
                button.ResolveReferences(includeInactive);
                button.SubButtonClicked -= HandleSubButtonClicked;
                button.SubButtonClicked += HandleSubButtonClicked;
                subMenuButtons.Add(button);
            }
        }

        private SubMenuButton[] CollectOrderedSubButtons(bool includeInactive)
        {
            if (subMenuList == null)
            {
                return Array.Empty<SubMenuButton>();
            }

            List<(SubMenuButton button, int index)> buttons = new List<(SubMenuButton, int)>();
            for (int i = 0; i < subMenuList.childCount; i++)
            {
                Transform child = subMenuList.GetChild(i);
                if (!SubMenuNodeNames.TryParseSubButtonIndex(child.name, out int slotIndex))
                {
                    continue;
                }

                if (!includeInactive && !child.gameObject.activeInHierarchy)
                {
                    continue;
                }

                SubMenuButton subMenuButton = child.GetComponent<SubMenuButton>();
                if (subMenuButton == null)
                {
                    Debug.LogWarning(
                        $"[SubMenuPopup] {child.name} 缺少 SubMenuButton 组件，请在 Btn_Sub 上手动添加。",
                        this);
                    continue;
                }

                buttons.Add((subMenuButton, slotIndex));
            }

            buttons.Sort((a, b) => a.index.CompareTo(b.index));

            SubMenuButton[] orderedButtons = new SubMenuButton[buttons.Count];
            for (int i = 0; i < buttons.Count; i++)
            {
                orderedButtons[i] = buttons[i].button;
            }

            return orderedButtons;
        }

        private void HandleSubButtonClicked(SubMenuButton subMenuButton)
        {
            if (subMenuButton == null || currentPrimaryOwner == null)
            {
                return;
            }

            SubMenuItemConfig config = subMenuButton.CurrentConfig;
            if (config == null || !config.enabled)
            {
                return;
            }

            SubItemClicked?.Invoke(currentPrimaryOwner, subMenuButton, config, subMenuButton.SlotIndex);
        }

        private void SetPopupActive(bool active)
        {
            if (popupRoot != null)
            {
                popupRoot.gameObject.SetActive(active);
                return;
            }

            gameObject.SetActive(active);
        }
    }
}
