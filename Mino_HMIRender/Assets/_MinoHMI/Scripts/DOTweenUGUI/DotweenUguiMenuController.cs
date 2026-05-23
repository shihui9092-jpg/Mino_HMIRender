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
        [SerializeField] private DotweenUguiMenuItemView subMenuButtonTemplate;

        [Header("菜单数据")]
        [SerializeField] private List<DotweenUguiMainMenuData> menuDataList = new List<DotweenUguiMainMenuData>();
        [SerializeField] private int defaultMainMenuIndex;

        [Header("回调事件")]
        [SerializeField] private DotweenUguiMainMenuSelectedEvent onMainMenuSelected;
        [SerializeField] private DotweenUguiSubMenuSelectedEvent onSubMenuButtonClicked;

        /// <summary>主菜单选中时触发，参数为 (menuId, menuName)。</summary>
        public DotweenUguiMainMenuSelectedEvent MainMenuSelected => onMainMenuSelected;

        /// <summary>子菜单按钮点击时触发，参数为 (mainMenuId, mainMenuName, subButtonId, subButtonName)。</summary>
        public DotweenUguiSubMenuSelectedEvent SubMenuButtonClicked => onSubMenuButtonClicked;

        private readonly List<DotweenUguiMenuItemView> mainMenuViews = new List<DotweenUguiMenuItemView>();
        private int currentMainMenuIndex = -1;

        private bool UsesAuthorStructure => mainMenuRoot != null && subMenuPanel != null;

        private bool HasNestedStructureChildren =>
            FindSceneMainMenuRoot() != null || FindSceneSubMenuPanel() != null;

        private void Awake()
        {
            if (!DotweenUguiSceneUtility.IsSceneInstance(this))
            {
                return;
            }

            BindAuthorStructureFromHierarchy();

            if (UsesAuthorStructure || HasNestedStructureChildren)
            {
                BindAuthorStructureFromHierarchy();
                ValidateAuthorStructure();
            }
            else
            {
                TryBootstrapDefaultStructure();
            }

            CacheReferences();
            PurgeOrphanedMainMenuRuntimeItems();
        }

        private void Start()
        {
            BuildMainMenu();
            SelectMainMenu(Mathf.Clamp(defaultMainMenuIndex, 0, Mathf.Max(menuDataList.Count - 1, 0)));
        }

        /// <summary>
        /// 将结构引用绑定到场景中的嵌套 MainMenuRoot / SubMenuRoot 实例，保留预制体上的 RectTransform 与 Grid 参数。
        /// </summary>
        private void BindAuthorStructureFromHierarchy()
        {
            RectTransform hierarchyMainRoot = FindSceneMainMenuRoot();
            DotweenUguiSubMenuPanel hierarchySubPanel = FindSceneSubMenuPanel();

            if (mainMenuRoot != null && !DotweenUguiSceneUtility.IsSceneInstance(mainMenuRoot))
            {
                Debug.LogWarning("[DotweenUguiMenuController] Main Menu Root 指向 Project 预制体资源，已改为场景嵌套实例。", this);
                mainMenuRoot = hierarchyMainRoot;
            }
            else if (mainMenuRoot == null)
            {
                mainMenuRoot = hierarchyMainRoot;
            }

            if (subMenuPanel != null && !DotweenUguiSceneUtility.IsSceneInstance(subMenuPanel))
            {
                Debug.LogWarning("[DotweenUguiMenuController] Sub Menu Panel 指向 Project 预制体资源，已改为场景嵌套实例。", this);
                subMenuPanel = hierarchySubPanel;
            }
            else if (subMenuPanel == null)
            {
                subMenuPanel = hierarchySubPanel;
            }

            if (mainMenuGrid != null && !DotweenUguiSceneUtility.IsSceneInstance(mainMenuGrid))
            {
                mainMenuGrid = null;
            }

            if (mainMenuRoot != null)
            {
                ResolveMainMenuGrid();
            }

            if (subMenuPanel != null)
            {
                subMenuPanel.BindStructureFromHierarchy();
            }
        }

        private RectTransform FindSceneMainMenuRoot()
        {
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == transform || candidate.name != "MainMenuRoot")
                {
                    continue;
                }

                RectTransform rectTransform = candidate as RectTransform;
                if (rectTransform != null && DotweenUguiSceneUtility.IsSceneInstance(rectTransform))
                {
                    return rectTransform;
                }
            }

            return null;
        }

        private DotweenUguiSubMenuPanel FindSceneSubMenuPanel()
        {
            DotweenUguiSubMenuPanel[] panels = GetComponentsInChildren<DotweenUguiSubMenuPanel>(true);
            for (int i = 0; i < panels.Length; i++)
            {
                DotweenUguiSubMenuPanel panel = panels[i];
                if (DotweenUguiSceneUtility.IsSceneInstance(panel))
                {
                    return panel;
                }
            }

            return null;
        }

        /// <summary>
        /// 作者结构模式：仅校验引用，不创建、不修改 Grid。
        /// </summary>
        private void ValidateAuthorStructure()
        {
            if (!DotweenUguiSceneUtility.IsSceneInstance(mainMenuRoot))
            {
                Debug.LogError("[DotweenUguiMenuController] Main Menu Root 必须是场景中的嵌套实例。", this);
                return;
            }

            if (ResolveMainMenuGrid() == null)
            {
                Debug.LogError("[DotweenUguiMenuController] Main Menu Root 下未找到 GridLayoutGroup。", this);
            }

            if (!DotweenUguiSceneUtility.IsSceneInstance(subMenuPanel))
            {
                Debug.LogError("[DotweenUguiMenuController] Sub Menu Panel 必须是场景中的嵌套实例。", this);
            }
            else
            {
                subMenuPanel.ValidateAuthorStructure(mainMenuRoot);
            }
        }

        /// <summary>
        /// 兜底模式：结构引用未配置时自动创建默认节点。
        /// </summary>
        private void TryBootstrapDefaultStructure()
        {
            if (HasNestedStructureChildren)
            {
                BindAuthorStructureFromHierarchy();
                return;
            }

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
                    mainMenuGrid.cellSize = new Vector2(160f, 44f);
                    mainMenuGrid.spacing = new Vector2(12f, 12f);
                    mainMenuGrid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    mainMenuGrid.constraintCount = 1;
                }
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

                GridLayoutGroup subMenuGrid = subMenuRoot.GetComponent<GridLayoutGroup>();
                if (subMenuGrid == null)
                {
                    subMenuGrid = subMenuRoot.gameObject.AddComponent<GridLayoutGroup>();
                    subMenuGrid.cellSize = new Vector2(160f, 42f);
                    subMenuGrid.spacing = new Vector2(12f, 12f);
                    subMenuGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    subMenuGrid.constraintCount = 3;
                }

                if (subMenuButtonTemplate != null)
                {
                    subMenuPanel.Configure(subMenuRoot, subMenuGrid, subMenuButtonTemplate);
                }
                else
                {
                    DotweenUguiMenuItemView runtimeSubButtonTemplate = CreateButtonTemplate(subMenuRoot, "SubMenuButtonTemplate", "子按钮");
                    subMenuPanel.Configure(subMenuRoot, subMenuGrid, runtimeSubButtonTemplate);
                }
            }
        }

        private void CacheReferences()
        {
            if (mainMenuRoot == null)
            {
                Debug.LogError("[DotweenUguiMenuController] 未绑定 Main Menu Root。请将嵌套 MainMenuRoot 实例拖到结构引用槽位。", this);
            }

            if (mainMenuRoot != null)
            {
                ResolveMainMenuGrid();
            }

            if (subMenuPanel == null)
            {
                subMenuPanel = GetComponentInChildren<DotweenUguiSubMenuPanel>(true);
                if (subMenuPanel == null)
                {
                    Debug.LogError("[DotweenUguiMenuController] 未绑定 Sub Menu Panel。请将嵌套 SubMenuRoot 实例上的组件拖到结构引用槽位。", this);
                }
            }

            if (subMenuPanel != null && subMenuButtonTemplate != null)
            {
                subMenuPanel.SetSubButtonTemplate(subMenuButtonTemplate);
            }
        }

        private RectTransform ResolveMainMenuSpawnRoot()
        {
            if (mainMenuRoot == null)
            {
                return null;
            }

            if (!DotweenUguiSceneUtility.IsSceneInstance(mainMenuRoot))
            {
                Debug.LogError("[DotweenUguiMenuController] Main Menu Root 不是场景实例，无法生成主菜单按钮。", this);
                return null;
            }

            GridLayoutGroup grid = ResolveMainMenuGrid();
            if (grid != null)
            {
                return grid.transform as RectTransform;
            }

            return mainMenuRoot;
        }

        /// <summary>
        /// 解析主菜单 Grid：Inspector 绑定有效则沿用，否则自动从 Main Menu Root 子树查找。
        /// </summary>
        private GridLayoutGroup ResolveMainMenuGrid()
        {
            if (mainMenuRoot == null)
            {
                return null;
            }

            GridLayoutGroup assignedGrid = mainMenuGrid;
            if (assignedGrid != null)
            {
                bool isSceneInstance = DotweenUguiSceneUtility.IsSceneInstance(assignedGrid);
                bool underRoot = IsSameOrChildOf(assignedGrid.transform, mainMenuRoot.transform);

                if (isSceneInstance && underRoot)
                {
                    return assignedGrid;
                }

                if (!isSceneInstance)
                {
                    Debug.LogWarning("[DotweenUguiMenuController] Main Menu Grid 指向 Project 预制体资源，已忽略并自动查找。", this);
                }
                else
                {
                    Debug.LogWarning(
                        $"[DotweenUguiMenuController] Main Menu Grid（{DotweenUguiSceneUtility.GetHierarchyPath(assignedGrid.transform)}）与 Main Menu Root（{DotweenUguiSceneUtility.GetHierarchyPath(mainMenuRoot)}）不匹配，已自动从 Root 子树重新查找。",
                        this);
                }

                mainMenuGrid = null;
            }

            mainMenuGrid = mainMenuRoot.GetComponent<GridLayoutGroup>();
            if (mainMenuGrid == null)
            {
                mainMenuGrid = mainMenuRoot.GetComponentInChildren<GridLayoutGroup>(true);
            }

            return mainMenuGrid;
        }

        private static bool IsSameOrChildOf(Transform target, Transform ancestor)
        {
            if (target == null || ancestor == null)
            {
                return false;
            }

            return target == ancestor || target.IsChildOf(ancestor);
        }

        private void PurgeOrphanedMainMenuRuntimeItems()
        {
            RectTransform spawnRoot = ResolveMainMenuSpawnRoot();
            if (spawnRoot == null)
            {
                return;
            }

            DotweenUguiMenuRuntimeItem.PurgeUnder(spawnRoot);
        }

        public void RefreshMenu()
        {
            BuildMainMenu();
            int targetIndex = Mathf.Clamp(currentMainMenuIndex, 0, Mathf.Max(menuDataList.Count - 1, 0));
            SelectMainMenu(targetIndex, forceRefresh: true);
        }

        private void OnDestroy()
        {
            ClearMainMenuViews();
        }

        private void BuildMainMenu()
        {
            ClearMainMenuViews();
            PurgeOrphanedMainMenuRuntimeItems();

            RectTransform spawnRoot = ResolveMainMenuSpawnRoot();
            if (spawnRoot == null ||
                mainMenuButtonTemplate == null ||
                menuDataList.Count == 0)
            {
                return;
            }

            for (int i = 0; i < menuDataList.Count; i++)
            {
                DotweenUguiMainMenuData menuData = menuDataList[i];
                int menuIndex = i;

                DotweenUguiMenuItemView itemView = Instantiate(mainMenuButtonTemplate, spawnRoot);
                itemView.name = $"{DotweenUguiMenuRuntimeItem.MainButtonNamePrefix}{i}_{menuData.menuId}";
                itemView.gameObject.SetActive(true);
                DotweenUguiMenuRuntimeItem.Mark(itemView.gameObject);
                itemView.Setup(menuData.menuName, () => HandleMainMenuClicked(menuIndex));
                mainMenuViews.Add(itemView);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(spawnRoot);
        }

        private void HandleMainMenuClicked(int menuIndex)
        {
            if (menuIndex < 0 || menuIndex >= mainMenuViews.Count)
            {
                return;
            }

            if (menuIndex == currentMainMenuIndex)
            {
                return;
            }

            mainMenuViews[menuIndex].PlayClickFeedback(() => SelectMainMenu(menuIndex));
        }

        private void SelectMainMenu(int menuIndex, bool forceRefresh = false)
        {
            if (menuDataList.Count == 0 || menuIndex < 0 || menuIndex >= menuDataList.Count)
            {
                if (subMenuPanel != null)
                {
                    subMenuPanel.HideImmediate();
                }
                return;
            }

            if (!forceRefresh && menuIndex == currentMainMenuIndex)
            {
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
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!DotweenUguiSceneUtility.IsSceneInstance(this))
            {
                return;
            }

            BindAuthorStructureFromHierarchy();

            if (mainMenuButtonTemplate != null && mainMenuButtonTemplate.gameObject.scene.IsValid())
            {
                Debug.LogWarning("[DotweenUguiMenuController] Main Menu Button Template 应指向 Project 预制体，而非 Hierarchy 场景对象。", this);
            }

            if (subMenuButtonTemplate != null && subMenuButtonTemplate.gameObject.scene.IsValid())
            {
                Debug.LogWarning("[DotweenUguiMenuController] Sub Menu Button Template 应指向 Project 预制体，而非 Hierarchy 场景对象。", this);
            }
        }
#endif

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
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
