using System;

using System.Collections.Generic;

using MinoHMI.CarPaint;

using UnityEngine;



namespace MinoHMI.DOTweenUGUI

{

    /// <summary>

    /// 子菜单按钮 ID 与 CarPaintSwitcher 预设索引的映射项。

    /// </summary>

    [Serializable]

    public class DotweenUguiCarPaintBinding

    {

        [Tooltip("与 DotweenUguiMenuController.menuDataList 中子按钮的 buttonId 一致")]

        public string subButtonId = "Paint_0";



        [Tooltip("对应 CarPaintSwitcher.paintPresets 数组下标")]

        public int presetIndex;

    }



    /// <summary>

    /// DOTweenUGUI 子菜单 → 车漆切换 接入桥接。

    /// 详细步骤见同目录文档：车漆菜单接入指南.md

    /// </summary>

    [AddComponentMenu("MinoHMI/DOTweenUGUI/车漆菜单桥接")]

    public class DotweenUguiCarPaintMenuBridge : MonoBehaviour

    {

        private const string PaintButtonIdPrefix = "Paint_";



        [Header("引用")]

        [SerializeField] private bool autoFindReferences = true;

        [SerializeField] private DotweenUguiMenuController menuController;

        [SerializeField] private CarPaintSwitcher carPaintSwitcher;



        [Header("过滤")]

        [SerializeField]

        [Tooltip("仅当主菜单 menuId 与此值一致时才切换车漆；留空表示不过滤主菜单")]

        private string carPaintMainMenuId = string.Empty;



        [SerializeField]

        [Tooltip("当映射表未配置时，自动解析 buttonId 为 Paint_0、Paint_1… 并当作预设下标")]

        private bool autoParsePaintIndexFromButtonId = true;



        [Header("子按钮 → 预设索引")]

        [SerializeField] private List<DotweenUguiCarPaintBinding> paintBindings = new List<DotweenUguiCarPaintBinding>();



        private readonly Dictionary<string, int> bindingLookup = new Dictionary<string, int>();



        private void Awake()

        {

            if (autoFindReferences)

            {

                TryAutoFindReferences();

            }

        }



        private void OnEnable()

        {

            if (autoFindReferences)

            {

                TryAutoFindReferences();

            }



            RebuildBindingLookup();



            if (menuController == null)

            {

                Debug.LogWarning(

                    $"[{nameof(DotweenUguiCarPaintMenuBridge)}] 未找到 DotweenUguiMenuController，子菜单点击无法切换车漆。请拖入引用或与本脚本挂在同一物体上。",

                    this);

                return;

            }



            menuController.SubMenuButtonClicked.AddListener(HandleSubMenuButtonClicked);

        }



        private void OnDisable()

        {

            if (menuController != null)

            {

                menuController.SubMenuButtonClicked.RemoveListener(HandleSubMenuButtonClicked);

            }

        }



        private void OnValidate()

        {

            RebuildBindingLookup();

        }



        /// <summary>

        /// 供 Inspector UnityEvent 绑定，或与代码订阅等效。

        /// </summary>

        public void HandleSubMenuButtonClicked(string mainMenuId, string mainMenuName, string subButtonId, string subButtonName)

        {

            if (carPaintSwitcher == null)

            {

                Debug.LogWarning(

                    $"[{nameof(DotweenUguiCarPaintMenuBridge)}] 未指定 CarPaintSwitcher，请在场景车模根节点挂载并拖入引用。",

                    this);

                return;

            }



            if (!carPaintSwitcher.isActiveAndEnabled)

            {

                Debug.LogWarning(

                    $"[{nameof(DotweenUguiCarPaintMenuBridge)}] CarPaintSwitcher 未启用（可能 Awake 时未找到车漆 Mesh），无法切换。",

                    carPaintSwitcher);

                return;

            }



            if (!string.IsNullOrEmpty(carPaintMainMenuId) &&

                !string.Equals(mainMenuId, carPaintMainMenuId, StringComparison.Ordinal))

            {

                return;

            }



            if (!TryResolvePresetIndex(subButtonId, out int presetIndex))

            {

                Debug.LogWarning(

                    $"[{nameof(DotweenUguiCarPaintMenuBridge)}] 未找到子按钮映射: {subButtonId}（主菜单: {mainMenuName}）。请配置 Paint Bindings 或使用 Paint_0 形式的 buttonId。",

                    this);

                return;

            }



            if (presetIndex < 0 || presetIndex >= carPaintSwitcher.PresetCount)

            {

                Debug.LogWarning(

                    $"[{nameof(DotweenUguiCarPaintMenuBridge)}] 预设索引 {presetIndex} 超出范围 [0, {carPaintSwitcher.PresetCount})。",

                    this);

                return;

            }



            carPaintSwitcher.SwitchToPreset(presetIndex);

        }



        private bool TryResolvePresetIndex(string subButtonId, out int presetIndex)

        {

            if (bindingLookup.TryGetValue(subButtonId, out presetIndex))

            {

                return true;

            }



            if (autoParsePaintIndexFromButtonId && TryParsePaintButtonIndex(subButtonId, out presetIndex))

            {

                return true;

            }



            presetIndex = -1;

            return false;

        }



        private static bool TryParsePaintButtonIndex(string subButtonId, out int presetIndex)

        {

            presetIndex = -1;

            if (string.IsNullOrEmpty(subButtonId) ||

                !subButtonId.StartsWith(PaintButtonIdPrefix, StringComparison.Ordinal))

            {

                return false;

            }



            string indexText = subButtonId.Substring(PaintButtonIdPrefix.Length);

            return int.TryParse(indexText, out presetIndex);

        }



        private void TryAutoFindReferences()

        {

            if (menuController == null)

            {

                menuController = GetComponent<DotweenUguiMenuController>();

            }



            if (menuController == null)

            {

                menuController = FindFirstObjectByType<DotweenUguiMenuController>();

            }



            if (carPaintSwitcher == null)

            {

                carPaintSwitcher = FindFirstObjectByType<CarPaintSwitcher>();

            }

        }



        /// <summary>

        /// 根据 CarPaintSwitcher 上的预设数量生成 Paint_0… 绑定列表。

        /// </summary>

        [ContextMenu("从车漆预设生成绑定列表")]

        private void GenerateBindingsFromPaintPresets()

        {

            if (carPaintSwitcher == null)

            {

                return;

            }



            paintBindings.Clear();

            int count = carPaintSwitcher.PresetCount;

            for (int i = 0; i < count; i++)

            {

                paintBindings.Add(new DotweenUguiCarPaintBinding

                {

                    subButtonId = $"{PaintButtonIdPrefix}{i}",

                    presetIndex = i

                });

            }



            RebuildBindingLookup();

        }



        private void RebuildBindingLookup()

        {

            bindingLookup.Clear();

            for (int i = 0; i < paintBindings.Count; i++)

            {

                DotweenUguiCarPaintBinding binding = paintBindings[i];

                if (string.IsNullOrEmpty(binding.subButtonId))

                {

                    continue;

                }



                bindingLookup[binding.subButtonId] = binding.presetIndex;

            }

        }

    }

}


