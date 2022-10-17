using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "nchw")]
    public class NCHWPreprocessor : AbstractPreprocessor
    {
        public override string Name => "NCHW";
        public override object Process(object inp)
        {
            //var input = inp as Mat;
            var input = InputSlots[0].Data as Mat;
            var res2 = input.Split();
            List<float[]> rets = new List<float[]>();
            foreach (var item in res2)
            {
                item.GetArray<float>(out float[] ret1);
                rets.Add(ret1);
            }
            

            var inputData = new float[rets.Sum(z => z.Length)];
            int offset = 0;
            foreach (var item in rets)
            {
                Array.Copy(item, 0, inputData, offset, item.Length);
                offset += item.Length;
            }
            
            OutputSlots[0].Data = inputData;
            return inputData;
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<nchw/>");
        }
    }
}
