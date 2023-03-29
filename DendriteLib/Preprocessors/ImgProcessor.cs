using OpenCvSharp;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "img")]
    public class ImgProcessor : AbstractPreprocessor, IImageContainer
    {
        public override string Name => "img";

        public override void ParseXml(XElement sb)
        {
            if (sb.Attribute("mode") != null)
            {
                Mode = (ImgProcessorMode)Enum.Parse(typeof(ImgProcessorMode), sb.Attribute("mode").Value);
            }
            base.ParseXml(sb);
        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<img mode=\"{Mode}\"/>");
        }
        public ImgProcessorMode Mode { get; set; }
        public enum ImgProcessorMode
        {
            RGB, Binary
        }
        public override Type ConfigControl => null;

        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object inp)
        {
            //var list = inp as object[];
            var ar = InputSlots[0].Data as InternalArray;
            if (Mode == ImgProcessorMode.RGB)
            {
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
            }
            else if (Mode == ImgProcessorMode.Binary)
            {
                byte[] outp = new byte[ar.Data.Length];
                for (int i = 0; i < ar.Data.Length; i++)
                {
                    outp[i] = (byte)ar.Data[i];
                }
                Mat mat = new Mat(new int[] { ar.Shape[2], ar.Shape[3] }, MatType.CV_8UC1, outp.ToArray());
                OutputSlots[0].Data = mat;

            }
            return OutputSlots[0].Data;
        }
    }

}
