using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using System;

namespace Dendrite.Preprocessors
{
    public class MeanStdPreprocessor : AbstractPreprocessor
    {
        public override Type ConfigControl => typeof(MeanStdConfigControl);
        public double[] Mean = new double[3];
        public double[] Std = new double[3];

        public override object Process(object inp)
        {
            var input = inp as Mat;
            Cv2.Subtract(input, new Scalar(Mean[0], Mean[1], Mean[2]), input);
            var res = Cv2.Split(input);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] /= Std[i];
            }
            Cv2.Merge(res, input);
            return input;
        }
    }
}
