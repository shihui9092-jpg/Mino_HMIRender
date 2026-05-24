using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MinoHMI.UI_MenuSystem
{
    /// <summary>
    /// UI_MenuSystem 层级查找工具。
    /// </summary>
    internal static class MenuSystemHierarchyUtility
    {
        private static readonly Regex ConsecutiveWhitespacePattern = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// 规范化节点名：Trim 并折叠连续空白，便于容错匹配。
        /// </summary>
        public static string NormalizeNodeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            return ConsecutiveWhitespacePattern.Replace(name.Trim(), " ");
        }

        public static T FindNamedComponent<T>(Transform root, string nodeName, bool includeInactive)
            where T : Component
        {
            if (root == null)
            {
                return null;
            }

            string normalizedExpected = NormalizeNodeName(nodeName);
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (!IsNameMatch(candidate.name, nodeName, normalizedExpected))
                {
                    continue;
                }

                T component = candidate.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        public static Transform FindNamedTransform(Transform root, string nodeName, bool includeInactive)
        {
            if (root == null)
            {
                return null;
            }

            string normalizedExpected = NormalizeNodeName(nodeName);
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (IsNameMatch(candidate.name, nodeName, normalizedExpected))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool IsNameMatch(string candidateName, string expectedName, string normalizedExpected)
        {
            if (string.Equals(candidateName, expectedName, StringComparison.Ordinal))
            {
                return true;
            }

            return string.Equals(NormalizeNodeName(candidateName), normalizedExpected, StringComparison.Ordinal);
        }
    }
}
