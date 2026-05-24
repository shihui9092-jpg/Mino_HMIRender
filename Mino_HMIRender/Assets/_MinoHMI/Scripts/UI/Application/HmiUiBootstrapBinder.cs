using MinoHMI.CarPaint;
using MinoHMI.UI.Core;
using UnityEngine;

namespace MinoHMI.UI.Application
{
    /// <summary>
    /// 拖入场景后自动绑定车模、相机等业务引用。
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public class HmiUiBootstrapBinder : MonoBehaviour
    {
        [SerializeField] private bool autoBindSceneReferences = true;
        [SerializeField] private UICommandBridge commandBridge;

        private void Awake()
        {
            if (!autoBindSceneReferences)
            {
                return;
            }

            if (commandBridge == null)
            {
                commandBridge = GetComponent<UICommandBridge>();
            }

            CarPaintSwitcher carPaintSwitcher = FindObjectOfType<CarPaintSwitcher>(true);
            MinoCameraController cameraController = FindObjectOfType<MinoCameraController>(true);
            UIPageController pageController = GetComponent<UIPageController>();

            BindUseCases(carPaintSwitcher, cameraController, pageController);
        }

        private void BindUseCases(
            CarPaintSwitcher carPaintSwitcher,
            MinoCameraController cameraController,
            UIPageController pageController)
        {
            CarPaintSwitchUseCase[] paintUseCases = GetComponentsInChildren<CarPaintSwitchUseCase>(true);
            for (int index = 0; index < paintUseCases.Length; index++)
            {
                paintUseCases[index].Bind(carPaintSwitcher);
            }

            CameraPresetUseCase[] cameraUseCases = GetComponentsInChildren<CameraPresetUseCase>(true);
            for (int index = 0; index < cameraUseCases.Length; index++)
            {
                cameraUseCases[index].Bind(cameraController);
            }

            UiPageNavigationUseCase[] pageUseCases = GetComponentsInChildren<UiPageNavigationUseCase>(true);
            for (int index = 0; index < pageUseCases.Length; index++)
            {
                pageUseCases[index].Bind(pageController);
            }
        }
    }
}
