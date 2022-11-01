﻿using OpenCvSharp;
using System.Text;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "img")]
    public class ImgProcessor : AbstractPreprocessor, IImageContainer
    {
        public override string Name => "img";

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine("<img/>");
        }

        public override Type ConfigControl => null;

        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object inp)
        {
            //var list = inp as object[];
            var ar = InputSlots[0].Data as InternalArray;

            //clipping [0;255]
            var data = ar.ToFloatArray().Select(z => (byte)(Math.Min(255, Math.Max(z, 0)))).ToArray();

            var sz = new System.Drawing.Size();
            sz.Height = ar.Shape[2];
            sz.Width = ar.Shape[3];

            //NCHW->HWC 
            Mat mat = new Mat(new[] { sz.Width, sz.Height }, MatType.CV_8UC1, data.Take(sz.Height * sz.Width).ToArray());
            Mat mat2 = new Mat(new[] { sz.Width, sz.Height }, MatType.CV_8UC1, data.Skip(sz.Height * sz.Width).Take(sz.Height * sz.Width).ToArray());
            Mat mat3 = new Mat(new[] { sz.Width, sz.Height }, MatType.CV_8UC1, data.Skip(2 * sz.Height * sz.Width).ToArray());

            Mat dst = new Mat();
            Cv2.Merge(new[] { mat, mat2, mat3 }, dst);
            OutputSlots[0].Data = dst;
            return dst;
        }
    }
}