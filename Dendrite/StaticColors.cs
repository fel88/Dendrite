using System.Drawing;

namespace Dendrite
{
    public static class StaticColors
    {
        public static Brush ConvBrush = new SolidBrush(Color.FromArgb(51, 85, 136));
        public static Brush DropoutBrush = new SolidBrush(Color.FromArgb(69, 71, 112));
        public static Brush BatchNormBrush = new SolidBrush(Color.FromArgb(51, 85, 68));
        public static Brush ReluBrush = new SolidBrush(Color.FromArgb(112, 41, 33));
        public static Brush ConcatBrush = new SolidBrush(Color.FromArgb(89, 66, 59));        
        public static Brush MathBrush = Brushes.Black;
        public static Brush PoolBrush = new SolidBrush(Color.FromArgb(51, 85, 51));
        public static Brush EndpointBrush = new SolidBrush(Color.FromArgb(238, 238, 238));
    }
}
