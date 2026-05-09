using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ShadersTest;
using ShadersTest.Export;
using System;

namespace UserInterface
{
    static class ExportView
    {
        private static int sheetColumns = 8;
        private static int sheetRows = 8;
        private static int sheetMargin = 0;
        private static int gifFrames = 32;
        /// <summary>GIF delay per frame in centiseconds (1/100 s), standard GIF timing unit.</summary>
        private static int gifDelayCentiseconds = 5;

        private static readonly int ContentX = Config.PLANET_RECT_SIZE + 2 * Config.PLANET_PADDING + 20;
        private static readonly int ValueX = ContentX + 190;

        private const int GifFieldsTop = 268;
        private const int GifRowSpacing = 22;

        public static void HandleInput()
        {
            int mx = Mouse.GetState().X;
            int my = Mouse.GetState().Y;
            int wheelDelta = MouseStates.GetMouseWheelDelta();
            int step = wheelDelta > 0 ? 1 : wheelDelta < 0 ? -1 : 0;

            if (step != 0)
            {
                if (FieldRect(0).Contains(mx, my))
                    sheetColumns = MathHelper.Clamp(sheetColumns + step, 1, 32);
                else if (FieldRect(1).Contains(mx, my))
                    sheetRows = MathHelper.Clamp(sheetRows + step, 1, 32);
                else if (FieldRect(2).Contains(mx, my))
                    sheetMargin = MathHelper.Clamp(sheetMargin + step, 0, 32);
                else if (GifFieldRect(0).Contains(mx, my))
                    gifFrames = MathHelper.Clamp(gifFrames + step * 8, 8, 512);
                else if (GifFieldRect(1).Contains(mx, my))
                    gifDelayCentiseconds = MathHelper.Clamp(gifDelayCentiseconds + step, 1, 255);
            }

            if (MouseStates.MouseLeftClickReleased())
            {
                if (SheetBtnRect.Contains(mx, my))
                    Exporter.SaveSpriteSheetWithDialog(sheetColumns, sheetRows, sheetMargin);
                else if (GifBtnRect.Contains(mx, my))
                    Exporter.SaveGifWithDialog(gifFrames, gifDelayCentiseconds);
                else if (CancelBtnRect.Contains(mx, my))
                    State.ExportViewOpen = false;
            }

            if (ShadersTest.Keyboard.KeyIsReleased(Keys.Escape))
                State.ExportViewOpen = false;
        }

        public static void Draw()
        {
            int mx = Mouse.GetState().X;
            int my = Mouse.GetState().Y;

            // --- Spritesheet ---
            Renderer.DrawBoldOrange("Spritesheet", ContentX, 50);
            Renderer.DrawEmptyColorRectangle(ContentX, 70, 300, 1, Color.DimGray);

            DrawField("Frames (width)", sheetColumns.ToString(), 0, mx, my);
            DrawField("Frames (height)", sheetRows.ToString(), 1, mx, my);
            DrawField("Pixel margin", sheetMargin.ToString(), 2, mx, my);

            int total = sheetColumns * sheetRows;
            int cell = Config.PLANET_RECT_SIZE;
            int resW = sheetColumns * cell + (sheetColumns + 1) * sheetMargin;
            int resH = sheetRows * cell + (sheetRows + 1) * sheetMargin;
            Renderer.DrawSimple($"Total: {total} frames   {resW} x {resH} px", ContentX, 158, Color.Gray);

            DrawButton("Export spritesheet", SheetBtnRect, mx, my);

            // --- Gif ---
            Renderer.DrawBoldOrange("Gif", ContentX, 230);
            Renderer.DrawEmptyColorRectangle(ContentX, 250, 300, 1, Color.DimGray);

            DrawGifRow("Frames", gifFrames.ToString(), 0, mx, my);
            DrawGifRow("Frame delay (cs)", gifDelayCentiseconds.ToString(), 1, mx, my);
            Renderer.DrawSimple(
                "cs = centiseconds (1/100 s) per frame - wheel over a row to edit",
                ContentX,
                GifFieldsTop + 2 * GifRowSpacing + 6,
                Color.Gray);

            DrawButton("Export gif", GifBtnRect, mx, my);

            // --- Cancel ---
            DrawButton("CANCEL", CancelBtnRect, mx, my);
        }

        // --- Layout ---

        private static int FieldY(int index) => 86 + index * 22;

        private static Rectangle FieldRect(int index) =>
            new Rectangle(ContentX - 2, FieldY(index), 300, 20);

        private static Rectangle GifFieldRect(int row) =>
            new Rectangle(ContentX - 2, GifFieldsTop + row * GifRowSpacing, 300, 20);

        private static Rectangle SheetBtnRect =>
            new Rectangle(ContentX, 186, 220, 24);

        private static Rectangle GifBtnRect =>
            new Rectangle(ContentX, 336, 220, 24);

        private static Rectangle CancelBtnRect =>
            new Rectangle(Config.VIEWPORT_WIDTH - 120, Config.VIEWPORT_HEIGHT - 40, 100, 24);

        // --- Drawing helpers ---

        private static void DrawField(string label, string value, int index, int mx, int my)
        {
            bool hovered = FieldRect(index).Contains(mx, my);
            int y = FieldY(index);
            Renderer.DrawSimple(label, ContentX, y, hovered ? Color.Orange : Color.DarkOrange);
            Renderer.DrawSimple(value, ValueX, y, hovered ? Color.Cyan : Color.GreenYellow);
            if (hovered)
                Renderer.DrawLShapedSelection(ContentX - 2, y, 300, 18, Color.DarkOrange);
        }

        private static void DrawGifRow(string label, string value, int row, int mx, int my)
        {
            int y = GifFieldsTop + row * GifRowSpacing;
            bool hovered = GifFieldRect(row).Contains(mx, my);
            Renderer.DrawSimple(label, ContentX, y, hovered ? Color.Orange : Color.DarkOrange);
            Renderer.DrawSimple(value, ValueX, y, hovered ? Color.Cyan : Color.GreenYellow);
            if (hovered)
                Renderer.DrawLShapedSelection(ContentX - 2, y, 300, 18, Color.DarkOrange);
        }

        private static void DrawButton(string text, Rectangle rect, int mx, int my)
        {
            bool hovered = rect.Contains(mx, my);
            Renderer.DrawEmptyColorRectangle(rect, hovered ? Color.Orange : Color.DimGray);
            Renderer.DrawSimple(text, rect.X + 8, rect.Y + 4, hovered ? Color.White : Color.AliceBlue);
        }
    }
}
