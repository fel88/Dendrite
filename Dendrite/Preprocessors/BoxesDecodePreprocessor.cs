using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dendrite.Preprocessors
{
    /// <summary>
    /// FaceBoxes decoder
    /// </summary>
    [XmlName(XmlKey = "boxesDecoder")]
    public class BoxesDecodePostProcessor : AbstractPreprocessor
    {

        public BoxesDecodePostProcessor()
        {
            InputSlots.Add(new DataSlot() { Name = "conf" });
            InputSlots.Add(new DataSlot() { Name = "loc" });
        }

        public override string Name => "boxes decoder";
        public float NmsThreshold = 0.8f;
        public double Threshold = 0.4;
        public float VisThreshold = 0.5f;

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<boxesDecoder/>");
        }

        public List<string> AllowedClasses = new List<string>();
        public ObjectDetectionInfo[] BoxesDecode(Nnet net, Mat mat1)
        {
            
            
            var rets1 = InputSlots[0].Data as float[];
            var rets3 = InputSlots[1].Data as float[];

            var dims = net.Nodes.First(z => z.IsInput).Dims;
            var sz = new System.Drawing.Size(dims[3], dims[2]);
            if (dims[2] == -1)
            {
                sz.Height = mat1.Height;
                sz.Width = mat1.Width;
            }
            string key = $"{sz.Width}x{sz.Height}";
            if (!Decoders.allPriorBoxes.ContainsKey(key))
            {
                var pd = Decoders.PriorBoxes2(sz.Width, sz.Height); ;
                Decoders.allPriorBoxes.Add(key, pd);
            }
            var prior_data = Decoders.allPriorBoxes[key];
            var ret = Decoders.BoxesDecode(mat1.Size(), rets3, rets1, sz, prior_data, VisThreshold);

            List<ObjectDetectionInfo> ret2 = new List<ObjectDetectionInfo>();
            for (int i = 0; i < ret.Item1.Length; i++)
            {
                var rect = ret.Item1[i];
                var odi = new ObjectDetectionInfo()
                {
                    Rect = rect,
                    Conf = ret.Item2[i],
                    Class = ret.Item3[i]
                };
                ret2.Add(odi);
            }
            return ret2.ToArray();

        }
        public override Type ConfigControl => null;
        public override object Process(object inp)
        {
            var list = inp as object[];
            var net = list.First(z => z is Nnet) as Nnet;
            if (InputSlots[0].FetchData != null)
            {
                foreach (var item in InputSlots)
                {
                    item.FetchData(item);
                }
            }
            else
            {
                var f1 = net.Nodes.FirstOrDefault(z => z.Tags.Contains("conf"));
                var f2 = net.Nodes.FirstOrDefault(z => z.Tags.Contains("loc"));
                if (f1 == null || f2 == null)
                    return null;

                InputSlots[0].Data = net.OutputDatas[f2.Name];
                InputSlots[1].Data = net.OutputDatas[f1.Name];
            }
            var ret = BoxesDecode(net, net.lastReadedMat);

            return ret;
        }
    }
}
