using System;

namespace ShadersTest
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new PixelPlanets())
                game.Run();
        }
    }
}
