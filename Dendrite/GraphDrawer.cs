using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Dendrite
{
    public static class GraphDrawer
    {
        public static void DrawNodes(IDrawingContext ctx, GraphModel model, GraphLayout currentLayout, Brush textBrush, Font f, GraphNode selected, GraphNode hovered,
            bool ShowFullNames)
        {
            var list = model.Nodes.ToList();
            if (model.Groups.Any())
            {
                list = list.Except(model.Groups.SelectMany(z => z.Nodes)).ToList();
                list.AddRange(model.Groups);
            }
            foreach (var item in list)
            //foreach (var item in model.Nodes)
            {

                var dtag = item.DrawTag as GraphNodeDrawInfo;
                if (dtag == null) continue;
                var rr = ctx.Transform(dtag.Rect);
                var rr2 = ctx.Transform(new RectangleF(dtag.Rect.Left, dtag.Rect.Top, dtag.Rect.Width, dtag.Rect.Height));

                textBrush = Brushes.Black;

                int cornerRadius = (int)(15 * ctx.Zoom);
                Brush brush = Brushes.LightGray;
                textBrush = Brushes.White;
                switch (item.LayerType)
                {
                    case LayerType.Gemm:
                    case LayerType.Conv:
                    case LayerType.Lstm:
                        brush = StaticColors.ConvBrush;
                        break;
                    case LayerType.Dropout:
                        brush = StaticColors.DropoutBrush;
                        break;
                    case LayerType.Gather:
                    case LayerType.Squeeze:
                    case LayerType.Batch:
                        brush = StaticColors.BatchNormBrush;
                        break;
                    case LayerType.Log:
                    case LayerType.Softmax:
                    case LayerType.Relu:
                        brush = StaticColors.ReluBrush;
                        break;
                    case LayerType.PrimitiveMath:
                    case LayerType.MathOperation:
                        brush = StaticColors.MathBrush;
                        break;
                    case LayerType.Transpose:
                    case LayerType.Pool:
                        brush = StaticColors.PoolBrush;
                        break;
                    case LayerType.Input:
                    case LayerType.Output:
                        brush = StaticColors.EndpointBrush;
                        textBrush = Brushes.Black;
                        break;
                    case LayerType.Pad:
                    case LayerType.Concat:
                        brush = StaticColors.ConcatBrush;
                        break;
                }
                var borderPen = Pens.Black;
                if (item is GroupNode)
                {
                    brush = StaticColors.GroupBrush;
                    borderPen = new Pen(Color.Black, 3);
                    textBrush = Brushes.Black;
                }
                if (item == hovered)
                {
                    textBrush = Brushes.Black;
                    if (currentLayout.DrawHeadersAllowed && item.DrawHeader)
                    {
                        RectangleF headerRect = new RectangleF(rr2.Left, rr2.Top, rr2.Width, item.HeaderHeight * ctx.Zoom);
                        ctx.Graphics.FillPath(Brushes.White, Helpers.RoundedRect(rr2, cornerRadius));
                        ctx.Graphics.FillPath(Brushes.White, Helpers.HalfRoundedRect(headerRect, cornerRadius));
                        ctx.Graphics.DrawPath(Pens.Black, Helpers.HalfRoundedRect(headerRect, cornerRadius));

                    }
                    else
                    {
                        FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.Zoom), Brushes.LightYellow);

                    }
                }
                else if (currentLayout.FlashHoveredRelatives && item.Parents.Contains(hovered))
                {
                    FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.Zoom), Brushes.LightPink);
                }
                else if (currentLayout.FlashHoveredRelatives && item.Childs.Contains(hovered))
                {
                    FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.Zoom), Brushes.LightBlue);
                }
                else
                if (item == selected)
                {
                    textBrush = Brushes.Black;

                    if (currentLayout.DrawHeadersAllowed && item.DrawHeader)
                    {
                        RectangleF headerRect = new RectangleF(rr2.Left, rr2.Top, rr2.Width, item.HeaderHeight * ctx.Zoom);
                        ctx.Graphics.FillPath(Brushes.White, Helpers.RoundedRect(rr2, cornerRadius));
                        ctx.Graphics.FillPath(Brushes.LightGreen, Helpers.HalfRoundedRect(headerRect, cornerRadius));
                        ctx.Graphics.DrawPath(Pens.Black, Helpers.HalfRoundedRect(headerRect, cornerRadius));
                    }
                    else
                    {
                        FillRoundedRectangle(ctx.Graphics, rr2, (int)(40 * ctx.Zoom), Brushes.LightGreen);
                    }
                }
                else
                {
                    if (currentLayout.DrawHeadersAllowed && item.DrawHeader)
                    {
                        RectangleF headerRect = new RectangleF(rr2.Left, rr2.Top, rr2.Width, item.HeaderHeight * ctx.Zoom);
                        ctx.Graphics.FillPath(Brushes.White, Helpers.RoundedRect(rr2, cornerRadius));

                        ctx.Graphics.FillPath(brush, Helpers.HalfRoundedRect(headerRect, cornerRadius));
                        ctx.Graphics.DrawPath(Pens.Black, Helpers.HalfRoundedRect(headerRect, cornerRadius));

                    }
                    else
                    {
                        ctx.Graphics.FillPath(brush, Helpers.RoundedRect(rr2, cornerRadius));

                        //FillRoundedRectangle(ctx.Graphics, rr2, cornerRadius, brush);
                    }
                }
                ctx.Graphics.DrawPath(borderPen, Helpers.RoundedRect(rr2, cornerRadius));

                //DrawRoundedRectangle(ctx.Graphics, rr, (int)(40 * ctx.zoom), Pens.Black);


                ctx.Graphics.ResetTransform();
                var sh = ctx.Transform(dtag.Rect.Left, dtag.Rect.Top + 10);
                ctx.Graphics.TranslateTransform(sh.X, sh.Y);
                ctx.Graphics.ScaleTransform(ctx.Zoom, ctx.Zoom);
                //ctx.Graphics.DrawString($"{item.Name}: ({item.OpType})", f, Brushes.Black, 0, 0);
                if (ShowFullNames || item.LayerType == LayerType.Input || item.LayerType == LayerType.Output)
                {
                    var ms = ctx.Graphics.MeasureString($"{item.Name}:{item.OpType}", f);
                    ctx.Graphics.DrawString($"{item.Name}:{item.OpType}", f, textBrush, +dtag.Rect.Width / 2 - ms.Width / 2, 0);
                }
                else
                {
                    var ms = ctx.Graphics.MeasureString($"{item.OpType}", f);
                    ctx.Graphics.DrawString($"{item.OpType}", f, textBrush, +dtag.Rect.Width / 2 - ms.Width / 2, 0);
                }
                if (item is GroupNode gn)
                {
                    var ms = ctx.Graphics.MeasureString($"Group: {gn.Prefix}", f);
                    ctx.Graphics.DrawString($"Group: {gn.Prefix}", f, textBrush, +dtag.Rect.Width / 2 - ms.Width / 2, 0);
                    /*ctx.Graphics.DrawPath(new Pen(Color.Blue, 5), Helpers.RoundedRect(new RectangleF(10, 0, 60, 60), (int)(cornerRadius / ctx.Zoom)));
                    ctx.Graphics.DrawLine(new Pen(Color.Blue, 5), 50, 10, 50, 50);
                    ctx.Graphics.DrawLine(new Pen(Color.Blue, 5), 20, 50, 50, 50);*/
                }
                if (currentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Conv)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {

                        ctx.Graphics.DrawString("W:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 45, 35);
                    }
                    if (item.Data.Count > 1)
                    {
                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 65);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[1].Dims)})", f, Brushes.Black, 45, 65);
                    }
                }
                if (currentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Gather)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Attributes.Count > 0)
                    {
                        ctx.Graphics.DrawString("Indicies = ", f, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Attributes[0].IntData)})", f, Brushes.Black, 120, 35);
                    }

                }
                if (currentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Gemm)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {

                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 45, 35);
                    }
                    if (item.Data.Count > 1)
                    {
                        ctx.Graphics.DrawString("C:", fb, Brushes.Black, 5, 65);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[1].Dims)})", f, Brushes.Black, 45, 65);
                    }
                }
                if (currentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Lstm)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {

                        ctx.Graphics.DrawString("W:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 45, 35);
                    }
                    if (item.Data.Count > 1)
                    {
                        ctx.Graphics.DrawString("R:", fb, Brushes.Black, 5, 65);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[1].Dims)})", f, Brushes.Black, 45, 65);
                    }
                    if (item.Data.Count > 2)
                    {
                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 95);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[2].Dims)})", f, Brushes.Black, 45, 95);
                    }
                }
                if (currentLayout.DrawHeadersAllowed && item.LayerType == LayerType.Batch)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {

                        ctx.Graphics.DrawString("scale:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 85, 35);
                    }
                    if (item.Data.Count > 1)
                    {
                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 65);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[1].Dims)})", f, Brushes.Black, 85, 65);
                    }
                    if (item.Data.Count > 2)
                    {
                        ctx.Graphics.DrawString("mean:", fb, Brushes.Black, 5, 95);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[2].Dims)})", f, Brushes.Black, 85, 95);
                    }
                    if (item.Data.Count > 3)
                    {
                        ctx.Graphics.DrawString("var:", fb, Brushes.Black, 5, 125);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[3].Dims)})", f, Brushes.Black, 85, 125);
                    }
                }
                if (currentLayout.DrawHeadersAllowed && item.LayerType == LayerType.MathOperation)
                {
                    var fb = new Font(f.FontFamily, f.Size, FontStyle.Bold);
                    if (item.Data.Count > 0)
                    {
                        ctx.Graphics.DrawString("B:", fb, Brushes.Black, 5, 35);
                        ctx.Graphics.DrawString($"({string.Join("x", item.Data[0].Dims)})", f, Brushes.Black, 45, 35);
                    }

                }
                var ms2 = ctx.Graphics.MeasureString(item.OpType, f);
                //ctx.Graphics.DrawString(item.OpType, f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 30);
                if (currentLayout.FlashHoveredRelatives)
                {
                    if (item.Parents.Contains(hovered))
                    {
                        ctx.Graphics.DrawString("child", f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 60);
                    }
                    if (item.Childs.Contains(hovered))
                    {
                        ctx.Graphics.DrawString("parent", f, textBrush, +dtag.Rect.Width / 2 - ms2.Width / 2, 60);
                    }
                }


                ctx.Graphics.ResetTransform();
            }
        }
        public static void DrawLabels(IDrawingContext ctx, GraphModel model, Font f2, Brush textBrush)
        {
            foreach (var item in model.Nodes)
            {

                var dtag = item.DrawTag as GraphNodeDrawInfo;
                if (dtag == null) continue;


                if (item.Shape != null)
                {
                    ctx.Graphics.ResetTransform();
                    var sh = ctx.Transform(dtag.Rect.Left, dtag.Rect.Bottom + 10);
                    ctx.Graphics.TranslateTransform(sh.X, sh.Y);
                    ctx.Graphics.ScaleTransform(ctx.Zoom, ctx.Zoom);

                    var s1 = string.Join("x", item.Shape);


                    ctx.Graphics.DrawString(s1, f2, textBrush, +dtag.Rect.Width / 2 + 10, 0);
                    ctx.Graphics.ResetTransform();
                }
            }
        }
        public static void DrawRoundedRectangle(Graphics g,
                              RectangleF r, int d, Pen pen)
        {
            g.DrawPath(pen, GetRoundedRectangle(r, d));
        }
        public static GraphicsPath GetRoundedRectangle(RectangleF r, int d)
        {
            if (d == 0) { d = 1; }
            GraphicsPath gp = new GraphicsPath();

            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d,
                                                             0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);

            return gp;
        }

        public static void FillRoundedRectangle(Graphics g,
                                RectangleF r, int d, Brush myBrush)
        {
            g.FillPath(myBrush, GetRoundedRectangle(r, d));
        }
        public static void DrawEdges(IDrawingContext ctx, GraphModel model, GraphLayout currentLayout)
        {
            if (model.Edges != null && currentLayout.EdgesDrawAllowed)
                foreach (var item in model.Edges)
                {
                    item.Draw(ctx);
                }
            else
                foreach (var item in model.Nodes)
                {

                    var dtag = item.DrawTag as GraphNodeDrawInfo;
                    if (dtag == null) continue;
                    foreach (var citem in item.Childs)
                    {
                        var dtag2 = citem.DrawTag as GraphNodeDrawInfo;
                        if (dtag2 == null) continue;
                        var size = 6 * ctx.Zoom;
                        AdjustableArrowCap bigArrow = new AdjustableArrowCap(size, size, true);
                        Pen pen1 = new Pen(Color.Black);
                        pen1.CustomEndCap = bigArrow;

                        ctx.Graphics.DrawLine(pen1,
                            ctx.Transform(dtag.Rect.Location.X + dtag.Rect.Size.Width / 2, dtag.Rect.Location.Y + dtag.Rect.Height / 2),
                            ctx.Transform(dtag2.Rect.Location.X + dtag2.Rect.Size.Width / 2, dtag2.Rect.Location.Y + dtag2.Rect.Height / 2)
                            );
                    }
                }
        }


        public static Bitmap ExportImage(GraphModel model, Brush textBrush, Font f, Font f2, bool showFullNames = false, int exportImageMaxDim = 4000)
        {
            var layout = new Layouts.DagreGraphLayout();
            BitmapDrawingContext ectx = new BitmapDrawingContext();
            var ar = model.Nodes.Where(z => z.DrawTag != null).Select(z => z.DrawTag as GraphNodeDrawInfo).ToArray();
            var minx = ar.Min(z => z.X);
            var miny = ar.Min(z => z.Y);
            var maxx = ar.Max(z => z.X);
            var maxy = ar.Max(z => z.Y);
            var ww = (int)Math.Ceiling(maxx - minx);
            var hh = (int)Math.Ceiling(maxy - miny);
            var maxDim = Math.Max(ww, hh);
            Bitmap bmp = null;
            if (maxDim > exportImageMaxDim)
            {
                ectx.Zoom = (float)exportImageMaxDim / maxDim;
                bmp = new Bitmap((int)(ww * ectx.Zoom), (int)(hh * ectx.Zoom));
            }
            else
            {
                bmp = new Bitmap(ww, hh);
            }

            ectx.Bmp = bmp;
            //fit all
            List<PointF> pp = new List<PointF>();
            foreach (var item in model.Nodes)
            {
                var dtag = item.DrawTag as GraphNodeDrawInfo;
                pp.Add(dtag.Rect.Location);
                pp.Add(new PointF(dtag.Rect.Right, dtag.Rect.Bottom));
            }

            ectx.FitToPoints(pp.ToArray(), 5);
            ectx.Graphics = Graphics.FromImage(bmp);
            ectx.Graphics.Clear(Color.White);
            ectx.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            DrawEdges(ectx, model, layout);
            DrawLabels(ectx, model, f2, textBrush);
            DrawNodes(ectx, model, layout, textBrush, f, null, null, showFullNames);

            return bmp;
        }
    }
}
