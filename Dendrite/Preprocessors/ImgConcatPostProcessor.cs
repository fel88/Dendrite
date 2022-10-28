using OpenCvSharp;

namespace Dendrite.Preprocessors
{
    public class ImgConcatPostProcessor : AbstractPreprocessor, IImageContainer
    {
        public bool Vertical { get; set; }

        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object input)
        {
            var mat1 = InputSlots[0].Data as Mat;
            var mat2 = InputSlots[1].Data as Mat;
            Mat res = new Mat();
            if (Vertical)
            {
                OpenCvSharp.Cv2.VConcat(mat1, mat2, res);
            }
            else
                OpenCvSharp.Cv2.HConcat(mat1, mat2, res);
            OutputSlots[0].Data = res;
            return res;
        }
    }
}
