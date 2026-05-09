namespace ShadersTest
{
    using System;
    using System.IO;
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

        public static GraphicsDevice GetGraphicsDevice()
        {
            return graphicsDevice;
        }

        public static void Clear()
        {
            graphicsDevice.Clear(Color.Black);
        }

        /// <summary>
        /// Renders shader passes. The original port uses <see cref="BlendState.Additive"/> — planet shaders are tuned for it.
        /// Using <see cref="BlendState.AlphaBlend"/> on the backbuffer tends to produce a flat white/wrong tint behind the sphere.
        /// </summary>
        public static void DrawPlanetPasses(Rectangle planetRect)
        {
            DrawPlanetPasses(planetRect, BlendState.Additive);
        }

        /// <summary>Same as <see cref="DrawPlanetPasses(Rectangle)"/> but chooses blend mode (e.g. alpha for RT export).</summary>
        public static void DrawPlanetPasses(Rectangle planetRect, BlendState blendState)
        {
            foreach (Effect fx in Shaders.CelestialEffects)
            {
                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    blendState,
                    SamplerState.PointClamp,
                    DepthStencilState.Default,
                    RasterizerState.CullNone,
                    fx
                );

                spriteBatch.Draw(dummyTexture, planetRect, Color.White);

                spriteBatch.End();
            }
        }

        public static void DrawShader()
        {
            DrawPlanetPasses(new Rectangle(
                Config.PLANET_PADDING,
                Config.PLANET_PADDING,
                Config.PLANET_RECT_SIZE,
                Config.PLANET_RECT_SIZE
            ));
        }

        /// <summary>Saves only the procedural planet (no UI chrome). Outside the sphere is transparent.</summary>
        public static void CapturePlanetToPng(string path)
        {
            using (RenderTarget2D rt = new RenderTarget2D(
                graphicsDevice,
                Config.PLANET_RECT_SIZE,
                Config.PLANET_RECT_SIZE,
                false,
                SurfaceFormat.Color,
                DepthFormat.None))
            {
                graphicsDevice.SetRenderTarget(rt);
                graphicsDevice.Clear(Color.Transparent);
                DrawPlanetPasses(new Rectangle(0, 0, Config.PLANET_RECT_SIZE, Config.PLANET_RECT_SIZE), BlendState.AlphaBlend);
                graphicsDevice.SetRenderTarget(null);

                using (Stream fs = File.Create(path))
                    rt.SaveAsPng(fs, rt.Width, rt.Height);
            }
        }

        /// <summary>Renders the current celestial into a buffer (row-major). Optionally clears with transparency for correct PNG/GIF alpha.</summary>
        public static void CapturePlanetPixels(Color[] buffer, bool transparentBackground)
        {
            int w = Config.PLANET_RECT_SIZE;
            int h = Config.PLANET_RECT_SIZE;
            if (buffer.Length < w * h)
                throw new ArgumentException("Buffer too small for planet capture.");

            using (RenderTarget2D rt = new RenderTarget2D(
                graphicsDevice,
                w,
                h,
                false,
                SurfaceFormat.Color,
                DepthFormat.None))
            {
                graphicsDevice.SetRenderTarget(rt);
                graphicsDevice.Clear(transparentBackground ? Color.Transparent : Color.Black);
                DrawPlanetPasses(
                    new Rectangle(0, 0, w, h),
                    transparentBackground ? BlendState.AlphaBlend : BlendState.Additive);
                graphicsDevice.SetRenderTarget(null);

                rt.GetData(buffer);
            }

            if (transparentBackground)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    Color c = buffer[i];
                    if (c.A == 0)
                        buffer[i] = Color.Transparent;
                }
            }
        }

        public static void DrawExportTexture()
        {
            DrawShader();
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
