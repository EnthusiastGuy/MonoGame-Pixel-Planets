using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace ShadersTest.Export
{
    public static class Exporter
    {
        private static RenderTarget2D exportBuffer;
        private static int index = 0;

        private static void InitializeExportBuffer(int width, int height)
        {
            exportBuffer = new RenderTarget2D(
                Renderer.GetGraphicsDevice(),
                width,
                height
            );
                
        }

        public static void Export(int w, int h)
        {
            //InitializeExportBuffer(w, h);
            //Renderer.GetGraphicsDevice().SetRenderTarget(exportBuffer);

            Renderer.DrawExportTexture();

            // Drop the render target
            //Renderer.GetGraphicsDevice().SetRenderTarget(null);

            //saveIt(w, h);
        }

        public static void SaveScreenshot()
        {
            //Pull the picture from the buffer 
            int[] backBuffer = new int[Config.VIEWPORT_WIDTH * Config.VIEWPORT_HEIGHT];
            Renderer.GetGraphicsDevice().GetBackBufferData(backBuffer);

            //Copy to texture
            Texture2D texture = new Texture2D(
                Renderer.GetGraphicsDevice(),
                Config.VIEWPORT_WIDTH,
                Config.VIEWPORT_HEIGHT,
                false,
                Renderer.GetGraphicsDevice().PresentationParameters.BackBufferFormat
            );

            texture.SetData(backBuffer);

            //Get a date for file name
            Stream stream = File.Create(string.Format("screenshot_{0}.png", index++));

            //Save as PNG
            texture.SaveAsPng(stream, Config.VIEWPORT_WIDTH, Config.VIEWPORT_HEIGHT);
            stream.Dispose();
            texture.Dispose();
        }

        // Update the export buffers
        public static void Update()
        {

        }

        // Draw the export buffers
        public static void Draw()
        {

        }

    }
}
