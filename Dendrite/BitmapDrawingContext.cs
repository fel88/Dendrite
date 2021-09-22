using System.Drawing;
using System.Linq;

namespace Dendrite
{
    public class BitmapDrawingContext : IDrawingContext
    {
        public Bitmap Bmp { get; set; }

        public Graphics Graphics { get; set; }

        public float Zoom { get; set; }

        public PointF Shift { get; set; }

        public void FitToPoints(PointF[] points, int gap = 0)
        {
            var maxx = points.Max(z => z.X) + gap;
            var minx = points.Min(z => z.X) - gap;
            var maxy = points.Max(z => z.Y) + gap;
            var miny = points.Min(z => z.Y) - gap;

            var w = Bmp.Width;
            var h = Bmp.Height;

            var dx = maxx - minx;
            var kx = w / dx;
            var dy = maxy - miny;
            var ky = h / dy;

            var oz = Zoom;
            var sz1 = new Size((int)(dx * kx), (int)(dy * kx));
            var sz2 = new Size((int)(dx * ky), (int)(dy * ky));
            Zoom = kx;
            if (sz1.Width > w || sz1.Height > h) Zoom = ky;

            var x = dx / 2 + minx;
            var y = dy / 2 + miny;

            Shift = new PointF(((w / 2f) / Zoom - x), ((h / 2f) / Zoom - y));            

        }

        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + Shift.X) * Zoom, ( 1) * (p1.Y + Shift.Y) * Zoom);
        }
        public virtual PointF Transform(double x, double y)
        {
            return new PointF(((float)(x) + Shift.X) * Zoom, ( 1) * ((float)(y) + Shift.Y) * Zoom);
        }
        public Rectangle Transform(Rectangle rect)
        {
            var t1 = Transform(new PointF(rect.Left, rect.Top));
            return new Rectangle((int)t1.X, (int)t1.Y, (int)(rect.Width * Zoom), (int)(rect.Height * Zoom));
        }
        public RectangleF Transform(RectangleF rect)
        {
            var t1 = Transform(new PointF(rect.Left, rect.Top));
            return new RectangleF(t1.X, t1.Y, (rect.Width * Zoom), (rect.Height * Zoom));
        }

    }
}
