using UnityEngine;
using UnityEngine.UI;

namespace MinoHMI.UI.Performance
{
    /// <summary>
    /// UI 帧预算监控：统计 Canvas 与 Raycaster 数量并输出告警状态。
    /// </summary>
    [DisallowMultipleComponent]
    public class UiFrameBudgetWatcher : MonoBehaviour
    {
        [SerializeField] private int maxActiveCanvasCount = 6;
        [SerializeField] private int maxActiveRaycasterCount = 4;
        [SerializeField] private float sampleIntervalSeconds = 0.5f;
        [SerializeField] private bool emitWarningLog = true;
        [SerializeField] private float warningLogIntervalSeconds = 5f;

        private float nextSampleTime;
        private float nextAllowedLogTime;

        public int ActiveCanvasCount { get; private set; }
        public int ActiveRaycasterCount { get; private set; }
        public bool IsOverBudget { get; private set; }

        private void Update()
        {
            if (Time.unscaledTime < nextSampleTime)
            {
                return;
            }

            nextSampleTime = Time.unscaledTime + sampleIntervalSeconds;
            SampleUiBudget();
            if (emitWarningLog && IsOverBudget && Time.unscaledTime >= nextAllowedLogTime)
            {
                nextAllowedLogTime = Time.unscaledTime + warningLogIntervalSeconds;
                Debug.LogWarning(
                    $"[UiFrameBudgetWatcher] UI 预算超限。Canvas={ActiveCanvasCount}/{maxActiveCanvasCount}, Raycaster={ActiveRaycasterCount}/{maxActiveRaycasterCount}",
                    this);
            }
        }

        private void SampleUiBudget()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>(true);

            ActiveCanvasCount = CountActiveBehaviours(canvases);
            ActiveRaycasterCount = CountActiveBehaviours(raycasters);
            IsOverBudget = ActiveCanvasCount > maxActiveCanvasCount
                           || ActiveRaycasterCount > maxActiveRaycasterCount;
        }

        private static int CountActiveBehaviours<T>(T[] behaviours) where T : Behaviour
        {
            int activeCount = 0;
            for (int index = 0; index < behaviours.Length; index++)
            {
                T behaviour = behaviours[index];
                if (behaviour != null && behaviour.isActiveAndEnabled)
                {
                    activeCount++;
                }
            }

            return activeCount;
        }
    }
}
