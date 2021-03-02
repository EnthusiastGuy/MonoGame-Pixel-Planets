using Microsoft.Xna.Framework;
using ShadersTest;
using ShadersTest.UI;
using System;
using System.Collections.Generic;

namespace UserInterface
{
    static class UI
    {
        private static List<DisplayableProperty> drawableProps = new List<DisplayableProperty>();
        private static int largestDisplayNameWidth;
        private static DisplayableProperty lockedProperty;

        public static void Update()
        {
            // Update UI stuff here
            largestDisplayNameWidth = 0;
            drawableProps = State.GetUIParamsList();
            foreach (DisplayableProperty property in drawableProps)
            {
                Vector2 displayArea = Fonts.VerdanaBold.MeasureString(property.DisplayName);
                property.Rectangle = new Rectangle(
                    Config.PLANET_RECT_SIZE + 2 * Config.PLANET_PADDING,
                    10 + property.Position * 20, (int)displayArea.X,
                    (int)displayArea.Y);

                if (largestDisplayNameWidth < displayArea.X)
                    largestDisplayNameWidth = (int)displayArea.X;


                property.Hovered = property.Rectangle.Contains(State.MouseX, State.MouseY);
                

                if (!State.MouseL)
                {
                    lockedProperty = null;
                }
                else
                {
                    if (property.Hovered && lockedProperty == null)
                    {
                        property.Locked = true;
                        lockedProperty = property;
                    }
                    else if (lockedProperty != null && lockedProperty.Key == property.Key)
                    {
                        if (MouseStates.MouseMiddleClickPressed())
                        {
                            State.SetDefaultValueFor(property.Key);
                        } else
                        {
                            int mouseDelta = MouseStates.GetMouseWheelDelta();
                            if (mouseDelta != 0)
                                State.ModifyValueFor(property.Key, mouseDelta);
                        }
                        property.Locked = true;
                    }
                }
            }

            // Some offset to display the value a bit to the right
            largestDisplayNameWidth += 4;
        }

        public static void Draw()
        {
            Renderer.Clear();
            Renderer.DrawShader();
            DrawTexts();
        }

        private static void DrawTexts()
        {
            Renderer.StartTextBatch();

            Renderer.DrawTitle(State.GetCurrentCelestialName(), 10, 10);
            Renderer.DrawInfo(State.GetCurrentCelestialInfo(), 10, 34);

            Renderer.DrawInfo("Press Up/Down to change: " + Math.Floor(State.Pixels) + " pixels", 10, 52);

            Renderer.DrawSimple(State.HelperLine1, 10, Config.VIEWPORT_HEIGHT - 48, Color.AliceBlue);
            Renderer.DrawSimple(State.HelperLine2, 10, Config.VIEWPORT_HEIGHT - 30, Color.AliceBlue);

            foreach(DisplayableProperty property in drawableProps)
            {
                Renderer.DrawSimpleOrange(property.DisplayName, property.Rectangle.X, property.Rectangle.Y);
                Renderer.DrawSimple(
                    property.Value,
                    property.Rectangle.X + largestDisplayNameWidth,
                    property.Rectangle.Y + 1,
                    property.IsDefault ? Color.GreenYellow : Color.Cyan);

                if (property.Locked)
                {
                    Renderer.DrawLShapedSelection(
                        property.Rectangle.X - 2,
                        property.Rectangle.Y,
                        275,
                        18, Color.OrangeRed); ;
                } else if (property.Hovered && lockedProperty == null)
                {
                    Renderer.DrawLShapedSelection(
                        property.Rectangle.X - 2,
                        property.Rectangle.Y,
                        275,
                        18, Color.DarkOrange);
                }
            }

            Renderer.EndBatch();
        }
    }
}
