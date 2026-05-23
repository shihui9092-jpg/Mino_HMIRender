using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MinoHMI.DOTweenUGUI
{
    /// <summary>
    /// UGUI 按钮视图，负责按钮文案与点击反馈动画。
    /// </summary>
    public class DotweenUguiMenuItemView : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private Button button;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text tmpLabel;
        [SerializeField] private Text legacyLabel;

        [Header("点击反馈")]
        [SerializeField] private float clickScale = 1.08f;
        [SerializeField] private float clickDuration = 0.14f;
        [SerializeField] private Ease clickEaseOut = Ease.OutQuad;
        [SerializeField] private Ease clickEaseBack = Ease.OutBack;

        private Vector3 originalScale = Vector3.one;
        private Tween clickTween;
        private Action clickCallback;

        public RectTransform RectTransform => rectTransform;
        public Button Button => button;

        private void Awake()
        {
            CacheReferences();
            originalScale = rectTransform.localScale;
            button.onClick.AddListener(HandleButtonClick);
        }

        private void OnDestroy()
        {
            KillTweens();
            button.onClick.RemoveListener(HandleButtonClick);
        }

        public void Setup(string title, Action onClick)
        {
            CacheReferences();
            clickCallback = onClick;
            SetTitle(title);
            rectTransform.localScale = originalScale;
            SetAlpha(1f);
            SetInteractable(true);
        }

        public void SetTitle(string title)
        {
            if (tmpLabel != null)
            {
                tmpLabel.text = title;
            }

            if (legacyLabel != null)
            {
                legacyLabel.text = title;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }

        public Tween FadeTo(float targetAlpha, float duration, float delay)
        {
            if (canvasGroup == null)
            {
                return null;
            }

            return canvasGroup
                .DOFade(targetAlpha, duration)
                .SetDelay(delay)
                .SetEase(Ease.Linear);
        }

        public Tween MoveToX(float targetX, float duration, float delay, Ease ease)
        {
            return rectTransform
                .DOAnchorPosX(targetX, duration)
                .SetDelay(delay)
                .SetEase(ease);
        }

        public void SetAnchoredPositionX(float x)
        {
            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.x = x;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        public void PlayClickFeedback(Action onAnimationComplete = null)
        {
            KillClickTween();
            rectTransform.localScale = originalScale;
            clickTween = DOTween.Sequence()
                .Append(rectTransform.DOScale(originalScale * clickScale, clickDuration * 0.5f).SetEase(clickEaseOut))
                .Append(rectTransform.DOScale(originalScale, clickDuration * 0.5f).SetEase(clickEaseBack))
                .OnComplete(() => onAnimationComplete?.Invoke());
        }

        public void KillTweens()
        {
            KillClickTween();
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }

            if (rectTransform != null)
            {
                rectTransform.DOKill();
            }
        }

        private void HandleButtonClick()
        {
            PlayClickFeedback(clickCallback);
        }

        private void KillClickTween()
        {
            if (clickTween != null && clickTween.IsActive())
            {
                clickTween.Kill();
            }

            clickTween = null;
        }

        private void CacheReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
}
