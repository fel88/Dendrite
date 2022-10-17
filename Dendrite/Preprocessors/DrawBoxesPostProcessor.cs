using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "drawBoxes")]
    public class DrawBoxesPostProcessor : AbstractPreprocessor, IPostDrawer, IImageContainer
    {

        public DrawBoxesPostProcessor()
        {
            InputSlots = new DataSlot[2];
            InputSlots[0] = new DataSlot() { Name = "detections" };
            InputSlots[1] = new DataSlot() { Name = "image" };

            OutputSlots[0].Name = "img";
        }

        public Dictionary<int, string> ClassNames { get; set; } = new Dictionary<int, string>();
        public float VisThreshold { get; set; } = 0.4f;
        public bool DrawLabels { get; set; } = true;
        //public PictureBox Pbox;
        public Mat LastMat { get; set; }
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<drawBoxes/>");
        }
        public override string Name => "draw boxes";

        public override Type ConfigControl => typeof(DrawBoxesConfigControl);

        public Mat Image => (Mat)OutputSlots[0].Data;

        public override object Process(object input)
        {
            //var list = input as object[];

            // if (!list.Any(z => z is ObjectDetectionInfo[])) return null;
            var list = InputSlots[0].Data as ObjectDetectionContext;
            var dets = list.Infos;
            var img = InputSlots[1].Data as Mat;

            //var net = list.First(z => z is Nnet) as Nnet;
            var ret = Helpers.DrawBoxes(img, dets, VisThreshold, DrawLabels);
            LastMat = ret;
            OutputSlots[0].Data = ret;
            /*if (Pbox != null)
            {
                Pbox.Invoke((Action)(() =>
                {
                    Pbox.Image = BitmapConverter.ToBitmap(LastMat);
                }));
            }*/

            return LastMat;
        }
    }
}
