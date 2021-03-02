namespace ShadersTest
{
    using Microsoft.Xna.Framework;
    
    public class PixelPlanets : Game
    {
        readonly GraphicsDeviceManager graphics;
        
        public PixelPlanets()
        {
            // TODO move the graphics from here
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Config.VIEWPORT_WIDTH,
                PreferredBackBufferHeight = Config.VIEWPORT_HEIGHT
            };
            graphics.ApplyChanges();
            
            GameContent.Initialize(Content);
        }

        protected override void Initialize()
        {
            base.Initialize();
            IsMouseVisible = true;
            Window.Title = string.Format("{0} - {1}", Config.APP_NAME, Config.APP_VERSION);

            Shaders.UpdateShader();
        }

        protected override void LoadContent()
        {
            Renderer.Initialize(GraphicsDevice);
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
