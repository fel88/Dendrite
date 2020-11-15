using System;
using System.Drawing;
using System.Windows.Forms;

namespace Dendrite
{
    public class DrawingContext
    {
        public Bitmap Bmp;
        public Graphics Graphics;
        public PictureBox Box;
        float startx, starty;
        float origsx, origsy;
        bool isDrag = false;


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
            Bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics = Graphics.FromImage(Bmp);
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

        private void Box_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
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

        public Action Redraw;
        public bool RecreateOnResize = false;
        private void Box_SizeChanged(object sender, EventArgs e)
        {
            if (!RecreateOnResize) return;
            Bmp = new Bitmap(Box.Width, Box.Height);
            Graphics = Graphics.FromImage(Bmp);
            Redraw?.Invoke();
            Box.Image = Bmp;
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
    }
}
