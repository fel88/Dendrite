using SkiaSharp.Views.Desktop;
using SkiaSharp;
using System.Drawing.Drawing2D;
using OpenTK;
using static System.Windows.Forms.AxHost;
using System.Drawing;
using System.IO;
using Microsoft.VisualBasic.Devices;
using OpenTK.Graphics.OpenGL;
using static System.Net.Mime.MediaTypeNames;

namespace Dendrite
{
    public class SkiaGLDrawingContext : IDrawingContext
    {
        public PathObject GetRoundedRectangle(RectangleF r, float d)
        {
            if (d == 0) { d = 1; }
            SKPath gp = new SKPath();

            gp.AddRoundRect(new SKRoundRect(r.ToSKRect(), d));

            return new SkiaPathObject() { Path = gp };
        }
        public PointF Shift { get; set; }

        public SkiaSharp.SKSurface Surface;
        public float sx { get; set; }
        public float sy { get; set; }
        public float zoom { get; set; } = 1;
        public PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, (p1.Y + sy) * zoom);
        }
        public PointF Transform(float x, float y)
        {
            return new PointF((x + sx) * zoom, (y + sy) * zoom);
        }
        public PointF Transform(double x, double y)
        {
            return new PointF((float)((x + sx) * zoom), (float)((y + sy) * zoom));
        }


        public PointF BackTransform(PointF p1)
        {
            var posx = (p1.X / zoom - sx);
            var posy = (-p1.Y / zoom - sy);
            return new PointF(posx, posy);
        }
        public virtual PointF BackTransform(float x, float y)
        {
            var posx = (x / zoom - sx);
            var posy = (-y / zoom - sy);
            return new PointF(posx, posy);
        }
        public void DrawLineTransformed(PointF point, PointF point2)
        {
            var canvas = Surface.Canvas;
            var pp = Transform(point);
            var pp2 = Transform(point2);
            DrawLine(pp, pp2);
        }


        public SizeF MeasureString(string text, Font font)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {

                paint.IsAntialias = true;
                //paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Fill;
                paint.TextSize = font.GetHeight();
                using (var font1 = new SKFont(SKTypeface.FromFamilyName(font.FontFamily.Name)))
                {
                    return new SizeF(paint.MeasureText(text), paint.TextSize);
                }
            }
        }

        public void DrawLine(PointF pp, PointF pp2)
        {
            var canvas = Surface.Canvas;
            canvas.DrawLine(pp.X, pp.Y, pp2.X, pp2.Y, CurrentPaint);
        }


        public void DrawLine(float x0, float y0, float x1, float y1)
        {
            DrawLine(new PointF(x0, y0), new PointF(x1, y1));
        }

        SKPaint CurrentPaint = new SKPaint();
        public void SetPen(System.Drawing.Pen pen)
        {
            CurrentPaint.Color = pen.Color.ToSKColor();
            CurrentPaint.IsAntialias = true;
            CurrentPaint.StrokeWidth = pen.Width;
            CurrentPaint.Style = SKPaintStyle.Stroke;
            CurrentPaint.PathEffect = null;
            if (pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid)
            {
                CurrentPaint.PathEffect = SKPathEffect.CreateDash(pen.DashPattern, pen.DashOffset);
            }
        }

        public void DrawRectangle(float rxm, float rym, float rdx, float rdy)
        {
            var canvas = Surface.Canvas;
            canvas.DrawRect(rxm, rym, rdx, rdy, CurrentPaint);
        }

        public void InitGraphics()
        {

        }

        SKCanvas Canvas => Surface == null ? null : Surface.Canvas;

        public void Clear(System.Drawing.Color white)
        {
            Canvas?.Clear(white.ToSKColor());
        }

        public static bool GlSupport = !System.Windows.Forms.SystemInformation.TerminalServerSession;
        public object GenerateRenderControl()
        {
            Control co = null;
            if (GlSupport)
            {
                co = new SKGLControl();
                ((SKGLControl)co).PaintSurface += Co_PaintSurface;
            }
            else
            {
                co = new SKControl();
                ((SKControl)co).PaintSurface += Co_PaintSurface1;
            }
            return co;
        }




        public Action Redraw { get; set; }
        public Action SwapAction { get; set; }

        private void Co_PaintSurface1(object sender, SKPaintSurfaceEventArgs e)
        {
            Surface = e.Surface;
            Redraw?.Invoke();
        }

        private void Co_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            Surface = e.Surface;
            Redraw?.Invoke();
        }

        public void DrawImage(Bitmap image, float x1, float y1, float x2, float y2)
        {
            var s = image.ToSKImage();
            var temp = CurrentPaint.FilterQuality;
            CurrentPaint.FilterQuality = SKFilterQuality.High;
            Canvas.DrawImage(s, new SKRect(x1, y1, x2, y2), CurrentPaint);
            CurrentPaint.FilterQuality = temp;

        }

        public void ResetMatrix()
        {
            Canvas.ResetMatrix();
        }

        public void RotateDegress(float deg)
        {
            Canvas.RotateDegrees(deg);
        }

        public void Translate(double x, double y)
        {
            Canvas.Translate((float)x, (float)y);
        }

        Stack<SKMatrix> stack = new Stack<SKMatrix>();
        public void PushMatrix()
        {
            stack.Push(Canvas.TotalMatrix);
        }

        public void PopMatrix()
        {
            Canvas.SetMatrix(stack.Pop());
        }

        public void Scale(double x, double y)
        {
            Canvas.Scale((float)x, (float)y);
        }

        public void DrawPath(System.Drawing.Pen pen, PathObject path)
        {
            var spo = path as SkiaPathObject;
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = (pen).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Stroke;
                canvas.DrawPath(spo.Path, paint);
            }
        }

        public void FillPath(System.Drawing.Brush p, PathObject path)
        {
            var spo = path as SkiaPathObject;
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = (p as SolidBrush).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                //paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawPath(spo.Path, paint);
            }
        }

        public Rectangle Transform(Rectangle rect)
        {
            throw new NotImplementedException();
        }

        public RectangleF Transform(RectangleF rect)
        {
            var l = Transform(rect.X, rect.Y);
            var r = Transform(rect.Right, rect.Bottom);
            return new RectangleF(l.X, l.Y, r.X - l.X, r.Y - l.Y);
        }

        public void FitToPoints(PointF[] points, int gap = 0)
        {
            var maxx = points.Max(z => z.X) + gap;
            var minx = points.Min(z => z.X) - gap;
            var maxy = points.Max(z => z.Y) + gap;
            var miny = points.Min(z => z.Y) - gap;

            var w = Box.Width;
            var h = Box.Height;

            double dx = maxx - minx;
            var kx = w / dx;
            var dy = maxy - miny;
            double ky = h / dy;

            var oz = zoom;
            var sz1 = new Size((int)(dx * kx), (int)(dy * kx));
            var sz2 = new Size((int)(dx * ky), (int)(dy * ky));
            zoom = (float)kx;
            if (sz1.Width > w || sz1.Height > h)
                zoom = (float)ky;

            var x = dx / 2 + minx;
            var y = dy / 2 + miny;

            sx = (float)((w / 2f) / zoom - x);
            sy = (float)((h / 2f) / zoom - y);

            var test = Transform(new PointF((float)x, (float)y));

        }

        public void ResetTransform()
        {
            Canvas.ResetMatrix();
        }

        public void ScaleTransform(float x, float y)
        {
            Canvas.Scale((float)x, (float)y);
        }

        public void DrawString(string s1, Font f2, Brush textBrush, float v1, float v2)
        {
            using (SKPaint paint = new SKPaint())
            {
                var clr = (textBrush as SolidBrush).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                paint.TextSize = f2.SizeInPoints;
                paint.TextAlign = SKTextAlign.Center;
                SKRect textBounds = SKRect.Empty;

                paint.MeasureText(s1, ref textBounds);


                SKTypeface tf = SKTypeface.FromFamilyName(f2.FontFamily.ToString(), SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                //paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Fill;

                Canvas.DrawText(s1, v1 + textBounds.Width / 2, v2 + textBounds.Height / 2, new SKFont(tf, f2.Size), paint);
            }
        }

        public void TranslateTransform(float x, float y)
        {
            Canvas.Translate((float)x, (float)y);
        }

        MeasureInfo IDrawingContext.MeasureString(string v, Font f)
        {
            var ms = MeasureString(v, f);
            return new MeasureInfo() { Width = ms.Width, Height = ms.Height };
        }

        public void FillPath(Brush brush, GraphicsPath graphicsPath)
        {

        }

        public class SkiaPathObject : PathObject
        {

            public SKPath Path = new SKPath();

            public override void AddBezier(PointF pointF, PointF cp1, PointF cp2, PointF cp3)
            {
                Path.MoveTo(pointF.X, pointF.Y);
                Path.CubicTo(cp1.X, cp1.Y, cp2.X, cp2.Y, cp3.X, cp3.Y);
                //Path.Close();
            }

            public override void AddLine(PointF pointF1, PointF pointF2)
            {
                Path.AddPoly(new SKPoint[] { new SKPoint(pointF1.X, pointF1.Y), new SKPoint(pointF2.X, pointF2.Y) });
            }

            public override RectangleF GetBounds()
            {
                return new RectangleF(Path.Bounds.Left, Path.Bounds.Top, Path.Bounds.Width, Path.Bounds.Height);
            }

            public override bool IsVisible(Point pos)
            {
                return Path.Contains(pos.X, pos.Y);
            }
        }

        public PathObject RoundedRect(RectangleF bounds, float radius)
        {
            float diameter = radius * 2;
            var size = new System.Drawing.SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            SKPath path = new SKPath();

            path.AddRoundRect(new SKRoundRect(bounds.ToSKRect(), radius));

            return new SkiaPathObject() { Path = path };
        }

        public PathObject HalfRoundedRect(RectangleF bounds, float radius)
        {
            var diameter = radius * 2;
            var size = new System.Drawing.SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            SKPath path = new SKPath();

            if (radius == 0)
            {
                path.AddRect(bounds.ToSKRect());
                return new SkiaPathObject() { Path = path };
            }

            path.AddRoundRect(new SKRoundRect(bounds.ToSKRect(), radius));

            // top left arc  
            //path.AddArc(arc.ToSKRect(), 180, 90);

            // top right arc  
            //arc.X = bounds.Right - diameter;
            //path.AddArc(arc.ToSKRect(), 270, 90);

            // bottom   
            //arc.Y = bounds.Bottom + diameter;
            //path.AddPoly(new SKPoint[] { new SKPoint(bounds.Right, bounds.Bottom), new SKPoint(bounds.Left, bounds.Bottom) });
            //path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);


            //path.Close();
            return new SkiaPathObject() { Path = path };
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            /*var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = (pen).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Stroke;
                canvas.DrawPath(path, paint);
            }*/
        }

        Control Box;

        public void Init(object pictureBox1)
        {
            Box = pictureBox1 as Control;


            Box.SizeChanged += Box_SizeChanged;
            Box.MouseDown += Box_MouseDown;
            Box.MouseUp += Box_MouseUp;
            Box.MouseWheel += Box_MouseWheel;
        }

        private void Box_MouseUp(object sender, MouseEventArgs e)
        {
            StopDrag();
        }

        public bool AllowDrag = true;
        public bool RecreateOnResize = false;

        private void Box_SizeChanged(object sender, EventArgs e)
        {
            if (!RecreateOnResize) return;
            /*Bmp = new Bitmap(Box.Width, Box.Height);
            Graphics = Graphics.FromImage(Bmp);
            Redraw?.Invoke();
            Box.Image = Bmp;*/
        }
        private void Box_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = Box.PointToClient(Cursor.Position);
            var p = Transform(pos);

            if (AllowDrag && e.Button == MouseButtons.Left)
            {
                isDrag = true;
                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
        }
        public static object lock1 = new object();

        float startx, starty;
        float origsx, origsy;
        bool isDrag = false;
        private void Box_MouseWheel(object sender, MouseEventArgs e)
        {
            lock (lock1)
            {
                float zold = zoom;
                if (e.Delta > 0) { zoom *= 1.2f; }
                else { zoom *= 0.8f; }
                if (zoom < 0.01) { zoom = 0.01f; }
                if (zoom > 1000) { zoom = 1000f; }

                var pos = Box.PointToClient(Cursor.Position);

                sx = -(pos.X / zold - sx - pos.X / zoom);
                sy = -(pos.Y / zold - sy - pos.Y / zoom);
            }
        }

        public void Update()
        {
            if (isDrag)
            {
                Point p = new Point();
                Box.Invoke((Action)(() => { p = Box.PointToClient(Cursor.Position); }));

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + ((p.Y - starty) / zoom);
            }
        }

        public void StopDrag()
        {
            isDrag = false;

        }

        public void Swap()
        {

        }

        public void AntiAlias(bool v)
        {

        }

        public void DrawLine(Pen pen1, PointF pp, PointF pp2)
        {
            Surface.Canvas?.DrawLine(pp.X, pp.Y, pp2.X, pp2.Y, CurrentPaint);
        }

        public PathObject NewPathObject()
        {
            return new SkiaPathObject();
        }


    }
}
