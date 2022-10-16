using System;
using System.Drawing;
using System.IO;

namespace Dendrite
{
    public static class Extensions
    {
        public static PointF Normalized(this PointF input)
        {
            var len = input.Length();
            PointF ret = new PointF(input.X / len, input.Y / len);
            return ret;
        }

        public static PointF Mul(this PointF input, float t)
        {
            PointF ret = new PointF(input.X * t, input.Y * t);
            return ret;
        }

        public static float Length(this PointF input)
        {
            var d = input.X * input.X + input.Y * input.Y;
            return (float)Math.Sqrt(d);
        }
    }
}

