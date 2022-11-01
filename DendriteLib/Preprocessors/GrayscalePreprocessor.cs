using OpenCvSharp;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "grayscale")]
    public class GrayscalePreprocessor : AbstractPreprocessor, IImageContainer
    {
        public Mat Image => OutputSlots[0].Data as Mat;
        public override string Name => "grayscale";

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<grayscale />");
        }

        public override object Process(object inp)
        {
            var input = InputSlots[0].Data as Mat;
            if (input.Channels() == 3)
            {
                Cv2.CvtColor(input, input, ColorConversionCodes.BGR2GRAY);
            }
            OutputSlots[0].Data = input;
            return OutputSlots[0].Data;
        }
    }
}
