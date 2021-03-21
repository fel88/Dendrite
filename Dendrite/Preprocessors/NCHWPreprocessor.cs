using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dendrite.Preprocessors
{
    public class NCHWPreprocessor : AbstractPreprocessor
    {
        public override object Process(object inp)
        {
            var input = inp as Mat;
            var res2 = input.Split();
            List<float[]> rets = new List<float[]>();
            foreach (var item in res2)
            {
                item.GetArray<float>(out float[] ret1);
                rets.Add(ret1);
            }
            /*res2[0].GetArray<float>(out float[] ret1);
            res2[1].GetArray<float>(out float[] ret2);
            res2[2].GetArray<float>(out float[] ret3);
            */

            var inputData = new float[rets.Sum(z => z.Length)];
            int offset = 0;
            foreach (var item in rets)
            {
                Array.Copy(item, 0, inputData, offset, item.Length);
                offset += item.Length;
            }
            /*Array.Copy(ret1, 0, inputData, 0, ret1.Length);
            Array.Copy(ret2, 0, inputData, ret1.Length, ret2.Length);
            Array.Copy(ret3, 0, inputData, ret1.Length + ret2.Length, ret3.Length);*/

            return inputData;
        }
    }
}
