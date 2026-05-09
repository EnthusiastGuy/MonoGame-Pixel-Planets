using Microsoft.Xna.Framework;
using ShadersTest;
using ShadersTest.UI;
using System.Collections.Generic;

namespace UserInterface
{
    /// <summary>
    /// Hit-testing, drag-lock, and wheel editing for the shader parameter list (right-hand column).
    /// </summary>
    static class ParameterInspector
    {
        private static List<DisplayableProperty> drawableProps = new List<DisplayableProperty>();
        private static int largestDisplayNameWidth;
        private static DisplayableProperty lockedProperty;

        public static DisplayableProperty LockedProperty => lockedProperty;

        public static IReadOnlyList<DisplayableProperty> DrawableProperties => drawableProps;

        public static int LargestDisplayNameWidth => largestDisplayNameWidth;

        public static void UpdateLayout()
        {
            largestDisplayNameWidth = 0;
            drawableProps = State.GetUIParamsList();

            foreach (DisplayableProperty property in drawableProps)
            {
                Vector2 displayArea = Fonts.VerdanaBold.MeasureString(property.DisplayName);
                property.Rectangle = new Rectangle(
                    Config.PLANET_RECT_SIZE + 2 * Config.PLANET_PADDING,
                    10 + property.Position * 20,
                    (int)displayArea.X,
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
                        }
                        else
                        {
                            int mouseDelta = MouseStates.GetMouseWheelDelta();
                            if (mouseDelta != 0)
                                State.ModifyValueFor(property.Key, mouseDelta);
                        }
                        property.Locked = true;
                    }
                }
            }

            largestDisplayNameWidth += 4;
        }
    }
}
