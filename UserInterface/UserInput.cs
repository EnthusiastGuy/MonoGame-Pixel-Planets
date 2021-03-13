using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ShadersTest
{
    static class UserInput
    {
        public static void Update()
        {
            Keyboard.Update();
            MouseStates.Update();

            InterpretKeyboardActions();
            InterpretMouseActions();
        }

        private static void InterpretKeyboardActions()
        {
            if (Keyboard.KeyIsReleased(Keys.Right))
            {
                State.NextCelestial();
                Shaders.UpdateShader();
            }
            else if (Keyboard.KeyIsReleased(Keys.Left))
            {
                State.PreviousCelestial();
                Shaders.UpdateShader();
            }

            if (Keyboard.IsPressed(Keys.Up))
            {
                State.Pixels += .3f;
                if (State.Pixels >= 400)
                {
                    State.Pixels = 400;
                }
                Shaders.UpdateShader();
            }

            if (Keyboard.IsPressed(Keys.Down))
            {
                State.Pixels -= .3f;
                if (State.Pixels < 16)
                {
                    State.Pixels = 16;
                }
                Shaders.UpdateShader();
            }

            if (Keyboard.KeyIsReleased(Keys.Space))
            {
                State.RotationPaused = !State.RotationPaused;
            }

            if (Keyboard.IsPressed(Keys.F1))
            {
                State.Save();
            }

            if (Keyboard.KeyIsReleased(Keys.Escape))
            {
                State.ExitRequested = true;
            }

            if (Keyboard.KeyIsReleased(Keys.Tab))
            {
                State.RandomizeParams();
            }

            if (Keyboard.KeyIsReleased(Keys.OemTilde))
            {
                State.RestoreDefaults();
            }

            if (Keyboard.IsPressed(Keys.X))
            {
                Export.Exporter.SaveScreenshot();
            }
        }

        private static void InterpretMouseActions()
        {
            State.MouseX = Mouse.GetState().X;
            State.MouseY = Mouse.GetState().Y;
            State.MouseL = Mouse.GetState().LeftButton == ButtonState.Pressed;

            State.TimeStoppedByMouse = (Mouse.GetState().RightButton == ButtonState.Pressed);

            if (MouseStates.MouseRightClickPressed())
            {
                State.ClickedMouseX = Mouse.GetState().X;
                State.ClickedMouseY = Mouse.GetState().Y;
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                int x = Mouse.GetState().X - State.ClickedMouseX;

                State.CustomTime = -(float)x / 10;

                State.HelperLine2 = "Rotating planet. Time offset: " + State.CustomTime;
            }
            else
            {
                State.Time += State.CustomTime;
                State.CustomTime = 0.0f;
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                int x = Mouse.GetState().X;
                int y = Mouse.GetState().Y;
                float limit = Config.PLANET_PADDING * 2 + Config.PLANET_RECT_SIZE;

                if (x >= 0 && x <= limit && y >= 0 && y <= limit)
                {
                    State.LightOrigin = new Vector2(x / limit, y / limit);
                    Shaders.UpdateShader();

                    State.HelperLine2 = "Moving light to: " + Math.Round(x / limit, 2) + ", " + Math.Round(y / limit, 2);
                }
            }

            if (MouseStates.NoMouseButtonsIsPressed())
            {
                State.HelperLine2 = "Click to move light, right click to rotate, space " + (State.RotationPaused ? "[PAUSED]" : "to pause") + 
                    ", click on a property, hold and scroll wheel to change it. Middle click to reset it while editing";
            }
        }
    }
}
