using System.Drawing;

namespace Dendrite
{
    public static class StaticColors
    {
        public static Brush ConvBrush = new SolidBrush(Color.FromArgb(51, 85, 136));
        public static Brush BatchNormBrush = new SolidBrush(Color.FromArgb(51, 85, 68));
        public static Brush ReluBrush = new SolidBrush(Color.FromArgb(75, 27, 22));
        public static Brush ConcatBrush = new SolidBrush(Color.FromArgb(89, 66, 59));
        public static Brush AddBrush = Brushes.Black;
    }
}
