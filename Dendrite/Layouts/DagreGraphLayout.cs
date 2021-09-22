using Dagre;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Dendrite.Layouts
{
    public class DagreGraphLayout : GraphLayout
    {
        public override bool FlashHoveredRelatives { get; set; } = false;
        public override bool DrawHeadersAllowed { get; set; } = true;
        public override bool EdgesDrawAllowed { get; set; } = true;

        void updateNodesSizes(GraphModel model)
        {
            foreach (var item in model.Nodes)
            {
                GraphNodeDrawInfo dd = new GraphNodeDrawInfo() { X = 0, Y = 0, Width = 300, Height = 100 };
                item.DrawTag = dd;
                switch (item.LayerType)
                {
                    case LayerType.Lstm:
                        dd.Width = 220;
                        dd.Height = 140;
                        break;
                    case LayerType.Conv:
                    case LayerType.Gemm:
                        dd.Width = 220;
                        dd.Height = 110;
                        break;
                    case LayerType.Batch:
                        dd.Width = 250;
                        dd.Height = 170;
                        break;
                    case LayerType.PrimitiveMath:
                        dd.Width = 150;
                        dd.Height = 50;
                        break;
                    case LayerType.MathOperation:

                        if (item.Parents.Count < 2)
                        {
                            dd.Width = 210;
                            dd.Height = 90;
                        }
                        else
                        {
                            dd.Width = 150;
                            dd.Height = 50;
                        }
                        break;
                    case LayerType.Gather:
                        dd.Width = 200;
                        dd.Height = 90;
                        break;
                    case LayerType.Squeeze:
                        dd.Width = 150;
                        dd.Height = 50;
                        break;
                    case LayerType.Output:
                    case LayerType.Input:
                        dd.Width = 150;
                        dd.Height = 50;
                        break;
                    case LayerType.Dropout:
                    case LayerType.Concat:
                    case LayerType.Relu:
                    case LayerType.Pad:
                        dd.Width = 120;
                        dd.Height = 50;
                        break;
                    case LayerType.Pool:
                    case LayerType.Transpose:
                    case LayerType.Softmax:
                    case LayerType.Log:
                        dd.Width = 150;
                        dd.Height = 50;
                        break;
                }
            }
        }

        public override void Layout(GraphModel model)
        {
            DagreInputGraph d = new DagreInputGraph();
            updateNodesSizes(model);

            model.Nodes = model.Nodes.Where(z => z.LayerType != LayerType.Constant && (z.Childs.Any() || z.Parent != null || z.Parents.Any())).ToArray();

            var list1 = model.Nodes.ToList();


            foreach (var gg in list1)
            {
                var tag = (gg.DrawTag as GraphNodeDrawInfo);
                d.AddNode(gg, tag.Rect.Width, tag.Rect.Height);
            }

            foreach (var gg in list1)
            {
                foreach (var item in gg.Childs)
                {
                    var nd1 = d.GetNode(gg);
                    var nd2 = d.GetNode(item);
                    var minlen = (item.Parents.Count == 0 || item.Childs.Count == 0 || gg.Parents.Count == 0 || gg.Childs.Count == 0) ? 3 : 1;
                    d.AddEdge(nd1, nd2, minlen);
                }
            }

            d.Layout();

            //back
            foreach (var n in model.Nodes)
            {
                var nd = d.GetNode(n);
                if (nd == null) continue;
                var tag = (n.DrawTag as GraphNodeDrawInfo);
                var xx = nd.X;
                var yy = nd.Y;
                tag.X = xx;
                tag.Y = yy;
            }


            List<EdgeNode> enodes = new List<EdgeNode>();
            foreach (var item in d.Edges())
            {
                var pnts = item.Points;
                List<PointF> rr = new List<PointF>();
                foreach (var itemz in pnts)
                {
                    rr.Add(new PointF(itemz.X, itemz.Y));
                }

                enodes.Add(new EdgeNode(rr.ToArray()));
            }
            model.Edges = enodes.ToArray();
        }
    }
}
