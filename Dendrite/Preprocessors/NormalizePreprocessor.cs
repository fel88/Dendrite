using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class NormalizePreprocessor : AbstractPreprocessor
    {
        public override object Process(object inp)
        {
            var input = inp as Mat;
            input /= 255f;
            return input;
        }
    }
}
