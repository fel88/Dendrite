using Dendrite.Dagre;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System;

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
        public static void Test3()
        {
            DagreGraph dg = DagreGraph.FromJson(ReadResourceTxt("outputLayoutGraph1.txt"));
            Bitmap bmp = new Bitmap(1000, 3000);
            Graphics gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            gr.SmoothingMode = SmoothingMode.HighQuality;
            var convColor = new SolidBrush(Color.FromArgb(51, 85, 136));
            var reluColor = new SolidBrush(Color.FromArgb(112, 41, 33));
            var poolColor = new SolidBrush(Color.FromArgb(51, 85, 51));
            var concatColor = new SolidBrush(Color.FromArgb(89, 66, 59));
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
                if (hh > 50)
                {
                    using (GraphicsPath path = RoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, hh), cornerRadius))
                    {
                        gr.DrawPath(Pens.Black, path);

                    }
                    using (GraphicsPath path = HalfRoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, 22), cornerRadius))
                    {
                        gr.FillPath(convColor, path);
                        gr.DrawPath(Pens.Black, path);
                    }

                }
                else
                {
                    using (GraphicsPath path = RoundedRect(new RectangleF(xx - ww / 2, yy - hh / 2, ww, hh), cornerRadius))
                    {
                        gr.FillPath(poolColor, path);
                        gr.DrawPath(Pens.Black, path);
                    }

                }

            }
            Clipboard.SetImage(bmp);
        }
        public static GraphicsPath RoundedRect(RectangleF bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
        public static GraphicsPath HalfRoundedRect(RectangleF bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom   
            arc.Y = bounds.Bottom - diameter;
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);


            path.CloseFigure();
            return path;
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
            DagreLayout.positionSelfEdges(dg);
            DagreLayout.removeBorderNodes(dg);

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
