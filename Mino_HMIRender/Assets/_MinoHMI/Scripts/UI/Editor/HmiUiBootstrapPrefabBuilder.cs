#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MinoHMI.UI.Application;
using MinoHMI.UI.Core;
using MinoHMI.UI.Interaction;
using MinoHMI.UI.Performance;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace MinoHMI.UI.Editor
{
    /// <summary>
    /// 生成可拖入场景的 HMI UI Bootstrap 预制体。
    /// </summary>
    public static class HmiUiBootstrapPrefabBuilder
    {
        private const string PrefabFolder = "Assets/_MinoHMI/Prefabs/UI";
        private const string PrefabPath = PrefabFolder + "/HMI_UIBootstrap.prefab";
        private const string QualityFolder = "Assets/_MinoHMI/Settings/UI/Performance";

        [MenuItem("MinoHMI/UI/Create UIBootstrap Prefab")]
        public static void CreateOrUpdatePrefab()
        {
            EnsureFolders();

            GameObject root = BuildHierarchy();
            try
            {
                GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Selection.activeObject = prefabAsset;
                EditorGUIUtility.PingObject(prefabAsset);
                Debug.Log($"[HmiUiBootstrapPrefabBuilder] 已生成预制体：{PrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_MinoHMI/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_MinoHMI", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/_MinoHMI/Prefabs", "UI");
            }

            if (!AssetDatabase.IsValidFolder("Assets/_MinoHMI/Settings"))
            {
                AssetDatabase.CreateFolder("Assets/_MinoHMI", "Settings");
            }

            if (!AssetDatabase.IsValidFolder("Assets/_MinoHMI/Settings/UI"))
            {
                AssetDatabase.CreateFolder("Assets/_MinoHMI/Settings", "UI");
            }

            if (!AssetDatabase.IsValidFolder(QualityFolder))
            {
                AssetDatabase.CreateFolder("Assets/_MinoHMI/Settings/UI", "Performance");
            }
        }

        private static GameObject BuildHierarchy()
        {
            GameObject root = new GameObject("HMI_UIBootstrap");

            UIRoot uiRoot = root.AddComponent<UIRoot>();
            UIPageController pageController = root.AddComponent<UIPageController>();
            UICommandCenter commandCenter = root.AddComponent<UICommandCenter>();
            UICommandBridge commandBridge = root.AddComponent<UICommandBridge>();
            HmiUiBootstrapBinder bootstrapBinder = root.AddComponent<HmiUiBootstrapBinder>();
            UIInteractionArbiter interactionArbiter = root.AddComponent<UIInteractionArbiter>();
            UiPerformanceGovernor performanceGovernor = root.AddComponent<UiPerformanceGovernor>();
            root.AddComponent<UiFrameBudgetWatcher>();

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.transform.SetParent(root.transform, false);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();

            GameObject canvasObject = CreateCanvasRoot(root.transform);
            UILayerStack layerStack = canvasObject.AddComponent<UILayerStack>();

            RectTransform layerBase = CreateLayer(canvasObject.transform, "Layer_Base");
            RectTransform layerPopup = CreateLayer(canvasObject.transform, "Layer_Popup");
            RectTransform layerSystem = CreateLayer(canvasObject.transform, "Layer_System");

            UIPageBase pageHome = CreatePage(layerBase, "Page_Home", UIPageId.Home, new Vector2(0f, 80f));
            UIPageBase pageCarPaint = CreatePage(layerBase, "Page_CarPaint", UIPageId.CarPaint, new Vector2(0f, -120f));

            ConfigureLayerStack(layerStack, layerBase, layerPopup, layerSystem);
            ConfigurePageController(pageController, layerStack, pageHome, pageCarPaint);
            ConfigureUiRoot(uiRoot, pageController);

            GameObject useCasesRoot = new GameObject("UseCases");
            useCasesRoot.transform.SetParent(root.transform, false);

            List<UICommandBridge.CommandRoute> commandRoutes = new List<UICommandBridge.CommandRoute>();
            CreateCarPaintPresetUseCases(useCasesRoot.transform, commandRoutes);

            ConfigureCommandBridge(commandBridge, commandCenter, commandRoutes);

            ConfigureBootstrapBinder(bootstrapBinder, commandBridge);
            ConfigureInteractionArbiter(interactionArbiter);
            ConfigurePerformanceGovernor(performanceGovernor);

            CreateLabel(pageHome.transform, "PageHome_Label", "3D 车模 HMI", new Vector2(0f, 220f));

            RectTransform colorDock = CreateCarPaintColorDock(layerBase);
            CreateCarPaintColorSwatches(colorDock, commandCenter);

            return root;
        }

        private static GameObject CreateCanvasRoot(Transform parent)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject;
        }

        private static RectTransform CreateLayer(Transform parent, string layerName)
        {
            GameObject layerObject = new GameObject(layerName);
            layerObject.transform.SetParent(parent, false);
            RectTransform rectTransform = layerObject.AddComponent<RectTransform>();
            StretchFull(rectTransform);
            return rectTransform;
        }

        private static RectTransform CreateCarPaintColorDock(RectTransform parent)
        {
            GameObject dockObject = new GameObject("CarPaintColorDock");
            dockObject.transform.SetParent(parent, false);
            RectTransform rectTransform = dockObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.sizeDelta = new Vector2(0f, 150f);
            rectTransform.anchoredPosition = Vector2.zero;

            Image background = dockObject.AddComponent<Image>();
            background.color = new Color(0.06f, 0.06f, 0.06f, 0.92f);

            HorizontalLayoutGroup layoutGroup = dockObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(32, 32, 20, 20);
            layoutGroup.spacing = 20f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            return rectTransform;
        }

        private static CarPaintSwitchUseCase[] CreateCarPaintPresetUseCases(
            Transform useCasesRoot,
            List<UICommandBridge.CommandRoute> commandRoutes)
        {
            CarPaintSwitchUseCase[] useCases = new CarPaintSwitchUseCase[HmiUiCommandIds.CarPaintPresetCount];
            for (int presetIndex = 0; presetIndex < HmiUiCommandIds.CarPaintPresetCount; presetIndex++)
            {
                useCases[presetIndex] = CreateCarPaintUseCase(
                    useCasesRoot,
                    $"UseCase_CarPaintPreset{presetIndex}",
                    CarPaintSwitchUseCase.SwitchMode.SpecifiedPreset,
                    presetIndex);

                commandRoutes.Add(CreateRoute(
                    HmiUiCommandIds.GetCarPaintPresetCommand(presetIndex),
                    useCases[presetIndex]));
            }

            return useCases;
        }

        private static void CreateCarPaintColorSwatches(
            RectTransform colorDock,
            UICommandCenter commandCenter)
        {
            Color[] swatchColors =
            {
                Color.white,
                Color.black,
                new Color(1f, 0.01f, 0f, 1f),
                new Color(0f, 0.82f, 1f, 1f),
                new Color(0.42f, 0.42f, 0.42f, 1f),
                new Color(1f, 0.73f, 0f, 1f)
            };

            string[] swatchNames =
            {
                "Swatch_White",
                "Swatch_Black",
                "Swatch_Red",
                "Swatch_Blue",
                "Swatch_Gray",
                "Swatch_Yellow"
            };

            for (int index = 0; index < HmiUiCommandIds.CarPaintPresetCount; index++)
            {
                CreateColorSwatchButton(
                    colorDock,
                    swatchNames[index],
                    swatchColors[index],
                    HmiUiCommandIds.GetCarPaintPresetCommand(index),
                    commandCenter);
            }
        }

        private static void CreateColorSwatchButton(
            RectTransform parent,
            string objectName,
            Color swatchColor,
            string commandId,
            UICommandCenter commandCenter)
        {
            GameObject buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(parent, false);

            LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 96f;
            layoutElement.preferredHeight = 96f;

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(96f, 96f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = swatchColor;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            GameObject borderObject = new GameObject("Border");
            borderObject.transform.SetParent(buttonObject.transform, false);
            RectTransform borderRect = borderObject.AddComponent<RectTransform>();
            StretchFull(borderRect);
            borderRect.offsetMin = new Vector2(-3f, -3f);
            borderRect.offsetMax = new Vector2(3f, 3f);
            Image borderImage = borderObject.AddComponent<Image>();
            borderImage.color = new Color(1f, 1f, 1f, 0.35f);
            borderImage.raycastTarget = false;
            borderObject.transform.SetAsFirstSibling();

            UIButtonCommandBinder binder = buttonObject.AddComponent<UIButtonCommandBinder>();
            SerializedObject serializedBinder = new SerializedObject(binder);
            serializedBinder.FindProperty("commandCenter").objectReferenceValue = commandCenter;
            serializedBinder.FindProperty("commandId").stringValue = commandId;
            serializedBinder.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UIPageBase CreatePage(RectTransform parent, string pageName, UIPageId pageId, Vector2 anchoredPosition)
        {
            GameObject pageObject = new GameObject(pageName);
            pageObject.transform.SetParent(parent, false);
            RectTransform rectTransform = pageObject.AddComponent<RectTransform>();
            StretchFull(rectTransform);
            rectTransform.anchoredPosition = anchoredPosition;

            Image background = pageObject.AddComponent<Image>();
            background.color = new Color(0.12f, 0.12f, 0.12f, 0.35f);

            UIPageBase page = pageObject.AddComponent<UIPageBase>();
            SerializedObject serializedPage = new SerializedObject(page);
            serializedPage.FindProperty("pageId").enumValueIndex = (int)pageId;
            serializedPage.FindProperty("layer").enumValueIndex = (int)UIPageLayer.Base;
            serializedPage.FindProperty("root").objectReferenceValue = pageObject;
            serializedPage.ApplyModifiedPropertiesWithoutUndo();
            return page;
        }

        private static void CreateLabel(Transform parent, string objectName, string label, Vector2 anchoredPosition)
        {
            GameObject labelObject = new GameObject(objectName);
            labelObject.transform.SetParent(parent, false);
            RectTransform rectTransform = labelObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(800f, 80f);
            rectTransform.anchoredPosition = anchoredPosition;
            Text text = labelObject.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 36;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static CarPaintSwitchUseCase CreateCarPaintUseCase(
            Transform parent,
            string objectName,
            CarPaintSwitchUseCase.SwitchMode switchMode,
            int presetIndex)
        {
            GameObject useCaseObject = new GameObject(objectName);
            useCaseObject.transform.SetParent(parent, false);
            CarPaintSwitchUseCase useCase = useCaseObject.AddComponent<CarPaintSwitchUseCase>();
            SerializedObject serializedUseCase = new SerializedObject(useCase);
            serializedUseCase.FindProperty("switchMode").enumValueIndex = (int)switchMode;
            serializedUseCase.FindProperty("presetIndex").intValue = presetIndex;
            serializedUseCase.ApplyModifiedPropertiesWithoutUndo();
            return useCase;
        }

        private static CameraPresetUseCase CreateCameraUseCase(
            Transform parent,
            string objectName,
            CameraPresetUseCase.PresetOperation operation,
            int presetIndex)
        {
            GameObject useCaseObject = new GameObject(objectName);
            useCaseObject.transform.SetParent(parent, false);
            CameraPresetUseCase useCase = useCaseObject.AddComponent<CameraPresetUseCase>();
            SerializedObject serializedUseCase = new SerializedObject(useCase);
            serializedUseCase.FindProperty("operation").enumValueIndex = (int)operation;
            serializedUseCase.FindProperty("presetIndex").intValue = presetIndex;
            serializedUseCase.ApplyModifiedPropertiesWithoutUndo();
            return useCase;
        }

        private static UiPageNavigationUseCase CreatePageNavUseCase(
            Transform parent,
            string objectName,
            UIPageId pageId,
            UIPageController pageController)
        {
            GameObject useCaseObject = new GameObject(objectName);
            useCaseObject.transform.SetParent(parent, false);
            UiPageNavigationUseCase useCase = useCaseObject.AddComponent<UiPageNavigationUseCase>();
            SerializedObject serializedUseCase = new SerializedObject(useCase);
            serializedUseCase.FindProperty("pageController").objectReferenceValue = pageController;
            serializedUseCase.FindProperty("targetPageId").enumValueIndex = (int)pageId;
            serializedUseCase.ApplyModifiedPropertiesWithoutUndo();
            return useCase;
        }

        private static UICommandBridge.CommandRoute CreateRoute(string commandId, MonoBehaviour executor)
        {
            return new UICommandBridge.CommandRoute
            {
                commandId = commandId,
                executorBehaviour = executor
            };
        }

        private static void ConfigureLayerStack(
            UILayerStack layerStack,
            RectTransform layerBase,
            RectTransform layerPopup,
            RectTransform layerSystem)
        {
            SerializedObject serializedLayerStack = new SerializedObject(layerStack);
            SerializedProperty layerRoots = serializedLayerStack.FindProperty("layerRoots");
            layerRoots.arraySize = 3;
            SetLayerRoot(layerRoots.GetArrayElementAtIndex(0), UIPageLayer.Base, layerBase);
            SetLayerRoot(layerRoots.GetArrayElementAtIndex(1), UIPageLayer.Popup, layerPopup);
            SetLayerRoot(layerRoots.GetArrayElementAtIndex(2), UIPageLayer.System, layerSystem);
            serializedLayerStack.ApplyModifiedPropertiesWithoutUndo();
            layerStack.RebuildLayerRootMap();
        }

        private static void SetLayerRoot(SerializedProperty element, UIPageLayer layer, RectTransform root)
        {
            element.FindPropertyRelative("layer").enumValueIndex = (int)layer;
            element.FindPropertyRelative("root").objectReferenceValue = root;
        }

        private static void ConfigurePageController(
            UIPageController pageController,
            UILayerStack layerStack,
            UIPageBase pageHome,
            UIPageBase pageCarPaint)
        {
            SerializedObject serializedController = new SerializedObject(pageController);
            serializedController.FindProperty("layerStack").objectReferenceValue = layerStack;
            SerializedProperty pages = serializedController.FindProperty("registeredPages");
            pages.arraySize = 2;
            pages.GetArrayElementAtIndex(0).objectReferenceValue = pageHome;
            pages.GetArrayElementAtIndex(1).objectReferenceValue = pageCarPaint;
            serializedController.FindProperty("startupPageId").enumValueIndex = (int)UIPageId.Home;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureUiRoot(UIRoot uiRoot, UIPageController pageController)
        {
            SerializedObject serializedRoot = new SerializedObject(uiRoot);
            serializedRoot.FindProperty("pageController").objectReferenceValue = pageController;
            serializedRoot.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureCommandBridge(
            UICommandBridge commandBridge,
            UICommandCenter commandCenter,
            List<UICommandBridge.CommandRoute> routes)
        {
            SerializedObject serializedBridge = new SerializedObject(commandBridge);
            serializedBridge.FindProperty("commandCenter").objectReferenceValue = commandCenter;
            SerializedProperty commandRoutes = serializedBridge.FindProperty("commandRoutes");
            commandRoutes.arraySize = routes.Count;
            for (int index = 0; index < routes.Count; index++)
            {
                SerializedProperty route = commandRoutes.GetArrayElementAtIndex(index);
                route.FindPropertyRelative("commandId").stringValue = routes[index].commandId;
                route.FindPropertyRelative("executorBehaviour").objectReferenceValue = routes[index].executorBehaviour;
            }

            serializedBridge.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureBootstrapBinder(HmiUiBootstrapBinder binder, UICommandBridge commandBridge)
        {
            SerializedObject serializedBinder = new SerializedObject(binder);
            serializedBinder.FindProperty("autoBindSceneReferences").boolValue = true;
            serializedBinder.FindProperty("commandBridge").objectReferenceValue = commandBridge;
            serializedBinder.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureInteractionArbiter(UIInteractionArbiter arbiter)
        {
            SerializedObject serializedArbiter = new SerializedObject(arbiter);
            serializedArbiter.FindProperty("blockCameraWhenPointerOverUi").boolValue = true;
            serializedArbiter.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigurePerformanceGovernor(UiPerformanceGovernor governor)
        {
            UniversalRenderPipelineAsset urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                "Assets/Settings/URP-HMI.asset");

            UrpQualityLevelProfile lowProfile = LoadOrCreateQualityProfile(
                "UrpQuality_Low",
                "Low",
                urpAsset,
                0.75f,
                1,
                35f,
                50f);
            UrpQualityLevelProfile mediumProfile = LoadOrCreateQualityProfile(
                "UrpQuality_Medium",
                "Medium",
                urpAsset,
                0.9f,
                1,
                42f,
                57f);
            UrpQualityLevelProfile highProfile = LoadOrCreateQualityProfile(
                "UrpQuality_High",
                "High",
                urpAsset,
                1f,
                1,
                48f,
                60f);

            SerializedObject serializedGovernor = new SerializedObject(governor);
            serializedGovernor.FindProperty("targetUrpAsset").objectReferenceValue = urpAsset;
            SerializedProperty profiles = serializedGovernor.FindProperty("qualityProfiles");
            profiles.arraySize = 3;
            profiles.GetArrayElementAtIndex(0).objectReferenceValue = lowProfile;
            profiles.GetArrayElementAtIndex(1).objectReferenceValue = mediumProfile;
            profiles.GetArrayElementAtIndex(2).objectReferenceValue = highProfile;
            serializedGovernor.FindProperty("startupProfileIndex").intValue = 1;
            serializedGovernor.FindProperty("enableAutoAdjust").boolValue = true;
            serializedGovernor.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UrpQualityLevelProfile LoadOrCreateQualityProfile(
            string assetName,
            string profileName,
            UniversalRenderPipelineAsset pipelineAsset,
            float renderScale,
            int msaaSampleCount,
            float downgradeThreshold,
            float upgradeThreshold)
        {
            string assetPath = $"{QualityFolder}/{assetName}.asset";
            UrpQualityLevelProfile profile = AssetDatabase.LoadAssetAtPath<UrpQualityLevelProfile>(assetPath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<UrpQualityLevelProfile>();
                AssetDatabase.CreateAsset(profile, assetPath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileName").stringValue = profileName;
            serializedProfile.FindProperty("pipelineAsset").objectReferenceValue = pipelineAsset;
            serializedProfile.FindProperty("renderScale").floatValue = renderScale;
            serializedProfile.FindProperty("msaaSampleCount").intValue = msaaSampleCount;
            serializedProfile.FindProperty("downgradeFpsThreshold").floatValue = downgradeThreshold;
            serializedProfile.FindProperty("upgradeFpsThreshold").floatValue = upgradeThreshold;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            return profile;
        }

        private static void StretchFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}
#endif
