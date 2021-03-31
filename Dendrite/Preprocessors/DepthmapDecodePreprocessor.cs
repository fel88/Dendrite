using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Dendrite.Preprocessors
{
    public class DepthmapDecodePreprocessor : AbstractPreprocessor, IPostDrawer
    {
        public override Type ConfigControl => typeof(DepthmapConfigControl);

        public PictureBox Pbox;
        public bool StackWithSourceImage = true;
        public Mat LastMat { get; set; }
        public override object Process(object input)
        {


            var list = input as object[];

            var net = list.First(z => z is Nnet) as Nnet;

            var f1 = net.Nodes.FirstOrDefault(z => !z.IsInput);


            var rets3 = net.OutputDatas[f1.Name] as float[];
            InternalArray arr = new InternalArray(f1.Dims);
            arr.Data = rets3.Select(z => (double)z).ToArray();


            Mat mat = new Mat(f1.Dims[2],
                f1.Dims[3], MatType.CV_8UC1,
                arr.Data.Select(z => (byte)(z * 255)).ToArray());

            Cv2.ApplyColorMap(mat, mat, ColormapTypes.Magma);
            mat = mat.Resize(net.lastReadedMat.Size());
            if (StackWithSourceImage)
            {
                Cv2.HConcat(net.lastReadedMat, mat, mat);
            }

            LastMat = mat;
            Pbox.Invoke((Action)(() =>
            {
                Pbox.Image = BitmapConverter.ToBitmap(mat);
            }));

            return mat;


        }
    }


}
