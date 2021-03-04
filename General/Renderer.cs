namespace ShadersTest
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    static class Renderer
    {
        private static GraphicsDeviceManager graphicsDeviceManager;
        private static SpriteBatch spriteBatch;
        private static GraphicsDevice graphicsDevice;
        private static Texture2D dummyTexture;

        private static Texture2D point;

        public static void Initialize()
        {
            graphicsDevice = graphicsDeviceManager.GraphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
            dummyTexture = new Texture2D(graphicsDevice, 1, 1);
            point = new Texture2D(graphicsDevice, 1, 1);
            point.SetData(new Color[] { Color.White });
        }

        public static void RegisterGraphicsDeviceManager(GraphicsDeviceManager gdm)
        {
            graphicsDeviceManager = gdm;
        }

        public static void RegisterGraphics()
        {
            graphicsDeviceManager.PreferredBackBufferWidth = Config.VIEWPORT_WIDTH;
            graphicsDeviceManager.PreferredBackBufferHeight = Config.VIEWPORT_HEIGHT;

            graphicsDeviceManager.IsFullScreen = false;
            graphicsDeviceManager.ApplyChanges();
        }

        public static void Clear()
        {
            graphicsDevice.Clear(Color.Transparent);
        }

        public static void DrawShader()
        {
            // Shader batch
            spriteBatch.Begin(
                SpriteSortMode.BackToFront,
                BlendState.Additive,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone
                , Shaders.CelestialEffect
                );

            spriteBatch.Draw(dummyTexture,
                new Rectangle(
                    Config.PLANET_PADDING,
                    Config.PLANET_PADDING,
                    Config.PLANET_RECT_SIZE,
                    Config.PLANET_RECT_SIZE
                ),
                Color.White
            );

            spriteBatch.End();
        }

        public static void DrawTitle(string text, int x, int y)
        {
            DrawString(Fonts.VerdanaBold, text, new Vector2(x, y), Color.LimeGreen);
        }

        public static void DrawInfo(string text, int x, int y)
        {
            DrawString(Fonts.Verdana, text, new Vector2(x, y), Color.Gray);
        }

        public static void DrawBoldOrange(string text, int x, int y)
        {
            DrawString(Fonts.VerdanaBold, text, new Vector2(x, y), Color.Orange);
        }

        public static void DrawSimpleOrange(string text, int x, int y)
        {
            DrawString(Fonts.Verdana, text, new Vector2(x, y), Color.Orange);
        }

        public static void DrawSimple(string text, int x, int y, Color color)
        {
            DrawString(Fonts.Verdana, text, new Vector2(x, y), color);
        }

        public static void StartTextBatch()
        {
            spriteBatch.Begin();
        }

        public static void EndBatch()
        {
            spriteBatch.End();
        }

        private static void DrawString(SpriteFont font, string text, Vector2 xy, Color color)
        {
            spriteBatch.DrawString(font, text, xy, color);
        }


        public static void DrawEmptyColorRectangle(Rectangle destRect, Color color)
        {
            DrawEmptyColorRectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height, color);
        }

        public static void DrawEmptyColorRectangle(int x, int y, int w, int h, Color color)
        {
            spriteBatch.Draw(point, new Rectangle(x, y, w - 1, 1), Config.ONE_PIXEL_RECT, color);
            spriteBatch.Draw(point, new Rectangle(x + w - 1, y, 1, h), Config.ONE_PIXEL_RECT, color);
            spriteBatch.Draw(point, new Rectangle(x, y + h, w, 1), Config.ONE_PIXEL_RECT, color);
            spriteBatch.Draw(point, new Rectangle(x, y, 1, h), Config.ONE_PIXEL_RECT, color);
        }

        public static void DrawLShapedSelection(Rectangle destRect, Color color)
        {
            DrawLShapedSelection(destRect.X, destRect.Y, destRect.Width, destRect.Height, color);
        }

        public static void DrawLShapedSelection(int x, int y, int w, int h, Color color)
        {
            spriteBatch.Draw(point, new Rectangle(x, y + h, w, 1), Config.ONE_PIXEL_RECT, color);
            spriteBatch.Draw(point, new Rectangle(x, y, 1, h), Config.ONE_PIXEL_RECT, color);
        }
    }
}
