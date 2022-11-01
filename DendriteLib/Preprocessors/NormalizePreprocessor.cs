//using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "normalize")]
    public class NormalizePreprocessor : AbstractPreprocessor
    {
        //public override Type ConfigControl => typeof(NormalizeConfigControl);
        public NormalizeRangeTypeEnum RangeType { get; set; }
        public override object Process(object inp)
        {
            var input = InputSlots[0].Data as Mat;
            if (input.Type() != MatType.CV_32FC3)
            {
                input.ConvertTo(input, MatType.CV_32FC3);
            }
            switch (RangeType)
            {
                case NormalizeRangeTypeEnum.ZeroOne:
                    input /= 255f;

                    break;
                case NormalizeRangeTypeEnum.MinusPlusOne:
                    input = input / 127.5f - 1f;
                    break;
            }
            OutputSlots[0].Data = input;
            return input;
        }

        public override void ParseXml(XElement sb)
        {
            RangeType = (NormalizeRangeTypeEnum)Enum.Parse(typeof(NormalizeRangeTypeEnum), sb.Attribute("range").Value);
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<normalize range=\"{RangeType}\"/>");
        }
        public enum NormalizeRangeTypeEnum
        {
            ZeroOne, MinusPlusOne
        }
    }  
}
