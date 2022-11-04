using Dendrite.Lib;
using OpenCvSharp;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "yoloDecoder")]
    public class YoloDecodePreprocessor : AbstractPreprocessor
    {
        public YoloDecodePreprocessor()
        {
            InputSlots = new DataSlot[3];
            InputSlots[0] = (new DataSlot() { Name = "conf" });
            InputSlots[1] = (new DataSlot() { Name = "loc" });
            InputSlots[2] = (new DataSlot() { Name = "img_size" });
        }
        public float NmsThreshold { get; set; } = 0.8f;
        public double Threshold { get; set; } = 0.4;
        public List<string> AllowedClasses { get; set; } = new List<string>();
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<yoloDecoder/>");
        }
        public static ObjectDetectionInfo[] yoloBoxesDecode(int w, int h, InternalArray input, float[] loc, float nms_tresh, double threshold, string[] allowedClasses = null)
        {
            List<ObjectDetectionInfo> ret = new List<ObjectDetectionInfo>();
            /*var f1 = net.Nodes.FirstOrDefault(z => z.Dims.Last() == 4);
            var f2 = net.Nodes.FirstOrDefault(z => z.Dims.Last() > 4);
            if (f1 == null || f2 == null)
            {
                return null;
            }
            
            var rets1 = net.OutputDatas[f2.Name] as float[];
            var rets3 = net.OutputDatas[f1.Name] as float[];
            var dims = net.Nodes.First(z => z.IsInput).Dims;*/
            var rets1 = input.ToFloatArray();
            var rets3 = loc;

            var nms = Helpers.ReadResource("coco.names").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            List<int> indexes = new List<int>();
            List<double> confs = new List<double>();
            List<int> classes = new List<int>();
            Dictionary<int, List<int>> perClassIndexes = new Dictionary<int, List<int>>();
            Dictionary<int, List<float>> perClassConfs = new Dictionary<int, List<float>>();
            int pos = 0;
            var cnt = input.Shape.Last();

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

        public override object Process(object inp)
        {
            var r1 = InputSlots[0].Data as InternalArray;
            var r2 = InputSlots[1].Data as InternalArray;
            var r3 = InputSlots[2].Data as int[];
            var rr2 = r2.ToFloatArray();
            var ret = yoloBoxesDecode(r3[3], r3[2], r1, rr2, NmsThreshold, Threshold, AllowedClasses.Count == 0 ? null : AllowedClasses.ToArray());
            ObjectDetectionContext ctx = new ObjectDetectionContext()
            {
                Infos = ret,
                Size = new OpenCvSharp.Size(r3[3], r3[2])
            };
            OutputSlots[0].Data = ctx;
            return ctx;
        }
    }
}
