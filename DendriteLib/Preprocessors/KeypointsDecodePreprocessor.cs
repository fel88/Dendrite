using Dendrite.Lib;
using OpenCvSharp;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "keypointsDecoder")]
    public class KeypointsDecodePreprocessor : AbstractPreprocessor
    {

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<keypointsDecoder/>");
        }
        public double Threshold { get; set; } = 0.4;
        public static KeypointsDetectionInfo[] Decode(int w, int h, InternalArray input, float[] scores, double threshold, string[] allowedClasses = null)
        {
            List<KeypointsDetectionInfo> ret = new List<KeypointsDetectionInfo>();
            /*var inp = net.Nodes.First(z => z.IsInput);
            var f1 = net.Nodes.FirstOrDefault(z => z.Dims.Last() == 3);
            var snd = net.Nodes.FirstOrDefault(z => z.Dims.Length == 1 && z.ElementType == typeof(float));
            if (f1 == null)
            {
                return null;
            }*/

            // var rets1 = net.OutputDatas[f1.Name] as float[];
            // var scores = net.OutputDatas[snd.Name] as float[];
            var rets1 = input.ToFloatArray();
            InternalArray ar = new InternalArray(input.Shape);
            ar.Data = new double[rets1.Length];
            for (int i = 0; i < rets1.Length; i++)
            {
                ar.Data[i] = rets1[i];
            }

            int cnt = scores.Length;
            for (int i = 0; i < cnt; i++)
            {
                if (scores[i] < 0.9) continue;
                var kp = new KeypointsDetectionInfo();
                ret.Add(kp);
                var sub = ar.Get2DImageFrom3DArray(i);
                List<Point2f> pp = new List<Point2f>();
                for (int j = 0; j < sub.Shape[0]; j++)
                {
                    pp.Add(new Point2f((float)(sub.Get2D(j, 0) / input.Shape[3] * w), (float)(sub.Get2D(j, 1) / input.Shape[2] * h)));
                }
                kp.Points = pp.ToArray();
            }
            return ret.ToArray();

        }

        public override object Process(object inp)
        {
            var data = InputSlots[0].Data as InternalArray;
            var scores = InputSlots[1].Data as InternalArray;

            var ret = Decode(900, 600, data, scores.ToFloatArray(), Threshold);
            OutputSlots[0].Data = ret;

            return ret;
        }
    }
}
