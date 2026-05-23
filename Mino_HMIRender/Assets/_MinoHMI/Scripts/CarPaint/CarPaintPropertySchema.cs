using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MinoHMI.CarPaint
{
    /// <summary>
    /// 按 Shader 缓存可插值属性与贴图属性列表，Shader 变更后自动跟随新属性表。
    /// </summary>
    internal static class CarPaintPropertySchema
    {
        internal sealed class Layout
        {
            public int[] FloatIds = System.Array.Empty<int>();
            public int[] ColorIds = System.Array.Empty<int>();
            public int[] VectorIds = System.Array.Empty<int>();
            public int[] IntIds = System.Array.Empty<int>();
            public int[] TextureIds = System.Array.Empty<int>();
        }

        private static readonly Dictionary<Shader, Layout> LayoutCache = new Dictionary<Shader, Layout>();

        public static Layout Get(Shader shader)
        {
            if (shader == null)
                return new Layout();

            if (LayoutCache.TryGetValue(shader, out Layout cachedLayout))
                return cachedLayout;

            Layout layout = BuildLayout(shader);
            LayoutCache[shader] = layout;
            return layout;
        }

        private static Layout BuildLayout(Shader shader)
        {
            var floatIds = new List<int>();
            var colorIds = new List<int>();
            var vectorIds = new List<int>();
            var intIds = new List<int>();
            var textureIds = new List<int>();

            int propertyCount = shader.GetPropertyCount();
            for (int propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                if (!ShouldIncludeProperty(shader, propertyIndex, out bool isTextureProperty))
                    continue;

                int propertyId = Shader.PropertyToID(shader.GetPropertyName(propertyIndex));
                if (isTextureProperty)
                {
                    textureIds.Add(propertyId);
                    continue;
                }

                switch (shader.GetPropertyType(propertyIndex))
                {
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        floatIds.Add(propertyId);
                        break;
                    case ShaderPropertyType.Color:
                        colorIds.Add(propertyId);
                        break;
                    case ShaderPropertyType.Vector:
                        vectorIds.Add(propertyId);
                        break;
                    case ShaderPropertyType.Int:
                        intIds.Add(propertyId);
                        break;
                }
            }

            return new Layout
            {
                FloatIds = floatIds.ToArray(),
                ColorIds = colorIds.ToArray(),
                VectorIds = vectorIds.ToArray(),
                IntIds = intIds.ToArray(),
                TextureIds = textureIds.ToArray()
            };
        }

        private static bool ShouldIncludeProperty(Shader shader, int propertyIndex, out bool isTextureProperty)
        {
            isTextureProperty = false;
            ShaderPropertyFlags flags = shader.GetPropertyFlags(propertyIndex);

            if ((flags & ShaderPropertyFlags.HideInInspector) != 0)
                return false;

            if ((flags & ShaderPropertyFlags.PerRendererData) != 0)
                return false;

            ShaderPropertyType propertyType = shader.GetPropertyType(propertyIndex);
            if (IsTexturePropertyType(propertyType))
            {
                isTextureProperty = true;
                return true;
            }

            switch (propertyType)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                case ShaderPropertyType.Color:
                case ShaderPropertyType.Vector:
                case ShaderPropertyType.Int:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsTexturePropertyType(ShaderPropertyType propertyType)
        {
            // 部分 Unity 版本中 Cubemap 与 Texture 共用同一枚举值
            return propertyType == ShaderPropertyType.Texture;
        }
    }
}
