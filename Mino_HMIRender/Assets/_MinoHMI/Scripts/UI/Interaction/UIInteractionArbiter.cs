using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MinoHMI.UI.Interaction
{
    /// <summary>
    /// UI 与 3D 交互仲裁器：当 UI 正在接管输入时禁用相机操控。
    /// </summary>
    [DisallowMultipleComponent]
    public class UIInteractionArbiter : MonoBehaviour
    {
        [SerializeField] private MinoCameraController cameraController;
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private List<GraphicRaycaster> graphicRaycasters = new List<GraphicRaycaster>();
        [SerializeField] private bool blockCameraWhenPointerOverUi = true;

        private readonly List<RaycastResult> raycastResultsCache = new List<RaycastResult>();
        private PointerEventData pointerEventData;

        /// <summary>当前帧是否由 UI 接管输入。</summary>
        public bool IsUiCapturingInput { get; private set; }

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = FindObjectOfType<MinoCameraController>(true);
            }

            if (eventSystem == null)
            {
                eventSystem = EventSystem.current;
            }

            if (graphicRaycasters.Count == 0)
            {
                graphicRaycasters.AddRange(FindObjectsOfType<GraphicRaycaster>(true));
            }
        }

        private void Update()
        {
            IsUiCapturingInput = CheckUiCapture();
            if (cameraController != null)
            {
                bool shouldBlockCameraInput = blockCameraWhenPointerOverUi && IsUiCapturingInput;
                cameraController.disableInput = shouldBlockCameraInput;
            }
        }

        private bool CheckUiCapture()
        {
            if (eventSystem == null)
            {
                return false;
            }

            if (eventSystem.IsPointerOverGameObject())
            {
                return true;
            }

            if (pointerEventData == null)
            {
                pointerEventData = new PointerEventData(eventSystem);
            }

            pointerEventData.position = Input.mousePosition;

            for (int index = 0; index < graphicRaycasters.Count; index++)
            {
                GraphicRaycaster raycaster = graphicRaycasters[index];
                if (raycaster == null || !raycaster.isActiveAndEnabled)
                {
                    continue;
                }

                raycastResultsCache.Clear();
                raycaster.Raycast(pointerEventData, raycastResultsCache);
                if (raycastResultsCache.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
