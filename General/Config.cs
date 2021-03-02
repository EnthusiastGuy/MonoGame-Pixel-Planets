using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Reflection;

namespace ShadersTest
{
    static class Config
    {
        public static readonly string APP_NAME = "Pixel Planets";
        public static readonly string APP_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly int VIEWPORT_WIDTH = 1280;
        public static readonly int VIEWPORT_HEIGHT = 640;

        public static readonly int PLANET_RECT_SIZE = 400;
        public static readonly int PLANET_PADDING = (VIEWPORT_HEIGHT - PLANET_RECT_SIZE) / 2;

        // Used in json outputs
        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        // Used for drawing
        public static readonly Rectangle ONE_PIXEL_RECT = new Rectangle(0, 0, 1, 1);
    }
}
