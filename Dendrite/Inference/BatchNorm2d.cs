using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dendrite
{
    public class BatchNorm2d : NeuralItem
    {
        double eps;
        public BatchNorm2d(int nOut, double eps = 1e-5)
        {
            this.eps = eps;
        }


        public override int SetData(List<InternalArray> arrays)
        {
            //if (!arrays[0].Name.Contains("we")) throw new ArgumentException("not bn.weight detected");
            //if (!arrays[1].Name.Contains("bias")) throw new ArgumentException("not bn.bias detected");
            Weight = arrays[0];
            Bias = arrays[1];
            RunningMean = arrays[2];
            RunningVar = arrays[3];
            return 4;
        }


        public InternalArray RunningMean;
        public InternalArray RunningVar;
        public InternalArray Bias;
        public InternalArray Weight;

        public override InternalArray Forward(InternalArray ar)
        {
#if PROFILER
            LogItem = new CalcLogItem(this, "batchNorm2d");
            var sw = Stopwatch.StartNew();
#endif

            InternalArray ret = new InternalArray(ar.Shape);

            var n = ar.Shape[0];
            var c = ar.Shape[1];
            List<double> data = new List<double>();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    var img = Helpers.Get2DImageFrom4DArray(ar, i, j);
                    for (int zi = 0; zi < img.Data.Length; zi++)
                    {
                        img.Data[zi] = ((img.Data[zi] - RunningMean.Data[j]) / Math.Sqrt(RunningVar.Data[j] + eps)) * Weight.Data[j] + Bias.Data[j];
                    }

                    data.AddRange(img.Data);
                }
            }

            ret.Data = data.ToArray();
#if PROFILER

            sw.Stop();
            if (Parent != null)
            {
                Profiler.AddLog(Parent.LogItem, LogItem);
            }
#endif
            return ret;
        }
    }
}
