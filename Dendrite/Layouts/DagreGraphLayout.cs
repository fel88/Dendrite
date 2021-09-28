using Dagre;
using System;
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
                        if (GetRenderTextWidth != null)
                        {
                            dd.Width = Math.Max(20 + GetRenderTextWidth(item), 220);
                        }

                        dd.Height = 110;
                        break;
                    case LayerType.Batch:
                        dd.Width = 250;
                        dd.Height = 170;
                        break;
                    case LayerType.PrimitiveMath:
                        if (GetRenderTextWidth != null)
                        {
                            dd.Width = 20 + GetRenderTextWidth(item);
                        }
                        else
                        {
                            dd.Width = 150;
                        }

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
                        if (GetRenderTextWidth != null)
                        {
                            dd.Width = 20 + GetRenderTextWidth(item);
                        }
                        else
                        {
                            dd.Width = 150;
                        }

                        dd.Height = 50;
                        break;
                    case LayerType.Dropout:
                    case LayerType.Concat:
                    case LayerType.Relu:
                    case LayerType.Pad:
                        if (GetRenderTextWidth != null)
                        {
                            dd.Width = 20 + GetRenderTextWidth(item);
                        }
                        else
                        {
                            dd.Width = 120;
                        }

                        dd.Height = 50;
                        break;
                    case LayerType.Pool:
                    case LayerType.Transpose:
                    case LayerType.Softmax:
                    case LayerType.Log:
                        if (GetRenderTextWidth != null)
                        {
                            dd.Width = 20 + GetRenderTextWidth(item);
                        }
                        else
                        {
                            dd.Width = 150;
                        }
                        dd.Height = 50;
                        break;
                }
            }
        }


        public void ExperimentalGroupLayout(GraphModel model)
        {
            DagreInputGraph d = new DagreInputGraph();
            d.VerticalLayout = VerticalLayout;
            updateNodesSizes(model);

            model.Nodes = model.Nodes.Where(z => z.LayerType != LayerType.Constant && (z.Childs.Any() || z.Parent != null || z.Parents.Any())).ToArray();
            model.Groups.Clear();
            var list1 = model.Nodes.ToList();

            foreach (var rgrp in RequestedGroups)
            {


                var group1 = model.Nodes.Where(z => z.Name.StartsWith(rgrp.Prefix)).ToArray();
                //replace group with big rectangle here?


                list1 = list1.Except(group1).ToList();
                var gnode = new GroupNode()
                {
                    Prefix = rgrp.Prefix,
                    Name = "group" + (1 + RequestedGroups.IndexOf(rgrp)),
                    DrawTag = new GraphNodeDrawInfo() { Width = 800, Height = 800 },
                    Nodes = group1.ToArray()
                };

                model.Groups.Add(gnode);
                list1.Add(gnode);

                foreach (var gg in list1)
                {
                    var tag = (gg.DrawTag as GraphNodeDrawInfo);
                    d.AddNode(gg, tag.Rect.Width, tag.Rect.Height);
                }
                foreach (var gg in list1)
                {
                    bool add = false;
                    foreach (var item in gg.Childs)
                    {
                        if (group1.Contains(item))
                        {
                            add = true;
                            break;
                        }
                    }

                    if (add)
                    {
                        gg.Childs.Add(gnode);
                        gnode.Parents.Add(gg);
                        gg.Childs.RemoveAll(z => group1.Contains(z));
                    }
                }
                foreach (var gg in list1)
                {
                    bool add = false;

                    foreach (var item in gg.Parents)
                    {
                        if (group1.Contains(item))
                        {
                            add = true;
                            break;
                        }
                    }
                    if (add)
                    {
                        gg.Parents.Add(gnode);
                        gg.Parents.RemoveAll(z => group1.Contains(z));
                    }
                }
                foreach (var gg in group1)
                {

                    foreach (var item in gg.Childs)
                    {
                        if (!group1.Contains(item))
                        {
                            gnode.Childs.Add(item);
                        }
                    }

                }
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
            foreach (var n in model.Nodes.Union(model.Groups))
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
        public override void Layout(GraphModel model)
        {
            if (RequestedGroups.Any())
            {
                ExperimentalGroupLayout(model);
                return;
            }
            DagreInputGraph d = new DagreInputGraph();
            d.VerticalLayout = VerticalLayout;
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
