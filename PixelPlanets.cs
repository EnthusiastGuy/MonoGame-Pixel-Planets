﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShadersTest
{
    public class PixelPlanets : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public PixelPlanets()
        {
            Renderer.RegisterGraphicsDeviceManager(new GraphicsDeviceManager(this) { GraphicsProfile = GraphicsProfile.HiDef });
            GameContent.RegisterContent(Content);
        }

        protected override void Initialize()
        {
            base.Initialize();
            Renderer.RegisterGraphics();
            IsMouseVisible = true;
            Window.Title = string.Format("{0} - {1}", Config.APP_NAME, Config.APP_VERSION);

            Shaders.UpdateShader();
        }

        protected override void LoadContent()
        {
            Renderer.Initialize();
            Fonts.LoadFonts();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            UserInput.Update();
            State.Update();
            Shaders.Update();
            Export.Exporter.Update();
            UserInterface.UI.Update();
            GameUpdate();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            UserInterface.UI.Draw();
            base.Draw(gameTime);
        }

        private void GameUpdate()
        {
            // Exit() method is a member of Game, which this what this PixelPlanets class inherits on,
            // therefore, the action is done here based on the state.ExitRequested flag
            if (State.ExitRequested)
                Exit();
        }
    }
}
