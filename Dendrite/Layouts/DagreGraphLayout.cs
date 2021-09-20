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
            DagreGraph dg = new DagreGraph(true);
            int ii = 0;
            updateNodesSizes(model);

            DagreLayout dl = new DagreLayout();

            model.Nodes = model.Nodes.Where(z => z.LayerType != LayerType.Constant && (z.Childs.Any() || z.Parent != null || z.Parents.Any())).ToArray();

            var list1 = model.Nodes.ToList();

            foreach (var gg in list1)
            {
                var ind = list1.IndexOf(gg);
                dg.setNodeRaw(ind + "", new JavaScriptLikeObject());
                var nd = dg.node(ind + "");

                nd["source"] = gg;

                var tag = (gg.DrawTag as GraphNodeDrawInfo);
                nd["width"] = tag.Rect.Width;
                nd["height"] = tag.Rect.Height;

            }
            foreach (var gg in list1)
            {
                var ind = list1.IndexOf(gg);

                foreach (var item in gg.Childs)
                {
                    JavaScriptLikeObject jj = new JavaScriptLikeObject();
                    jj.Add("minlen", 1);
                    if (item.LayerType == LayerType.Input || gg.LayerType == LayerType.Input || item.LayerType == LayerType.Output || gg.LayerType == LayerType.Output)
                    {
                        jj["minlen"] = 3;
                    }
                    jj.Add("weight", 1);
                    jj.Add("width", 0);
                    jj.Add("height", 0);
                    jj.Add("labeloffset", 10);
                    jj.Add("labelpos", "r");
                    jj.Add("source", "r");
                    dg.setEdgeRaw(new object[] { ind + "", list1.IndexOf(item) + "", jj });
                }
            }
            dg.graph()["ranksep"] = 20;
            dg.graph()["edgesep"] = 20;
            dg.graph()["nodesep"] = 25;
            dg.graph()["rankdir"] = "tb";
            dl.runLayout(dg);

            //back
            for (int i = 0; i < model.Nodes.Length; i++)
            {
                var node = dg.node(i + "");
                var n = model.Nodes[i];
                dynamic xx = node["x"];
                dynamic yy = node["y"];
                dynamic ww = node["width"];
                dynamic hh = node["height"];
                var tag = (n.DrawTag as GraphNodeDrawInfo);
                tag.X = (float)xx - (float)ww / 2;
                tag.Y = (float)yy - (float)hh / 2;
                //tag.X = (float)xx;
                //tag.Y = (float)yy;
            }

            List<EdgeNode> enodes = new List<EdgeNode>();
            foreach (var item in dg.edges())
            {

                var edge = dg.edge(item);
                var src = edge["source"];
                dynamic pnts = edge["points"];
                List<PointF> rr = new List<PointF>();
                foreach (dynamic itemz in pnts)
                {
                    rr.Add(new PointF((float)itemz["x"], (float)itemz["y"]));
                }

                enodes.Add(new EdgeNode(rr.ToArray()));
            }
            model.Edges = enodes.ToArray();
        }
    }
}
