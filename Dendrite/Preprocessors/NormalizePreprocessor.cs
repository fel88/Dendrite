using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;

namespace Dendrite.Preprocessors
{
    public class NormalizePreprocessor : AbstractPreprocessor
    {
        public override Type ConfigControl => typeof(NormalizeConfigControl);
        public NormalizeRangeTypeEnum RangeType;
        public override object Process(object inp)
        {
            var input = inp as Mat;
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
            return input;
        }
    }

    public enum NormalizeRangeTypeEnum
    {
        ZeroOne, MinusPlusOne
    }
}
