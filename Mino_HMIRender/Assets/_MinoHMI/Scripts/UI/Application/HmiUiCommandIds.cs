namespace MinoHMI.UI.Application
{
    /// <summary>
    /// 示例与业务共用的 UI 命令 ID。
    /// </summary>
    public static class HmiUiCommandIds
    {
        public const string CarPaintNext = "HMI.CarPaint.Next";
        public const string CarPaintPreset0 = "HMI.CarPaint.Preset0";
        public const string CarPaintPreset1 = "HMI.CarPaint.Preset1";
        public const string CarPaintPreset2 = "HMI.CarPaint.Preset2";
        public const string CarPaintPreset3 = "HMI.CarPaint.Preset3";
        public const string CarPaintPreset4 = "HMI.CarPaint.Preset4";
        public const string CarPaintPreset5 = "HMI.CarPaint.Preset5";

        public const string CameraApplySelected = "HMI.Camera.ApplySelected";
        public const string CameraApplyPreset0 = "HMI.Camera.Preset0";
        public const string PageOpenHome = "HMI.Page.OpenHome";
        public const string PageOpenCarPaint = "HMI.Page.OpenCarPaint";

        public const string TimeWeatherVariant0 = "HMI.TimeWeather.Variant0";
        public const string TimeWeatherVariant1 = "HMI.TimeWeather.Variant1";
        public const string TimeWeatherVariant2 = "HMI.TimeWeather.Variant2";
        public const string TimeWeatherVariant3 = "HMI.TimeWeather.Variant3";
        public const string TimeWeatherVariant4 = "HMI.TimeWeather.Variant4";
        public const string TimeWeatherVariant5 = "HMI.TimeWeather.Variant5";

        public const int CarPaintPresetCount = 6;

        public const int TimeWeatherVariantCommandCount = 6;

        /// <summary>
        /// 获取车漆预设命令 ID（0~5）。
        /// </summary>
        public static string GetCarPaintPresetCommand(int presetIndex)
        {
            return presetIndex switch
            {
                0 => CarPaintPreset0,
                1 => CarPaintPreset1,
                2 => CarPaintPreset2,
                3 => CarPaintPreset3,
                4 => CarPaintPreset4,
                5 => CarPaintPreset5,
                _ => $"HMI.CarPaint.Preset{presetIndex}"
            };
        }

        /// <summary>
        /// 获取时间天气材质变体命令 ID（variantIndex 0 对应 UI 变体 1）。
        /// </summary>
        public static string GetTimeWeatherVariantCommand(int variantIndex)
        {
            return variantIndex switch
            {
                0 => TimeWeatherVariant0,
                1 => TimeWeatherVariant1,
                2 => TimeWeatherVariant2,
                3 => TimeWeatherVariant3,
                4 => TimeWeatherVariant4,
                5 => TimeWeatherVariant5,
                _ => $"HMI.TimeWeather.Variant{variantIndex}"
            };
        }
    }
}
