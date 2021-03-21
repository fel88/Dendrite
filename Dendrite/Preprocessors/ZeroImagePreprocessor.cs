using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;

namespace Dendrite.Preprocessors
{
    public class ZeroImagePreprocessor : AbstractPreprocessor
    {
        public byte Filler = 0;
        public int Width;
        public int Height;
        public int Channels;
        public override Type ConfigControl => typeof(ZeroImageConfigControl);
        public override object Process(object inp)
        {
            Mat zmt = new Mat(Height, Width, Channels == 3 ? MatType.CV_8UC3 : MatType.CV_8UC1, new Scalar(Filler));
            return zmt;

        }
    }
}
