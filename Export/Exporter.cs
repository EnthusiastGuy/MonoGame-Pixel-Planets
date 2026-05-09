using Microsoft.Xna.Framework.Graphics;
using ShadersTest;
using XnaColor = Microsoft.Xna.Framework.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ShadersTest.Export
{
    public static class Exporter
    {
        private static int screenshotIndex;

        public static void Export(int w, int h)
        {
            Renderer.DrawExportTexture();
        }

        public static void SaveScreenshot()
        {
            int[] backBuffer = new int[Config.VIEWPORT_WIDTH * Config.VIEWPORT_HEIGHT];
            Renderer.GetGraphicsDevice().GetBackBufferData(backBuffer);

            using (Texture2D texture = new Texture2D(
                Renderer.GetGraphicsDevice(),
                Config.VIEWPORT_WIDTH,
                Config.VIEWPORT_HEIGHT,
                false,
                Renderer.GetGraphicsDevice().PresentationParameters.BackBufferFormat))
            {
                texture.SetData(backBuffer);

                string path = Path.Combine(
                    Environment.CurrentDirectory,
                    string.Format("screenshot_{0}.png", screenshotIndex++));

                using (Stream stream = File.Create(path))
                    texture.SaveAsPng(stream, Config.VIEWPORT_WIDTH, Config.VIEWPORT_HEIGHT);
            }
        }

        public static void SavePlanetPngWithDialog()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG image|*.png";
                dlg.FileName = SanitizeFileName(State.GetCurrentCelestialName()) + ".png";
                dlg.Title = "Export planet PNG";

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                Renderer.CapturePlanetToPng(dlg.FileName);
            }
        }

        /// <summary>Animated GIF (indexed color). Frames sample shader time from 0..1 across <paramref name="frameCount"/> samples.</summary>
        public static void SaveGifWithDialog(int frameCount = 32, int frameDelayHundredths = 5)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "GIF animation|*.gif";
                dlg.FileName = SanitizeFileName(State.GetCurrentCelestialName()) + ".gif";
                dlg.Title = "Export animated GIF";

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                RunAnimatedExport(() => ExportGifToPath(dlg.FileName, frameCount, frameDelayHundredths));
            }
        }

        /// <summary>Sprite sheet: <paramref name="columns"/> × <paramref name="rows"/> frames left-to-right, top-to-bottom.</summary>
        public static void SaveSpriteSheetWithDialog(int columns = 8, int rows = 8, int marginPx = 0)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG image|*.png";
                dlg.FileName = SanitizeFileName(State.GetCurrentCelestialName()) + "_spritesheet.png";
                dlg.Title = "Export sprite sheet";

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                RunAnimatedExport(() => ExportSpriteSheetToPath(dlg.FileName, columns, rows, marginPx));
            }
        }

        private static void RunAnimatedExport(Action exportAction)
        {
            bool wasPaused = State.RotationPaused;
            State.RotationPaused = true;
            State.SuspendTimeForExport = true;
            State.ExportAnimationActive = true;

            try
            {
                exportAction();
            }
            finally
            {
                State.ExportAnimationActive = false;
                State.SuspendTimeForExport = false;
                State.RotationPaused = wasPaused;
            }
        }

        private static void ExportGifToPath(string path, int frameCount, int delayHundredths)
        {
            float savedExportScale = State.ExportAnimationTimeScale;
            int frames = frameCount;
            State.ExportGifGodotTimeMapping = true;

            int w = Config.PLANET_RECT_SIZE;
            int h = Config.PLANET_RECT_SIZE;
            var buffer = new XnaColor[w * h];

            Image<Rgba32> gif = null;

            try
            {
                for (int i = 0; i < frames; i++)
                {
                    // Match Godot GUI.gd export_gif: lerp(0, 1, i/float(frames)) — samples [0, 1), even spacing including wrap.
                    State.ExportAnimationPhase01 = frames <= 1 ? 0f : i / (float)frames;

                    Shaders.Update();

                    Renderer.CapturePlanetPixels(buffer, transparentBackground: true);

                    Rgba32[] pixels = ColorsToRgba(buffer);

                    using (var frameImage = Image.LoadPixelData<Rgba32>(pixels, w, h))
                    {
                        if (gif == null)
                        {
                            gif = frameImage.Clone();
                            gif.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = Math.Max(1, delayHundredths);
                        }
                        else
                        {
                            gif.Frames.AddFrame(frameImage.Frames.RootFrame);
                            gif.Frames[gif.Frames.Count - 1].Metadata.GetGifMetadata().FrameDelay =
                                Math.Max(1, delayHundredths);
                        }
                    }
                }

                if (gif != null)
                {
                    gif.Metadata.GetGifMetadata().RepeatCount = 0;

                    using (var fs = File.Create(path))
                        gif.SaveAsGif(fs, new GifEncoder());
                }
            }
            finally
            {
                gif?.Dispose();
                State.ExportGifGodotTimeMapping = false;
                State.ExportAnimationTimeScale = savedExportScale;
            }
        }

        private static void ExportSpriteSheetToPath(string path, int columns, int rows, int marginPx)
        {
            float savedExportScale = State.ExportAnimationTimeScale;

            int total = columns * rows;
            if (total <= 0)
                return;

            State.ExportGifGodotTimeMapping = true;

            try
            {
                int cell = Config.PLANET_RECT_SIZE;
                int sheetW = columns * cell + (columns + 1) * marginPx;
                int sheetH = rows * cell + (rows + 1) * marginPx;

                var sheet = new Rgba32[sheetW * sheetH];
                for (int i = 0; i < sheet.Length; i++)
                    sheet[i] = default;

                var buffer = new XnaColor[cell * cell];

                for (int idx = 0; idx < total; idx++)
                {
                    // Match Godot: sample t in [0, 1) so animation loops seamlessly
                    State.ExportAnimationPhase01 = total <= 1 ? 0f : idx / (float)total;

                    Shaders.Update();

                    Renderer.CapturePlanetPixels(buffer, transparentBackground: true);

                    int col = idx % columns;
                    int row = idx / columns;
                    int destX = marginPx + col * (cell + marginPx);
                    int destY = marginPx + row * (cell + marginPx);

                    for (int py = 0; py < cell; py++)
                    {
                        int srcRow = py * cell;
                        int dstBase = (destY + py) * sheetW + destX;
                        for (int px = 0; px < cell; px++)
                        {
                            XnaColor c = buffer[srcRow + px];
                            sheet[dstBase + px] = new Rgba32(c.R, c.G, c.B, c.A);
                        }
                    }
                }

                using var img = Image.LoadPixelData<Rgba32>(sheet, sheetW, sheetH);
                using (var fs = File.Create(path))
                    img.SaveAsPng(fs);
            }
            finally
            {
                State.ExportGifGodotTimeMapping = false;
                State.ExportAnimationTimeScale = savedExportScale;
            }
        }

        private static Rgba32[] ColorsToRgba(XnaColor[] buffer)
        {
            var rgba = new Rgba32[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                XnaColor c = buffer[i];
                rgba[i] = new Rgba32(c.R, c.G, c.B, c.A);
            }

            return rgba;
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "planet";
            char[] invalid = Path.GetInvalidFileNameChars();
            return new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        }

        public static void Update()
        {
        }

        public static void Draw()
        {
        }
    }
}
