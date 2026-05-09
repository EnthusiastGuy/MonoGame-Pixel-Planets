using Microsoft.Xna.Framework;
using ShadersTest;
using ShadersTest.UI;
using System;

namespace UserInterface
{
    /// <summary>
    /// Draws chrome around the planet viewport and the scrollable parameter column.
    /// </summary>
    static class PlanetHud
    {
        public static void Draw()
        {
            Renderer.StartTextBatch();

            Renderer.DrawTitle(State.GetCurrentCelestialName(), 10, 10);
            Renderer.DrawInfo(State.GetCurrentCelestialInfo(), 10, 34);

            Renderer.DrawInfo("Press Up/Down to change: " + Math.Floor(State.Pixels) + " pixels", 10, 52);

            Renderer.DrawSimple(State.HelperLine1, 10, Config.VIEWPORT_HEIGHT - 48, Color.AliceBlue);
            Renderer.DrawSimple(State.HelperLine2, 10, Config.VIEWPORT_HEIGHT - 30, Color.AliceBlue);

            int labelWidth = ParameterInspector.LargestDisplayNameWidth;

            foreach (DisplayableProperty property in ParameterInspector.DrawableProperties)
            {
                Renderer.DrawSimpleOrange(property.DisplayName, property.Rectangle.X, property.Rectangle.Y);
                Renderer.DrawSimple(
                    property.Value,
                    property.Rectangle.X + labelWidth,
                    property.Rectangle.Y + 1,
                    property.IsDefault ? Color.GreenYellow : Color.Cyan);

                if (property.Locked)
                {
                    Renderer.DrawLShapedSelection(
                        property.Rectangle.X - 2,
                        property.Rectangle.Y,
                        275,
                        18,
                        Color.OrangeRed);
                }
                else if (property.Hovered && ParameterInspector.LockedProperty == null)
                {
                    Renderer.DrawLShapedSelection(
                        property.Rectangle.X - 2,
                        property.Rectangle.Y,
                        275,
                        18,
                        Color.DarkOrange);
                }
            }

            Renderer.EndBatch();
        }
    }
}
