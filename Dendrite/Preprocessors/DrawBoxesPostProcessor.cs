using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "drawBoxes")]
    public class DrawBoxesPostProcessor : AbstractPreprocessor, IPostDrawer
    {
        public float VisThreshold = 0.4f;
        public bool DrawLabels = true;
        public PictureBox Pbox;
        public Mat LastMat { get; set; }
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<drawBoxes/>");
        }
        public override string Name => "draw boxes";

        public override Type ConfigControl => typeof(DrawBoxesConfigControl);
        public override object Process(object input)
        {
            var list = input as object[];

            if (!list.Any(z => z is ObjectDetectionInfo[])) return null;

            var dets = list.First(z => z is ObjectDetectionInfo[]) as ObjectDetectionInfo[];

            var net = list.First(z => z is Nnet) as Nnet;
            var ret = Helpers.DrawBoxes(net.lastReadedMat, dets, VisThreshold, DrawLabels);
            LastMat = ret;

            if (Pbox != null)
            {
                Pbox.Invoke((Action)(() =>
                {
                    Pbox.Image = BitmapConverter.ToBitmap(LastMat);
                }));
            }

            return LastMat;
        }
    }
}
