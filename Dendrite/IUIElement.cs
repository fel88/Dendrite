using System.Drawing;

namespace Dendrite
{
    public interface IUIElement
    {
        PointF Position { get; set; }
        void Draw(DrawingContext ctx);
    }
}


