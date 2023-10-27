using System.Drawing;
using System.Drawing.Drawing2D;

namespace Dendrite
{
    public class EdgeNode
    {
        public static IDrawingContext DrawingContext;
        public EdgeNode() { }
        public EdgeNode(PointF[] p)
        {
            Init(p);
        }
        public void Init(PointF[] p)
        {
            Points = p;
            curve = new Curve(p, DrawingContext.NewPathObject());
        }
        public PointF[] Points;
        Curve curve;

        public void Draw(IDrawingContext ctx)
        {
            var size = 4 * ctx.zoom;
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(size, size, true);
            Pen pen1 = new Pen(Color.Black);
            pen1.CustomEndCap = bigArrow;
            ctx.PushMatrix();
            //var temp = ctx.Transform;

            ctx.ScaleTransform(ctx.zoom, ctx.zoom);
            ctx.TranslateTransform(ctx.sx, ctx.sy);

            ctx.DrawPath(pen1, curve.Path);
            //ctx.Transform = temp;
            ctx.PopMatrix();


        }
    }
}
