using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MinoHMI.DOTweenUGUI
{
    [Serializable]
    public class DotweenUguiMainMenuSelectedEvent : UnityEvent<string, string>
    {
    }

    [Serializable]
    public class DotweenUguiSubMenuSelectedEvent : UnityEvent<string, string, string, string>
    {
    }

    /// <summary>
    /// UGUI 主菜单控制器：生成主按钮，切换子菜单并转发点击事件。
    /// </summary>
    public class DotweenUguiMenuController : MonoBehaviour
    {
        [Header("结构引用")]
        [SerializeField] private RectTransform mainMenuRoot;
        [SerializeField] private GridLayoutGroup mainMenuGrid;
        [SerializeField] private DotweenUguiMenuItemView mainMenuButtonTemplate;
        [SerializeField] private DotweenUguiSubMenuPanel subMenuPanel;

        [Header("菜单数据")]
        [SerializeField] private List<DotweenUguiMainMenuData> menuDataList = new List<DotweenUguiMainMenuData>();
        [SerializeField] private int defaultMainMenuIndex;

        [Header("回调事件")]
        [SerializeField] private DotweenUguiMainMenuSelectedEvent onMainMenuSelected;
        [SerializeField] private DotweenUguiSubMenuSelectedEvent onSubMenuButtonClicked;

        private readonly List<DotweenUguiMenuItemView> mainMenuViews = new List<DotweenUguiMenuItemView>();
        private int currentMainMenuIndex = -1;

        private void Awake()
        {
            EnsureUiStructure();
            CacheReferences();
            BuildMainMenu();
            SelectMainMenu(Mathf.Clamp(defaultMainMenuIndex, 0, Mathf.Max(menuDataList.Count - 1, 0)));
        }

        private void OnDestroy()
        {
            ClearMainMenuViews();
        }

        public void RefreshMenu()
        {
            BuildMainMenu();
            int targetIndex = Mathf.Clamp(currentMainMenuIndex, 0, Mathf.Max(menuDataList.Count - 1, 0));
            SelectMainMenu(targetIndex);
        }

        private void BuildMainMenu()
        {
            ClearMainMenuViews();
            if (mainMenuRoot == null || mainMenuButtonTemplate == null || menuDataList.Count == 0)
            {
                return;
            }

            for (int i = 0; i < menuDataList.Count; i++)
            {
                DotweenUguiMainMenuData menuData = menuDataList[i];
                int menuIndex = i;

                DotweenUguiMenuItemView itemView = Instantiate(mainMenuButtonTemplate, mainMenuRoot);
                itemView.name = $"MainButton_{i}_{menuData.menuId}";
                itemView.gameObject.SetActive(true);
                itemView.Setup(menuData.menuName, () => HandleMainMenuClicked(menuIndex));
                mainMenuViews.Add(itemView);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(mainMenuRoot);
            mainMenuButtonTemplate.gameObject.SetActive(false);
        }

        private void HandleMainMenuClicked(int menuIndex)
        {
            if (menuIndex < 0 || menuIndex >= mainMenuViews.Count)
            {
                return;
            }

            mainMenuViews[menuIndex].PlayClickFeedback(() => SelectMainMenu(menuIndex));
        }

        private void SelectMainMenu(int menuIndex)
        {
            if (menuDataList.Count == 0 || menuIndex < 0 || menuIndex >= menuDataList.Count)
            {
                subMenuPanel.HideImmediate();
                return;
            }

            DotweenUguiMainMenuData mainData = menuDataList[menuIndex];
            currentMainMenuIndex = menuIndex;
            onMainMenuSelected?.Invoke(mainData.menuId, mainData.menuName);

            subMenuPanel.Rebuild(mainData.subButtons, subButtonData =>
            {
                onSubMenuButtonClicked?.Invoke(mainData.menuId, mainData.menuName, subButtonData.buttonId, subButtonData.buttonName);
            });
            subMenuPanel.PlayAppearAnimation();
        }

        private void ClearMainMenuViews()
        {
            for (int i = 0; i < mainMenuViews.Count; i++)
            {
                if (mainMenuViews[i] != null)
                {
                    mainMenuViews[i].KillTweens();
                    Destroy(mainMenuViews[i].gameObject);
                }
            }

            mainMenuViews.Clear();
            if (mainMenuButtonTemplate != null)
            {
                mainMenuButtonTemplate.gameObject.SetActive(false);
            }
        }

        private void CacheReferences()
        {
            if (mainMenuRoot == null)
            {
                mainMenuRoot = transform as RectTransform;
            }

            if (mainMenuGrid == null && mainMenuRoot != null)
            {
                mainMenuGrid = mainMenuRoot.GetComponent<GridLayoutGroup>();
            }

            if (subMenuPanel == null)
            {
                subMenuPanel = GetComponentInChildren<DotweenUguiSubMenuPanel>(true);
            }
        }

        private void EnsureUiStructure()
        {
            RectTransform rootRect = GetOrAddRectTransform(gameObject);
            if (mainMenuRoot == null)
            {
                mainMenuRoot = CreateUiRoot("MainMenuRoot", rootRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(520f, 120f));
            }

            if (mainMenuGrid == null)
            {
                mainMenuGrid = mainMenuRoot.GetComponent<GridLayoutGroup>();
                if (mainMenuGrid == null)
                {
                    mainMenuGrid = mainMenuRoot.gameObject.AddComponent<GridLayoutGroup>();
                }

                mainMenuGrid.cellSize = new Vector2(160f, 44f);
                mainMenuGrid.spacing = new Vector2(12f, 12f);
                mainMenuGrid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                mainMenuGrid.constraintCount = 1;
            }

            if (mainMenuButtonTemplate == null)
            {
                mainMenuButtonTemplate = CreateButtonTemplate(mainMenuRoot, "MainMenuButtonTemplate", "主菜单");
                mainMenuButtonTemplate.gameObject.SetActive(false);
            }

            if (subMenuPanel == null)
            {
                RectTransform subMenuRoot = CreateUiRoot("SubMenuRoot", rootRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -160f), new Vector2(720f, 420f));
                subMenuPanel = subMenuRoot.gameObject.AddComponent<DotweenUguiSubMenuPanel>();

                GridLayoutGroup subMenuGrid = subMenuRoot.gameObject.AddComponent<GridLayoutGroup>();
                subMenuGrid.cellSize = new Vector2(160f, 42f);
                subMenuGrid.spacing = new Vector2(12f, 12f);
                subMenuGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                subMenuGrid.constraintCount = 3;

                DotweenUguiMenuItemView subButtonTemplate = CreateButtonTemplate(subMenuRoot, "SubMenuButtonTemplate", "子按钮");
                subButtonTemplate.gameObject.SetActive(false);
                subMenuPanel.Configure(subMenuRoot, subMenuGrid, subButtonTemplate);
            }
        }

        private static DotweenUguiMenuItemView CreateButtonTemplate(Transform parent, string name, string labelText)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(DotweenUguiMenuItemView));
            buttonObject.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(160f, 44f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.22f, 0.26f, 0.35f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.28f, 0.34f, 0.45f, 1f);
            colors.pressedColor = new Color(0.18f, 0.22f, 0.3f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.18f, 0.2f, 0.24f, 0.6f);
            button.colors = colors;
            button.targetGraphic = image;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 6f);
            labelRect.offsetMax = new Vector2(-10f, -6f);

            Text label = labelObject.GetComponent<Text>();
            label.text = labelText;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 22;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            DotweenUguiMenuItemView itemView = buttonObject.GetComponent<DotweenUguiMenuItemView>();
            itemView.Setup(labelText, null);
            return itemView;
        }

        private static RectTransform CreateUiRoot(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject rootObject = new GameObject(objectName, typeof(RectTransform));
            rootObject.transform.SetParent(parent, false);
            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.anchorMin = anchorMin;
            rootRect.anchorMax = anchorMax;
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = sizeDelta;
            return rootRect;
        }

        private static RectTransform GetOrAddRectTransform(GameObject targetObject)
        {
            RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = targetObject.AddComponent<RectTransform>();
            }

            return rectTransform;
        }
    }
}
