#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MinoHMI.DOTweenUGUI.Editor
{
    /// <summary>
    /// 编辑器菜单：重新生成 DOTweenUGUI 按钮预制体（与运行时脚本要求一致）。
    /// </summary>
    public static class DotweenUguiPrefabGenerator
    {
        private const string PrefabFolder = "Assets/_MinoHMI/Prefabs/DOTweenUGUI";

        [MenuItem("MinoHMI/DOTweenUGUI/重新生成按钮预制体")]
        public static void RegenerateButtonPrefabs()
        {
            EnsureFolder();

            DotweenUguiMenuItemView mainTemplate = CreateButtonPrefab(
                $"{PrefabFolder}/MainMenuButton_Template.prefab",
                "MainMenuButton_Template",
                new Vector2(200f, 56f),
                new Color(0.22f, 0.26f, 0.35f, 1f),
                24,
                "主菜单");

            DotweenUguiMenuItemView subTemplate = CreateButtonPrefab(
                $"{PrefabFolder}/SubMenuButton_Template.prefab",
                "SubMenuButton_Template",
                new Vector2(160f, 48f),
                new Color(0.18f, 0.22f, 0.3f, 1f),
                22,
                "子按钮");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[{nameof(DotweenUguiPrefabGenerator)}] 已生成：\n- {mainTemplate.name}\n- {subTemplate.name}");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_MinoHMI/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_MinoHMI", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/_MinoHMI/Prefabs", "DOTweenUGUI");
            }
        }

        private static DotweenUguiMenuItemView CreateButtonPrefab(
            string assetPath,
            string objectName,
            Vector2 size,
            Color normalColor,
            int fontSize,
            string placeholderText)
        {
            var rootObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(DotweenUguiMenuItemView));
            var rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.sizeDelta = size;

            var image = rootObject.GetComponent<Image>();
            image.color = normalColor;
            image.raycastTarget = true;

            var button = rootObject.GetComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor * 1.15f;
            colors.pressedColor = normalColor * 0.85f;
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.18f, 0.2f, 0.24f, 0.6f);
            button.colors = colors;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(rootObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 6f);
            labelRect.offsetMax = new Vector2(-10f, -6f);

            var label = labelObject.GetComponent<Text>();
            label.text = placeholderText;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            var itemView = rootObject.GetComponent<DotweenUguiMenuItemView>();
            itemView.Setup(placeholderText, null);

            rootObject.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, assetPath);
            Object.DestroyImmediate(rootObject);
            return prefab.GetComponent<DotweenUguiMenuItemView>();
        }
    }
}
#endif
