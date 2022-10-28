using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class ToRGBPreprocessor : AbstractPreprocessor
    {


        public override object Process(object inp)
        {
            var input = inp as Mat;
            if (input.Channels() == 1)
            {
                OpenCvSharp.Cv2.CvtColor(input, input, ColorConversionCodes.GRAY2RGB);
            }
            return input;
        }
    }



}
