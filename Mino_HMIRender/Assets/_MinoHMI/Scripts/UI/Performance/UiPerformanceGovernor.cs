using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MinoHMI.UI.Performance
{
    /// <summary>
    /// 8295 场景性能治理：按帧率自动切换 URP 档位。
    /// </summary>
    [DisallowMultipleComponent]
    public class UiPerformanceGovernor : MonoBehaviour
    {
        [SerializeField] private UniversalRenderPipelineAsset targetUrpAsset;
        [SerializeField] private List<UrpQualityLevelProfile> qualityProfiles = new List<UrpQualityLevelProfile>();
        [SerializeField] private int startupProfileIndex = 1;
        [SerializeField] private float fpsSampleWindowSeconds = 1.0f;
        [SerializeField] private float decisionIntervalSeconds = 1.0f;
        [SerializeField] private bool enableAutoAdjust = true;

        private readonly Queue<float> frameDurations = new Queue<float>();
        private float sumFrameDuration;
        private float nextDecisionTime;
        private int currentProfileIndex = -1;

        public float AverageFps { get; private set; }
        public int CurrentProfileIndex => currentProfileIndex;

        private void Awake()
        {
            if (targetUrpAsset == null)
            {
                targetUrpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            }

            if (qualityProfiles.Count <= 0 || targetUrpAsset == null)
            {
                enabled = false;
                return;
            }

            startupProfileIndex = Mathf.Clamp(startupProfileIndex, 0, qualityProfiles.Count - 1);
            ApplyProfile(startupProfileIndex);
        }

        private void Update()
        {
            CollectFrameDuration(Time.unscaledDeltaTime);
            if (!enableAutoAdjust || Time.unscaledTime < nextDecisionTime)
            {
                return;
            }

            nextDecisionTime = Time.unscaledTime + decisionIntervalSeconds;
            EvaluateProfileSwitch();
        }

        private void CollectFrameDuration(float deltaTime)
        {
            frameDurations.Enqueue(deltaTime);
            sumFrameDuration += deltaTime;

            while (sumFrameDuration > fpsSampleWindowSeconds && frameDurations.Count > 1)
            {
                float oldestDuration = frameDurations.Dequeue();
                sumFrameDuration -= oldestDuration;
            }

            if (sumFrameDuration > 0.0001f)
            {
                AverageFps = frameDurations.Count / sumFrameDuration;
            }
        }

        private void EvaluateProfileSwitch()
        {
            if (!TryGetCurrentProfile(out UrpQualityLevelProfile currentProfile))
            {
                return;
            }

            if (AverageFps < currentProfile.DowngradeFpsThreshold)
            {
                int nextLowerProfileIndex = Mathf.Max(0, currentProfileIndex - 1);
                if (nextLowerProfileIndex != currentProfileIndex)
                {
                    ApplyProfile(nextLowerProfileIndex);
                }
                return;
            }

            if (AverageFps > currentProfile.UpgradeFpsThreshold)
            {
                int nextHigherProfileIndex = Mathf.Min(qualityProfiles.Count - 1, currentProfileIndex + 1);
                if (nextHigherProfileIndex != currentProfileIndex)
                {
                    ApplyProfile(nextHigherProfileIndex);
                }
            }
        }

        public bool ApplyProfile(int profileIndex)
        {
            if (profileIndex < 0 || profileIndex >= qualityProfiles.Count)
            {
                return false;
            }

            UrpQualityLevelProfile profile = qualityProfiles[profileIndex];
            if (profile == null)
            {
                return false;
            }

            UniversalRenderPipelineAsset activeAsset = profile.PipelineAsset != null
                ? profile.PipelineAsset
                : targetUrpAsset;

            if (activeAsset == null)
            {
                return false;
            }

            if (profile.PipelineAsset != null)
            {
                GraphicsSettings.defaultRenderPipeline = activeAsset;
                QualitySettings.renderPipeline = activeAsset;
                targetUrpAsset = activeAsset;
            }

            activeAsset.renderScale = profile.RenderScale;
            activeAsset.msaaSampleCount = profile.MsaaSampleCount;
            currentProfileIndex = profileIndex;
            return true;
        }

        private bool TryGetCurrentProfile(out UrpQualityLevelProfile profile)
        {
            profile = null;
            if (currentProfileIndex < 0 || currentProfileIndex >= qualityProfiles.Count)
            {
                return false;
            }

            profile = qualityProfiles[currentProfileIndex];
            return profile != null;
        }
    }
}
