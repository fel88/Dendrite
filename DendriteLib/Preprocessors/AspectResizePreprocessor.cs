//using Dendrite.Preprocessors.Controls;
using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class AspectResizePreprocessor : AbstractPreprocessor
    {

        public int[] Dims;

        public bool ForceH;
        public double H;
        //public override System.Type ConfigControl => typeof(AspectResizeConfigControl);

        public override object Process(object inp)
        {
            var input = inp as Mat;
            if (ForceH)
            {
                var neww = (int)((input.Width / (float)input.Height) * H);
                var newh = H;
                return input.Resize(new OpenCvSharp.Size(neww, newh));
            }
            {
                var neww = (int)((input.Width / (float)input.Height) * Dims[2]);
                var newh = Dims[2];
                return input.Resize(new OpenCvSharp.Size(neww, newh));
            }
        }
    }
}
