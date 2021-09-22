using System.Drawing;
using System.Drawing.Drawing2D;

namespace Dendrite
{
    public class EdgeNode
    {
        public EdgeNode() { }
        public EdgeNode(PointF[] p)
        {
            Init(p);
        }
        public void Init(PointF[] p)
        {
            Points = p;
            curve = new Curve(p);
        }
        public PointF[] Points;
        Curve curve;

        public void Draw(IDrawingContext ctx)
        {
            var size = 4 * ctx.Zoom;
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(size, size, true);
            Pen pen1 = new Pen(Color.Black);
            pen1.CustomEndCap = bigArrow;

            var temp = ctx.Graphics.Transform;

            ctx.Graphics.ScaleTransform(ctx.Zoom, ctx.Zoom);
            ctx.Graphics.TranslateTransform(ctx.Shift.X, ctx.Shift.Y);

            ctx.Graphics.DrawPath(pen1, curve.Path);
            ctx.Graphics.Transform = temp;


        }
    }
}
