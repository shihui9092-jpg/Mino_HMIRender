#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MinoHMI.CarPaint.Editor
{
    /// <summary>
    /// 为 CarRoot 预制体配置 CarPaintSwitcher 与 6 款车漆预设（顺序与 UI 色条一致）。
    /// </summary>
    public static class CarRootCarPaintSetup
    {
        private const string CarRootPrefabPath =
            "Assets/_MinoHMI/ArtResources/Models_3D/CarMod/Car/FX12-A2Car/CarRoot.prefab";

        private static readonly string[] PresetDisplayNames =
        {
            "珍珠白", "曜石黑", "烈焰红", "晴空蓝", "金属灰", "竞速黄"
        };

        private static readonly string[] PresetMaterialPaths =
        {
            "Assets/_MinoHMI/ArtResources/Models_3D/CarMod/Car/FX12-A2Car/MatOther/FX12_A2_CarPaintWhite.mat",
            "Assets/_MinoHMI/ArtResources/Models_3D/CarMod/Car/FX12-A2Car/MatOther/FX12_A2_CarPaintBlack.mat",
            "Assets/_MinoHMI/ArtResources/Models_3D/CarMod/Car/FX12-A2Car/MatOther/FX12_A2_CarPaintRed.mat",
            "Assets/_MinoHMI/ArtResources/Models_3D/CarMod/Car/FX12-A2Car/MatOther/FX12_A2_CarPaintBlue.mat",
            "Assets/_MinoHMI/ArtResources/Models_3D/CarMod/Car/FX12-A2Car/MatOther/FX12_A2_CarPaintGray.mat",
            "Assets/_MinoHMI/ArtResources/Models_3D/CarMod/Car/FX12-A2Car/MatOther/FX12_A2_CarPaintYellow.mat"
        };

        [MenuItem("MinoHMI/CarPaint/Setup CarRoot Presets")]
        public static void SetupCarRootPresets()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(CarRootPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"[CarRootCarPaintSetup] 未找到预制体：{CarRootPrefabPath}");
                return;
            }

            try
            {
                CarPaintSwitcher switcher = prefabRoot.GetComponent<CarPaintSwitcher>();
                if (switcher == null)
                {
                    switcher = prefabRoot.AddComponent<CarPaintSwitcher>();
                }

                CarPaintPresetSlot[] slots = new CarPaintPresetSlot[PresetMaterialPaths.Length];
                for (int index = 0; index < PresetMaterialPaths.Length; index++)
                {
                    Material sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(PresetMaterialPaths[index]);
                    slots[index] = new CarPaintPresetSlot
                    {
                        displayName = PresetDisplayNames[index],
                        sourceMaterial = sourceMaterial
                    };
                }

                SerializedObject serializedSwitcher = new SerializedObject(switcher);
                SerializedProperty presetsProperty = serializedSwitcher.FindProperty("paintPresets");
                presetsProperty.arraySize = slots.Length;
                for (int index = 0; index < slots.Length; index++)
                {
                    SerializedProperty slotProperty = presetsProperty.GetArrayElementAtIndex(index);
                    slotProperty.FindPropertyRelative("displayName").stringValue = slots[index].displayName;
                    slotProperty.FindPropertyRelative("sourceMaterial").objectReferenceValue = slots[index].sourceMaterial;
                }

                serializedSwitcher.FindProperty("defaultPresetIndex").intValue = 0;
                serializedSwitcher.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CarRootPrefabPath);
                Debug.Log("[CarRootCarPaintSetup] 已为 CarRoot 配置 6 款车漆预设（索引 0~5 与 UI 色条一致）。");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
#endif
