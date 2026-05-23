using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MinoHMI.DOTweenUGUI
{
    /// <summary>
    /// 子菜单面板控制器：负责子按钮生成、Grid 排版与出现动画。
    /// </summary>
    public class DotweenUguiSubMenuPanel : MonoBehaviour
    {
        [Header("结构引用")]
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private GridLayoutGroup contentGrid;
        [SerializeField] private DotweenUguiMenuItemView subButtonTemplate;

        [Header("子菜单出现动画")]
        [SerializeField] private float appearFromLeftOffset = 80f;
        [SerializeField] private float appearMoveDuration = 0.24f;
        [SerializeField] private float appearFadeDuration = 0.2f;
        [SerializeField] private float itemStagger = 0.04f;
        [SerializeField] private Ease appearEase = Ease.OutCubic;

        private readonly List<DotweenUguiMenuItemView> itemViews = new List<DotweenUguiMenuItemView>();
        private readonly List<Tween> activeTweens = new List<Tween>();

        private void Awake()
        {
            if (!DotweenUguiSceneUtility.IsSceneInstance(this))
            {
                return;
            }

            BindStructureFromHierarchy();
        }

        /// <summary>
        /// 绑定到当前 SubMenuRoot 场景实例上的 RectTransform 与 GridLayoutGroup，保留预制体参数。
        /// </summary>
        public void BindStructureFromHierarchy()
        {
            if (!DotweenUguiSceneUtility.IsSceneInstance(this))
            {
                return;
            }

            RectTransform ownRoot = transform as RectTransform;
            if (ownRoot != null)
            {
                contentRoot = ownRoot;
            }

            GridLayoutGroup ownGrid = GetComponent<GridLayoutGroup>();
            if (ownGrid != null && DotweenUguiSceneUtility.IsSceneInstance(ownGrid))
            {
                contentGrid = ownGrid;
                return;
            }

            ResolveContentGridReference();
        }

        /// <summary>
        /// 作者结构模式校验（由 MenuController 调用）。
        /// </summary>
        public void ValidateAuthorStructure(RectTransform authorMainMenuRoot)
        {
            if (contentRoot == null)
            {
                Debug.LogError("[DotweenUguiSubMenuPanel] Content Root 未配置。", this);
                return;
            }

            if (!DotweenUguiSceneUtility.IsSceneInstance(contentRoot))
            {
                Debug.LogError("[DotweenUguiSubMenuPanel] Content Root 必须是场景实例。", this);
            }

            if (ResolveContentGridReference() == null)
            {
                Debug.LogError("[DotweenUguiSubMenuPanel] Content Root 下未找到 GridLayoutGroup。", this);
            }
        }

        private static bool IsSameOrChildOf(Transform target, Transform ancestor)
        {
            if (target == null || ancestor == null)
            {
                return false;
            }

            return target == ancestor || target.IsChildOf(ancestor);
        }

        private GridLayoutGroup ResolveContentGridReference()
        {
            if (contentRoot == null)
            {
                return null;
            }

            GridLayoutGroup assignedGrid = contentGrid;
            if (assignedGrid != null)
            {
                bool isSceneInstance = DotweenUguiSceneUtility.IsSceneInstance(assignedGrid);
                bool underRoot = IsSameOrChildOf(assignedGrid.transform, contentRoot.transform);

                if (isSceneInstance && underRoot)
                {
                    return assignedGrid;
                }

                if (!isSceneInstance)
                {
                    Debug.LogWarning("[DotweenUguiSubMenuPanel] Content Grid 指向 Project 预制体资源，已忽略并自动查找。", this);
                }
                else
                {
                    Debug.LogWarning(
                        $"[DotweenUguiSubMenuPanel] Content Grid（{DotweenUguiSceneUtility.GetHierarchyPath(assignedGrid.transform)}）与 Content Root（{DotweenUguiSceneUtility.GetHierarchyPath(contentRoot)}）不匹配，已自动从 Root 子树重新查找。",
                        this);
                }

                contentGrid = null;
            }

            contentGrid = contentRoot.GetComponent<GridLayoutGroup>();
            if (contentGrid == null)
            {
                contentGrid = contentRoot.GetComponentInChildren<GridLayoutGroup>(true);
            }

            return contentGrid;
        }

        private RectTransform ResolveContentSpawnRoot()
        {
            if (contentRoot == null)
            {
                Debug.LogError("[DotweenUguiSubMenuPanel] Content Root 未配置，无法生成子按钮。", this);
                return null;
            }

            if (!DotweenUguiSceneUtility.IsSceneInstance(contentRoot))
            {
                Debug.LogError("[DotweenUguiSubMenuPanel] Content Root 不是场景实例，无法生成子按钮。", this);
                return null;
            }

            GridLayoutGroup grid = ResolveContentGridReference();
            if (grid != null)
            {
                return grid.transform as RectTransform;
            }

            return contentRoot;
        }

        private void PurgeOrphanedRuntimeItems()
        {
            RectTransform spawnRoot = ResolveContentSpawnRoot();
            if (spawnRoot == null)
            {
                return;
            }

            DotweenUguiMenuRuntimeItem.PurgeUnder(spawnRoot);
        }

        private void OnDestroy()
        {
            KillAnimationTweens();
        }

        public void Rebuild(IReadOnlyList<DotweenUguiSubButtonData> subButtonDataList, Action<DotweenUguiSubButtonData> onClick)
        {
            ClearItemViews();
            PurgeOrphanedRuntimeItems();

            RectTransform spawnRoot = ResolveContentSpawnRoot();
            if (subButtonDataList == null ||
                subButtonDataList.Count == 0 ||
                subButtonTemplate == null ||
                spawnRoot == null)
            {
                return;
            }

            for (int i = 0; i < subButtonDataList.Count; i++)
            {
                DotweenUguiSubButtonData data = subButtonDataList[i];
                DotweenUguiMenuItemView itemView = Instantiate(subButtonTemplate, spawnRoot);
                itemView.name = $"{DotweenUguiMenuRuntimeItem.SubButtonNamePrefix}{i}_{data.buttonId}";
                itemView.gameObject.SetActive(true);
                DotweenUguiMenuRuntimeItem.Mark(itemView.gameObject);
                itemView.Setup(data.buttonName, () => onClick?.Invoke(data));
                itemViews.Add(itemView);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(spawnRoot);
        }

        public void PlayAppearAnimation()
        {
            KillAnimationTweens();
            if (itemViews.Count == 0)
            {
                return;
            }

            float[] targetXValues = new float[itemViews.Count];
            for (int i = 0; i < itemViews.Count; i++)
            {
                targetXValues[i] = itemViews[i].RectTransform.anchoredPosition.x;
            }

            for (int i = 0; i < itemViews.Count; i++)
            {
                DotweenUguiMenuItemView itemView = itemViews[i];
                itemView.SetInteractable(false);
                itemView.SetAnchoredPositionX(targetXValues[i] - appearFromLeftOffset);
                itemView.SetAlpha(0f);

                float delay = i * itemStagger;
                Tween moveTween = itemView.MoveToX(targetXValues[i], appearMoveDuration, delay, appearEase);
                Tween fadeTween = itemView.FadeTo(1f, appearFadeDuration, delay);
                activeTweens.Add(moveTween);
                activeTweens.Add(fadeTween);
            }

            float totalDelay = (itemViews.Count - 1) * itemStagger;
            float finishDelay = Mathf.Max(appearMoveDuration, appearFadeDuration) + totalDelay;
            Tween callbackTween = DOVirtual.DelayedCall(finishDelay, EnableAllItemsInteractable);
            activeTweens.Add(callbackTween);
        }

        public void HideImmediate()
        {
            KillAnimationTweens();
            for (int i = 0; i < itemViews.Count; i++)
            {
                itemViews[i].SetAlpha(0f);
                itemViews[i].SetInteractable(false);
            }
        }

        private void EnableAllItemsInteractable()
        {
            for (int i = 0; i < itemViews.Count; i++)
            {
                itemViews[i].SetInteractable(true);
            }
        }

        private void ClearItemViews()
        {
            KillAnimationTweens();
            for (int i = 0; i < itemViews.Count; i++)
            {
                if (itemViews[i] != null)
                {
                    itemViews[i].KillTweens();
                    Destroy(itemViews[i].gameObject);
                }
            }

            itemViews.Clear();
        }

        private void KillAnimationTweens()
        {
            for (int i = 0; i < activeTweens.Count; i++)
            {
                Tween tween = activeTweens[i];
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }
            }

            activeTweens.Clear();
        }

        public void Configure(RectTransform targetContentRoot, GridLayoutGroup targetContentGrid, DotweenUguiMenuItemView targetTemplate)
        {
            contentRoot = targetContentRoot;
            contentGrid = targetContentGrid;
            subButtonTemplate = targetTemplate;
        }

        /// <summary>
        /// 设置子按钮模板（由 MenuController 调用，优先级高于 Inspector 配置）。
        /// </summary>
        public void SetSubButtonTemplate(DotweenUguiMenuItemView template)
        {
            if (template == null)
            {
                return;
            }

            subButtonTemplate = template;
        }
    }
}
