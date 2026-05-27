using UnityEngine;
using UnityEngine.UI;

namespace MinoHMI.Rendering.Examples
{
    /// <summary>
    /// 纯反射效果演示
    /// 展示不同反射效果的切换
    /// </summary>
    public class PureReflectionDemo : MonoBehaviour
    {
        [Header("反射材质")]
        [Tooltip("使用平面反射 Shader 的材质")]
        public Material reflectionMaterial;

        [Header("UI 控件(可选)")]
        public Slider intensitySlider;
        public Slider alphaSlider;
        public Slider fresnelPowerSlider;
        public Slider distortionSlider;
        public Dropdown presetDropdown;
        public Toggle waterWaveToggle;

        [Header("预设效果")]
        public ReflectionPresetData[] presets = new ReflectionPresetData[]
        {
            new ReflectionPresetData { name = "完美镜面", preset = PureReflectionPreset.PerfectMirror },
            new ReflectionPresetData { name = "静态水面", preset = PureReflectionPreset.StaticWater },
            new ReflectionPresetData { name = "动态水面", preset = PureReflectionPreset.DynamicWater },
            new ReflectionPresetData { name = "玻璃反射", preset = PureReflectionPreset.Glass },
            new ReflectionPresetData { name = "金属镜面", preset = PureReflectionPreset.Metal }
        };

        // Shader 属性 ID
        private static readonly int ReflectionIntensityID = Shader.PropertyToID("_ReflectionIntensity");
        private static readonly int ReflectionTintID = Shader.PropertyToID("_ReflectionTint");
        private static readonly int AlphaID = Shader.PropertyToID("_Alpha");
        private static readonly int FresnelPowerID = Shader.PropertyToID("_FresnelPower");
        private static readonly int FresnelStrengthID = Shader.PropertyToID("_FresnelStrength");
        private static readonly int FresnelColorID = Shader.PropertyToID("_FresnelColor");
        private static readonly int DistortionStrengthID = Shader.PropertyToID("_DistortionStrength");
        private static readonly int DistortionSpeedID = Shader.PropertyToID("_DistortionSpeed");

        private void Start()
        {
            InitializeUI();
            
            // 默认应用第一个预设
            if (presets.Length > 0)
            {
                ApplyPreset(presets[0].preset);
            }
        }

        private void InitializeUI()
        {
            // 强度滑块
            if (intensitySlider != null)
            {
                intensitySlider.minValue = 0f;
                intensitySlider.maxValue = 1f;
                intensitySlider.value = 1f;
                intensitySlider.onValueChanged.AddListener(SetIntensity);
            }

            // 透明度滑块
            if (alphaSlider != null)
            {
                alphaSlider.minValue = 0f;
                alphaSlider.maxValue = 1f;
                alphaSlider.value = 1f;
                alphaSlider.onValueChanged.AddListener(SetAlpha);
            }

            // 菲涅尔滑块
            if (fresnelPowerSlider != null)
            {
                fresnelPowerSlider.minValue = 0f;
                fresnelPowerSlider.maxValue = 5f;
                fresnelPowerSlider.value = 2f;
                fresnelPowerSlider.onValueChanged.AddListener(SetFresnelPower);
            }

            // 扭曲滑块
            if (distortionSlider != null)
            {
                distortionSlider.minValue = 0f;
                distortionSlider.maxValue = 0.1f;
                distortionSlider.value = 0f;
                distortionSlider.onValueChanged.AddListener(SetDistortion);
            }

            // 预设下拉菜单
            if (presetDropdown != null)
            {
                presetDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>();
                foreach (var preset in presets)
                {
                    options.Add(preset.name);
                }
                presetDropdown.AddOptions(options);
                presetDropdown.onValueChanged.AddListener(OnPresetChanged);
            }

            // 水波开关
            if (waterWaveToggle != null)
            {
                waterWaveToggle.isOn = false;
                waterWaveToggle.onValueChanged.AddListener(OnWaterWaveToggle);
            }
        }

        #region 参数设置

        /// <summary>
        /// 设置反射强度
        /// </summary>
        public void SetIntensity(float value)
        {
            if (reflectionMaterial != null)
            {
                reflectionMaterial.SetFloat(ReflectionIntensityID, value);
            }
        }

        /// <summary>
        /// 设置透明度
        /// </summary>
        public void SetAlpha(float value)
        {
            if (reflectionMaterial != null)
            {
                reflectionMaterial.SetFloat(AlphaID, value);
            }
        }

        /// <summary>
        /// 设置反射色调
        /// </summary>
        public void SetTint(Color color)
        {
            if (reflectionMaterial != null)
            {
                reflectionMaterial.SetColor(ReflectionTintID, color);
            }
        }

        /// <summary>
        /// 设置菲涅尔强度
        /// </summary>
        public void SetFresnelPower(float value)
        {
            if (reflectionMaterial != null)
            {
                reflectionMaterial.SetFloat(FresnelPowerID, value);
            }
        }

        /// <summary>
        /// 设置菲涅尔混合强度
        /// </summary>
        public void SetFresnelStrength(float value)
        {
            if (reflectionMaterial != null)
            {
                reflectionMaterial.SetFloat(FresnelStrengthID, value);
            }
        }

        /// <summary>
        /// 设置菲涅尔颜色
        /// </summary>
        public void SetFresnelColor(Color color)
        {
            if (reflectionMaterial != null)
            {
                reflectionMaterial.SetColor(FresnelColorID, color);
            }
        }

        /// <summary>
        /// 设置扭曲强度
        /// </summary>
        public void SetDistortion(float value)
        {
            if (reflectionMaterial != null)
            {
                reflectionMaterial.SetFloat(DistortionStrengthID, value);
            }
        }

        /// <summary>
        /// 设置扭曲速度
        /// </summary>
        public void SetDistortionSpeed(float value)
        {
            if (reflectionMaterial != null)
            {
                reflectionMaterial.SetFloat(DistortionSpeedID, value);
            }
        }

        #endregion

        #region 预设效果

        /// <summary>
        /// 应用预设效果
        /// </summary>
        public void ApplyPreset(PureReflectionPreset preset)
        {
            if (reflectionMaterial == null)
            {
                Debug.LogWarning("反射材质未设置!");
                return;
            }

            switch (preset)
            {
                case PureReflectionPreset.PerfectMirror:
                    ApplyPerfectMirror();
                    break;

                case PureReflectionPreset.StaticWater:
                    ApplyStaticWater();
                    break;

                case PureReflectionPreset.DynamicWater:
                    ApplyDynamicWater();
                    break;

                case PureReflectionPreset.Glass:
                    ApplyGlass();
                    break;

                case PureReflectionPreset.Metal:
                    ApplyMetal();
                    break;
            }

            UpdateUIFromMaterial();
        }

        /// <summary>
        /// 完美镜面
        /// </summary>
        private void ApplyPerfectMirror()
        {
            SetIntensity(1.0f);
            SetTint(Color.white);
            SetAlpha(1.0f);
            SetFresnelPower(0.0f);
            SetFresnelStrength(0.0f);
            SetDistortion(0.0f);
            
            Debug.Log("应用预设: 完美镜面");
        }

        /// <summary>
        /// 静态水面
        /// </summary>
        private void ApplyStaticWater()
        {
            SetIntensity(0.8f);
            SetTint(new Color(0.8f, 0.9f, 1.0f));
            SetAlpha(0.9f);
            SetFresnelPower(2.0f);
            SetFresnelStrength(0.3f);
            SetFresnelColor(new Color(0.7f, 0.85f, 1.0f));
            SetDistortion(0.0f);
            
            Debug.Log("应用预设: 静态水面");
        }

        /// <summary>
        /// 动态水面
        /// </summary>
        private void ApplyDynamicWater()
        {
            SetIntensity(0.8f);
            SetTint(new Color(0.8f, 0.9f, 1.0f));
            SetAlpha(0.9f);
            SetFresnelPower(2.0f);
            SetFresnelStrength(0.3f);
            SetFresnelColor(new Color(0.7f, 0.85f, 1.0f));
            SetDistortion(0.02f);
            SetDistortionSpeed(1.5f);
            
            Debug.Log("应用预设: 动态水面");
        }

        /// <summary>
        /// 玻璃反射
        /// </summary>
        private void ApplyGlass()
        {
            SetIntensity(0.6f);
            SetTint(new Color(0.95f, 0.95f, 1.0f));
            SetAlpha(0.7f);
            SetFresnelPower(3.0f);
            SetFresnelStrength(0.5f);
            SetFresnelColor(Color.white);
            SetDistortion(0.0f);
            
            Debug.Log("应用预设: 玻璃反射");
        }

        /// <summary>
        /// 金属镜面
        /// </summary>
        private void ApplyMetal()
        {
            SetIntensity(1.0f);
            SetTint(new Color(0.9f, 0.85f, 0.8f));
            SetAlpha(1.0f);
            SetFresnelPower(1.5f);
            SetFresnelStrength(0.2f);
            SetFresnelColor(new Color(1.0f, 0.95f, 0.9f));
            SetDistortion(0.0f);
            
            Debug.Log("应用预设: 金属镜面");
        }

        #endregion

        #region UI 回调

        private void OnPresetChanged(int index)
        {
            if (index >= 0 && index < presets.Length)
            {
                ApplyPreset(presets[index].preset);
            }
        }

        private void OnWaterWaveToggle(bool enabled)
        {
            if (enabled)
            {
                SetDistortion(0.02f);
                SetDistortionSpeed(1.5f);
            }
            else
            {
                SetDistortion(0.0f);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 从材质更新UI
        /// </summary>
        private void UpdateUIFromMaterial()
        {
            if (reflectionMaterial == null)
                return;

            if (intensitySlider != null)
                intensitySlider.value = reflectionMaterial.GetFloat(ReflectionIntensityID);

            if (alphaSlider != null)
                alphaSlider.value = reflectionMaterial.GetFloat(AlphaID);

            if (fresnelPowerSlider != null)
                fresnelPowerSlider.value = reflectionMaterial.GetFloat(FresnelPowerID);

            if (distortionSlider != null)
                distortionSlider.value = reflectionMaterial.GetFloat(DistortionStrengthID);

            if (waterWaveToggle != null)
                waterWaveToggle.isOn = reflectionMaterial.GetFloat(DistortionStrengthID) > 0.001f;
        }

        /// <summary>
        /// 打印当前设置
        /// </summary>
        public void PrintCurrentSettings()
        {
            if (reflectionMaterial == null)
                return;

            string settings = $"=== 当前反射设置 ===\n" +
                            $"强度: {reflectionMaterial.GetFloat(ReflectionIntensityID):F2}\n" +
                            $"透明度: {reflectionMaterial.GetFloat(AlphaID):F2}\n" +
                            $"色调: {reflectionMaterial.GetColor(ReflectionTintID)}\n" +
                            $"菲涅尔强度: {reflectionMaterial.GetFloat(FresnelPowerID):F2}\n" +
                            $"扭曲: {reflectionMaterial.GetFloat(DistortionStrengthID):F3}";

            Debug.Log(settings);
        }

        #endregion
    }

    /// <summary>
    /// 纯反射预设类型
    /// </summary>
    public enum PureReflectionPreset
    {
        PerfectMirror,  // 完美镜面
        StaticWater,    // 静态水面
        DynamicWater,   // 动态水面
        Glass,          // 玻璃
        Metal           // 金属
    }

    /// <summary>
    /// 预设数据
    /// </summary>
    [System.Serializable]
    public class ReflectionPresetData
    {
        public string name;
        public PureReflectionPreset preset;
    }
}
