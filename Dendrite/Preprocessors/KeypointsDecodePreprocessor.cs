using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dendrite.Preprocessors
{
    public class KeypointsDecodePreprocessor : AbstractPreprocessor
    {


        public double Threshold = 0.4;
        public static KeypointsDetectionInfo[] Decode(Nnet net, int w, int h, double threshold, string[] allowedClasses = null)
        {
            List<KeypointsDetectionInfo> ret = new List<KeypointsDetectionInfo>();
            var inp = net.Nodes.First(z => z.IsInput);
            var f1 = net.Nodes.FirstOrDefault(z => z.Dims.Last() == 3);
            var snd = net.Nodes.FirstOrDefault(z => z.Dims.Length == 1 && z.ElementType == typeof(float));
            if (f1 == null)
            {
                return null;
            }

            var rets1 = net.OutputDatas[f1.Name] as float[];
            var scores = net.OutputDatas[snd.Name] as float[];
            InternalArray ar = new InternalArray(f1.Dims);
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
                    pp.Add(new Point2f((float)(sub.Get2D(j, 0) / inp.Dims[3] * w), (float)(sub.Get2D(j, 1) / inp.Dims[2] * h)));
                }
                kp.Points = pp.ToArray();
            }
            return ret.ToArray();

        }

        public override object Process(object inp)
        {
            var list = inp as object[];
            var net = list.First(z => z is Nnet) as Nnet;

            var ret = Decode(net, net.lastReadedMat.Width, net.lastReadedMat.Height, Threshold);

            return ret;
        }
    }
}
