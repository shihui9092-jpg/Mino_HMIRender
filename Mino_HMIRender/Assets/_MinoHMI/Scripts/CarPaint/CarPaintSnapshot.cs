using System.Collections.Generic;
using UnityEngine;

namespace MinoHMI.CarPaint
{
    /// <summary>
    /// 基于 Material 实际属性的动态快照，用于车漆参数插值。
    /// </summary>
    public sealed class CarPaintSnapshot
    {
        private readonly Dictionary<int, float> floatValues = new Dictionary<int, float>();
        private readonly Dictionary<int, Color> colorValues = new Dictionary<int, Color>();
        private readonly Dictionary<int, Vector4> vectorValues = new Dictionary<int, Vector4>();
        private readonly Dictionary<int, int> intValues = new Dictionary<int, int>();

        public string[] ShaderKeywords { get; private set; }

        public static CarPaintSnapshot FromMaterial(Material material)
        {
            var snapshot = new CarPaintSnapshot();
            if (material == null)
                return snapshot;

            CarPaintPropertySchema.Layout layout = CarPaintPropertySchema.Get(material.shader);

            for (int i = 0; i < layout.FloatIds.Length; i++)
            {
                int propertyId = layout.FloatIds[i];
                if (material.HasProperty(propertyId))
                    snapshot.floatValues[propertyId] = material.GetFloat(propertyId);
            }

            for (int i = 0; i < layout.ColorIds.Length; i++)
            {
                int propertyId = layout.ColorIds[i];
                if (material.HasProperty(propertyId))
                    snapshot.colorValues[propertyId] = material.GetColor(propertyId);
            }

            for (int i = 0; i < layout.VectorIds.Length; i++)
            {
                int propertyId = layout.VectorIds[i];
                if (material.HasProperty(propertyId))
                    snapshot.vectorValues[propertyId] = material.GetVector(propertyId);
            }

            for (int i = 0; i < layout.IntIds.Length; i++)
            {
                int propertyId = layout.IntIds[i];
                if (material.HasProperty(propertyId))
                    snapshot.intValues[propertyId] = material.GetInt(propertyId);
            }

            snapshot.ShaderKeywords = material.shaderKeywords;
            return snapshot;
        }

        public static CarPaintSnapshot Lerp(CarPaintSnapshot from, CarPaintSnapshot to, float t)
        {
            t = Mathf.Clamp01(t);
            var blended = new CarPaintSnapshot();

            LerpFloats(from, to, blended, t);
            LerpColors(from, to, blended, t);
            LerpVectors(from, to, blended, t);
            LerpInts(from, to, blended, t);

            blended.ShaderKeywords = t >= 1f ? to.ShaderKeywords : from.ShaderKeywords;
            return blended;
        }

        public void ApplyTo(Material material, bool applyKeywords)
        {
            if (material == null)
                return;

            foreach (KeyValuePair<int, float> pair in floatValues)
            {
                if (material.HasProperty(pair.Key))
                    material.SetFloat(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<int, Color> pair in colorValues)
            {
                if (material.HasProperty(pair.Key))
                    material.SetColor(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<int, Vector4> pair in vectorValues)
            {
                if (material.HasProperty(pair.Key))
                    material.SetVector(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<int, int> pair in intValues)
            {
                if (material.HasProperty(pair.Key))
                    material.SetInt(pair.Key, pair.Value);
            }

            if (applyKeywords && ShaderKeywords != null)
                material.shaderKeywords = ShaderKeywords;
        }

        public static void CopyTextures(Material source, Material destination)
        {
            if (source == null || destination == null || source.shader != destination.shader)
            {
                CopyTexturesCrossShader(source, destination);
                return;
            }

            CarPaintPropertySchema.Layout layout = CarPaintPropertySchema.Get(source.shader);
            for (int i = 0; i < layout.TextureIds.Length; i++)
            {
                int propertyId = layout.TextureIds[i];
                if (!source.HasProperty(propertyId) || !destination.HasProperty(propertyId))
                    continue;

                destination.SetTexture(propertyId, source.GetTexture(propertyId));
                destination.SetTextureOffset(propertyId, source.GetTextureOffset(propertyId));
                destination.SetTextureScale(propertyId, source.GetTextureScale(propertyId));
            }
        }

        private static void CopyTexturesCrossShader(Material source, Material destination)
        {
            if (source == null || destination == null)
                return;

            CarPaintPropertySchema.Layout sourceLayout = CarPaintPropertySchema.Get(source.shader);
            for (int i = 0; i < sourceLayout.TextureIds.Length; i++)
            {
                int propertyId = sourceLayout.TextureIds[i];
                if (!source.HasProperty(propertyId) || !destination.HasProperty(propertyId))
                    continue;

                destination.SetTexture(propertyId, source.GetTexture(propertyId));
                destination.SetTextureOffset(propertyId, source.GetTextureOffset(propertyId));
                destination.SetTextureScale(propertyId, source.GetTextureScale(propertyId));
            }
        }

        private static void LerpFloats(CarPaintSnapshot from, CarPaintSnapshot to, CarPaintSnapshot blended, float t)
        {
            var visited = new HashSet<int>();
            foreach (KeyValuePair<int, float> pair in from.floatValues)
            {
                visited.Add(pair.Key);
                float endValue = to.floatValues.TryGetValue(pair.Key, out float value) ? value : pair.Value;
                blended.floatValues[pair.Key] = Mathf.Lerp(pair.Value, endValue, t);
            }

            foreach (KeyValuePair<int, float> pair in to.floatValues)
            {
                if (visited.Contains(pair.Key))
                    continue;

                float startValue = from.floatValues.TryGetValue(pair.Key, out float fromValue) ? fromValue : pair.Value;
                blended.floatValues[pair.Key] = Mathf.Lerp(startValue, pair.Value, t);
            }
        }

        private static void LerpColors(CarPaintSnapshot from, CarPaintSnapshot to, CarPaintSnapshot blended, float t)
        {
            var visited = new HashSet<int>();
            foreach (KeyValuePair<int, Color> pair in from.colorValues)
            {
                visited.Add(pair.Key);
                Color endValue = to.colorValues.TryGetValue(pair.Key, out Color value) ? value : pair.Value;
                blended.colorValues[pair.Key] = Color.Lerp(pair.Value, endValue, t);
            }

            foreach (KeyValuePair<int, Color> pair in to.colorValues)
            {
                if (visited.Contains(pair.Key))
                    continue;

                Color startValue = from.colorValues.TryGetValue(pair.Key, out Color fromValue) ? fromValue : pair.Value;
                blended.colorValues[pair.Key] = Color.Lerp(startValue, pair.Value, t);
            }
        }

        private static void LerpVectors(CarPaintSnapshot from, CarPaintSnapshot to, CarPaintSnapshot blended, float t)
        {
            var visited = new HashSet<int>();
            foreach (KeyValuePair<int, Vector4> pair in from.vectorValues)
            {
                visited.Add(pair.Key);
                Vector4 endValue = to.vectorValues.TryGetValue(pair.Key, out Vector4 value) ? value : pair.Value;
                blended.vectorValues[pair.Key] = Vector4.Lerp(pair.Value, endValue, t);
            }

            foreach (KeyValuePair<int, Vector4> pair in to.vectorValues)
            {
                if (visited.Contains(pair.Key))
                    continue;

                Vector4 startValue = from.vectorValues.TryGetValue(pair.Key, out Vector4 fromValue) ? fromValue : pair.Value;
                blended.vectorValues[pair.Key] = Vector4.Lerp(startValue, pair.Value, t);
            }
        }

        private static void LerpInts(CarPaintSnapshot from, CarPaintSnapshot to, CarPaintSnapshot blended, float t)
        {
            var visited = new HashSet<int>();
            foreach (KeyValuePair<int, int> pair in from.intValues)
            {
                visited.Add(pair.Key);
                int endValue = to.intValues.TryGetValue(pair.Key, out int value) ? value : pair.Value;
                blended.intValues[pair.Key] = Mathf.RoundToInt(Mathf.Lerp(pair.Value, endValue, t));
            }

            foreach (KeyValuePair<int, int> pair in to.intValues)
            {
                if (visited.Contains(pair.Key))
                    continue;

                int startValue = from.intValues.TryGetValue(pair.Key, out int fromValue) ? fromValue : pair.Value;
                blended.intValues[pair.Key] = Mathf.RoundToInt(Mathf.Lerp(startValue, pair.Value, t));
            }
        }
    }
}
