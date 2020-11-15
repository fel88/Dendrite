using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dendrite
{
    public class AvgPool2d : NeuralItem
    {
        public AvgPool2d(int kernelSize, int? stride = null, int padding = 0)
        {
            this.padding = new int[] { padding, padding };
            if (stride == null)
            {
                stride = kernelSize;
            }
            this.stride = new[] { stride.Value, stride.Value };
            this.kernelSize = new[] { kernelSize, kernelSize };
        }

        int[] kernelSize;
        int[] padding;
        int[] stride;



        public InternalArray Process2DImage(InternalArray ar)
        {
            var hin = ar.Shape[0];
            var win = ar.Shape[1];


            var hout = ((hin + 2 * padding[0] - kernelSize[0]) / stride[0]) + 1;
            var wout = ((win + 2 * padding[1] - kernelSize[1]) / stride[1]) + 1;

            InternalArray ret = new InternalArray(new int[] { hout, wout });

            for (int i = 0; i < hout; i++)
            {
                for (int j = 0; j < wout; j++)
                {
                    double avg = 0;

                    for (int i1 = 0; i1 < kernelSize[0]; i1++)
                    {
                        for (int j1 = 0; j1 < kernelSize[1]; j1++)
                        {
                            var x = i * stride[0] + i1 - kernelSize[0] / 2;
                            var y = j * stride[1] + j1 - kernelSize[1] / 2;
                            if (ar.WithIn(x, y))
                            {
                                avg += ar.Get2D(x, y);
                            }
                        }
                    }
                    avg /= kernelSize[0] * kernelSize[1];
                    ret.Set2D(i, j, avg);
                }
            }

            return ret;
        }

        public override InternalArray Forward(InternalArray ar)
        {

#if PROFILER
            LogItem = new CalcLogItem(this, "avgPool2D");
            var sw = Stopwatch.StartNew();
#endif
            var hin = ar.Shape[2];
            var win = ar.Shape[3];
            var n = ar.Shape[0];
            var c = ar.Shape[1];

            var hout = ((hin + 2 * padding[0] - kernelSize[0]) / stride[0]) + 1;
            var wout = ((win + 2 * padding[1] - kernelSize[1]) / stride[1]) + 1;
            InternalArray ret = new InternalArray(new int[] { n, c, hout, wout });

            //get all 2d images
            //append them together to [n,c]            
            List<double> data = new List<double>();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    var img = Helpers.Get2DImageFrom4DArray(ar, i, j);
                    var img2 = Process2DImage(img);
                    data.AddRange(img2.Data);
                }
            }

            ret.Data = data.ToArray();

#if PROFILER
            sw.Stop();
            Profiler.AddLog(LogItem, this, "exec", sw.ElapsedMilliseconds, true);
#endif
            return ret;
        }
    }
}
