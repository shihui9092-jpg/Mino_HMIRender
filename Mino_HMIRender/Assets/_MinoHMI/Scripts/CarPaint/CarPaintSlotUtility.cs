using System.Collections.Generic;
using UnityEngine;

namespace MinoHMI.CarPaint
{
    /// <summary>
    /// 从车模根节点自动收集并绑定车漆材质槽。
    /// </summary>
    internal static class CarPaintSlotUtility
    {
        public const string DefaultShaderName = "Mino/Unlit_CarPaint";

        public sealed class MaterialSlot
        {
            public MaterialSlot(Renderer renderer, int materialIndex, Material runtimeMaterial)
            {
                Renderer = renderer;
                MaterialIndex = materialIndex;
                RuntimeMaterial = runtimeMaterial;
            }

            public Renderer Renderer { get; }
            public int MaterialIndex { get; }
            public Material RuntimeMaterial { get; }
        }

        public static Shader ResolveShaderFromPresets(CarPaintPresetSlot[] paintPresets)
        {
            if (paintPresets != null)
            {
                for (int i = 0; i < paintPresets.Length; i++)
                {
                    Material sourceMaterial = paintPresets[i]?.sourceMaterial;
                    if (sourceMaterial != null && sourceMaterial.shader != null)
                        return sourceMaterial.shader;
                }
            }

            return Shader.Find(DefaultShaderName);
        }

        public static bool TryBindSlots(Transform root, List<MaterialSlot> outputSlots, Shader targetShader)
        {
            outputSlots.Clear();
            if (targetShader == null || root == null)
                return false;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            var runtimeMaterialsByRenderer = new Dictionary<Renderer, Material[]>();

            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Material[] sharedMaterials = renderer.sharedMaterials;

                for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
                {
                    Material sharedMaterial = sharedMaterials[materialIndex];
                    if (sharedMaterial == null || sharedMaterial.shader != targetShader)
                        continue;

                    if (!runtimeMaterialsByRenderer.TryGetValue(renderer, out Material[] runtimeMaterials))
                    {
                        runtimeMaterials = renderer.materials;
                        runtimeMaterialsByRenderer[renderer] = runtimeMaterials;
                    }

                    if (materialIndex < 0 || materialIndex >= runtimeMaterials.Length)
                        continue;

                    outputSlots.Add(new MaterialSlot(renderer, materialIndex, runtimeMaterials[materialIndex]));
                }
            }

            return outputSlots.Count > 0;
        }

        public static void DestroyRuntimeMaterials(IReadOnlyList<MaterialSlot> slots)
        {
            var destroyedMaterials = new HashSet<Material>();
            for (int i = 0; i < slots.Count; i++)
            {
                Material material = slots[i].RuntimeMaterial;
                if (material == null || !destroyedMaterials.Add(material))
                    continue;

                Object.Destroy(material);
            }
        }
    }
}
