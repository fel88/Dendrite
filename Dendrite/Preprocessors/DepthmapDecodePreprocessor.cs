using Dendrite.Preprocessors.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Dendrite.Preprocessors
{
    [XmlName(XmlKey = "depthmap")]
    public class DepthmapDecodePreprocessor : AbstractPreprocessor, IImageContainer
    {
        // public override Type ConfigControl => typeof(DepthmapConfigControl);
        public override Type ConfigControl => null;

        public override string Name => "depthmap";

        //public bool StackWithSourceImage = true;
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<depthmap colormap=\"{Colormap}\"/>");
        }

        public override void ParseXml(XElement sb)
        {
            Colormap = (ColormapTypes)Enum.Parse(typeof(ColormapTypes), sb.Attribute("colormap").Value);
        }

        public ColormapTypes Colormap { get; set; } = ColormapTypes.Magma;

        public Mat Image => OutputSlots[0].Data as Mat;

        public override object Process(object input)
        {

            //var rets3 = InputSlots[0].Data as float[];
            var arr = InputSlots[0].Data as InternalArray;
            //  var rets3 = arr.Data.Select(z => (float)z).ToArray();
            /*var list = input as object[];

            var net = list.First(z => z is Nnet) as Nnet;

            var f1 = net.Nodes.FirstOrDefault(z => !z.IsInput);


            var rets3 = net.OutputDatas[f1.Name] as float[];*/
            //InternalArray arr = new InternalArray(f1.Dims);
            //arr.Data = rets3.Select(z => (double)z).ToArray();


            Mat mat = new Mat(arr.Shape[2],
                arr.Shape[3], MatType.CV_8UC1,
                arr.Data.Select(z => (byte)(z * 255)).ToArray());

            Cv2.ApplyColorMap(mat, mat, Colormap);
            //mat = mat.Resize(net.lastReadedMat.Size());
            /* if (StackWithSourceImage)
             {
                 Cv2.HConcat(net.lastReadedMat, mat, mat);
             }*/

            OutputSlots[0].Data = mat;

            return mat;
        }
    }
}
