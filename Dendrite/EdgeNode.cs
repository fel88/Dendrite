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

        internal void Draw(DrawingContext ctx)
        {
            var size = 4 * ctx.zoom;
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(size, size, true);
            Pen pen1 = new Pen(Color.Black);
            pen1.CustomEndCap = bigArrow;

            var temp = ctx.Graphics.Transform;

            ctx.Graphics.ScaleTransform(ctx.zoom, ctx.zoom);
            ctx.Graphics.TranslateTransform(ctx.sx, ctx.sy);

            ctx.Graphics.DrawPath(pen1, curve.Path);
            ctx.Graphics.Transform = temp;


        }
    }
}
