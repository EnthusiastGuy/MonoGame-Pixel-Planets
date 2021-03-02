using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadersTest
{
    public class MouseStates
    {
        static MouseState currentMouseState;
        static MouseState previousMouseState;

        public static void Update()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }

        public static bool MouseRightClickPressed()
        {
            return currentMouseState.RightButton == ButtonState.Pressed &&
                previousMouseState.RightButton == ButtonState.Released;
        }

        public static bool MouseMiddleClickPressed()
        {
            return currentMouseState.MiddleButton == ButtonState.Pressed &&
                previousMouseState.MiddleButton == ButtonState.Released;
        }

        public static int GetMouseWheelDelta()
        {
            return currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
        }

        public static bool NoMouseButtonsIsPressed()
        {
            return currentMouseState.RightButton == ButtonState.Released &&
                currentMouseState.LeftButton == ButtonState.Released &&
                currentMouseState.MiddleButton == ButtonState.Released &&
                currentMouseState.XButton1 == ButtonState.Released &&
                currentMouseState.XButton2 == ButtonState.Released;
        }
    }
}
