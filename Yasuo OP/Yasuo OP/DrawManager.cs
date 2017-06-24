using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK.Rendering;
using SharpDX;
using SharpDX.Direct3D9;
using static Yasuo_OP.Helper;
using static Yasuo_OP.SpellsManager;

namespace Yasuo_OP
{
    class DrawManager
    {
        private static Font font;
        public static void Load()
        {
            Drawing.OnDraw += Drawing_OnDraw;

            font = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Segoi UI",
                    Height = 22,
                    OutputPrecision = FontPrecision.Device,
                    Quality = FontQuality.ClearType
                });
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            Circle.Draw(Color.Black, GetQRange(), Me);
        }
    }
}
