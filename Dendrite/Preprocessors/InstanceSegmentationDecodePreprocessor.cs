using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Preprocessors
{
    public class InstanceSegmentationDecodePreprocessor : AbstractPreprocessor
    {

        public override Type ConfigControl => typeof(InstanceSegmentationDecoderConfigControl);
        public double Threshold = 0.5;
        public double MaskThreshold = 0.4;
        public List<string> AllowedClasses = new List<string>();
        public SegmentationDetectionInfo[] Decode(Nnet net, int w, int h, string[] allowedClasses = null)
        {
            List<SegmentationDetectionInfo> ret = new List<SegmentationDetectionInfo>();
            var inp = net.Nodes.First(z => z.IsInput);
            var f1 = net.Nodes.FirstOrDefault(z => z.Dims.Last() == inp.Dims.Last());
            var snd = net.Nodes.FirstOrDefault(z => z.Dims.Length == 1 && z.ElementType == typeof(float));
            var labels = net.Nodes.FirstOrDefault(z => z.Dims.Length == 1 && z.ElementType != typeof(float));
            var boxes = net.Nodes.FirstOrDefault(z => z.Dims.Last() == 4);
            if (f1 == null)
            {
                return null;
            }
            var nms = Helpers.ReadResource("coco.2.names").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();


            var rets1 = net.OutputDatas[f1.Name] as float[];
            var labls = net.OutputDatas[labels.Name] as Int64[];
            var bxs = net.OutputDatas[boxes.Name] as float[];
            var scores = net.OutputDatas[snd.Name] as float[];
            InternalArray ar = new InternalArray(f1.Dims);
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
            var list = inp as object[];
            var net = list.First(z => z is Nnet) as Nnet;

            var ret = Decode(net, net.lastReadedMat.Width, net.lastReadedMat.Height);

            return ret;
        }
    }
}
