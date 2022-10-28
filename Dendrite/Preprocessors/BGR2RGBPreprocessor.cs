﻿using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class BGR2RGBPreprocessor : AbstractPreprocessor,IImageContainer
    {
        public override string Name => "bgr2rgb";

        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object inp)
        {
            var input = InputSlots[0].Data as Mat;            
            if (input.Channels() == 3)
            {
                Cv2.CvtColor(input, input, ColorConversionCodes.BGR2RGB);
            }
            OutputSlots[0].Data = input;
            return input;
        }
    }
}
