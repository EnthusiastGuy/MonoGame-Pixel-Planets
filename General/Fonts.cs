using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadersTest
{
    static class Fonts
    {
        public static SpriteFont VerdanaBold;
        public static SpriteFont Verdana;

        public static void LoadFonts()
        {
            VerdanaBold = GameContent.LoadSpriteFont("Fonts/VerdanaBold");
            Verdana = GameContent.LoadSpriteFont("Fonts/Verdana");
        }
    }
}
