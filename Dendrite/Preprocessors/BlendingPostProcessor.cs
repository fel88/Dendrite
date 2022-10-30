using OpenCvSharp;
using System;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "blend")]
    public class BlendingPostProcessor : AbstractPreprocessor, IImageContainer
    {
        public BlendingPostProcessor()
        {
            InputSlots = new DataSlot[2];
            InputSlots[0] = new DataSlot() { Name = "img1" };
            InputSlots[1] = new DataSlot() { Name = "img2" };
        }

        public override void ParseXml(XElement sb)
        {
            
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<blend />");
        }        

        public override string Name => "blend";
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
            }
            
            var alpha = 0.5;
            var beta = (1.0 - alpha);
            Mat dst = new Mat();
            Cv2.AddWeighted(mat1, alpha, mat2, beta, 0.0, dst);            

            OutputSlots[0].Data = dst;
            return dst;
        }
    }
}
