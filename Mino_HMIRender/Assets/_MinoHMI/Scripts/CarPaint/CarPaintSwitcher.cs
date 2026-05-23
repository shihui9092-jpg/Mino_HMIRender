using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace MinoHMI.CarPaint
{
    /// <summary>
    /// 车漆切换：挂在车模主节点（如 CarRoot）上即可。
    /// 自动扫描子物体车漆 Mesh，并按 Material 上实际存在的属性动态插值。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MinoHMI/车漆切换")]
    public class CarPaintSwitcher : MonoBehaviour
    {
        [Header("车漆预设")]
        [SerializeField]
        private CarPaintPresetSlot[] paintPresets = Array.Empty<CarPaintPresetSlot>();

        [SerializeField]
        [Tooltip("进入场景时的默认颜色索引")]
        private int defaultPresetIndex;

        [Header("过渡动画")]
        [SerializeField]
        [Min(0.01f)]
        private float transitionDuration = 1.2f;

        [SerializeField]
        private Ease transitionEase = Ease.InOutCubic;

        private Shader targetCarPaintShader;
        private readonly List<CarPaintSlotUtility.MaterialSlot> activeSlots = new List<CarPaintSlotUtility.MaterialSlot>();
        private CarPaintSnapshot transitionFromSnapshot;
        private CarPaintSnapshot transitionToSnapshot;
        private Tween activeTransitionTween;
        private Material pendingSourceMaterial;
        private int currentPresetIndex = -1;
        private bool isTransitioning;

        public int CurrentPresetIndex => currentPresetIndex;
        public bool IsTransitioning => isTransitioning;
        public int PresetCount => paintPresets?.Length ?? 0;
        public int ActiveSlotCount => activeSlots.Count;

        public event Action<int> OnPaintTransitionStarted;
        public event Action<int> OnPaintTransitionCompleted;

        private void Awake()
        {
            targetCarPaintShader = CarPaintSlotUtility.ResolveShaderFromPresets(paintPresets);
            if (targetCarPaintShader == null)
            {
                Debug.LogError(
                    $"[{nameof(CarPaintSwitcher)}] 无法识别车漆 Shader，请检查预设材质或 Shader 名称 {CarPaintSlotUtility.DefaultShaderName}",
                    this);
                enabled = false;
                return;
            }

            if (!CarPaintSlotUtility.TryBindSlots(transform, activeSlots, targetCarPaintShader))
            {
                Debug.LogError(
                    $"[{nameof(CarPaintSwitcher)}] 未找到使用 {targetCarPaintShader.name} 的车漆 Mesh",
                    this);
                enabled = false;
                return;
            }

            if (PresetCount > 0 && IsValidPresetIndex(defaultPresetIndex))
                ApplyPresetImmediate(defaultPresetIndex);
            else if (TryGetPrimaryRuntimeMaterial(out Material primaryMaterial))
                transitionFromSnapshot = CarPaintSnapshot.FromMaterial(primaryMaterial);
        }

        private void OnDestroy()
        {
            activeTransitionTween?.Kill();
            CarPaintSlotUtility.DestroyRuntimeMaterials(activeSlots);
            activeSlots.Clear();
        }

        public void SwitchToPreset(int presetIndex)
        {
            if (!IsValidPresetIndex(presetIndex))
                return;

            if (presetIndex == currentPresetIndex && !isTransitioning)
                return;

            Material targetSource = paintPresets[presetIndex].sourceMaterial;
            if (targetSource == null)
            {
                Debug.LogWarning($"[{nameof(CarPaintSwitcher)}] 预设 {presetIndex} 未配置材质球", this);
                return;
            }

            BeginTransition(presetIndex, CarPaintSnapshot.FromMaterial(targetSource), targetSource);
        }

        public void SwitchToMaterial(Material sourceMaterial)
        {
            if (sourceMaterial == null)
                return;

            BeginTransition(FindPresetIndexByMaterial(sourceMaterial), CarPaintSnapshot.FromMaterial(sourceMaterial), sourceMaterial);
        }

        public void ApplyPresetImmediate(int presetIndex)
        {
            if (!IsValidPresetIndex(presetIndex))
                return;

            Material source = paintPresets[presetIndex].sourceMaterial;
            if (source == null)
                return;

            activeTransitionTween?.Kill();
            isTransitioning = false;

            CarPaintSnapshot targetSnapshot = CarPaintSnapshot.FromMaterial(source);
            ApplySnapshotToAllSlots(targetSnapshot, applyKeywords: true);
            CopyTexturesToAllSlots(source);

            transitionFromSnapshot = targetSnapshot;
            transitionToSnapshot = targetSnapshot;
            currentPresetIndex = presetIndex;
        }

        public void SwitchToNextPreset()
        {
            if (PresetCount == 0)
                return;

            int nextIndex = currentPresetIndex < 0 ? 0 : (currentPresetIndex + 1) % PresetCount;
            SwitchToPreset(nextIndex);
        }

        private void BeginTransition(int presetIndex, CarPaintSnapshot targetSnapshot, Material sourceMaterial)
        {
            activeTransitionTween?.Kill();

            pendingSourceMaterial = sourceMaterial;
            transitionFromSnapshot = TryGetPrimaryRuntimeMaterial(out Material primaryMaterial)
                ? CarPaintSnapshot.FromMaterial(primaryMaterial)
                : new CarPaintSnapshot();
            transitionToSnapshot = targetSnapshot;
            isTransitioning = true;

            int resolvedIndex = presetIndex >= 0 ? presetIndex : currentPresetIndex;
            OnPaintTransitionStarted?.Invoke(resolvedIndex);

            float progress = 0f;
            activeTransitionTween = DOTween.To(
                    () => progress,
                    value =>
                    {
                        progress = value;
                        CarPaintSnapshot blended = CarPaintSnapshot.Lerp(transitionFromSnapshot, transitionToSnapshot, progress);
                        ApplySnapshotToAllSlots(blended, applyKeywords: false);
                    },
                    1f,
                    transitionDuration)
                .SetEase(transitionEase)
                .SetTarget(this)
                .OnKill(() => isTransitioning = false)
                .OnComplete(() => CompleteTransition(presetIndex));
        }

        private void CompleteTransition(int presetIndex)
        {
            isTransitioning = false;
            ApplySnapshotToAllSlots(transitionToSnapshot, applyKeywords: true);

            if (IsValidPresetIndex(presetIndex))
                currentPresetIndex = presetIndex;

            CopyTexturesToAllSlots(pendingSourceMaterial);
            pendingSourceMaterial = null;
            transitionFromSnapshot = transitionToSnapshot;
            OnPaintTransitionCompleted?.Invoke(currentPresetIndex);
        }

        private void ApplySnapshotToAllSlots(CarPaintSnapshot snapshot, bool applyKeywords)
        {
            for (int i = 0; i < activeSlots.Count; i++)
                snapshot.ApplyTo(activeSlots[i].RuntimeMaterial, applyKeywords);
        }

        private void CopyTexturesToAllSlots(Material source)
        {
            if (source == null)
                return;

            for (int i = 0; i < activeSlots.Count; i++)
                CarPaintSnapshot.CopyTextures(source, activeSlots[i].RuntimeMaterial);
        }

        private bool TryGetPrimaryRuntimeMaterial(out Material primaryMaterial)
        {
            primaryMaterial = activeSlots.Count > 0 ? activeSlots[0].RuntimeMaterial : null;
            return primaryMaterial != null;
        }

        private int FindPresetIndexByMaterial(Material sourceMaterial)
        {
            if (paintPresets == null)
                return -1;

            for (int i = 0; i < paintPresets.Length; i++)
            {
                if (paintPresets[i]?.sourceMaterial == sourceMaterial)
                    return i;
            }

            return -1;
        }

        private bool IsValidPresetIndex(int presetIndex)
        {
            return paintPresets != null && presetIndex >= 0 && presetIndex < paintPresets.Length;
        }
    }
}
