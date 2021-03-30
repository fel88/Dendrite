using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite.Preprocessors
{
    public class DrawKeypointsPostProcessor : AbstractPreprocessor, IPostDrawer
    {
        public float VisThreshold = 0.4f;
        public PictureBox Pbox;
        public Mat LastMat { get; set; }

        public static int[] Edges = new int[] {
            0,1,0,2,2,4,1,3,6,8,8,10,5,7,7,9,5,11,11,13,13,15,6,12,12,14,14,16,5,6
        };

        public override object Process(object input)
        {
            var list = input as object[];
            var dets = list.First(z => z is KeypointsDetectionInfo[]) as KeypointsDetectionInfo[];
            var net = list.First(z => z is Nnet) as Nnet;
            var ret = Helpers.drawKeypoints(net.lastReadedMat, dets, VisThreshold);
            LastMat = ret;
            Pbox.Invoke((Action)(() => {
                Pbox.Image = BitmapConverter.ToBitmap(ret);
            }));
            

            return ret;
        }
    }
    public interface IPostDrawer
    {
        Mat LastMat { get; }
    }
}
