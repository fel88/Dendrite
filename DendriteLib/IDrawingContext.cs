using System.Drawing;

namespace Dendrite
{
    public interface IDrawingContext
    {
        Bitmap Bmp { get; }
        Graphics Graphics { get; }
        float Zoom { get; }
        PointF Shift { get; }
        PointF Transform(PointF p1);
        PointF Transform(double x, double y);
        Rectangle Transform(Rectangle rect);
        RectangleF Transform(RectangleF rect);

        void FitToPoints(PointF[] points, int gap = 0);
    }

}
