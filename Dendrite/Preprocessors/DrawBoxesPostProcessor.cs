using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite.Preprocessors
{
    public class DrawBoxesPostProcessor : AbstractPreprocessor
    {
        public float VisThreshold = 0.4f;
        public PictureBox Pbox;
        public Mat LastMat;
        public override object Process(object input)
        {
            var list = input as object[];
            var dets = list.First(z => z is ObjectDetectionInfo[]) as ObjectDetectionInfo[];
            var net = list.First(z => z is Nnet) as Nnet;
            var ret = Helpers.drawBoxes(net.lastReadedMat, dets, VisThreshold);
            LastMat = ret;
            Pbox.Image = BitmapConverter.ToBitmap(ret);

            return ret;
        }
    }
}
