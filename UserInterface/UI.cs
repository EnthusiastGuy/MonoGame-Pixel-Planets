using ShadersTest;

namespace UserInterface
{
    /// <summary>
    /// Composition root for UI: delegates layout/input to <see cref="ParameterInspector"/> and drawing to <see cref="PlanetHud"/>.
    /// </summary>
    static class UI
    {
        public static void Update()
        {
            ParameterInspector.UpdateLayout();
        }

        public static void Draw()
        {
            Renderer.Clear();
            Renderer.DrawShader();
            PlanetHud.Draw();
        }
    }
}
