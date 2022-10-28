using OpenCvSharp;
using System;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "concat")]
    public class ImgConcatPostProcessor : AbstractPreprocessor, IImageContainer
    {
        public ImgConcatPostProcessor()
        {
            InputSlots = new DataSlot[2];
            InputSlots[0] = new DataSlot() { Name = "img1" };
            InputSlots[1] = new DataSlot() { Name = "img2" };
        }

        public override void ParseXml(XElement sb)
        {
            Vertical = bool.Parse(sb.Attribute("vertical").Value);
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<concat vertical=\"{Vertical}\"/>");
        }

        public bool Vertical { get; set; }

        public override string Name => "concat";
        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object input)
        {
            var mat1 = InputSlots[0].Data as Mat;
            var mat2 = InputSlots[1].Data as Mat;
            if (mat1 == null || mat2 == null)
            {
                throw new ArgumentException("empty input");
            }
            if (mat1.Type() != mat2.Type())
            {
                mat1.ConvertTo(mat1, MatType.CV_8UC3);
                mat2.ConvertTo(mat2, MatType.CV_8UC3);
                if (mat1.Channels() != 3)
                    mat1 = mat1.CvtColor(ColorConversionCodes.GRAY2RGB);
                if (mat2.Channels() != 3)
                    mat2 = mat2.CvtColor(ColorConversionCodes.GRAY2RGB);

            }
            Mat res = new Mat();
            if (Vertical)
                Cv2.VConcat(mat1, mat2, res);
            else
                Cv2.HConcat(mat1, mat2, res);

            OutputSlots[0].Data = res;
            return res;
        }
    }
}
