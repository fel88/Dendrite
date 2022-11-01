using Dendrite.Lib;
//using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "drawBoxes")]
    public class DrawBoxesPostProcessor : AbstractPreprocessor, IImageContainer
    {
        public DrawBoxesPostProcessor()
        {
            InputSlots = new DataSlot[3];
            InputSlots[0] = new DataSlot() { Name = "detections" };
            InputSlots[1] = new DataSlot() { Name = "image" };
            InputSlots[2] = new DataSlot() { Name = "origin_img_size" };

            OutputSlots[0].Name = "img";
        }

        public Dictionary<int, string> ClassNames { get; set; } = new Dictionary<int, string>();
        public float VisThreshold { get; set; } = 0.4f;
        public bool DrawLabels { get; set; } = true;

        public override void ParseXml(XElement sb)
        {
            if (sb.Attribute("drawLabels") != null)
                DrawLabels = bool.Parse(sb.Attribute("drawLabels").Value);
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<drawBoxes drawLabels=\"{DrawLabels}\"/>");
        }
        public override string Name => "draw boxes";

        //public override Type ConfigControl => typeof(DrawBoxesConfigControl);

        public Mat Image => (Mat)OutputSlots[0].Data;

        public override object Process(object input)
        {
            //var list = input as object[];

            // if (!list.Any(z => z is ObjectDetectionInfo[])) return null;
            var list = InputSlots[0].Data as ObjectDetectionContext;
            var dets = list.Infos;
            var img = InputSlots[1].Data as Mat;
            double scalerX = 1;
            double scalerY = 1;
            bool rescale = false;
            if (InputSlots[2].Data is int[] ss)
            {
                if (ss.Length == 4)//NCHW
                {
                    scalerX = ss[3] / (double)(list.Size.Width);
                    scalerY = ss[2] / (double)(list.Size.Height);
                    rescale = true;
                }
                else if (ss.Length == 2)//WH
                {
                    scalerX = ss[0] / (double)(list.Size.Width);
                    scalerY = ss[1] / (double)(list.Size.Height);
                    rescale = true;
                }
            }
            if (rescale)
            {
                List<ObjectDetectionInfo> dets2 = new List<ObjectDetectionInfo>();
                foreach (var item in dets)
                {
                    var t = new ObjectDetectionInfo();
                    t.Conf = item.Conf;
                    t.Class = item.Class;
                    t.Label = item.Label;
                    t.Rect = new Rect(
                        (int)(item.Rect.X * scalerX),
                        (int)(item.Rect.Y * scalerY),
                        (int)(item.Rect.Width * scalerX),
                        (int)(item.Rect.Height * scalerY));
                    dets2.Add(t);
                }
                dets = dets2.ToArray();
            }

            var ret = Helpers.DrawBoxes(img, dets, VisThreshold, DrawLabels);

            OutputSlots[0].Data = ret;

            return ret;
        }
    }
}
