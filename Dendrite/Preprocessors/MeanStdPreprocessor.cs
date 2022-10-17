using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "meanStd")]
    public class MeanStdPreprocessor : AbstractPreprocessor
    {
        public override string Name => "mean/std";

        public override Type ConfigControl => typeof(MeanStdConfigControl);
        public double[] Mean = new double[3];
        public double[] Std = new double[3];

        public override object Process(object inp)
        {
            //var input = inp as Mat;
            var input = InputSlots[0].Data as Mat;
            Cv2.Subtract(input, new Scalar(Mean[0], Mean[1], Mean[2]), input);
            var res = Cv2.Split(input);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] /= Std[i];
            }
            Cv2.Merge(res, input);
            OutputSlots[0].Data = input;
            return input;
        }

        public override void ParseXml(XElement sb)
        {
            Mean = sb.Attribute("mean").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(Helpers.ParseDouble).ToArray();
            Std = sb.Attribute("std").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(Helpers.ParseDouble).ToArray();
        }
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<meanStd mean=\"{Mean[0]};{Mean[1]};{Mean[2]}\" std=\"{Std[0]};{Std[1]};{Std[2]}\"/>");
        }
    }
}
