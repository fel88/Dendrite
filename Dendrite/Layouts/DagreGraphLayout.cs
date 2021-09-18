﻿using Dagre;
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
                    case LayerType.Conv:
                        dd.Width = 200;
                        dd.Height = 110;
                        break;
                    case LayerType.MathOperation:
                        dd.Width = 180;
                        dd.Height = 90;
                        break;
                    case LayerType.Output:
                    case LayerType.Input:
                        dd.Width = 150;
                        dd.Height = 50;
                        break;                    
                    case LayerType.Relu:                  
                    case LayerType.Pad:
                        dd.Width = 120;
                        dd.Height = 50;
                        break;
                    case LayerType.Pool:
                    case LayerType.Transpose:
                    case LayerType.Softmax:
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

            var list1 = model.Nodes.ToList();
            foreach (var gg in list1)
            {
                var ind = list1.IndexOf(gg);
                dg.setNodeRaw(ind + "", new JavaScriptLikeObject());
                var nd = dg.node(ind + "");
                nd["width"] = 100;
                nd["height"] = 50;
                nd["source"] = gg;
                /*if (gg.LayerType == LayerType.Relu)
                {
                    nd["width"] = 50;
                    nd["height"] = 25;

                }
                if (gg.LayerType == LayerType.Input || gg.LayerType == LayerType.Output)
                {
                    nd["width"] = 40;
                    nd["height"] = 25;

                }
                if (gg.LayerType == LayerType.Pad)
                {
                    nd["width"] = 40;
                    nd["height"] = 25;

                }
                if (gg.LayerType == LayerType.Transpose)
                {
                    nd["width"] = 70;
                    nd["height"] = 25;

                }
                if (gg.LayerType == LayerType.Softmax)
                {
                    nd["width"] = 70;
                    nd["height"] = 25;

                }
                if (gg.LayerType == LayerType.Pool)
                {
                    nd["width"] = 70;
                    nd["height"] = 25;

                }*/
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
        }
    }
}
