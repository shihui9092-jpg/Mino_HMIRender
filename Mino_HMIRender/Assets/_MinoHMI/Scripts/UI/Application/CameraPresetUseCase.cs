using UnityEngine;

namespace MinoHMI.UI.Application
{
    /// <summary>
    /// 相机机位用例，供 UICommandBridge 调用。
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraPresetUseCase : MonoBehaviour, IUiCommandExecutor
    {
        public enum PresetOperation
        {
            ApplySelected = 0,
            ApplyByIndex = 1,
            CaptureCurrentToSelected = 2,
            CaptureCurrentToIndex = 3
        }

        [SerializeField] private MinoCameraController cameraController;
        [SerializeField] private PresetOperation operation = PresetOperation.ApplySelected;
        [SerializeField] private int presetIndex;

        public void Bind(MinoCameraController controller)
        {
            cameraController = controller;
        }

        public void ExecuteCommand()
        {
            if (cameraController == null)
            {
                return;
            }

            switch (operation)
            {
                case PresetOperation.ApplySelected:
                    cameraController.ApplySelectedPreset();
                    break;
                case PresetOperation.ApplyByIndex:
                    cameraController.ApplyPresetSlot(presetIndex);
                    break;
                case PresetOperation.CaptureCurrentToSelected:
                    cameraController.CaptureCurrentViewToSelectedPreset();
                    break;
                case PresetOperation.CaptureCurrentToIndex:
                    cameraController.CaptureCurrentViewToPreset(presetIndex);
                    break;
            }
        }
    }
}
