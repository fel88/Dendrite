﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite
{
    public class DrawingContext
    {
        public DrawingContext()
        {

        }
        public Bitmap Bmp
        {
            get
            {
                return blist[CurrentIndex];
            }
        }
        public Graphics Graphics
        {
            get
            {
                return glist[CurrentIndex];
            }
        }
        public PictureBox Box;
        List<Graphics> glist = new List<Graphics>();
        List<Bitmap> blist = new List<Bitmap>();

        int CurrentIndex;
        float startx, starty;
        float origsx, origsy;
        bool isDrag = false;

        public void Swap()
        {
            Box.Image = blist[CurrentIndex];
            CurrentIndex++;
            CurrentIndex %= blist.Count;
        }

        public float sx, sy;
        public float zoom = 1;
        public Graphics gr;
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

        public void Init(PictureBox pictureBox1)
        {
            Box = pictureBox1;
            for (int i = 0; i < 2; i++)
            {
                blist.Add(new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
                glist.Add(Graphics.FromImage(blist.Last()));
            }

            Box.Image = Bmp;
            Box.SizeChanged += Box_SizeChanged;
            Box.MouseDown += Box_MouseDown;
            Box.MouseUp += Box_MouseUp;
            Box.MouseWheel += Box_MouseWheel;
        }

        private void Box_MouseWheel(object sender, MouseEventArgs e)
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

        public void StopDrag()
        {
            isDrag = false;
        }
        private void Box_MouseUp(object sender, MouseEventArgs e)
        {
            StopDrag();
        }

        private void Box_MouseDown(object sender, MouseEventArgs e)
        {            
            var pos = Box.PointToClient(Cursor.Position);
            var p = Transform(pos);

            if (e.Button == MouseButtons.Left)
            {
                isDrag = true;
                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
        }
        public static object lock1 = new object();
        public Action Redraw;
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
    }
}
