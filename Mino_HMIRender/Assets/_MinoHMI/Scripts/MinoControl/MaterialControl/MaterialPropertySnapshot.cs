using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MinoHMI.MY26HMI.MaterialControl
{
    /// <summary>
    /// 材质属性快照，用于本体与变体之间的平滑插值。
    /// </summary>
    public sealed class MaterialPropertySnapshot
    {
        private readonly Dictionary<int, float> floatValues = new Dictionary<int, float>();
        private readonly Dictionary<int, Color> colorValues = new Dictionary<int, Color>();
        private readonly Dictionary<int, Vector4> vectorValues = new Dictionary<int, Vector4>();
        private readonly Dictionary<int, int> intValues = new Dictionary<int, int>();
        private readonly Dictionary<int, Texture> textureValues = new Dictionary<int, Texture>();

        public string[] ShaderKeywords { get; private set; } = System.Array.Empty<string>();

        public static MaterialPropertySnapshot FromMaterial(Material material)
        {
            MaterialPropertySnapshot snapshot = new MaterialPropertySnapshot();
            if (material == null || material.shader == null)
            {
                return snapshot;
            }

            Shader shader = material.shader;
            int propertyCount = shader.GetPropertyCount();
            for (int propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                int propertyId = shader.GetPropertyNameId(propertyIndex);
                if (!material.HasProperty(propertyId))
                {
                    continue;
                }

                switch (shader.GetPropertyType(propertyIndex))
                {
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        snapshot.floatValues[propertyId] = material.GetFloat(propertyId);
                        break;
                    case ShaderPropertyType.Color:
                        snapshot.colorValues[propertyId] = material.GetColor(propertyId);
                        break;
                    case ShaderPropertyType.Vector:
                        snapshot.vectorValues[propertyId] = material.GetVector(propertyId);
                        break;
                    case ShaderPropertyType.Int:
                        snapshot.intValues[propertyId] = material.GetInt(propertyId);
                        break;
                    case ShaderPropertyType.Texture:
                        snapshot.textureValues[propertyId] = material.GetTexture(propertyId);
                        break;
                }
            }

            snapshot.ShaderKeywords = material.shaderKeywords;
            return snapshot;
        }

        /// <summary>
        /// 将插值结果写入当前快照（复用字典，避免每帧分配）。
        /// </summary>
        public void LerpInto(
            MaterialPropertySnapshot from,
            MaterialPropertySnapshot to,
            float normalizedTime,
            MaterialDiscretePropertySwitchTiming discretePropertySwitchTiming)
        {
            float blend = Mathf.Clamp01(normalizedTime);
            bool useTargetDiscreteProperties = ShouldUseTargetDiscreteProperties(blend, discretePropertySwitchTiming);

            floatValues.Clear();
            colorValues.Clear();
            vectorValues.Clear();

            LerpFloatDictionary(from.floatValues, to.floatValues, floatValues, blend);
            LerpColorDictionary(from.colorValues, to.colorValues, colorValues, blend);
            LerpVectorDictionary(from.vectorValues, to.vectorValues, vectorValues, blend);

            if (useTargetDiscreteProperties)
            {
                CopyIntDictionary(to.intValues, intValues);
                CopyTextureDictionary(to.textureValues, textureValues);
                ShaderKeywords = to.ShaderKeywords;
            }
            else
            {
                CopyIntDictionary(from.intValues, intValues);
                CopyTextureDictionary(from.textureValues, textureValues);
                ShaderKeywords = from.ShaderKeywords;
            }
        }

        /// <summary>
        /// 计算离散属性切换时机。
        /// </summary>
        private static bool ShouldUseTargetDiscreteProperties(
            float blend,
            MaterialDiscretePropertySwitchTiming discretePropertySwitchTiming)
        {
            switch (discretePropertySwitchTiming)
            {
                case MaterialDiscretePropertySwitchTiming.AtStart:
                    return blend > 0f;
                case MaterialDiscretePropertySwitchTiming.AtEnd:
                default:
                    return blend >= 1f;
            }
        }

        public void ApplyTo(Material material)
        {
            if (material == null)
            {
                return;
            }

            foreach (KeyValuePair<int, float> pair in floatValues)
            {
                material.SetFloat(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<int, Color> pair in colorValues)
            {
                material.SetColor(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<int, Vector4> pair in vectorValues)
            {
                material.SetVector(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<int, int> pair in intValues)
            {
                material.SetInt(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<int, Texture> pair in textureValues)
            {
                material.SetTexture(pair.Key, pair.Value);
            }

            // 始终回写关键词，确保目标为空关键词时可清空旧状态
            material.shaderKeywords = ShaderKeywords ?? System.Array.Empty<string>();
        }

        private static void LerpFloatDictionary(
            Dictionary<int, float> from,
            Dictionary<int, float> to,
            Dictionary<int, float> output,
            float blend)
        {
            foreach (KeyValuePair<int, float> pair in from)
            {
                output[pair.Key] = pair.Value;
            }

            foreach (KeyValuePair<int, float> pair in to)
            {
                if (from.TryGetValue(pair.Key, out float fromValue))
                {
                    output[pair.Key] = Mathf.Lerp(fromValue, pair.Value, blend);
                }
                else
                {
                    output[pair.Key] = pair.Value;
                }
            }
        }

        private static void LerpColorDictionary(
            Dictionary<int, Color> from,
            Dictionary<int, Color> to,
            Dictionary<int, Color> output,
            float blend)
        {
            foreach (KeyValuePair<int, Color> pair in from)
            {
                output[pair.Key] = pair.Value;
            }

            foreach (KeyValuePair<int, Color> pair in to)
            {
                if (from.TryGetValue(pair.Key, out Color fromValue))
                {
                    output[pair.Key] = Color.Lerp(fromValue, pair.Value, blend);
                }
                else
                {
                    output[pair.Key] = pair.Value;
                }
            }
        }

        private static void LerpVectorDictionary(
            Dictionary<int, Vector4> from,
            Dictionary<int, Vector4> to,
            Dictionary<int, Vector4> output,
            float blend)
        {
            foreach (KeyValuePair<int, Vector4> pair in from)
            {
                output[pair.Key] = pair.Value;
            }

            foreach (KeyValuePair<int, Vector4> pair in to)
            {
                if (from.TryGetValue(pair.Key, out Vector4 fromValue))
                {
                    output[pair.Key] = Vector4.Lerp(fromValue, pair.Value, blend);
                }
                else
                {
                    output[pair.Key] = pair.Value;
                }
            }
        }

        private static void CopyIntDictionary(Dictionary<int, int> source, Dictionary<int, int> destination)
        {
            destination.Clear();
            foreach (KeyValuePair<int, int> pair in source)
            {
                destination[pair.Key] = pair.Value;
            }
        }

        private static void CopyTextureDictionary(Dictionary<int, Texture> source, Dictionary<int, Texture> destination)
        {
            destination.Clear();
            foreach (KeyValuePair<int, Texture> pair in source)
            {
                destination[pair.Key] = pair.Value;
            }
        }
    }
}
