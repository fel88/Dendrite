using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System;
using Dagre;

namespace Dendrite
{
    public static class DagreTester
    {
        public static string ReadResourceTxt(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public class moveContext
        {
            float _x0;
            float _x1;
            float _y0;
            float _y1;
            public GraphicsPath Path;

            public void bezierCurveTo(PointF cp1, PointF cp2, PointF cp3)
            {
                Path.AddBezier(new PointF(_x1, _y1), cp1, cp2, cp3);
                _x1 = cp3.X;
                _y1 = cp3.Y;
            }
            public void moveTo(float x, float y)
            {
                this._x0 = x;
                this._x1 = x;
                this._y0 = y;
                this._y1 = y;
            }

            internal void lineTo(float x, float y)
            {
                Path.AddLine(new PointF(_x1, _y1), new PointF(x, y));
                _x1 = x;
                _y1 = y;

            }
        }
        public class Curve
        {
            moveContext context = new moveContext();
            public GraphicsPath Path = new GraphicsPath();
            float _x0;
            float _x1;
            float _y0;
            float _y1;


            int _point;
            int _line;
            public void LineStart()
            {
                context.Path = Path;
                this._x0 = float.NaN;
                this._x1 = float.NaN;
                this._y0 = float.NaN;
                this._y1 = float.NaN;
                this._point = 0;

            }

            void curve(float x, float y)
            {
                PointF p0 = new PointF(_x0, _y0);
                PointF p1 = new PointF(_x1, _y1);

                PointF cp1 = new PointF((2 * p0.X + p1.X) / 3, (2 * p0.Y + p1.Y) / 3);
                PointF cp2 = new PointF((p0.X + 2 * p1.X) / 3, (p0.Y + 2 * p1.Y) / 3);
                PointF cp3 = new PointF((p0.X + 4 * p1.X + x) / 6, (p0.Y + 4 * p1.Y + y) / 6);

                context.bezierCurveTo(cp1, cp2, cp3);
            }
            public void lineEnd()
            {
                switch (this._point)
                {
                    case 3:
                        this.curve(this._x1, this._y1);
                        context.lineTo(this._x1, this._y1);


                        break;
                    case 2:
                        context.lineTo(this._x1, this._y1);
                        break;
                }
                if (this._line != 0 || (this._line != 0 && this._point == 1))
                {
                    //Path.CloseFigure();
                    //this._context.closePath();
                }
                this._line = 1 - this._line;
            }
            public void point(float x, float y)
            {
                x = +x;
                y = +y;
                switch (this._point)
                {
                    case 0:
                        this._point = 1;
                        if (this._line != 0)
                        {
                            context.lineTo(x, y);
                        }
                        else
                        {
                            context.moveTo(x, y);
                        }
                        break;
                    case 1:
                        this._point = 2;
                        break;
                    case 2:
                        this._point = 3;
                        context.lineTo((5 * this._x0 + this._x1) / 6, (5 * this._y0 + this._y1) / 6);
                        this.curve(x, y);
                        break;
                    default:
                        this.curve(x, y);
                        break;
                }
                this._x0 = this._x1;
                this._x1 = x;
                this._y0 = this._y1;
                this._y1 = y;
            }

        }
        public static Bitmap DrawGraph(DagreGraph dg)
        {
            Bitmap bmp = new Bitmap(1000, 3000);
            Graphics gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            gr.SmoothingMode = SmoothingMode.HighQuality;
            var convColor = new SolidBrush(Color.FromArgb(51, 85, 136));
            var reluColor = new SolidBrush(Color.FromArgb(112, 41, 33));
            var poolColor = new SolidBrush(Color.FromArgb(51, 85, 51));
            var mathColor = Brushes.Black;
            var concatColor = new SolidBrush(Color.FromArgb(89, 66, 59));
            
            var inputColor = new SolidBrush(Color.FromArgb(238, 238, 238));
            foreach (dynamic d in dg._edgeLabels)
            {
                dynamic pnts = d.Value["points"];
                List<PointF> rr = new List<PointF>();
                foreach (dynamic item in pnts)
                {
                    rr.Add(new PointF((float)item["x"], (float)item["y"]));
                }
                AdjustableArrowCap bigArrow = new AdjustableArrowCap(3, 3);
                Pen pen1 = new Pen(Color.Black);
                pen1.CustomEndCap = bigArrow;
                //gr.DrawLines(pen1, rr.ToArray());
                var curve = new Curve();
                for (var i = 0; i < rr.Count; i++)
                {
                    if (i == 0)
                    {
                        curve.LineStart();
                    }
                    curve.point(rr[i].X, rr[i].Y);
                    if (i == rr.Count - 1)
                    {
                        curve.lineEnd();
                    }
                }
                gr.DrawPath(pen1, curve.Path);

            }
            foreach (dynamic d in dg._nodesRaw)
            {
                var xx = (float)d.Value["x"];
                var yy = (float)d.Value["y"];
                var ww = (float)d.Value["width"];
                var hh = (float)d.Value["height"];
                var cornerRadius = 8;
                if (d.Value.ContainsKey("source"))
                {
                    GraphNode source = d.Value["source"];
                    Brush br = convColor;
                    Brush fr = Brushes.White;
                    bool withHeader = false;
                    if (source.LayerType == LayerType.Concat)
                    {
                        br = concatColor;
                    }
                    if (source.LayerType == LayerType.Conv)
                    {
                        withHeader = true;
                        br = convColor;
                    }
                    if (source.LayerType == LayerType.Pool)
                    {
                        br = poolColor;
                    }
                    if (source.LayerType == LayerType.MathOperation)
                    {
                        withHeader = true;
                        br = mathColor;
                    }
                    if (source.LayerType == LayerType.Softmax)
                    {
                        br = reluColor;
                    }
                    if (source.LayerType == LayerType.Transpose)
                    {
                        br = poolColor;
                    }
                    if (source.LayerType == LayerType.Pad)
                    {
                        br = concatColor;
                    }
                    if (source.LayerType == LayerType.Relu)
                    {
                        br = reluColor;
                    }
                    if (source.LayerType == LayerType.Output || source.LayerType == LayerType.Input)
                    {
                        br = inputColor;
                        fr = Brushes.Black;
                    }
                    if (withHeader)
                    {
                        using (GraphicsPath path = Helpers.RoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, hh), cornerRadius))
                        {
                            gr.DrawPath(Pens.Black, path);

                        }
                        using (GraphicsPath path = Helpers.HalfRoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, 22), cornerRadius))
                        {
                            gr.FillPath(br, path);
                            gr.DrawPath(Pens.Black, path);
                        }
                    }
                    else
                    {
                        using (GraphicsPath path = Helpers.RoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, hh), cornerRadius))
                        {
                            gr.FillPath(br, path);
                            gr.DrawPath(Pens.Black, path);
                        }
                    }
                    int gap = 5;
                    gr.DrawString(source.Name, SystemFonts.DefaultFont, fr, new RectangleF(xx - ww / 2 + gap, yy - hh / 2 + gap, ww - gap * 2, hh - gap * 2));
                }
                else
                {
                    if (hh > 50)
                    {
                        using (GraphicsPath path = Helpers.RoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, hh), cornerRadius))
                        {
                            gr.DrawPath(Pens.Black, path);

                        }
                        using (GraphicsPath path = Helpers.HalfRoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, 22), cornerRadius))
                        {
                            gr.FillPath(convColor, path);
                            gr.DrawPath(Pens.Black, path);
                        }

                    }
                    else
                    {
                        using (GraphicsPath path = Helpers.RoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, hh), cornerRadius))
                        {
                            gr.FillPath(poolColor, path);
                            gr.DrawPath(Pens.Black, path);
                        }

                    }
                }

            }
            return bmp;
        }

        public static void Test3()
        {
            DagreGraph dg = DagreGraph.FromJson(ReadResourceTxt("outputLayoutGraph1.txt"));
            var bmp = DrawGraph(dg);
            Clipboard.SetImage(bmp);
        }

       

        public static void Test5()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "onnx|*.onnx";

            if (ofd.ShowDialog() != DialogResult.OK) return;
            DagreGraph dg1 = DagreGraph.FromJson(ReadResourceTxt("json.txt"));

            DagreLayout dl = new DagreLayout();
            var p = new OnnxModelProvider();
            var g = p.LoadFromFile(ofd.FileName);
            DagreGraph dg = new DagreGraph(true);
            var list1 = g.Nodes.ToList();
            foreach (var gg in list1)
            {
                var ind = list1.IndexOf(gg);
                dg.setNodeRaw(ind + "", new JavaScriptLikeObject());
                var nd = dg.node(ind + "");
                nd["width"] = 100;
                nd["height"] = 50;
                nd["source"] = gg;
                if (gg.LayerType == LayerType.Relu)
                {
                    nd["width"] = 50;
                    nd["height"] = 25;

                }
                if (gg.LayerType == LayerType.Input || gg.LayerType == LayerType.Output )
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

                }

            }
            foreach (var gg in list1)
            {
                var ind = list1.IndexOf(gg);

                foreach (var item in gg.Childs)
                {
                    JavaScriptLikeObject jj = new JavaScriptLikeObject();
                    jj.Add("minlen", 1);
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
            var bmp = DrawGraph(dg);
            Clipboard.SetImage(bmp);
        }
        public static void Test1()
        {
            var dl = new DagreLayout();
            DagreGraph dg = DagreGraph.FromJson(ReadResourceTxt("json.txt"));

            dl.makeSpaceForEdgeLabels(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterMakeSpaceForEdgeLabels.txt")).Compare(dg)) throw new DagreException("wrong");
            dl.removeSelfEdges(dg);
            acyclic.run(dg);

            if (!DagreGraph.FromJson(ReadResourceTxt("beforeNestingRun.txt")).Compare(dg)) throw new DagreException("wrong");

            nestingGraph.run(dg);

            if (!DagreGraph.FromJson(ReadResourceTxt("beforeAsNoneCompoundGraph.txt")).Compare(dg)) throw new DagreException("wrong");

            var ncg = util.asNonCompoundGraph(dg);


            if (!DagreGraph.FromJson(ReadResourceTxt("beforeRank.txt")).Compare(ncg)) throw new DagreException("wrong");
            dl.rank(ncg);



            if (!DagreGraph.FromJson(ReadResourceTxt("beforeInjectEdgeLabelProxies.txt")).Compare(dg)) throw new DagreException("wrong");
            DagreLayout.injectEdgeLabelProxies(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeRemoveEmptyRanks.txt")).Compare(dg)) throw new DagreException("wrong");
            DagreLayout.removeEmptyRanks(dg);
            nestingGraph.cleanup(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterCleanup.txt")).Compare(dg)) throw new DagreException("wrong");

            util.normalizeRanks(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterNormalizeRanks.txt")).Compare(dg)) throw new DagreException("wrong");

            dl.assignRankMinMax(dg);

            dl.removeEdgeLabelProxies(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeNormalize.txt")).Compare(dg)) throw new DagreException("wrong");

            normalize.run(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterNormalize.txt")).Compare(dg)) throw new DagreException("wrong");

            parentDummyChains._parentDummyChains(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterParentDummies.txt")).Compare(dg)) throw new DagreException("wrong");

            addBorderSegments._addBorderSegments(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterAddBorderSegments.txt")).Compare(dg)) throw new DagreException("wrong");

            order._order(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterOrder.txt")).Compare(dg)) throw new DagreException("wrong");

            dl.insertSelfEdges(dg);

            coordinateSystem.adjust(dg);
            DagreLayout.position(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterPosition.txt")).Compare(dg)) throw new DagreException("wrong");

            DagreLayout.positionSelfEdges(dg);
            DagreLayout.removeBorderNodes(dg);

            if (!DagreGraph.FromJson(ReadResourceTxt("beforeDenormalize.txt")).Compare(dg)) throw new DagreException("wrong");

            normalize.undo(dg);

            DagreLayout.fixupEdgeLabelCoords(dg);
            coordinateSystem.undo(dg);
            DagreLayout.translateGraph(dg);
            DagreLayout.assignNodeIntersects(dg);
            DagreLayout.reversePointsForReversedEdges(dg);
            acyclic.undo(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterAcyclicUndo.txt")).Compare(dg)) throw new DagreException("wrong");

            var bmp = DrawGraph(dg);
            Clipboard.SetImage(bmp);
        }

        public static void Test4()
        {
            var dg = DagreGraph.FromJson(ReadResourceTxt("beforePosition.txt"));
            util.uniqueCounter = 98;
            DagreLayout.position(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterPosition.txt")).Compare(dg)) throw new DagreException("wrong");

            DagreLayout.positionSelfEdges(dg);

            DagreLayout.removeBorderNodes(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeDenormalize.txt")).Compare(dg)) throw new DagreException("wrong");

            normalize.undo(dg);

            DagreLayout.fixupEdgeLabelCoords(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterFixupEdgeLabels.txt")).Compare(dg)) throw new DagreException("wrong");

            coordinateSystem.undo(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeTranslateGraph.txt")).Compare(dg)) throw new DagreException("wrong");

            DagreLayout.translateGraph(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterTranslateGraph.txt")).Compare(dg)) throw new DagreException("wrong");

            DagreLayout.assignNodeIntersects(dg);
            DagreLayout.reversePointsForReversedEdges(dg);
            acyclic.undo(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterAcyclicUndo.txt")).Compare(dg)) throw new DagreException("wrong");

            var bmp = DrawGraph(dg);
            Clipboard.SetImage(bmp);

        }

        public static void Test2()
        {
            var dl = new DagreLayout();
            var dg = DagreGraph.FromJson(ReadResourceTxt("beforeAsNoneCompoundGraph.txt"));
            //DagreGraph ncg = DagreGraph.FromJson(ReadResourceTxt("beforeRank.txt"));
            var ncg = util.asNonCompoundGraph(dg);

            dl.rank(ncg);

            if (!DagreGraph.FromJson(ReadResourceTxt("beforeInjectEdgeLabelProxies.txt")).Compare(dg)) throw new DagreException("wrong");
            util.uniqueCounter = 1;
            DagreLayout.injectEdgeLabelProxies(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeRemoveEmptyRanks.txt")).Compare(dg)) throw new DagreException("wrong");
            DagreLayout.removeEmptyRanks(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterRemoveEmptyRanks.txt")).Compare(dg)) throw new DagreException("wrong");

            nestingGraph.cleanup(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterCleanup.txt")).Compare(dg)) throw new DagreException("wrong");

            util.normalizeRanks(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterNormalizeRanks.txt")).Compare(dg)) throw new DagreException("wrong");

            dl.assignRankMinMax(dg);

            dl.removeEdgeLabelProxies(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeNormalize.txt")).Compare(dg)) throw new DagreException("wrong");

            normalize.run(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterNormalize.txt")).Compare(dg)) throw new DagreException("wrong");

            parentDummyChains._parentDummyChains(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterParentDummies.txt")).Compare(dg)) throw new DagreException("wrong");

            addBorderSegments._addBorderSegments(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterAddBorderSegments.txt")).Compare(dg)) throw new DagreException("wrong");

            order._order(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("afterOrder.txt")).Compare(dg)) throw new DagreException("wrong");

            dl.insertSelfEdges(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforeCoordSystemAdjust.txt")).Compare(dg)) throw new DagreException("wrong");

            coordinateSystem.adjust(dg);
            if (!DagreGraph.FromJson(ReadResourceTxt("beforePosition.txt")).Compare(dg)) throw new DagreException("wrong");

            DagreLayout.position(dg);

            DagreLayout.positionSelfEdges(dg);

            DagreLayout.removeBorderNodes(dg);

        }
    }
}
