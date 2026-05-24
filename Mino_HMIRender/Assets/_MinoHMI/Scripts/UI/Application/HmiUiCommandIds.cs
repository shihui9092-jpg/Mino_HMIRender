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

        public const int CarPaintPresetCount = 6;

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
    }
}
