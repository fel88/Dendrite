using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dendrite
{
    public class PipelineUI
    {
        public List<IUIElement> Elements = new List<IUIElement>();
        PipelineGraph Graph;

        public void Draw(DrawingContext ctx)
        {
            //draw nodes first
            foreach (var item in Elements)
            {
                item.Draw(ctx);
            }
            //draw links
            var ar = Elements.OfType<NodeUI>().SelectMany(z => z.Node.Inputs.Union(z.Node.Outputs)).ToArray();
            var links = ar.SelectMany(z => z.InputLinks.Union(z.OutputLinks)).Distinct().ToArray();
            var nui = Elements.OfType<NodeUI>().ToArray();
            foreach (var item in links)
            {
                var fr = nui.First(z => z.Node == item.Input.Parent);
                var fr2 = nui.First(z => z.Node == item.Output.Parent);
                var pos1 = fr.GetPinPosition(ctx, item.Input);
                var rpos2 = fr2.GetPinPosition(ctx, item.Output);
                var vec = new PointF(pos1.X - rpos2.X, pos1.Y - rpos2.Y);
                var dir = vec.Normalized();
                PointF norm = new PointF(-dir.Y, dir.X);
                var cp1 = vec.Mul(0.25f);
                var cp2 = vec.Mul(0.75f);
                int side = (int)(vec.Length()/3);
                cp1 = new PointF(rpos2.X + cp1.X + norm.X * side, rpos2.Y + cp1.Y + norm.Y * side);
                cp2 = new PointF(rpos2.X + cp2.X - norm.X * side, rpos2.Y + cp2.Y - norm.Y * side);

                //ctx.Graphics.FillEllipse(Brushes.Red, cp1.X, cp1.Y, 10, 10);
                //ctx.Graphics.FillEllipse(Brushes.Red, cp2.X, cp2.Y, 10, 10);
                ctx.Graphics.DrawBezier(new Pen(Color.Yellow,2), pos1, cp1, cp2, rpos2);
            }
        }

        public void Init(PipelineGraph pg)
        {
            Graph = pg;
            foreach (var item in Graph.Nodes)
            {
                Elements.Add(new NodeUI() { Node = item });
            }

            //layout
            //topo sort first
            int xx = 0;
            int yy = 100;
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].Position = new PointF(xx, yy);
                xx += 250;
                yy += 150;
            }
        }
    }
}


