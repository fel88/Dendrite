using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class TransposePreprocessor : AbstractPreprocessor
    {   
        public override object Process(object inp)
        {
            var input = inp as Mat;
            input = input.Transpose();            
            return input;
        }
    }
}
