using UnityEngine;
using UnityEngine.UI;

namespace MinoHMI.Rendering.Examples
{
    /// <summary>
    /// 反射控制示例
    /// 演示如何在运行时控制反射效果
    /// </summary>
    public class ExampleReflectionController : MonoBehaviour
    {
        [Header("引用")]
        public PlanarReflectionManager reflectionManager;
        public PlanarReflectionPlane reflectionPlane;

        [Header("UI控件(可选)")]
        public Toggle reflectionToggle;
        public Slider intensitySlider;
        public Dropdown qualityDropdown;
        public Dropdown presetDropdown;

        [Header("快捷键")]
        public KeyCode toggleKey = KeyCode.R;
        public KeyCode increaseIntensityKey = KeyCode.Plus;
        public KeyCode decreaseIntensityKey = KeyCode.Minus;
        public KeyCode cycleQualityKey = KeyCode.Q;

        private void Start()
        {
            InitializeUI();
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        /// <summary>
        /// 初始化UI控件
        /// </summary>
        private void InitializeUI()
        {
            // 反射开关Toggle
            if (reflectionToggle != null)
            {
                reflectionToggle.isOn = reflectionManager.enableReflection;
                reflectionToggle.onValueChanged.AddListener(OnReflectionToggleChanged);
            }

            // 强度Slider
            if (intensitySlider != null)
            {
                intensitySlider.value = reflectionPlane.reflectionIntensity;
                intensitySlider.onValueChanged.AddListener(OnIntensitySliderChanged);
            }

            // 质量下拉菜单
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "低质量", "中等质量", "高质量", "超高质量"
                });
                qualityDropdown.value = (int)reflectionManager.runtimeQuality;
                qualityDropdown.onValueChanged.AddListener(OnQualityDropdownChanged);
            }

            // 预设下拉菜单
            if (presetDropdown != null)
            {
                presetDropdown.ClearOptions();
                presetDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "湿滑地面", "干燥沥青", "大理石", "哑光地面", "自定义"
                });
                presetDropdown.onValueChanged.AddListener(OnPresetDropdownChanged);
            }
        }

        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            // 切换反射
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleReflection();
            }

            // 增加强度
            if (Input.GetKeyDown(increaseIntensityKey))
            {
                AdjustIntensity(0.1f);
            }

            // 减少强度
            if (Input.GetKeyDown(decreaseIntensityKey))
            {
                AdjustIntensity(-0.1f);
            }

            // 循环切换质量
            if (Input.GetKeyDown(cycleQualityKey))
            {
                CycleQuality();
            }
        }

        #region 公共方法

        /// <summary>
        /// 切换反射开关
        /// </summary>
        public void ToggleReflection()
        {
            if (reflectionManager != null)
            {
                bool newState = !reflectionManager.enableReflection;
                reflectionManager.ToggleReflection(newState);
                
                if (reflectionToggle != null)
                {
                    reflectionToggle.isOn = newState;
                }
                
                Debug.Log($"反射{(newState ? "已启用" : "已禁用")}");
            }
        }

        /// <summary>
        /// 设置反射强度
        /// </summary>
        public void SetIntensity(float intensity)
        {
            if (reflectionManager != null)
            {
                reflectionManager.SetReflectionIntensity(intensity);
                
                if (intensitySlider != null)
                {
                    intensitySlider.value = intensity;
                }
            }
        }

        /// <summary>
        /// 调整反射强度
        /// </summary>
        public void AdjustIntensity(float delta)
        {
            if (reflectionPlane != null)
            {
                float newIntensity = Mathf.Clamp01(reflectionPlane.reflectionIntensity + delta);
                SetIntensity(newIntensity);
            }
        }

        /// <summary>
        /// 设置质量等级
        /// </summary>
        public void SetQuality(ReflectionQuality quality)
        {
            if (reflectionManager != null)
            {
                reflectionManager.ApplyQualitySettings(quality);
                
                if (qualityDropdown != null)
                {
                    qualityDropdown.value = (int)quality;
                }
                
                Debug.Log($"反射质量已设置为: {quality}");
            }
        }

        /// <summary>
        /// 循环切换质量等级
        /// </summary>
        public void CycleQuality()
        {
            if (reflectionManager == null || reflectionManager.settings == null)
                return;

            var currentQuality = reflectionManager.settings.currentQuality;
            var nextQuality = (ReflectionQuality)(((int)currentQuality + 1) % 4);
            SetQuality(nextQuality);
        }

        /// <summary>
        /// 应用预设效果
        /// </summary>
        public void ApplyPreset(ReflectionPreset preset)
        {
            if (reflectionPlane == null)
                return;

            switch (preset)
            {
                case ReflectionPreset.WetGround:
                    // 湿滑地面(雨后)
                    reflectionPlane.reflectionIntensity = 0.8f;
                    reflectionPlane.roughness = 0.05f;
                    reflectionPlane.reflectionTint = new Color(0.9f, 0.95f, 1.0f);
                    reflectionPlane.fresnelPower = 1.5f;
                    Debug.Log("应用预设: 湿滑地面");
                    break;

                case ReflectionPreset.DryAsphalt:
                    // 干燥沥青
                    reflectionPlane.reflectionIntensity = 0.3f;
                    reflectionPlane.roughness = 0.3f;
                    reflectionPlane.reflectionTint = Color.white;
                    reflectionPlane.fresnelPower = 3.0f;
                    Debug.Log("应用预设: 干燥沥青");
                    break;

                case ReflectionPreset.Marble:
                    // 大理石地面
                    reflectionPlane.reflectionIntensity = 0.9f;
                    reflectionPlane.roughness = 0.02f;
                    reflectionPlane.reflectionTint = Color.white;
                    reflectionPlane.fresnelPower = 2.0f;
                    Debug.Log("应用预设: 大理石");
                    break;

                case ReflectionPreset.Matte:
                    // 哑光地面
                    reflectionPlane.reflectionIntensity = 0.2f;
                    reflectionPlane.roughness = 0.5f;
                    reflectionPlane.reflectionTint = new Color(0.8f, 0.8f, 0.8f);
                    reflectionPlane.fresnelPower = 4.0f;
                    Debug.Log("应用预设: 哑光地面");
                    break;
            }

            UpdateUIFromPlane();
        }

        /// <summary>
        /// 设置淡出距离
        /// </summary>
        public void SetFadeDistance(float start, float end)
        {
            if (reflectionManager != null)
            {
                reflectionManager.SetFadeDistance(start, end);
                Debug.Log($"淡出距离已设置: {start}m - {end}m");
            }
        }

        #endregion

        #region UI回调

        private void OnReflectionToggleChanged(bool isOn)
        {
            if (reflectionManager != null)
            {
                reflectionManager.ToggleReflection(isOn);
            }
        }

        private void OnIntensitySliderChanged(float value)
        {
            SetIntensity(value);
        }

        private void OnQualityDropdownChanged(int index)
        {
            SetQuality((ReflectionQuality)index);
        }

        private void OnPresetDropdownChanged(int index)
        {
            if (index < 4) // 不是"自定义"
            {
                ApplyPreset((ReflectionPreset)index);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 从反射平面更新UI
        /// </summary>
        private void UpdateUIFromPlane()
        {
            if (reflectionPlane == null)
                return;

            if (intensitySlider != null)
            {
                intensitySlider.value = reflectionPlane.reflectionIntensity;
            }
        }

        /// <summary>
        /// 打印当前设置
        /// </summary>
        public void PrintCurrentSettings()
        {
            if (reflectionPlane == null)
                return;

            string settings = $"=== 当前反射设置 ===\n" +
                            $"启用: {reflectionManager?.enableReflection}\n" +
                            $"质量: {reflectionManager?.settings?.currentQuality}\n" +
                            $"强度: {reflectionPlane.reflectionIntensity:F2}\n" +
                            $"粗糙度: {reflectionPlane.roughness:F2}\n" +
                            $"色调: {reflectionPlane.reflectionTint}\n" +
                            $"菲涅尔: {reflectionPlane.fresnelPower:F2}\n" +
                            $"淡出: {reflectionPlane.fadeStart}m - {reflectionPlane.fadeEnd}m";

            Debug.Log(settings);
        }

        #endregion
    }

    /// <summary>
    /// 反射效果预设
    /// </summary>
    public enum ReflectionPreset
    {
        WetGround,    // 湿滑地面
        DryAsphalt,   // 干燥沥青
        Marble,       // 大理石
        Matte         // 哑光
    }
}
