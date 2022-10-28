using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class GrayscalePreprocessor : AbstractPreprocessor, IImageContainer
    {
        public Mat Image => throw new System.NotImplementedException();

        public override object Process(object inp)
        {
            var input = InputSlots[0].Data as Mat;
            if (input.Channels() == 3)
            {
                OpenCvSharp.Cv2.CvtColor(input, input, ColorConversionCodes.BGR2GRAY);
            }
            OutputSlots[0].Data = input;
            return OutputSlots[0].Data;
        }
    }
}
