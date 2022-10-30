using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "craftMapDecoder")]
    public class CraftMapDecoderPostProcessor : DepthmapDecodePreprocessor
    {
        public CraftMapDecoderPostProcessor()
        {

        }

        protected override Mat GetMap()
        {
            var inp = InputSlots[0].Data as InternalArray;

            List<double> score_text = new List<double>();

            for (int i = 0; i < inp.Data.Length; i += 2)
            {
                score_text.Add(inp.Data[i]);
                //score_link.Add(inp.Data[i + 1]);
            }
            Mat mat = new Mat(inp.Shape[1], inp.Shape[2], MatType.CV_8UC1, score_text.Select(z => (byte)(Math.Min((int)(z * 255), 255))).ToArray());

            return mat;
        }
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<craftMapDecoder colormap=\"{Colormap}\"/>");
        }        
    }
}
