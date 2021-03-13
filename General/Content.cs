using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShadersTest
{
    static class GameContent
    {
        public static ContentManager Content;

        public static void RegisterContent(ContentManager content)
        {
            Content = content;
            Content.RootDirectory = "Content";
        }

        public static Texture2D LoadTexture2D(string resourcePath)
        {
            return Content.Load<Texture2D>(resourcePath);
        }

        public static SpriteFont LoadSpriteFont(string resourcePath)
        {
            return Content.Load<SpriteFont>(resourcePath);
        }

        public static Effect LoadEffect(string resourcePath)
        {
            return Content.Load<Effect>("ShadersV2/" + resourcePath);
        }
    }
}
