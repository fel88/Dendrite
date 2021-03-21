using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class GrayscalePreprocessor : AbstractPreprocessor
    {


        public override object Process(object inp)
        {
            var input = inp as Mat;
            if (input.Channels() == 3)
            {
                OpenCvSharp.Cv2.CvtColor(input, input, ColorConversionCodes.BGR2GRAY);
            }
            return input;
        }
    }
    public class BGR2RGBPreprocessor : AbstractPreprocessor
    {


        public override object Process(object inp)
        {
            var input = inp as Mat;
            if (input.Channels() == 3)
            {
                OpenCvSharp.Cv2.CvtColor(input, input, ColorConversionCodes.BGR2RGB);
            }
            return input;
        }
    }
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
