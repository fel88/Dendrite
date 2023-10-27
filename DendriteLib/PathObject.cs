using System.Drawing;

namespace Dendrite
{
    public abstract class PathObject
    {
        public abstract RectangleF GetBounds();
        
        public abstract bool IsVisible(Point pos);

        public abstract void AddBezier(PointF pointF, PointF cp1, PointF cp2, PointF cp3);
        public abstract void AddLine(PointF pointF1, PointF pointF2);
    }

}
