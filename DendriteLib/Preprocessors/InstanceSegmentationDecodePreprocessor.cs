using Dendrite.Lib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Preprocessors
{
    public class InstanceSegmentationDecodePreprocessor : AbstractPreprocessor
    {

        public InstanceSegmentationDecodePreprocessor()
        {
            InputSlots = new DataSlot[4];
            InputSlots[0] = new DataSlot() { Name = "input" };
            InputSlots[1] = new DataSlot() { Name = "scores" };
            InputSlots[2] = new DataSlot() { Name = "labels" };
            InputSlots[3] = new DataSlot() { Name = "boxes" };
            InputSlots[4] = new DataSlot() { Name = "size" };
        }

        public override string Name => "instance segmentation decoder";

        //public override Type ConfigControl => typeof(InstanceSegmentationDecoderConfigControl);
        public double Threshold { get; set; } = 0.5;
        public double MaskThreshold { get; set; } = 0.4;

        public List<string> AllowedClasses = new List<string>();
        public SegmentationDetectionInfo[] Decode(int w, int h, InternalArray f1, float[] bxs, float[] scores, Int64[] labls, string[] allowedClasses = null)
        {
            List<SegmentationDetectionInfo> ret = new List<SegmentationDetectionInfo>();
            /*  var inp = net.Nodes.First(z => z.IsInput);
              var f1 = net.Nodes.FirstOrDefault(z => z.Dims.Last() == inp.Dims.Last());
              var snd = net.Nodes.FirstOrDefault(z => z.Dims.Length == 1 && z.ElementType == typeof(float));
              var labels = net.Nodes.FirstOrDefault(z => z.Dims.Length == 1 && z.ElementType != typeof(float));
              var boxes = net.Nodes.FirstOrDefault(z => z.Dims.Last() == 4);
              if (f1 == null)
              {
                  return null;
              }*/
            var nms = Helpers.ReadResource("coco.2.names").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var rets1 = f1.ToFloatArray();
            /*var rets1 = net.OutputDatas[f1.Name] as float[];
            var labls = net.OutputDatas[labels.Name] as Int64[];
            var bxs = net.OutputDatas[boxes.Name] as float[];
            var scores = net.OutputDatas[snd.Name] as float[];*/
            InternalArray ar = new InternalArray(f1.Shape);
            ar.Data = new double[rets1.Length];

            for (int i = 0; i < rets1.Length; i++)
            {
                ar.Data[i] = rets1[i];

                if (rets1[i] > MaskThreshold)
                {
                    ar.Data[i] = 1;
                }
                else
                {
                    ar.Data[i] = 0;
                }
            }

            int cnt = scores.Length;

            for (int i = 0; i < cnt; i++)
            {
                if (scores[i] < Threshold) continue;

                var kp = new SegmentationDetectionInfo();

                kp.Class = (int)(labls[i]);
                kp.Conf = scores[i];
                if (labls[i] < nms.Length)
                    kp.Label = nms[labls[i]];
                else
                    kp.Label = "(unknown)";
                if (AllowedClasses.Count != 0 && !AllowedClasses.Contains(kp.Label)) continue;
                ret.Add(kp);
                double fx = 1f / 900 * w;
                double fy = 1f / 600 * h;
                Rect rect = new Rect((int)(bxs[i * 4] * fx), (int)(bxs[i * 4 + 1] * fy), (int)((bxs[i * 4 + 2] - bxs[i * 4]) * fx), (int)((bxs[i * 4 + 3] - bxs[i * 4 + 1]) * fy));
                kp.Rect = rect;
                var sub = ar.Get3DImageFrom4DArray(i);
                byte[] arr = new byte[sub.Data.Length];
                for (int j = 0; j < sub.Data.Length; j++)
                {
                    arr[j] = (byte)sub.Data[j];
                }
                var cnt2 = arr.Count(z => z == 1);
                if (cnt2 > 0)
                {

                }
                Mat mat = new Mat(600, 900, MatType.CV_8UC1, arr);
                kp.Mask = mat;
                //mat.SaveImage("test1.jpg");
            }
            return ret.ToArray();

        }

        public override object Process(object inp)
        {
            var f1 = InputSlots[0].Data as InternalArray;
            var scores = InputSlots[1].Data as InternalArray;
            var labels = InputSlots[2].Data as Int64[];
            var boxes = InputSlots[3].Data as InternalArray;
            var sz = InputSlots[4].Data as int[];
            var ww = sz[3];
            var hh = sz[2];
            var ret = Decode(ww, hh, f1, boxes.ToFloatArray(), scores.ToFloatArray(), labels);

            return ret;
        }
    }
}
