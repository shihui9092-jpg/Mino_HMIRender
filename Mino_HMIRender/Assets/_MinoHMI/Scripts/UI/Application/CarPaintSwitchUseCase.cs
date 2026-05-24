using MinoHMI.CarPaint;
using UnityEngine;

namespace MinoHMI.UI.Application
{
    /// <summary>
    /// 车漆切换用例，供 UICommandBridge 调用。
    /// </summary>
    [DisallowMultipleComponent]
    public class CarPaintSwitchUseCase : MonoBehaviour, IUiCommandExecutor
    {
        public enum SwitchMode
        {
            NextPreset = 0,
            SpecifiedPreset = 1
        }

        [SerializeField] private CarPaintSwitcher carPaintSwitcher;
        [SerializeField] private SwitchMode switchMode = SwitchMode.NextPreset;
        [SerializeField] private int presetIndex;

        public void Bind(CarPaintSwitcher switcher)
        {
            carPaintSwitcher = switcher;
        }

        public void ExecuteCommand()
        {
            if (carPaintSwitcher == null)
            {
                return;
            }

            if (switchMode == SwitchMode.NextPreset)
            {
                carPaintSwitcher.SwitchToNextPreset();
                return;
            }

            carPaintSwitcher.SwitchToPreset(presetIndex);
        }
    }
}
