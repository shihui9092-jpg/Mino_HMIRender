using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MinoHMI.UI_MenuSystem
{
    [Serializable]
    public class SubMenuItemSelectedEvent : UnityEvent<PrimaryMenuButton, string, int>
    {
    }

    /// <summary>
    /// UI_MenuSystem 总控制器：注册主菜单、管理子菜单弹窗开关与事件转发。
    /// </summary>
    public class MenuSystemController : MonoBehaviour
    {
        [Header("结构引用")]
        [SerializeField] private SubMenuPopup subMenuPopup;

        [Header("事件")]
        [SerializeField] private SubMenuItemSelectedEvent onSubMenuItemSelected;

        public SubMenuItemSelectedEvent SubMenuItemSelected => onSubMenuItemSelected;

        private readonly List<PrimaryMenuButton> primaryMenuButtons = new List<PrimaryMenuButton>();

        private void Awake()
        {
            ResolveReferences();
            RegisterPrimaryMenuButtons();
            if (subMenuPopup != null)
            {
                subMenuPopup.SubItemClicked += HandleSubMenuItemClicked;
            }
        }

        private void OnDestroy()
        {
            if (subMenuPopup != null)
            {
                subMenuPopup.SubItemClicked -= HandleSubMenuItemClicked;
            }
        }

        public void ResolveReferences()
        {
            if (subMenuPopup == null)
            {
                subMenuPopup = GetComponentInChildren<SubMenuPopup>(true);
            }

            if (subMenuPopup == null)
            {
                Transform popupTransform = MenuSystemHierarchyUtility.FindNamedTransform(
                    transform, SubMenuNodeNames.SubMenuPopup, true);
                if (popupTransform != null)
                {
                    subMenuPopup = popupTransform.GetComponent<SubMenuPopup>();
                }
            }

            subMenuPopup?.ResolveReferences(includeInactive: true);
        }

        public void RegisterPrimaryMenuButtons()
        {
            primaryMenuButtons.Clear();
            PrimaryMenuButton[] buttons = GetComponentsInChildren<PrimaryMenuButton>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                RegisterPrimaryMenuButton(buttons[i]);
            }
        }

        public void RegisterPrimaryMenuButton(PrimaryMenuButton primaryMenuButton)
        {
            if (primaryMenuButton == null || primaryMenuButtons.Contains(primaryMenuButton))
            {
                return;
            }

            primaryMenuButton.BindMenuSystem(this);
            primaryMenuButtons.Add(primaryMenuButton);
        }

        /// <summary>
        /// 主菜单点击：同一项 Toggle 关闭，不同项切换内容。
        /// </summary>
        public void OnPrimaryMenuClicked(PrimaryMenuButton primaryOwner)
        {
            if (primaryOwner == null || subMenuPopup == null)
            {
                return;
            }

            PrimaryMenuProfile profile = primaryOwner.MenuProfile;
            if (profile == null || profile.subItems == null || profile.subItems.Length == 0)
            {
                subMenuPopup.CloseImmediate();
                return;
            }

            if (subMenuPopup.IsOwnedBy(primaryOwner))
            {
                subMenuPopup.CloseImmediate();
                return;
            }

            subMenuPopup.Open(primaryOwner, profile);
        }

        public void CloseSubMenu()
        {
            subMenuPopup?.CloseImmediate();
        }

        /// <summary>
        /// 主菜单 Profile 变更后刷新已打开的子菜单（Play Mode / Inspector 调参）。
        /// </summary>
        public void RefreshSubMenuIfOwnedBy(PrimaryMenuButton primaryOwner)
        {
            if (primaryOwner == null || subMenuPopup == null)
            {
                return;
            }

            if (subMenuPopup.IsOwnedBy(primaryOwner))
            {
                subMenuPopup.RefreshCurrent();
            }
        }

        private void HandleSubMenuItemClicked(
            PrimaryMenuButton primaryOwner,
            SubMenuButton subMenuButton,
            SubMenuItemConfig config,
            int slotIndex)
        {
            if (config == null)
            {
                return;
            }

            onSubMenuItemSelected?.Invoke(primaryOwner, config.buttonId, slotIndex);
        }
    }
}
