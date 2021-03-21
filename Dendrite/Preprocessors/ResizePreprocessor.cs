using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;

namespace Dendrite.Preprocessors
{
    public class ResizePreprocessor : AbstractPreprocessor
    {

        public override Type ConfigControl => typeof(ResizeConfigControl);

        public int[] Dims;

        public override object Process(object inp)
        {
            var input = inp as Mat;
            return input.Resize(new OpenCvSharp.Size(Dims[3], Dims[2]));
        }
    }
}
