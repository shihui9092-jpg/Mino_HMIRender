using UnityEngine;

namespace MinoHMI.UI_MenuSystem
{
    /// <summary>
    /// UI_MenuSystem 程序化 Sprite 工具：Inspector 未指定图片时使用纯白占位图。
    /// </summary>
    internal static class MenuSystemSpriteUtility
    {
        private static Sprite cachedWhiteSprite;

        /// <summary>
        /// 获取 1x1 纯白色 Sprite（运行时缓存，不写入磁盘）。
        /// </summary>
        public static Sprite GetWhiteSprite()
        {
            if (cachedWhiteSprite != null)
            {
                return cachedWhiteSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);

            cachedWhiteSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                100f);
            cachedWhiteSprite.name = "MenuSystem_WhiteSprite";
            cachedWhiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return cachedWhiteSprite;
        }

        /// <summary>
        /// 未指定 Sprite 时返回纯白占位图。
        /// </summary>
        public static Sprite ResolveOrWhite(Sprite sprite)
        {
            return sprite != null ? sprite : GetWhiteSprite();
        }
    }
}
