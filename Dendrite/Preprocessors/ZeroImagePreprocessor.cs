using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;

namespace Dendrite.Preprocessors
{
    public class ZeroImagePreprocessor : AbstractPreprocessor, IImageContainer
    {
        public ZeroImagePreprocessor()
        {
            InputSlots = new DataSlot[0];
        }
        public byte Filler { get; set; } = 0;
        public int Width { get; set; } = 512;
        public int Height { get; set; } = 512;
        public int Channels { get; set; } = 3;
        public override Type ConfigControl => typeof(ZeroImageConfigControl);

        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object inp)
        {
            Mat zmt = new Mat(Height, Width,
                Channels == 3 ? MatType.CV_8UC3 : MatType.CV_8UC1,
                Channels == 3 ? new Scalar(Filler, Filler, Filler) : new Scalar(Filler));
            OutputSlots[0].Data = zmt;
            return zmt;

        }
    }
}
