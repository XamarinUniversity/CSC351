using Xamarin.Forms;
using System;

namespace FractlExplorer
{
    static class ColorExtensions
    {
        public static int ToABGR(this Color c)
        {
            return 255 << 24 
            | ((byte)(c.B * 255)) << 16
            | ((byte)(c.G * 255)) << 8
            | ((byte)(c.R * 255));
        }
    }
}
