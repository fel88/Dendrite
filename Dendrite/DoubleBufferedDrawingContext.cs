using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite
{
    public class DoubleBufferedDrawingContext : DrawingContext
    {
        public DoubleBufferedDrawingContext()
        {

        }

        public override Bitmap Bmp
        {
            get
            {
                return blist[CurrentIndex];
            }
        }
        public override Graphics Graphics
        {
            get
            {
                return glist[CurrentIndex];
            }
        }

        List<Graphics> glist = new List<Graphics>();
        List<Bitmap> blist = new List<Bitmap>();

        int CurrentIndex;        

        public override void Swap()
        {
            /*Box.Image = blist[CurrentIndex];
            CurrentIndex++;
            CurrentIndex %= blist.Count;*/
        }

        public override void Init(PictureBox pictureBox1)
        {
            pictureBox1.Paint += PictureBox1_Paint;
            for (int i = 0; i < 1; i++)
            {
                blist.Add(new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
                glist.Add(Graphics.FromImage(blist.Last()));
            }
            base.Init(pictureBox1);

        }

        private void PictureBox1_Paint(object? sender, PaintEventArgs e)
        {
            Redraw?.Invoke();
        }
    }
}
