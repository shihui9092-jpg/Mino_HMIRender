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
            if (contentRoot == null)
            {
                contentRoot = transform as RectTransform;
            }

            if (contentGrid == null)
            {
                contentGrid = GetComponentInChildren<GridLayoutGroup>(true);
            }

            if (subButtonTemplate != null)
            {
                subButtonTemplate.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            KillAnimationTweens();
        }

        public void Rebuild(IReadOnlyList<DotweenUguiSubButtonData> subButtonDataList, Action<DotweenUguiSubButtonData> onClick)
        {
            ClearItemViews();
            if (subButtonDataList == null || subButtonDataList.Count == 0 || subButtonTemplate == null || contentRoot == null)
            {
                return;
            }

            for (int i = 0; i < subButtonDataList.Count; i++)
            {
                DotweenUguiSubButtonData data = subButtonDataList[i];
                DotweenUguiMenuItemView itemView = Instantiate(subButtonTemplate, contentRoot);
                itemView.name = $"SubButton_{i}_{data.buttonId}";
                itemView.gameObject.SetActive(true);
                itemView.Setup(data.buttonName, () => onClick?.Invoke(data));
                itemViews.Add(itemView);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
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

            if (subButtonTemplate != null)
            {
                subButtonTemplate.gameObject.SetActive(false);
            }
        }
    }
}
