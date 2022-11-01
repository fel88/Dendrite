using Dendrite.Lib;
//using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Preprocessors
{
    public class YoloDecodePreprocessor : AbstractPreprocessor
    {
        public float NmsThreshold = 0.8f;
        public double Threshold = 0.4;
        public List<string> AllowedClasses = new List<string>();

        public static ObjectDetectionInfo[] yoloBoxesDecode(Nnet net, int w, int h, float nms_tresh, double threshold, string[] allowedClasses = null)
        {
            List<ObjectDetectionInfo> ret = new List<ObjectDetectionInfo>();
            var f1 = net.Nodes.FirstOrDefault(z => z.Dims.Last() == 4);
            var f2 = net.Nodes.FirstOrDefault(z => z.Dims.Last() > 4);
            if (f1 == null || f2 == null)
            {
                return null;
            }
            var nms = Helpers.ReadResource("coco.names").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var rets1 = net.OutputDatas[f2.Name] as float[];
            var rets3 = net.OutputDatas[f1.Name] as float[];
            var dims = net.Nodes.First(z => z.IsInput).Dims;

            List<int> indexes = new List<int>();
            List<double> confs = new List<double>();
            List<int> classes = new List<int>();
            Dictionary<int, List<int>> perClassIndexes = new Dictionary<int, List<int>>();
            Dictionary<int, List<float>> perClassConfs = new Dictionary<int, List<float>>();
            int pos = 0;
            var cnt = f2.Dims.Last();

            for (int i = 0; i < rets1.Length; i += cnt, pos++)
            {
                int maxind = -1;
                double maxv = double.NaN;
                for (int j = 0; j < cnt; j++)
                {
                    if (allowedClasses != null && !allowedClasses.Contains(nms[j]))
                    {
                        continue;
                    }

                    if (maxind == -1 || rets1[i + j] > maxv)
                    {
                        maxv = rets1[i + j];
                        maxind = j;
                    }
                }
                if (maxind != -1 && maxv > threshold)
                {
                    confs.Add(maxv);
                    classes.Add(maxind);
                    indexes.Add(pos);
                    if (!perClassIndexes.ContainsKey(maxind))
                    {
                        perClassIndexes.Add(maxind, new List<int>());
                        perClassConfs.Add(maxind, new List<float>());
                    }
                    perClassIndexes[maxind].Add(pos);
                    perClassConfs[maxind].Add((float)maxv);
                }
            }



            List<int> res = new List<int>();
            foreach (var item in perClassIndexes)
            {
                List<float[]> boxes = new List<float[]>();
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var box = rets3.Skip(item.Value[i] * 4).Take(4).ToArray();
                    boxes.Add(new float[] { box[0], box[1], box[2], box[3], (float)perClassConfs[item.Key][i] });

                }
                var res2 = Decoders.nms(boxes, nms_tresh);
                res.AddRange(res2.Select(z => item.Value[z]));
            }



            for (int i = 0; i < indexes.Count; i++)
            {
                if (!res.Contains(indexes[i])) continue;
                int offset = indexes[i] * 4;
                //var box = rets3.Skip(indexes[i] * 4).Take(4).ToArray();
                ret.Add(new ObjectDetectionInfo()
                {
                    Class = classes[i],
                    Conf = (float)confs[i],
                    Rect = new Rect((int)(rets3[0 + offset] * w),
                    (int)(rets3[1 + offset] * h),
                    (int)((rets3[2 + offset] - rets3[0 + offset]) * w),
                    (int)((rets3[3 + offset] - rets3[1 + offset]) * h)),
                    Label = nms[classes[i]]
                });
            }

            return ret.ToArray();

        }

        //public override Type ConfigControl => typeof(YoloDecoderConfigControl);
        public override object Process(object inp)
        {
            var list = inp as object[];
            var net = list.First(z => z is Nnet) as Nnet;

            var ret = yoloBoxesDecode(net, net.lastReadedMat.Width, net.lastReadedMat.Height, NmsThreshold, Threshold, AllowedClasses.Count() == 0 ? null : AllowedClasses.ToArray());

            return ret;
        }
    }
}
