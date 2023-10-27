using OpenTK.Graphics.ES20;
using SkiaSharp.Views.Desktop;
using SkiaSharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite
{

    public class DrawingContext : IDrawingContext
    {
        public DrawingContext()
        {

        }

        public float Zoom
        {
            get
            {
                return zoom;
            }
        }

        public PointF Shift
        {
            get
            {
                return new PointF(sx, sy);
            }
        }

        public virtual Bitmap Bmp
        {
            get; set;
        }

        public virtual Graphics Graphics
        {
            get; set;
        }

        public float sx { get; set; }
        public float sy { get; set; }

        public Action SwapAction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PictureBox Box;


        float startx, starty;
        float origsx, origsy;
        bool isDrag = false;


        public virtual PointF BackTransform(PointF p1)
        {
            return new PointF((p1.X / zoom - sx), (InvertY ? (-1) : 1) * (p1.Y / zoom - sy));
        }

        public float zoom { get; set; } = 1;
        //public Graphics gr;
        public Bitmap bmp;
        public bool InvertY = false;
        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, (InvertY ? (-1) : 1) * (p1.Y + sy) * zoom);
        }
        public virtual PointF Transform(double x, double y)
        {
            return new PointF(((float)(x) + sx) * zoom, (InvertY ? (-1) : 1) * ((float)(y) + sy) * zoom);
        }

        public virtual void Init(PictureBox pictureBox1)
        {
            Box = pictureBox1;


            Box.Image = Bmp;
            Box.SizeChanged += Box_SizeChanged;
            Box.MouseDown += Box_MouseDown;
            Box.MouseUp += Box_MouseUp;
            Box.MouseWheel += Box_MouseWheel;
        }

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

        public void StopDrag()
        {
            isDrag = false;
        }

        private void Box_MouseUp(object sender, MouseEventArgs e)
        {
            StopDrag();
        }

        public bool AllowDrag = true;

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
        public Action Redraw { get; set; }
        public bool RecreateOnResize = false;
        private void Box_SizeChanged(object sender, EventArgs e)
        {
            if (!RecreateOnResize) return;
            /*Bmp = new Bitmap(Box.Width, Box.Height);
            Graphics = Graphics.FromImage(Bmp);
            Redraw?.Invoke();
            Box.Image = Bmp;*/
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

        public Rectangle Transform(Rectangle rect)
        {
            var t1 = Transform(new PointF(rect.Left, rect.Top));
            return new Rectangle((int)t1.X, (int)t1.Y, (int)(rect.Width * zoom), (int)(rect.Height * zoom));
        }
        public RectangleF Transform(RectangleF rect)
        {
            var t1 = Transform(new PointF(rect.Left, rect.Top));
            return new RectangleF(t1.X, t1.Y, (rect.Width * zoom), (rect.Height * zoom));
        }

        public void FitToPoints(PointF[] points, int gap = 0)
        {
            var maxx = points.Max(z => z.X) + gap;
            var minx = points.Min(z => z.X) - gap;
            var maxy = points.Max(z => z.Y) + gap;
            var miny = points.Min(z => z.Y) - gap;

            var w = Box.Width;
            var h = Box.Height;

            var dx = maxx - minx;
            var kx = w / dx;
            var dy = maxy - miny;
            var ky = h / dy;

            var oz = zoom;
            var sz1 = new Size((int)(dx * kx), (int)(dy * kx));
            var sz2 = new Size((int)(dx * ky), (int)(dy * ky));
            zoom = kx;
            if (sz1.Width > w || sz1.Height > h) zoom = ky;

            var x = dx / 2 + minx;
            var y = dy / 2 + miny;

            sx = ((w / 2f) / zoom - x);
            sy = ((h / 2f) / zoom - y);

            var test = Transform(new PointF(x, y));

        }

        public void ResetTransform()
        {
            Graphics.ResetTransform();
        }

        public void ScaleTransform(float zoom1, float zoom2)
        {
            Graphics.ScaleTransform(zoom1, zoom2);
        }

        public void DrawString(string s1, Font f2, Brush textBrush, float v1, float v2)
        {
            Graphics.DrawString(s1, f2, textBrush, v1, v2);
        }

        public void TranslateTransform(float x, float y)
        {
            Graphics.TranslateTransform(x, y);
        }

        public MeasureInfo MeasureString(string v, Font f)
        {
            var r = Graphics.MeasureString(v, f);
            return new MeasureInfo() { Width = r.Width, Height = r.Height };
        }

        public void FillPath(Brush brush, PathObject graphicsPath)
        {
            var gpo = graphicsPath as GdiPathObject;
            Graphics.FillPath(brush, gpo.Path);
        }

        public void DrawPath(Pen borderPen, PathObject graphicsPath)
        {
            var gpo = graphicsPath as GdiPathObject;
            Graphics.DrawPath(borderPen, gpo.Path);
        }

        public void Init(object pictureBox1)
        {

            Box = pictureBox1 as PictureBox;
            Init(Box);



        }

        public void Clear(Color white)
        {
            Graphics.Clear(white);
        }

        public virtual void Swap()
        {

        }

        public void AntiAlias(bool v)
        {
            Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        }
        public PathObject RoundedRect(RectangleF bounds, float radius)
        {
            var diameter = radius * 2;
            var size = new System.Drawing.SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return new GdiPathObject() { Path = path };
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
            return new GdiPathObject() { Path = path };

        }

        public class GdiPathObject : PathObject
        {
            public GraphicsPath Path = new GraphicsPath();

            public override RectangleF GetBounds()
            {
                return Path.GetBounds();
            }

            public override bool IsVisible(Point pos)
            {
                return Path.IsVisible(pos);
            }

            public override void AddBezier(PointF pointF, PointF cp1, PointF cp2, PointF cp3)
            {
                Path.AddBezier(pointF, cp1, cp2, cp3);
            }

            public override void AddLine(PointF pointF1, PointF pointF2)
            {
                Path.AddLine(pointF1, pointF2);
            }
        }

        public PathObject GetRoundedRectangle(RectangleF r, float d)
        {
            if (d == 0) { d = 1; }
            GraphicsPath gp = new GraphicsPath();

            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d,
                                                             0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);

            return new GdiPathObject() { Path = gp };
        }

        public PathObject HalfRoundedRect(RectangleF bounds, float radius)
        {
            var diameter = radius * 2;
            var size = new System.Drawing.SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return new GdiPathObject() { Path = path };
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
            return new GdiPathObject() { Path = path };
        }

        public void DrawLine(Pen pen1, PointF p1, PointF p2)
        {
            Graphics.DrawLine(pen1, p1, p2);
        }

        Stack<Matrix> mtr = new Stack<Matrix>();
        public void PushMatrix()
        {
            mtr.Push(Graphics.Transform);
        }

        public void PopMatrix()
        {
            Graphics.Transform = mtr.Pop();
        }

        public object GenerateRenderControl()
        {
            return new PictureBox() { BorderStyle = BorderStyle.FixedSingle };
        }

        public PathObject NewPathObject()
        {
            return new GdiPathObject();
        }
    }

}
