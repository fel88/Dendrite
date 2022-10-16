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

        public void Swap()
        {
            Box.Image = blist[CurrentIndex];
            CurrentIndex++;
            CurrentIndex %= blist.Count;
        }

        public override void Init(PictureBox pictureBox1)
        {
            for (int i = 0; i < 2; i++)
            {
                blist.Add(new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
                glist.Add(Graphics.FromImage(blist.Last()));
            }
            base.Init(pictureBox1);

        }
    }
}
