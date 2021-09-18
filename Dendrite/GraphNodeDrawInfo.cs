using System.Drawing;

namespace Dendrite
{
    public class GraphNodeDrawInfo
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public RectangleF Rect
        {
            get
            {
                return new RectangleF(X, Y, Width, Height);
            }
        }
        public string Text;
    }
}
