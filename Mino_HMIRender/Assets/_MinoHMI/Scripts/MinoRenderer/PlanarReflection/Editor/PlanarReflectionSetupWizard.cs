using UnityEngine;
using UnityEditor;

namespace MinoHMI.Rendering.Editor
{
    /// <summary>
    /// 平面反射设置向导
    /// 快速配置反射系统
    /// </summary>
    public class PlanarReflectionSetupWizard : EditorWindow
    {
        private const int DefaultReflectionTextureWidth = 512;
        private const int DefaultReflectionTextureHeight = 512;
        private const float MinResolutionScale = 0.25f;
        private const float MaxResolutionScale = 1f;

        private static readonly string[] GroundShaderNames =
        {
            "MinoHMI/PlanarReflectionPlane",
            "MinoHMI/PlanarReflectionTransparentOnly"
        };

        private static readonly string[] GroundShaderDisplayNames =
        {
            "平面反射(完整效果)",
            "平面反射(纯透明)"
        };

        private enum SetupStep
        {
            Welcome,
            CreateSettings,
            SetupCamera,
            SetupGround,
            Complete
        }

        private SetupStep currentStep = SetupStep.Welcome;
        private PlanarReflectionSettings settings;
        private GameObject reflectionCameraObject;
        private GameObject groundObject;
        private Material groundMaterial;
        private int selectedGroundShaderIndex;

        private Vector2 scrollPosition;

        [MenuItem("MinoHMI/工具/平面反射设置向导")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlanarReflectionSetupWizard>("平面反射设置向导");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            // 标题
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("MinoHMI 平面反射设置向导", titleStyle);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"步骤 {(int)currentStep + 1} / 5", EditorStyles.centeredGreyMiniLabel);
            
            EditorGUILayout.Space(10);
            DrawProgressBar();
            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (currentStep)
            {
                case SetupStep.Welcome:
                    DrawWelcomeStep();
                    break;
                case SetupStep.CreateSettings:
                    DrawCreateSettingsStep();
                    break;
                case SetupStep.SetupCamera:
                    DrawSetupCameraStep();
                    break;
                case SetupStep.SetupGround:
                    DrawSetupGroundStep();
                    break;
                case SetupStep.Complete:
                    DrawCompleteStep();
                    break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            DrawNavigationButtons();
        }

        private void DrawProgressBar()
        {
            float progress = ((int)currentStep + 1) / 5f;
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(rect, progress, $"{Mathf.RoundToInt(progress * 100)}%");
        }

        private void DrawWelcomeStep()
        {
            EditorGUILayout.HelpBox(
                "欢迎使用平面反射设置向导!\n\n" +
                "此向导将帮助您快速配置HMI车模的地面反射效果。\n\n" +
                "准备工作:\n" +
                "• 确保场景中已有车辆模型\n" +
                "• 确保已有地面平面或模型\n" +
                "• 确保项目使用URP渲染管线",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("功能特点:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("✓ 实时平面反射渲染");
            EditorGUILayout.LabelField("✓ 多质量等级支持");
            EditorGUILayout.LabelField("✓ 距离淡出效果");
            EditorGUILayout.LabelField("✓ 菲涅尔反射");
            EditorGUILayout.LabelField("✓ 性能优化选项");

            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "点击'下一步'开始配置",
                MessageType.None
            );
        }

        private void DrawCreateSettingsStep()
        {
            EditorGUILayout.LabelField("创建配置文件", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "首先需要创建反射系统的配置文件,用于存储质量预设和性能参数。",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            settings = (PlanarReflectionSettings)EditorGUILayout.ObjectField(
                "配置文件",
                settings,
                typeof(PlanarReflectionSettings),
                false
            );

            if (settings == null)
            {
                EditorGUILayout.Space(5);
                if (GUILayout.Button("创建新配置文件", GUILayout.Height(30)))
                {
                    CreateSettings();
                }
            }
            else
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("✓ 配置文件已准备就绪", MessageType.None);

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("预设质量等级:", EditorStyles.boldLabel);
                
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"低: {settings.lowQuality.resolution.x}x{settings.lowQuality.resolution.y}");
                EditorGUILayout.LabelField($"中: {settings.mediumQuality.resolution.x}x{settings.mediumQuality.resolution.y}");
                EditorGUILayout.LabelField($"高: {settings.highQuality.resolution.x}x{settings.highQuality.resolution.y}");
                EditorGUILayout.LabelField($"超高: {settings.ultraQuality.resolution.x}x{settings.ultraQuality.resolution.y}");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSetupCameraStep()
        {
            EditorGUILayout.LabelField("设置反射相机", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "反射相机用于渲染镜像画面。将在地面位置创建一个相机对象。",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            reflectionCameraObject = (GameObject)EditorGUILayout.ObjectField(
                "反射相机对象",
                reflectionCameraObject,
                typeof(GameObject),
                true
            );

            if (reflectionCameraObject == null)
            {
                EditorGUILayout.Space(5);
                if (GUILayout.Button("创建反射相机", GUILayout.Height(30)))
                {
                    CreateReflectionCamera();
                }
            }
            else
            {
                EditorGUILayout.Space(5);
                var camera = reflectionCameraObject.GetComponent<PlanarReflectionCamera>();
                if (camera != null)
                {
                    if (EnsureCameraSettingsValid(camera))
                    {
                        EditorGUILayout.HelpBox("已自动修正反射分辨率/缩放到有效范围。", MessageType.Warning);
                    }

                    EditorGUILayout.HelpBox("✓ 反射相机已设置", MessageType.None);

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("相机配置:", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"分辨率: {camera.reflectionResolution.x}x{camera.reflectionResolution.y}");
                    EditorGUILayout.LabelField($"HDR: {(camera.useHDR ? "开启" : "关闭")}");
                    EditorGUILayout.LabelField($"最大距离: {camera.maxReflectionDistance}m");
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("该对象缺少PlanarReflectionCamera组件!", MessageType.Warning);
                }
            }
        }

        private void DrawSetupGroundStep()
        {
            EditorGUILayout.LabelField("设置地面反射", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "选择场景中的地面对象,为其配置反射材质和组件。",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            groundObject = (GameObject)EditorGUILayout.ObjectField(
                "地面对象",
                groundObject,
                typeof(GameObject),
                true
            );

            if (groundObject != null)
            {
                EditorGUILayout.Space(5);
                
                var renderer = groundObject.GetComponent<Renderer>();
                if (renderer == null)
                {
                    EditorGUILayout.HelpBox("地面对象需要有Renderer组件!", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField("材质配置:", EditorStyles.boldLabel);
                    selectedGroundShaderIndex = EditorGUILayout.Popup(
                        "反射 Shader",
                        selectedGroundShaderIndex,
                        GroundShaderDisplayNames
                    );
                    groundMaterial = (Material)EditorGUILayout.ObjectField(
                        "反射材质",
                        groundMaterial,
                        typeof(Material),
                        false
                    );

                    if (groundMaterial == null)
                    {
                        if (GUILayout.Button("创建反射材质", GUILayout.Height(25)))
                        {
                            CreateReflectionMaterial();
                        }
                    }
                    else
                    {
                        EditorGUILayout.Space(5);
                        if (GUILayout.Button("应用到地面", GUILayout.Height(30)))
                        {
                            SetupGround();
                        }
                    }
                }

                var plane = groundObject.GetComponent<PlanarReflectionPlane>();
                if (plane != null)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox("✓ 地面反射组件已配置", MessageType.None);
                }
            }
        }

        private void DrawCompleteStep()
        {
            EditorGUILayout.LabelField("设置完成!", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "平面反射系统已配置完成!\n\n" +
                "接下来:\n" +
                "1. 进入Play模式查看效果\n" +
                "2. 调整反射参数以获得最佳效果\n" +
                "3. 根据性能需求选择质量等级",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            if (GUILayout.Button("创建管理器", GUILayout.Height(30)))
            {
                CreateManager();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("快速调整:", EditorStyles.boldLabel);

            if (reflectionCameraObject != null)
            {
                var camera = reflectionCameraObject.GetComponent<PlanarReflectionCamera>();
                if (camera != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("启用反射");
                    camera.enableReflection = EditorGUILayout.Toggle(camera.enableReflection);
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (groundObject != null)
            {
                var plane = groundObject.GetComponent<PlanarReflectionPlane>();
                if (plane != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("反射强度");
                    plane.reflectionIntensity = EditorGUILayout.Slider(plane.reflectionIntensity, 0f, 1f);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "提示: 可以在场景中选择反射相机或地面对象,在Inspector中进一步调整参数。",
                MessageType.None
            );
        }

        private void DrawNavigationButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = currentStep > SetupStep.Welcome;
            if (GUILayout.Button("< 上一步", GUILayout.Height(30)))
            {
                currentStep--;
            }
            GUI.enabled = true;

            if (currentStep < SetupStep.Complete)
            {
                bool canProceed = CanProceedToNextStep();
                GUI.enabled = canProceed;
                if (GUILayout.Button("下一步 >", GUILayout.Height(30)))
                {
                    currentStep++;
                }
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("完成", GUILayout.Height(30)))
                {
                    Close();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool CanProceedToNextStep()
        {
            switch (currentStep)
            {
                case SetupStep.Welcome:
                    return true;
                case SetupStep.CreateSettings:
                    return settings != null;
                case SetupStep.SetupCamera:
                    return reflectionCameraObject != null && 
                           reflectionCameraObject.GetComponent<PlanarReflectionCamera>() != null;
                case SetupStep.SetupGround:
                    return groundObject != null && 
                           groundObject.GetComponent<PlanarReflectionPlane>() != null;
                default:
                    return true;
            }
        }

        private void CreateSettings()
        {
            settings = ScriptableObject.CreateInstance<PlanarReflectionSettings>();
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存反射设置",
                "PlanarReflectionSettings",
                "asset",
                "选择保存位置"
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(settings);
                Debug.Log($"已创建反射设置: {path}");
            }
        }

        private void CreateReflectionCamera()
        {
            reflectionCameraObject = new GameObject("ReflectionPlane");
            reflectionCameraObject.transform.position = Vector3.zero;
            reflectionCameraObject.transform.rotation = Quaternion.identity;

            var camera = reflectionCameraObject.AddComponent<PlanarReflectionCamera>();
            camera.reflectionResolution = new Vector2Int(DefaultReflectionTextureWidth, DefaultReflectionTextureHeight);
            camera.resolutionScale = MaxResolutionScale;
            camera.useHDR = false;
            camera.maxReflectionDistance = 50f;
            EnsureCameraSettingsValid(camera);

            Undo.RegisterCreatedObjectUndo(reflectionCameraObject, "Create Reflection Camera");
            Selection.activeGameObject = reflectionCameraObject;
            
            Debug.Log("已创建反射相机");
        }

        /// <summary>
        /// 校验并修正反射相机关键参数，避免创建 0 尺寸 RT
        /// </summary>
        private bool EnsureCameraSettingsValid(PlanarReflectionCamera camera)
        {
            if (camera == null)
                return false;

            bool hasModified = false;
            Vector2Int sanitizedResolution = camera.reflectionResolution;
            float sanitizedScale = camera.resolutionScale;

            if (sanitizedResolution.x <= 0)
            {
                sanitizedResolution.x = DefaultReflectionTextureWidth;
                hasModified = true;
            }

            if (sanitizedResolution.y <= 0)
            {
                sanitizedResolution.y = DefaultReflectionTextureHeight;
                hasModified = true;
            }

            float clampedScale = Mathf.Clamp(sanitizedScale, MinResolutionScale, MaxResolutionScale);
            if (!Mathf.Approximately(clampedScale, sanitizedScale))
            {
                sanitizedScale = clampedScale;
                hasModified = true;
            }

            if (hasModified)
            {
                Undo.RecordObject(camera, "Sanitize Reflection Camera Settings");
                camera.reflectionResolution = sanitizedResolution;
                camera.resolutionScale = sanitizedScale;
                EditorUtility.SetDirty(camera);
            }

            return hasModified;
        }

        private void CreateReflectionMaterial()
        {
            selectedGroundShaderIndex = Mathf.Clamp(selectedGroundShaderIndex, 0, GroundShaderNames.Length - 1);
            string shaderName = GroundShaderNames[selectedGroundShaderIndex];
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("错误", $"找不到反射 Shader: {shaderName}", "确定");
                return;
            }

            groundMaterial = new Material(shader);
            string materialName = shaderName.Substring(shaderName.LastIndexOf('/') + 1);
            groundMaterial.name = materialName;

            string path = EditorUtility.SaveFilePanelInProject(
                "保存反射材质",
                materialName,
                "mat",
                "选择保存位置"
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(groundMaterial, path);
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(groundMaterial);
                Debug.Log($"已创建反射材质: {path}");
            }
        }

        private void SetupGround()
        {
            if (groundObject == null || groundMaterial == null)
                return;

            var renderer = groundObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Undo.RecordObject(renderer, "Apply Reflection Material");
                renderer.sharedMaterial = groundMaterial;
            }

            var plane = groundObject.GetComponent<PlanarReflectionPlane>();
            if (plane == null)
            {
                Undo.AddComponent<PlanarReflectionPlane>(groundObject);
                plane = groundObject.GetComponent<PlanarReflectionPlane>();
            }

            if (plane != null)
            {
                Undo.RecordObject(plane, "Setup Reflection Plane");
                plane.reflectionIntensity = 0.5f;
                plane.reflectionBlur = 0.0f;
                plane.roughness = 0.1f;
                plane.fresnelPower = 2.0f;
                plane.fadeStart = 20f;
                plane.fadeEnd = 50f;
            }

            Debug.Log("已配置地面反射");
        }

        private void CreateManager()
        {
            GameObject managerObject = new GameObject("ReflectionManager");
            var manager = managerObject.AddComponent<PlanarReflectionManager>();

            if (settings != null)
                manager.settings = settings;

            if (reflectionCameraObject != null)
                manager.reflectionCamera = reflectionCameraObject.GetComponent<PlanarReflectionCamera>();

            if (groundObject != null)
            {
                var plane = groundObject.GetComponent<PlanarReflectionPlane>();
                if (plane != null)
                {
                    manager.reflectionPlanes = new PlanarReflectionPlane[] { plane };
                }
            }

            Undo.RegisterCreatedObjectUndo(managerObject, "Create Reflection Manager");
            Selection.activeGameObject = managerObject;
            
            Debug.Log("已创建反射管理器");
        }
    }
}
